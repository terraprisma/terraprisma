using Mono.Cecil;
using Mono.Cecil.Rocks;
using Terraprisma.Docs.SSG.Compiler.DotNet.Documentation.Models;
using GenericParameter = Terraprisma.Docs.SSG.Compiler.DotNet.Documentation.Models.GenericParameter;

namespace Terraprisma.Docs.SSG.Compiler.DotNet.Documentation;

public sealed class TypeDocumentation : MemberDocumentation {
    public AccessModifierKind AccessModifier { get; set; }

    public InstanceKind InstanceKind { get; set; }

    public SealedKind SealedKind { get; set; }

    public AbstractKind AbstractKind { get; set; }

    public List<GenericParameter>? GenericParameters { get; set; }

    public List<TypeDocumentation>? NestedTypes { get; set; }

    public List<ConstructorDocumentation>? Constructors { get; set; }

    public List<FieldDocumentation>? Fields { get; set; }

    public List<PropertyDocumentation>? Properties { get; set; }

    public List<MethodDocumentation>? Methods { get; set; }

    public List<EventDocumentation>? Events { get; set; }

    public TypeDocumentation(string @namespace, string name, string assemblyName) : base(@namespace, name, assemblyName) { }

    public override string ToString() {
        var name = $"{NormalizeName(Namespace)}-{NormalizeName(Name)}";

        // Cecil names include the generic count as `x
        //if (GenericParameters is not null)
        //    name += $"-{GenericParameters.Count}";

        return name;
    }

    public static TypeDocumentation FromTypeDefinition(TypeDefinition typeDefinition) {
        var typeDoc = new TypeDocumentation(typeDefinition.Namespace, typeDefinition.Name, typeDefinition.Module.Name);

        // NotPublic = internal
        // Public = public
        // NestedPublic = (nested) public
        // NestedPrivate = (nested) private
        // NestedFamily = (nested) protected
        // NestedAssembly = (nested) internal
        // NestedFamANDAssem = (nested) impossible to be declared in c#
        // NestedFamORAssem = (nested) protected internal
        if (typeDefinition.IsNotPublic)
            typeDoc.AccessModifier = AccessModifierKind.Internal;
        else if (typeDefinition.IsPublic)
            typeDoc.AccessModifier = AccessModifierKind.Public;
        else if (typeDefinition.IsNestedPublic)
            typeDoc.AccessModifier = AccessModifierKind.Public;
        else if (typeDefinition.IsNestedPrivate)
            typeDoc.AccessModifier = AccessModifierKind.Private;
        else if (typeDefinition.IsNestedFamily)
            typeDoc.AccessModifier = AccessModifierKind.Protected;
        else if (typeDefinition.IsNestedAssembly)
            typeDoc.AccessModifier = AccessModifierKind.Internal;
        else if (typeDefinition.IsNestedFamilyOrAssembly)
            typeDoc.AccessModifier = AccessModifierKind.ProtectedInternal;
        else
            throw new Exception($"Unknown access modifier for type {typeDefinition.FullName}");

        typeDoc.InstanceKind = typeDefinition.IsAbstract ? InstanceKind.Static : InstanceKind.Instance;
        typeDoc.SealedKind = typeDefinition.IsSealed ? SealedKind.Sealed : SealedKind.Unsealed;
        typeDoc.AbstractKind = typeDefinition.IsAbstract ? AbstractKind.Abstract : AbstractKind.NotAbstract;

        if (typeDefinition.HasGenericParameters) {
            typeDoc.GenericParameters = new List<GenericParameter>();
            foreach (var genericParameter in typeDefinition.GenericParameters)
                typeDoc.GenericParameters.Add(new GenericParameter {
                    Name = genericParameter.Name,
                });
        }

        if (typeDefinition.HasNestedTypes) {
            typeDoc.NestedTypes = new List<TypeDocumentation>();
            foreach (var nestedType in typeDefinition.NestedTypes)
                typeDoc.NestedTypes.Add(FromTypeDefinition(nestedType));
        }

        if (typeDefinition.HasMethods) {
            var constructors = typeDefinition.GetConstructors().ToList();

            if (constructors.Count > 0) {
                typeDoc.Constructors = new List<ConstructorDocumentation>();
                foreach (var ctor in constructors)
                    typeDoc.Constructors.Add(ConstructorDocumentation.FromConstructorDefinition(ctor, typeDoc));
            }

            var methods = typeDefinition.GetMethods().Where(x => !x.IsGetter && !x.IsSetter).ToList();

            if (methods.Count > 0) {
                typeDoc.Methods = new List<MethodDocumentation>();
                foreach (var method in methods)
                    typeDoc.Methods.Add(MethodDocumentation.FromMethodDefinition(method));
            }
        }

        if (typeDefinition.HasFields) {
            typeDoc.Fields = new List<FieldDocumentation>();
            foreach (var field in typeDefinition.Fields)
                typeDoc.Fields.Add(FieldDocumentation.FromFieldDefinition(field));
        }

        if (typeDefinition.HasProperties) {
            typeDoc.Properties = new List<PropertyDocumentation>();
            foreach (var property in typeDefinition.Properties)
                typeDoc.Properties.Add(PropertyDocumentation.FromPropertyDefinition(property));
        }

        if (typeDefinition.HasEvents) {
            typeDoc.Events = new List<EventDocumentation>();
            foreach (var @event in typeDefinition.Events)
                typeDoc.Events.Add(EventDocumentation.FromEventDefinition(@event));
        }

        return typeDoc;
    }
}

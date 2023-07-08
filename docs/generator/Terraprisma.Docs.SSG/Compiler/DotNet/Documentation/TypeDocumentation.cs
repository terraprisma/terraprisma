using Mono.Cecil;

namespace Terraprisma.Docs.SSG.Compiler.DotNet.Documentation;

public sealed class TypeDocumentation : MemberDocumentation {
    public string Namespace { get; set; }

    public string TypeName { get; set; }

    public List<TypeDocumentation>? NestedTypes { get; set; }

    public List<MethodDocumentation>? Methods { get; set; }

    public List<ConstructorDocumentation>? Constructors { get; set; }

    public List<FieldDocumentation>? Fields { get; set; }

    public List<EventDocumentation>? Events { get; set; }

    public List<PropertyDocumentation>? Properties { get; set; }

    public TypeDocumentation(string @namespace, string typeName) {
        Namespace = @namespace;
        TypeName = typeName;
    }

    public override string ToString() {
        return $"{NormalizeName(Namespace)}-{NormalizeName(TypeName)}";
    }

    public static TypeDefinition FromTypeDefinition(TypeDefinition Definition) {
        return default!;
    }
}

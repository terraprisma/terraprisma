﻿using System.Text;
using HtmlAgilityPack;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Locator;
using Mono.Cecil;
using Terraprisma.Docs.SSG.Compiler.DotNet.Documentation;
using Terraprisma.Docs.SSG.Configuration;

namespace Terraprisma.Docs.SSG.Compiler.DotNet;

/// <summary>
///     An MSBuild-powered .NET compiler which can compile a project and analyze
///     the compiled assembly, along with a resulting summary file.
/// </summary>
public sealed class DotNetCompiler : ICompiler {
    private static readonly List<string> name_blacklist = new() {
        "-<module>", // <Module>
    };

    public Dictionary<string, CompiledPage> Compile(CompilationContext context, CompilationNamespace ns) {
        if (!File.Exists(ns.Input))
            throw new FileNotFoundException($"The file {ns.Input} does not exist.");

        if (!MSBuildLocator.IsRegistered) {
            var instance = MSBuildLocator.QueryVisualStudioInstances().OrderByDescending(x => x.Version).First();
            MSBuildLocator.RegisterInstance(instance);
        }

        var (assemblyFilePath, summaryFilePath) = CompileMsBuildProject(ns.Input);
        return GenerateDocumentationFromAssembly(assemblyFilePath, summaryFilePath);
    }

    private (string assemblyFilePath, string? summaryFilePath) CompileMsBuildProject(string projectPath) {
        var projectCollection = new ProjectCollection();
        var project = new Project(projectPath, null, null, projectCollection);

        var outputDir = project.GetPropertyValue("OutputPath");
        var outputFileName = project.GetPropertyValue("AssemblyName") + ".dll";
        var summaryFileName = project.GetPropertyValue("DocumentationFile");

        var outputPath = Path.Combine(Path.GetDirectoryName(projectPath)!, outputDir, outputFileName).Replace('\\', Path.DirectorySeparatorChar);
        var summaryPath = Path.Combine(Path.GetDirectoryName(projectPath)!, outputDir, summaryFileName).Replace('\\', Path.DirectorySeparatorChar);

        if (!project.Build(new Microsoft.Build.Logging.ConsoleLogger()))
            throw new Exception("Failed to build project.");

        if (!File.Exists(outputPath))
            throw new FileNotFoundException($"The output file '{outputPath}' does not exist.");

        if (string.IsNullOrEmpty(summaryFileName) || !File.Exists(summaryPath)) {
            Console.WriteLine($"    Warning: The summary file '{summaryPath}' does not exist.");
            summaryPath = null;
        }

        return (outputPath, summaryPath);
    }

    private static Dictionary<string, CompiledPage> GenerateDocumentationFromAssembly(string assemblyFilePath, string? summaryFilePath) {
        var typeDocs = CreateDocsFromAssembly(assemblyFilePath).Where(x => !name_blacklist.Contains(x.ToString()));
        var output = new Dictionary<string, CompiledPage>();
        foreach (var typeDoc in typeDocs)
            GenerateOutputsFromType(typeDoc, output);
        return output;
    }

    private static List<TypeDocumentation> CreateDocsFromAssembly(string assemblyFilePath) {
        var module = ModuleDefinition.ReadModule(assemblyFilePath);
        var typeDocs = new List<TypeDocumentation>();

        // use Types and not GetTypes as we handle nested types ourselves.
        var types = module.Types; //module.GetTypes();
        foreach (var type in types)
            typeDocs.Add(AddMemberDocsForType(type));

        return typeDocs;
    }

    private static void GenerateOutputsFromType(TypeDocumentation typeDoc, Dictionary<string, CompiledPage> output) {
        if (name_blacklist.Contains(typeDoc.ToString()))
            return;

        // TODO: Just marking for now. We need a page for the type and a page
        // for each of its members.
        output.Add(typeDoc.ToString(), MakeTypePage(typeDoc));

        if (typeDoc.Constructors is not null) {
            foreach (var constructor in typeDoc.Constructors)
                output.Add($"{typeDoc}/{constructor}", CompiledPage.EMPTY);
        }

        if (typeDoc.Fields is not null) {
            foreach (var field in typeDoc.Fields) {
                if (field.Name.EndsWith("k__backingfield"))
                    continue;

                output.Add($"{typeDoc}/{field}", CompiledPage.EMPTY);
            }
        }

        if (typeDoc.Properties is not null) {
            foreach (var property in typeDoc.Properties)
                output.Add($"{typeDoc}/{property}", CompiledPage.EMPTY);
        }

        if (typeDoc.Methods is not null) {
            foreach (var method in typeDoc.Methods)
                output.Add($"{typeDoc}/{method}", CompiledPage.EMPTY);
        }

        if (typeDoc.Events is not null) {
            foreach (var @event in typeDoc.Events)
                output.Add($"{typeDoc}/{@event}", CompiledPage.EMPTY);
        }

        if (typeDoc.NestedTypes is not null) {
            foreach (var nestedType in typeDoc.NestedTypes)
                GenerateOutputsFromType(nestedType, output);
        }
    }

    private static CompiledPage MakeTypePage(TypeDocumentation typeDoc) {
        var readableTypeName = typeDoc.Name;

        if (typeDoc.GenericParameters is not null) {
            readableTypeName = readableTypeName[..readableTypeName.IndexOf('`')];
            readableTypeName += "<";

            for (var i = 0; i < typeDoc.GenericParameters.Count; i++) {
                var genericParameter = typeDoc.GenericParameters[i];
                readableTypeName += genericParameter.Name;
                if (i < typeDoc.GenericParameters.Count - 1)
                    readableTypeName += ", ";
            }

            readableTypeName += ">";
        }

        var htmlReadableTypeName = readableTypeName.Replace("<", "&lt;").Replace(">", "&gt;");

        var mainNode = HtmlNode.CreateNode("<main></main>");

        var classHeaderDiv = HtmlNode.CreateNode("<div class=\"class-header\"></div>");

        var classHeaderNamespaceCode = HtmlNode.CreateNode($"<code class=\"class-header-namespace\">{typeDoc.Namespace}</code>");
        classHeaderDiv.AppendChild(classHeaderNamespaceCode);

        var classHeaderClassdefDiv = HtmlNode.CreateNode($"<div class=\"class-header-classdef\"><code>{htmlReadableTypeName}</code></div>");
        classHeaderDiv.AppendChild(classHeaderClassdefDiv);

        var metadata = "";
        var appendLineBreak = false;

        if (typeDoc.Inheritance is not null && typeDoc.Inheritance.Count > 0) {
            metadata += $"Subclass of: <code>{typeDoc.Inheritance[0]}</code>";
            appendLineBreak = true;
        }

        // TODO: Maybe try to have superclasses too.

        if (typeDoc.Implements is not null && typeDoc.Implements.Count > 0) {
            if (appendLineBreak)
                metadata += "<br>";
            metadata += "Implements: ";

            for (var i = 0; i < typeDoc.Implements.Count; i++) {
                metadata += $"<code>{typeDoc.Implements[i]}</code>";

                if (i < typeDoc.Implements.Count - 1)
                    metadata += ", ";
            }

            appendLineBreak = true;
        }

        var metadataP = HtmlNode.CreateNode($"<p>{metadata}</p>");
        classHeaderDiv.AppendChild(metadataP);

        mainNode.AppendChild(classHeaderDiv);

        var classCtorDiv = HtmlNode.CreateNode("<div class=\"class-constructors\"></div>");
        var classCtorTable = HtmlNode.CreateNode("<table></table>");

        if (typeDoc.Constructors is not null && typeDoc.Constructors.Count > 0) {
            foreach (var ctor in typeDoc.Constructors) {
                var ctorRow = HtmlNode.CreateNode("<tr></tr>");
                var ctorParameterString = "";
                if (ctor.Parameters is not null && ctor.Parameters.Count > 0) {
                    for (var i = 0; i < ctor.Parameters.Count; i++) {
                        ctorParameterString += ctor.Parameters[i].TypeOrGenericName;
                        if (i < ctor.Parameters.Count - 1)
                            ctorParameterString += ", ";
                    }
                }
                
                ctorRow.AppendChild(HtmlNode.CreateNode($"<p><a href=\"./{typeDoc}/{ctor}.html\"><b>{typeDoc.Name}({ctorParameterString})</b></a></p>"));
                classCtorTable.AppendChild(ctorRow);
            }
        }

        classCtorDiv.AppendChild(classCtorTable);
        mainNode.AppendChild(classCtorDiv);

        var classPropertiesDiv = HtmlNode.CreateNode("<div class=\"class-properties\"></div>");
        var classPropertiesTable = HtmlNode.CreateNode("<table></table>");

        if (typeDoc.Properties is not null && typeDoc.Properties.Count > 0) {
            foreach (var prop in typeDoc.Properties) {
                var propRow = HtmlNode.CreateNode("<tr></tr>");
                propRow.AppendChild(HtmlNode.CreateNode($"<p><a href=\"./{typeDoc}/{prop}.html\"><b>{prop.Name}</b></a></p>"));
                classPropertiesTable.AppendChild(propRow);
            }
        }

        classPropertiesDiv.AppendChild(classPropertiesTable);
        mainNode.AppendChild(classPropertiesDiv);

        var classMethodsDiv = HtmlNode.CreateNode("<div class=\"class-methods\"></div>");
        var classMethodsTable = HtmlNode.CreateNode("<table></table>");

        if (typeDoc.Methods is not null && typeDoc.Methods.Count > 0) {
            foreach (var method in typeDoc.Methods) {
                var methodParameterString = "";
                if (method.Parameters is not null && method.Parameters.Count > 0) {
                    for (var i = 0; i < method.Parameters.Count; i++) {
                        methodParameterString += method.Parameters[i].TypeOrGenericName;
                        if (i < method.Parameters.Count - 1)
                            methodParameterString += ", ";
                    }
                }
                var methodGenericParameterString = "";
                if (method.GenericParameters is not null && method.GenericParameters.Count > 0) {
                    methodGenericParameterString = "&lt;";
                    for (var i = 0; i < method.GenericParameters.Count; i++) {
                        methodGenericParameterString += method.GenericParameters[i].Name;
                        if (i < method.GenericParameters.Count - 1)
                            methodGenericParameterString += ", ";
                        else
                            methodGenericParameterString += "&gt;";
                    }
                }
                var methodRow = HtmlNode.CreateNode("<tr></tr>");
                methodRow.AppendChild(HtmlNode.CreateNode($"<p><a href=\"./{typeDoc}/{method}.html\"><b>{method.Name}{methodGenericParameterString}({methodParameterString})</b></a></p>"));
                classMethodsTable.AppendChild(methodRow);
            }
        }

        classMethodsDiv.AppendChild(classMethodsTable);
        mainNode.AppendChild(classMethodsDiv);

        return new CompiledPage(readableTypeName, mainNode);
    }

    private static TypeDocumentation AddMemberDocsForType(TypeDefinition type) {
        return TypeDocumentation.FromTypeDefinition(type);
    }
}

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpCodeUtility.Core;

public static class CsharpRefactorer
{
    public static string FixNamespaceAndUsings(string code, string filePath, string projectRoot, string rootNamespace, List<string>? extraUsings = null)
    {
        // 1. Calculate Expected Namespace
        string relativePath = Path.GetRelativePath(projectRoot, Path.GetDirectoryName(filePath)!);

        // Handle case where file is in project root
        string namespaceSuffix = relativePath == "." ? "" : relativePath.Replace(Path.DirectorySeparatorChar, '.');

        string expectedNamespace = string.IsNullOrEmpty(namespaceSuffix)
            ? rootNamespace
            : $"{rootNamespace}.{namespaceSuffix}";

        // 2. Update Namespace
        var tree = CSharpSyntaxTree.ParseText(code);
        var root = tree.GetRoot();
        bool modified = false;

        // Handle Block Namespace
        var namespaceDecl = root.DescendantNodes().OfType<NamespaceDeclarationSyntax>().FirstOrDefault();
        if (namespaceDecl != null)
        {
            if (namespaceDecl.Name.ToString() != expectedNamespace)
            {
                var newNamespaceDecl = namespaceDecl.WithName(SyntaxFactory.ParseName(expectedNamespace))
                    .WithTrailingTrivia(namespaceDecl.GetTrailingTrivia());
                root = root.ReplaceNode(namespaceDecl, newNamespaceDecl);
                modified = true;
            }
        }
        // Handle File-Scoped Namespace
        else
        {
            var fileNamespaceDecl = root.DescendantNodes().OfType<FileScopedNamespaceDeclarationSyntax>().FirstOrDefault();
            if (fileNamespaceDecl != null)
            {
                if (fileNamespaceDecl.Name.ToString() != expectedNamespace)
                {
                    var newNamespaceDecl = fileNamespaceDecl.WithName(SyntaxFactory.ParseName(expectedNamespace))
                        .WithTrailingTrivia(fileNamespaceDecl.GetTrailingTrivia());
                    root = root.ReplaceNode(fileNamespaceDecl, newNamespaceDecl);
                    modified = true;
                }
            }
        }

        if (modified)
        {
            code = root.ToFullString();
        }

        // 3. Add Usings
        if (extraUsings != null)
        {
            foreach (var u in extraUsings)
            {
                code = CsharpModifier.AddUsing(code, u);
            }
        }

        return code;
    }
}

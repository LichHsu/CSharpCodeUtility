using CSharpCodeUtility.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpCodeUtility.Core;

public static class CsharpParser
{
    /// <summary>
    /// Parses the C# code and returns a flat list of code items (Classes, Methods, Properties).
    /// </summary>
    public static List<CsharpCodeItem> GetStructure(string code)
    {
        var tree = CSharpSyntaxTree.ParseText(code);
        var root = tree.GetRoot();
        var items = new List<CsharpCodeItem>();

        var classes = root.DescendantNodes().OfType<ClassDeclarationSyntax>();
        foreach (var cls in classes)
        {
            var classItem = CreateItem(cls, "Class", cls.Identifier.Text, cls.Modifiers.ToString());
            items.Add(classItem);

            // Methods
            foreach (var method in cls.Members.OfType<MethodDeclarationSyntax>())
            {
                var methodItem = CreateItem(method, "Method", method.Identifier.Text, method.Modifiers.ToString());
                methodItem.Signature = method.ParameterList.ToString();
                classItem.Children.Add(methodItem);
                items.Add(methodItem);
            }

            // Properties
            foreach (var prop in cls.Members.OfType<PropertyDeclarationSyntax>())
            {
                var propItem = CreateItem(prop, "Property", prop.Identifier.Text, prop.Modifiers.ToString());
                propItem.Signature = prop.Type.ToString();
                classItem.Children.Add(propItem);
                items.Add(propItem);
            }
        }

        return items;
    }

    private static CsharpCodeItem CreateItem(SyntaxNode node, string type, string name, string modifiers)
    {
        var lineSpan = node.SyntaxTree.GetLineSpan(node.Span);
        return new CsharpCodeItem
        {
            Type = type,
            Name = name,
            Content = node.ToString(),
            StartLine = lineSpan.StartLinePosition.Line + 1,
            EndLine = lineSpan.EndLinePosition.Line + 1,
            Modifiers = modifiers.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList()
        };
    }

    /// <summary>
    /// Extracts the body of a specific method.
    /// </summary>
    public static string? GetMethodBody(string code, string methodName)
    {
        var tree = CSharpSyntaxTree.ParseText(code);
        var root = tree.GetRoot();

        var method = root.DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .FirstOrDefault(m => m.Identifier.Text == methodName);

        return method?.Body?.ToString() ?? method?.ExpressionBody?.ToString();
    }
}

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpCodeUtility.Core;

public static class CsharpModifier
{
    /// <summary>
    /// Updates the body of a specific method.
    /// </summary>
    public static string UpdateMethodBody(string code, string methodName, string newBodyContent)
    {
        var tree = CSharpSyntaxTree.ParseText(code);
        var root = tree.GetRoot();

        var method = root.DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .FirstOrDefault(m => m.Identifier.Text == methodName);

        if (method == null)
        {
            throw new Exception($"Method '{methodName}' not found.");
        }

        // Parse the new body content into a BlockSyntax
        // We wrap it in a dummy method to parse it correctly as a block
        string dummyMethod = $"void Dummy() {{ {newBodyContent} }}";
        var dummyTree = CSharpSyntaxTree.ParseText(dummyMethod);
        var dummyRoot = dummyTree.GetRoot();
        var newBlock = dummyRoot.DescendantNodes().OfType<BlockSyntax>().First();

        // Replace the old body with the new block
        var newMethod = method.WithBody(newBlock).WithExpressionBody(null).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.None));

        // Format the new node to match standard indentation (basic)
        newMethod = newMethod.NormalizeWhitespace();

        var newRoot = root.ReplaceNode(method, newMethod);
        return newRoot.ToFullString();
    }

    /// <summary>
    /// Adds a using directive if it doesn't exist.
    /// </summary>
    public static string AddUsing(string code, string namespaceName)
    {
        var tree = CSharpSyntaxTree.ParseText(code);
        var root = tree.GetRoot() as CompilationUnitSyntax;

        if (root == null) return code;

        // Check if using already exists
        if (root.Usings.Any(u => u.Name?.ToString() == namespaceName))
        {
            return code;
        }

        var newUsing = SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(namespaceName))
            .NormalizeWhitespace()
            .WithTrailingTrivia(SyntaxFactory.ElasticCarriageReturnLineFeed);

        var newRoot = root.AddUsings(newUsing);
        return newRoot.ToFullString();
    }
}

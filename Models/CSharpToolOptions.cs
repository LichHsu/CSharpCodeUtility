using Lichs.MCP.Core.Attributes;

namespace CSharpCodeUtility.Models;

public class CSharpEditOptions
{
    // For UpdateMethod
    public string? MethodName { get; set; }
    public string? NewBody { get; set; } // Can be passed in options or content depending on new design

    // For FixNamespace
    public string? ProjectRoot { get; set; }
    public string? RootNamespace { get; set; }
}

using System.Text.Json.Serialization;

namespace CSharpCodeUtility.Models;

/// <summary>
/// Represents a code item (Class, Method, Property, etc.)
/// </summary>
public class CsharpCodeItem
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty; // "Class", "Method", "Property", "Interface", "Enum"

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("signature")]
    public string Signature { get; set; } = string.Empty;

    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;

    [JsonPropertyName("startLine")]
    public int StartLine { get; set; }

    [JsonPropertyName("endLine")]
    public int EndLine { get; set; }

    [JsonPropertyName("modifiers")]
    public List<string> Modifiers { get; set; } = new();

    [JsonPropertyName("children")]
    public List<CsharpCodeItem> Children { get; set; } = new();
}

/// <summary>
/// Represents an in-memory editing session
/// </summary>
public class CsharpSession
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonPropertyName("filePath")]
    public string? FilePath { get; set; }

    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;

    [JsonPropertyName("isDirty")]
    public bool IsDirty { get; set; }

    [JsonPropertyName("lastModified")]
    public DateTime LastModified { get; set; } = DateTime.Now;
}

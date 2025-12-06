using System.Text.Json;
using CSharpCodeUtility.Core;
using CSharpCodeUtility.Models;
using Lichs.MCP.Core.Attributes;

namespace CSharpCodeUtility.MCP;

public static class ToolHandlers
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    [McpTool("get_code_structure", "Parses C# code and returns a flat list of structure items (Classes, Methods, Properties).")]
    public static string HandleGetCodeStructure(
        [McpParameter("File path (optional if sessionId provided)", false)] string? path = null,
        [McpParameter("Session ID (optional if path provided)", false)] string? sessionId = null)
    {
        string code;
        if (!string.IsNullOrEmpty(sessionId))
        {
            var session = CsharpSessionManager.GetSession(sessionId);
            code = session.Content;
        }
        else if (!string.IsNullOrEmpty(path))
        {
            code = File.ReadAllText(path);
        }
        else
        {
            throw new ArgumentException("Either 'sessionId' or 'path' must be provided.");
        }

        var structure = CsharpParser.GetStructure(code);
        return JsonSerializer.Serialize(structure, _jsonOptions);
    }

    [McpTool("get_method", "Extracts the body of a specific method.")]
    public static string HandleGetMethod(
        [McpParameter("The method name to find")] string methodName,
        [McpParameter("File path (optional if sessionId provided)", false)] string? path = null,
        [McpParameter("Session ID (optional if path provided)", false)] string? sessionId = null)
    {
        string code;
        if (!string.IsNullOrEmpty(sessionId))
        {
            var session = CsharpSessionManager.GetSession(sessionId);
            code = session.Content;
        }
        else if (!string.IsNullOrEmpty(path))
        {
            code = File.ReadAllText(path);
        }
        else
        {
            throw new ArgumentException("Either 'sessionId' or 'path' must be provided.");
        }

        var body = CsharpParser.GetMethodBody(code, methodName);
        
        if (body == null) return $"Method '{methodName}' not found.";
        return body;
    }

    [McpTool("update_method", "Updates the body of a specific method.")]
    public static string HandleUpdateMethod(
        [McpParameter("The method name to update")] string methodName,
        [McpParameter("New method body content (statements only)")] string newBody,
        [McpParameter("File path (optional if sessionId provided)", false)] string? path = null,
        [McpParameter("Session ID (optional if path provided)", false)] string? sessionId = null)
    {
        if (!string.IsNullOrEmpty(sessionId))
        {
            var session = CsharpSessionManager.GetSession(sessionId);
            string newCode = CsharpModifier.UpdateMethodBody(session.Content, methodName, newBody);
            CsharpSessionManager.UpdateSessionContent(session.Id, newCode);
            return $"Updated method '{methodName}' in session {session.Id}.";
        }
        else if (!string.IsNullOrEmpty(path))
        {
            string code = File.ReadAllText(path);
            string newCode = CsharpModifier.UpdateMethodBody(code, methodName, newBody);
            File.WriteAllText(path, newCode);
            return $"Updated method '{methodName}' in file {path}.";
        }
        
        throw new ArgumentException("Either 'sessionId' or 'path' must be provided.");
    }

    [McpTool("add_using", "Adds a using directive if missing.")]
    public static string HandleAddUsing(
        [McpParameter("Using namespace to add")] string @namespace,
        [McpParameter("File path (optional if sessionId provided)", false)] string? path = null,
        [McpParameter("Session ID (optional if path provided)", false)] string? sessionId = null)
    {
        if (!string.IsNullOrEmpty(sessionId))
        {
            var session = CsharpSessionManager.GetSession(sessionId);
            string newCode = CsharpModifier.AddUsing(session.Content, @namespace);
            CsharpSessionManager.UpdateSessionContent(session.Id, newCode);
            return $"Added using '{@namespace}' to session {session.Id}.";
        }
        else if (!string.IsNullOrEmpty(path))
        {
            string code = File.ReadAllText(path);
            string newCode = CsharpModifier.AddUsing(code, @namespace);
            File.WriteAllText(path, newCode);
            return $"Added using '{@namespace}' to file {path}.";
        }

        throw new ArgumentException("Either 'sessionId' or 'path' must be provided.");
    }

    [McpTool("start_csharp_session", "Starts a new in-memory editing session.")]
    public static string HandleStartSession([McpParameter("Initial file path to load", false)] string? path = null)
    {
        var session = CsharpSessionManager.CreateSession(path);
        return JsonSerializer.Serialize(session, _jsonOptions);
    }

    [McpTool("update_csharp_session", "Updates the content of a session.")]
    public static string HandleUpdateSession(
        [McpParameter("Session ID")] string sessionId,
        [McpParameter("New content")] string content)
    {
        CsharpSessionManager.UpdateSessionContent(sessionId, content);
        return $"Session {sessionId} updated.";
    }

    [McpTool("save_csharp_session", "Saves the session content to disk.")]
    public static string HandleSaveSession([McpParameter("Session ID")] string sessionId)
    {
        CsharpSessionManager.SaveSession(sessionId);
        return $"Session {sessionId} saved to disk.";
    }

    [McpTool("fix_namespace_and_usings", "Fixes namespace mismatches and adds missing usings.")]
    public static string HandleFixNamespaceAndUsings(
        [McpParameter("Directory to scan")] string directory,
        [McpParameter("Project root directory")] string projectRoot,
        [McpParameter("Root namespace of the project")] string rootNamespace,
        [McpParameter("List of extra usings to add", false)] List<string>? extraUsings = null)
    {
        if (!Directory.Exists(directory)) throw new DirectoryNotFoundException($"Directory not found: {directory}");

        var files = Directory.GetFiles(directory, "*.cs", SearchOption.AllDirectories);
        int fixedCount = 0;

        foreach (var file in files)
        {
            string code = File.ReadAllText(file);
            string newCode = CsharpRefactorer.FixNamespaceAndUsings(code, file, projectRoot, rootNamespace, extraUsings);
            
            if (code != newCode)
            {
                File.WriteAllText(file, newCode);
                fixedCount++;
            }
        }

        return $"Fixed namespaces and usings in {fixedCount} files.";
    }
}

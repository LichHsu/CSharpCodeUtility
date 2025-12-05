using System.Text.Json;
using CSharpCodeUtility.Core;
using CSharpCodeUtility.Models;

namespace CSharpCodeUtility.MCP;

public static class ToolHandlers
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static string HandleGetCodeStructure(JsonElement args)
    {
        string code;
        if (args.TryGetProperty("sessionId", out var sessionId))
        {
            var session = CsharpSessionManager.GetSession(sessionId.GetString()!);
            code = session.Content;
        }
        else if (args.TryGetProperty("path", out var path))
        {
            code = File.ReadAllText(path.GetString()!);
        }
        else
        {
            throw new ArgumentException("Either 'sessionId' or 'path' must be provided.");
        }

        var structure = CsharpParser.GetStructure(code);
        return JsonSerializer.Serialize(structure, _jsonOptions);
    }

    public static string HandleGetMethod(JsonElement args)
    {
        string code;
        if (args.TryGetProperty("sessionId", out var sessionId))
        {
            var session = CsharpSessionManager.GetSession(sessionId.GetString()!);
            code = session.Content;
        }
        else if (args.TryGetProperty("path", out var path))
        {
            code = File.ReadAllText(path.GetString()!);
        }
        else
        {
            throw new ArgumentException("Either 'sessionId' or 'path' must be provided.");
        }

        string methodName = args.GetProperty("methodName").GetString()!;
        var body = CsharpParser.GetMethodBody(code, methodName);
        
        if (body == null) return $"Method '{methodName}' not found.";
        return body;
    }

    public static string HandleUpdateMethod(JsonElement args)
    {
        string methodName = args.GetProperty("methodName").GetString()!;
        string newBody = args.GetProperty("newBody").GetString()!;
        
        if (args.TryGetProperty("sessionId", out var sessionId))
        {
            var session = CsharpSessionManager.GetSession(sessionId.GetString()!);
            string newCode = CsharpModifier.UpdateMethodBody(session.Content, methodName, newBody);
            CsharpSessionManager.UpdateSessionContent(session.Id, newCode);
            return $"Updated method '{methodName}' in session {session.Id}.";
        }
        else if (args.TryGetProperty("path", out var path))
        {
            string filePath = path.GetString()!;
            string code = File.ReadAllText(filePath);
            string newCode = CsharpModifier.UpdateMethodBody(code, methodName, newBody);
            File.WriteAllText(filePath, newCode);
            return $"Updated method '{methodName}' in file {filePath}.";
        }
        
        throw new ArgumentException("Either 'sessionId' or 'path' must be provided.");
    }

    public static string HandleAddUsing(JsonElement args)
    {
        string namespaceName = args.GetProperty("namespace").GetString()!;
        
        if (args.TryGetProperty("sessionId", out var sessionId))
        {
            var session = CsharpSessionManager.GetSession(sessionId.GetString()!);
            string newCode = CsharpModifier.AddUsing(session.Content, namespaceName);
            CsharpSessionManager.UpdateSessionContent(session.Id, newCode);
            return $"Added using '{namespaceName}' to session {session.Id}.";
        }
        else if (args.TryGetProperty("path", out var path))
        {
            string filePath = path.GetString()!;
            string code = File.ReadAllText(filePath);
            string newCode = CsharpModifier.AddUsing(code, namespaceName);
            File.WriteAllText(filePath, newCode);
            return $"Added using '{namespaceName}' to file {filePath}.";
        }

        throw new ArgumentException("Either 'sessionId' or 'path' must be provided.");
    }

    public static string HandleStartSession(JsonElement args)
    {
        string? path = args.TryGetProperty("path", out var p) ? p.GetString() : null;
        var session = CsharpSessionManager.CreateSession(path);
        return JsonSerializer.Serialize(session, _jsonOptions);
    }

    public static string HandleUpdateSession(JsonElement args)
    {
        string sessionId = args.GetProperty("sessionId").GetString()!;
        string content = args.GetProperty("content").GetString()!;
        CsharpSessionManager.UpdateSessionContent(sessionId, content);
        return $"Session {sessionId} updated.";
    }

    public static string HandleSaveSession(JsonElement args)
    {
        string sessionId = args.GetProperty("sessionId").GetString()!;
        CsharpSessionManager.SaveSession(sessionId);
        return $"Session {sessionId} saved to disk.";
    }

    public static string HandleSplitRazorFile(JsonElement args)
    {
        string path = args.GetProperty("path").GetString()!;
        return CSharpCodeUtility.Operations.RazorSplitter.SplitFile(path);
    }
}

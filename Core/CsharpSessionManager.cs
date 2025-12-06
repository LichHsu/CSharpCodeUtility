using CSharpCodeUtility.Models;
using System.Collections.Concurrent;

namespace CSharpCodeUtility.Core;

public static class CsharpSessionManager
{
    private static readonly ConcurrentDictionary<string, CsharpSession> _sessions = new();

    /// <summary>
    /// Creates a new session. If filePath is provided, loads content from file.
    /// </summary>
    public static CsharpSession CreateSession(string? filePath = null)
    {
        string content = "";
        if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
        {
            content = File.ReadAllText(filePath);
        }

        var session = new CsharpSession
        {
            FilePath = filePath,
            Content = content,
            IsDirty = false
        };

        _sessions[session.Id] = session;
        return session;
    }

    /// <summary>
    /// Gets an existing session by ID.
    /// </summary>
    public static CsharpSession GetSession(string sessionId)
    {
        if (_sessions.TryGetValue(sessionId, out var session))
        {
            return session;
        }
        throw new Exception($"Session not found: {sessionId}");
    }

    /// <summary>
    /// Updates the content of a session.
    /// </summary>
    public static void UpdateSessionContent(string sessionId, string newContent)
    {
        var session = GetSession(sessionId);
        session.Content = newContent;
        session.IsDirty = true;
        session.LastModified = DateTime.Now;
    }

    /// <summary>
    /// Saves the session content to its file path.
    /// </summary>
    public static void SaveSession(string sessionId)
    {
        var session = GetSession(sessionId);
        if (string.IsNullOrEmpty(session.FilePath))
        {
            throw new Exception("Session has no associated file path.");
        }

        File.WriteAllText(session.FilePath, session.Content);
        session.IsDirty = false;
    }

    /// <summary>
    /// Closes (removes) a session.
    /// </summary>
    public static void CloseSession(string sessionId)
    {
        _sessions.TryRemove(sessionId, out _);
    }

    /// <summary>
    /// Lists all active sessions.
    /// </summary>
    public static List<CsharpSession> ListSessions()
    {
        return _sessions.Values.ToList();
    }
}

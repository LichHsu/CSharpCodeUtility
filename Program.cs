using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using CSharpCodeUtility.MCP;

namespace CSharpCodeUtility;

internal class Program
{
    private static readonly string _logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mcp_debug_log.txt");
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    static async Task Main(string[] args)
    {
        Console.OutputEncoding = new UTF8Encoding(false);
        Console.InputEncoding = new UTF8Encoding(false);

        if (args.Length > 0 && args[0] == "--test")
        {
            CSharpCodeUtility.Testing.TestRunner.RunAllTests();
            return;
        }

        Log("=== C# Server Started ===");

        try
        {
            while (true)
            {
                string? line = await Console.In.ReadLineAsync();
                if (line == null) break;
                if (string.IsNullOrWhiteSpace(line)) continue;

                Log($"[RECV]: {line}");

                var request = JsonSerializer.Deserialize<JsonRpcRequest>(line, _jsonOptions);
                if (request == null) continue;

                object? result = null;

                switch (request.Method)
                {
                    case "initialize":
                        result = new
                        {
                            protocolVersion = "2024-11-05",
                            capabilities = new { tools = new { } },
                            serverInfo = new { name = "csharp-code-utility", version = "1.0.0" }
                        };
                        break;

                    case "notifications/initialized":
                        continue;

                    case "tools/list":
                        result = new { tools = GetToolDefinitions() };
                        break;

                    case "tools/call":
                        try
                        {
                            result = HandleToolCall(request.Params);
                        }
                        catch (Exception ex)
                        {
                            Log($"[ERROR]: {ex.Message}");
                            SendResponse(new JsonRpcResponse
                            {
                                Id = request.Id,
                                Error = new { code = -32602, message = ex.Message }
                            });
                            continue;
                        }
                        break;
                }

                if (result != null)
                {
                    SendResponse(new JsonRpcResponse { Id = request.Id, Result = result });
                }
            }
        }
        catch (Exception ex)
        {
            Log($"[FATAL]: {ex}");
        }
    }

    private static object[] GetToolDefinitions()
    {
        return
        [
            new
            {
                name = "get_code_structure",
                description = "Parses C# code and returns a flat list of structure items (Classes, Methods, Properties).",
                inputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        path = new { type = "string", description = "File path (optional if sessionId provided)" },
                        sessionId = new { type = "string", description = "Session ID (optional if path provided)" }
                    }
                }
            },
            new
            {
                name = "get_method",
                description = "Extracts the body of a specific method.",
                inputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        path = new { type = "string" },
                        sessionId = new { type = "string" },
                        methodName = new { type = "string" }
                    },
                    required = new[] { "methodName" }
                }
            },
            new
            {
                name = "update_method",
                description = "Updates the body of a specific method.",
                inputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        path = new { type = "string" },
                        sessionId = new { type = "string" },
                        methodName = new { type = "string" },
                        newBody = new { type = "string", description = "New method body content (statements only)" }
                    },
                    required = new[] { "methodName", "newBody" }
                }
            },
            new
            {
                name = "add_using",
                description = "Adds a using directive if missing.",
                inputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        path = new { type = "string" },
                        sessionId = new { type = "string" },
                        @namespace = new { type = "string" }
                    },
                    required = new[] { "namespace" }
                }
            },
            new
            {
                name = "start_csharp_session",
                description = "Starts a new in-memory editing session.",
                inputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        path = new { type = "string", description = "Initial file path to load" }
                    }
                }
            },
            new
            {
                name = "update_csharp_session",
                description = "Updates the content of a session.",
                inputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        sessionId = new { type = "string" },
                        content = new { type = "string" }
                    },
                    required = new[] { "sessionId", "content" }
                }
            },
            new
            {
                name = "save_csharp_session",
                description = "Saves the session content to disk.",
                inputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        sessionId = new { type = "string" }
                    },
                    required = new[] { "sessionId" }
                }
            },
            new
            {
                name = "split_razor_file",
                description = "Splits a .razor file into .razor, .razor.cs, and .razor.css.",
                inputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        path = new { type = "string", description = "Path to the .razor file" }
                    },
                    required = new[] { "path" }
                }
            }
        ];
    }

    private static object HandleToolCall(JsonElement paramsEl)
    {
        string name = paramsEl.GetProperty("name").GetString() ?? "";
        JsonElement args = paramsEl.GetProperty("arguments");

        string resultText = name switch
        {
            "get_code_structure" => ToolHandlers.HandleGetCodeStructure(args),
            "get_method" => ToolHandlers.HandleGetMethod(args),
            "update_method" => ToolHandlers.HandleUpdateMethod(args),
            "add_using" => ToolHandlers.HandleAddUsing(args),
            "start_csharp_session" => ToolHandlers.HandleStartSession(args),
            "update_csharp_session" => ToolHandlers.HandleUpdateSession(args),
            "save_csharp_session" => ToolHandlers.HandleSaveSession(args),
            "split_razor_file" => ToolHandlers.HandleSplitRazorFile(args),
            _ => throw new Exception($"Unknown tool: {name}")
        };

        return new { content = new[] { new { type = "text", text = resultText } } };
    }

    private static void SendResponse(JsonRpcResponse response)
    {
        string json = JsonSerializer.Serialize(response, _jsonOptions);
        Log($"[SEND]: {json}");
        Console.Write(json + "\n");
        Console.Out.Flush();
    }

    private static void Log(string message)
    {
        try { File.AppendAllText(_logPath, $"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}"); }
        catch { }
    }
}


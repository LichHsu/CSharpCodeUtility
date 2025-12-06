using System.Text;
using System.Text.Json;
using CSharpCodeUtility.MCP;
using Lichs.MCP.Core;

namespace CSharpCodeUtility;

internal class Program
{
    private static readonly string _logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mcp_debug_log.txt");

    static async Task Main(string[] args)
    {
        Console.OutputEncoding = new UTF8Encoding(false);
        Console.InputEncoding = new UTF8Encoding(false);

        if (args.Length > 0)
        {
            if (args[0] == "--test")
            {
                CSharpCodeUtility.Testing.TestRunner.RunAllTests();
                return;
            }

            if (args[0] == "fix-namespace")
            {
                // Usage: fix-namespace <directory> <projectRoot> <rootNamespace> [extraUsings...]
                string directory = args[1];
                string projectRoot = args[2];
                string rootNamespace = args[3];
                List<string>? extraUsings = args.Length > 4 ? args.Skip(4).ToList() : null;

                var files = Directory.GetFiles(directory, "*.cs", SearchOption.AllDirectories);
                int fixedCount = 0;

                foreach (var file in files)
                {
                    string code = File.ReadAllText(file);
                    string newCode = CSharpCodeUtility.Core.CsharpRefactorer.FixNamespaceAndUsings(code, file, projectRoot, rootNamespace, extraUsings);
                    
                    if (code != newCode)
                    {
                        File.WriteAllText(file, newCode);
                        fixedCount++;
                    }
                }
                Console.WriteLine($"Fixed namespaces and usings in {fixedCount} files.");
                return;
            }
        }

        var server = new McpServer("csharp-code-utility", "1.0.0");
        RegisterTools(server);
        await server.RunAsync(args);
    }

    private static void RegisterTools(McpServer server)
    {
        server.RegisterTool("get_code_structure",
            "Parses C# code and returns a flat list of structure items (Classes, Methods, Properties).",
            new { type = "object", properties = new { path = new { type = "string", description = "File path (optional if sessionId provided)" }, sessionId = new { type = "string", description = "Session ID (optional if path provided)" } } },
            args => ToolHandlers.HandleGetCodeStructure(args));

        server.RegisterTool("get_method",
            "Extracts the body of a specific method.",
            new { type = "object", properties = new { path = new { type = "string" }, sessionId = new { type = "string" }, methodName = new { type = "string" } }, required = new[] { "methodName" } },
            args => ToolHandlers.HandleGetMethod(args));

        server.RegisterTool("update_method",
            "Updates the body of a specific method.",
            new { type = "object", properties = new { path = new { type = "string" }, sessionId = new { type = "string" }, methodName = new { type = "string" }, newBody = new { type = "string", description = "New method body content (statements only)" } }, required = new[] { "methodName", "newBody" } },
            args => ToolHandlers.HandleUpdateMethod(args));

        server.RegisterTool("add_using",
            "Adds a using directive if missing.",
            new { type = "object", properties = new { path = new { type = "string" }, sessionId = new { type = "string" }, @namespace = new { type = "string" } }, required = new[] { "namespace" } },
            args => ToolHandlers.HandleAddUsing(args));

        server.RegisterTool("start_csharp_session",
            "Starts a new in-memory editing session.",
            new { type = "object", properties = new { path = new { type = "string", description = "Initial file path to load" } } },
            args => ToolHandlers.HandleStartSession(args));

        server.RegisterTool("update_csharp_session",
            "Updates the content of a session.",
            new { type = "object", properties = new { sessionId = new { type = "string" }, content = new { type = "string" } }, required = new[] { "sessionId", "content" } },
            args => ToolHandlers.HandleUpdateSession(args));

        server.RegisterTool("save_csharp_session",
            "Saves the session content to disk.",
            new { type = "object", properties = new { sessionId = new { type = "string" } }, required = new[] { "sessionId" } },
            args => ToolHandlers.HandleSaveSession(args));

        server.RegisterTool("fix_namespace_and_usings",
            "Fixes namespace mismatches and adds missing usings.",
            new { type = "object", properties = new { directory = new { type = "string", description = "Directory to scan" }, projectRoot = new { type = "string", description = "Project root directory" }, rootNamespace = new { type = "string", description = "Root namespace of the project" }, extraUsings = new { type = "array", items = new { type = "string" }, description = "List of extra usings to add" } }, required = new[] { "directory", "projectRoot", "rootNamespace" } },
            args => ToolHandlers.HandleFixNamespaceAndUsings(args));
    }

    private static void Log(string message)
    {
        try { File.AppendAllText(_logPath, $"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}"); }
        catch { }
    }
}

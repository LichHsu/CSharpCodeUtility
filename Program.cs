using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using CSharpCodeUtility.MCP;
using CSharpCodeUtility.Operations;
using Lichs.MCP.Core;
using Lichs.MCP.Core.Attributes;

namespace CSharpCodeUtility;

public class Program
{
    private static readonly string _logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mcp_debug_log.txt");
    private static readonly JsonSerializerOptions _jsonPrettyOptions = new() { WriteIndented = true };

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
                if (args.Length < 4)
                {
                    Console.WriteLine("Usage: fix-namespace <directory> <projectRoot> <rootNamespace> [extraUsings...]");
                    return;
                }

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
        
        // 自動掃描 [McpTool]
        server.RegisterToolsFromAssembly(System.Reflection.Assembly.GetExecutingAssembly());
        
        await server.RunAsync(args);
    }

    // --- New Tools ---

    [McpTool("get_project_references", "分析指定目錄或解決方案下的專案依賴關係圖。")]
    public static string GetProjectReferences([McpParameter("解決方案路徑 (.sln/.slnx) 或根目錄")] string rootPath)
    {
        var refs = ProjectDependencyAnalyzer.GetProjectReferences(rootPath);
        return JsonSerializer.Serialize(refs, _jsonPrettyOptions);
    }

    [McpTool("find_symbol_definition", "在專案中快速搜尋符號 (Class/Method/Property) 的定義位置 (檔案與行號)。")]
    public static string FindSymbolDefinition(
        [McpParameter("搜尋根目錄")] string rootPath,
        [McpParameter("符號名稱 (例如 'MyClass', 'GetId')")] string symbolName)
    {
        var locs = SymbolDefinitionFinder.FindSymbolDefinition(rootPath, symbolName);
        return JsonSerializer.Serialize(locs, _jsonPrettyOptions);
    }

    // --- Existing Tools Wrapper (if needed, but ToolHandlers.cs/Program.cs usually handle this) ---
    // Note: If previous handlers were migrated to static methods in Program.cs, they should be here.
    // However, in the CSharpCodeUtility migration task (Step 6.4), we updated ToolHandlers.cs to have [McpTool] attributes and used RegisterToolsFromAssembly.
    // So we DON'T need to duplicate them here. Just the new tools are enough if they are in Program.cs.
    // Or we can move these new tools to a separate NewTools.cs file to keep Program.cs clean.
    // Let's put them here for now as requested.
}

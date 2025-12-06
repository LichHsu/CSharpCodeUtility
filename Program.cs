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
        
        // 自動掃描 [McpTool]
        server.RegisterToolsFromAssembly(System.Reflection.Assembly.GetExecutingAssembly());
        
        await server.RunAsync(args);
    }
}

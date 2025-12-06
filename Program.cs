using CSharpCodeUtility.Core;
using CSharpCodeUtility.Models;
using CSharpCodeUtility.Operations;
using Lichs.MCP.Core;
using Lichs.MCP.Core.Attributes;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CSharpCodeUtility;

public class Program
{
    private static readonly JsonSerializerOptions _jsonPrettyOptions = new()
    {
        WriteIndented = true,
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

        var server = new McpServer("csharp-code-utility", "2.1.0");
        server.RegisterToolsFromAssembly(System.Reflection.Assembly.GetExecutingAssembly());
        await server.RunAsync(args);
    }

    [McpTool("analyze_csharp", "分析 C# 專案結構、符號定義與參考引用。")]
    public static string AnalyzeCsharp(
        [McpParameter("分析目標路徑 (檔案或目錄)")] string path,
        [McpParameter("分析類型 (Structure, References, SymbolDefinition, Usages)")] string analysisType,
        [McpParameter("搜尋字串 (用於 FindSymbol 或 FindReferences)", false)] string? query = null)
    {
        // ... (Logic remains mostly same, just checking enum/string match)
        if (analysisType.Equals("Structure", StringComparison.OrdinalIgnoreCase))
        {
            string content = File.ReadAllText(path);
            var structure = CsharpParser.GetStructure(content);
            return JsonSerializer.Serialize(structure, _jsonPrettyOptions);
        }
        else if (analysisType.Equals("References", StringComparison.OrdinalIgnoreCase))
        {
            var refs = ProjectDependencyAnalyzer.GetProjectReferences(path);
            return JsonSerializer.Serialize(refs, _jsonPrettyOptions);
        }
        else if (analysisType.Equals("SymbolDefinition", StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrEmpty(query)) throw new ArgumentException("查詢字串 (query) 不可為空");
            var locs = SymbolDefinitionFinder.FindSymbolDefinition(path, query);
            return JsonSerializer.Serialize(locs, _jsonPrettyOptions);
        }
        else if (analysisType.Equals("Usages", StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrEmpty(query)) throw new ArgumentException("查詢字串 (query) 不可為空");
            var refs = ReferenceFinder.FindReferences(path, query);
            return JsonSerializer.Serialize(refs, _jsonPrettyOptions);
        }

        throw new ArgumentException($"未知的分析類型: {analysisType}");
    }

    [McpTool("inspect_csharp", "讀取特定的 C# 程式碼片段。")]
    public static string InspectCsharp(
        [McpParameter("C# 檔案路徑")] string path,
        [McpParameter("元素名稱")] string elementName,
        [McpParameter("元素類型 (Method, Property)", false)] string elementType = "Method")
    {
        if (!File.Exists(path)) throw new FileNotFoundException("File not found", path);
        string content = File.ReadAllText(path);

        if (elementType.Equals("Method", StringComparison.OrdinalIgnoreCase))
        {
            var body = CsharpParser.GetMethodBody(content, elementName);
            return body ?? $"Method '{elementName}' not found.";
        }
        return "Unsupported element type.";
    }

    [McpTool("edit_csharp", "修改 C# 程式碼。")]
    public static string EditCsharp(
        [McpParameter("目標路徑 (檔案或目錄)")] string path,
        [McpParameter("操作類型 (UpdateMethod, AddUsing, FixNamespace)")] string operation,
        [McpParameter("內容 (MethodBody, Namespace, etc.)", false)] string? content = null,
        [McpParameter("選項參數", false)] CSharpEditOptions? options = null)
    {
        options ??= new CSharpEditOptions();

        if (operation.Equals("UpdateMethod", StringComparison.OrdinalIgnoreCase))
        {
            if (!File.Exists(path)) throw new FileNotFoundException("File not found", path);

            // Legacy behavior: content was methodName, newBody was in options?
            // Let's standardize: content = NewBody? Or content = MethodName? 
            // To be extremely clear: Use Options where possible.

            string methodName = options.MethodName ?? content ?? "";
            string newBody = options.NewBody ?? "";

            // Wait, if content was used as MethodName in previous turn.
            // Let's stick to safe fallback.

            if (string.IsNullOrEmpty(methodName)) throw new ArgumentException("MethodName required");
            if (string.IsNullOrEmpty(newBody)) throw new ArgumentException("NewBody required");

            string code = File.ReadAllText(path);
            string newCode = CsharpModifier.UpdateMethodBody(code, methodName, newBody);
            File.WriteAllText(path, newCode);
            return $"Updated method '{methodName}' in {path}";
        }
        else if (operation.Equals("AddUsing", StringComparison.OrdinalIgnoreCase))
        {
            if (!File.Exists(path)) throw new FileNotFoundException("File not found", path);
            string code = File.ReadAllText(path);
            string newCode = CsharpModifier.AddUsing(code, content ?? ""); // content is namespace
            File.WriteAllText(path, newCode);
            return $"Added using '{content}' to {path}";
        }
        else if (operation.Equals("FixNamespace", StringComparison.OrdinalIgnoreCase))
        {
            if (!Directory.Exists(path)) throw new DirectoryNotFoundException("Path must be a directory");

            string projectRoot = options.ProjectRoot ?? "";
            string rootNamespace = options.RootNamespace ?? "";

            var files = Directory.GetFiles(path, "*.cs", SearchOption.AllDirectories);
            int count = 0;
            foreach (var file in files)
            {
                string code = File.ReadAllText(file);
                string newCode = CsharpRefactorer.FixNamespaceAndUsings(code, file, projectRoot, rootNamespace, null);
                if (code != newCode)
                {
                    File.WriteAllText(file, newCode);
                    count++;
                }
            }
            return $"Fixed namespaces in {count} files.";
        }

        throw new ArgumentException($"未知的操作類型: {operation}");
    }
}

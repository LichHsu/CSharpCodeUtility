using System.Text.RegularExpressions;

namespace CSharpCodeUtility.Operations;

public static class SymbolDefinitionFinder
{
    public class SymbolLocation
    {
        public string SymbolName { get; set; } = "";
        public string FilePath { get; set; } = "";
        public int LineNumber { get; set; }
        public string DefinitionLine { get; set; } = "";
        public string Type { get; set; } = "unknown"; // class, interface, method, property
    }

    /// <summary>
    /// 在指定目錄中搜尋符號定義 (Class, Method, Property)
    /// 注意：這是基於 Regex 的輕量級搜尋，不是完整的 Roslyn 語義分析，速度快但可能不完美。
    /// </summary>
    public static List<SymbolLocation> FindSymbolDefinition(string rootPath, string symbolName)
    {
        var results = new List<SymbolLocation>();
        if (!Directory.Exists(rootPath)) return results;

        var files = Directory.GetFiles(rootPath, "*.cs", SearchOption.AllDirectories);

        // 最佳化 Regex：匹配 "public class MySymbol", "void MySymbol(", "MySymbol ="
        // 排除 "new MySymbol", "MySymbol." 等使用處

        // Strategy:
        // 1. Class/Interface: (class|interface|record|struct)\s+SymbolName\b
        // 2. Method: \s+SymbolName\s*\(
        // 3. Property: \s+SymbolName\s*\{

        // 這裡使用三個特定模式來提高精確度
        var classPattern = new Regex($@"\b(class|interface|record|struct|enum)\s+{Regex.Escape(symbolName)}\b", RegexOptions.Compiled);
        var methodPattern = new Regex($@"\b{Regex.Escape(symbolName)}\s*\(", RegexOptions.Compiled); // func name(
        var propPattern = new Regex($@"\b{Regex.Escape(symbolName)}\s*{{", RegexOptions.Compiled);   // PropName { get;

        foreach (var file in files)
        {
            // 忽略 bin/obj 目錄
            if (file.Contains(Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar) ||
                file.Contains(Path.DirectorySeparatorChar + "obj" + Path.DirectorySeparatorChar)) continue;

            var lines = File.ReadLines(file).ToArray();
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (string.IsNullOrEmpty(line) || line.StartsWith("//")) continue;

                string type = null;
                if (classPattern.IsMatch(line)) type = "class/type";
                else if (methodPattern.IsMatch(line) && !line.StartsWith("new ")) type = "method";
                // 排除 "new Foo()" 調用
                else if (propPattern.IsMatch(line)) type = "property";

                if (type != null)
                {
                    results.Add(new SymbolLocation
                    {
                        SymbolName = symbolName,
                        FilePath = file,
                        LineNumber = i + 1,
                        DefinitionLine = lines[i].Trim(), // 原行內容
                        Type = type
                    });
                }
            }
        }

        return results;
    }
}

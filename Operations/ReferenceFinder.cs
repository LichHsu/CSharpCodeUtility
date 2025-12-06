using System.Text.RegularExpressions;

namespace CSharpCodeUtility.Operations;

public static class ReferenceFinder
{
    /// <summary>
    /// Finds all references to a specific symbol (class, method, etc.) in a directory.
    /// Uses Regex for fast approximation.
    /// </summary>
    public static List<ReferenceResult> FindReferences(string rootPath, string symbolName)
    {
        var results = new List<ReferenceResult>();
        
        if (!Directory.Exists(rootPath)) return results;

        var files = Directory.GetFiles(rootPath, "*.cs", SearchOption.AllDirectories);
        
        // Basic pattern: word boundary + symbol name + word boundary
        // avoiding matches where it's part of another word
        var regex = new Regex($@"\b{Regex.Escape(symbolName)}\b", RegexOptions.Compiled);

        foreach (var file in files)
        {
            // Skip automated directories
            if (file.Contains("\\bin\\") || file.Contains("\\obj\\") || file.Contains("\\.git\\")) continue;

            string content = File.ReadAllText(file);
            var matches = regex.Matches(content);

            foreach (Match match in matches)
            {
                // Simple line number calculation
                int lineNumber = content.Take(match.Index).Count(c => c == '\n') + 1;
                string lineContent = GetLineContent(content, match.Index);

                results.Add(new ReferenceResult
                {
                    FilePath = file,
                    LineNumber = lineNumber,
                    LineContent = lineContent.Trim()
                });
            }
        }

        return results;
    }

    private static string GetLineContent(string content, int index)
    {
        int start = content.LastIndexOf('\n', index) + 1;
        int end = content.IndexOf('\n', index);
        if (end == -1) end = content.Length;
        return content.Substring(start, end - start);
    }
}

public class ReferenceResult
{
    public string FilePath { get; set; } = "";
    public int LineNumber { get; set; }
    public string LineContent { get; set; } = "";
}

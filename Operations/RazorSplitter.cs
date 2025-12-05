using CSharpCodeUtility.Core;

namespace CSharpCodeUtility.Operations;

public static class RazorSplitter
{
    public static string SplitFile(string razorPath)
    {
        if (!File.Exists(razorPath))
        {
            throw new FileNotFoundException("Razor file not found.", razorPath);
        }

        string content = File.ReadAllText(razorPath);
        string fileNameWithoutExt = Path.GetFileNameWithoutExtension(razorPath);
        string directory = Path.GetDirectoryName(razorPath) ?? "";

        // 1. Extract Code
        string? codeBlock = RazorParser.ExtractCodeBlock(content);
        if (!string.IsNullOrWhiteSpace(codeBlock))
        {
            string csPath = Path.Combine(directory, $"{fileNameWithoutExt}.razor.cs");
            
            // Try to find namespace
            string namespaceName = "MyApp.Components"; // Default
            // TODO: Extract namespace from @namespace or infer from folder structure? 
            // For now, let's look for @namespace directive in the file
            // or just wrap it in a namespace if we can infer it.
            // Actually, usually .razor.cs is a partial class.
            
            // Let's assume the user wants the content inside the class.
            // But wait, @code block usually contains members of the class.
            // So we need to wrap it in:
            // namespace [Namespace] { public partial class [ClassName] { [Content] } }
            
            // Infer namespace from file content if possible
            // Simple check for @namespace
            var namespaceMatch = System.Text.RegularExpressions.Regex.Match(content, @"@namespace\s+([\w\.]+)");
            if (namespaceMatch.Success)
            {
                namespaceName = namespaceMatch.Groups[1].Value;
            }

            string classContent = $@"using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Components;

namespace {namespaceName}
{{
    public partial class {fileNameWithoutExt}
    {{
{codeBlock}
    }}
}}";
            File.WriteAllText(csPath, classContent);
        }

        // 2. Extract Style
        string? styleBlock = RazorParser.ExtractStyleBlock(content);
        if (!string.IsNullOrWhiteSpace(styleBlock))
        {
            string cssPath = Path.Combine(directory, $"{fileNameWithoutExt}.razor.css");
            File.WriteAllText(cssPath, styleBlock);
        }

        // 3. Update .razor file
        string newRazorContent = RazorParser.RemoveBlocks(content);
        File.WriteAllText(razorPath, newRazorContent);

        return $"Successfully split {fileNameWithoutExt}.razor";
    }
}

using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace CSharpCodeUtility.Operations;

public static class ProjectDependencyAnalyzer
{
    public class ProjectNode
    {
        public string Name { get; set; } = "";
        public string Path { get; set; } = "";
        public List<string> Dependencies { get; set; } = new();
    }

    /// <summary>
    /// 分析方案檔或目錄下的專案依賴關係
    /// </summary>
    public static List<ProjectNode> GetProjectReferences(string rootPath)
    {
        var projects = new List<ProjectNode>();
        string[] projectFiles;

        if (File.Exists(rootPath) && (rootPath.EndsWith(".sln") || rootPath.EndsWith(".slnx")))
        {
            // 解析解決方案檔案 (簡易版：讀取內容中的路徑)
            // 由於 SLN 格式複雜，這裡簡化為：掃描 rootPath 所在目錄下的所有 csproj
            string dir = Path.GetDirectoryName(rootPath)!;
            projectFiles = Directory.GetFiles(dir, "*.csproj", SearchOption.AllDirectories);
        }
        else if (Directory.Exists(rootPath))
        {
            projectFiles = Directory.GetFiles(rootPath, "*.csproj", SearchOption.AllDirectories);
        }
        else
        {
            throw new FileNotFoundException("找不到解決方案或目錄", rootPath);
        }

        foreach (var projPath in projectFiles)
        {
            var node = new ProjectNode
            {
                Name = Path.GetFileNameWithoutExtension(projPath),
                Path = projPath
            };

            try
            {
                var doc = XDocument.Load(projPath);
                var refs = doc.Descendants("ProjectReference")
                    .Select(e => e.Attribute("Include")?.Value)
                    .Where(v => v != null)
                    .Select(v => Path.GetFileNameWithoutExtension(v!)) // 正規化為專案名稱
                    .ToList();
                
                node.Dependencies.AddRange(refs);
            }
            catch 
            {
                // 忽略解析錯誤 (例如無效 XML)
            }

            projects.Add(node);
        }

        return projects;
    }
}

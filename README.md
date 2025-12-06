# CSharpCodeUtility - C# 程式碼操作 MCP 伺服器

> **Part of Lichs.MCP Workspace**

`CSharpCodeUtility` 是一個基於 Roslyn 的 C# 程式碼操作工具，專為 AI Agent 設計。它提供語法感知的修改能力，並支援「記憶體內工作階段」，避免頻繁 I/O 與中間狀態錯誤。

本專案基於 **Lichs.MCP.Core** 構建。

## 🌟 核心特色

*   **結構化視圖**: `get_code_structure` 提供類別、方法、屬性的扁平化索引。
*   **局部讀取**: `get_method` 僅讀取關注的方法實作，節省 Token。
*   **語法感知修改**: `update_method` 使用 Roslyn 進行安全替換，確保大括號平衡與語法正確。
*   **智慧引用**: `add_using` 與 `fix_namespace_and_usings` 自動管理 Namespace 與 Usings。
*   **工作階段管理**: 
    *   `start_csharp_session`: 開啟 Session。
    *   `update_method` (with sessionId): 在 Session 中修改。
    *   `save_csharp_session`: 一次性寫入磁碟。

## 📦 安裝與配置

### 建置
```bash
cd "d:\Lichs Projects\MCP"
dotnet build Lichs.MCP.slnx
```

### MCP 客戶端配置
```json
{
  "mcpServers": {
    "csharp-utility": {
      "command": "dotnet",
      "args": ["d:\\Lichs Projects\\MCP\\CSharpCodeUtility\\bin\\Debug\\net10.0\\CSharpCodeUtility.dll"]
    }
  }
}
```

## 💻 CLI 模式

- **修正命名空間**: `dotnet run -- fix-namespace <directory> <projectRoot> <rootNamespace> [extraUsings...]`

---
*Powered by Lichs.MCP.Core*

# CSharpCodeUtility - C# 程式碼處理 MCP 伺服器

`CSharpCodeUtility` 是一個專為 AI Agent 設計的 Model Context Protocol (MCP) 伺服器，旨在提供高效、低 Token 消耗的 C# 原始碼操作能力。它利用 Roslyn (Microsoft.CodeAnalysis) 進行精確的語法分析，並支援「記憶體內工作階段 (In-Memory Session)」，避免頻繁的磁碟讀寫。

## 功能特色

*   **結構化解析 (`get_code_structure`)**：快速取得程式碼的扁平化結構（類別、方法、屬性），適合 AI 快速理解檔案全貌。
*   **精確讀取 (`get_method`)**：僅讀取特定方法的實作內容，大幅節省 Context Window。
*   **語法感知修改 (`update_method`)**：使用 Roslyn 進行方法的安全替換，確保格式正確。
*   **智慧引用 (`add_using`)**：自動檢查並添加缺少的 `using` 指令。
*   **記憶體內工作階段 (`Session Management`)**：
    *   支援在記憶體中進行多次修改，最後再一次性寫入磁碟。
    *   避免因中間狀態錯誤而破壞原始檔案。

## 設計理念：Agent-First (AI 優先)

本工具的核心設計目標是**消除 AI 在編輯程式碼時常見的錯誤**：

1.  **避免語法錯誤**：AI 常因漏寫大括號 `}` 或分號 `;` 導致編譯失敗。本工具透過 Roslyn 語法樹操作，確保結構完整性。
2.  **減少 Token 消耗**：AI 不需要讀取整個檔案來修改一個小方法。`get_method` 讓 AI 專注於局部，大幅提升上下文效率。
3.  **防止幻覺 (Hallucination)**：透過 `get_code_structure` 提供的精確索引，AI 能準確定位目標，避免修改不存在的程式碼。
4.  **原子化操作**：所有的修改都是針對特定節點（如 Method, Property），而非脆弱的行號或字串替換。

這是一個**給 AI 使用的 Power Tool**，旨在讓 AI 成為更可靠的 C# 工程師。

## 安裝與執行

本專案為 .NET 10.0 Console 應用程式。

### 建置
```bash
dotnet build
```

### 執行測試
```bash
dotnet run -- --test
```

### 作為 MCP 伺服器執行
```bash
dotnet run
```
（請將此指令配置於您的 MCP 客戶端設定檔中）

## 可用工具 (Tools)

### 1. `get_code_structure`
解析 C# 程式碼並回傳結構列表。

*   **參數**:
    *   `path` (string, optional): 檔案路徑。
    *   `sessionId` (string, optional): 工作階段 ID。
*   **回傳**: `List<CsharpCodeItem>` (包含 Type, Name, Signature, StartLine, EndLine 等)。

### 2. `get_method`
取得特定方法的實作內容。

*   **參數**:
    *   `methodName` (string): 方法名稱。
    *   `path` / `sessionId`: 來源。
*   **回傳**: 方法的完整程式碼字串。

### 3. `update_method`
更新特定方法的實作內容。

*   **參數**:
    *   `methodName` (string): 目標方法名稱。
    *   `newBody` (string): 新的方法本體 (不包含大括號外的簽章，僅內部陳述句)。
    *   `path` / `sessionId`: 目標。
*   **回傳**: 更新結果訊息。

### 4. `add_using`
添加 `using` 指令。

*   **參數**:
    *   `namespace` (string): 要引入的命名空間 (例如 "System.IO")。
    *   `path` / `sessionId`: 目標。

### 5. 工作階段管理 (Session Management)
*   `start_csharp_session`: 開始一個新的編輯工作階段 (可載入檔案)。
*   `update_csharp_session`: 直接更新工作階段內容。
*   `save_csharp_session`: 將工作階段內容寫回磁碟。

## 使用範例 (JSON-RPC)

### 讀取檔案結構
```json
{
  "name": "get_code_structure",
  "arguments": {
    "path": "D:\\Projects\\MyClass.cs"
  }
}
```

### 開始工作階段並修改方法
```json
// 1. Start Session
{
  "name": "start_csharp_session",
  "arguments": { "path": "D:\\Projects\\MyClass.cs" }
}
// Returns: { "id": "session-123", ... }

// 2. Update Method
{
  "name": "update_method",
  "arguments": {
    "sessionId": "session-123",
    "methodName": "Calculate",
    "newBody": "return x + y;"
  }
}

// 3. Save
{
  "name": "save_csharp_session",
  "arguments": { "sessionId": "session-123" }
}
```

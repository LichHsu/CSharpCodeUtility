# CSharpCodeUtility

A powerful MCP server for analyzing, inspecting, and editing C# codebases.

## Tools

### 1. `analyze_csharp`
Analyzes C# project structure and references.
*   **Parameters**:
    *   `path` (string): Path to file or directory.
    *   `analysisType` (string):
        *   `Structure`: Parses file structure (Classes, Methods).
        *   `References`: Analyzes Project-to-Project references.
        *   `SymbolDefinition`: Finds where a symbol is defined (`find_symbol`).
        *   `Usages`: (**NEW**) Finds all references/usages of a symbol in the project (`find_references`).
    *   `query` (string, optional): The symbol name or search term.

### 2. `inspect_csharp`
Inspects specific code elements.
*   **Parameters**:
    *   `path` (string): Path to C# file.
    *   `elementName` (string): Name of the method or property.
    *   `elementType` (string): `Method` (default) or `Property`.

### 3. `edit_csharp`
Modifies C# code.
*   **Parameters**:
    *   `path` (string): Target file or directory.
    *   `operation` (string):
        *   `UpdateMethod`: Replaces the body of a method.
        *   `AddUsing`: Adds a using directive if missing.
        *   `FixNamespace`: Batch fixes namespaces to match folder structure.
    *   `content` (string): The primary data (MethodName, Namespace, etc.).
    *   `optionsJson` (json string): `{ "newBody": "...", "projectRoot": "..." }`.

## Development
Run `dotnet build` to compile.
Run `CSharpCodeUtility.exe --test` to execute internal unit tests.

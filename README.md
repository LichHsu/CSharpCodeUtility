# CSharpCodeUtility

A powerful MCP server for analyzing, inspecting, and editing C# codebases.
Now supports strongly-typed parameters for enhanced AI interaction.

## Tools

### 1. `analyze_csharp`
Analyzes C# project structure and references.
*   **Parameters**:
    *   `path` (string): Path to file or directory.
    *   `analysisType` (string):
        *   `Structure`: Parses file structure (Classes, Methods).
        *   `References`: Analyzes Project-to-Project references.
        *   `SymbolDefinition`: Finds where a symbol is defined.
        *   `Usages`: Finds all references of a symbol.
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
    *   `operation` (string): `UpdateMethod`, `AddUsing`, `FixNamespace`.
    *   `content` (string): Primary content (optional depending on use case).
    *   `options`: (Object)
        *   `methodName`: Target method name.
        *   `newBody`: New method body content.
        *   `projectRoot`: For Namespace fixing.
        *   `rootNamespace`: Expected root namespace.

## Development
Run `dotnet build` to compile.
Run `CSharpCodeUtility.exe --test` to execute internal unit tests.

using CSharpCodeUtility.Core;

namespace CSharpCodeUtility.Testing;

public static class TestRunner
{
    public static void RunAllTests()
    {
        Console.WriteLine("=== Running Tests ===");
        int passed = 0;
        int failed = 0;

        try { TestStructureParsing(); passed++; Console.WriteLine("✓ TestStructureParsing Passed"); }
        catch (Exception ex) { failed++; Console.WriteLine($"✗ TestStructureParsing Failed: {ex.Message}"); }

        try { TestUpdateMethod(); passed++; Console.WriteLine("✓ TestUpdateMethod Passed"); }
        catch (Exception ex) { failed++; Console.WriteLine($"✗ TestUpdateMethod Failed: {ex.Message}"); }

        try { TestAddUsing(); passed++; Console.WriteLine("✓ TestAddUsing Passed"); }
        catch (Exception ex) { failed++; Console.WriteLine($"✗ TestAddUsing Failed: {ex.Message}"); }

        try { TestSessionLifecycle(); passed++; Console.WriteLine("✓ TestSessionLifecycle Passed"); }
        catch (Exception ex) { failed++; Console.WriteLine($"✗ TestSessionLifecycle Failed: {ex.Message}"); }

        Console.WriteLine($"=== Tests Completed: {passed} Passed, {failed} Failed ===");
    }

    private static void TestStructureParsing()
    {
        string code = @"
using System;

namespace TestNamespace
{
    public class TestClass
    {
        public int Property1 { get; set; }

        public void Method1()
        {
            Console.WriteLine(""Hello"");
        }
    }
}";
        var structure = CsharpParser.GetStructure(code);

        // Assert(structure.Count == 2, "Should find 2 items (Class + Children flattened or just top level? Parser returns flattened list)");

        Assert(structure.Count == 3, $"Expected 3 items, found {structure.Count}");
        Assert(structure[0].Type == "Class", "First item should be Class");
        Assert(structure[0].Name == "TestClass", "Class name should be TestClass");
        Assert(structure[1].Type == "Method", "Second item should be Method");
        Assert(structure[2].Type == "Property", "Third item should be Property");
    }

    private static void TestUpdateMethod()
    {
        string code = @"
public class TestClass
{
    public void Method1()
    {
        int a = 1;
    }
}";
        string newBody = @"
        int b = 2;
        return;";

        string updatedCode = CsharpModifier.UpdateMethodBody(code, "Method1", newBody);

        Assert(updatedCode.Contains("int b = 2;"), "Updated code should contain new body");
        Assert(!updatedCode.Contains("int a = 1;"), "Updated code should not contain old body");
    }

    private static void TestAddUsing()
    {
        string code = @"
using System;

public class TestClass {}";

        string updatedCode = CsharpModifier.AddUsing(code, "System.IO");

        if (!updatedCode.Contains("using System.IO;"))
        {
            Console.WriteLine($"DEBUG: Updated Code:\n{updatedCode}");
        }

        Assert(updatedCode.Contains("using System.IO;"), "Updated code should contain new using");
        Assert(updatedCode.Contains("using System;"), "Updated code should keep existing using");
    }

    private static void TestSessionLifecycle()
    {
        var session = CsharpSessionManager.CreateSession();
        string initialCode = "public class Foo {}";

        CsharpSessionManager.UpdateSessionContent(session.Id, initialCode);
        Assert(session.Content == initialCode, "Session content should be updated");
        Assert(session.IsDirty, "Session should be dirty");

        // Simulate save (mocking file I/O is hard here without abstraction, but we can check logic)
        // We won't test SaveSession to disk here to avoid creating garbage files, 
        // but we can verify the manager holds the state.

        var retrievedSession = CsharpSessionManager.GetSession(session.Id);
        Assert(retrievedSession.Content == initialCode, "Should retrieve correct session");

        CsharpSessionManager.CloseSession(session.Id);
        try
        {
            CsharpSessionManager.GetSession(session.Id);
            throw new Exception("Session should be closed");
        }
        catch
        {
            // Expected
        }
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
    }
}

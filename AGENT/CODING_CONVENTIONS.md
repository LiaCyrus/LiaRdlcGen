# Coding Conventions (Compact C# Style)

Follow a compact C# formatting style.

1. **Keep method parameters on one line** whenever reasonably possible.
   ```csharp
   public void Generate(string templatePath, string outputPath, RdlcSettings settings)
   {
   }
   ```
   Avoid line‑breaks for short parameter lists.
   ```csharp
   public void Generate(
       string templatePath,
       string outputPath,
       RdlcSettings settings)
   {
   }
   ```

2. **Keep constructor parameters on one line** when reasonably possible.

3. **Keep short property declarations and simple expressions on one line**.
   ```csharp
   public string Name { get; set; } = "";
   public int Count => items.Count;
   ```

4. **Introduce line breaks only when necessary**:
   - The line becomes excessively long.
   - The expression is difficult to read.
   - Multiple logical operations need separation.
   - A collection or object initializer is clearer across multiple lines.

5. **Use consistent indentation** (4 spaces) and standard C# naming conventions (PascalCase for types and members, camelCase for parameters, `_` prefix for private fields if desired).

6. **Do not unnecessarily split method calls, parameter lists, LINQ expressions, or property declarations** across multiple lines.

7. **Do not remove or modify existing comments without a specific reason.** Preserve comments when editing nearby code. Change or remove a comment only when it is incorrect, obsolete, misleading, or directly affected by the code change.

**General rule:** Prefer compact, readable code over vertical expansion. Do not sacrifice readability merely to keep everything on one line.


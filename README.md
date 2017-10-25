# SqlScriptGenerator
Generates SQL scripts from database metadata.

## Template Switches
Lines starting with --# denote a template switch.

Switch | Args | Use
------ | ---- | ---
FILESPEC | filespec | Indicates the path from the output root folder to the script file to write. The path can contain substitution items.

## C# Content
Blocks of C# content can be placed between /&#42;- and -&#42;/. The block start and end lines must be on lines of their own.

Lines starting with --; denote single lines of C# code, with everything following the --; being C#.

## Inline Substitutions
Substitutions are enclosed in &#123;braces&#125;. Everything within the braces is a C# expression, the ToString() of the expression is written to the script.

## Built-in Functions
### Loop
**Loop(IEnumerable)** returns a loop wrapper. You can use it in a for loop to create a wrapper that makes it easier to generate conditional code at the start and end of the enumeration.
````
--; for(var loop = Loop(Columns) ; loop.IsValid ; loop.Next()) { ; var col = loop.Element;
   {loop.FirstOr(' ', ',')}@{col}
--; }
````

`Loop.IsValid` returns false once the loop has run out of elements.

`Loop.IsFirst` returns true for the first element in the iteration.

`Loop.IsLast` returns true for the last element in the iteration.

`Loop.Element` returns the current element.

`Loop.Next()` moves to the next element in the iteration and returns false if there are no more elements.

`Loop.Elements` returns all of the elements as an array.

`Loop.FirstOr(object returnIfFirst, object returnOtherwise)` returns one value or the other depending on whether this is the first iteration.

`Loop.LastOr(object returnIfLast, object returnOtherwise)` returns one value or the other depending on whether this is the last iteration.

### TextAndTab
`TextAndTab(string text, int minWidth = -1, int addSpace = 1)` returns the text with trailing space padding to bring the string to a minimum width.


## Template Model
Any C# expression or condition in the template can access the template model:

Variable Name | Description
------------- | -----------
Database | The current database
Schema | The current schema
Entity | The current table, view or UDTT.
Columns | A list of columns from the entity in ordinal order.
ColumnsByName | A dictionary of columns from the entity indexed by name.
ColumnsByOrdinal | A dictionary of columns from the entity indexed by ordinal number (0 based).
IsCaseSensitive | True if column names are case sensitive.
Table | The table entity or null if the entity is not a table.
View | The view entity or null if the entity is not a view.
UDTT | The UDTT entity or null if the entity is not a UDTT.

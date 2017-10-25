# SqlScriptGenerator
Generates SQL scripts from database metadata.

## Template Switches
Lines starting with --# denote a template switch.

Switch | Args | Use
------ | ---- | ---
FILESPEC | filespec | Indicates the path from the output root folder to the script file to write. The path can contain substitution items.

## Template Commands
Lines starting with --; denote a template command.

Command | Args | Use
------- | ---- | ---
IF | C# condition | Outputs the lines up to the following ENDIF if the condition is true.
ENDIF | | Terminates an IF.
LOOP | name = C# enumerable | Repeats the lines up to the following ENDLOOP, assigning each result from the enumerable to the name specified.
ENDLOOP | | Terminates a LOOP.
SET | name = C# expression | Assigns a value to a variable.

## Inline Substitutions
Substitutions are enclosed in &#123;braces&#125;. Any variable declared via --; SET can be inserted, as can any value in the template model. The content of the braces can be any C# expression.

## Inline Commands
Output in chevrons indicates inline commands.

Format | Output
------ | ------
<first&#124;other&#124;last> | Only usable within a loop. "First" is output for the first iteration of the loop, "Last" for the last and "Other" for everything else.

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

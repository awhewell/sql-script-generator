# SqlScriptGenerator
Generates SQL scripts from database metadata.

## Sample Script
````sql
--# FILESPEC {Schema}\procs\{Entity}_Save.sql
/*;
var createdCol =        ColumnsByName["Created"];
var updatedCol =        ColumnsByName["Updated"];
var deactivatedCol =    ColumnsByName["Deactivated"];
var identityCol =       Columns.FirstOrDefault(r => r.IsIdentity);
var paramColumns =      Columns.Where(r => r != createdCol && r != updatedCol && r != deactivatedCol).ToArray();
var maxColNameLength =  paramColumns.Max(r => r.Name.Length);
;*/
-- Inserts or updates {Entity} records.
-- USE [{Database}];

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_SCHEMA = '{Schema}' AND ROUTINE_NAME = '{Entity}_Save')
BEGIN
    EXEC sp_executesql N'CREATE PROCEDURE [{Schema}].[{Entity}_Save] AS BEGIN SET NOCOUNT ON; END;';
END;
GO

ALTER PROCEDURE [{Schema}].[{Entity}_Save]
--; for(var loop = Loop(paramColumns.OrderBy(r => r.Ordinal)) ; loop.IsValid ; loop.Next()) { var col = loop.Element;
   {loop.FirstOr(' ', ',')}@{PadText(col.Name, maxColNameLength)}{col.SqlType.ToUpper()}
--; }
--; if(updatedCol != null) {
   ,@{PadText(updatedCol.Name, maxColNameLength)}{updatedCol.SqlType.ToUpper()}
--; }
AS
BEGIN
    --
    -- WARNING: This script was generated by SqlScriptGenerator.
    --          Manual changes could be lost unless you modify the generator project first.
    --

    SET NOCOUNT ON;

    DECLARE @now AS DATETIME2 = GETUTCDATE();
    DECLARE @rowCount AS INTEGER = 0;

    IF @{identityCol} <> 0
    BEGIN
        UPDATE [{Schema}].[{Entity}]
        --; for(var loop = Loop(paramColumns.Where(r => r != identityCol).OrderBy(r => r.Ordinal)) ; loop.IsValid ; loop.Next()) { var col = loop.Element;
        {loop.FirstOr("SET    ", "      ,")}{PadText("[" + col.Name + "] =", maxColNameLength + 4)}@{col}
        --; }
        --; if(updatedCol != null) {
              ,{PadText("[" + updatedCol.Name + "] =", maxColNameLength + 4)}@now
        --; }
        WHERE  [{identityCol}] = @{identityCol}{updatedCol == null ? ";" : ""}
        --; if(updatedCol != null) {
        AND    [{updatedCol}] = @{updatedCol};
        --; }
        SET @rowCount = @@ROWCOUNT;
    END
    ELSE
    BEGIN
        INSERT INTO [{Schema}].[{Entity}] (
        --; for(var loop = Loop(paramColumns.OrderBy(r => r.Ordinal).Where(r => r != identityCol)); loop.IsValid ; loop.Next()) { var col = loop.Element;
              {loop.FirstOr(' ', ',')}[{col}]
        --; }
        --; if(createdCol != null) {
              ,[{createdCol}]
        --; }
        --; if(updatedCol != null) {
              ,[{updatedCol}]
        --; }
        --; if(deactivatedCol != null) {
              ,[{deactivatedCol}]
        --; }
        ) VALUES (
        --; for(var loop = Loop(paramColumns.OrderBy(r => r.Ordinal).Where(r => r != identityCol)) ; loop.IsValid ; loop.Next()) { var col = loop.Element;
              {loop.FirstOr(' ', ',')}@{col}
        --; }
        --; if(createdCol != null) {
              ,@now
        --; }
        --; if(updatedCol != null) {
              ,@now
        --; }
        --; if(deactivatedCol != null) {
              ,NULL
        --; }
        );
        SET @{identityCol} = SCOPE_IDENTITY();
        SET @rowCount = 1;
    END;

    --; var outMaxNameLength = Math.Max(identityCol.Name.Length, "@rowCount".Length);
    SELECT {PadText("@" + identityCol.Name, outMaxNameLength)}AS [ID]
          ,{PadText("@rowCount", outMaxNameLength)}AS [RowsSaved]
          ,{PadText("@now", outMaxNameLength)}AS [Now];
END;
GO
````

## Sample output
When applied to a table called dbo.User:

````sql
-- Inserts or updates User records.
-- USE [SampleDB];

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_SCHEMA = 'dbo' AND ROUTINE_NAME = 'User_Save')
BEGIN
    EXEC sp_executesql N'CREATE PROCEDURE [dbo].[User_Save] AS BEGIN SET NOCOUNT ON; END;';
END;
GO

ALTER PROCEDURE [dbo].[User_Save]
    @UserID       BIGINT
   ,@UserName     VARCHAR(20)
   ,@Name         NVARCHAR(160)
   ,@HashScheme   TINYINT
   ,@Salt         BINARY(1024)
   ,@PasswordHash BINARY(2048)
   ,@Updated      DATETIME2
AS
BEGIN
    --
    -- WARNING: This script was generated by SqlScriptGenerator.
    --          Manual changes could be lost unless you modify the generator project first.
    --

    SET NOCOUNT ON;

    DECLARE @now AS DATETIME2 = GETUTCDATE();
    DECLARE @rowCount AS INTEGER = 0;

    IF @UserID <> 0
    BEGIN
        UPDATE [dbo].[User]
        SET    [UserName] =     @UserName
              ,[Name] =         @Name
              ,[HashScheme] =   @HashScheme
              ,[Salt] =         @Salt
              ,[PasswordHash] = @PasswordHash
              ,[Updated] =      @now
        WHERE  [UserID] = @UserID
        AND    [Updated] = @Updated;
        SET @rowCount = @@ROWCOUNT;
    END
    ELSE
    BEGIN
        INSERT INTO [dbo].[User] (
               [UserName]
              ,[Name]
              ,[HashScheme]
              ,[Salt]
              ,[PasswordHash]
              ,[Created]
              ,[Updated]
              ,[Deactivated]
        ) VALUES (
               @UserName
              ,@Name
              ,@HashScheme
              ,@Salt
              ,@PasswordHash
              ,@now
              ,@now
              ,NULL
        );
        SET @UserID = SCOPE_IDENTITY();
        SET @rowCount = 1;
    END;

    SELECT @UserID   AS [ID]
          ,@rowCount AS [RowsSaved]
          ,@now      AS [Now];
END;
GO
````

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

### PadText
`PadText(string text, int minWidth = -1, int addSpace = 1)` returns the text with trailing space padding to bring the string to a minimum width.


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

The `ToString()` of all models returns the `Name` of the model.

### Database Properties

Property | Description
-------- | -----------
Name | Database name
Parent | Always null
Children | Dictionary of schemas
IsCaseSensitive | True if default collation is case sensitive collation
Schemas | Dictionary of schemas (not including system schemas) indexed by name

### Schema Properties

Property | Description
-------- | -----------
Name | Schema name
Parent | The owning database
Children | Dictionary of Tables, Views and UDTTs indexed by name
IsCaseSensitive | Is case sensitive collation the default
Tables | Dictionary of tables indexed by name
Views | Dictionary of views indexed by name
UserDefinedTableTypes | Dictionary of UDTTs indexed by name
StoredProcedures | Dictionary of stored procedures indexed by name

### Table / View / UDTT Properties

Property | Description
-------- | -----------
Name | Table / view / UDTT name
Parent | The owning schema
Children | Dictionary of columns indexed by name
IsCaseSensitive | Is case sensitive collation the default
Columns | Dictionary of columns indexed by name
UsedByStoredProcedures | **(UDTT only)** A list of stored procedures that take the UDTT as a parameter

### Column Properties

Property | Description
-------- | -----------
Name | Column name
Parent | The owning table / view / UDTT
Children | An empty collection
Ordinal | Ordinal number (1 based)
SqlType | English description of the column type
HasDefaultValue | True if the column has a default value
IsCaseSensitive | Is using case sensitive collation
IsComputed | True if the column is computed
IsIdentity | True if the column is the table's identity column
IsNullable | True if nullable
IsPrimaryKeyMember | True if column is a part of the primary key

### Stored Procedure Properties

Property | Description
-------- | -----------
Schema | Reference to the owning schema
Name | Procedure name

## Project Model

Property | Type | Description
-------- | ---- | -----------
ConnectionString | string | Connection string to use if none supplied on command-line.
TemplateFolder | string | Path to root folder containing templates. Can be relative to the project file.
ScriptFolder | string | Path to root folder where scripts are saved. Can be relative to the project file.
EntityTemplates | array | List of templates to apply to each entity.

### Entity Template Model

Property | Type | Description
-------- | ---- | -----------
Entity | string | Name of entity.
Templates | array[string] | Array of templates that can be applied to this entity.


## Command Line Switches

Running the program without any switches displays the basic usage:

````
usage: SqlScriptGenerator <command> [options]
  -createProj    Create or update a project file
  -generate      Generate a script from a template

PROJECT OPTIONS
  -project       Full path to the project file []

TEMPLATE OPTIONS
  -template      Template file []
  -script        Script file []
  -entity        Name of entity to use []

DATABASE ENGINE OPTIONS
  -engine        Database engine [SqlServer]
                 (SqlServer)
  -connection    The connection string []
  -askPassword   Ask for the connection password at runtime

MISC OPTIONS
  -writeSource   Write generated C# template code to filename provided []
````

### -createProj Command
`-project` Specifies the path to the project file to create / update. Update just ensures that the schema is up-to-date.

### -generate Command
`-project` Path to project file to read defaults from. Anything not specified on the command line is taken from the project.

`-entity` Database entity whose metadata is to be used. If not specified then all entities in the project are used in turn.

`-template` Template to apply to the entity. If not specified then all templates for the entity (as specified in the project) are generated.

`-script` Only applicable if a single script is being generated. Specifies the path to the script file. If multiple scripts are generated then the `--# FILESPEC` template switch is used to form the script filename.

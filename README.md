# DFEngine.Compilers.TSQL
[![Build Status](https://dev.azure.com/Hansalytics/DFEngine.Compilers.TSQL/_apis/build/status/Production?branchName=master)](https://dev.azure.com/Hansalytics/DFEngine.Compilers.TSQL/_build/latest?definitionId=22&branchName=master)

A compiler that is capable of detecting relationships between objects inside T-SQL by performing static code analysis.
Originally written for the  free-to-use sql-visualization-tool [dfengine.io](https://dfengine.io)

## Get started
```csharp
//1. Initialize
string your_sql = "INSERT INTO dbo.Costumers (Name, Age, Rating) VALUES (appDB.dbo.Costumers.Name, appDB.[dbo].Costumers.Age, 4)";     //The sql you want to have analyzed
string std_server = "companyserver";   //The name of the server on which the sql is executed on
string std_database = "datawarehouse"; //The name of the initial database on which the sql is executed on
string causer = "testcase";            //The entity that is executes the sql e.g. the name of a script
var options = new CompilerOptions();   //Options for compilation

//2. Compile
var result = new Compiler().compile(your_sql, sql_server, std_database, causer, options);
```

The result contains 
-> DataQueries (SELECT (only top-level))
-> DataManipulations (INSERT, UPDATE, DELETE, MERGE)

In the example above, the compiler result will contain a single data manipulation with 3 expressions; each for a column, that is beeing
targeted within the INSERT-statement. Each of these expressions again have child expressions that represent
the relations and form an expression tree.
Note that the purpose of this library is showing the relations between database objects and columns. Constant values
are not taken into the expression tree. The thrid expressionin the example will not have a child expression, since '4' is a constant value.

## Attributions
This compiler uses the [TSQL-Parser](https://github.com/bruce-dunwiddie/tsql-parser) library by [Bruce Dunwiddie](https://github.com/bruce-dunwiddie) for the tokenization of the raw tsql.
Saved me alot of work. Thanks alot!

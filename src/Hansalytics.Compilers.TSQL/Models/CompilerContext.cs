using Hansalytics.Compilers.TSQL.Helpers;
using Hansalytics.Compilers.TSQL.Models.DataEntities;
using System;
using System.Collections.Generic;
using System.Text;
using Hansalytics.Compilers.TSQL.Models;

namespace Hansalytics.Compilers.TSQL.Models
{
    /// <summary>
    /// A context that is always related to exactly one compiler instance
    /// and contains all kinds of information about the current compiling action 
    /// </summary>
    class CompilerContext
    {
        /// <summary>
        /// The causing entity thats responsible for the execution of the tsql, e.g. the name of script
        /// that contains the tsql content
        /// </summary>
        internal string Causer { get; }

        /// <summary>
        /// The server on which the sql is executed on
        /// </summary>
        internal string CurrentServerContext { get;}

        /// <summary>
        /// The current database context
        /// </summary>
        internal string CurrentDbContext { get; set; }

        /// <summary>
        /// DO NOT PUSH DIRECTLY TO THIS STACK! Use the AddDatabaseObjectToCurrentContext method instead
        /// Contains all database objects that are in the current scope of the statement that is beeing resolved
        /// After a select statement has been resolved, its objects are poped from the stack
        /// </summary>
        internal Stack<DatabaseObject> CurrentDatabaseObjectContext { get; } = new Stack<DatabaseObject>();

        /// <summary>
        /// Contains all Data Manipulations(INSERT, UPDATE, DELETE, etc.) found in the SQL
        /// </summary>
        internal List<DataManipulation> DataManipulations { get; set; } = new List<DataManipulation>();

        /// <summary>
        /// Contains all top level SELECT statements
        /// </summary>
        internal List<Expression> DataQueries { get; set; } = new List<Expression>();

        /// <summary>
        /// Contains all variables
        /// </summary>
        internal List<Variable> Variables { get; }

        /// <summary>
        /// Contains all table variables
        /// </summary>
        internal List<DatabaseObject> TableVariables { get; }

        internal CompilerContext(string causer, string serverName, string databaseName)
        {
            Causer = causer;
            CurrentServerContext = serverName;
            CurrentDbContext = databaseName;
            Variables = new List<Variable>();
        }

        /// <summary>
        /// A SELECT statement containing an INTO clause is not a data query but a data manipulation
        /// This method pushes such a SELECT statement to the related list 
        /// </summary>
        internal void AddSelectWithIntoClause(SelectStatement statement)
        {
            var manipulation = new DataManipulation();

            foreach (var expr in statement.Expression.ChildExpressions)
            {
                if (expr.Type.Equals(ExpressionType.COLUMN))
                {
                    var singleManipulation = new Expression(ExpressionType.COLUMN)
                    {
                        Name = StatementResolveHelper.EnhanceNotation(statement.TargetObject, InternalConstants.UNRELATED_COLUMN_NAME)
                    };

                    singleManipulation.ChildExpressions.Add(expr);
                    manipulation.Expressions.Add(singleManipulation);
                }
            }

            DataManipulations.Add(manipulation);
        }

        /// <summary>
        /// Adds a database object to the current context stack and adds missing parts
        /// </summary>
        internal void AddDatabaseObjectToCurrentContext(DatabaseObject dbo)
        {
            if (dbo == null)
                throw new ArgumentNullException("Invalid database object");

            CurrentDatabaseObjectContext.Push(dbo);
        }

        /// <summary>
        /// Drops all common table expressions. This method should be called after each
        /// SELECT, INSERT, UPDATE, DELETE or MERGE statement
        /// </summary>
        /// <see href="https://docs.microsoft.com/en-us/sql/t-sql/queries/with-common-table-expression-transact-sql?view=sql-server-ver15"/>
        internal void DropCommonTableExpressions()
        {
            while (CurrentDatabaseObjectContext.Count > 0 && CurrentDatabaseObjectContext.Peek().Type.Equals(DatabaseObjectType.CTE))
                CurrentDatabaseObjectContext.Pop();
        }
    }
}

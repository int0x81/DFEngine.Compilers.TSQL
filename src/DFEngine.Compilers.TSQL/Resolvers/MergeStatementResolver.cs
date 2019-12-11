using System;
using DFEngine.Compilers.TSQL.Models;
using DFEngine.Compilers.TSQL.Exceptions;
using DFEngine.Compilers.TSQL.Helpers;
using TSQL.Tokens;
using DFEngine.Compilers.TSQL.Models.DataEntities;
using System.Collections.Generic;

namespace DFEngine.Compilers.TSQL.Resolvers
{
    /// <summary>
    /// Resolves a merge statement
    /// </summary>
    /// <see href="https://docs.microsoft.com/en-us/sql/t-sql/statements/merge-transact-sql?view=sql-server-ver15"/>
    class MergeStatementResolver : IDataManipulationResolver
    {
        DataManipulation manipulation;

        DatabaseObject targetObject;

        public DataManipulation Resolve(ReadOnlySpan<TSQLToken> tokens, ref int fileIndex, CompilerContext context)
        {
            manipulation = new DataManipulation();

            fileIndex++; //skip "merge"

            //skip top expression
            SkipTopExpression(tokens, ref fileIndex, context);

            if (tokens[fileIndex].Text.ToLower().Equals("into"))
                fileIndex++;

            targetObject = StatementResolveHelper.ResolveDatabaseObject(tokens, ref fileIndex, context);

            if (!tokens[fileIndex].Text.ToLower().Equals("using"))
                throw new InvalidSqlException("Trying to resolve a merge-statement without using keyword");
            
            var source = ResolveUsingStatement(tokens, ref fileIndex, context);

            context.AddDatabaseObjectToCurrentContext(source);

            if (!tokens[fileIndex].Text.Equals("on", StringComparison.InvariantCultureIgnoreCase))
                throw new InvalidSqlException("Expected 'ON' keyword when resolving a 'MERGE'-statement");

            fileIndex++; //skip 'on'

            SearchConditionResolver.Resolve(tokens, ref fileIndex, context);

            ResolveWhenExpression(tokens, ref fileIndex, context);

            var beautified = new List<Expression>();

            foreach (var exp in manipulation.Expressions)
                beautified.Add(StatementResolveHelper.BeautifyColumns(exp, context));

            manipulation.Expressions = beautified;

            while (!tokens[fileIndex].Text.ToLower().Equals(";"))
            {
                fileIndex++;
                if (fileIndex == tokens.Length)
                    throw new InvalidSqlException("Trying to resolve a merge-statement without proper ';' determination");
            }

            fileIndex++; //skip ';'

            context.CurrentDatabaseObjectContext.Pop();

            return manipulation;
        }

        private DatabaseObject ResolveUsingStatement(ReadOnlySpan<TSQLToken> tokens, ref int fileIndex, CompilerContext context)
        {
            fileIndex++; //skip 'using'
            return StatementResolveHelper.ResolveDatabaseObject(tokens, ref fileIndex, context);
        }

        private void SkipTopExpression(ReadOnlySpan<TSQLToken> tokens, ref int fileIndex, CompilerContext context)
        {
            if (!tokens[fileIndex].Text.ToLower().Equals("top"))
                return;

            fileIndex++; //skip top

            StatementResolveHelper.ResolveExpression(tokens, ref fileIndex, context);

            if (tokens[fileIndex].Text.ToLower().Equals("percent"))
                fileIndex++;
        }

        /// <summary>
        /// Checks if a when expression is set within the MERGE-statement and resolves it if so
        /// </summary>
        private void ResolveWhenExpression(ReadOnlySpan<TSQLToken> tokens, ref int fileIndex, CompilerContext context)
        {
            if (!tokens[fileIndex].Text.Equals("when", StringComparison.InvariantCultureIgnoreCase))
                return;
            else
                fileIndex++; //skip 'when'

            if(tokens[fileIndex].Text.Equals("matched", StringComparison.InvariantCultureIgnoreCase))
            {
                fileIndex++; //skip 'matched'
                if(tokens[fileIndex].Text.Equals("and", StringComparison.InvariantCultureIgnoreCase))
                {
                    fileIndex++; //skip 'and'
                    SearchConditionResolver.Resolve(tokens, ref fileIndex, context);
                }

                if (!tokens[fileIndex].Text.Equals("then", StringComparison.InvariantCultureIgnoreCase))
                    throw new InvalidSqlException("WHEN-expression does not contain the THEN-Keyword");
                else
                {
                    fileIndex++; //skip 'then'
                    ResolveMergeMatched(tokens, ref fileIndex, context);
                }
            }
            else if(tokens[fileIndex].Text.Equals("not", StringComparison.InvariantCultureIgnoreCase))
            {
                fileIndex += 2; //skip 'not matched'

                if(tokens[fileIndex].Text.Equals("by", StringComparison.InvariantCultureIgnoreCase))
                {
                    fileIndex++; //skip 'by'
                    if (tokens[fileIndex].Text.Equals("source", StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (tokens[fileIndex].Text.Equals("and", StringComparison.InvariantCultureIgnoreCase))
                        {
                            fileIndex++; //skip 'and'
                            SearchConditionResolver.Resolve(tokens, ref fileIndex, context);
                        }

                        if (!tokens[fileIndex].Text.Equals("then", StringComparison.InvariantCultureIgnoreCase))
                            throw new InvalidSqlException("WHEN-expression does not contain the THEN-Keyword");
                        else
                        {
                            fileIndex++; //skip 'then'
                            ResolveMergeMatched(tokens, ref fileIndex, context);
                        }

                        return;
                    }
                    if (tokens[fileIndex].Text.Equals("target", StringComparison.InvariantCultureIgnoreCase))
                        fileIndex++;
                }

                if (tokens[fileIndex].Text.Equals("and", StringComparison.InvariantCultureIgnoreCase))
                {
                    fileIndex++; //skip 'and'
                    SearchConditionResolver.Resolve(tokens, ref fileIndex, context);
                }

                if (!tokens[fileIndex].Text.Equals("then", StringComparison.InvariantCultureIgnoreCase))
                    throw new InvalidSqlException("WHEN-expression does not contain the THEN-Keyword");
                else
                {
                    fileIndex++; //skip 'then'
                    ResolveMergeNotMatched(tokens, ref fileIndex, context);
                }
            }
            else
                throw new InvalidSqlException("Invalid WHEN-expression within MERGE-statement");
        }

        /// <summary>
        /// Gets called when the rows matched
        /// </summary>
        private void ResolveMergeMatched(ReadOnlySpan<TSQLToken> tokens, ref int fileIndex, CompilerContext context)
        {
            if (tokens[fileIndex].Text.Equals("update", StringComparison.InvariantCultureIgnoreCase))
            {
                fileIndex += 2; //skip 'update set'
                ResolveSetClause(tokens, ref fileIndex, context);
            }
            else if (tokens[fileIndex].Text.Equals("delete", StringComparison.InvariantCultureIgnoreCase))
                fileIndex++; //skip 'delete'
            else
                throw new InvalidSqlException("Invalid WHEN-expression within MERGE-statement");
        }

        /// <summary>
        /// Gets called when the rows didnt match
        /// </summary>
        private void ResolveMergeNotMatched(ReadOnlySpan<TSQLToken> tokens, ref int fileIndex, CompilerContext context)
        {
            if (!tokens[fileIndex].Text.Equals("insert", StringComparison.InvariantCultureIgnoreCase))
                throw new InvalidSqlException("A 'WHEN NOT MATCHED'-clause did not contain an INSERT statement");

            List<Expression> targets = new List<Expression>();

            List<Expression> sources = new List<Expression>();

            fileIndex++; //skip 'insert'

            if (tokens[fileIndex].Text.Equals("("))
            {
                fileIndex++; //skip '('

                do
                {
                    var target = StatementResolveHelper.ResolveExpression(tokens, ref fileIndex, context);
                    AddTargetObject(target);
                    targets.Add(target);

                    if (tokens[fileIndex].Text.Equals(","))
                    {
                        fileIndex++; //skip ','
                        continue;
                    }
                    else
                        break;

                } while (true);

                fileIndex++; //skip ')'
            }

            if (tokens[fileIndex].Text.Equals("values", StringComparison.InvariantCultureIgnoreCase))
            {
                fileIndex += 2; //skip 'values ('

                do
                {
                    var source = StatementResolveHelper.ResolveExpression(tokens, ref fileIndex, context);
                    sources.Add(source);

                    if (fileIndex < tokens.Length && tokens[fileIndex].Text.Equals(","))
                    {
                        fileIndex++; //skip ','
                        continue;
                    }
                    else
                        break;

                } while (true);

                fileIndex++; //skip ')'
            }
            else if (tokens[fileIndex].Text.Equals("default", StringComparison.InvariantCultureIgnoreCase))
                fileIndex += 2; //skip 'default values'
            else
                throw new InvalidSqlException("Unable to compile ");

        }

        /// <summary>
        /// Resolves a Set clause
        /// </summary>
        private void ResolveSetClause(ReadOnlySpan<TSQLToken> tokens, ref int fileIndex, CompilerContext context)
        {
            do
            {
                var target = StatementResolveHelper.ResolveExpression(tokens, ref fileIndex, context); //resolve target column

                AddTargetObject(target);

                fileIndex++; //skip '='

                var source = StatementResolveHelper.ResolveExpression(tokens, ref fileIndex, context); //resolve source column
                target.ChildExpressions.Add(source);

                manipulation.Expressions.Add(target);

                if (fileIndex < tokens.Length && tokens[fileIndex].Text.Equals(","))
                {
                    fileIndex++; //skip ','
                    continue;
                }
                else
                    break;

            } while (true);
        }

        /// <summary>
        /// Adds the target object to a target expression if it is not explicitly
        /// mentioned
        /// </summary>
        private void AddTargetObject(Expression target)
        {
            if (target.Type != ExpressionType.COLUMN)
                throw new ArgumentException("Expression has to be a column");

            Helper.SplitColumnNotationIntoSingleParts(target.Name, out string databaseName, out string databaseSchema, out string databaseObjectName, out string columnName, true);

            if (databaseObjectName == null)
            {
                databaseObjectName = targetObject.Name;
                target.Name = databaseObjectName + "." + columnName;
            }

            if (databaseSchema == null && targetObject.Schema != null)
            {
                databaseSchema = targetObject.Schema;
                target.Name = databaseSchema + "." + target.Name;
            }
            else
                return;

            if (databaseName == null && targetObject.Database != null)
            {
                databaseName = targetObject.Database;
                target.Name = databaseName + "." + target.Name;
            }
        }
    }
}
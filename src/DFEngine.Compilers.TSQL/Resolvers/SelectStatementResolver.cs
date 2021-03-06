﻿using DFEngine.Compilers.TSQL.Models;
using DFEngine.Compilers.TSQL.Exceptions;
using DFEngine.Compilers.TSQL.Helpers;
using System;
using System.Collections.Generic;
using TSQL.Tokens;
using DFEngine.Compilers.TSQL.Constants;

namespace DFEngine.Compilers.TSQL.Resolvers
{
    class SelectStatementResolver : IExpressionResolver
    {
        SelectStatement statement;

        //States who many database objects are added to the current scope 
        //by this statement (subselects are excluded). At the end of the statement
        //as many objects are poped from the context stack
        int objectsAddedToContext = 0;

        internal SelectStatementResolver()
        {
        }

        public Expression Resolve(ReadOnlySpan<TSQLToken> tokens, ref int fileIndex, CompilerContext context)
        {
            ResolveStatement(tokens, ref fileIndex, context);
            return statement.Expression;
        }

        public SelectStatement ResolveTopLevel(ReadOnlySpan<TSQLToken> tokens, ref int fileIndex, CompilerContext context)
        {
            ResolveStatement(tokens, ref fileIndex, context);
            return statement;
        }

        private SelectStatement ResolveStatement(ReadOnlySpan<TSQLToken> tokens, ref int fileIndex, CompilerContext context)
        {
            statement = new SelectStatement();

            SkipSelectPrequelStatements(tokens, ref fileIndex, context);

            statement.Expression = DetermineSourceColumns(tokens, ref fileIndex, context);

            ResolveIntoClause(tokens, ref fileIndex, context);

            var objects = StatementResolveHelper.ResolveFromStatement(tokens, ref fileIndex, context);

            AddObjectsToContext(objects, context);

            Beautifier.BeautifyColumns(statement.Expression, context);

            AddSynonymousObjects(objects);

            StatementResolveHelper.ResolveWhereStatement(tokens, ref fileIndex, context);

            ResolveGroupByClause(tokens, ref fileIndex, context);

            ResolveOrderByClause(tokens, ref fileIndex, context);

            //Resolve FOR-CLAUSE

            ResolveUnionclause(tokens, ref fileIndex, context);

            PopObjectsFromContextStack(context);

            statement.Expression.Name = "SELECT";

            return statement;
        }

        /// <summary>
        /// Pops as many database objects from the stack as this statement pushed
        /// </summary>
        private void PopObjectsFromContextStack(CompilerContext context)
        {
            while(objectsAddedToContext > 0)
            {
                context.CurrentDatabaseObjectContext.Pop();
                objectsAddedToContext--;
            }
        }

        private void AddObjectsToContext(List<DatabaseObject> objects, CompilerContext context)
        {
            foreach(var obj in objects)
                context.AddDatabaseObjectToCurrentContext(obj);
                
            objectsAddedToContext = objects.Count;
        }

        private void ResolveIntoClause(ReadOnlySpan<TSQLToken> tokens, ref int fileIndex, CompilerContext context)
        {
            if (fileIndex < tokens.Length && tokens[fileIndex].Text.ToLower().Equals("into"))
            {
                fileIndex++;
                var target = StatementResolveHelper.ResolveDatabaseObject(tokens, ref fileIndex, context);

                statement.TargetObject = target;
                
            }
        }

        /// <summary>
        /// Moves the current fileIndex behind the select keyword and skips specific restraining keywords like "top" or "distinct"
        /// </summary>
        private void SkipSelectPrequelStatements(ReadOnlySpan<TSQLToken> tokens, ref int fileIndex, CompilerContext context)
        {
            fileIndex++; //skip "select"

            if (tokens[fileIndex].Text.ToLower().Equals("distinct"))
                fileIndex++;

            if (tokens[fileIndex].Text.ToLower().Equals("top"))
            {
                fileIndex++; //skip 'top'
                if (tokens[fileIndex].Text.Equals("("))
                    fileIndex++;
                fileIndex++;
                if (tokens[fileIndex].Text.Equals(")"))
                    fileIndex++;
            }

            if (tokens[fileIndex].Text.ToLower().Equals("percent"))
                fileIndex++;
        }

        /// <summary>
        /// Returns the query containing all data columns. The column properties (schema, table) may not have been assigned
        /// correctly after this function since the from statement still needs to resolved
        /// </summary>
        private Expression DetermineSourceColumns(ReadOnlySpan<TSQLToken> tokens, ref int fileIndex, CompilerContext context)
        {
            Expression query = new Expression(ExpressionType.SCALAR_FUNCTION)
            {
                Name = "SELECT"
            };

            if (tokens[fileIndex].Text.Equals("*"))
            {
                var column = new Expression(ExpressionType.COLUMN)
                {
                    Name = tokens[fileIndex].Text
                };

                query.ChildExpressions.Add(column);
                    fileIndex++;

                return query;
            }

            do
            {
                var innerExpression = StatementResolveHelper.ResolveExpression(tokens, ref fileIndex, context);

                if (fileIndex < tokens.Length && tokens[fileIndex].Text.ToLower().Equals("="))
                {
                    //An expression followed by a "=" is equivalent to: CASE WHEN col1 = col2 THEN 1 ELSE 0 END
                    fileIndex++; //skip '='
                    StatementResolveHelper.ResolveExpression(tokens, ref fileIndex, context);
                }
                
                if (fileIndex >= tokens.Length)
                    return query;

                if (tokens[fileIndex].Text.ToLower().Equals("as"))
                {
                    fileIndex++; //skip as
                    string possibleAlias = tokens[fileIndex].Text;
                    fileIndex++; //skip alias
                    var aliasExpression = new Expression(ExpressionType.ALIAS) { Name = possibleAlias };
                    aliasExpression.ChildExpressions.Add(innerExpression);
                    query.ChildExpressions.Add(aliasExpression);
                }
                else if (tokens[fileIndex].Type.Equals(TSQLTokenType.Identifier))
                {
                    string possibleAlias = tokens[fileIndex].Text;
                    fileIndex++; //skip alias
                    var aliasExpression = new Expression(ExpressionType.ALIAS) { Name = possibleAlias };
                    aliasExpression.ChildExpressions.Add(innerExpression);
                    query.ChildExpressions.Add(aliasExpression);
                }
                else
                    query.ChildExpressions.Add(innerExpression);

                if (tokens[fileIndex].Text.Equals(","))
                {
                    fileIndex++; //skip ','
                    continue;
                }
                else { break; }

            } while (true);

            return query;
        }

        private void ResolveGroupByClause(ReadOnlySpan<TSQLToken> tokens, ref int fileIndex, CompilerContext context)
        {
            if (fileIndex >= tokens.Length || !tokens[fileIndex].Text.ToLower().Equals("group"))
            {
                return;
            }
            else
            {
                fileIndex += 2; //skip 'group by'
            }

            do
            {
                StatementResolveHelper.ResolveExpression(tokens, ref fileIndex, context);

                if (fileIndex >= tokens.Length)
                    break;

                if (tokens[fileIndex].Text.Equals(","))
                {
                    fileIndex++;
                    continue;
                }
                else
                {
                    break;
                }

            } while (true);
        }

        private void ResolveOrderByClause(ReadOnlySpan<TSQLToken> tokens, ref int fileIndex, CompilerContext context)
        {
            if (fileIndex >= tokens.Length || !tokens[fileIndex].Text.ToLower().Equals("order"))
            {
                return;
            }
            else
            {
                fileIndex += 2; //skip 'order by'
            }

            do
            {
                StatementResolveHelper.ResolveExpression(tokens, ref fileIndex, context);

                if (fileIndex >= tokens.Length)
                    break;

                if (tokens[fileIndex].Text.Equals(","))
                {
                    fileIndex++;
                    continue;
                }
                else
                {
                    break;
                }

            } while (true);

            if (fileIndex < tokens.Length && (tokens[fileIndex].Text.ToLower().Equals("asc") || tokens[fileIndex].Text.ToLower().Equals("desc")))
                fileIndex++;
        }

        private void ResolveUnionclause(ReadOnlySpan<TSQLToken> tokens, ref int fileIndex, CompilerContext context)
        {
            if (fileIndex < tokens.Length && tokens[fileIndex].Text.ToLower().Equals("union"))
            {
                fileIndex++; //skip "union"

                if (tokens[fileIndex].Text.ToLower().Equals("all"))
                    fileIndex++;

                int openBracketCounter = 0;

                while (tokens[fileIndex].Text.Equals("("))
                {
                    openBracketCounter++;
                    fileIndex++;
                }
                    
                if (!tokens[fileIndex].Text.ToLower().Equals("select"))
                    throw new InvalidSqlException("'union keyword was not followed by a select'");

                SelectStatementResolver innerResolver = new SelectStatementResolver();

                Expression nextUnion = innerResolver.Resolve(tokens, ref fileIndex, context);
                statement.Expression.ChildExpressions.Add(nextUnion);

                while(openBracketCounter > 0)
                {
                    openBracketCounter--;
                    fileIndex++;
                }
            }
        }

        /// <summary>
        /// Loops through all collected database objects and add a
        /// whole-object-synonymous for each object that is not referenced
        /// in a column
        /// </summary>
        private void AddSynonymousObjects(List<DatabaseObject> databaseObjects)
        {
            foreach (var dbo in databaseObjects)
            {
                if (!dbo.Type.Equals(DatabaseObjectType.REAL))
                    continue;

                bool isReferenced = false;

                foreach (var expr in statement.Expression.ChildExpressions)
                {
                    if (!expr.Type.Equals(ExpressionType.COLUMN))
                        continue;

                    Helper.SplitColumnNotationIntoSingleParts(expr.Name, out string databaseName, out string schemaName, out string dboName, out string columnName);

                    if (dbo.Name.Equals(dboName))
                    {
                        isReferenced = true;
                        break;
                    }
                }

                if(!isReferenced)
                {
                    var sourceSynonymous = new Expression(ExpressionType.COLUMN)
                    {
                        Name = Beautifier.EnhanceNotation(dbo, InternalConstants.WHOLE_OBJECT_SYNONYMOUS),
                        WholeObjectSynonymous = true
                    };
                    statement.Expression.ChildExpressions.Add(sourceSynonymous);
                }
            }
        }
    }
}
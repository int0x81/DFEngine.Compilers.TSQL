﻿using DFEngine.Compilers.TSQL.Models;
using DFEngine.Compilers.TSQL.Exceptions;
using DFEngine.Compilers.TSQL.Resolvers;
using System;
using TSQL.Tokens;
using System.Collections.Generic;
using DFEngine.Compilers.TSQL.Constants;

namespace DFEngine.Compilers.TSQL.Helpers
{
    static class StatementResolveHelper
    {
        /// <summary>
        /// Resolves a database object and adds it to the current context as well
        /// as to the landscape
        /// </summary>
        /// <param name="asTargetTable">If the database object is expected to be a target table standart database and server are
        /// not added. We also may expect an opening bracket directly after the identifier which is the then not count as a function start</param>
        /// <returns>The database object</returns>
        internal static DatabaseObject ResolveDatabaseObject(ReadOnlySpan<TSQLToken> tokens, ref int fileIndex, CompilerContext context, bool asTargetTable = false)
        {
            DatabaseObject dbo;

            if (tokens[fileIndex].Text.Equals("("))
            {
                fileIndex++; //skip '('
                dbo = ResolveDatabaseObject(tokens, ref fileIndex, context, asTargetTable);
                fileIndex++; //skip ')'
            }
            else if(tokens[fileIndex].Text.ToLower().Equals("select"))
            {
                var resolver = new SelectStatementResolver();
                var statement = resolver.Resolve(tokens, ref fileIndex, context);
                dbo = new DatabaseObject(DatabaseObjectType.SELECTION);
                dbo.Expressions.AddRange(statement.ChildExpressions);
            }
            else if(tokens[fileIndex].Type.Equals(TSQLTokenType.Identifier))
            {
                int positionBeforeNotationResolution = fileIndex;
                string notation = ResolveDatabaseObjectIdentifier(tokens, ref fileIndex);

                if (tokens.Length > fileIndex && tokens[fileIndex].Text.Equals("(") && !asTargetTable)
                {
                    //In this section we asssume that the database object is returned by a function
                    fileIndex = positionBeforeNotationResolution;
                    var functionResolver = new TsqlFunctionResolver();
                    var function = functionResolver.Resolve(tokens, ref fileIndex, context);
                    dbo = new DatabaseObject(DatabaseObjectType.SELECTION);
                    dbo.Expressions.Add(function);
                }
                else
                    dbo = new DatabaseObject(notation, context, DatabaseObjectType.REAL, !asTargetTable);     
            }
            else if(tokens[fileIndex].Type.Equals(TSQLTokenType.Variable))
            {
                dbo = new DatabaseObject(DatabaseObjectType.VARIABLE)
                {
                    Name = tokens[fileIndex].Text,
                    Database = InternalConstants.UNRELATED_DATABASE_NAME,
                    Schema = InternalConstants.UNRELATED_SCHEMA_NAME,
                    Server = InternalConstants.UNRELATED_SERVER_NAME
                };

                fileIndex++;
            }
            else if(tokens[fileIndex].Type.Equals(TSQLTokenType.StringLiteral) || tokens[fileIndex].Type.Equals(TSQLTokenType.NumericLiteral) || tokens[fileIndex].Type.Equals(TSQLTokenType.BinaryLiteral))
            {
                //The dbo is a set of constants e.g. ('Design Engineer', 'Tool Designer', 'Marketing Assistant')
                var dataset = ResolvesDataSet(tokens, ref fileIndex, context);
                dbo = new DatabaseObject(DatabaseObjectType.SET);
                dbo.Expressions.Add(dataset);
            }
            else if(tokens[fileIndex].Text.Equals("openquery", StringComparison.InvariantCultureIgnoreCase))
            {
                dbo = new OpenQueryStatementResolver().Resolve(tokens, ref fileIndex, context);
            }
            else
            {
                throw new InvalidSqlException("Invalid database object");
            }

            SkipTableHints(tokens, ref fileIndex);

            string alias = ResolveAlias(tokens, ref fileIndex, context);

            if (!string.IsNullOrEmpty(alias))
            {
                dbo.Alias = alias;
            }

            SkipTableHints(tokens, ref fileIndex); //Table hints can appear before OR after an alias

            return dbo;
        }

        /// <summary>
        /// Resolves an expression
        /// </summary>
        internal static Expression ResolveExpression(ReadOnlySpan<TSQLToken> tokens, ref int fileIndex, CompilerContext context)
        {
            List<Expression> expressions = new List<Expression>();

            do
            {
                if (tokens.Length > fileIndex && tokens[fileIndex].Text.Equals("("))
                {
                    fileIndex++; //skip '('
                    var innerExpression = ResolveExpression(tokens, ref fileIndex, context);
                    fileIndex++; //skip ')'
                    expressions.Add(innerExpression);
                }
                else
                {
                    var innerExpression = ResolveSimpleExpression(tokens, ref fileIndex, context);

                    if (innerExpression.Type.Equals(ExpressionType.SCALAR_FUNCTION) || innerExpression.Type.Equals(ExpressionType.COMPLEX))
                        expressions.AddRange(innerExpression.ChildExpressions);
                    else if (innerExpression.Type.Equals(ExpressionType.COLUMN))
                        expressions.Add(innerExpression);

                    SkipOverClause(tokens, ref fileIndex);
                }

                if (tokens.Length > fileIndex && Operators.IsArithmeticOperator(tokens[fileIndex].Text))
                {
                    fileIndex++; //skip operator
                    continue;
                }
                else
                    break;
            }
            while (true);

            expressions = StripeConstantValues(expressions);

            if(TryGetSingleRealExpression(expressions, out Expression possibleSingleReal))
                return possibleSingleReal;
            else
            {
                return new Expression(ExpressionType.COMPLEX)
                {
                    ChildExpressions = expressions
                };
            }
        }

        /// <summary>
        /// Resolves a simple expression that is not a concatenation of multiple expressions
        /// </summary>
        private static Expression ResolveSimpleExpression(ReadOnlySpan<TSQLToken> tokens, ref int fileIndex, CompilerContext context)
        {
            Expression expression;

            if (tokens[fileIndex].Text.Equals("("))
            {
                fileIndex++; //skip '('
                var innerExpression = ResolveExpression(tokens, ref fileIndex, context);
                
                if (string.IsNullOrEmpty(innerExpression.Name))
                    throw new InvalidOperationException("expression must have a name"); //DEBUG

                if (!tokens[fileIndex].Text.Equals(")"))
                    throw new InvalidSqlException("Missing ')'");

                fileIndex++; 

                expression = innerExpression;
            }
            else
            {
                if (fileIndex + 1 < tokens.Length && TsqlKeywordDictionary.EXPRESSION_RESOLVERS.TryGetValue(tokens[fileIndex].Text.ToLower(), out IExpressionResolver innerResolver))
                {
                    expression = innerResolver.Resolve(tokens, ref fileIndex, context);
                }
                else
                {
                    if (tokens[fileIndex].Type.Equals(TSQLTokenType.NumericLiteral) || tokens[fileIndex].Text.ToLower().Equals("null"))
                    {
                        expression = new Expression(ExpressionType.CONSTANT)
                        {
                            Name = tokens[fileIndex].Text
                        };
                        fileIndex++;
                    }
                    else if(tokens[fileIndex].Type.Equals(TSQLTokenType.StringLiteral))
                    {
                        //TSQL may have '' as escape characters (WTF?!) in a string like this: DELETE OPENQUERY (OracleSvr, 'SELECT name FROM joe.titles WHERE name = ''NewTitle''');
                        //https://docs.microsoft.com/en-us/sql/t-sql/functions/openquery-transact-sql?view=sql-server-ver15
                        //The TSQL-Parser library is also not aware of this and causes a bug which is why this section is pretty ugly :/
                        if(fileIndex + 2 < tokens.Length && tokens[fileIndex].Text.Equals("''") && (tokens[fileIndex + 2].Text.Equals("''") || (tokens[fileIndex + 1].Text.Length > 2 && tokens[fileIndex + 1].Text.Substring(tokens[fileIndex + 1].Text.Length - 2).Equals("''"))))
                        {
                            fileIndex++; //skip ''
                            expression = new Expression(ExpressionType.CONSTANT);

                            if (tokens[fileIndex + 1].Text.Length > 2 && tokens[fileIndex + 1].Text.Substring(tokens[fileIndex + 1].Text.Length - 2).Equals("''"))
                                expression.Name = tokens[fileIndex].Text[0..^2];
                            else
                                expression.Name = tokens[fileIndex].Text;

                            fileIndex++;

                            if (tokens[fileIndex].Text.Equals("''"))
                                fileIndex++;
                        }
                        else
                        {
                            expression = new Expression(ExpressionType.CONSTANT)
                            {
                                Name = tokens[fileIndex].Text
                            };
                            fileIndex++;
                        }
                    }
                    else if (tokens[fileIndex].Type.Equals(TSQLTokenType.Variable))
                    {
                        //Variable resolution not yet implemented, we are treat them like constants

                        expression = new Expression(ExpressionType.CONSTANT)
                        {
                            Name = tokens[fileIndex].Text
                        };

                        fileIndex++;
                    }
                    else if (tokens[fileIndex].Text.Equals("-") || tokens[fileIndex].Text.Equals("+"))
                    {
                        //this case may appear when an expression is a negated constant like (-2). The Tokenizer will split this into 2 tokens
                        //so we just return a 0 constant expression without skipping the operator token
                        expression = new Expression(ExpressionType.CONSTANT)
                        {
                            Name = "0"
                        };
                    }
                    else if (tokens[fileIndex].Text.Equals("*"))
                    {
                        //we are dealing with an all-columns-synonymous
                        expression = new Expression(ExpressionType.COLUMN)
                        {
                            Name = tokens[fileIndex].Text
                        };
                        fileIndex++;
                    }
                    else
                    {
                        //Now we re assuming that token is an identifier
                        string column = ResolveColumnIdentifier(tokens, ref fileIndex);

                        if (fileIndex < tokens.Length && tokens[fileIndex].Text.Equals("(")) //That means that the resolved column is actually a function
                        {
                            innerResolver = new TsqlFunctionResolver();
                            fileIndex--;
                            expression = innerResolver.Resolve(tokens, ref fileIndex, context);
                        }
                        else
                        {
                            expression = new Expression(ExpressionType.COLUMN)
                            {
                                Name = column
                            }; 
                        }
                    }
                }

                SkipCollateExpression(tokens, ref fileIndex);
            }

            return expression;
        }

        /// <summary>
        /// Loops through a list of expressions and returns a new list that only contains actual columns or scalar functions.
        /// Does NOT step into recursion
        /// </summary>
        private static List<Expression> StripeConstantValues(List<Expression> expressions)
        {
            var newList = new List<Expression>();

            foreach(var exp in expressions)
            {
                if (exp.Type.Equals(ExpressionType.COLUMN) || exp.Type.Equals(ExpressionType.SCALAR_FUNCTION))
                    newList.Add(exp);
            }

            return newList;
        }

        /// <summary>
        /// Resolves a from statement and returns all database objects found in it
        /// </summary>
        internal static List<DatabaseObject> ResolveFromStatement(ReadOnlySpan<TSQLToken> tokens, ref int fileIndex, CompilerContext context)
        {
            List<DatabaseObject> dbos = new List<DatabaseObject>();

            if (fileIndex >= tokens.Length || !tokens[fileIndex].Text.ToLower().Equals("from"))
                return dbos;

            fileIndex++; //skip 'from'

            while (true)
            {
                var dbo = ResolveDatabaseObject(tokens, ref fileIndex, context);
                dbos.Add(dbo);

                if (fileIndex >= tokens.Length)
                    break;

                if (tokens[fileIndex].Text.Equals(","))
                {
                    fileIndex++;
                    continue;
                }

                while (DetectTableJoin(tokens, ref fileIndex))
                {
                    var joinObject = ResolveTableJoin(tokens, ref fileIndex, context);
                    dbos.Add(joinObject);

                    if (fileIndex >= tokens.Length)
                        break;
                }

                SkipPivotClause(tokens, ref fileIndex);

                break;
            }

            return dbos;
        }

        /// <summary>
        /// Resolves a WHERE statement. The search conditions are not considered relevant for this library
        /// </summary>
        internal static void ResolveWhereStatement(ReadOnlySpan<TSQLToken> tokens, ref int fileIndex, CompilerContext context)
        {
            if (fileIndex >= tokens.Length || !tokens[fileIndex].Text.ToLower().Equals("where"))
                return;

            fileIndex++; //skip 'WHERE'

            SearchConditionResolver.Resolve(tokens, ref fileIndex, context);
        }

        /// <summary>
        /// Takes in an object notation and extracts the single entities according to the TSQL definition
        /// </summary>
        /// <param name="objectNotation">The notation of the object. Each part may contains square brackets</param>
        /// <see href="https://docs.microsoft.com/en-us/sql/t-sql/language-elements/transact-sql-syntax-conventions-transact-sql?view=sql-server-ver15"/>
        internal static void SplitObjectNotationIntoSingleParts(string objectNotation, out string serverName, out string databaseName, out string databaseSchema, out string databaseObjectName)
        {
            if (string.IsNullOrEmpty(objectNotation))
                throw new ArgumentNullException("Invalid object notation");

            serverName = null;
            databaseName = null;
            databaseSchema = null;

            int counter = objectNotation.Length - 1;

            while (counter > 0)
            {
                if(objectNotation[counter].Equals('.'))
                {
                    databaseObjectName = objectNotation.Substring(counter + 1);
                    int objectStart = counter;
                    counter--;
                    if(objectNotation[counter].Equals('.'))
                    { //Schema is omitted
                        counter--;
                        if(objectNotation[counter].Equals('.'))
                        { //Database is omitted
                            serverName = objectNotation.Substring(0, counter);
                            return;
                        }
                        else
                        {
                            int dbEnd = counter;

                            while (counter > 0)
                            {
                                if (objectNotation[counter].Equals('.'))
                                {
                                    //counter--;
                                    databaseName = objectNotation.Substring(counter + 1, dbEnd - counter);
                                    if (counter > 0)
                                        serverName = objectNotation.Substring(0, counter);

                                    return;
                                }

                                counter--;
                            }

                            databaseName = objectNotation.Substring(0, dbEnd + 1);
                        }
                    }
                    else
                    {
                        while(counter > 0)
                        {
                            if(objectNotation[counter].Equals('.'))
                            {
                                int schemaStart = counter + 1;
                                databaseSchema = objectNotation.Substring(schemaStart, objectStart - schemaStart);
                                counter--;

                                if (objectNotation[counter].Equals('.'))
                                {
                                    serverName = objectNotation.Substring(0, counter);
                                    return;
                                }

                                while (counter > 0)
                                {
                                    if(objectNotation[counter].Equals('.'))
                                    {
                                        databaseName = objectNotation.Substring(counter + 1, schemaStart - 2 - counter);
                                        serverName = objectNotation.Substring(0, counter);
                                        return;
                                    }

                                    counter--;
                                }

                                databaseName = objectNotation.Substring(0, schemaStart - 1);

                                return;
                            }

                            counter--;
                        }

                        databaseSchema = objectNotation.Substring(0, objectStart);
                    }
                    return;
                }
                counter--;
            }

            databaseObjectName = objectNotation.Substring(counter);
        }

        /// <summary>
        /// Checks if two group of expression have the equal amount
        /// real expression. Mocked expression that are acting as synonymous
        /// are not considered
        /// </summary>
        internal static bool HaveEqualAmountOfRealExpression(List<Expression> one, List<Expression> two)
        {
            int counter = 0;
            List<Expression> biggerOne = one.Count > two.Count ? one : two;
            List<Expression> smallerOne = one.Count > two.Count ? two : one;

            foreach (var el in biggerOne)
            {
                if(!el.Type.Equals(ExpressionType.COLUMN))
                {
                    counter++;
                    continue;
                }

                if (!el.WholeObjectSynonymous)
                    counter++;
            }
            
            return counter == smallerOne.Count;
        }

        /// <summary>
        /// Checks if a list of expressions contains a single real expression
        /// </summary>
        private static bool TryGetSingleRealExpression(List<Expression> expressions, out Expression singleRealExpression)
        {
            singleRealExpression = null;

            foreach(var expression in expressions)
            {
                if (!expression.Type.Equals(ExpressionType.COLUMN))
                    continue;

                if (!expression.WholeObjectSynonymous)
                {
                    if (singleRealExpression != null)
                    {
                        singleRealExpression = null;
                        return false;
                    }
                    else
                        singleRealExpression = expression;
                }
            }

            if (singleRealExpression == null)
                return false;
            else
                return true;
        }

        /// <summary>
        /// Resolves a dataset such as: ('Design Engineer', 'Tool Designer', 'Marketing Assistant')
        /// </summary>
        private static Expression ResolvesDataSet(ReadOnlySpan<TSQLToken> tokens, ref int fileIndex, CompilerContext context)
        {
            if (!tokens[fileIndex].Type.Equals(TSQLTokenType.StringLiteral) && !tokens[fileIndex].Type.Equals(TSQLTokenType.NumericLiteral) && !tokens[fileIndex].Type.Equals(TSQLTokenType.NumericLiteral))
                throw new InvalidSqlException("Unable to resolve dataset");

            Expression dataset = new Expression(ExpressionType.SCALAR_FUNCTION);
            do
            {
                var element = ResolveExpression(tokens, ref fileIndex, context);
                dataset.ChildExpressions.Add(element);
                
                if (tokens.Length > fileIndex && tokens[fileIndex].Text.Equals(","))
                    fileIndex++; //skip ','
                else
                    break;

            }
            while (true);

            return dataset;
        }

        /// <summary>
        /// Checks if a database object reference is followed by an alias and skips the alias
        /// and the AS keyword.
        /// </summary>
        /// <returns>The alias or null if no alias was found</returns>
        private static string ResolveAlias(ReadOnlySpan<TSQLToken> tokens, ref int fileIndex, CompilerContext context)
        {
            if (tokens.Length > fileIndex && tokens[fileIndex].Text.ToLower().Equals("as"))
                fileIndex++;

            if (tokens.Length <= fileIndex || (!tokens[fileIndex].Type.Equals(TSQLTokenType.Identifier) && !tokens[fileIndex].Type.Equals(TSQLTokenType.StringLiteral)))
                return null;

            string alias = tokens[fileIndex].Text.ToLower();
            fileIndex++;

            return alias;
        }

        /// <summary>
        /// Gets the notation of a database object
        /// </summary>
        private static string ResolveDatabaseObjectIdentifier(ReadOnlySpan<TSQLToken> tokens, ref int fileIndex)
        {
            string objectNotation = "";
            bool expectingDot = false;

            while (tokens.Length > fileIndex && (tokens[fileIndex].Type.Equals(TSQLTokenType.Identifier) || tokens[fileIndex].Text.Equals(".")))
            {
                if (expectingDot && tokens[fileIndex].Type.Equals(TSQLTokenType.Identifier))
                    return objectNotation;

                objectNotation += StringHelper.RemoveSquareBrackets(tokens[fileIndex].Text);
                fileIndex++;
                expectingDot = !expectingDot;
            }

            return objectNotation;
        }

        /// <summary>
        /// Gets the notation of a column
        /// </summary>
        private static string ResolveColumnIdentifier(ReadOnlySpan<TSQLToken> tokens, ref int fileIndex)
        {
            string objectNotation = StringHelper.RemoveSquareBrackets(tokens[fileIndex].Text);
            bool expectingDot = true;

            fileIndex++;

            while (tokens.Length > fileIndex)
            {
                if (expectingDot && !tokens[fileIndex].Text.Equals("."))
                    return objectNotation;

                objectNotation += StringHelper.RemoveSquareBrackets(tokens[fileIndex].Text);
                fileIndex++;
                expectingDot = !expectingDot;
            }

            return objectNotation;
        }

        /// <summary>
        /// Skips a collation statement
        /// </summary>
        /// <see href="https://docs.microsoft.com/en-us/sql/t-sql/statements/collations?view=sql-server-ver15"/>
        private static void SkipCollateExpression(ReadOnlySpan<TSQLToken> tokens, ref int fileIndex)
        {
            if (fileIndex >= tokens.Length)
                return;

            if (!tokens[fileIndex].Text.ToLower().Equals("collate"))
                return;

            fileIndex += 2; //skip 'collate <collation_name>'

            if (fileIndex < tokens.Length && (tokens[fileIndex].Text.ToLower().Equals("asc") || tokens[fileIndex].Text.ToLower().Equals("desc")))
                fileIndex++;
        }

        /// <summary>
        /// Resolves a join statement and returns the joined database object
        /// </summary>
        private static DatabaseObject ResolveTableJoin(ReadOnlySpan<TSQLToken> tokens, ref int fileIndex, CompilerContext context)
        {
            if (!tokens[fileIndex].Text.ToLower().Equals("join") && !tokens[fileIndex].Text.ToLower().Equals("apply"))
                throw new ArgumentException("Expected a 'join' or an 'apply' keyword");

            fileIndex++; //skip join or apply

            var dbo = ResolveDatabaseObject(tokens, ref fileIndex, context);

            if (fileIndex < tokens.Length && tokens[fileIndex].Text.ToLower().Equals("on"))
            {
                fileIndex++; //skip 'on'

                SearchConditionResolver.Resolve(tokens, ref fileIndex, context);
            }

            return dbo;
        }

        /// <summary>
        /// Detects a possible join statement and skips the related key words 
        /// </summary>
        /// <returns>The state if a join was detected</returns>
        private static bool DetectTableJoin(ReadOnlySpan<TSQLToken> tokens, ref int fileIndex)
        {
            if (fileIndex >= tokens.Length)
                return false;

            switch (tokens[fileIndex].Text.ToLower())
            {
                case "join": return true;
                case "outer":
                case "cross":

                    {
                        if (tokens[fileIndex + 1].Text.ToLower().Equals("join") || tokens[fileIndex + 1].Text.ToLower().Equals("apply"))
                        {
                            fileIndex++;
                            return true;
                        }
                        else
                        {
                            //outer and cross keyword can also be preceding in "apply" statements
                            return false;
                        }
                    }
                case "self":
                case "inner":
                    {
                        if (tokens[fileIndex + 1].Text.ToLower().Equals("join"))
                        {
                            fileIndex++;
                            return true;
                        }
                        else
                        {
                            throw new InvalidSqlException("Missing Join statement");
                        }
                    }
                case "left":
                case "right":
                case "full":
                    {
                        if (tokens[fileIndex + 1].Text.ToLower().Equals("join"))
                        {
                            fileIndex++;
                            return true;
                        }
                        else
                        {
                            if (tokens[fileIndex + 1].Text.ToLower().Equals("inner") || tokens[fileIndex + 1].Text.ToLower().Equals("outer"))
                            {
                                if (tokens[fileIndex + 2].Text.ToLower().Equals("join"))
                                {
                                    fileIndex += 2;
                                    return true;
                                }
                                else
                                {
                                    throw new InvalidSqlException("Invalid join statement");
                                }
                            }
                            else
                            {
                                throw new InvalidSqlException("Invalid join statement");
                            }
                        }
                    }
                default: return false;
            }
        }

        private static void SkipTableHints(ReadOnlySpan<TSQLToken> tokens, ref int fileIndex)
        {
            if (fileIndex >= tokens.Length || !tokens[fileIndex].Text.ToLower().Equals("with"))
                return;

            fileIndex++; //skip "with"
            fileIndex++; //skip '('
            do
            {
                fileIndex++; //skip "HINT"
                if (tokens[fileIndex].Text.Equals(","))
                {
                    fileIndex++;
                    continue;
                }
                else
                    break;
            }
            while (true);

            fileIndex++; //skip ')'

            return;
        }

        private static void SkipOverClause(ReadOnlySpan<TSQLToken> tokens, ref int fileIndex)
        {
            if (fileIndex < tokens.Length && tokens[fileIndex].Text.ToLower().Equals("over"))
            {
                fileIndex += 2; //skip ' over ('
                int openBracketCounter = 1;

                while (openBracketCounter != 0)
                {
                    if (tokens[fileIndex].Text.Equals("("))
                        openBracketCounter++;

                    if (tokens[fileIndex].Text.Equals(")"))
                        openBracketCounter--;

                    if (fileIndex >= tokens.Length)
                        throw new InvalidSqlException("Unable to resolve over clause");

                    fileIndex++;
                }
            }
        }

        private static void SkipPivotClause(ReadOnlySpan<TSQLToken> tokens, ref int fileIndex)
        {
            while (true)
            {
                if (fileIndex >= tokens.Length || (!tokens[fileIndex].Text.ToLower().Equals("pivot") && !tokens[fileIndex].Text.ToLower().Equals("unpivot")))
                {
                    return;
                }

                fileIndex += 2; //skip 'pivot ('

                int openBracketCounter = 1;

                while (openBracketCounter > 0)
                {
                    if (tokens[fileIndex].Text.ToLower().Equals("("))
                        openBracketCounter++;

                    if (tokens[fileIndex].Text.ToLower().Equals(")"))
                        openBracketCounter--;

                    fileIndex++;
                }


                if (tokens[fileIndex].Text.ToLower().Equals("as"))
                {
                    fileIndex += 2;

                }
                else if (tokens[fileIndex].Type.Equals(TSQLTokenType.Identifier))
                {
                    fileIndex++;
                }
            }

        }
    }
}
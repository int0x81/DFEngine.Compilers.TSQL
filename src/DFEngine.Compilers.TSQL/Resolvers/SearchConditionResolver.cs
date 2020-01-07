using DFEngine.Compilers.TSQL.Models;
using DFEngine.Compilers.TSQL.Exceptions;
using DFEngine.Compilers.TSQL.Helpers;
using System;
using TSQL.Tokens;
using DFEngine.Compilers.TSQL.Constants;

namespace DFEngine.Compilers.TSQL.Resolvers
{
    /// <summary>
    /// Resolves search conditions which are used in WHERE and MATCH statements
    /// <see href="https://docs.microsoft.com/en-us/sql/t-sql/queries/search-condition-transact-sql?view=sql-server-ver15"/>
    /// </summary>
    static class SearchConditionResolver
    {
        /// <summary>
        /// Is a combination of one or more predicates that use the logical operators AND, OR, and NOT. 
        /// <see href="https://docs.microsoft.com/en-us/sql/t-sql/queries/search-condition-transact-sql?view=sql-server-ver15"/>
        /// </summary>
        internal static void Resolve(ReadOnlySpan<TSQLToken> tokens, ref int fileIndex, CompilerContext context)
        {
            bool dummy = false;
            ResolveConcatenatedConditions(tokens, ref fileIndex, context, ref dummy);
        }

        /// <summary>
        /// The method of death. Dont change anything. Trust me, just dont.
        /// </summary>
        private static void ResolveConcatenatedConditions(ReadOnlySpan<TSQLToken> tokens, ref int fileIndex, CompilerContext context, ref bool expectingSecondPart)
        {
            bool expectDatabaseObject = false;
            bool isSecondPartofBetweenExpression = false;

            do
            {
                if (tokens.Length <= fileIndex)
                    break;

                if (tokens[fileIndex].Text.ToLower().Equals("not") || Operators.IsUnaryOperator(tokens[fileIndex].Text.ToLower()))
                    fileIndex++; //skip operator

                if(IsLogicalExpressionOperator(tokens[fileIndex].Text.ToLower()))
                    fileIndex++;

                if (tokens[fileIndex].Text.Equals("("))
                {
                    fileIndex++; //skip '('
                    ResolveConcatenatedConditions(tokens, ref fileIndex, context, ref expectingSecondPart);
                    fileIndex++; //skip ')'
                }
                else
                {
                    do
                    {
                        if(fileIndex < tokens.Length && tokens[fileIndex].Text.ToLower().Equals("null"))
                            fileIndex++; //skip null
                        else if(fileIndex < tokens.Length && (tokens[fileIndex].Text.ToLower().Equals("contains") || tokens[fileIndex].Text.ToLower().Equals("exists")))
                        {
                            var resolver = new TsqlFunctionResolver();
                            resolver.Resolve(tokens, ref fileIndex, context);
                            break;
                        }
                        else if (expectDatabaseObject)
                            StatementResolveHelper.ResolveDatabaseObject(tokens, ref fileIndex, context);
                        else
                        {
                            StatementResolveHelper.ResolveExpression(tokens, ref fileIndex, context);
                            if (isSecondPartofBetweenExpression)
                            {
                                if (!tokens[fileIndex].Text.ToLower().Equals("and"))
                                    throw new InvalidSqlException("BETWEEN keyword was not followed by an AND");

                                fileIndex++; //skip 'AND'
                                StatementResolveHelper.ResolveExpression(tokens, ref fileIndex, context);
                                isSecondPartofBetweenExpression = false;
                            }

                            if (fileIndex < tokens.Length && tokens[fileIndex].Text.Equals("escape", StringComparison.InvariantCultureIgnoreCase))
                                fileIndex += 2; // skip 'escape [pattern]'
                        }

                        if (expectingSecondPart)
                            expectingSecondPart = false;

                        if (fileIndex >= tokens.Length)
                            break;

                        if (tokens[fileIndex].Text.ToLower().Equals("not") || tokens[fileIndex].Text.ToLower().Equals("~"))
                            fileIndex++;

                        string possibleLogicalExpressionOperator = tokens[fileIndex].Text.ToLower();

                        if (possibleLogicalExpressionOperator.Equals("is"))
                        {
                            if (expectingSecondPart)
                                throw new InvalidSqlException("Expression contains multiple logical operators");

                            expectingSecondPart = true;
                            fileIndex++; // skip 'is'

                            if (tokens[fileIndex].Text.ToLower().Equals("not"))
                                fileIndex++;

                            continue;
                        }

                        if (possibleLogicalExpressionOperator.Equals("between"))
                        {
                            if (expectingSecondPart)
                                throw new InvalidSqlException("Expression contains multiple logical operators");

                            expectingSecondPart = true;
                            fileIndex++; // skip 'between'
                            isSecondPartofBetweenExpression = true;
                            continue;
                        }

                        if (possibleLogicalExpressionOperator.Equals("in"))
                        {
                            if (expectingSecondPart)
                                throw new InvalidSqlException("Expression contains multiple logical operators");

                            expectingSecondPart = true;
                            expectDatabaseObject = true;
                            fileIndex++; // skip 'in'
                            continue;
                        }
                        else
                            expectDatabaseObject = false;

                        if (IsLogicalExpressionOperator(tokens[fileIndex].Text))
                        {
                            if (expectingSecondPart)
                                throw new InvalidSqlException("Expression contains multiple logical operators");

                            expectingSecondPart = !expectingSecondPart;

                            fileIndex++;
                            continue;
                        }

                        

                        if (tokens[fileIndex].Text.ToLower().Equals("and") && isSecondPartofBetweenExpression)
                        {
                            fileIndex++;
                            continue;
                        }
                        else
                            break;

                    } while (true);
                }

                if (tokens.Length > fileIndex && !expectingSecondPart && IsLogicalExpressionOperator(tokens[fileIndex].Text.ToLower()))
                {
                    expectingSecondPart = true;
                    fileIndex++; // skip 'operator'
                    continue;
                }

                if (tokens.Length > fileIndex && (tokens[fileIndex].Text.ToLower().Equals("and") || tokens[fileIndex].Text.ToLower().Equals("or")))
                {
                    fileIndex++;
                    continue;
                }
                else
                    break;
            }
            while (true);
        }

        private static bool IsLogicalExpressionOperator(string possibleLogicalExpressionOperator)
        {
            return possibleLogicalExpressionOperator.Equals("=")
                            || possibleLogicalExpressionOperator.Equals("!=")
                            || possibleLogicalExpressionOperator.Equals("<")
                            || possibleLogicalExpressionOperator.Equals(">")
                            || possibleLogicalExpressionOperator.Equals("<=")
                            || possibleLogicalExpressionOperator.Equals(">=")
                            || possibleLogicalExpressionOperator.Equals("<>")
                            || possibleLogicalExpressionOperator.Equals("like")
                            || possibleLogicalExpressionOperator.Equals("between")
                            || possibleLogicalExpressionOperator.Equals("is");
        }
    }
}
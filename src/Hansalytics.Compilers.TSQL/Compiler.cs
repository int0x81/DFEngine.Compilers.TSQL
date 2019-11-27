using System;
using System.Collections.Generic;
using TSQL;
using TSQL.Tokens;
using System.Runtime.CompilerServices;
using Hansalytics.Compilers.TSQL.Models;
using Hansalytics.Compilers.TSQL.Resolvers;
using Hansalytics.Compilers.TSQL.Helpers;

[assembly: InternalsVisibleTo("Hansalytics.Compilers.TSQL.UnitTests")]
[assembly: InternalsVisibleTo("ZSM.Compilers.TSQL.UnitTests")]

namespace Hansalytics.Compilers.TSQL
{
    public class Compiler
    {
        CompilerContext context;
        CompilerResult result;

        /// <summary>
        /// Takes in a tsql string, analyzes it and return all found data queries and data manipulations
        /// </summary>
        /// <param name="tsqlContent">A string containing tsql</param>
        /// <param name="serverName">The name of the server on which the tsql is executed on</param>
        /// <param name="databaseName">The initial database this tsql is targeting</param>
        /// <param name="causer">The name of the entity that executes this sql e.g. the name of a script</param>
        /// <returns>The compiler result containing all queries and manipulations</returns>
        public CompilerResult Compile(string tsqlContent, string serverName, string databaseName, string causer)
        {
            context = new CompilerContext(causer, serverName, databaseName);
            result = new CompilerResult();
            
            AnalyzeTSQLContentString(tsqlContent, context);
            return result;
        }

        internal void AnalyzeTSQLContentString(string tsqlContent, CompilerContext context)
        {
            List<TSQLToken> tokensWithComments = TSQLTokenizer.ParseTokens(tsqlContent);
            ReadOnlySpan<TSQLToken> tokens = FormatTokenList(tokensWithComments);

            int fileIndex = 0;

            while (fileIndex < tokens.Length)
            {
                IDataManipulationResolver manipulationResolver;
                DataManipulation manipulation;

                switch (tokens[fileIndex].Text.ToLower())
                {
                    case "use":
                        new UseStatementResolver().ResolveUseStatement(tokens, ref fileIndex, context);
                        break;
                    //case "declare":
                    //    new VariableDeclarationResolver().Resolve(tokens, ref fileIndex, context);
                    //    break;
                    //case "set":
                    //    gic.AssignVariableValue(tokens, ref fileIndex);
                    //    break;
                    case "with":
                        var withResolver = new WithStatementResolver();
                        withResolver.Resolve(tokens, ref fileIndex, context);
                        break;
                    case "merge":
                        manipulationResolver = new MergeStatementResolver();
                        manipulation = manipulationResolver.Resolve(tokens, ref fileIndex, context);
                        context.DataManipulations.Add(manipulation);
                        context.DropCommonTableExpressions();
                        break;
                    case "bulk":
                        var bulkStatementResolver = new BulkInsertStatementResolver();
                        bulkStatementResolver.Resolve(tokens, ref fileIndex, context);
                        break;
                    case "insert":
                        manipulationResolver = new InsertStatementResolver();
                        manipulation = manipulationResolver.Resolve(tokens, ref fileIndex, context);
                        context.DataManipulations.Add(manipulation);
                        context.DropCommonTableExpressions();
                        break;
                    case "update":
                        manipulationResolver = new UpdateStatementResolver();
                        manipulation = manipulationResolver.Resolve(tokens, ref fileIndex, context);
                        context.DataManipulations.Add(manipulation);
                        context.DropCommonTableExpressions();
                        break;
                    case "select":
                        SelectStatementResolver selectResolver = new SelectStatementResolver();
                        SelectStatement statement = selectResolver.ResolveTopLevel(tokens, ref fileIndex, context);
                        if (statement.TargetObject == null)
                            context.DataQueries.Add(statement.Expression);
                        else
                            context.AddSelectWithIntoClause(statement);
                        context.DropCommonTableExpressions();
                        break;
                    //case "drop":
                    //    gic.ResolveDropStatement(tokens, ref fileIndex, causer);
                    //    break;
                    case "create":
                        new CreateStatementResolver().Resolve(tokens, ref fileIndex, context);
                        break;
                    //case "exec":
                    //case "execute": throw new NotImplementedException("Execute statements are no supported yet");
                    default:
                        fileIndex++;
                        break;
                }
            }

            result.DataQueries = context.DataQueries;
            result.DataManipulations = context.DataManipulations;
        }

        /// <summary>
        /// Stripes all comments from a list of tsql tokens
        /// </summary>
        /// <param name="allTokens">The tokens</param>
        /// <returns>The tokens without comments</returns>
        private ReadOnlySpan<TSQLToken> FormatTokenList(List<TSQLToken> allTokens)
        {
            List<TSQLToken> strippedTokens = new List<TSQLToken>();

            foreach(TSQLToken token in allTokens)
            {
                if (!token.Type.Equals(TSQLTokenType.MultilineComment) && !token.Type.Equals(TSQLTokenType.SingleLineComment))
                    strippedTokens.Add(token);
            }

            ReadOnlySpan<TSQLToken> spanOfTokens = strippedTokens.ToArray();

            return spanOfTokens;
        }
    }
}
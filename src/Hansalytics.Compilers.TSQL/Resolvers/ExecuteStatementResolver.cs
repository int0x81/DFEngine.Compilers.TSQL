//using System;
//using System.Collections.Generic;
//using Hansalytics.Compilers.TSQL.Models;
//using Hansalytics.Compilers.TSQL.Helpers;
//using TSQL.Tokens;

//namespace Hansalytics.Compilers.TSQL.Resolvers
//{
//    /// <summary>
//    /// Requires rework
//    /// </summary>
//    class ExecuteStatementResolver
//    {
//        SQLStatement statement;
//        public List<SQLStatement> Resolve(ReadOnlySpan<TSQLToken> tokens, ref int fileIndex, GlobalInformationContainer gic, string causer)
//        {
//            if(!tokens[fileIndex].Text.ToLower().Equals("exec") && !tokens[fileIndex].Text.ToLower().Equals("execute"))
//                throw new ArgumentException("Trying to resolve execute statement that does not start with a related keyword");

//            statement = new SQLStatement();
//            statement.Type = SQLStatementType.FUNCTION;

//            fileIndex++; //skip "execute"

//            int openingBrackets = TSQLHelper.SkipOpeningBrackets(tokens, ref fileIndex);

//            string executeArgument = string.Empty;

//            if (tokens[fileIndex].Type.Equals(TSQLTokenType.Variable))
//            {
//                executeArgument = StringHelper.RemoveQuotationMarks(gic.GetVariableValue(tokens[fileIndex]));
//            }
//            else if (tokens[fileIndex].Type.Equals(TSQLTokenType.StringLiteral))
//            {
//                executeArgument = StringHelper.RemoveQuotationMarks(tokens[fileIndex].Text);
//            }
//            else if(tokens[fileIndex].Type.Equals(TSQLTokenType.Identifier))
//            {
//                StatementResolveHelper.ResolveTableSource(tokens, ref fileIndex, causer); //Actually resolve stored procedure
//            }

//            if(!string.IsNullOrEmpty(executeArgument))
//            {
//                List<SQLStatement> executeStatements = new Compiler().Compile(executeArgument, gic.ServerName, gic.CurrentDbContext, causer);
//                TSQLHelper.SkipClosingBrackets(tokens, ref fileIndex, openingBrackets);
//                return executeStatements;
//            }
//            else
//            {
//                throw new NotImplementedException("Type of execute-statement is not supported yet");
//            }
            
//        }
//    }
//}

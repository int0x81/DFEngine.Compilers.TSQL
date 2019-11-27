using System;
using System.Collections.Generic;
using System.Text;
using Hansalytics.Compilers.TSQL.Models;
using Hansalytics.Compilers.TSQL.Helpers;
using TSQL.Tokens;

namespace Hansalytics.Compilers.TSQL.Resolvers
{
    class CreateStatementResolver
    {
        /// <summary>
        /// We are not interested in the actual statement, so just skip it
        /// </summary>
        public void Resolve(ReadOnlySpan<TSQLToken> tokens, ref int fileIndex, CompilerContext context)
        {
            fileIndex++; //skip 'create'

            switch (tokens[fileIndex].Text.ToLower())
            {
                case "table":
                    {
                        fileIndex += 2;
                        MoveToFinalBracket(tokens, ref fileIndex);
                        break;
                    }
                case "unique":
                case "clustered":
                case "nonclustered":
                    {
                        while (!tokens[fileIndex].Text.Equals("("))
                            fileIndex++;

                        MoveToFinalBracket(tokens, ref fileIndex); //skip column name

                        if (tokens[fileIndex].Text.ToLower().Equals("include"))
                        {
                            fileIndex++;
                            MoveToFinalBracket(tokens, ref fileIndex);
                        }
                        if (tokens[fileIndex].Text.ToLower().Equals("with"))
                        {
                            fileIndex++;
                            MoveToFinalBracket(tokens, ref fileIndex);
                        }
                        if (tokens[fileIndex].Text.ToLower().Equals("on"))
                        {
                            fileIndex++;
                            StatementResolveHelper.ResolveExpression(tokens, ref fileIndex, context);
                        }

                        break;
                    }
            }
        }

        private void MoveToFinalBracket(ReadOnlySpan<TSQLToken> tokens, ref int fileIndex)
        {
            if (!tokens[fileIndex].Text.Equals("("))
                return;

            fileIndex++; //skip '('

            int openBracketCounter = 1;

            while (openBracketCounter > 0)
            {
                if (tokens[fileIndex].Text.Equals("("))
                    openBracketCounter++;
                if (tokens[fileIndex].Text.Equals(")"))
                    openBracketCounter--;

                fileIndex++;
            }
        }
    }
}

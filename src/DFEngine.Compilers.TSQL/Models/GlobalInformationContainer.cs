using DFEngine.Compilers.TSQL.Models;
using DFEngine.Compilers.TSQL.Exceptions;
using DFEngine.Compilers.TSQL.Helpers;
using System;
using System.Collections.Generic;
using System.Text;
using TSQL.Tokens;
using DFEngine.Compilers.TSQL.Resolvers;

namespace DFEngine.Compilers.TSQL.Models
{
    //Represents the CompilerContext from an older version. May contain some interesting functions
    class GlobalInformationContainer
    {
        
        //public GlobalInformationContainer()
        //{
        //    CurrentVariables = new List<TemporaryVariable>();
        //    ResolvedStatements = new List<SQLStatement>();
        //    CurrentCTEs = new List<CommonTableExpression>();
        //    TemporaryTables = new List<DataObjectAlias>();
        //}

        //internal void AssignVariableValue(ReadOnlySpan<TSQLToken> tokens, ref int fileIndex)
        //{
        //    TSQLHelper.CheckIsCorrectStatement(tokens, fileIndex, "set");

        //    fileIndex++; //skip 'set'

        //    foreach (TemporaryVariable variable in CurrentVariables)
        //    {
        //        if(variable.Identifier.Equals(tokens[fileIndex].Text))
        //        {
        //            variable.Value = tokens[fileIndex + 2].Text;
        //            return;
        //        }
        //    }
        //    if (tokens[fileIndex].Type.Equals(TSQLTokenType.SystemIdentifier))
        //        return;
        //}

        //internal void ResolveVariableDeclaration(ReadOnlySpan<TSQLToken> tokens, ref int fileIndex, string causer)
        //{
        //    TSQLHelper.CheckIsCorrectStatement(tokens, fileIndex, "declare");

        //    fileIndex++; //skip "declare"
        //    do
        //    {
        //        string identifier = tokens[fileIndex].Text;
        //        fileIndex++;

        //        if (tokens[fileIndex].Text.ToLower().Equals("as"))
        //            fileIndex++;

        //        string variableType = tokens[fileIndex].Text.ToLower();

        //        switch (variableType)
        //        {
        //            case "table":
        //                TemporaryTables.Add(new DataObjectAlias(identifier));
        //                fileIndex++;
        //                TSQLHelper.MoveIndexToFinalBracket(tokens, ref fileIndex);
        //                return;
        //            case "cursor":
        //                TSQLHelper.ResolveCursorDeclaration(tokens, ref fileIndex, causer);
        //                return;
        //            default:
        //                IStatementResolver resolver = TsqlKeywordDictionary.CreateStatementResolver(tokens[fileIndex], causer);
        //                if (resolver == null)
        //                    throw new NotImplementedException($"The type {tokens[fileIndex].Text} is not implemented yet");

        //                resolver.Resolve(tokens, ref fileIndex, causer); //skip to end of type

        //                if (fileIndex < tokens.Length && tokens[fileIndex].Text.Equals("="))
        //                {
        //                    fileIndex++; //skip '='
        //                    CurrentVariables.Add(new TemporaryVariable(identifier, tokens[fileIndex].Text));
        //                    fileIndex++; //skip value
        //                }
        //                else
        //                {
        //                    CurrentVariables.Add(new TemporaryVariable(identifier));
        //                }
        //                break;
        //        }

        //        if (fileIndex < tokens.Length && tokens[fileIndex].Text.Equals(","))
        //        {
        //            fileIndex++;
        //            continue;
        //        }
        //        else
        //        {
        //            break;
        //        }

        //    } while (true);
        //}

        //internal void ResolveVariableDependencies(SQLStatement statement)
        //{
        //    foreach(DataObjectAlias alias in TemporaryTables)
        //    {
        //        if (statement.TargetTable == null)
        //            break;

        //        if(statement.TargetTable.Name.Equals(alias.Alias.ToLower()))
        //        {
        //            alias.ReferencingTables.AddRange(statement.SourceTables);
        //            alias.ReferencingFiles.AddRange(statement.SourceFiles);
        //            statement.TargetTable = null;
        //            return;
        //        }
        //    }
            
        //    for(int counter = 0; counter < statement.SourceTables.Count; counter++)
        //    {
        //        Table sourceTable = statement.SourceTables[counter];

        //        foreach(DataObjectAlias alias in TemporaryTables)
        //        {
        //            if(sourceTable.Equals(alias.Alias))
        //            {
        //                statement.SourceTables.RemoveAt(counter);
        //                counter--;

        //                statement.SourceTables.AddRange(alias.ReferencingTables);
        //                counter += alias.ReferencingTables.Count;

        //                statement.SourceFiles.AddRange(alias.ReferencingFiles);
        //            }
        //        }
        //    }
        //}

        ///// <summary>
        ///// Deletes the specified table from the created table list.
        ///// If the specified table occures in one of the resolved statements these statements get deleted aswell
        ///// and new ghost statements are created to map the dependencies between the incoming and outgoing dependencies
        ///// of the dropped table
        ///// </summary>
        ///// <param name="tokens">The tokens</param>
        ///// <param name="fileIndex">The current file index</param>
        //internal void ResolveDropStatement(ReadOnlySpan<TSQLToken> tokens, ref int fileIndex, string causer)
        //{
        //    TSQLHelper.CheckIsCorrectStatement(tokens, fileIndex, "drop");

        //    fileIndex++; //skip "drop"

        //    if (!tokens[fileIndex].Text.ToLower().Equals("table"))
        //        return;

        //    fileIndex++;

        //    if (tokens[fileIndex].Text.ToLower().Equals("if"))
        //        fileIndex += 2; //skip "if exsists"

        //    Table tableToBeDropped;

        //    TableResolvingOperationResult result = StatementResolveHelper.ResolveTableSource(tokens, ref fileIndex, causer);

        //    if (result.ConcreteTables.Count == 1 && result.TableAliases.Count == 0)
        //    {
        //        tableToBeDropped = result.ConcreteTables[0];

        //        PostResolvingProcessor.AddMissingContexts(tableToBeDropped, ServerName, CurrentDbContext);
        //    }
        //    else
        //    {
        //        throw new InvalidSqlException("Unable to resolve target table in drop statement");
        //    }

        //    List<Table> incomingTableDependencies = new List<Table>();
        //    List<string> incomingFileDependencies = new List<string>();
        //    List<Table> outgoingDependencies = new List<Table>();

        //    for (int c0 = 0; c0 < ResolvedStatements.Count; c0++)
        //    {
        //        SQLStatement statement = ResolvedStatements[c0];

        //        if (statement.TargetTable == null)
        //            continue;

        //        for(int c1 = 0; c1 < statement.SourceTables.Count; c1++)
        //        {
        //            if(statement.SourceTables[c1].Equals(tableToBeDropped))
        //            {
        //                if(statement.SourceTables.Count == 1)
        //                {
        //                    outgoingDependencies.Add(statement.TargetTable);

        //                    ResolvedStatements.RemoveAt(c0);
        //                    c0--;
        //                    break;
        //                }
        //                else
        //                {
        //                    statement.SourceTables.RemoveAt(c1);
        //                    c1--;
        //                }
        //            }
        //        }

        //        if(statement.TargetTable.Equals(tableToBeDropped))
        //        {
        //            incomingTableDependencies.AddRange(statement.SourceTables);
        //            incomingFileDependencies.AddRange(statement.SourceFiles);

        //            ResolvedStatements.RemoveAt(c0);
        //            c0--;
        //        }
        //    }

        //    foreach(Table ghostTarget in outgoingDependencies)
        //    {
        //        SQLStatement ghostStatement = new SQLStatement
        //        {
        //            TargetTable = ghostTarget,
        //            SourceFiles = incomingFileDependencies,
        //            SourceTables = incomingTableDependencies
        //        };

        //        ResolvedStatements.Add(ghostStatement);
        //    }
        //}
    }
}

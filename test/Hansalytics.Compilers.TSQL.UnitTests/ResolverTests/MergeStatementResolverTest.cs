using DFEngine.Compilers.TSQL.Models;
using DFEngine.Compilers.TSQL.Helpers;
using System.Collections.Generic;
using Xunit;

namespace DFEngine.Compilers.TSQL.UnitTests.ResolverTests
{
    
    public class MergeStatementResolverTest
    {
        /// <summary>
        /// Resolves a merge statement. To check if the Statement is resolved as whole (and the insert, updates, etc are not resolved as single statements), 
        /// the sql is resolved by a whole compiler instance
        /// </summary>
        [Fact]
        public void ShouldResolveRecursiveMergeStatement()
        {
            //Arrange
            string rawTsql = "    MERGE BI_STAGE.dbo.ERP_Company AS t " +
                             "    USING( " +
                             "        SELECT [Name], " +
                             "             RTRIM(LEFT([Name], 5)) COLLATE Latin1_General_CI_AS  as CompanyNo, " +
                             "             (SELECT [Type of Company] FROM SOMEOBFSERVER.ERP2099PROD.dbo.[Compam] WHERE [Name] = Company.[Name]) AS AccountType, " +
                             "             CAST(timestamp as bigint) AS Timestamp " +
                             "        FROM ERP_SERVER.ERP2099PROD.dbo.[Compam] " +
                             "        WHERE " +
                             "            ( " +
                             "            ISNUMERIC(LEFT([Name], 4)) = 1 " +
                             "            OR ([Name] LIKE '[0-9][0-9][0-9][0-9][A-Z][ ]') " +
                             "            ) " +
                             "     ) AS s " +
                             "     ON t.No = s.CompanyNo " +
                             "     WHEN MATCHED THEN " +
                             "          UPDATE SET " +
                             "                             t.Name = s.[Name], " +
                             "                             t.AccountType = s.AccountType, " +
                             "                             t.Timestamp = s.Timestamp, " +
                             "                             LoadRunId = @LoadRun " +
                             "     WHEN NOT MATCHED THEN " +
                             "          INSERT(            No, " +
                             "                             Name, " +
                             "                             AccountType, " +
                             "                             Timestamp, " +
                             "                             LoadRunId) " +
                             "          VALUES(            s.CompanyNo, " +
                             "                             s.Name, " +
                             "                             s.AccountType, " +
                             "                             s.timestamp, " +
                             "                             @LoadRun) " +
                             "      WHEN NOT MATCHED BY SOURCE THEN " +
                             "           UPDATE SET t.IsDeleted = 1, " +
                             "                      t.NoImport = 1 " +
                             "       OUTPUT $ACTION ,Inserted.*,Deleted.*;";

            //Act
            var result = new Compiler().Compile(rawTsql, "std_server", "std_database", "Xunit");

            //Assert
            Assert.Empty(result.DataQueries);
            Assert.Single(result.DataManipulations);
        }
    }
}

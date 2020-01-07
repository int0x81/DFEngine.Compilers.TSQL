﻿using DFEngine.Compilers.TSQL.Models;
using System;
using System.IO;

namespace DFEngine.Compilers.TSQL.LiveTest
{
    /// <summary>
    /// Simple console application for testing purposes that compiles scripts in a local folder
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            const string folderPath = @"C:\Users\Finn.Fiedler\source\Workspaces\BI\BI_ETL\ETL_Scripts\active";

            var files = Directory.EnumerateFiles(folderPath, "*.sql");

            var compiler = new Compiler();

            foreach(string file in files)
            {
                string tsqlContent = File.ReadAllText(file);
   
                var options = new CompilerOptions() { ConsiderQueries = true };
                var result = compiler.Compile(tsqlContent, "std_server", "std_db", file, options);
                //CheckAllColumnNotations(result);
            }
        }

        /// <summary>
        /// Checks if all columns of a result generated by the compiler have full notation
        /// (database.schema.dbo.column)
        /// </summary>
        private static void CheckAllColumnNotations(CompilerResult result)
        {
            foreach (var mani in result.DataManipulations)
            {
                foreach (var exp in mani.Expressions)
                    CheckColumnNotation(exp);
            }

            foreach (var exp in result.DataQueries)
                CheckColumnNotation(exp);
        }

        /// <summary>
        /// Checks if an expression including all child expressions have full notation
        /// (database.schema.dbo.column)
        /// </summary>
        private static void CheckColumnNotation(Expression expression)
        {
            if (!expression.Type.Equals(ExpressionType.COLUMN))
                return;
            else
            {
                Helper.SplitColumnNotationIntoSingleParts(expression.Name, out string databaseName, out string databaseSchema, out string databaseObjectName, out string columnName, true);


                if (string.IsNullOrEmpty(columnName))
                    throw new ArgumentException("Column name may not be emtpy.", "columnNotation");

                if (string.IsNullOrEmpty(databaseObjectName))
                    throw new ArgumentException("Database object may not be emtpy.", "columnNotation");

                if (string.IsNullOrEmpty(databaseSchema))
                    throw new ArgumentException("Database schema may not be emtpy.", "columnNotation");

                if (string.IsNullOrEmpty(databaseName))
                    throw new ArgumentException("Database name may not be emtpy.", "columnNotation");
            }

            foreach (var child in expression.ChildExpressions)
                CheckColumnNotation(child);
        }
    }
}

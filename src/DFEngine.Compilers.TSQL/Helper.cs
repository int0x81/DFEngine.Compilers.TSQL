using DFEngine.Compilers.TSQL.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace DFEngine.Compilers.TSQL
{
    /// <summary>
    /// Contains helper method that are considered usefull for users of this library
    /// </summary>
    public static class Helper
    {
        /// <summary>
        /// Takes in a column notation and extracts in the single entities
        /// </summary>
        /// <param name="columnNotation">the notation of the column. Each part may contains square brackets</param>
        public static void SplitColumnNotationIntoSingleParts(string columnNotation, out string databaseName, out string databaseSchema, out string databaseObjectName, out string columnName)
        {
            if (string.IsNullOrEmpty(columnNotation))
                throw new ArgumentException("Invalid columnname");

            int counter = columnNotation.Length - 1;
            int lastDotPosition = -1;

            databaseName = null;
            databaseSchema = null;
            databaseObjectName = null;
            columnName = null;

            while (counter > 0)
            {
                if (columnNotation[counter].Equals('.'))
                {
                    if (lastDotPosition == -1)
                        columnName = StringHelper.RemoveSquareBrackets(columnNotation.Substring(counter + 1));
                    else
                    {
                        if (databaseObjectName == null)
                            databaseObjectName = StringHelper.RemoveSquareBrackets(columnNotation.Substring(counter + 1, lastDotPosition - counter));
                        else if (databaseSchema == null)
                            databaseSchema = StringHelper.RemoveSquareBrackets(columnNotation.Substring(counter + 1, lastDotPosition - counter));
                    }

                    lastDotPosition = counter - 1;
                }

                counter--;
            }

            counter--;

            if (lastDotPosition == -1)
                columnName = StringHelper.RemoveSquareBrackets(columnNotation.Substring(counter + 1));
            else
            {
                if (databaseObjectName == null)
                    databaseObjectName = StringHelper.RemoveSquareBrackets(columnNotation.Substring(counter + 1, lastDotPosition - counter));
                else if (databaseSchema == null)
                    databaseSchema = StringHelper.RemoveSquareBrackets(columnNotation.Substring(counter + 1, lastDotPosition - counter));
                else if (databaseName == null)
                    databaseName = StringHelper.RemoveSquareBrackets(columnNotation.Substring(counter + 1, lastDotPosition - counter));
            }
        }
    }
}

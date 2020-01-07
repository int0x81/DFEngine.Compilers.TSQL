using System;
using System.Collections.Generic;
using System.Text;

namespace DFEngine.Compilers.TSQL.Helpers
{
    /// <summary>
    /// Provides multiple helper methods for string manipulation
    /// </summary>
    public static class StringHelper
    {
        /// <summary>
        /// Removes all square brackets from a string
        /// </summary>
        public static string RemoveSquareBrackets(string inputString)
        {
            if (string.IsNullOrEmpty(inputString))
                throw new ArgumentException("Trying to remove brackets from an empty string", "inputString");

            return inputString.Replace("[", string.Empty).Replace("]", string.Empty);
        }

        /// <summary>
        /// Removes all quotation marks from a string
        /// </summary>
        public static string RemoveQuotationMarks(string inputString)
        {
            if (string.IsNullOrEmpty(inputString))
                throw new ArgumentException("Trying to remove quotation marks from an empty string", "inputString");

            return inputString.Replace("\"", string.Empty).Replace("'", string.Empty);
        }
    }
}
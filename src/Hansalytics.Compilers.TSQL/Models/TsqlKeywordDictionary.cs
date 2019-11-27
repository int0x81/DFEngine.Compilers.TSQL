using Hansalytics.Compilers.TSQL.Resolvers;
using System.Collections.Generic;

namespace Hansalytics.Compilers.TSQL.Models
{
    static class TsqlKeywordDictionary
    {
        internal static readonly Dictionary<string, IExpressionResolver> EXPRESSION_RESOLVERS = new Dictionary<string, IExpressionResolver>()
        {
             { "select", new SelectStatementResolver() },
             { "case", new CaseStatementResolver() },
             { "cast",  new CastStatementResolver() },

             //typecasts
             { "decimal", new TypeCasterResolver() },
             { "varchar", new TypeCasterResolver() },
             { "nvarchar", new TypeCasterResolver() },
             { "nchar", new TypeCasterResolver() },
             { "int", new TypeCasterResolver() },
             { "bigint", new TypeCasterResolver() },
             { "datetime", new TypeCasterResolver() },
             { "date", new TypeCasterResolver() },
             { "time", new TypeCasterResolver() },
             { "float", new TypeCasterResolver() },
             { "tinyint", new TypeCasterResolver() },
             { "bit", new TypeCasterResolver() },
             { "varbinary", new TypeCasterResolver() },
             { "binary", new TypeCasterResolver() },
             { "money", new TypeCasterResolver() },
             { "uniqueidentifier", new TypeCasterResolver() },
             { "char", new TypeCasterResolver() },

                //special datatypes
                // "geography",

                //analytic functions
                //{ "lag", new TsqlFunctionResolver() },

                //date functions
                //{ "dateadd", new TsqlFunctionResolver() },
                //{ "datediff", new TsqlFunctionResolver() },
                //{ "getdate", new TsqlFunctionResolver() },
                //{ "year", new TsqlFunctionResolver() },
                //{ "month", new TsqlFunctionResolver() },
                //{ "hour", new TsqlFunctionResolver() },
                //{ "second", new TsqlFunctionResolver() },
                //{ "minute", new TsqlFunctionResolver() },
                //{ "datepart", new TsqlFunctionResolver() },
                //{ "sysdatetimeoffset", new TsqlFunctionResolver() },
                //{ "sysdatetime", new TsqlFunctionResolver() },
                //{ "sysutcdatetime", new TsqlFunctionResolver() },

                //string functions
                //{ "left", new TsqlFunctionResolver() },
                //{ "rtrim", new TsqlFunctionResolver() },
                //{ "ltrim", new TsqlFunctionResolver() },
                //{ "substring", new TsqlFunctionResolver() },
                //{ "len", new TsqlFunctionResolver() },
                //{ "charindex", new TsqlFunctionResolver() },
                //{ "patindex", new TsqlFunctionResolver() },
                //{ "reverse", new TsqlFunctionResolver() },
                //{ "str", new TsqlFunctionResolver() },
                //{ "replace", new TsqlFunctionResolver() },
                //{ "stuff", new TsqlFunctionResolver() },
                //{ "string_split", new TsqlFunctionResolver() },

                //math functions
                //{ "power", new TsqlFunctionResolver() },
                //{ "sum", new TsqlFunctionResolver() },
                //{ "avg", new TsqlFunctionResolver() },
                //{ "abs", new TsqlFunctionResolver() },

                //metadata functions
                //{ "object_id", new TsqlFunctionResolver() },

                //aggregate functions
                //{ "min", new TsqlFunctionResolver() },
                //{ "max", new TsqlFunctionResolver() },
                //{ "hashbytes", new TsqlFunctionResolver() },
                //{ "isnumeric", new TsqlFunctionResolver() },

                //other functions
                //{ "rank", new TsqlFunctionResolver() },
                //{ "error_message", new TsqlFunctionResolver() },
                //{ "error_procedure", new TsqlFunctionResolver() },
                //{ "error_number", new TsqlFunctionResolver() },
                //{ "error_severity", new TsqlFunctionResolver() },
                //{ "error_state", new TsqlFunctionResolver() },
                //{ "error_line", new TsqlFunctionResolver() },
                //{ "isnull", new TsqlFunctionResolver() },
                //{ "convert", new TsqlFunctionResolver() },
                //{ "coalesce", new TsqlFunctionResolver() },
                //{ "row_number", new TsqlFunctionResolver() },
                //{ "nullif", new TsqlFunctionResolver() },
        };
    }
}
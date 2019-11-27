using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Hansalytics.Compilers.TSQL.UnitTests
{
    public class HelperTest
    {
        /// <summary>
        /// Takes a simple column notation (without table reference etc),
        /// builds the correct object and should places it in the landscape 
        /// </summary>
        [Fact]
        public void ShouldSplitSimpleColumnNotation()
        {
            //Arrange
            string notation = "testcolumn";

            //Act
            Helper.SplitColumnNotationIntoSingleParts(notation, out string databaseName, out string databaseSchema, out string databaseObjectName, out string columnName);

            //Assert
            Assert.Equal("testcolumn", columnName);
        }

        /// <summary>
        /// Takes a full notated column (including database, schema and table),
        /// builds the correct objects and should places them in the landscape
        /// </summary>
        [Fact]
        public void ShouldSplitFullColumnNotation()
        {
            //Arrange
            string notation = "[userschema].[testTable].testcolumn";

            //Act
            Helper.SplitColumnNotationIntoSingleParts(notation, out string databaseName, out string databaseSchema, out string databaseObjectName, out string columnName);

            //Assert
            Assert.Null(databaseName);
            Assert.Equal("userschema", databaseSchema);
            Assert.Equal("testTable", databaseObjectName);
            Assert.Equal("testcolumn", columnName);
        }
    }
}

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data;
using System.Data.SQLite;

namespace KBKA.Tests
{
    /// <summary>
    /// Test Class for SQLDA.cs
    /// </summary>
    [TestClass()]
    public class SQLiteDATests
    {
        /// <summary>
        /// Checks if method can establish connection with SQLLite
        /// </summary>
        [TestMethod()]
        public void OpenConnectionTestNull()
        {
            SQLiteConnection expected = null;
            SQLiteConnection sqlConn = KBKA.SQLiteDA.OpenConnection();
            Assert.AreNotEqual(expected, sqlConn);
        }
        /// <summary>
        /// Checks if method returns correct data if database contains needed data
        /// </summary>
        [TestMethod()]
        public void GetDataTestNull()
        {
            DataTable expected = null;
            DataTable data = KBKA.SQLiteDA.GetData("todo","todo","03.06.2020");
            Assert.AreNotEqual(expected, data);
        }
    }
}
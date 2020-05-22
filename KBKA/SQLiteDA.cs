using Dapper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace KBKA
{
    public static class SQLiteDA
    {
        private static SQLiteConnection sqlConn;
        private static SQLiteCommand sqlCmd;
        private static SQLiteDataAdapter DA;
        private static DataSet DS = new DataSet();
        private static DataTable DT = new DataTable();
       

        //set connection and open the database
        public static SQLiteConnection OpenConnection()
        {
            sqlConn = new SQLiteConnection("Data Source= data.db;Version=3;");
            try
            {
                sqlConn.Open();
            }
            catch (Exception ex)
            {

            }
            return sqlConn;
        }

        //set function to execute the query

        public static void ExecuteQuerry(string querryText)
        {
            OpenConnection();
            sqlCmd = sqlConn.CreateCommand();
            sqlCmd.CommandText = querryText;
            sqlCmd.ExecuteNonQuery();
            sqlConn.Close();
        }

        //set function to show data in datagrids

        public static DataTable GetData(string column,string table)
        {
            OpenConnection();
            sqlCmd = sqlConn.CreateCommand();
            sqlCmd.CommandText = "select "+column+" from " + table;
            sqlCmd.ExecuteNonQuery();

            DA = new SQLiteDataAdapter(sqlCmd);
            DT = new DataTable();
            DA.Fill(DT);
            sqlConn.Close();
            return DT;
        }

    }
}

using Dapper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.Diagnostics;
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
           // sqlConn.Close();
        }
        public static bool LookForTable(string table)
        {
            bool exists=false;
            OpenConnection();

            try
            {
              
                sqlCmd = sqlConn.CreateCommand();
                sqlCmd.CommandText = "select case when exists((select * from information_schema.tables where table_name = '" + table + "')) then 1 else 0 end";
                
                exists = (int)sqlCmd.ExecuteScalar() == 1;
            }
            catch
            {
            }
           // sqlConn.Close();
            return exists;
        }
        // create user's tables
        public static void CreateTables (string usersub)
        {
            OpenConnection();
            string createTodo = "CREATE TABLE IF NOT EXISTS " +'"'+ usersub + "td"+'"'+" (date TEXT NOT NULL, todo TEXT NOT NULL )";
            string createInprogress = "CREATE TABLE IF NOT EXISTS " + '"' + usersub + "ip" + '"' + " (date TEXT NOT NULL, inprogress TEXT NOT NULL )";
            string createDone = "CREATE TABLE IF NOT EXISTS " + '"' + usersub + "d" + '"' + "  (date TEXT NOT NULL, done TEXT NOT NULL )";

            sqlCmd = sqlConn.CreateCommand();
            sqlCmd.CommandText = createTodo;
            sqlCmd.ExecuteNonQuery();
            //sqlCmd = sqlConn.CreateCommand();
            sqlCmd.CommandText = createInprogress;
            sqlCmd.ExecuteNonQuery();
            sqlCmd = sqlConn.CreateCommand();
            sqlCmd.CommandText = createDone;
            sqlCmd.ExecuteNonQuery();
            //sqlConn.Close();
        }

        //set function to show data in datagrids

        public static DataTable GetData(string table, string column, string date)
        {
            OpenConnection();
            sqlCmd = sqlConn.CreateCommand();
            sqlCmd.CommandText = "select "+column+" from " +'"'+ table+'"'+ " where date = "+'"'+date+'"';
            sqlCmd.ExecuteNonQuery();

            DA = new SQLiteDataAdapter(sqlCmd);
            DT = new DataTable();
            DA.Fill(DT);
           // sqlConn.Close();
            return DT;
        }

        public static void Add (string table, string column, string date, string content)
        {
            OpenConnection();
            string add = "insert into '" + table + "' (date,'" + column + "') values('" + date + "',"+'"' + content + '"'+')';
            sqlCmd = sqlConn.CreateCommand();
            sqlCmd.CommandText = add;
            sqlCmd.ExecuteNonQuery();
        }

        public static void Edit(string table, string column, string date, string tbcontent,string dgcontent)
        {
            OpenConnection();

            string edit = "update "+'"'+ table +'"' +" set " + column + '='+'"'+tbcontent+'"'+" where date= "+'"'+ date+'"'+" AND "+ column +'='+'"'+dgcontent+'"';
            Trace.WriteLine(edit);
            sqlCmd.CommandText = edit;
            sqlCmd.ExecuteNonQuery();
        }
        public static void Delete(string table, string column, string date, string content)
        {
            OpenConnection();
            string edit = "delete '" + table + "' where date= " + '"' + date + '"'+"AND"+'"'+column+'='+'"'+content+'"';
           // n AND sth
            sqlCmd = sqlConn.CreateCommand();
            sqlCmd.CommandText = edit;
            sqlCmd.ExecuteNonQuery();
        }

    }
}

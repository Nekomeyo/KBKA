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
using System.Windows;

namespace KBKA
{/// <summary>
/// Logic of interaction with Database
/// </summary>
    public static class SQLiteDA
    {
        private static SQLiteConnection sqlConn;
        private static SQLiteCommand sqlCmd;
        private static SQLiteDataAdapter DA;
        private static DataSet DS = new DataSet();
        private static DataTable DT = new DataTable();


        /// <summary>
        /// Sets connection and opens the database
        /// </summary>
        /// <returns>
        /// Acces to database
        /// </returns>
        public static SQLiteConnection OpenConnection()
        {
            sqlConn = new SQLiteConnection("Data Source= data.db;Version=3;");
            try
            {
                sqlConn.Open();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return sqlConn;
        }


        /// <summary>
        /// Set function to execute the query
        /// </summary>
        /// <param name="querryText"></param>
        public static void ExecuteQuerry(string querryText)
        {
            OpenConnection();
            sqlCmd = sqlConn.CreateCommand();
            sqlCmd.CommandText = querryText;
            sqlCmd.ExecuteNonQuery();
            
        }

        /// <summary>
        /// Creates user's tables if they do not exist
        /// </summary>
        /// <param name="usersub"></param>
        public static void CreateTables(string usersub)
        {
            try
            {

                OpenConnection();
                string createTodo = "CREATE TABLE IF NOT EXISTS " + '"' + usersub + "td" + '"' + " (" + '"' + "No" + '"' + " INTEGER NOT NULL PRIMARY KEY , Date TEXT NOT NULL, ToDo TEXT NOT NULL )";
                string createInprogress = "CREATE TABLE IF NOT EXISTS " + '"' + usersub + "ip" + '"' + " (" + '"' + "No" + '"' + " INTEGER NOT NULL PRIMARY KEY , Date TEXT NOT NULL, InProgress TEXT NOT NULL )";
                string createDone = "CREATE TABLE IF NOT EXISTS " + '"' + usersub + "d" + '"' + " (" + '"' + "No" + '"' + " INTEGER NOT NULL PRIMARY KEY ,  Date TEXT NOT NULL, Done TEXT NOT NULL )";

                sqlCmd = sqlConn.CreateCommand();
                sqlCmd.CommandText = createTodo;
                sqlCmd.ExecuteNonQuery();
               
                sqlCmd.CommandText = createInprogress;
                sqlCmd.ExecuteNonQuery();
               
                sqlCmd.CommandText = createDone;
                sqlCmd.ExecuteNonQuery();
                sqlConn.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        /// <summary>
        /// Set function to show data in datagrids
        /// </summary>
        /// <param name="table"></param>
        /// <param name="column"></param>
        /// <param name="date"></param>
        /// <returns>
        /// Data from database shown in datagrids
        /// </returns>
        public static DataTable GetData(string table, string column, string date)
        {
            try
            {
                OpenConnection();
                sqlCmd = sqlConn.CreateCommand();
                sqlCmd.CommandText = "select " + '"' + "No" + '"' + ", " + column + " from " + '"' + table + '"' + " where Date = " + '"' + date + '"';
             
            sqlCmd.ExecuteNonQuery();

                DA = new SQLiteDataAdapter(sqlCmd);
                DT = new DataTable();
                DA.Fill(DT);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return DT;
        }
        /// <summary>
        /// Adds row with content to table 
        /// </summary>
        /// <param name="table"></param>
        /// <param name="column"></param>
        /// <param name="date"></param>
        /// <param name="tbContent"></param>
        /// <param name="noContent"></param>
        public static void Add(string table, string column, string date, string tbContent, string noContent)
        {
            try {
                OpenConnection();
                string add = "insert into '" + table + "' (" + '"' + "No" + '"' + ", " + " Date, " + '"' + column + '"'+") values("+'"' + noContent + '"' + ", " + '"' + date +'"'+ ", " + '"' + tbContent +'"'+ ')';
                
            sqlCmd = sqlConn.CreateCommand();
                sqlCmd.CommandText = add;
                sqlCmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
               MessageBox.Show("Looks like something has gone wrong."+"\n"+"Be sure that entered data is correct");
            }
        } 
        /// <summary>
        /// Used to edit existing rows in table and alter their content
        /// </summary>
        /// <param name="table"></param>
        /// <param name="column"></param>
        /// <param name="date"></param>
        /// <param name="tbContent"></param>
        /// <param name="noContent"></param>
        public static void Edit(string table, string column, string date, string tbContent,string noContent)
        {
            try {
            OpenConnection();

            string edit = "update "+'"'+ table +'"' +" set " +'"'+ column +'"'+ "="+'"'+tbContent+'"'+ " Where ("+'"'+"No"+'"'+" = " + '"' + noContent + '"' + " AND "+'"'+"Date"+'"'+"= "+'"'+ date+'"'+ ")" ;
            Trace.WriteLine(edit);
            sqlCmd.CommandText = edit;
            sqlCmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Looks like something has gone wrong." + "\n" + "Be sure that entered data is correct");
            }
        }
        /// <summary>
        /// Used to delate rows with their content
        /// </summary>
        /// <param name="table"></param>
        /// <param name="date"></param>
        /// <param name="noContent"></param>
        public static void Delete(string table, string date, string noContent)
        {
            try { 
            OpenConnection();
            string edit = "delete from "+'"'+ table+'"' + " Where (" + '"' + "No" + '"' + " = " + '"' + noContent + '"' + " AND " + '"' + "Date" + '"' + "= " +'"'+ date+'"'+")";

            sqlCmd = sqlConn.CreateCommand();
            sqlCmd.CommandText = edit;
            sqlCmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Looks like something has gone wrong." + "\n" + "Be sure that entered data is correct");
            }
        }

    }
}

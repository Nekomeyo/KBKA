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
                MessageBox.Show(ex.Message);
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
            
        }
      
        // create user's tables if not exists
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

        //set function to show data in datagrids

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

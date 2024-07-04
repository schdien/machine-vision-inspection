using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
//using System.Data.SQLite;
using Microsoft.Data.Sqlite;
using System.IO;
using System.ComponentModel;

namespace IndustrialCamera
{
    static class ImageDatabase
    {
        public static SqliteConnection dbConnection;

        static private void CreateFile(string databaseFileName)
        {
            File.WriteAllBytes(databaseFileName, new byte[0]);
        }
        public static DataView Table
        {
            get
            {
                return GetTable().DefaultView;
            }
        }
        /*
        public static void Create()
        {
            if (!File.Exists("ImageDatabase.sqlite"))
                SqliteConnection.CreateFile("ImageDatabase.sqlite");
        }
        */
        public static bool CreateAccount(string userName,string password)
        {
            if (userName == null || userName == "")
            {
                return false;
            }
            else
            {
                string fileName = userName + ".sqlite";
                if (File.Exists(fileName))
                {
                    return false;//用户已存在
                }
                else
                {
                    CreateFile(fileName);
                    var conn = new SqliteConnectionStringBuilder("Data Source = " + fileName + ";")
                    {
                        Mode = SqliteOpenMode.ReadWriteCreate,
                        Password = password
                    }.ToString();//使用这个方式设置密码，避免sql注入
                    dbConnection = new SqliteConnection(conn);
                    dbConnection.Open();
                    dbConnection.Dispose();
                    dbConnection = null;
                    return true;
                }
            } 
        }
        /*
        public static void Connect()
        {
            if(dbConnection == null)
            {
                dbConnection = new SqliteConnection("Data Source = ImageDatabase.sqlite;");
                dbConnection.Open();
                command = dbConnection.CreateCommand();
            }
        }
        */
        public static bool ConnectAccount(string userName, string password)
        {
            string fileName = userName + ".sqlite";
            if(!File.Exists(fileName))
            {
                return false;
            }
            else
            {
                try
                {
                    dbConnection = new SqliteConnection("Data Source = " + fileName + "; Password = " + password + ";");
                    dbConnection.Open();
                    var command = dbConnection.CreateCommand();
                    command = dbConnection.CreateCommand();
                    return true;
                }
                catch
                {
                    //用户名或密码错误
                    return false;
                }
            }
            
        }
        public static bool ChangePassword(string userName, string oldPassword, string newPassword)
        {
            string fileName = userName + ".sqlite";
            if (!File.Exists(fileName))
            {
                return false;
            }
            else
            {
                try
                {
                    dbConnection = new SqliteConnection("Data Source = " + fileName + "; Password = " + oldPassword + ";");
                    dbConnection.Open();
                    var command = dbConnection.CreateCommand();
                    //dbConnection.ChangePassword(newPassword);
                    command.CommandText = "SELECT quote($newPassword);";
                    command.Parameters.AddWithValue("$newPassword", newPassword);
                    var quotedNewPassword = (string)command.ExecuteScalar();

                    command.CommandText = "PRAGMA rekey = " + quotedNewPassword;
                    command.Parameters.Clear();
                    command.ExecuteNonQuery();

                    dbConnection.Dispose();
                    dbConnection = null;
                    return true;
                }
                catch
                {
                    return false;
                }
            } 
        }
        public static void Disconnect()
        {
            dbConnection.Close();
            dbConnection.Dispose();
            dbConnection = null;

        }
        public static void CreateTable()
        {
            var command = dbConnection.CreateCommand();
            command.CommandText = "CREATE TABLE IF NOT EXISTS imageInfoTab (name varchar(30), date varchar(20), device varchar(30), path varchar(200), detectionType varchar(20), result varchar(10))";
            command.ExecuteNonQuery();
        }

        public static void InsertToTable(string name, string date, string device, string path, string detectionType, string result)
        {
            var command = dbConnection.CreateCommand();
            command.CommandText = "INSERT INTO imageInfoTab VALUES(\"" + name + "\",\"" + date + "\",\"" + device + "\",\"" + path + "\",\"" + detectionType + "\",\"" + result + "\")";
            command.ExecuteNonQuery();
        }
        public static void InsertToTable(string name, string date, string device, string path)
        {
            var command = dbConnection.CreateCommand();
            command.CommandText = "INSERT INTO imageInfoTab (name, date, device, path) VALUES(\"" + name + "\",\"" + date + "\",\"" + device + "\",\"" + path + "\")";
            command.ExecuteNonQuery();
        }
        public static void DeleteFromTable(string path)
        {
            var command = dbConnection.CreateCommand();
            command.CommandText = "DELETE FROM imageInfoTab WHERE path" + "=\"" + path + "\"";
            command.ExecuteNonQuery();
        }
        public static System.Data.DataTable GetTable()
        {
            var command = dbConnection.CreateCommand();
            command.CommandText = "SELECT * FROM imageInfoTab";
            SqliteDataReader reader = command.ExecuteReader();
            DataTable dt = new DataTable();
            dt.Load(reader);
            return dt;
        }
    }
}

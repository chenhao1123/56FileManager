﻿using System.IO;
using System.Data.SQLite;
using System;

namespace FileManager
{
    public class SQLiteHelper
    {
        static string settingPath = Setting.settingFile;
        static string dbPath = Setting.dbFile;
        public static byte[] passwd;
        static public SQLiteConnection login;
        static public SQLiteConnection conn;
        public SQLiteHelper()
        {

        }

        public bool CreatNewDatabase()
        {
            if (File.Exists(dbPath))
            {
                return false;
            }
            else
            {
                SQLiteConnection.CreateFile(settingPath);
                Setting.HidFile(settingPath);
                SQLiteConnection.CreateFile(dbPath);
                Setting.HidFile(dbPath);
                conn = new SQLiteConnection("Data Source=" + dbPath + ";Version=3;");
                conn.Open();
                conn.ChangePassword(passwd);
                conn.Close();
                return true;
            }
        }
        ///   <summary>
        ///   连接到数据库
        ///   </summary>
        ///   <param   name="flag">0-settings/1-db</param>
        public void ConnectToDatabase(int flag)
        {
            if(flag == 0)
            {
                login = new SQLiteConnection("Data Source=" + settingPath + ";Version=3;");
                login.Open();
            }
            else if(flag == 1)
            {
                conn = new SQLiteConnection("Data Source=" + dbPath + ";Version=3;");
                conn.SetPassword(passwd);
                conn.Open();
            }
        }

        public void DatabaseInitial(int flag, string input = "")
        {

            if (flag == 0)
            {
                login = new SQLiteConnection("Data Source=" + settingPath + ";Version=3;");
                login.Open();
                string initial = "create table settings_table (id int, setting VARCHAR(20), string VARCHAR(100), number int)";
                SQLiteCommand commond = new SQLiteCommand(initial, login);
                commond.ExecuteNonQuery();
                string setpasswd = "insert into settings_table (id, setting, string) values (0, 'password', '" + input + "')";
                SQLiteCommand setpassedsc = new SQLiteCommand(setpasswd, login);
                setpassedsc.ExecuteNonQuery();
                login.Close();
            }
            else if (flag == 1)
            {
                conn = new SQLiteConnection("Data Source=" + dbPath + ";Version=3;");
                conn.SetPassword(passwd);
                conn.Open();

                string key_table_sql = "create table keys_table (filename VARCHAR(20), key VARCHAR(20))";
                SQLiteCommand ksql = new SQLiteCommand(key_table_sql, conn);
                ksql.ExecuteNonQuery();

                string logs_table_sql = "create table logs_table (filename VARCHAR(200), time VARCHAR(30), op VARCHAR(20))";
                SQLiteCommand lsql = new SQLiteCommand(logs_table_sql, conn);
                lsql.ExecuteNonQuery();

                string backupfile_table_sql = "create table backupfile_table (fullPath VARCHAR(200), lastWriteTime DATETIME)";
                SQLiteCommand bsql = new SQLiteCommand(backupfile_table_sql, conn);
                bsql.ExecuteNonQuery();

                conn.Close();
            }  

        }

        public bool CheckPasswd(string input)
        {
            ConnectToDatabase(0);
            string sql = "select * from settings_table where id = 0 ";
            SQLiteCommand cp = new SQLiteCommand(sql, login);
            SQLiteDataReader result = cp.ExecuteReader();
            if (result.StepCount == 1)
            {
                result.Read();
                if (result[2].ToString() == Setting.MD5Encrypt(input))
                {
                    passwd = System.Text.Encoding.UTF8.GetBytes(result[2].ToString());
                    return true;
                }
            }
            return false;
        }

        public void InsertSettings(int id, string setting, string str = "", int num = 0)
        {
            string insertset = "insert into settings_table (id, setting, string, number) values ("+ id +", '"+ setting +"', '" + str + "'," + num + ")";
            SQLiteCommand insertSet = new SQLiteCommand(insertset, login);
            insertSet.ExecuteNonQuery();
        }

        public void InsertKeys(string path, string key)
        {
            path = Path.GetFileName(path);
            string insertkey = "insert into keys_table (filename, key) values ('" + path + "', '" + key + "')";
            SQLiteCommand insertKey = new SQLiteCommand(insertkey, conn);
            insertKey.ExecuteNonQuery();
        }

        public void InsertLogs(string path, string time, string op)
        {
            string insertlog = "insert into logs_table (filename, time, op) values ('" + path + "','"+ time +"','" + op + "')";
            SQLiteCommand insertLog = new SQLiteCommand(insertlog, conn);
            insertLog.ExecuteNonQuery();
        }

        public string SelectKeys(string path)
        {
            path = Path.GetFileNameWithoutExtension(path);
            string selectkey = "select * from keys_table where filename = '" + path + "'";
            SQLiteCommand selectKey = new SQLiteCommand(selectkey, conn);
            SQLiteDataReader result = selectKey.ExecuteReader();
            if (result.StepCount == 1)
            {
                result.Read();
                return result[1].ToString();
            }
            else
            { 
                return null;
            }
                
        }


        public void CloseDatabase(int flag)
        {
            if (flag == 0)
            {
                login.Close();
            }
            else if (flag == 1)
            {
                conn.Close();
            }
        }




    }
}
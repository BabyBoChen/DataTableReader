using System;
using System.Data;
using Npgsql;
using System.Data.SQLite;
using System.Data.SqlClient;
using System.Data.Common;
using Newtonsoft.Json;
using System.IO;

namespace DataTableReader.DAL
{
    public class DbContext
    {
        private DbConnection conn = null;
        private DbProvider dbProvider;
        public DbContext(string connString, DbProvider provider)
        {
            this.dbProvider = provider;
            if(provider == DbProvider.PostgreSQL)
            {
                this.conn = new NpgsqlConnection(connString);
            }
            else if(provider == DbProvider.SQLite)
            {
                SQLiteConnectionStringBuilder builder = new SQLiteConnectionStringBuilder(connString);
                builder.FailIfMissing = true;
                this.conn = new SQLiteConnection(builder.ConnectionString);
            }
            else if(provider == DbProvider.SQLServer)
            {
                this.conn = new SqlConnection(connString);
            }
        }
        public void TryConnection()
        {
            using (conn)
            {
                try
                {
                    conn.Open();
                    if (conn.State != ConnectionState.Open)
                    {
                        throw new Exception("無法連線！原因不明！");
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        private DbDataAdapter getAdapter()
        {
            DbDataAdapter adapter = null;
            if (this.dbProvider == DbProvider.PostgreSQL)
            {
                adapter = new NpgsqlDataAdapter();
            }
            else if (this.dbProvider == DbProvider.SQLite)
            {
                adapter = new SQLiteDataAdapter();
            }
            else if (this.dbProvider == DbProvider.SQLServer)
            {
                adapter = new SqlDataAdapter();
            }
            else
            {
                throw new Exception("Couldn't find Data Adapter!");
            }
            return adapter;
        }
        public DataTable RunSql(string sql)
        {
            DataTable dt = new DataTable("DataTable");
            using (conn)
            {
                try
                {
                    var command = conn.CreateCommand();
                    command.CommandText = sql;
                    DbDataAdapter adapter = getAdapter();
                    adapter.SelectCommand = command;
                    adapter.Fill(dt);
                    adapter.Dispose();
                    command.Dispose();
                }
                catch (Exception)
                {
                    throw;
                }
            }
            return dt;
        }
        public static DataTable ReadXmlToDataTable(string filename)
        {
            DataTable dt = new DataTable();
            try
            {
                DataSet ds = new DataSet();
                ds.ReadXmlSchema(filename);
                if(ds.Tables.Count > 0)
                {
                    string dtName = ds.Tables[0].TableName;
                    dt.TableName = dtName;
                    dt.ReadXml(filename);
                    dt.TableName = "DataTable";
                }
                else
                {
                    throw new Exception("此XML檔沒有任何資料表！");
                }
            }
            catch (Exception)
            {
                throw;
            }
            return dt;
        }
        public static void ConvertToJson(DataTable dt, string filename)
        {
            try
            {
                string jsonString = JsonConvert.SerializeObject(dt);
                System.IO.File.WriteAllText(filename, jsonString);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }

    public enum DbProvider
    {
        PostgreSQL, 
        SQLite,
        SQLServer,
    }
}

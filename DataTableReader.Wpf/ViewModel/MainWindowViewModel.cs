using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using DataTableReader.DAL;

namespace DataTableReader.Wpf.ViewModel
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private List<string> dbProviders = Enum.GetNames(typeof(DbProvider)).ToList();
        public List<string> DbProviders 
        {
            get
            {
                return this.dbProviders;
            }
            set
            {
                if (value != this.dbProviders)
                {
                    this.dbProviders = value;
                    NotifyPropertyChanged();
                }
            }
        }
        private string selectDbProvider = Enum.GetNames(typeof(DbProvider)).ToList()[0];
        public string SelectedDbProvider 
        { 
            get
            {
                return this.selectDbProvider;
            }
            set
            {
                if (value != this.selectDbProvider)
                {
                    this.selectDbProvider = value;
                    NotifyPropertyChanged();
                }
            }
        }
        private string connString = "";
        public string ConnString 
        { 
            get
            {
                return this.connString;
            }
            set
            {
                if (value != this.connString)
                {
                    this.connString = value;
                    NotifyPropertyChanged();
                }
            }
        }
        private string sql = "SQL Script...";
        public string Sql 
        { 
            get
            {
                return this.sql;
            }
            set
            {
                if (value != this.sql)
                {
                    this.sql = value;
                    NotifyPropertyChanged();
                }
            }
        }
        private DbContext db;
        public DbContext Db 
        { 
            get
            {
                DbProvider provider = parseDbProvider(this.SelectedDbProvider);
                this.db = new DbContext(this.ConnString, provider);
                return this.db;
            }
        }
        private DataTable result = new DataTable();
        public DataTable Result
        {
            get
            {
                return this.result;
            }
            set
            {
                if (value != this.result)
                {
                    this.result = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public string ErrMsgTryConnect { get; set; } = "";
        public string ErrMsgRunSql { get; set; } = "";
        public string ErrMsgReadXml { get; set; } = "";
        public string ErrMsgWriteXml { get; set; } = "";
        public string ErrMsgWriteJSON { get; set; } = "";
        public Task TryConnection()
        {
            Task t = Task.Run(() => {
                this.ErrMsgTryConnect = "";
                string connString = this.ConnString;
                string provider_str = this.SelectedDbProvider;
                DbProvider provider = (DbProvider)Enum.Parse(typeof(DbProvider), provider_str);
                try
                {
                    this.Db.TryConnection();
                }
                catch (Exception ex)
                {
                    this.ErrMsgTryConnect = ex.Message;
                }
            });
            return t;
        }
        public Task<DataTable> RunSql()
        {
            Task<DataTable> t = Task.Run(() =>
            {
                this.ErrMsgRunSql = "";
                string sql = this.Sql;
                string connString = this.ConnString;
                string provider_str = this.SelectedDbProvider;
                DbProvider provider = (DbProvider)Enum.Parse(typeof(DbProvider), provider_str);
                DataTable dt = new DataTable();
                try
                {
                    dt = this.Db.RunSql(sql);
                }
                catch (Exception ex)
                {
                    this.ErrMsgRunSql = ex.Message;
                }
                return dt;
            });
            return t;
        }
        public Task<DataTable> ReadXmlToDataTable(string filename)
        {
            Task<DataTable> t = Task.Run(() => {
                this.ErrMsgReadXml = "";
                DataTable dt = new DataTable();
                try
                {
                    dt = DbContext.ReadXmlToDataTable(filename);
                }
                catch (Exception ex)
                {
                    this.ErrMsgReadXml = ex.Message;
                }
                return dt;
            });
            return t;
        }
        public Task WriteXml(string saveFilePath)
        {
            Task t = Task.Run(() => 
            {
                this.ErrMsgWriteXml = "";
                try
                {
                    this.Result.DefaultView.ToTable().WriteXml(saveFilePath, XmlWriteMode.WriteSchema);
                }
                catch (Exception ex)
                {
                    this.ErrMsgWriteXml = ex.Message;
                }
            });
            return t;
        }
        public Task WriteJSON(string saveFilePath)
        {
            Task t = Task.Run(() => 
            {
                this.ErrMsgWriteJSON = "";
                try
                {
                    DataTable dt = this.Result.DefaultView.ToTable();
                    DbContext.ConvertToJson(dt, saveFilePath);
                }
                catch (Exception ex)
                {
                    this.ErrMsgWriteJSON = ex.Message;
                }
            });
            return t;
        }
        private DbProvider parseDbProvider(string selectedItem)
        {
            object? provider = null;
            if(Enum.TryParse(typeof(DbProvider), selectedItem, out provider))
            {
                return (DbProvider)provider;
            }
            else
            {
                return 0;
            }
        }
        public string ShowExampleConnString()
        {
            string exampleConnString = "連接字串";
            if(!string.IsNullOrWhiteSpace(SelectedDbProvider))
            {
                DbProvider provider = this.parseDbProvider(SelectedDbProvider);
                if(provider == DbProvider.PostgreSQL)
                {
                    exampleConnString = "Server={hostname};Port={5432};User Id={username};Password={password};Database={database name};";
                }
                else if(provider == DbProvider.SQLite)
                {
                    exampleConnString = "Data Source={absolutePath.db};";
                }
                else if(provider == DbProvider.SQLServer)
                {
                    exampleConnString = "Data Source={hostname};Initial Catalog={database name};Persist Security Info=True;User ID={username};Password={password}";
                }
            }
            return exampleConnString;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using DataTableReader.DAL;
using DataTableReader.Wpf.ViewModel;
using Microsoft.Win32;

namespace DataTableReader.Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainWindowViewModel vm;
        public MainWindow()
        {
            InitializeComponent();
            vm = this.mainWindowViewModel;
        }
        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            this.btnConnect.IsEnabled = false;
            vm.TryConnection().ContinueWith((t) => 
            {
                t.Wait();
                if(string.IsNullOrWhiteSpace(vm.ErrMsgTryConnect))
                {
                    MessageBox.Show("連線成功！");
                }
                else
                {
                    MessageBox.Show(vm.ErrMsgTryConnect);
                }
                this.Dispatcher.Invoke(() => {
                    this.btnConnect.IsEnabled = true;
                });
            });
        }
        private void btnRun_Click(object sender, RoutedEventArgs e)
        {
            this.btnRun.IsEnabled = false;
            vm.RunSql().ContinueWith((t) =>
            {
                DataTable dt = t.Result;
                if(!string.IsNullOrWhiteSpace(vm.ErrMsgRunSql))
                {
                    MessageBox.Show(vm.ErrMsgRunSql);
                }
                this.Dispatcher.Invoke(() => {
                    this.vm.Result = dt;
                    this.btnRun.IsEnabled = true;
                });
            });
        }
        private void btnOpen_Click(object sender, RoutedEventArgs e)
        {
            this.btnOpen.IsEnabled = false;
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = false;
            openFileDialog.Filter = "XML files (*.xml)|*.xml|所有檔案 (*.*)|*.*";

            if (openFileDialog.ShowDialog() != true)
            {
                this.btnOpen.IsEnabled = true;
                return;
            }

            this.vm.ReadXmlToDataTable(openFileDialog.FileName).ContinueWith((t) => 
            {
                DataTable dt = t.Result;
                if(!string.IsNullOrWhiteSpace(this.vm.ErrMsgReadXml))
                {
                    MessageBox.Show(this.vm.ErrMsgReadXml);
                }
                this.Dispatcher.Invoke(() => {
                    this.vm.Result = dt;
                    this.btnOpen.IsEnabled = true;
                });
            });
        }
        private void btnWriteXML_Click(object sender, RoutedEventArgs e)
        {
            this.btnWriteXML.IsEnabled = false;
            DataTable dt = vm.Result;
            if(dt.Columns.Count == 0)
            {
                this.btnWriteXML.IsEnabled = true;
                return;
            }
            
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.DefaultExt = "xml";
            saveFileDialog.Filter = "XML files (*.xml)|*.xml|所有檔案 (*.*)|*.*";
            if (saveFileDialog.ShowDialog() != true)
            {
                this.btnWriteXML.IsEnabled = true;
                return;
            }

            vm.WriteXml(saveFileDialog.FileName).ContinueWith((t) => 
            {
                if(!string.IsNullOrWhiteSpace(vm.ErrMsgWriteXml))
                {
                    MessageBox.Show(vm.ErrMsgWriteXml);
                }
                this.Dispatcher.Invoke(() => {
                    this.btnWriteXML.IsEnabled = true;
                });
            });
        }
        private void btnWriteJSON_Click(object sender, RoutedEventArgs e)
        {
            this.btnWriteJSON.IsEnabled = false;
            DataTable dt = vm.Result;
            if (dt.Columns.Count == 0)
            {
                this.btnWriteJSON.IsEnabled = true;
                return;
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.DefaultExt = ".json";
            saveFileDialog.Filter = "JSON files (*.json)|*.json|所有檔案 (*.*)|*.*";
            if (saveFileDialog.ShowDialog() != true)
            {
                this.btnWriteJSON.IsEnabled = true;
                return;
            }

            vm.WriteJSON(saveFileDialog.FileName).ContinueWith((t) => 
            { 
                if(!string.IsNullOrWhiteSpace(vm.ErrMsgWriteJSON))
                {
                    MessageBox.Show(vm.ErrMsgWriteJSON);
                }
                this.Dispatcher.Invoke(() => {
                    this.btnWriteJSON.IsEnabled = true;
                });
            });
        }
        private void comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string exampleConnstring = mainWindowViewModel.ShowExampleConnString();
            mainWindowViewModel.ConnString = exampleConnstring;
        }
    }
}

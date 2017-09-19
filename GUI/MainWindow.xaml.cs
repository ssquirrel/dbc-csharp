using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using DbcLib.DBC.Model;
using DbcLib.DBC.Parser;

namespace GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private string from = "";
        private string to = "";
        private string status = "Ready";

        public string FromPath
        {
            get { return from; }
            set
            {
                from = value;

                OnPropertyChanged("FromPath");
            }
        }

        public string ToPath
        {
            get { return to; }
            set
            {
                to = value;

                OnPropertyChanged("ToPath");
            }
        }

        public string StatusText
        {
            get { return status; }
            set
            {
                status = value;

                OnPropertyChanged("StatusText");
            }
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        void Set_From_Path(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog
            {
                FileName = "",
                Filter = "Target Files (*.dbc, *.xlsx, *xls)|*.dbc;*.xlsx;*.xls|" +
                "CANdb Network (*.dbc)|*.dbc|" +
                "Excel Worksheets (*.xlsx)|*.xlsx;*.xls"
            };

            // Show save file dialog box
            bool? result = dlg.ShowDialog();

            // Process save file dialog box results
            if (result != true)
            {
                return;
            }

            // Save document
            string filename = dlg.FileName;
            FromPath = filename;

            if (FromPath.EndsWith(".dbc"))
                ToPath = FromPath.Substring(0, FromPath.Length - 4) + ".xlsx";
            else if (FromPath.EndsWith(".xlsx") || FromPath.EndsWith(".xls"))
                ToPath = FromPath.Substring(0, FromPath.Length - 5) + ".dbc";

            //throw new Exception();
        }

        async void Convert(object sender, RoutedEventArgs e)
        {
            DbcParser parser = new DbcParser(from);
            try
            {
                DBC dbc = await Task.Run(() => parser.Parse());

                StatusText = dbc.Messages[0].Name;
            }
            catch (ParseException ex)
            {
                StatusText = ex.Message;
            }
        }
    }
}

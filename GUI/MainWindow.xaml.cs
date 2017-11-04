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

using Microsoft.WindowsAPICodePack.Dialogs;

using DbcLib.Model;
using DbcLib.Excel.Parser;
using DbcLib.Excel.Writer;
using DbcLib.DBC.Parser;
using DbcLib.DBC.Writer;

namespace GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private bool running = false;

        private string from = "";
        private string to = "";
        private string status = "Ready";

        public MainWindow()
        {
            InitializeComponent();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public string FromPath
        {
            get
            {
                return from;
            }

            set
            {
                from = value;

                OnPropertyChanged("FromPath");
            }
        }

        public string ToPath
        {
            get
            {
                return to;
            }
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

        private Task<bool> ExcelToDBC()
        {
            running = true;

            return Task.Run(() =>
            {
                using (ExcelDBC d = ExcelParser.Parse(FromPath, "Message_Detail"))
                {
                    if (d.DBC != null)
                    {
                        using (var stream = new StreamWriter(ToPath, false, Encoding.Default))
                        using (DbcWriter writer = new DbcWriter(stream))
                        {
                            writer.Write(d.DBC);
                        }

                        return true;
                    }

                    return false;
                }
            });
        }

        private Task<bool> DbcToExcel()
        {
            running = true;

            return Task.Run(() =>
            {
                try
                {
                    DBC dbc = DbcParser.Parse(FromPath);

                    using (ExcelWriter writer = new ExcelWriter(ToPath))
                    {
                        writer.Add("Message_Detail", dbc);
                        writer.Write();
                    }
                }
                catch (DbcParseException)
                {
                    return false;
                }

                return true;
            });
        }

        private void HandleResult(bool result)
        {
            running = false;

            if (result)
                StatusText = "Success!";
            else
                StatusText = "Failure!";
        }

        private string FileChooser()
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
            return result == true ? dlg.FileName : null;
        }

        private void Set_From_Path(object sender, RoutedEventArgs e)
        {
            var path = FileChooser();

            if (path == null)
                return;

            FromPath = path;

            StatusText = "Ready";

            InputTextBox.ScrollToHorizontalOffset(99999);

            if (ToPath == "")
            {
                ToPath = Inverse(FromPath);

                OutputTextBox.ScrollToHorizontalOffset(99999);
            }
            else if (ToPath[ToPath.Length - 1] == '\\')
            {
                ToPath += Inverse(Path.GetFileName(FromPath));

                OutputTextBox.ScrollToHorizontalOffset(99999);
            }
        }

        private void Set_To_Path(object sender, RoutedEventArgs e)
        {
            var dialog = new CommonOpenFileDialog
            {
                IsFolderPicker = true
            };

            CommonFileDialogResult result = dialog.ShowDialog();
            if (result != CommonFileDialogResult.Ok)
                return;

            string complement = null;

            try
            {
                complement = Inverse(Path.GetFileName(FromPath));
            }
            catch (ArgumentException) { }

            if (complement != null)
            {
                ToPath = dialog.FileName + "\\" + complement;
            }
            else
            {
                ToPath = dialog.FileName + "\\";
            }

            StatusText = "Ready";

            OutputTextBox.ScrollToHorizontalOffset(99999);
        }



        private int ValidatePathExt()
        {
            if ((FromPath.EndsWith(".xlsx") || FromPath.EndsWith(".xls")) &&
                ToPath.EndsWith(".dbc"))
            {
                return 0;
            }

            if ((ToPath.EndsWith(".xlsx") || ToPath.EndsWith(".xls")) &&
                  FromPath.EndsWith(".dbc"))
            {
                return 1;
            }

            MessageBox.Show("Please specify files with correct ext", "Error");

            return -1;
        }

        private bool ValidatePath()
        {
            if (!File.Exists(FromPath))
            {
                MessageBox.Show("The Input file doesn't exist.", "Error");

                return false;
            }

            if (File.Exists(ToPath))
            {
                string text = "Output file already exists.\nDo you wish to override it?";

                var result = MessageBox.Show(text,
                    "Error",
                    MessageBoxButton.YesNoCancel);

                if (result != MessageBoxResult.Yes)
                    return false;
            }

            return true;
        }

        async private void Convert(object sender, RoutedEventArgs e)
        {
            if (running)
                return;

            int which = ValidatePathExt();

            if (which == -1 || !ValidatePath())
                return;

            try
            {
                if (which == 0)
                {
                    bool result = await ExcelToDBC();
                    HandleResult(result);
                }
                else if (which == 1)
                {
                    bool result = await DbcToExcel();
                    HandleResult(result);
                }
            }
            catch (UnauthorizedAccessException)
            {
                running = false;
                MessageBox.Show("Program doesn't have permission to write to the target location", "Error");
            }
            catch (IOException)
            {
                running = false;
                MessageBox.Show("An unkonwn IO exception occurred.", "Error");
            }

        }

        private static string Inverse(string path)
        {
            if (path.EndsWith(".dbc"))
            {
                return path.Substring(0, path.Length - 4) + ".xlsx";
            }

            if (path.EndsWith(".xls"))
            {
                return path.Substring(0, path.Length - 4) + ".dbc";
            }

            if (path.EndsWith(".xlsx"))
            {
                return path.Substring(0, path.Length - 5) + ".dbc";
            }

            return null;
        }
    }
}

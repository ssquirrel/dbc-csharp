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
    public partial class MainWindow : Window
    {
        private bool running = false;

        public MainWindow()
        {
            InitializeComponent();
        }

        private Task<bool> ExcelToDBC(string from, string to)
        {
            running = true;

            return Task.Run(() =>
            {
                using (ExcelDBC d = ExcelParser.Parse(from, "Message_Detail"))
                {
                    if (d.DBC == null)
                    {
                        return false;
                    }

                    using (var stream = new StreamWriter(to, false, Encoding.Default))
                    using (DbcWriter writer = new DbcWriter(stream))
                    {
                        writer.Write(d.DBC);
                    }

                    return true;
                }
            });
        }

        private Task<bool> DbcToExcel(string from, string to)
        {
            running = true;

            return Task.Run(() =>
            {
                try
                {
                    DBC dbc = DbcParser.Parse(from);

                    using (ExcelWriter writer = new ExcelWriter(to))
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
                StatusText.Text = "Success!";
            else
                StatusText.Text = "Failure!";
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

        private void Set_Input_Path(object sender, RoutedEventArgs e)
        {
            var path = FileChooser();

            if (path == null)
                return;

            InputTextBox.Text = path;
            StatusText.Text = "Ready";

            InputTextBox.ScrollToHorizontalOffset(99999);

            var output = OutputTextBox.Text;

            if (output == "")
            {
                OutputTextBox.Text = Inverse(path);

                OutputTextBox.ScrollToHorizontalOffset(99999);
            }
            else
            {
                string fn = Path.GetFileName(path);

                if (output == "")
                {
                    OutputTextBox.Text = output + Inverse(fn);
                    OutputTextBox.ScrollToHorizontalOffset(99999);
                }
                else
                {
                    string ofn = GetFileName(output);

                    if (ofn.EndsWith(".dbc") ||
                        ofn.EndsWith(".xlsx") ||
                        ofn.EndsWith(".xls"))
                    {
                        output = output.Substring(0, output.Length - ofn.Length);
                    }

                    OutputTextBox.Text = Path.Combine(output, Inverse(fn));
                    OutputTextBox.ScrollToHorizontalOffset(99999);
                }

            }
        }

        private void Set_Output_Path(object sender, RoutedEventArgs e)
        {
            var dialog = new CommonOpenFileDialog
            {
                IsFolderPicker = true
            };

            CommonFileDialogResult result = dialog.ShowDialog();
            if (result != CommonFileDialogResult.Ok)
                return;

            var file = Inverse(GetFileName(InputTextBox.Text));

            if (file != "")
            {
                OutputTextBox.Text = dialog.FileName + "\\" + file;
            }
            else
            {
                OutputTextBox.Text = dialog.FileName;
            }


            StatusText.Text = "Ready";

            OutputTextBox.ScrollToHorizontalOffset(99999);
        }

        private bool CheckFiles(string from, string to)
        {
            if (!File.Exists(from))
            {
                MessageBox.Show("The Input file doesn't exist.", "Error");
                return false;
            }

            if (File.Exists(to))
            {
                string text = "Output file already exists.\nDo you wish to override it?";

                var result = MessageBox.Show(text,
                    "Warning",
                    MessageBoxButton.YesNoCancel);

                if (result == MessageBoxResult.Yes)
                    return true;

                return false;
            }

            try
            {
                using (File.Create(to, 1, FileOptions.DeleteOnClose)) { }
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("Program doesn't have permission to write to the target location.", "Error");
                return false;
            }
            catch (ArgumentException)
            {
                MessageBox.Show("The path contains invalid characters ", "Error");
                return false;
            }
            catch (DirectoryNotFoundException)
            {
                MessageBox.Show("Output directory doesn't exist.", "Error");
                return false;
            }
            catch (PathTooLongException)
            {
                MessageBox.Show("Output path is too long", "Error");
                return false;
            }
            catch (NotSupportedException)
            {
                MessageBox.Show("Output path is illegal.", "Error");
                return false;
            }
            catch (IOException)
            {
                MessageBox.Show("An unkonwn IO exception occurred.", "Error");
                return false;
            }

            return true;
        }

        private int Validate(string from, string to)
        {
            from = from.ToLower();
            to = to.ToLower();

            if ((from.EndsWith(".xlsx") || from.EndsWith(".xls"))
                && to.EndsWith(".dbc"))
            {
                if (CheckFiles(from, to))
                    return 0;
            }
            else if (from.EndsWith(".dbc") &&
                (to.EndsWith(".xlsx") || to.EndsWith(".xls")))
            {
                if (CheckFiles(from, to))
                    return 1;
            }
            else
            {
                MessageBox.Show("Please specify files with correct ext", "Error");
            }

            return -1;
        }

        async private void Convert(object sender, RoutedEventArgs e)
        {
            if (running)
                return;

            string from = InputTextBox.Text;
            string to = OutputTextBox.Text;

            int which = Validate(from, to);

            if (which == -1)
                return;

            try
            {
                if (which == 0)
                {
                    bool result = await ExcelToDBC(from, to);
                    HandleResult(result);
                }
                else if (which == 1)
                {
                    bool result = await DbcToExcel(from, to);
                    HandleResult(result);
                }
            }
            catch (IOException)
            {
                running = false;
                MessageBox.Show("An unkonwn IO exception occurred.", "Error");
            }

        }

        private void Open_About(object sender, RoutedEventArgs e)
        {
            About about = new About();
            about.ShowDialog();
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

            return "";
        }

        private static string GetFileName(string path)
        {
            int ps = path.LastIndexOf(@"\");

            if (ps == -1)
                return path;

            return path.Substring(ps + 1);
        }
    }
}

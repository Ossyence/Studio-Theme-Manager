using Studio_Theme_Manager.Libraries;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Studio_Theme_Manager {
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Console : Window {
        ConsoleContent dc = new ConsoleContent();

        private string GetStartupString() {
            return "BOOTED Studio-Theme-Manager (" + GeneralLibrary.GetVersion() + ") DEV CONSOLE (type \"help\" to get commands)\n";
        }

        public Console() {
            dc.console = this;

            InitializeComponent();
            DataContext = dc;
            Loaded += MainWindow_Loaded;

            BypassWriteLine(GetStartupString());

            KeyDown += new KeyEventHandler(delegate {
                if (GeneralLibrary.KeystrokeDown("clearconsole")) {
                    ConsoleLibrary.ClearConsole();
                }
            });
        }

        public void BypassWriteLine(string content) {
            dc.BypassWriteLine(content);
        }

        public void WriteLine(string content) {
            dc.WriteLine(content);
        }

        public void Write(string content, int readIndex = -1) {
            dc.Write(content, readIndex);
        }

        public void Clear() {
            dc.Clear();

            BypassWriteLine(GetStartupString());
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e) {
            InputBlock.KeyDown += InputBlock_KeyDown;
            InputBlock.Focus();
        }

        void InputBlock_KeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                dc.ConsoleInput = InputBlock.Text;
                dc.RunCommand();
                InputBlock.Focus();
                Scroller.ScrollToBottom();
            }
        }

        private void topbarContainer_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            DragMove();
        }

        private void resizer_DragDelta(object sender, DragDeltaEventArgs e) {
            Width = Math.Max((Width + e.HorizontalChange), 435);
            Height = Math.Max((Height + e.VerticalChange), 80);
        }

        private void minimize_button_Click(object sender, RoutedEventArgs e) {
            WindowState = WindowState.Minimized;
        }
    }

    public class ConsoleContent : INotifyPropertyChanged {
        public string consoleInput = string.Empty;
        public Console console;

        ObservableCollection<string> consoleOutput = new ObservableCollection<string>() { };

        public string ConsoleInput {
            get {
                return consoleInput;
            }
            set {
                consoleInput = value;
                OnPropertyChanged("ConsoleInput");
            }
        }

        public ObservableCollection<string> ConsoleOutput {
            get {
                return consoleOutput;
            }
            set {
                consoleOutput = value;
                OnPropertyChanged("ConsoleOutput");
            }
        }

        public void BypassWriteLine(string content) {
            console.Dispatcher.BeginInvoke(new Action(() => consoleOutput.Add(content)));
        }

        public void WriteLine(string content) {
            console.Dispatcher.BeginInvoke(new Action(() => consoleOutput.Add(content)));
        }

        public void Write(string content, int readIndex = -1) {
            if (readIndex == -1) {
                readIndex = ConsoleOutput.Count - 1;
            }

            console.Dispatcher.BeginInvoke(new Action(() => consoleOutput[readIndex] = consoleOutput[readIndex] + content));
        }

        public void Clear() {
            console.Dispatcher.BeginInvoke(new Action(() => consoleOutput.Clear()));
        }

        public void RunCommand() {
            ConsoleLibrary.WriteLine(ConsoleInput, false, false);

            if (ConsoleInput != string.Empty) {
                string[] split = ConsoleInput.Split(' ');

                if (split.Length > 0) {
                    string command = split[0].ToLower();

                    List<string> arguements = new List<string>(split);
                    arguements.RemoveAt(0);

                    bool success = false;

                    foreach (KeyValuePair<string, Func<List<string>, (bool errored, bool newLine, string message)>> delegateInfo in ConsoleLibrary.Commands.Delegates) {
                        if (delegateInfo.Key == command) {
                            var data = delegateInfo.Value(arguements);

                            if (data.message != null) {
                                if (data.errored) {
                                    data.message = "[ERRORED] " + data.message;
                                }

                                if (data.newLine) {
                                    ConsoleLibrary.WriteLine(data.message + "\n");
                                } else {
                                    ConsoleLibrary.Write(data.message);
                                }
                            }

                            success = true;

                            break;
                        }
                    }

                    if (!success) {
                        ConsoleLibrary.WriteLine("Unknown command \"" + command + "\", type \"help\" to get a list" + "\n", false, false);
                    }
                }
            }

            ConsoleInput = string.Empty;
        }


        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged(string propertyName) {
            if (null != PropertyChanged)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

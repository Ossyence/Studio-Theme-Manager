using Newtonsoft.Json.Linq;
using Studio_Theme_Manager.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Studio_Theme_Manager.Libraries;
using System.Xml.Linq;
using System.Windows;
using System.Windows.Threading;

namespace Studio_Theme_Manager.Libraries {
    public static class ConsoleLibrary {
        private static Console console;
        private static string logs = "";
        
        public static bool IsConsoleOpen() {
            return console != null;
        }

        public static bool SettingsAllow() {
            return ((string)FileLibrary.GetSetting("console-print")).StringToBool();
        }

        public static void ClearConsole() {
            logs = "";

            if (IsConsoleOpen()) {
                console.Clear();
            }
        }

        public static void StartConsole(bool override_ = false, bool freshStart = false) {
            if (!SettingsAllow() && override_ == false) return;

            if (!IsConsoleOpen()) {
                console = new Console();
                console.Show();

                console.close_button.Click += delegate { CloseConsole(); };

                if (!freshStart) {
                    BypassWriteLine(logs);
                }
            }
        }

        public static void CloseConsole() {
            console.Close();
            console = null;
        }

        public static void BypassWriteLine(string content, bool override_ = false) {
            if (IsConsoleOpen()) {
                console.Dispatcher.BeginInvoke(new Action(() => console.BypassWriteLine(content)));
            }
        }

        public static void WriteLine(string content, bool override_ = false, bool system = true) {
            if (system) {
                content = "[SYSTEM] " + content;
            }

            logs += content + "\n";

            if (IsConsoleOpen()) {
                console.Dispatcher.BeginInvoke(new Action(() => console.WriteLine(content)));
            }
        }

        public static void Write(string content, bool override_ = false) {
            logs += content;

            if (IsConsoleOpen()) {
                console.Dispatcher.BeginInvoke(new Action(() => console.Write(content)));
            }
        }

        public class Commands {
            public static Dictionary<string, string[]> CommandInfo = new Dictionary<string, string[]>()
            {
                { "help",           new string[] { "help",                          "returns all commands avaliable" } },
                { "describe",       new string[] { "describe [command name]",       "returns the description of the command" } },
                { "version",        new string[] { "version",                       "returns the current version of the app" } },
                { "shutdown",       new string[] { "shutdown",                      "completely cancells the app process" } },
                { "clear",          new string[] { "clear",                         "clears the console and its logs" } },
                { "settings",       new string[] { "settings",                      "returns every setting" } },
                { "getsetting",     new string[] { "getsetting [setting]",          "returns the value of a setting" } },
                { "setsetting" ,    new string[] { "setsetting [setting] [value]",  "sets the value of a setting" } },
                { "themecount",     new string[] { "themecount",                    "returns the number of themes installed" } },
                { "themelist",      new string[] { "themelist",                     "returns a list of names of themes installed" } },
                { "themeinfo",      new string[] { "themeinfo [name]",              "returns the themeinfo JSON of the name shown" } },
                { "themedata",      new string[] { "themedata [name]",              "returns the entirety of the theme JSON of the name shown" } },
            };

            public static Dictionary<string, Func<List<string>, (bool errored, bool newLine, string message)>> Delegates = new Dictionary<string, Func<List<string>, (bool errored, bool newLine, string message)>>()
            {
                {"help", delegate (List<string> arguements) {
                    string compiledString = "here is a list of all commands: \n";

                    foreach (KeyValuePair<string, string[]> pair in CommandInfo) {
                        compiledString += pair.Value[0] + " - " +  pair.Value[1] + "\n";
                    }

                    return (errored: false, newLine: true, message: compiledString);
                } },

                { "describe", delegate (List<string> arguements) {
                    if (arguements.Count > 0) {
                        if (CommandInfo.ContainsKey(arguements[0])) {
                            return (errored: false, newLine: true, message: $"{CommandInfo[arguements[0]][0]}\n{CommandInfo[arguements[0]][1]}");
                        } else {
                            return (errored: true, newLine: true, message: $"command \"{arguements[0]}\" does not exist");
                        }
                    } else {
                        return (errored: true, newLine: true, message: $"tell me the command pookie");
                    }
                } },

                {"version", delegate (List<string> arguements) {
                    return (errored: false, newLine: true, message: "App-version: " + GeneralLibrary.GetVersion());
                } },

                {"shutdown", delegate (List<string> arguements) {
                    Application.Current.Shutdown();

                    return (errored: false, newLine: true, message: null);
                } },

                { "clear", delegate (List<string> arguements) {
                    ClearConsole();

                    return (errored: false, newLine: true, message: $"Cleared console");
                } },

                {"settings", delegate (List<string> arguements) {
                    return (errored : false, newLine : true, message : $"Settings: \n{FileLibrary.Settings()}");
                } },

                {"getsetting", delegate (List<string> arguements) {
                    if (arguements.Count > 0) {
                        if (FileLibrary.Settings().ContainsKey(arguements[0])) {
                            return (errored : false, newLine : true, message : $"{arguements[0]} = {FileLibrary.GetSetting(arguements[0])}");
                        } else {
                            return (errored : true, newLine : true, message : $"\"{arguements[0]}\" is not a valid setting");
                        }
                    }

                    return (errored : true, newLine : true, message : $"insufficient arguements");
                } },

                {"setsetting", delegate (List<string> arguements) {
                    if (arguements.Count > 1) {
                        if (FileLibrary.Settings().ContainsKey(arguements[0])) {
                            FileLibrary.SetSetting(arguements[0], arguements[1]);

                            return (errored : false, newLine : true, message : $"successfully set setting \"{arguements[0]}\" to \"{arguements[1]}\"");
                        } else {
                            return (errored : true, newLine : true, message : $"\"{arguements[0]}\" is not a valid setting");
                        }
                    }

                    return (errored : true, newLine : true, message : $"insufficient arguements");
                } },

                {"themecount", delegate (List<string> arguements) {
                    DirectoryInfo themes = new DirectoryInfo(Path.Combine(Environment.CurrentDirectory, "InstalledThemes"));

                    return (errored : false, newLine : true, message : "total themes installed: " + themes.GetDirectories().Length.ToString());
                } },

                {"themelist", delegate (List<string> arguements) {
                    DirectoryInfo themes = new DirectoryInfo(Path.Combine(Environment.CurrentDirectory, "InstalledThemes"));

                    string names = "heres a list of all themes installed:\n";

                    foreach (DirectoryInfo dir in themes.GetDirectories()) {
                        names += $"{dir.Name}\n";
                    }

                    return (errored : false, newLine : true, message : names);
                } },

                {"themeinfo", delegate (List<string> arguements) {
                    if (arguements.Count > 0) {
                        string combinedPath = Path.Combine(Environment.CurrentDirectory, "InstalledThemes", arguements[0]);

                        if (Directory.Exists(combinedPath)) {
                            string filePath = Path.Combine(combinedPath, "ThemeInfo.json");

                            if (File.Exists(filePath)) {
                                return (errored : false, newLine : true, message : $"Theme info \"{arguements[0]}\"'s JSON is:\n{File.ReadAllText(filePath)}");
                            } else {
                                return (errored : true, newLine : true, message : $"theme \"{arguements[0]}/ThemeInfo.json\" does not exist");
                            }
                        } else {
                            return (errored : true, newLine : true, message : $"theme \"{arguements[0]}\" does not exist as a folder");
                        }
                    }

                    return (errored : true, newLine : true, message : $"please tell the name you idiot");
                } },

                {"themedata", delegate (List<string> arguements) {
                    if (arguements.Count > 0) {
                        string combinedPath = Path.Combine(Environment.CurrentDirectory, "InstalledThemes", arguements[0]);

                        if (Directory.Exists(combinedPath)) {
                            string filePath = Path.Combine(combinedPath, "Theme.json");

                            if (File.Exists(filePath)) {
                                return (errored : false, newLine : true, message : $"Theme data \"{arguements[0]}\"'s JSON is:\n{File.ReadAllText(filePath)}");
                            } else {
                                return (errored : true, newLine : true, message : $"theme data \"{arguements[0]}/Theme.json\" does not exist");
                            }
                        } else {
                            return (errored : true, newLine : true, message : $"theme data \"{arguements[0]}\" does not exist as a folder");
                        }
                    }

                    return (errored : true, newLine : true, message : $"please tell the name you idiot");
                } },
            };
        }
    }
}

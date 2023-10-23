using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Studio_Theme_Manager.Libraries
{
    public class GeneralLibrary {
        public enum ErrorLevel {
            LOSS_OF_FUNCTIONALITY,
            CRASH_INDUCING,
            UI_DEPENDANCY,
            MINOR,
            UNKNOWN
        }

        public static Dictionary<string, Key[]> Keystrokes = new Dictionary<string, Key[]>() { };

        public static void RegisterKeystroke(string name, Key[] keys) { Keystrokes.Add(name, keys); }

        public static bool ProcessOpen(string name)
        {
            return Process.GetProcessesByName(name).Length > 0;
        }

        public static string GetVersion() {
            return FileLibrary.GetSetting("version-number").ToString() + "-" + FileLibrary.GetSetting("version-stage").ToString();
        }

        public static string ResourcesPath(string basicPath) {
            return "pack://application:,,,/" + basicPath;
        }

        public static string KeystrokeToString(string keystrokeName) {
            string finalized = "";

            foreach (Key key in Keystrokes[keystrokeName]) {
                finalized += key.ToString() + "+";
            }

            finalized = finalized.Remove(finalized.Length - 1, 1);

            return finalized;
        }

        public static bool KeystrokeDown(string keystrokeName) {
            foreach (Key key in Keystrokes[keystrokeName]) {
                if (!Keyboard.IsKeyDown(key)) {
                    return false;
                }
            }

            return true;
        }

        public static bool AreKeysDown(Key[] keys) {
            foreach (Key key in keys) {
                if (!Keyboard.IsKeyDown(key)) {
                    return false;
                }
            }

            return true;
        }

        public static void ShowError(string content, ErrorLevel level, bool halt) {
            string message = content;
            string caption = "HALTED: " + halt.ToString() + " | UI ERROR: " + level;

            ConsoleLibrary.WriteLine($"[ERROR] {message}\n{caption}");

            MessageBoxImage errorIcon = MessageBoxImage.Exclamation;

            switch (level) {
                case ErrorLevel.LOSS_OF_FUNCTIONALITY:
                    errorIcon = MessageBoxImage.Error;
                    break;
                case ErrorLevel.MINOR:
                    errorIcon = MessageBoxImage.Warning;
                    break;
                case ErrorLevel.UI_DEPENDANCY:
                    errorIcon = MessageBoxImage.Exclamation;
                    break;
                case ErrorLevel.CRASH_INDUCING:
                    errorIcon = MessageBoxImage.Exclamation;
                    break;
                case ErrorLevel.UNKNOWN:
                    errorIcon = MessageBoxImage.Question;
                    break;
                default:
                    errorIcon = MessageBoxImage.Information;
                    break;
            }

            if (halt) {
                MessageBox.Show(message, caption, MessageBoxButton.OK, errorIcon);
            }
            else {
                Task.Run(new Action(delegate {
                    MessageBox.Show(message, caption, MessageBoxButton.OK, errorIcon);
                }));
            }
        }
    }

   
}

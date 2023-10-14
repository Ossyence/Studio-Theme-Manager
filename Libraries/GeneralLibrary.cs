using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;
using System.Threading;

namespace Studio_Theme_Manager.Libraries {
    public class GeneralLibrary {
        public enum ErrorLevel {
            LOSS_OF_FUNCTIONALITY,
            CRASH_INDUCING,
            UI_DEPENDANCY,
            MINOR,
            UNKNOWN
        }

        public static string ConvertBasicPathToResources(string basicPath) {
            return "pack://application:,,,/" + basicPath;
        }

        public static void ShowError(string content, ErrorLevel level, bool halt) {
            string message = content;
            string caption = "HALTED: " + halt.ToString() + " | UI ERROR: " + level;

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

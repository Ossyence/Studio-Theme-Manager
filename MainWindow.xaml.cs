using System;
using System.Collections.Generic;
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
using System.Numerics;
using System.Windows.Threading;
using Studio_Theme_Manager.Libraries;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Windows.Media.Animation;
using System.Windows.Controls.Primitives;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Xml.Linq;

namespace Studio_Theme_Manager {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public enum ThemeType {
            Light = 1,
            Dark = 2,
            UNKNOWN = 100000000
        }

        public UILibrary LibraryInstance = new UILibrary();
        public Array[] categoryData = new Array[] { };

        public ImageBrush unavaliableBrush = new ImageBrush();

        private string automaticId = "gaminggamerstonisautomaticthisisuneccessarilylonghehe:money_mouth::slots:";

        public string myPath = System.Reflection.Assembly.GetExecutingAssembly().Location;

        public string componentsPath = Path.Combine(Environment.CurrentDirectory, "ApplicationData");
        public string themesPath = Path.Combine(Environment.CurrentDirectory, "InstalledThemes");

        public string dataPath = Path.Combine(Path.Combine(Environment.CurrentDirectory, "ApplicationData"), "Data.json");

        public JObject themeStructure = JObject.Parse(File.ReadAllText(Path.Combine(Path.Combine(Environment.CurrentDirectory, "ApplicationData"), "Structure.json")));
        public JObject themeInfoStructure = JObject.Parse(File.ReadAllText(Path.Combine(Path.Combine(Environment.CurrentDirectory, "ApplicationData"), "StructureCounterpart.json")));

        public string versionsFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Roblox", "Versions");

        public string shortcutName = "Roblox Studio Patched";

        public string patchedExecutableName = "RobloxStudioPatched.exe";
        public string studioExecutableName = "RobloxStudioBeta.exe";

        public (string displayName, string jsonPath, string infoPath, string directoryPath, ThemeType themeType, bool favourited, JObject jsonParsed, JObject infoParsed) GetThemeData(string name) {
            string[] directories = Directory.GetDirectories(themesPath);

            foreach (string directoryPath in directories) {
                DirectoryInfo info = new DirectoryInfo(directoryPath);
                
                if (info.Name == name) {
                    string jsonPath = Path.Combine(directoryPath, "Theme.json");
                    string infoPath = Path.Combine(directoryPath, "ThemeInfo.json");

                    JObject jsonParsed = JObject.Parse(File.ReadAllText(jsonPath));
                    JObject infoParsed = JObject.Parse(File.ReadAllText(infoPath));
                    
                    bool isFavourite = false;
                    string displayName = (string)infoParsed["name"];
                    ThemeType themeType = (ThemeType)(int)infoParsed["theme-type"];

                    foreach (JToken favourite in JObject.Parse(File.ReadAllText(dataPath))["favourited"]) {
                        if (favourite.ToString() == directoryPath) {
                            isFavourite = true;
                            break;
                        }
                    }

                    return (
                        displayName: displayName,

                        jsonPath: jsonPath,
                        infoPath: infoPath,
                        directoryPath: directoryPath,

                        themeType: themeType,
                        favourited: isFavourite, 
                        
                        jsonParsed: jsonParsed,
                        infoParsed: infoParsed
                    );
                }
            }

            GeneralLibrary.ShowError("FATAL ERROR, THEME DIRECTORY \"" + name + "\" DOES NOT EXIST, CLOSING.", GeneralLibrary.ErrorLevel.CRASH_INDUCING, true);
            
            Application.Current.Shutdown();

            return (
                displayName: "UNKNOWN",

                jsonPath: null,
                infoPath: null,
                directoryPath: null,

                themeType: ThemeType.Dark,
                favourited: false, 

                jsonParsed: null, 
                infoParsed: null
            );
        }

        public object[] GetEveryThemeJSONData(bool sort = true, ThemeType keep = ThemeType.UNKNOWN) {
            string[] directories = Directory.GetDirectories(themesPath);
            List<object[]> themes = new List<object[]>();

            foreach (string directoryPath in directories) {
                DirectoryInfo info = new DirectoryInfo(directoryPath);

                var data = GetThemeData(info.Name);
                object[] objectified = new object[] { data.displayName, data.jsonPath, data.infoPath, data.directoryPath, data.themeType, data.favourited, data.jsonParsed, data.infoParsed };

                if (keep != ThemeType.UNKNOWN) {
                    if (data.themeType == keep) {
                        themes.Add(objectified);
                    }
                }
                else {
                    themes.Add(objectified);
                }
            }

            if (sort) {
                int favouriteSort(object[] x, object[] y) {
                    bool xfav = (bool)x[5];
                    bool yfav = (bool)y[5];

                    if (xfav == true && yfav == false) {
                        return 1;
                    }
                    else if (xfav == true && yfav == true) {
                        return 0;
                    }
                    else {
                        return -1;
                    }
                }

                themes.Sort(favouriteSort);
            }

            return themes.ToArray();
        }

        public void CreateShortcut(string name, string createAt, string executableDirectory, string executableName, string iconPath) {
            Type t = Type.GetTypeFromCLSID(new Guid("72C24DD5-D70A-438B-8A42-98424B88AFB8")); //Windows Script Host Shell Object
            dynamic shell = Activator.CreateInstance(t);

            try {
                var lnk = shell.CreateShortcut(Path.Combine(createAt, name + ".lnk"));
                try {
                    lnk.TargetPath = Path.Combine(executableDirectory, executableName);
                    lnk.IconLocation = iconPath;
                    lnk.WorkingDirectory = executableDirectory;
                    lnk.Save();
                }
                finally {
                    Marshal.FinalReleaseComObject(lnk);
                }
            }
            finally {
                Marshal.FinalReleaseComObject(shell);
            }
        }

        public void DeleteDirectory(string path) {
            if (!Directory.Exists(path)) { return; }

            System.IO.DirectoryInfo directory = new System.IO.DirectoryInfo(path);

            foreach (System.IO.FileInfo file in directory.GetFiles()) {
                file.Delete();
            }

            foreach (System.IO.DirectoryInfo subDirectory in directory.GetDirectories()) {
                subDirectory.Delete(true);
            }

            directory.Delete();
        }

        public void SwitchCategory(Canvas canvas) {
            foreach (object[] categoryData in categoryData) {
                Canvas assosciatedCanvas = (Canvas)categoryData[0];
                Button assosciatedButton = (Button)categoryData[1];

                if (canvas == assosciatedCanvas) {
                    assosciatedCanvas.Visibility = Visibility.Visible;
                    assosciatedButton.OpacityMask = null;
                } else {
                    assosciatedCanvas.Visibility = Visibility.Collapsed;
                    assosciatedButton.OpacityMask = unavaliableBrush;
                }
            }
        }

        public void SetWorkingText(string caption, string content) {
            LibraryInstance.RunOnUIThread(new Action(delegate {
                working_title.Content = caption;
                working_progress.Text = content;
            }));
        }

        public void BeginWorking(string caption, string content) {
            LibraryInstance.RunOnUIThread(new Action(delegate {
                SetWorkingText(caption, content);

                LibraryInstance.PlayAnimation("WorkingFrameAppear");
            }));
        }

        public void StopWorking() {
            LibraryInstance.RunOnUIThread(new Action(delegate {
                LibraryInstance.PlayAnimation("WorkingFrameDissapear");
            }));
        }

        public MainWindow() {
            LibraryInstance.caller = this;

            InitializeComponent();
            
            unavaliableBrush.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Images/unavaliable-opacity-mask.png"));
        
            categoryData = new Array[] {
                new object[] { editor_frame, editor_category_button },
                new object[] { settings_frame, settings_category_button },
                new object[] { patcher_frame, patcher_category_button },
                new object[] { themes_frame, themes_category_button },
            };

            SwitchCategory(patcher_frame);
        }

        // TOPBAR BUTTONS
        private void closeButton_Click(object sender, RoutedEventArgs e) { Application.Current.Shutdown(); }
        private void minimizeButton_Click(object sender, RoutedEventArgs e) { WindowState = WindowState.Minimized; }

        private void topbarContainer_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) { DragMove(); }

        // CATEGORY BUTTONS \\
        private void patcher_category_button_Click(object sender, RoutedEventArgs e) { SwitchCategory(patcher_frame); }
        private void themes_category_button_Click(object sender, RoutedEventArgs e) { SwitchCategory(themes_frame); }
        private void editor_category_button_Click(object sender, RoutedEventArgs e) { SwitchCategory(editor_frame); }
        private void settings_category_button_Click(object sender, RoutedEventArgs e) { SwitchCategory(settings_frame); }

        // CHECKBOX INITIALIZING AND CLICKING \\
        private void CheckboxClick(object sender, RoutedEventArgs e) { LibraryInstance.SetCheckboxState((Button)sender, true); }
        private void CheckboxInitialized(object sender, EventArgs e) { LibraryInstance.InitializeCheckbox(sender); }

        // DROPDOWN INITIALISING AND CLICK SETUP \\
        private void light_override_dropdown_Loaded(object sender, RoutedEventArgs e) { LibraryInstance.InitializeDropdown(sender, new object[] { GetThemeData("DefaultLight").displayName, GetThemeData("DefaultLight").jsonPath }); }
        private void dark_override_dropdown_Loaded(object sender, RoutedEventArgs e) { LibraryInstance.InitializeDropdown(sender, new object[] { GetThemeData("DefaultDark").displayName, GetThemeData("DefaultDark").jsonPath }); }
        private void studio_instance_dropdown_Loaded(object sender, RoutedEventArgs e) { LibraryInstance.InitializeDropdown(sender, new object[] { "Automatic", automaticId }); }

        private void studio_instance_dropdown_Click(object sender, RoutedEventArgs e) {
            List<object[]> list = new List<object[]> {
                new object[] { "Automatic", automaticId },
            };

            string[] versionsDirectory = Directory.GetDirectories(versionsFolderPath);
            
            foreach (string version in versionsDirectory) {
                DirectoryInfo info = new DirectoryInfo(version);
                
                foreach (FileInfo file in info.GetFiles()) {
                    if (file.Name == studioExecutableName) {
                        list.Add(new object[] { info.Name, info.FullName });

                        break;
                    } 
                }
            }

            LibraryInstance.CreateDropdown(sender as Button, GeneralLibrary.ConvertBasicPathToResources("Images/studio.png"), list.ToArray());
        }

        private void dark_override_dropdown_Click(object sender, RoutedEventArgs e) {
            LibraryInstance.CreateDropdown(sender as Button, GeneralLibrary.ConvertBasicPathToResources("Images/colorpick.png"), GetEveryThemeJSONData(true, ThemeType.Dark));
        }

        private void light_override_dropdown_Click(object sender, RoutedEventArgs e) {
            LibraryInstance.CreateDropdown(sender as Button, GeneralLibrary.ConvertBasicPathToResources("Images/colorpick.png"), GetEveryThemeJSONData(true, ThemeType.Light));
        }

        // MAIN BUTTONS
        private void run_patch_button_Click(object sender, RoutedEventArgs e) {
            BeginWorking("INTIALIZING", "Finding theme JSONs and studio location...");
            
            string desktopDirectory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            string shortcutPath = Path.Combine(desktopDirectory, shortcutName);

            string[] versionsDirectory = Directory.GetDirectories(versionsFolderPath);
            string targetDir = (string)LibraryInstance.GetDropdownValue(studio_instance_dropdown as Button);

            if (targetDir == automaticId) {
                targetDir = versionsDirectory.FirstOrDefault(dir => File.Exists(Path.Combine(dir, studioExecutableName)));
            }

            string platformPath = Path.Combine(targetDir, "Platform");
            string fullPlatformPath = Path.Combine(targetDir, "Platform", "Base", "QtUI", "themes");

            string patchedPath = Path.Combine(targetDir, patchedExecutableName);

            Task.Run(new Action(delegate {
                Thread.Sleep(1000);

                SetWorkingText("CLEARING", "Deleting old \"Platforms\" data and patched executable...");

                DeleteDirectory(platformPath);

                if (File.Exists(patchedPath)) {
                    File.Delete(patchedPath);
                }

                Thread.Sleep(500);

                SetWorkingText("ADDING DATA", "Creating \"Platforms\" directory inside studio...");

                Directory.CreateDirectory(fullPlatformPath);

                Thread.Sleep(500);

                SetWorkingText("ADDING DATA", "Copying JSONs into the \"Platforms\" directory...");
                
                void CopyJSONOver(string jsonPath, string overwriteName) {
                    File.WriteAllText(Path.Combine(fullPlatformPath, overwriteName + ".json"), File.ReadAllText(jsonPath));
                }

                CopyJSONOver((string)LibraryInstance.GetDropdownValue((Button)dark_override_dropdown), "DarkTheme");
                CopyJSONOver((string)LibraryInstance.GetDropdownValue((Button)light_override_dropdown), "LightTheme");

                Thread.Sleep(500);

                SetWorkingText("PATCHING", "Running patch on studio...");

                byte[] patchedBytes = File.ReadAllBytes(Path.Combine(targetDir, studioExecutableName));

                for (int i = 0; i <= patchedBytes.Length - 1; i++) {
                    if (patchedBytes[i] == 0x3A) {
                        if (patchedBytes[i + 1] == 0x2F) {
                            if (patchedBytes[i + 2] == 0x50) {
                                if (patchedBytes[i + 3] == 0x6C) {
                                    patchedBytes[i] = 0x2E;
                                    patchedBytes[i + 1] = 0x2F;
                                    patchedBytes[i + 2] = 0x50;
                                    patchedBytes[i + 3] = 0x6C;
                                }
                            }
                        }
                    }
                }

                File.WriteAllBytes(patchedPath, patchedBytes);

                Thread.Sleep(500);

                if (LibraryInstance.GetCheckboxState(create_shortcut_checkbox as Button) == true) {
                    SetWorkingText("FINALIZING", "Creating shortcut on Desktop...");

                    CreateShortcut(shortcutName, desktopDirectory, targetDir, patchedExecutableName, myPath);
                } else {
                    SetWorkingText("FINALIZING", "Opening patched executables path in explorer...");

                    Process.Start("explorer.exe", "/select, \"" + patchedPath + "\"");
                }

                Thread.Sleep(500);

                if (LibraryInstance.GetCheckboxState(create_shortcut_checkbox as Button) == true) {
                    SetWorkingText("FINALIZING", "Opening your patched studio...");

                    var startInfo = new ProcessStartInfo();
                    startInfo.WorkingDirectory = targetDir;
                    startInfo.FileName = patchedPath;

                    Process.Start(startInfo);

                    Thread.Sleep(500);
                }

                SetWorkingText("DONE", "Completely finished all operations!");

                Thread.Sleep(1000);

                StopWorking();
            }));
        }
    }
}

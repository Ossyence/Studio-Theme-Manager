using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Windows.Media.Animation;
using System.Collections.ObjectModel;
using Microsoft.Win32;

using Newtonsoft.Json.Linq;
using Studio_Theme_Manager.Libraries;
using System.Xml.Linq;

namespace Studio_Theme_Manager { 
    // TODO: MAKE THEME DROPDOWNS IN PATCHER CATEGORY NOT REQUIRE THE ROBLOX DEFAULT THEMES TO EXIST AND INSTEAD RELY ON WHAT THE USER INSTALLED
    // TODO: MAKE PATCHING SYSTEM RECOGNISE THAT THERE CAN BE NO THEME IN THAT OVERRIDE SLOT SO IT DOWNLOADS THE DEFAULT VERSION

    /// </summary>
    /// This program is kinda poorly made dont expect amazing programming and good optimisation                                                  
    /// I also stopped developing this for a whole week and completely forgot how to work on the code 🤑🤑🤑
    /// This uses the patching code from rbxrootx's command prompt based patcher go show it some love! (https://github.com/rbxrootx/Roblox-Studio-CustomTheme-Patcher)
    public class ListViewItemsData {
        public string ImageSource { get; set; }
        public string LabelContent { get; set; }

        public string ID { get; set; }
    }

    public partial class MainWindow : Window {
        private ObservableCollection<ListViewItemsData> DarkListViewItemsCollections { get { return _DarkListViewItemsCollections; } }
        private ObservableCollection<ListViewItemsData> LightListViewItemsCollections { get { return _LightListViewItemsCollections; } }

        private ObservableCollection<ListViewItemsData> _DarkListViewItemsCollections = new ObservableCollection<ListViewItemsData>();
        private ObservableCollection<ListViewItemsData> _LightListViewItemsCollections = new ObservableCollection<ListViewItemsData>();

        public enum ThemeType {
            Light = 1,
            Dark = 2,
            UNKNOWN = 100000000
        }

        public UILibrary UILibraryInstance = new UILibrary();
        private Array[] categoryData = new Array[] { };
        private Canvas[] storedPopups = new Canvas[] { };

        private ImageBrush unavaliableBrush = new ImageBrush();

        public string automaticId = "gaminggamerstonisautomaticthisisuneccessarilylonghehe:money_mouth::slots:";

        public string myPath = System.Reflection.Assembly.GetExecutingAssembly().Location;

        public string componentsPath = Path.Combine(Environment.CurrentDirectory, "Data");
        public string themesPath = Path.Combine(Environment.CurrentDirectory, "Themes");

        public JObject themeStructure = JObject.Parse(File.ReadAllText(Path.Combine(Path.Combine(Environment.CurrentDirectory, "Data"), "Structure.json")));

        public string versionsFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Roblox", "Versions");

        public string shortcutName = "Roblox Studio Patched";

        public string patchedExecutableName = "RobloxStudioPatched.exe";
        public string studioExecutableName = "RobloxStudioBeta.exe";

        public string patchedProcessName = "RobloxStudioPatched";
        public string studioProcessName = "RobloxStudioBeta";

        private string importingFile = "";

        // Idk what to call these
        public bool StudioOpen() {
            return GeneralLibrary.ProcessOpen(studioProcessName) || GeneralLibrary.ProcessOpen(patchedProcessName);
        }

        // Theme functions
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

                    foreach (JToken favourite in FileLibrary.GetSetting("favourited")) {
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
                object[] objectified = new object[] { data.displayName, data.jsonPath, data.infoPath, data.directoryPath, data.themeType, data.favourited, data.jsonParsed, data.infoParsed, info.Name };

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

        public void RefreshThemeList()
        {
            ConsoleLibrary.WriteLine("[MAIN WINDOW] Theme list refreshing...");

            DarkListViewItemsCollections.Clear();
            LightListViewItemsCollections.Clear();

            dark_themes_list_view.ItemsSource = DarkListViewItemsCollections;
            light_themes_list_view.ItemsSource = LightListViewItemsCollections;

            object[] darkJSONs = GetEveryThemeJSONData(true, ThemeType.Dark);
            object[] lightJSONs = GetEveryThemeJSONData(true, ThemeType.Light);

            void Append(ListView box, object[] info, ObservableCollection<ListViewItemsData> data)
            {
                ConsoleLibrary.WriteLine($"[MAIN WINDOW] Adding theme \"{info[0]}\" to theme list");

                string imagePath = "";

                if ((bool)info[5])
                {
                    ConsoleLibrary.WriteLine($"[MAIN WINDOW] Theme \"{info[0]}\" is favourited");

                    imagePath = GeneralLibrary.ResourcesPath("Images/favourite.png");
                }

                data.Add(new ListViewItemsData()
                {
                    ImageSource = imagePath,
                    LabelContent = (string)info[0],
                    ID = (string)info[8]
                });

                box.ItemsSource = data;
            }

            foreach (object[] darkInfo in darkJSONs)
            {
                Append(dark_themes_list_view, darkInfo, DarkListViewItemsCollections);
            }

            foreach (object[] lightInfo in lightJSONs)
            {
                Append(light_themes_list_view, lightInfo, LightListViewItemsCollections);
            }
        }

        // UI functions
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

            FileLibrary.SetSetting("last-open-window", canvas.Name);
        }

        public void SetWorkingText(string caption, string content) {
            UILibraryInstance.RunOnUIThread(new Action(delegate {
                ConsoleLibrary.WriteLine($"[CUSTOM PROCESS] {caption} | {content}");

                working_title.Content = caption;
                working_progress.Text = content;
            }));
        }

        public void StartProgress(Canvas frame) {
            UILibraryInstance.RunOnUIThread(new Action(delegate { 
                UILibraryInstance.PlayAnimation("ProgressFrameAppear");

                foreach (Canvas canvas in storedPopups) {
                    if (canvas != frame) {
                        canvas.Visibility = Visibility.Hidden;
                    } else {
                        canvas.Visibility = Visibility.Visible;
                    }
                }
            }));
        }

        public void StopProgress() {
            UILibraryInstance.RunOnUIThread(new Action(delegate { 
                Storyboard animation = UILibraryInstance.PlayAnimation("ProgressFrameDissappear");
                
                Task.Run(new Action(delegate {
                    Thread.Sleep(500);

                    UILibraryInstance.RunOnUIThread(new Action(delegate {
                        foreach (Canvas canvas in storedPopups) {
                            canvas.Visibility = Visibility.Hidden;
                        }
                    }));
                }));
            }));
        }

        // Favouriting functions
        public bool DeleteFavourite(string path) {
            JArray favourites = (JArray)FileLibrary.GetSetting("favourited");

            foreach (JToken favourite in favourites) {
                if (favourite.ToString() == path) {
                    ConsoleLibrary.WriteLine($"[MAIN WINDOW] Removing theme \"{path}\" from favourites list");

                    favourites.Remove(favourite);

                    FileLibrary.SetSetting("favourited", favourites);

                    return true;
                }
            }

            return false;
        }

        public void AddFavourite(string path) {
            DeleteFavourite(path);
            
            JArray favourites = (JArray)FileLibrary.GetSetting("favourited");
            
            favourites.Add(path);

            FileLibrary.SetSetting("favourited", favourites);
        }

        // THE MAIN THING 🤤🤤🤤
        public void PatchStudio(string studioInstancePath, string lightOverridePath, string darkOverridePath, bool open = false, bool createShortcut = false) {
            StartProgress(working_progress_frame);

            SetWorkingText("INTIALIZING", "Finding theme JSONs and studio location...");

            string desktopDirectory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            string shortcutPath = Path.Combine(desktopDirectory, shortcutName);

            string[] versionsDirectory = Directory.GetDirectories(versionsFolderPath);
            
            if (studioInstancePath == automaticId)
            {
                studioInstancePath = versionsDirectory.FirstOrDefault(dir => File.Exists(Path.Combine(dir, studioExecutableName)));
            }

            string platformPath = Path.Combine(studioInstancePath, "Platform");
            string fullPlatformPath = Path.Combine(studioInstancePath, "Platform", "Base", "QtUI", "themes");

            string patchedPath = Path.Combine(studioInstancePath, patchedExecutableName);

            Task.Run(new Action(delegate {
                Thread.Sleep(1000);

                if (StudioOpen())
                {
                    SetWorkingText("ATTENTION", "Please close every instance of ROBLOX Studio...");

                    while (StudioOpen()) { Thread.Sleep(100); }
                }

                SetWorkingText("CLEARING", "Deleting old \"Platforms\" data and patched executable...");

                FileLibrary.DeleteDirectory(platformPath);

                if (File.Exists(patchedPath))
                {
                    File.Delete(patchedPath);
                }

                Thread.Sleep(500);

                SetWorkingText("ADDING DATA", "Creating \"Platforms\" directory inside studio...");

                Directory.CreateDirectory(fullPlatformPath);

                Thread.Sleep(500);

                SetWorkingText("ADDING DATA", "Copying JSONs into the \"Platforms\" directory...");

                void CopyJSONOver(string jsonPath, string overwriteName)
                {
                    File.WriteAllText(Path.Combine(fullPlatformPath, overwriteName + ".json"), File.ReadAllText(jsonPath));
                }

                CopyJSONOver(darkOverridePath, "DarkTheme");
                CopyJSONOver(lightOverridePath, "LightTheme");

                Thread.Sleep(500);

                SetWorkingText("PATCHING", "Running patch on studio...");

                // This extract of code is from rbxrootx's command prompt based patcher (https://github.com/rbxrootx/Roblox-Studio-CustomTheme-Patcher)
                // The program would not be possible without their patching code!

                // EXTRACT
                byte[] patchedBytes = File.ReadAllBytes(Path.Combine(studioInstancePath, studioExecutableName));

                for (int i = 0; i <= patchedBytes.Length - 1; i++)
                {
                    if (patchedBytes[i] == 0x3A)
                    {
                        if (patchedBytes[i + 1] == 0x2F)
                        {
                            if (patchedBytes[i + 2] == 0x50)
                            {
                                if (patchedBytes[i + 3] == 0x6C)
                                {
                                    patchedBytes[i] = 0x2E;
                                    patchedBytes[i + 1] = 0x2F;
                                    patchedBytes[i + 2] = 0x50;
                                    patchedBytes[i + 3] = 0x6C;
                                }
                            }
                        }
                    }
                }
                // EXTRACT END

                File.WriteAllBytes(patchedPath, patchedBytes);

                Thread.Sleep(500);

                if (createShortcut)
                {
                    SetWorkingText("FINALIZING", "Creating shortcut on Desktop...");

                    FileLibrary.CreateShortcut(shortcutName, desktopDirectory, studioInstancePath, patchedExecutableName, myPath);
                }
                else
                {
                    SetWorkingText("FINALIZING", "Opening patched executables path in explorer...");

                    Process.Start("explorer.exe", "/select, \"" + patchedPath + "\"");
                }

                Thread.Sleep(500);

                if (open)
                {
                    SetWorkingText("FINALIZING", "Opening your patched studio...");

                    var startInfo = new ProcessStartInfo();
                    startInfo.WorkingDirectory = studioInstancePath;
                    startInfo.FileName = patchedPath;

                    Process.Start(startInfo);

                    Thread.Sleep(500);
                }

                SetWorkingText("DONE", "Completely finished all operations!");

                Thread.Sleep(1000);

                StopProgress();
            }));
        }

        // ui element pickup event stuff idk
        
        // APPLICATION STARTUP
        public MainWindow() {
            ConsoleLibrary.WriteLine("[MAIN WINDOW] Registering Keystrokes");

            GeneralLibrary.RegisterKeystroke("openconsole", new Key[] { Key.LeftCtrl, Key.LeftShift, Key.C });
            GeneralLibrary.RegisterKeystroke("clearconsole", new Key[] { Key.LeftCtrl, Key.LeftShift, Key.D });

            ConsoleLibrary.WriteLine("[MAIN WINDOW] Registering UILibrary ownership");

            UILibraryInstance.caller = this;

            InitializeComponent();

            unavaliableBrush.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Images/unavaliable-opacity-mask.png"));
        
            categoryData = new Array[] {
                new object[] { editor_frame, editor_category_button },
                new object[] { settings_frame, settings_category_button },
                new object[] { patcher_frame, patcher_category_button },
                new object[] { themes_frame, themes_category_button },
            };

            storedPopups = new Canvas[] {
                working_progress_frame, 
                save_frame
            };

            ConsoleLibrary.WriteLine("[MAIN WINDOW] Opening window to last opened one");

            SwitchCategory((Canvas)FindName((string)FileLibrary.GetSetting("last-open-window")));
            
            app_version.Content = GeneralLibrary.GetVersion();

            ConsoleLibrary.WriteLine("[MAIN WINDOW] Adding themes to theme list");

            RefreshThemeList();

            ConsoleLibrary.WriteLine("[MAIN WINDOW] Binding Keystrokes");
            
            KeyDown += new KeyEventHandler(delegate {
                if (ConsoleLibrary.SettingsAllow()) {
                    if (GeneralLibrary.KeystrokeDown("openconsole")) {
                        ConsoleLibrary.StartConsole();
                    } else if (GeneralLibrary.KeystrokeDown("clearconsole")) {
                        ConsoleLibrary.ClearConsole();
                    }
                }
            });
            
            ConsoleLibrary.WriteLine("[MAIN WINDOW] Window fully initialised");
        }

        // TOPBAR BUTTONS
        private void close_button_Click(object sender, RoutedEventArgs e) {
            Application.Current.Shutdown();
        }

        private void minimize_button_Click(object sender, RoutedEventArgs e) {
            WindowState = WindowState.Minimized;
        }

        private void app_icon_Click(object sender, RoutedEventArgs e) {
            if (ConsoleLibrary.SettingsAllow()) {
                Task.Run(new Action(() =>
                {
                    var returned = UILibraryInstance.SpawnDropdown(app_icon, GeneralLibrary.ResourcesPath("Images/monochromed-icon.png"), new object[]  {
                        new object[] {
                            $"Open Console ({GeneralLibrary.KeystrokeToString("openconsole")})", "openconsole"
                        },

                         new object[] {
                            $"Clear Console ({GeneralLibrary.KeystrokeToString("clearconsole")})", "clearconsole",
                        }
                    });

                    if (returned.success) {
                        switch ((string)returned.selected[1]) {
                            case "openconsole":
                                UILibraryInstance.RunOnUIThread(new Action(() => ConsoleLibrary.StartConsole()));
                                break;
                            case "clearconsole":
                                UILibraryInstance.RunOnUIThread(new Action(() => ConsoleLibrary.ClearConsole()));
                                break;
                        };
                    }
                }));
            }
        }

        private void topbar_container_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) { DragMove(); }

        // CATEGORY BUTTONS \\
        private void patcher_category_button_Click(object sender, RoutedEventArgs e) { SwitchCategory(patcher_frame); }
        private void themes_category_button_Click(object sender, RoutedEventArgs e) { SwitchCategory(themes_frame); }
        private void editor_category_button_Click(object sender, RoutedEventArgs e) { SwitchCategory(editor_frame); }
        private void settings_category_button_Click(object sender, RoutedEventArgs e) { SwitchCategory(settings_frame); }

        // CHECKBOX INITIALIZING AND CLICKING \\
        private void CheckboxClick(object sender, RoutedEventArgs e) { UILibraryInstance.SetCheckboxState((Button)sender, true); }
        private void CheckboxInitialized(object sender, EventArgs e) { UILibraryInstance.InitializeCheckbox(sender); }

        // DROPDOWN INITIALISING AND CLICK SETUP \\
        private void light_override_dropdown_Loaded(object sender, RoutedEventArgs e) { UILibraryInstance.InitializeDropdown(sender, new object[] { GetThemeData("DefaultLight").displayName, GetThemeData("DefaultLight").jsonPath }); }
        private void dark_override_dropdown_Loaded(object sender, RoutedEventArgs e) { UILibraryInstance.InitializeDropdown(sender, new object[] { GetThemeData("DefaultDark").displayName, GetThemeData("DefaultDark").jsonPath }); }
        private void studio_instance_dropdown_Loaded(object sender, RoutedEventArgs e) { UILibraryInstance.InitializeDropdown(sender, new object[] { "Automatic", automaticId }); }
        private void theme_type_dropdown_Loaded(object sender, RoutedEventArgs e) { UILibraryInstance.InitializeDropdown(sender, new object[] { "Dark", ThemeType.Dark }); }

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

            UILibraryInstance.CreateDropdown(sender as Button, GeneralLibrary.ResourcesPath("Images/studio.png"), list.ToArray());
        }

        private void dark_override_dropdown_Click(object sender, RoutedEventArgs e) {
            UILibraryInstance.CreateDropdown(sender as Button, GeneralLibrary.ResourcesPath("Images/colorpick.png"), GetEveryThemeJSONData(true, ThemeType.Dark));
        }

        private void light_override_dropdown_Click(object sender, RoutedEventArgs e) {
            UILibraryInstance.CreateDropdown(sender as Button, GeneralLibrary.ResourcesPath("Images/colorpick.png"), GetEveryThemeJSONData(true, ThemeType.Light));
        }

        private void theme_type_dropdown_Click(object sender, RoutedEventArgs e) {
            object[] list = new object[] {
                new object[] { "Dark", ThemeType.Dark },
                new object[] { "Light", ThemeType.Light }
            };

            UILibraryInstance.CreateDropdown(sender as Button, GeneralLibrary.ResourcesPath("Images/colorpick.png"), list);
        }

        // MAIN BUTTONS
        private void run_patch_button_Click(object sender, RoutedEventArgs e) {
            PatchStudio(
                (string)UILibraryInstance.GetDropdownValue(studio_instance_dropdown),
                (string)UILibraryInstance.GetDropdownValue(light_override_dropdown),
                (string)UILibraryInstance.GetDropdownValue(dark_override_dropdown),
                UILibraryInstance.GetCheckboxState(create_shortcut_checkbox),
                UILibraryInstance.GetCheckboxState(auto_open_checkbox)
            );
        }

        private void refresh_themes_button_Click(object sender, RoutedEventArgs e) { 
            RefreshThemeList();
        }

        private void delete_theme_button_Click(object sender, RoutedEventArgs e) {
            void DeletionDelegate(ListViewItemsData choice) {
                if (choice != null) {
                    string name = choice.ID;
                    string compiledPath = Path.Combine(themesPath, name);

                    if (Directory.Exists(compiledPath)) {
                        ConsoleLibrary.WriteLine($"[MAIN WINDOW] Deleting theme \"{choice.LabelContent}\"");

                        DeleteFavourite(compiledPath);

                        FileLibrary.DeleteDirectory(compiledPath);
                    }
                }
            }

            DeletionDelegate((ListViewItemsData)dark_themes_list_view.SelectedItem);
            DeletionDelegate((ListViewItemsData)light_themes_list_view.SelectedItem);

            RefreshThemeList();
        }

        private void edit_theme_button_Click(object sender, RoutedEventArgs e) {

        } // INCOMPLETE

        private void import_theme_button_Click(object sender, RoutedEventArgs e) {
            var dialog = new OpenFileDialog();
            dialog.InitialDirectory = FileLibrary.KnownFolders.GetPath(FileLibrary.KnownFolder.Downloads);
            dialog.Filter = "JSON File|*.json";
            dialog.Title = "Select a theme JSON to import";
            
            if ((bool)dialog.ShowDialog()) {
                importingFile = dialog.FileName;

                save_title.Content = "IMPORT";
                theme_name_box.Text = dialog.SafeFileName.Remove(dialog.SafeFileName.Length - 5, 5);

                StartProgress(save_frame);

            }
        }

        private void favourite_theme_button_Click(object sender, RoutedEventArgs e) {
            void FavouriteDelegate(ListViewItemsData choice) {
                if (choice != null) {
                    string name = choice.ID;
                    string compiledPath = Path.Combine(themesPath, name);

                    if (Directory.Exists(compiledPath)) {
                        bool deleted = DeleteFavourite(compiledPath);

                        if (!deleted) {
                            AddFavourite(compiledPath);
                        }
                    }
                }
            }

            FavouriteDelegate((ListViewItemsData)dark_themes_list_view.SelectedItem);
            FavouriteDelegate((ListViewItemsData)light_themes_list_view.SelectedItem);

            RefreshThemeList();
        }

        private void upload_theme_button_Click(object sender, RoutedEventArgs e) { 
            MessageBox.Show("this dont work yet 🤑🤑 come back next version (if there is)\n(idk how to set up a database)", "this isnt ready!!!");
        } // INCOMPLETE

        private void save_theme_button_Click(object sender, RoutedEventArgs e) {
            if (string.IsNullOrEmpty(importingFile)) { return; }

            bool favourite = UILibraryInstance.GetCheckboxState(favourite_theme_checkbox);
            string name = theme_name_box.Text;
            ThemeType type = (ThemeType)UILibraryInstance.GetDropdownValue(theme_type_dropdown);

            string directoryName = $"{Directory.GetDirectories(themesPath).Length + 1}_{name}";
            string fullPath = Path.Combine(themesPath, directoryName);

            Directory.CreateDirectory(fullPath);

            JObject jsonified = FileLibrary.JSONify(File.ReadAllText(importingFile));

            if (jsonified.ContainsKey("Name")) {
                jsonified["Name"] = type.ToString();

                JObject infoJSON = FileLibrary.JSONify(@"{
	                ""name"": ""Name"", // Name of the theme
	                ""creator-name"": ""Unknown"", // Name of the creator

	                ""theme-type"": """", // 1 = Light, 2 = Dark, Any others = Dark

	                ""source"": ""Locally Stored"" // Source of the file
                }");

                infoJSON["name"] = name;
                infoJSON["theme-type"] = ((int)type).ToString();

                FileLibrary.WriteJSONToPath(Path.Combine(fullPath, "Theme.json"), jsonified);
                FileLibrary.WriteJSONToPath(Path.Combine(fullPath, "ThemeInfo.json"), infoJSON);

                if (favourite) { AddFavourite(fullPath); }
            } else {
                GeneralLibrary.ShowError("Your theme JSON is invalid", GeneralLibrary.ErrorLevel.MINOR, true);
            }

            StopProgress();
        }
    }
}

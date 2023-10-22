using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Studio_Theme_Manager.Libraries {
    internal class FileLibrary {
        public enum KnownFolder {
            Contacts,
            Downloads,
            Favorites,
            Links,
            SavedGames,
            SavedSearches
        }

        public static class KnownFolders {
            private static readonly Dictionary<KnownFolder, Guid> _guids = new Dictionary<KnownFolder, Guid>()
            {
                [KnownFolder.Contacts] = new Guid("56784854-C6CB-462B-8169-88E350ACB882"),
                [KnownFolder.Downloads] = new Guid("374DE290-123F-4565-9164-39C4925E467B"),
                [KnownFolder.Favorites] = new Guid("1777F761-68AD-4D8A-87BD-30B759FA33DD"),
                [KnownFolder.Links] = new Guid("BFB9D5E0-C6A9-404C-B2B2-AE6DB6AF4968"),
                [KnownFolder.SavedGames] = new Guid("4C5C32FF-BB9D-43B0-B5B4-2D72E54EAAA4"),
                [KnownFolder.SavedSearches] = new Guid("7D1D3A04-DEBB-4115-95CF-2F29DA2920DA")
            };

            public static string GetPath(KnownFolder knownFolder) {
                return SHGetKnownFolderPath(_guids[knownFolder], 0);
            }

            [DllImport("shell32", CharSet = CharSet.Unicode, ExactSpelling = true, PreserveSig = false)]
            private static extern string SHGetKnownFolderPath(
                [MarshalAs(UnmanagedType.LPStruct)] Guid rfid, uint dwFlags,
                int hToken = 0);
        }

        public static string settingsPath = Path.Combine(Path.Combine(Environment.CurrentDirectory, "Data"), "Data.json");

        public static JObject JSONify(string text) {
            return JObject.Parse(text);
        }

        public static JObject GetDataAsJSON(string path) {
            return JSONify(File.ReadAllText(path));
        }
        
        public static void WriteJSONToPath(string path, JObject json) {
            ConsoleLibrary.WriteLine($"[FILE LIBRARY] Writing JSON to \"{path}\", data: {json}");

            string stringified = json.ToString();

            File.WriteAllText(path, stringified);
        }

        public static JObject Settings() {
            return GetDataAsJSON(settingsPath);
        }

        public static JToken GetSetting(string name) {
            return Settings()[name];
        }

        public static bool SetSetting(string name, JToken value) {
            JObject data = Settings();

            if (data.ContainsKey(name)) {
                ConsoleLibrary.WriteLine($"[FILE LIBRARY] Setting setting \"{name}\" to: {value}");

                data[name] = value;

                WriteJSONToPath(settingsPath, data);

                return true;
            }

            return false;
        }

        public static void CreateShortcut(string name, string createAt, string executableDirectory, string executableName, string iconPath) {
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

        public static void DeleteDirectory(string path) {
            if (!Directory.Exists(path)) { return; }

            DirectoryInfo directory = new DirectoryInfo(path);

            foreach (FileInfo file in directory.GetFiles()) {
                ConsoleLibrary.WriteLine($"[FILE LIBRARY] Deleting file \"{file.Name}\"");
                file.Delete();
                ConsoleLibrary.WriteLine($"[FILE LIBRARY] Deleted file \"{file.Name}\"");
            }

            foreach (DirectoryInfo subDirectory in directory.GetDirectories()) {
                ConsoleLibrary.WriteLine($"[FILE LIBRARY] Deleting directory \"{subDirectory.Name}\"");
                subDirectory.Delete(true);
                ConsoleLibrary.WriteLine($"[FILE LIBRARY] Deleted directory \"{subDirectory.Name}\"");
            }

            directory.Delete();
        }
    }
}

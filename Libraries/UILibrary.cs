using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Studio_Theme_Manager.Libraries {
    public class UILibrary {
        public List<object[]> checkboxes = new List<object[]> { };
        public List<object[]> dropdowns = new List<object[]> { };

        public Window caller = null;

        private ImageBrush checkboxNotSelectedBrush = new ImageBrush();
        private ImageBrush checkboxSelectedBrush = new ImageBrush();

        public UILibrary(string checkboxSelectedURL = "pack://application:,,,/Images/check.png", string checkboxNotSelectedURL = "pack://application:,,,/Images/unchecked.png") {
            ConsoleLibrary.WriteLine($"[UILIBRARY] UILibrary intialized");

            checkboxNotSelectedBrush.ImageSource = new BitmapImage(new Uri(checkboxNotSelectedURL));
            checkboxSelectedBrush.ImageSource = new BitmapImage(new Uri(checkboxSelectedURL));
        }

        // OTHERS \\
        public void RunOnUIThread(Delegate method) {
            ConsoleLibrary.WriteLine($"[UILIBRARY] [THREAD SWITCH] Running a method on the UI Thread");

            caller.Dispatcher.BeginInvoke(method);
        }

        // HIERARCHY ASSISTS \\
        public static T GetParent<T>(DependencyObject child) where T : DependencyObject {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);

            if (parentObject == null) return null;

            T parent = parentObject as T;

            if (parent != null) { return parent; }
            else {
                return GetParent<T>(parentObject);
            }
        }

        public static T GetChildByName<T>(DependencyObject parent, string childName) where T : DependencyObject {
            if (parent == null) return null;

            T foundChild = null;

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);

            for (int i = 0; i < childrenCount; i++) {
                var child = VisualTreeHelper.GetChild(parent, i);

                T childType = child as T;

                if (childType == null) {
                    foundChild = GetChildByName<T>(child, childName);

                    if (foundChild != null) break;
                }
                else if (!string.IsNullOrEmpty(childName)) {
                    var frameworkElement = child as FrameworkElement;

                    if (frameworkElement != null && frameworkElement.Name == childName) {
                        foundChild = (T)child;
                        break;
                    }
                }
                else {
                    foundChild = (T)child;
                    break;
                }
            }
            return foundChild;
        }

        //-- This code dont work 🤑🤑🤑🤑🤑🤑
        public static T GetChildByType<T>(DependencyObject parent, Type type) where T : DependencyObject {
            if (parent == null) return null;

            T foundChild = null;

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);

            for (int i = 0; i < childrenCount; i++) {
                var child = VisualTreeHelper.GetChild(parent, i);

                T childType = child as T;

                if (childType == null || !child.Equals(type)) {
                    
                    foundChild = GetChildByType<T>(child, type);

                    if (foundChild != null) break;
                }
                else {
                    foundChild = (T)child;
                    break;
                }
            }

            return foundChild;
        }

        // ANIMATIONS \\
        public Storyboard PlayAnimation(string animationName) {
            Storyboard animation = (Storyboard)caller.TryFindResource(animationName);

            if (animation == null) {
                GeneralLibrary.ShowError("ANIMATION \"" + animationName + "\" DOES NOT EXIST IN THE CALLED WINDOW", GeneralLibrary.ErrorLevel.MINOR, false);
            }
            else {
                animation.Begin();

                ConsoleLibrary.WriteLine($"[UILIBRARY] Starting animation \"{animationName}\"");
            }

            return animation;
        }

        // CUSTOM  OBJECT ASSIST FUNCTIONS \\
        private (object[] array, int index) GetDataFromArrayInfo<T>(List<object[]> array, T reference, Action errorCall) {
            int trackedIndex = -1;

            foreach (object[] objectArray in array) {
                trackedIndex += 1;

                if ((object)reference == (object)objectArray[0]) {
                    return (array: objectArray, index: trackedIndex);
                }
            }

            errorCall();

            return (array: null, index: -1);
        }

        // CUSTOM  DROPDOWN FUNCTIONS \\
        public (object[] array, int index) GetDropdownInfo(Button dropdown) {
            var tuple = GetDataFromArrayInfo(
                dropdowns,
                dropdown,
                () => GeneralLibrary.ShowError("FAILED TO GET DROPDOWN INFO \"" + dropdown.Name + "\" AS IT IS NOT IN THE DROPDOWN ARRAY", GeneralLibrary.ErrorLevel.UNKNOWN, false)
            );

            return tuple;
        }

        public object GetDropdownValue(Button dropdown) {
            var returned = GetDropdownInfo(dropdown);

            if (returned.array != null) {
                RunOnUIThread(new Action(() => ConsoleLibrary.WriteLine($"[UILIBRARY] Dropdown \"{dropdown.Name}\"'s value is {returned.array[2]}")));

                return returned.array[2];
            }

            return false;
        }

        public void SetDropdownValue(Button dropdown, object[] values) {
            RunOnUIThread(new Action(() => ConsoleLibrary.WriteLine($"[UILIBRARY] Setting dropdown \"{dropdown.Name}\"'s value to [Display: {values[0]}, Actual: {values[1]}]")));

            var tuple = GetDropdownInfo(dropdown);

            if (tuple.index != -1) {
                (tuple.array[1] as Label).Content = values[0];
                dropdowns[tuple.index][2] = values[1];
            }
        }

        public (bool success, object[] selected) SpawnDropdown(UIElement parent, string icon, object[] items) {
            bool canContinue = false;
            bool success = false;
            object[] selected = items[1] as object[];

            RunOnUIThread(new Action(delegate {
                ContextMenu menu = new ContextMenu();
                menu.PlacementTarget = parent;

                foreach (object[] itemInfo in items) {
                    void Selected() {
                        selected = itemInfo;
                        success = true;
                        canContinue = true;
                    }

                    MenuItem menuItem = new MenuItem();
                    menuItem.Click += delegate { Selected(); };
                    menuItem.MouseDown += delegate { Selected(); };
                    menuItem.Header = itemInfo[0];
                    menuItem.Icon = new Image { Source = new BitmapImage(new Uri(icon)) };

                    menu.Items.Add(menuItem);
                }

                menu.IsOpen = true;
                menu.Closed += delegate {
                    canContinue = true;
                };
            }));

            while (canContinue == false) { Thread.Sleep(25); }

            return (success: success, selected: selected);
        }

        public void CreateDropdown(Button dropdown, string icon, object[] items) {
            RunOnUIThread(new Action(() => ConsoleLibrary.WriteLine($"[UILIBRARY] Creating dropdown for \"{dropdown.Name}\" with a total of {items.Length} items")));

            var tuple = GetDropdownInfo(dropdown);

            if (tuple.index != -1) {
                Task.Run(new Action(delegate {
                    var returned = SpawnDropdown(dropdown, icon, items);

                    if (returned.success && tuple.index != -1) {
                        RunOnUIThread(new Action(delegate {
                            SetDropdownValue(dropdown, returned.selected);
                        }));
                    }
                }));
            }

            //return (success: success, itemInfo: goal);
        }

        public bool InitializeDropdown(object dropdownObj, object[] defaults) {
            Button dropdown = dropdownObj as Button;
            Label assosciatedLabel = (Label)caller.FindName(dropdown.Name + "_text");

            ConsoleLibrary.WriteLine($"[UILIBRARY] Initializing dropdown \"{dropdown.Name}\", default value [Display: {defaults[0]}, Actual: {defaults[1]}]");

            if (assosciatedLabel == null) {
                GeneralLibrary.ShowError("FAILED TO INITIALISE DROPDOWN \"" + dropdown.Name + "\" AS IT DOES NOT HAVE AN ASSOSCIATED LABEL", GeneralLibrary.ErrorLevel.LOSS_OF_FUNCTIONALITY, false);

                return false;
            }

            dropdowns.Add(new object[] { dropdown, assosciatedLabel, defaults[1] });

            assosciatedLabel.Content = defaults[0];

            ConsoleLibrary.WriteLine($"[UILIBRARY] Successfully initialized dropdown \"{dropdown.Name}\"");
            
            return true;
        }

        // CUSTOM  CHECKBOX FUNCTIONS \\
        public (object[] array, int index) GetCheckboxArray(Button checkbox) {
            var tuple = GetDataFromArrayInfo(
                checkboxes,
                checkbox,
                () => GeneralLibrary.ShowError("FAILED TO GET CHECKBOX \"" + checkbox.Name + "\" AS IT IS NOT IN THE CHECKBOXES ARRAY", GeneralLibrary.ErrorLevel.UNKNOWN, false)
            );

            return tuple;
        }

        public bool GetCheckboxState(Button checkbox) {
            var returned = GetCheckboxArray(checkbox);

            if (returned.array != null) {
                RunOnUIThread(new Action(() => ConsoleLibrary.WriteLine($"[UILIBRARY] Checkbox \"{checkbox.Name}\"'s state is {returned.array[2]}")));

                return (bool)returned.array[2];
            }

            return false;
        }

        public void SetCheckboxState(Button checkbox, bool toggling, bool decided = false) {
            var returned = GetCheckboxArray(checkbox);

            if (returned.array != null) {
                void Format(bool state) {
                    Rectangle imageRef = (Rectangle)returned.array[1];

                    checkboxes[returned.index] = new object[] { (Button)returned.array[0], imageRef, state };

                    switch (state) {
                        case true:
                            imageRef.OpacityMask = checkboxSelectedBrush;
                            break;
                        case false:
                            imageRef.OpacityMask = checkboxNotSelectedBrush;
                            break;
                    }
                }

                bool boolean = !(bool)returned.array[2];

                if (!toggling) {
                    boolean = decided;
                }

                Format(boolean);

                RunOnUIThread(new Action(() => ConsoleLibrary.WriteLine($"[UILIBRARY] Set checkbox \"{checkbox.Name}\" to {boolean}")));
            }
        }

        public bool InitializeCheckbox(object checkbox, bool initialValue = false) {
            Button button = (Button)checkbox;
            Rectangle assosciatedCheck = (Rectangle)caller.FindName(button.Name + "_image");

            RunOnUIThread(new Action(() => ConsoleLibrary.WriteLine($"[UILIBRARY] Initializing checkbox \"{button.Name}\", default value is {initialValue}")));

            if (assosciatedCheck == null) {
                GeneralLibrary.ShowError("FAILED TO INITIALISE CHECKBOX \"" + button.Name + "\" AS IT DOES NOT HAVE AN ASSOSCIATED IMAGE", GeneralLibrary.ErrorLevel.LOSS_OF_FUNCTIONALITY, false);
                return false;
            }

            checkboxes.Add(new object[] { button, assosciatedCheck, false });

            SetCheckboxState(button, false, initialValue);

            RunOnUIThread(new Action(() => ConsoleLibrary.WriteLine($"[UILIBRARY] Initialized checkbox \"{button.Name}\"")));

            return true;
        }
    }
}

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

namespace Studio_Theme_Manager.Libraries {
    public class UILibrary {
        public List<object[]> checkboxes = new List<object[]> { };
        public List<object[]> dropdowns = new List<object[]> { };

        public Window caller = null;

        private ImageBrush checkboxNotSelectedBrush = new ImageBrush();
        private ImageBrush checkboxSelectedBrush = new ImageBrush();

        public UILibrary(string checkboxSelectedURL = "pack://application:,,,/Images/check.png", string checkboxNotSelectedURL = "pack://application:,,,/Images/unchecked.png") {
            checkboxNotSelectedBrush.ImageSource = new BitmapImage(new Uri(checkboxNotSelectedURL));
            checkboxSelectedBrush.ImageSource = new BitmapImage(new Uri(checkboxSelectedURL));
        }

        // OTHERS \\
        public void RunOnUIThread(Delegate method) {
            caller.Dispatcher.BeginInvoke(method);
        }

        // ANIMATIONS \\
        public Storyboard PlayAnimation(string animationName) {
            Storyboard animation = caller.FindResource(animationName) as Storyboard;

            if (animation == null) {
                GeneralLibrary.ShowError("ANIMATION \"" + animationName + "\" DOES NOT EXIST IN THE CALLED WINDOW", GeneralLibrary.ErrorLevel.MINOR, false);
            }
            else {
                animation.Begin();
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
                return returned.array[2];
            }

            return false;
        }

        public void SetDropdownValue(Button dropdown, object[] values) {
            var tuple = GetDropdownInfo(dropdown);

            if (tuple.index != -1) {
                (tuple.array[1] as Label).Content = values[0];
                dropdowns[tuple.index][2] = values[1];
            }
        }

        public void CreateDropdown(Button dropdown, string icon, object[] items) {
            var tuple = GetDropdownInfo(dropdown);

            object[] goal = new object[] { };
            bool canContinue = false;
            bool success = false;

            if (tuple.index != -1) {
                RunOnUIThread(new Action(delegate {
                    ContextMenu menu = new ContextMenu();
                    menu.PlacementTarget = tuple.array[0] as UIElement;

                    foreach (object[] itemInfo in items) {
                        void Selected() {
                            goal = itemInfo;
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

                Task.Run(new Action(delegate {
                    while (canContinue == false) { Thread.Sleep(25); }

                    if (success && tuple.index != -1) {
                        RunOnUIThread(new Action(delegate {
                            SetDropdownValue(dropdown, goal);
                        }));
                    }
                }));
            }

            //return (success: success, itemInfo: goal);
        }

        public bool InitializeDropdown(object dropdownObj, object[] defaults) {
            Button dropdown = dropdownObj as Button;
            Label assosciatedLabel = (Label)caller.FindName(dropdown.Name + "_text");

            if (assosciatedLabel == null) {
                GeneralLibrary.ShowError("FAILED TO INITIALISE DROPDOWN \"" + dropdown.Name + "\" AS IT DOES NOT HAVE AN ASSOSCIATED LABEL", GeneralLibrary.ErrorLevel.LOSS_OF_FUNCTIONALITY, false);

                return false;
            }

            dropdowns.Add(new object[] { dropdown, assosciatedLabel, defaults[1] });

            assosciatedLabel.Content = defaults[0];

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

                if (toggling) {
                    Format(!(bool)returned.array[2]);
                }
                else {
                    Format(decided);
                }
            }
        }

        public bool InitializeCheckbox(object checkbox, bool initialValue = false) {
            Button button = (Button)checkbox;
            Rectangle assosciatedCheck = (Rectangle)caller.FindName(button.Name + "_image");

            if (assosciatedCheck == null) {
                GeneralLibrary.ShowError("FAILED TO INITIALISE CHECKBOX \"" + button.Name + "\" AS IT DOES NOT HAVE AN ASSOSCIATED IMAGE", GeneralLibrary.ErrorLevel.LOSS_OF_FUNCTIONALITY, false);
                return false;
            }

            checkboxes.Add(new object[] { button, assosciatedCheck, false });

            SetCheckboxState(button, false, initialValue);

            return true;
        }
    }
}

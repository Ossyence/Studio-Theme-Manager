using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Studio_Theme_Manager.Libraries {
    public static class ExtensionLibrary {
        public static T Clamp<T>(this T val, T min, T max) where T : IComparable<T> {
            if (val.CompareTo(min) < 0) return min;
            else if (val.CompareTo(max) > 0) return max;
            else return val;
        }

        public static bool StringToBool(this string val) {
            string lowered = val as string;
            lowered = lowered.ToLower();

            if (lowered == "true" || lowered == "yes" || lowered == "y") return true;
            else if (lowered == "false" || lowered == "no" || lowered == "n") return false;
            else return false;
        }

        public static string BoolToString(this bool val) {
            if (val == true ) return "true";
            else return "false";
        }
    }
}

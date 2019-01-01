using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Reflection;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace Ultimud
{
    [Obfuscation(Exclude = true, ApplyToMembers = false)]
    internal class Utils
    {
        [Obfuscation(Exclude = true)]
        public enum Perspective
        {
            FirstPerson,
            SecondPerson,
            ThirdPerson
        }

        public static Dictionary<string, List<T>> ConsolidateList<T>(List<T> list) where T : IObject
        {
            Dictionary<string, List<T>> dictionary = new Dictionary<string, List<T>>();
            foreach (T t in list)
            {
                if (string.IsNullOrEmpty(t.Name))
                    continue;

                if (!dictionary.ContainsKey(t.Name))
                    dictionary.Add(t.Name, new List<T>());
                
                dictionary[t.Name].Add(t);
            }
            return dictionary;
        }
    }
}

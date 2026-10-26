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

        public static Dictionary<string, List<T>> ConsolidateList<T>(List<T> list) where T : Object
        {
            Dictionary<string, List<T>> dictionary = new Dictionary<string, List<T>>();
            foreach (T t in list)
            {
                if (t.Name == null || t.Name == string.Empty)
                    continue;

                if (!dictionary.ContainsKey(t.Name))
                    dictionary.Add(t.Name, new List<T>());

                dictionary[t.Name].Add(t);
            }
            return dictionary;
        }

        [Obfuscation(Exclude = false)]
        public class AssemblyObjectSelector<T> : UITypeEditor
        {
            ListBox listBox_ = new ListBox();

            public AssemblyObjectSelector()
            {
                this.listBox_.BorderStyle = BorderStyle.None;
                this.listBox_.Sorted = true;
            }

            public override bool IsDropDownResizable
            {
                get { return true; }
            }
            public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
            {
                return UITypeEditorEditStyle.DropDown;
            }
            public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
            {
                this.listBox_.Items.Clear();
                Assembly assembly = typeof(T).Assembly;
                foreach (Type type in assembly.GetTypes())
                {
                    if (type.GetInterface("Ultimud.IFightSystem") != null)
                    {
                        if (value != null && value.GetType() == type)
                            this.listBox_.Items.Add(value);
                        else
                            this.listBox_.Items.Add(assembly.CreateInstance(type.FullName));
                    }
                }

                IWindowsFormsEditorService service = provider.GetService(typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;
                this.listBox_.MouseClick += delegate(object sender, MouseEventArgs e)
                {
                    service.CloseDropDown();
                };
                service.DropDownControl(this.listBox_);
                return (this.listBox_.SelectedItem != null ? this.listBox_.SelectedItem : value);
            }
        }
    }
}

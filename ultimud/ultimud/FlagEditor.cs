using System;
using System.Collections;
using System.Text;
using System.Drawing.Design;
using System.ComponentModel;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using System.Reflection;

namespace Ultimud
{
    [Obfuscation(Exclude = false)]
    public class FlagEditor<T> : UITypeEditor
    {
        CheckedListBox listBox_ = new CheckedListBox();

        public FlagEditor()
        {
            this.listBox_.BorderStyle = BorderStyle.None;
            this.listBox_.CheckOnClick = true;
        }

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.DropDown;
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            if (value != null)
            {
                BitArray flags = value as BitArray;

                this.listBox_.Items.Clear();

                string[] flagNames = Enum.GetNames(typeof(T));
                for (int x = 0; x < flagNames.Length; ++x)
                    this.listBox_.Items.Add(flagNames[x], flags.Get(x));

                IWindowsFormsEditorService service = provider.GetService(typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;
                service.DropDownControl(this.listBox_);

                for (int x = 0; x < this.listBox_.Items.Count; ++x)
                    flags.Set(x, this.listBox_.GetItemChecked(x));

                return flags;
            }

            return value;
        }

        public override bool IsDropDownResizable
        {
            get { return true; }
        }
    }
}

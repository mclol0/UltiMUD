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
    public class FlagEditor<T> : UITypeEditor
    {
        CheckedListBox listbox_ = new CheckedListBox();

        public override bool IsDropDownResizable
        {
            get { return true; }
        }

        public FlagEditor()
        {
            this.listbox_.BorderStyle = BorderStyle.None;
            this.listbox_.CheckOnClick = true;
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
                this.listbox_.Items.Clear();
                string[] flagnames = Enum.GetNames(typeof(T));
                for (int x = 0; x < flagnames.Length; ++x)
                    this.listbox_.Items.Add(flagnames[x], flags.Get(x));

                IWindowsFormsEditorService service = provider.GetService(typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;
                service.DropDownControl(this.listbox_);

                for (int x = 0; x < this.listbox_.Items.Count; ++x)
                    flags.Set(x, this.listbox_.GetItemChecked(x));

                this.OnReturn(flags);
                return flags;
            }

            return value;
        }

        public virtual void OnReturn(BitArray flags)
        {
        }
    }
}

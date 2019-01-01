using System;
using System.Text;
using System.Drawing.Design;
using System.ComponentModel;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using System.Reflection;

namespace Ultimud
{
    [Obfuscation(Exclude = false)]
    class StringEditor : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.DropDown;
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            string str = value as string;
            IWindowsFormsEditorService service = provider.GetService(typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;
            Damdai.Forms.TextEditorWindow editor = new Damdai.Forms.TextEditorWindow();
            editor.Text = str;
            if (service.ShowDialog(editor) == DialogResult.OK)
                return editor.Text.Replace("\n", "\r\n").Replace("\r\r\n", "\r\n");
            else
                return str;
        }

        public override bool IsDropDownResizable
        {
            get { return true; }
        }
    }
}

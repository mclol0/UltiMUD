//using System;
//using System.Text;
//using System.Drawing.Design;
//using System.ComponentModel;
//using System.Windows.Forms;
//using System.Windows.Forms.Design;
//using System.Reflection;

//namespace Ultimud
//{
//    [Obfuscation(Exclude = false)]
//    public class StringEditor : UITypeEditor
//    {
//        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
//        {
//            return UITypeEditorEditStyle.DropDown;
//        }

//        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
//        {
//            string str = value as string;
//            IWindowsFormsEditorService service = provider.GetService(typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;
//            Damdai.Forms.TextEditor editor = new Damdai.Forms.TextEditor();
//            editor.Text = str;
//            service.DropDownControl(editor);
//            return editor.Text.Replace("\n", "\r\n").Replace("\r\r\n", "\r\n");
//        }

//        public override bool IsDropDownResizable
//        {
//            get { return true; }
//        }
//    }
//}

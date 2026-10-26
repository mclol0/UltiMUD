using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using System.Drawing.Design;
using System.ComponentModel;

namespace Ultimud
{
    [Serializable]
    [Obfuscation(Exclude = true)]
    internal class Script : IIdentifiable
    {
        internal enum Types
        {
            Npc,
            Ability,
            StatModifier
        }

        Guid id_;
        string name_;
        string code_;
        Types type_;

        #region Properties
        [Browsable(false)]
        public Guid Id
        {
            get { return this.id_; }
            set { this.id_ = value; }
        }
        
        public string Name
        {
            get { return this.name_; }
            set { this.name_ = value; }
        }
        
        [Editor(typeof(StringEditor), typeof(UITypeEditor))]
        public string Code
        {
            get { return this.code_; }
            set { this.code_ = value; }
        }
        
        public Types ScriptType
        {
            get { return this.type_; }
            set { this.type_ = value; }
        }
        #endregion

        public Script() { }
        public Script(Types type, string name, string code)
        {
            this.type_ = type;
            this.name_ = name;
            this.code_ = code;
        }

        [Obfuscation(Exclude = false)]
        public override string ToString()
        {
            return (string.IsNullOrEmpty(this.name_) ? base.ToString() : this.name_);
        }

        internal static void Init(string loadpath, string serverpath)
        {
            if (!Server.Instance.DataSets.ContainsKey(typeof(Script)))
                Server.Instance.DataSets.Add(typeof(Script), new Dictionary<Guid, IIdentifiable>());
        }

        [Obfuscation(Exclude = false)]
        public abstract class ScriptSelector : UITypeEditor
        {
            protected ListBox listbox_ = new ListBox();
            protected Script.Types scripttype_;
            IWindowsFormsEditorService service_;

            public ScriptSelector()
            {
                this.listbox_.BorderStyle = BorderStyle.None;
                this.listbox_.Sorted = true;
                this.listbox_.MouseClick += delegate(object sender, MouseEventArgs e)
                {
                    if (this.service_ != null)
                        this.service_.CloseDropDown();
                };
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
                Guid scriptid = Guid.Empty;
                if (value != null)
                    scriptid = (Guid)value;

                this.listbox_.Items.Clear();
                this.listbox_.Items.Add(" <None>");
                this.listbox_.Items.Add(" <New>");
                if (scriptid != Guid.Empty && Server.Instance.Get<Script>(scriptid) != null)
                    this.listbox_.Items.Add(" <Edit>");
                foreach (Script s in Server.Instance.DataSets[typeof(Script)].Values)
                {
                    if (s.type_ == this.scripttype_)
                    {
                        this.listbox_.Items.Add(s);
                        if (s.Id == scriptid)
                            this.listbox_.SelectedItem = s;
                    }
                }

                this.service_ = provider.GetService(typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;
                this.service_.DropDownControl(this.listbox_);
                object selecteditem = this.listbox_.SelectedItem;
                if (selecteditem != null)
                {
                    if (selecteditem is Script) return (this.listbox_.SelectedItem as Script).Id;
                    else if (selecteditem.ToString() == " <None>") return Guid.Empty;
                    else if (selecteditem.ToString() == " <New>")
                    {
                        PropertyGrid propgrid = new PropertyGrid();
                        Script script = new Script();
                        script.type_ = this.scripttype_;
                        propgrid.SelectedObject = script;
                        Damdai.Forms.Dialog dialog = new Damdai.Forms.Dialog(propgrid, "New Script");
                        if (dialog.ShowDialog() == DialogResult.OK)
                        {
                            Server.Instance.Add<Script>(script);
                            return script.Id;
                        }
                        else return scriptid;
                    }
                    else if (selecteditem.ToString() == " <Edit>")
                    {
                        Script script = Server.Instance.Get<Script>(scriptid);
                        if (script != null)
                        {
                            PropertyGrid propgrid = new PropertyGrid();
                            propgrid.SelectedObject = script;
                            Damdai.Forms.Dialog dialog = new Damdai.Forms.Dialog(propgrid, "Edit Script");
                            dialog.ShowDialog();
                        }
                        return scriptid;
                    }
                    else return Guid.Empty;
                }
                else
                    return value;
            }
        }
        
        [Obfuscation(Exclude = false)]
        public class AbilityScriptSelector : ScriptSelector
        {
            public AbilityScriptSelector()
                : base()
            {
                this.scripttype_ = Types.Ability;
            }
        }
        
        [Obfuscation(Exclude = false)]
        public class NpcScriptSelector : ScriptSelector
        {
            public NpcScriptSelector()
                : base()
            {
                this.scripttype_ = Types.Npc;
            }
        }
        
        [Obfuscation(Exclude = false)]
        public class StatModifierScriptSelector : ScriptSelector
        {
            public StatModifierScriptSelector()
                : base()
            {
                this.scripttype_ = Types.StatModifier;
            }
        }
    }
}

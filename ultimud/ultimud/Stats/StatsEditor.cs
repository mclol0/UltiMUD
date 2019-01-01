using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing.Design;
using System.ComponentModel;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using System.Reflection;
using Damdai.Forms;

namespace Ultimud
{
    [Obfuscation(Exclude = false)]
    class StatsEditor : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }
        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            if (value != null)
            {
                Dictionary<Guid, Stat> dictionary = value as Dictionary<Guid, Stat>;

                List<Stat> stats = new List<Stat>();
                foreach (StatBlueprint statBlueprint in Server.Instance.DataSets[typeof(StatBlueprint)].Values)
                {
                    if (dictionary.ContainsKey(statBlueprint.Id))
                        stats.Add(dictionary[statBlueprint.Id]);
                    else
                        stats.Add(new Stat(statBlueprint.Id));
                }

                Stat[] array = new Stat[stats.Count];
                stats.CopyTo(array, 0);

                StatsEditorControl editor = new StatsEditorControl(array);

                IWindowsFormsEditorService service = provider.GetService(typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;
                Damdai.Forms.Dialog dialog = new Damdai.Forms.Dialog(editor, "Edit Stats");
                if (service.ShowDialog(dialog) == DialogResult.OK)
                {
                    dictionary.Clear();
                    foreach (Stat stat in editor.Objects)
                    {
                        if (stat.Max != 0 || stat.Current != 0)
                        {
                            dictionary.Add(stat.Id, stat);
                        }
                    }
                }

                return dictionary;
            }

            return value;
        }
    }
}

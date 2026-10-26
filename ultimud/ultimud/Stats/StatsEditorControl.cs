using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using Damdai.Forms;

namespace Ultimud
{
    [Obfuscation(Exclude = false)]
    class StatsEditorControl : ObjectListEditor<Stat>
    {
        public StatsEditorControl(ICollection<object> objects)
            : base(objects, Ultimud.Properties.Resources.stat, null)
        {
        }

        public override void CreateNewObject(Type type)
        {
            StatBlueprint statBluePrint = new StatBlueprint();
            Damdai.Forms.PropertyGrid propertyGrid = new Damdai.Forms.PropertyGrid();
            propertyGrid.SelectedObject = statBluePrint;
            Dialog dialog = new Dialog(propertyGrid, "New Stat");
            while (dialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    Server.Instance.Add<StatBlueprint>(statBluePrint);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    continue;
                }

                Stat stat = new Stat(statBluePrint.Id);
                this.toolStripComboBox1.Items.Add(stat);
                this.toolStripComboBox1.SelectedItem = stat;
                this.propertyGrid1.SelectedObject = stat;
                this.AddedObjects.Add(stat);
                break;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Reflection;

namespace Ultimud
{
    public partial class FightSystemEditor : Form
    {
        public IFightSystem FightSystem
        {
            get { return this.propertyGrid1.SelectedObject as IFightSystem; }
        }

        public FightSystemEditor(IFightSystem fightSystem)
        {
            InitializeComponent();
            this.Init(fightSystem);
        }

        void Init(IFightSystem fightSystem)
        {
            Assembly assembly = this.GetType().Assembly;
            foreach (Type type in assembly.GetTypes())
            {
                if (type.GetInterface("Ultimud.IFightSystem") != null)
                {
                    this.toolStripComboBox1.Items.Add(type.FullName);
                    if (fightSystem != null && fightSystem.GetType().FullName == type.FullName)
                    {
                        this.toolStripComboBox1.SelectedItem = fightSystem.GetType().FullName;
                    }
                }
            }

            this.propertyGrid1.SelectedObject = fightSystem;
        }

        private void toolStripComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            Assembly assembly = this.GetType().Assembly;
            this.propertyGrid1.SelectedObject = assembly.CreateInstance(this.toolStripComboBox1.Text);
        }
    }
}
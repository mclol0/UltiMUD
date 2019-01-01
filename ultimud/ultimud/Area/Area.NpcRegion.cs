using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using System.Runtime.Serialization;

namespace Ultimud
{
    public partial class Area
    {
        [Serializable]
        [Obfuscation(Exclude = true)]
        public class NpcRegion : Region
        {
            Dictionary<Guid, int> npcs_ = new Dictionary<Guid, int>();
            String spawnmsg_ = new String();
            uint spawnlimit_;
            uint currentcount_;

            #region Properties
            [Editor(typeof(NpcSelector), typeof(UITypeEditor))]
            [TypeConverter(typeof(Server.ObjectCollectionConverter<Npc>))]
            public Dictionary<Guid, int> Npcs
            {
                get { return this.npcs_; }
                set { lock (this.npcs_) { this.npcs_ = value; } }
            }

            public String SpawnMessage
            {
                get { return this.spawnmsg_; }
                set { this.spawnmsg_ = value; }
            }

            [Browsable(false)]
            public new string Name
            {
                get { return null; }
            }

            [Browsable(false)]
            public new string Plural
            {
                get { return null; }
            }

            [Browsable(false)]
            public new Telnet.Color FillColor
            {
                get { return base.FillColor; }
            }

            [Browsable(false)]
            public new char FillChar
            {
                get { return base.FillChar; }
            }

            [Browsable(false)]
            public new char FillChar2
            {
                get { return base.FillChar2; }
            }

            [Browsable(false)]
            public new Telnet.Color FillColor2
            {
                get { return base.FillColor2; }
            }

            [Browsable(false)]
            public new int FillChar2Percent
            {
                get { return this.FillChar2Percent; }
            }

            [Browsable(false)]
            public new bool FillChar2Randomize
            {
                get { return base.FillChar2Randomize; }
            }

            [Browsable(false)]
            public new BitArray LayerFlags
            {
                get { return base.LayerFlags; }
            }

            [Browsable(false)]
            public new bool CanRegen
            {
                get { return false; }
            }

            [Browsable(false)]
            public new string DeathMessage
            {
                get { return null; }
            }

            [Browsable(false)]
            public new string Description
            {
                get { return null; }
            }

            [Browsable(false)]
            public new int Height
            {
                get { return 0; }
            }

            [Browsable(false)]
            public new List<Stat> Stats
            {
                get { return null; }
            }

            [Browsable(false)]
            public new List<StatusEffect> StatusEffects
            {
                get { return null; }
            }

            [Browsable(false)]
            public new int Value
            {
                get { return 0; }
            }

            [Browsable(false)]
            public new BitArray ObjectFlags
            {
                get { return base.ObjectFlags; }
                set { base.ObjectFlags = value; }
            }

            [Browsable(false)]
            public new WeatherConditions WeatherConditions
            {
                get { return base.WeatherConditions; }
                set { base.WeatherConditions = value; }
            }

            [DefaultValue(0)]
            [Description("The number of npcs that can exist at any given time. 0 = unlimited.")]
            public uint SpawnLimit
            {
                get { return this.spawnlimit_; }
                set { this.spawnlimit_ = value; }
            }

#if !DEBUG
            [Browsable(false)]
#endif

            public uint CurrentCount
            {
                get { return this.currentcount_; }
            }

            [Browsable(false)]
            public override int IconIndex
            {
                get { return 25; }
            }
            #endregion

            public NpcRegion()
            {
                this.ObjectFlags.Set((int)Object.ObjFlags.Invisible, true);
            }

            [Obfuscation(Exclude = false)]
            public bool CheckEncounter(Character ch)
            {
                if (ch is Npc || (this.spawnlimit_ > 0 && this.currentcount_ >= this.spawnlimit_))
                    return false;

                lock (this.npcs_)
                {
                    foreach (KeyValuePair<Guid, int> pair in this.Npcs)
                    {
                        if (Server.RandomNumber.Next(100) < pair.Value)
                        {
                            Npc npc = Server.Instance.Get<Npc>(pair.Key);
                            if (npc != null)
                            {
                                Npc spawn = npc.GetType().GetMethod("Create", BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { npc }) as Npc;
                                spawn.DieEvent += this.spawn_DieEvent;
                                spawn.Location = ch.Location;
                                spawn.Area.Send(this.spawnmsg_, spawn, ch, 1);
                                ch.LastAggressiveAct = spawn.LastAggressiveAct = DateTime.Now;
                                if (ch.CurrentAction != null)
                                    ch.Stop(false);
                                spawn.MergeFightSystems(ch);
                                Ability a = spawn.GetRandomAggressiveAbility(ch);
                                if (a != null)
                                    a.OnPerform(spawn, ch, true, spawn.GetEquippedItem(a.BodyPart));
                                ++this.currentcount_;
                                return true;
                            }
                        }
                    }
                }
                return false;
            }

            [Obfuscation(Exclude = false)]
            void spawn_DieEvent(Object o)
            {
                --this.currentcount_;
                o.DieEvent -= this.spawn_DieEvent;
            }
            
            [OnDeserialized]
            [Obfuscation(Exclude = false)]
            internal new void OnDeserialized(StreamingContext context)
            {
                base.OnDeserialized(context);
                if (this.spawnmsg_ == null)
                    this.spawnmsg_ = new String();
            }

            [Obfuscation(Exclude = false)]
            public class NpcSelector : UITypeEditor
            {
                NumericUpDown control_ = new NumericUpDown();
                Damdai.Forms.ListView view_ = new Damdai.Forms.ListView();

                public NpcSelector()
                {
                    //this.control_.Maximum = 100;
                    this.control_.Minimum = 0;
                    this.view_.BorderStyle = BorderStyle.None;
                    this.view_.HeaderStyle = ColumnHeaderStyle.Nonclickable;
                    this.view_.CheckBoxes = true;
                    this.view_.Columns.Add("Npc");
                    this.view_.Columns.Add("Chance/Cost");
                    this.view_.SubItemEndEditing += this.view_SubItemEndEditing;
                    this.view_.SubItemClicked += this.view_SubItemClicked;
                    this.control_.Parent = this.view_;
                    this.control_.Visible = false;
                    this.view_.Size = new System.Drawing.Size(200, 300);
                }

                public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
                {
                    return UITypeEditorEditStyle.DropDown;
                }
                public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
                {
                    this.view_.Items.Clear();

                    Dictionary<Guid, int> dict = value as Dictionary<Guid, int>;
                    if (dict == null)
                        dict = new Dictionary<Guid, int>();

                    foreach (Npc npc in Server.Instance.DataSets[typeof(Npc)].Values)
                    {
                        int chance = 0;
                        bool found = dict.TryGetValue(npc.Id, out chance);
                        ListViewItem i = new ListViewItem(new string[] { npc.Name, chance.ToString() });
                        i.Tag = npc;
                        i.Checked = found;
                        this.view_.Items.Add(i);
                    }

                    IWindowsFormsEditorService service = provider.GetService(typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;
                    service.DropDownControl(this.view_);

                    dict.Clear();
                    foreach (ListViewItem i in this.view_.Items)
                        if (i.Checked)
                            dict.Add(((Npc)i.Tag).Id, int.Parse(i.SubItems[1].Text));

                    return dict;
                }
                private void view_SubItemClicked(object sender, Damdai.Forms.SubItemEventArgs e)
                {
                    if (e.SubItem == 1)
                    {
                        this.control_.Value = int.Parse(e.Item.SubItems[e.SubItem].Text);
                        this.view_.StartEditing(this.control_, e.Item, e.SubItem);
                        this.control_.Select(0, 10);
                    }
                }
                private void view_SubItemEndEditing(object sender, Damdai.Forms.SubItemEndEditingEventArgs e)
                {
                    if (e.SubItem == 1)
                        e.Item.SubItems[e.SubItem].Text = this.control_.Value.ToString();
                }
                public override bool IsDropDownResizable
                {
                    get { return true; }
                }
            }
        }
    }
}

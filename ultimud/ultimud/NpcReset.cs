using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.ComponentModel;
using System.Drawing.Design;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Serialization;

namespace Ultimud
{
    [Serializable]
    [Obfuscation(Exclude = true)]
    public class NpcReset : Reset
    {
        public enum TeachTriggers
        {
            OnRequest,
            OnDefeat
        }
        public enum ScriptType
        {
            OnDefeat,
            OnSpawn,
            OnUpdate
        }

#pragma warning disable 0169, 0414
        List<Guid> teachableAbilities_; //Deprecated.
#pragma warning restore 0169, 0414
        Area.Region boundary_;
#pragma warning disable 0169, 0414
        TeachTriggers teachTrigger_; //Deprecated.
#pragma warning restore 0169, 0414
        int count_ = 1;
        List<Guid> items_;
#pragma warning disable 0169, 0414
        BitArray npcFlags_; //Deprecated.
        List<TextTrigger> textTriggers_; //Deprecated.
        int teachChance_; //Deprecated.
        Dictionary<Guid, int> teachableAbilities2_; //Deprecated.
        Dictionary<ScriptType, string> scripts_; //Deprecated.
        Character.Positions startpos_; //Deprecated.
        Dictionary<ScriptType, Guid> scripts2_; //Deprecated.
#pragma warning restore 0169, 0414

        #region Properties
        [DisplayName("Npc")]
        [Description("The npc that this reset will spawn.")]
        [TypeConverter(typeof(Server.ObjectBlueprintIdToNameConverter<Npc>))]
        [Editor(typeof(Server.ObjectBlueprintIdSelector<Npc>), typeof(UITypeEditor))]
        public new Guid ObjectId
        {
            get { return base.ObjectId; }
            set { base.ObjectId = value; }
        }

        [DisplayName("Boundary")]
        [Description("A virtual fence restricting this NPC's movement. An NPC will not be able to travel beyond its boundary.")]
        [Editor(typeof(BoundarySelector.Editor), typeof(UITypeEditor))]
        public NpcReset This
        {
            get { return this; }
            set { }
        }

        [Browsable(false)]
        public Area.Region Boundary
        {
            get { return this.boundary_; }
            set { this.boundary_ = value; }
        }

        [Description("The number of NPCs to spawn.")]
        [DefaultValue(1)]
        public int Count
        {
            get { return this.count_; }
            set { this.count_ = value; }
        }

        [Description("The items that this NPC will spawn with.")]
        [Editor(typeof(Server.MultiObjectSelector<Item>), typeof(UITypeEditor))]
        [TypeConverter(typeof(Server.ObjectCollectionConverter<Item>))]
        public List<Guid> Items
        {
            get { return this.items_; }
            set { this.items_ = value; }
        }
        #endregion

        public NpcReset(Location loc) : base(loc) { }

        [Obfuscation(Exclude = false)]
        protected override bool CheckConditions()
        {
            return (this.objectIds_.Count < this.Count);
        }
        
        [Obfuscation(Exclude = false)]
        protected override Object Respawn()
        {
            Npc npc = base.Respawn() as Npc;
            if (npc != null)
            {
                npc.Reset = this;
                npc.DieEvent += this.npc_DieEvent;
                this.ResetItems(npc);
                npc.DoScript(Npc.ScriptType.OnSpawn, null);
            }
            return npc;
        }
        
        [Obfuscation(Exclude = false)]
        void ResetItems(Npc npc)
        {
            if (this.Items != null)
            {
                foreach (Guid id in this.Items)
                {
                    Item item = Server.Instance.Get<Item>(id);
                    if (item != null)
                    {
                        item = Item.Create(item);
                        item.Parent = npc;
                        if (item.EquipLocation != Character.BodyParts.None)
                            npc.Use(item, true);
                    }
                }
            }
        }
        
        void npc_DieEvent(Object o)
        {
            this.objectIds_.Remove(o.Id);
            o.DieEvent -= this.npc_DieEvent;
        }

        [OnSerializing]
        [Obfuscation(Exclude = false)]
        internal void OnSerializing(StreamingContext context)
        {
            this.teachableAbilities_ = null;
        }

        [OnDeserialized]
        [Obfuscation(Exclude = false)]
        internal new void OnDeserialized(StreamingContext context)
        {
            base.OnDeserialized(context);
            this.teachableAbilities_ = null;
            this.teachTrigger_ = TeachTriggers.OnRequest;
            this.npcFlags_ = null;
            this.teachChance_ = 0;
            this.teachableAbilities2_ = null;
            this.startpos_ = Object.Positions.Stopped;
            this.scripts2_ = null;

            if (this.scripts_ != null)
            {
                Server.Instance.Loaded += delegate()
                {
                    this.scripts_.OnDeserialization(null);
                    foreach (KeyValuePair<ScriptType, string> kvp in this.scripts_)
                    {
                        if (!string.IsNullOrEmpty(kvp.Value))
                        {
                            Guid scriptid = Guid.Empty;
                            foreach (Script s in Server.Instance.DataSets[typeof(Script)].Values)
                                if (s.Code == kvp.Value)
                                    return;

                            Script script = new Script(Script.Types.Npc, "Npc_" + Guid.NewGuid(), kvp.Value);
                            Server.Instance.Add<Script>(script);
                        }
                    }
                    this.scripts_ = null;
                };
            }
            if (this.textTriggers_ != null)
            {
                Server.Instance.Loaded += delegate()
                {
                    foreach (TextTrigger tt in this.textTriggers_)
                    {
                        if (!string.IsNullOrEmpty(tt.Response))
                        {
                            string str = "--" + tt.Trigger + "\r\n\r\n" + tt.Response;
                            Guid scriptid = Guid.Empty;
                            foreach (Script s in Server.Instance.DataSets[typeof(Script)].Values)
                                if (s.Code == str)
                                    return;

                            Script script = new Script(Script.Types.Npc, "TextTrigger_" + Guid.NewGuid().ToString(), str);
                            Server.Instance.Add<Script>(script);
                        }
                    }
                    this.textTriggers_ = null;
                };
            }
        }
    }
}

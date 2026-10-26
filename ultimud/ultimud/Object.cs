using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Design;
using System.Timers;
using System.Reflection;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Damdai;
using Damdai.TypeConverters;

namespace Ultimud
{
    [Serializable]
    [Obfuscation(Exclude = true)]
    public partial class Object : IIdentifiable, ILocation, IContainer
    {
        #region Enums
        public enum ObjFlags
        {
            Edible,
            Container,
            Indestructible,
            Invisible,
            Flying,
            Immovable,
            Unattackable,
            Money,
            Closeable,
            CanSwim,
            Vehicle,
        }
        public enum Positions
        {
            None,
            Dead,
            Unconscious,
            Sleeping,
            Stopped,
            Moving,
            Sitting,
            Resting,
            Concentrating,
            Recovering,
        }
        public enum Sex
        {
            None,
            Male,
            Female,
        }
        #endregion

        #region Members
        [Obfuscation(Exclude = false)]
        [NonSerialized]
        IFightSystem fightSystem_;

        [Obfuscation(Exclude = false)]
        [NonSerialized]
        DateTime lastAggressiveAct_;
        
        [Obfuscation(Exclude = false)]
        [NonSerialized]
        protected Object parent_;
        
        [Obfuscation(Exclude = false)]
        [NonSerialized]
        Dictionary<object, object> vars_ = new Dictionary<object, object>();

        Guid id_;
        string name_;
        Guid parentId_;
        List<Object> children_ = new List<Object>();
        Location location_ = new Location();
        BitArray objectFlags_ = new BitArray(16);
        protected Dictionary<Guid, Stat> stats_ = new Dictionary<Guid,Stat>();
        string mapSymbol_ = "* ";
        Telnet.Color mapColor_ = Telnet.Color.White;
        List<Action> actions_ = new List<Action>();
        Positions position_ = Positions.Stopped;
        Sex sex_ = Sex.None;
        int value_;
        List<StatusEffect> statusEffects_ = new List<StatusEffect>();
        int size_;
        string deathMessage_ = "{1} has been destroyed!";
#pragma warning disable 0169, 0414
        TimeSpan lifeSpan_; //Deprecated.
#pragma warning restore 0169, 0414
        public bool canRegen_;
        bool closed_;
        bool closeable_;
        bool locked_;
        Guid keyId_;
        Damdai.Timer lifespan_;
        string description_;
        Dictionary<object, object> savedvars_;
        string plural_;
        List<Guid> tags_ = new List<Guid>();
        #endregion

        #region Properties
        [Browsable(false)]
        public Guid Id
        {
            get { return this.id_; }
            set { this.id_ = value; }
        }

        [Category("Appearance")]
        [Description("The name of this object.")]
        public string Name
        {
            get { return (this.name_ == null ? string.Empty : this.name_); }
            set { this.name_ = Telnet.Decolorize(value); }
        }

        [Category("Appearance")]
        [Description("The plural name of this object.")]
        public string Plural
        {
            get { return this.plural_; }
            set { this.plural_ = value; }
        }

        [Description("The initial object that this object belongs to.")]
        [Browsable(false)]
        public Object Parent
        {
            get { return this.parent_; }
            set
            {
                if (this == value || (value != null && this == value.Parent))
                    throw new Exception("A parent cannot be a child of itself.");
                else if (this.Parent != null && this.Parent.Children != null)
                {
                    lock (this.Parent.Children)
                        this.Parent.Children.Remove(this);
                    this.Parent.OnChildRemoved(this);
                }

                Object oldParent = this.Parent;
                this.parent_ = value;
                if (this.Parent != null)
                {
                    lock (this.parent_.Children)
                    {
                        if (!this.parent_.Children.Contains(this))
                        {
                            this.Parent.Children.Add(this);
                            this.Parent.OnChildAdded(this);
                        }
                    }
                }
                else
                {
                    if (this.DieEvent != null)
                        this.DieEvent(this);
                }
            }
        }

        [Browsable(false)]
        public Guid ParentId
        {
            get
            {
                if (this.Parent != null)
                {
                    return this.Parent.Id;
                }
                else
                {
                    return Guid.Empty;
                }
            }
            set { this.Parent = Server.Instance.Find<Object>(value); }
        }

        [Description("The objects that this object contains.")]
        [Browsable(false)]
        public List<Object> Children
        {
            get { return this.children_; }
            set { this.children_ = value; }
        }

        [Description("Various flags that effect the behavior of this object.")]
        [Editor(typeof(FlagEditor<ObjFlags>), typeof(UITypeEditor))]
        [TypeConverter(typeof(Damdai.TypeConverters.BitArrayConverter<ObjFlags>))]
        public BitArray ObjectFlags
        {
            get { return this.objectFlags_; }
            set { this.objectFlags_ = value; }
        }

        [Description("The location of this object relative to its parent.")]
        [Browsable(false)]
        public virtual Location Location
        {
            get { return (this.parent_ is Vehicle ? this.parent_.Location : this.location_); }
            set
            {
                this.location_ = new Location(value.Area, value.Point);
                //this.location_.Area = value.Area;
                //this.location_.Point = value.Point;
                this.Parent = value.Area;
            }
        }

        [Description("The stats of this object.")]
        [Editor(typeof(StatsEditor), typeof(UITypeEditor))]
        [TypeConverter(typeof(StatCollectionConverter))]
        public Dictionary<Guid, Stat> Stats
        {
            get { return this.stats_; }
            set { this.stats_ = value; }
        }

        [Category("Appearance")]
        [Description("The ASCI character that represents this object on the map.")]
        [DefaultValue("* ")]
        public string MapSymbol
        {
            get { return this.mapSymbol_; }
            set { this.mapSymbol_ = value; }
        }

        [Category("Appearance")]
        [Description("The color of the MapSymbol.")]
        [DefaultValue(Telnet.Color.White)]
        public Telnet.Color MapColor
        {
            get { return this.mapColor_; }
            set { this.mapColor_ = value; }
        }

        [Browsable(false), XmlIgnore]
        public Area Area
        {
            get
            {
                Object parent = this.Parent;
                while (parent != null && !(parent is Area))
                    parent = parent.Parent;
                return parent as Area;
            }
        }

        [Browsable(false), XmlIgnore]
        public virtual int IconIndex
        {
            get { return 6; }
        }

        [Description("The actions that this object will perform."), Browsable(false)]
        protected List<Action> Actions
        {
            get { return this.actions_; }
            set { this.actions_ = value; }
        }

        [Browsable(false)]
        public Action CurrentAction
        {
            get { return (this.Actions.Count > 0 ? this.Actions[0] : null); }
        }

        [Description("The initial position of this object."), DefaultValue(Positions.Stopped)]
#if !DEBUG
        [Browsable(false)]
#endif
        public Positions Position
        {
            get { return this.position_; }
            set { this.position_ = value; }
        }

        [Description("The sex of this object, if applicable."), DefaultValue(Sex.None)]
        public Sex Gender
        {
            get { return this.sex_; }
            set { this.sex_ = value; }
        }

        [Description("How much this object is worth."), DefaultValue(0)]
        public int Value
        {
            get { return this.value_; }
            set { this.value_ = value; }
        }

        [Description("This objects initial status effects.")]
        [TypeConverter(typeof(ListConverter<StatusEffect>))]
        public List<StatusEffect> StatusEffects
        {
            get { return this.statusEffects_; }
            set { this.statusEffects_ = value; }
        }

        [Browsable(false)]
        public IFightSystem FightSystem
        {
            get { return this.fightSystem_; }
            set { this.fightSystem_ = value; }
        }

        [Browsable(false)]
        public DateTime LastAggressiveAct
        {
            get { return this.lastAggressiveAct_; }
            set { this.lastAggressiveAct_ = value; }
        }

        [Description("The size of this object. The size of an object is used to determine if it can fit within other objects, such as containers, as well as how much it can hold. A value of 0 means unlimited.")]
        [DefaultValue(0)]
        public int Size
        {
            get { return this.size_; }
            set { this.size_ = value; }
        }

        [Description("The amount of space remaining inside this object.")]
        [Browsable(false)]
        public int RemainingSpace
        {
            get
            {
                int total = 0;
                foreach (Object o in this.Children)
                    total += o.Size;
                return this.Size - total;
            }
        }

        [Description("The message that is broadcast to the room when this object is killed/destroyed.")]
        [DefaultValue("{1} has been destroyed!")]
        public string DeathMessage
        {
            get { return this.deathMessage_; }
            set { this.deathMessage_ = value; }
        }

        [Description("The length of time before this object dies. A value of 00:00:00 means infinite.")]
        public Damdai.Timer LifeSpan
        {
            get { return this.lifespan_; }
            set { this.lifespan_ = value; }
        }

        [Description("Whether or not this object's stats can regenerate.")]
        [DefaultValue(false)]
        public bool CanRegen
        {
            get { return this.canRegen_; }
            set { this.canRegen_ = value; }
        }

        [Browsable(false)]
        [Category("Closeable Properties")]
        [Description("Can this item be closed?")]
        [DefaultValue(false)]
        public bool Closeable
        {
            get { return this.closeable_ || (this.objectFlags_ != null && this.objectFlags_.Get((int)ObjFlags.Closeable)); }
            set
            {
                this.closeable_ = value;
                if (this.objectFlags_ != null)
                    this.objectFlags_.Set((int)ObjFlags.Closeable, value);
            }
        }

        [Browsable(false)]
        [Category("Closeable Properties")]
        [Description("Is this item closed?")]
        [DefaultValue(false)]
        public bool Closed
        {
            get { return this.closed_; }
            set { this.closed_ = value; }
        }

        [Browsable(false)]
        [Category("Closeable Properties")]
        [Description("Is this item locked?")]
        [DefaultValue(false)]
        public bool Locked
        {
            get { return this.locked_; }
            set { this.locked_ = value; }
        }

        [Browsable(false)]
        [Category("Closeable Properties")]
        [Description("The item that is required to lock and unlock this item.")]
        [Editor(typeof(Server.ObjectBlueprintSelector<Item>), typeof(UITypeEditor))]
        public Item Key
        {
            get { return Server.Instance.Get<Item>(this.keyId_); }
            set { this.keyId_ = value != null ? value.Id : Guid.Empty; }
        }
        
        [Browsable(false)]
        public bool Incapacitated
        {
            get { return (this.Position <= Positions.Sleeping); }
        }

        [Description("This text will be seen when someone looks at this object.")]
        [Editor(typeof(StringEditor), typeof(UITypeEditor))]
        public string Description
        {
            get { return this.description_; }
            set { this.description_ = value; }
        }

#if !DEBUG
        [Browsable(false)]
#endif
        public bool IsInside
        {
            get
            {
                Area area = this.Area;
                if (area == null)
                {
                    return false;
                }
                else
                {
                    Area.Layer layer = area.GetLayer<Area.Layer>(this.Location.Point, false);
                    return ((layer != null && layer.LayerFlags.Get((int)Area.Layer.LayerFlag.Indoors)) || area.AreaFlags.Get((int)Area.AreaFlag.Indoors));
                }
            }
        }

        public bool IsSwimming
        {
            get
            {
                Area area = this.Area;
                if (area == null || this.objectFlags_.Get((int)ObjFlags.Flying))
                {
                    return false;
                }
                else
                {
                    Area.Layer layer = area.GetLayer<Area.Layer>(this.Location.Point, false);
                    return ((layer != null && layer.LayerFlags.Get((int)Area.Layer.LayerFlag.Swim)) || (layer == null && area.AreaFlags.Get((int)Area.AreaFlag.Swim)));
                }
            }
        }

#if !DEBUG
        [Browsable(false)]
#endif
        public bool IsFlying
        {
            get { return this.objectFlags_.Get((int)ObjFlags.Flying); }
        }

#if !DEBUG
        [Browsable(false)]
#endif
        public Area.Layer OnLayer
        {
            get
            {
                if (this.Area == null)
                    return null;
                else
                    return this.Area.GetLayer<Area.Layer>(this.Location.Point, false, this as Area.Layer);
            }
        }
        
        public object this[object var]
        {
            get
            {
                object val;
                this.vars_.TryGetValue(var, out val);
                return val;
            }
            set
            {
                if (this.vars_.ContainsKey(var))
                    this.vars_[var] = value;
                else
                    this.vars_.Add(var, value);
            }
        }

        [Browsable(false)]
        public Dictionary<object, object> Vars
        {
            get { return this.vars_; }
        }

        [Editor(typeof(Server.MultiObjectSelector<Tag>), typeof(UITypeEditor))]
        [TypeConverter(typeof(Server.ObjectCollectionConverter<Tag>))]
        public List<Guid> Tags
        {
            get { return this.tags_; }
            set { this.tags_ = value; }
        }
        #endregion

        #region Events
        [Obfuscation(Exclude = false)]
        public delegate void MovedHandler(Object o, Area.Direction dir);
        public delegate void DieHandler(Object o);
        [Obfuscation(Exclude = false)]
        public delegate void AbilityPerformedHandler(Ability ability, Object actor, Object target, Ability.Status status);
        [Obfuscation(Exclude = false)]
        public delegate void NewObjectHandler(Object o);

        [Obfuscation(Exclude = false)]
        [field: NonSerialized]
        public event MovedHandler MovedEvent;
        [Obfuscation(Exclude = false)]
        [field: NonSerialized]
        public event AbilityPerformedHandler AbilityPerformedEvent;
        [Obfuscation(Exclude = false)]
        [field: NonSerialized]
        public static event NewObjectHandler NewObjectEvent;
        [Obfuscation(Exclude = false)]
        [field: NonSerialized]
        public event DieHandler DieEvent;

        [Obfuscation(Exclude = false)]
        void Moved(Area.Direction dir)
        {
            if (this.MovedEvent != null)
                MovedEvent(this, dir);
        }

        [Obfuscation(Exclude = false)]
        public void AbilityPerformed(Ability ability, Object target, Ability.Status status)
        {
            if (this.AbilityPerformedEvent != null)
                this.AbilityPerformedEvent(ability, this, target, status);
        }
        #endregion

        public Object()
        {
            if (Server.Instance != null)
                this.Id = Server.Instance.NewGuid();
            if (NewObjectEvent != null)
                NewObjectEvent(this);
        }

        public static Object Create(Object blueprint)
        {
            Object o = blueprint.MemberwiseClone() as Object;
            o.Init(blueprint);
            return o;
        }
        public void Init(Object other)
        {
            this.Id = Server.Instance.NewGuid();
            this.Children = new List<Object>();
            this.ObjectFlags = new BitArray(other.ObjectFlags);
            this.vars_ = new Dictionary<object, object>();
            if (other.LifeSpan != null)
                this.LifeSpan = new Damdai.Timer(other.LifeSpan.Length);

            this.Stats = new Dictionary<Guid, Stat>();
            lock (this.Stats)
            {
                lock (other.Stats)
                {
                    foreach (Stat stat in other.Stats.Values)
                        this.Stats.Add(stat.Id, stat.Clone() as Stat);
                }
            }

            this.StatusEffects = new List<StatusEffect>();
            lock (this.StatusEffects)
            {
                lock (other.StatusEffects)
                {
                    foreach (StatusEffect status in other.StatusEffects)
                        this.StatusEffects.Add(StatusEffect.Create(status));
                }
            }
        }
        public bool Move(Area.Direction direction, String leavemsg, String arrivemsg, bool force)
        {
            if (!(this.Parent is Area))
                return false;

            Point newPoint = Point.Empty;

            if (!force && this.position_ != Positions.Stopped && this.position_ != Positions.Moving)
            {
                this.Send("You can't move in that position.");
                return false;
            }

            switch (direction)
            {
                case Area.Direction.Random:
                    return this.Move((Area.Direction)Server.RandomNumber.Next(0, 8), leavemsg, arrivemsg, force);
                default:
                    newPoint = Area.GetNextPoint(this.Location.Point, direction);
                    break;
            }

            if (!this.CanMove(newPoint, force))
                return false;

            if (this.ObjectFlags.Get((int)ObjFlags.Flying))
                this.LeaveTrail();

            Point oldPoint = this.Location.Point;
            Area oldArea = this.Area;
            Area.Region oldRegion = (oldArea != null ? oldArea.GetLayer<Area.Region>(oldPoint, true) : null);

            if (leavemsg != null)
                this.Area.Send(leavemsg.Replace("{dir}", direction.ToString().ToLower()), this, null, 0);

            this.Location.Point = newPoint;

            if (arrivemsg != null)
                this.Area.Send(arrivemsg.Replace("{dir}", Area.ReverseDirection(direction).ToString().ToLower()), this, null, 0);

            Area.Region newRegion = (this.Area != null ? this.Area.GetLayer<Area.Region>(newPoint, true) : null);
            if (oldRegion != newRegion)
            {
                if (oldRegion != null)
                    oldRegion.OnObjectLeave(this);
                if (newRegion != null)
                    newRegion.OnObjectEnter(this);
            }

            this.Moved(direction);
            this.CheckAutoEnter();
            this.OnMoved();
            return true;
        }
        public virtual bool CanMove(Point location, bool force)
        {
            if (!(this.Parent is Area))
                return false;

            Area.Layer layer = this.Area.GetLayer<Area.Layer>(location, false);
            bool nowalk = (layer != null ? layer.LayerFlags.Get((int)Area.Layer.LayerFlag.NoWalk) : this.Area.AreaFlags.Get((int)Area.AreaFlag.NoWalk));
            bool nofly = (layer != null ? layer.LayerFlags.Get((int)Area.Layer.LayerFlag.NoFly) : this.Area.AreaFlags.Get((int)Area.AreaFlag.NoFly));
            bool swim = (layer != null ? layer.LayerFlags.Get((int)Area.Layer.LayerFlag.Swim) : this.Area.AreaFlags.Get((int)Area.AreaFlag.Swim));
            if (nowalk && !this.ObjectFlags.Get((int)ObjFlags.Flying))
                return false;
            if (nofly && this.ObjectFlags.Get((int)ObjFlags.Flying))
                return false;
            if (swim && !this.ObjectFlags.Get((int)ObjFlags.CanSwim) && !this.ObjectFlags.Get((int)ObjFlags.Flying))
                return false;
            if (layer != null)
            {
                object[] rvals = layer.DoScript(Area.Layer.LayerScriptType.OnObjectMove, this);
                if (rvals != null && rvals[0].ToString() == "cancel")
                    return false;
            }

            return true;
        }
        
        [Obfuscation(Exclude = false)]
        protected virtual void OnMoved() { }
        
        [Obfuscation(Exclude = false)]
        void CheckAutoEnter()
        {
            List<IPortal> portals = this.Area.GetChildren<IPortal>(this.Location.Point);
            if (portals != null && portals.Count > 0)
            {
                IPortal portal = portals[0];
                if (portal != null && portal.AutoEnter)
                {
                    this.Enter(portal);
                    return;
                }
            }
        }
        
        public Area.Direction GetDirection(Object o)
        {
            return this.GetDirection(o.Location.Point);
        }
        public Area.Direction GetDirection(Point p)
        {
            bool north = false, south = false, east = false, west = false;

            if (p.X > this.Location.Point.X)
                east = true;
            else if (p.X < this.Location.Point.X)
                west = true;

            if (p.Y < this.Location.Point.Y)
                north = true;
            else if (p.Y > this.Location.Point.Y)
                south = true;

            return (
                north && east ? Area.Direction.Northeast :
                north && west ? Area.Direction.Northwest :
                south && east ? Area.Direction.Southeast :
                south && west ? Area.Direction.Southwest :
                north ? Area.Direction.North :
                south ? Area.Direction.South :
                east ? Area.Direction.East :
                Area.Direction.West);
        }
        
        [Obfuscation(Exclude = false)]
        public List<T> GetChildren<T>() where T : class, IIdentifiable
        {
            List<T> list = new List<T>();
            List<Object> children = null;
            lock(this.children_)
                children = new List<Object>(this.children_);
            foreach (Object o in children)
                if (o is T && this.CanSee(o, false))
                    list.Add(o as T);
            return list;
        }
        
        public long SetStat(Guid id, long current, long max)
        {
            Stat stat;

            lock (this.Stats)
            {
                if (this.Stats.ContainsKey(id))
                    stat = this.Stats[id];
                else
                {
                    stat = new Stat(id);
                    this.Stats[id] = stat;
                }
            }

            if (max > -1)
                stat.Max = max;

            stat.Current = (current > stat.Max ? stat.Max : current);
            return stat.Current;
        }
        public long SetStat(Guid id, long current)
        {
            return this.SetStat(id, current, -1);
        }
        public long SetStat(string name, long current, long max)
        {
            StatBlueprint bp = Server.Instance.Get<StatBlueprint>(name);
            if (bp == null)
            {
                Trace.WriteLine("Stat does not exist: " + name, "ERROR");
                return 0;
            }
            return this.SetStat(bp.Id, current, max);
        }
        public long SetStat(string name, long current)
        {
            return this.SetStat(name, current, -1);
        }
        public Stat SetStat(Stat stat)
        {
            Stat s = this.GetStat(stat.Id);
            s.Max = stat.Max;
            s.Current = stat.Current;
            return s;
        }
        public Stat GetStat(Guid id)
        {
            lock (this.Stats)
            {
                if (!this.Stats.ContainsKey(id))
                    return null;
                return this.Stats[id];
            }
        }
        public Stat GetStat(string name)
        {
            StatBlueprint bp = Server.Instance.Get<StatBlueprint>(name);
            if (bp != null)
                return this.GetStat(bp.Id);
            else
                return null;
        }
        public long GetStatCurrent(string name)
        {
            Stat stat = this.GetStat(name);
            return (stat == null ? 0 : stat.Current);
        }
        public long GetStatMax(string name)
        {
            Stat stat = this.GetStat(name);
            return (stat == null ? 0 : stat.Max);
        }
        public bool IsAffected(string statuseffect)
        {
            return this.statusEffects_.Exists(delegate(StatusEffect effect)
            {
                return effect.Blueprint.Name.Equals(statuseffect, StringComparison.CurrentCultureIgnoreCase);
            });
        }
        public StatusEffect GetStatusEffect(Guid id)
        {
            return this.statusEffects_.Find(delegate(StatusEffect effect)
            {
                return (effect.BlueprintId == id);
            });
        }
        public void RemoveTemporaryStatusEffects()
        {
            foreach (StatusEffect status in Damdai.Collections.Util.CopyList<StatusEffect>(this.StatusEffects))
                if (!status.IsPermanent)
                    this.RemoveStatusEffect(status);
        }
        public void RemoveStatusEffect(StatusEffect effect)
        {
            lock (this.StatusEffects)
            {
                effect.Undo(this);
                this.StatusEffects.Remove(effect);
            }
        }
        public void RemoveStatusEffect(Guid id)
        {
            StatusEffect status = null;
            while ((status = this.GetStatusEffect(id)) != null)
                this.RemoveStatusEffect(status);
        }
        public int Distance(IObject o)
        {
            return Distance(o.Location.Point);
        }
        public virtual int Distance(Point p)
        {
            return System.Math.Abs(this.Location.Point.X - p.X) + System.Math.Abs(this.Location.Point.Y - p.Y);
        }

        [Obfuscation(Exclude = false)]
        public virtual bool Intersects(System.Drawing.Rectangle rect)
        {
            return rect.Contains(this.Location.Point);
        }
        
        [Obfuscation(Exclude = false)]
        public virtual void Draw(Area.Map map)
        {
            Point p = new Point(this.Location.Point.X - map.Offset.X, this.Location.Point.Y - map.Offset.Y);
            string mapSymbol = this.MapSymbol;
            if (mapSymbol.Length < 2)
                mapSymbol += " ";
            map.Grid[p.X, p.Y] = mapSymbol;
            map.Color[p.X, p.Y] = this.MapColor;
        }
        
        [Obfuscation(Exclude = false)]
        internal virtual void Draw(Graphics g, AreaCanvas canvas, AreaCanvas.View view)
        {
            if (canvas.Contains(this))
            {
                g.DrawImage(Ultimud.Properties.Resources.diamond, 
                    (this.Location.Point.X - canvas.Offset.X) * canvas.Zoom,
                    (this.Location.Point.Y - canvas.Offset.Y) * canvas.Zoom,
                    canvas.Zoom,
                    canvas.Zoom);
            }
        }
        
        public virtual void Send(string str, IObject actor)
        {
        }
        public virtual void Send(string str)
        {
            this.Send(str, null);
        }
        
        [Obfuscation(Exclude = false)]
        public virtual void Update()
        {
            if (this.position_ == Positions.None && this.Area != null)
                this.position_ = Positions.Stopped;

            if (this.Position != Positions.Dead)
            {
                this.UpdateAction();
                this.UpdateStatusEffects();
                this.UpdateStats();
            }
            this.UpdateChildren();

            if (this.LifeSpan != null && !this.LifeSpan.Update())
            {
                this.Die(null);
                this.LifeSpan = null;
            }
        }
        
        [Obfuscation(Exclude = false)]
        void UpdateAction()
        {
            lock (this.Actions)
            {
                Action action = this.CurrentAction;
                if (action != null)
                {
                    action.Update();
                }
            }
        }
        
        [Obfuscation(Exclude = false)]
        void UpdateStatusEffects()
        {
            lock (this.StatusEffects)
            {
                StatusEffect[] statusEffects = new StatusEffect[this.StatusEffects.Count];
                this.StatusEffects.CopyTo(statusEffects);
                foreach (StatusEffect status in statusEffects)
                {
                    status.Update(this);
                }
            }
        }
        
        [Obfuscation(Exclude = false)]
        void UpdateStats()
        {
            lock (this.Stats)
            {
                List<Stat> stats = new List<Stat>(this.Stats.Values);
                foreach (Stat stat in stats)
                {
                    stat.Update(this);
                }
            }
        }
        
        [Obfuscation(Exclude = false)]
        void UpdateChildren()
        {
            List<Object> children = new List<Object>(this.children_);
            foreach (Object o in children)
            {
                o.Update();
            }
        }
        
        public Object FindObject(string name, bool searchChildren)
        {
            return this.FindChild<Object>(name, false, searchChildren);
        }
        
        [Obfuscation(Exclude = false)]
        public T FindChild<T>(Guid id, bool searchChildren) where T : class, IIdentifiable
        {
            if (id == Guid.Empty)
                return null;

            lock (this.Children)
            {
                foreach (Object child in this.Children)
                {
                    //if (!this.CanSee(child))
                    //{
                    //    continue;
                    //}

                    if ((child.Id == id || (child is Item && ((Item)child).BlueprintId == id))&& child is T)
                        return child as T;
                    else if (searchChildren)
                    {
                        T t = child.FindChild<T>(id, true);
                        if (t != null)
                            return t;
                    }
                }
            }

            return null;
        }
        
        [Obfuscation(Exclude = false)]
        public T FindChild<T>(string name, bool sightlimited, bool searchChildren) where T : class, IIdentifiable
        {
            int num = 1;
            if (System.Text.RegularExpressions.Regex.IsMatch(name, @"^[0-9]+\..+"))
            {
                string[] parts = name.Split(".".ToCharArray(), 2);
                num = int.Parse(parts[0]);
                name = parts[1];
            }

            int count = 0;
            foreach (Object child in Damdai.Collections.Util.CopyList<Object>(this.Children))
            {
                if (sightlimited && !this.CanSee(child, false))
                    continue;

                if (child is T && String.FindArgument(child.Name, name) && ++count == num)
                    return child as T;
                else if (searchChildren)
                {
                    T t = child.FindChild<T>(name, sightlimited, true);
                    if (t != null)
                        return t;
                }
            }

            return null;
        }
        
        [Obfuscation(Exclude = false)]
        public T FindNearby<T>(string name, int rangemin, int rangemax, bool sightlimited) where T : class, IObject
        {
            if (!string.IsNullOrEmpty(name))
            {
                int num = 1;
                int count = 0;
                if (System.Text.RegularExpressions.Regex.IsMatch(name, @"^[0-9]+\..+"))
                {
                    string[] parts = name.Split(".".ToCharArray(), 2);
                    num = int.Parse(parts[0]);
                    name = parts[1];
                }

                foreach (T t in this.NearbyObjects<T>(this.location_.Point, rangemin, rangemax, sightlimited, null))
                    if (String.FindArgument(t.Name, name) && ++count == num)
                        return t;
            }
            return null;
        }
        
        [Obfuscation(Exclude = false)]
        public T FindNearby<T>(string name, int range, bool sightlimited) where T : class, IObject
        {
            return this.FindNearby<T>(name, 0, range, sightlimited);
        }
        
        [Obfuscation(Exclude = false)]
        public T FindNearby<T>(string name, int[] range, bool sightlimited) where T : class, IObject
        {
            return this.FindNearby<T>(name, range[0], range[1], sightlimited);
        }
        
        public Object FindNearby(string type, string name, int rangemin, int rangemax, bool sightlimited)
        {
            if (!type.StartsWith("Ultimud."))
                type = "Ultimud." + type;

            int num = 1;
            if (System.Text.RegularExpressions.Regex.IsMatch(name, @"^[0-9]+\..+"))
            {
                string[] parts = name.Split(".".ToCharArray(), 2);
                num = int.Parse(parts[0]);
                name = parts[1];
            }

            int count = 0;
            List<Object> list = Damdai.Collections.Util.CopyList<Object>(this.Area.Children);
            list.Sort(new Comparison<Object>(
                delegate(Object t1, Object t2)
                {
                    int d1 = this.Distance(t1);
                    int d2 = this.Distance(t2);
                    if (d1 > d2)
                        return 1;
                    else if (d1 < d2)
                        return -1;
                    else
                        return 0;
                }));            
            foreach (Object o in list)
            {
                int distance = o.Distance(this);
                if (distance >= rangemin && distance <= rangemax && (o.GetType().FullName == type || o.GetType().IsSubclassOf(Type.GetType(type))) && String.FindArgument(o.Name, name) && (!sightlimited || this.CanSee(o, true)) && ++count == num)
                    return o;
            }
            return null;
        }
        public Object FindNearby(string type, string name, int range, bool sightlimited)
        {
            return this.FindNearby(type, name, 0, range, sightlimited);
        }
        public List<Object> GetNearbyObjects(string type, int range, bool sightlimited)
        {
            List<Object> list = new List<Object>();
            foreach (Object o in Damdai.Collections.Util.CopyList<Object>(this.Area.Children))
                if (o.GetType().Name == type && o.Distance(this) <= range && (!sightlimited || this.CanSee(o, true)))
                    list.Add(o);
            return list;
        }
        public void EnqueueAction(Action action)
        {
            this.Actions.Add(action);
        }
        public void StartNewAction(Action action)
        {
            if (this.CurrentAction != null)
            {
                if (!this.CurrentAction.Wait())
                {
                    return;
                }
            }
            this.Actions.Insert(0, action);
            action.Start();
        }
        public void ReplaceCurrentAction(Action action)
        {
            if (this.CurrentAction != null)
                if (!this.CurrentAction.Stop(false))
                    return;

            this.Actions.Insert(0, action);
            action.Start();
        }
        public void RemoveAction(Action action)
        {
            if (action == null)
                return;

            bool startNext = false;
            if (this.CurrentAction == action)
                startNext = true;

            this.Actions.Remove(action);
            if (this.CurrentAction != null && startNext)
                this.CurrentAction.Start();
        }
        public void RemoveAction(Type type)
        {
            while (true)
            {
                bool found = false;
                for (int x = 0; x < this.Actions.Count; ++x)
                {
                    if (this.Actions[x].GetType() == type)
                    {
                        found = true;
                        this.RemoveAction(this.Actions[x]);
                        break;
                    }
                }

                if (!found)
                {
                    break;
                }
            }
        }
        public void Stop(bool silent)
        {
            Action a = CurrentAction;
            if (a == null)
            {
                if(!silent)
                    this.Send("You aren't doing anything.");
            }
            else
                a.Stop(silent);
        }
        public void Wait()
        {
            Action a = CurrentAction;
            if (a == null)
                this.Send("You aren't doing anything.");
            else
                a.Wait();
        }
        public void Go()
        {
            Action a = CurrentAction;
            if (a == null)
                this.Send("You weren't doing anything.");
            else
                a.Go();
        }
        public string GenderFilter(string none, string male, string female)
        {
            switch (this.sex_)
            {
                case Sex.Male: return male;
                case Sex.Female: return female;
                case Sex.None:
                default:
                    return none;
            }
        }
        public string GenderFilter(string[] vars)
        {
            return this.GenderFilter(vars[0], vars[1], vars[2]);
        }
        virtual public void Die(Object killer)
        {
            if (this == Server.Instance.MainArea)
                return;

            Area area = this.Area;
            if (area != null)
            {
                String str = new String(null, null, this.DeathMessage, this.DeathMessage, this.DeathMessage, null);
                area.Send(str, this, killer, 0, null, null);
            }

            if (this.Position != Positions.Dead)
            {
                lock (this.Actions)
                    this.Actions.Clear();
                this.RemoveTemporaryStatusEffects();
                this.Position = Positions.Dead;
                if (this.DieEvent != null)
                    this.DieEvent(this);
            }
        }
        virtual public void Faint(Object cause)
        {
            if (this.Position > Positions.Unconscious)
            {
                lock (this.Actions)
                    this.Actions.Clear();

                this.RemoveTemporaryStatusEffects();
                this.Position = Positions.Unconscious;
            }
        }
        public void Restore(bool silent)
        {
            //this.Position = Positions.Stopped;
            lock (this.Stats)
            {
                foreach (Stat stat in this.Stats.Values)
                {
                    StatBlueprint bp = stat.Blueprint;
                    if (bp != null && bp.CanRestore && stat.Current < stat.Max)
                        stat.Current = stat.Max;
                }
            }
            if(!silent)
                this.Send("You have been restored!");
        }
        public void Enter(Vehicle vehicle)
        {
            if (vehicle == null)
                return;

            if (this.Parent == vehicle)
            {
                if (vehicle.Closed)
                {
                    this.Send(String.Capitalize(vehicle.Name) + " is closed.");
                    return;
                }
                Location exitLoc = vehicle.Location;
                this.Location = exitLoc;
                if (this.Area != null)
                {
                    String msg = new String("You disembark from {2}.", "You disembark from {2}.", null, null, "{1} disembarks from {2}.", null);
                    this.Area.Send(msg, this, vehicle, 0);
                }
                return;
            }

            if (vehicle.Closed)
            {
                this.Send(String.Capitalize(vehicle.Name) + " is closed.");
                return;
            }
            if (vehicle.Locked)
            {
                this.Send(String.Capitalize(vehicle.Name) + " is locked.");
                return;
            }
            if (vehicle.Capacity > 0 && vehicle.GetChildren<Character>().Count >= vehicle.Capacity)
            {
                this.Send(String.Capitalize(vehicle.Name) + " is full.");
                return;
            }

            if (this.Area != null)
            {
                String msg = new String("You board {2}.", "You board {2}.", null, null, "{1} boards {2}.", null);
                this.Area.Send(msg, this, vehicle, 0);
            }
            this.location_ = vehicle.Location;
            this.Parent = vehicle;
        }
        public void Enter(IPortal portal)
        {
            Object o = portal as Object;

            if (portal.Closed)
            {
                this.Send(String.Capitalize(o.Name) + " is closed.");
                return;
            }
            else if (portal.Locked)
            {
                this.Send(String.Capitalize(o.Name) + " is locked.");
                return;
            }

            Area area = portal.Destination.Area;
            if (area != null)
            {
                String msg = portal.EnterMessage;
                if (msg == null || msg.Empty)
                    msg = new String("You enter {2}.", "You enter {2}.", null, null, "{1} enters {2}.", null);
                this.Area.Send(msg, this, o, 0);
                this.Location = portal.Destination;
                msg = portal.ExitMessage;
                if (msg == null)
                    msg = new String(null, null, null, null, "{1} has arrived from {2}.", null);
                this.Area.Send(msg, this, o, 0);
                this.Moved(Area.Direction.Random);
            }
            else
            {
                Debug.WriteLine("WARNING: Portal destination is null!");
                this.Send(String.Capitalize(o.Name) + " does not lead anywhere.");
            }
        }
        public void Destroy(Object o)
        {
            if (o.ObjectFlags.Get((int)ObjFlags.Indestructible))
            {
                this.Send("You can't destroy that.");
                return;
            }

            String str = new String(
                "You destroy {2}!", "You destroy {2}!",
                "{1} destroys you!", "{1} destroys you!", 
                "{1} destroys {2}!", null);
            this.Area.Send(str, this, o, 0);
            o.Die(this);
            o.Parent = null;
        }
        public void MergeFightSystems(Object target)
        {
            if (target == null || Server.Instance.FightSystem == null)
                return;

            this.lastAggressiveAct_ = target.lastAggressiveAct_ = DateTime.Now;

            if (this.fightSystem_ == null && target.fightSystem_ == null)
            {
                this.fightSystem_ = Server.Instance.FightSystem.Clone() as IFightSystem;
                if (this.fightSystem_ != null)
                {
                    this.fightSystem_.AddParticipants(this, target);
                    this.fightSystem_.Start();
                }
            }
            else if (this.fightSystem_ != null && target.fightSystem_ == null)
                this.fightSystem_.AddParticipants(this, target);
            else if (this.fightSystem_ == null && target.fightSystem_ != null)
                target.fightSystem_.AddParticipants(this, target);
            else if (this.fightSystem_ != target.fightSystem_)
                this.fightSystem_.Merge(target.fightSystem_);
        }
        public string PrintChildList(bool tree, string prefix)
        {
            string str = string.Empty;
            foreach (Object child in this.GetChildren<Object>())
                str += prefix + child.Name + "\r\n";
            return str;
        }
        
        [Obfuscation(Exclude = false)]
        public List<T> NearbyObjects<T>(Point location, int minDistance, int maxDistance, bool sightlimited, ObjFlags[] flags, params Object[] ignore) where T : class, IObject
        {
            List<T> objects = new List<T>();

            if (this.Area == null)
                return objects;

            foreach (Object o in Damdai.Collections.Util.CopyList<Object>(this.Area.Children))
            {
                if (!(o is T) || string.IsNullOrEmpty(o.Name) || (sightlimited && !this.CanSee(o, false)))
                    continue;

                int distance = o.Distance(location);
                if (distance <= maxDistance && distance >= minDistance)
                {
                    bool skip = false;
                    if (flags != null)
                    {
                        foreach (ObjFlags f in flags)
                        {
                            if (!o.ObjectFlags.Get((int)f))
                            {
                                skip = true;
                                break;
                            }
                        }
                    }

                    if (skip)
                        continue;

                    foreach (Object ignoreObject in ignore)
                    {
                        if (ignoreObject == o)
                        {
                            skip = true;
                            break;
                        }
                    }

                    if (skip)
                        continue;

                    objects.Add(o as T);
                }
            }
            objects.Sort(new Comparison<T>(
                delegate(T t1, T t2)
                {
                    int d1 = this.Distance(t1);
                    int d2 = this.Distance(t2);
                    if (d1 > d2)
                        return 1;
                    else if (d1 < d2)
                        return -1;
                    else
                        return 0;
                }));
            return objects;
        }
        
        [Obfuscation(Exclude = false)]
        public List<T> NearbyObjects<T>(int range, bool sightlimited, params Object[] ignore) where T : class, IObject
        {
            return this.NearbyObjects<T>(this.Location.Point, 0, range, sightlimited, null, ignore);
        }
        
        [Obfuscation(Exclude = false)]
        public List<T> NearbyObjects<T>(int[] range, bool sightlimited, params Object[] ignore) where T : class, IObject
        {
            return this.NearbyObjects<T>(this.Location.Point, range[0], range[1], sightlimited, null, ignore);
        }
        
        [Obfuscation(Exclude = false)]
        public bool Exists<T>(int mindistance, int maxdistance, bool sightlimited)
        {
            if (this.Area == null)
            {
                Debug.WriteLine("Area is null.", "WARNING");
                return false;
            }

            List<Object> list = new List<Object>(this.Area.Children);
            foreach (Object o in list)
            {
                if (!(o is T) || (sightlimited && !this.CanSee(o, false)))
                    continue;

                int distance = o.Distance(this);
                if (distance <= maxdistance && distance >= mindistance)
                    return true;
            }
            return false;
        }
        
        public void LeaveTrail()
        {
            Item item = new Item();
            item.DeathMessage = null;
            item.MapSymbol = "~ ";
            item.Location = this.Location;
            item.Parent = this.Parent;
            item.LifeSpan = new Damdai.Timer(new TimeSpan(0, 0, 5));
        }
        public bool CanHear()
        {
            return (this.Position > Positions.Sleeping);
        }
        public static bool CanHear(IObject o)
        {
            return o.CanHear();
        }
        public bool CanFit(IContainer container)
        {
            return (container.RemainingSpace - this.Size >= 0);
        }
        public virtual bool CanSee(Object o, bool fogOfWar)
        {
            if (o.ObjectFlags.Get((int)Object.ObjFlags.Invisible))
            {
                return false;
            }

            if (!(o is Area.Layer))
            {
                if (o.Parent != this && !this.CanSee(o.Location.Point))
                {
                    return false;
                }
            }
            else
            {
                return this.CanSee(o as Area.Layer, fogOfWar);
            }

            Area.Layer layer = o.OnLayer;
            if (layer != null && layer.LayerFlags.Get((int)Area.Layer.LayerFlag.Indoors) && layer.CeilingChar > 0 && !layer.Contains(this.Location.Point))
            {
                return false;
            }

            return true;
        }
        bool CanSee(Area.Layer layer, bool fogOfWar)
        {
            Stat sight = this.GetStat("sight");
            if (sight != null)
            {
                int dist = (int)sight.Current;
                for (int x = this.Location.Point.X - dist; x <= this.Location.Point.X + dist; ++x)
                {
                    for (int y = this.Location.Point.Y - dist; y <= this.Location.Point.Y + dist; ++y)
                    {
                        Point p = new Point(x, y);
                        if (layer.Contains(p) && (fogOfWar || this.CanSee(p)))
                            return true;
                    }
                }
            }
            return false;
        }
        public virtual bool CanSee(Point p)
        {
            int distance = this.Distance(p);

            Stat sight = this.GetStat("sight");
            if (sight != null && distance > sight.Current)
            {
                return false;
            }

            if (this.Area.LineOfSight)
            {
                int height = 0;
                Area.Layer l = this.OnLayer;
                if (l != null)
                {
                    height = l.Height;
                }
                List<Point> line = Damdai.Math.Line(this.Location.Point, p);
                List<Area.Layer> layers = this.Area.GetChildren<Area.Layer>(this.Location.Point, distance);
                foreach (Area.Layer layer in layers)
                {
                    if (layer.Height > height && layer.Contains(line))
                    {
                        return false;
                    }
                }
            }

            return true;
        }
        public void SetLifeSpan(int days, int hours, int minutes, int seconds)
        {
            this.lifespan_ = new Damdai.Timer(new TimeSpan(days, hours, minutes, seconds));
        }
        public object GetVar(object var)
        {
            return this[var];
        }
        public void SetVar(object var, object val)
        {
            this[var] = val;
        }
        public bool RemVar(object var)
        {
            return this.vars_.Remove(var);
        }
        public void ApplyStatMod(string statname, int mod, bool applytocurrent, bool applytomax, bool silent)
        {
            if (applytomax || applytocurrent)
            {
                using (StatModifier statmod = new StatModifier())
                {
                    statmod.ApplyTo = (applytocurrent && applytomax ? StatModifier.ApplyTarget.CurrentAndMax : applytocurrent ? StatModifier.ApplyTarget.Current : StatModifier.ApplyTarget.Max);
                    Stat stat = this.GetStat(statname);
                    if (stat == null)
                        throw new Exception("Stat not found: " + statname);
                    statmod.StatId = stat.Id;
                    statmod.Apply(this, null, null, null, "return " + mod, silent);
                }
            }
        }

        [Obfuscation(Exclude = false)]
        protected virtual void OnChildAdded(Object child) { }
        
        [Obfuscation(Exclude = false)]
        protected virtual void OnChildRemoved(Object child) { }
        
        public void GoTo(Area area, int x, int y)
        {
            this.Location = new Location(area, new Point(x, y));
            this.CheckAutoEnter();
        }
        public void GoTo(string areaname, int x, int y)
        {
            Area area = Server.Instance.GetArea(areaname);
            if (area != null)
                this.GoTo(area, x, y);
            else
                Trace.WriteLine("Area not found: " + areaname, "WARNING");
        }
        public void GoTo(Area area)
        {
            this.GoTo(area, area.Start.X, area.Start.Y);
        }
        public void GoTo(string areaname)
        {
            Area area = Server.Instance.GetArea(areaname);
            if (area != null)
                this.GoTo(area);
            else
                Trace.WriteLine("Area not found: " + areaname, "WARNING");
        }
        public bool ContainsTag(string tagname)
        {
            Tag tag = Server.Instance.Get<Tag>(tagname);
            return (tag != null && this.Tags.Contains(tag.Id));
        }

        #region Trigger Actions
        public static void Die(Object victim, Object killer)
        {
            victim.Die(killer);
        }
        public static void Faint(Object victim, Object actor)
        {
            victim.Faint(actor);
        }
        public static void Restore(Object target, Object na)
        {
            target.Restore(false);
        }
        public static void LevelUp(Object target, Object na)
        {
            target.LevelUp();
        }
        public void LevelUp()
        {
            Server.Instance.Settings.LevelUpSettings.Apply(this);
        }
        
        [Obfuscation(Exclude = false)]
        public static List<MethodInfo> TriggerActions
        {
            get
            {
                MethodInfo[] methods = typeof(Object).GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                List<MethodInfo> triggerActions = new List<MethodInfo>();
                foreach (MethodInfo method in methods)
                {
                    ParameterInfo[] parameters = method.GetParameters();
                    if (parameters.Length == 2 && parameters[0].ParameterType == typeof(Object) && parameters[1].ParameterType == typeof(Object))
                        triggerActions.Add(method);
                }
                return triggerActions;
            }
        }
        
        [Obfuscation(Exclude = false)]
        public static string[] TriggerActionNames
        {
            get
            {
                List<MethodInfo> triggerActions = Object.TriggerActions;
                if (triggerActions != null)
                {
                    string[] names = new string[triggerActions.Count];
                    int count = 0;
                    foreach (MethodInfo mi in triggerActions)
                        names[count++] = mi.Name;
                    return names;
                }
                else
                {
                    return null;
                }
            }
        }
        
        [Obfuscation(Exclude = false)]
        public static MethodInfo GetTriggerAction(string name)
        {
            MethodInfo[] methods = typeof(Object).GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (MethodInfo method in methods)
            {
                ParameterInfo[] parameters = method.GetParameters();
                if (parameters.Length == 2 && parameters[0].ParameterType == typeof(Object) && parameters[1].ParameterType == typeof(Object) && method.Name == name)
                    return method;
            }
            return null;
        }

        [TypeConverter(typeof(TriggerActionConverter))]
        public delegate void TriggerAction(Object self, Object other);
        
        [Obfuscation(Exclude = false)]
        internal class TriggerActionConverter : TypeConverter
        {
            public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                return true;
            }
            public override System.ComponentModel.TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                string[] triggerActionNames = Object.TriggerActionNames;
                return new StandardValuesCollection(triggerActionNames);
            }
            public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
            {
                if (sourceType == typeof(string) || sourceType == typeof(TriggerAction))
                    return true;
                return base.CanConvertFrom(context, sourceType);
            }
            public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
            {
                return true;
            }
            public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
            {
                if (value is string)
                {
                    MethodInfo mi = Object.GetTriggerAction(value as string);
                    if (mi != null)
                        return Delegate.CreateDelegate(typeof(Object.TriggerAction), mi) as Object.TriggerAction;
                    else
                        return null;
                }
                else if (value is TriggerAction)
                {
                    Object.TriggerAction a = value as Object.TriggerAction;
                    return a.Method.Name;
                }

                return base.ConvertFrom(context, culture, value);
            }
            public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
            {
                if (value != null && value is TriggerAction && destinationType == typeof(string))
                {
                    Object.TriggerAction a = value as Object.TriggerAction;
                    return a.Method.Name;
                }
                else if (value is string)
                    return value;
                return null;// base.ConvertTo(context, culture, value, destinationType);
            }
        }
        #endregion

        [OnSerializing]
        [Obfuscation(Exclude = false)]
        internal void OnSerializing(StreamingContext context)
        {
            this.parentId_ = this.ParentId;
            this.savedvars_ = new Dictionary<object,object>();
            lock (this.vars_)
            {
                foreach (KeyValuePair<object, object> pair in this.vars_)
                    if ((pair.Key is ValueType || pair.Key is string) && (pair.Value is ValueType || pair.Value is string))
                        this.savedvars_.Add(pair.Key, pair.Value);
            }
            if (this.savedvars_.Count == 0)
                this.savedvars_ = null;
        }
        
        [OnDeserialized]
        [Obfuscation(Exclude = false)]
        internal void OnDeserialized(StreamingContext context)
        {
            if (this.parentId_ != Guid.Empty)
                this.Parent = Server.Instance.Find<Object>(this.parentId_);

            if (this.location_.Area == null && this.parent_ != null)
                this.location_.Area = (this.parent_ is Area ? this.parent_ as Area : this.parent_.Area);

            foreach (Object o in this.Children)
                o.parent_ = this;

            if (this.lifeSpan_.Ticks > 0)
            {
                this.LifeSpan = new Damdai.Timer(this.lifeSpan_);
                this.lifeSpan_ = TimeSpan.Zero;
            }

            if (this.statusEffects_ == null)
                this.statusEffects_ = new List<StatusEffect>();

            if (this.savedvars_ != null)
            {
                this.savedvars_.OnDeserialization(null);
                this.vars_ = new Dictionary<object, object>(this.savedvars_);
            }
            else
                this.vars_ = new Dictionary<object, object>();

            if (this.tags_ == null)
                this.tags_ = new List<Guid>();
        }
    }
}

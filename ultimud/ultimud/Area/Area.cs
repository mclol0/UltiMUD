using System;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;
using System.Drawing.Design;
using System.Diagnostics;
using System.Reflection;
using Damdai.TypeConverters;

namespace Ultimud
{
    [Serializable]
    [Obfuscation(Exclude = true)]
    public partial class Area : Object, IContainer, IPortal
    {
        #region Enums
        public enum AreaFlag
        {
            StartingArea,
            Indoors,
            NoWalk,
            NoFly,
            Swim,
            NoAttack
        }
        public enum Direction
        {
            North,
            South,
            East,
            West,
            Northeast,
            Northwest,
            Southeast,
            Southwest,
            //Up,
            //Down,
            Random
        }
        public enum ShortDirection
        {
            N,
            S,
            E,
            W,
            NE,
            NW,
            SE,
            SW
        }
        public enum Range
        {
            Self = -1,
            Room = 0,
            Nearby = 3,
            Far = 6,
            Area = -2,
            World = -3
        }
        #endregion

        #region Members
        char fillChar_ = ' ';
        Telnet.Color fillColor_ = Telnet.Color.Reset;
        Point start_ = new Point(0, 0);
        BitArray areaFlags_ = new BitArray(16);
        List<Reset> resets_ = new List<Reset>();
        BackgroundImageSettings backgroundImageSettings_ = new BackgroundImageSettings();
        WeatherConditions weather_ = new WeatherConditions();
        char fillChar2_;
        Telnet.Color fillColor2_ = Telnet.Color.Reset;
        int fillChar2Percent_;
        String enterMessage_ = new String("You enter {2}.", "You enter {2}.", null, null, "{1} enters {2}.", null);
        bool autoEnter_;
        bool los_;
        bool fow_;
        String exitMessage_ = new String(null, null, null, null, "{1} has arrived from {2}.", null);
        List<StatusEffect> applystatuseffects_ = new List<StatusEffect>();
        #endregion

        #region Properties
        [Category("Appearance")]
        [DisplayName("FillChar1")]
        [Description("The ASCI character that will be used to fill this area on the map.")]
        public char FillChar
        {
            get { return this.fillChar_; }
            set
            {
                if (value == 0)
                    this.fillChar_ = ' ';
                else
                    this.fillChar_ = value;
            }
        }

        [Category("Appearance")]
        [DisplayName("FillChar1Color")]
        [Description("The color of the FillChar."), DefaultValue(Telnet.Color.Reset)]
        public Telnet.Color FillColor
        {
            get { return this.fillColor_; }
            set { this.fillColor_ = value; }
        }

        [Category("Appearance")]
        [Description("A secondary ASCI character that will be used to fill this area on the map.")]
        public char FillChar2
        {
            get { return this.fillChar2_; }
            set { this.fillChar2_ = value; }
        }

        [Category("Appearance")]
        [DisplayName("FillChar2Color")]
        [Description("The color of FillChar2.")]
        [DefaultValue(Telnet.Color.Reset)]
        public Telnet.Color FillColor2
        {
            get { return this.fillColor2_; }
            set { this.fillColor2_ = value; }
        }

        [Category("Appearance")]
        [Description("The percentage of the area to fill using FillChar2.")]
        [DefaultValue(0)]
        public int FillChar2Percent
        {
            get { return this.fillChar2Percent_; }
            set { this.fillChar2Percent_ = value; }
        }

        [Description("Various settings that effect the behavior of this area.")]
        [Editor(typeof(FlagEditor<AreaFlag>), typeof(UITypeEditor))]
        [TypeConverter(typeof(Damdai.TypeConverters.BitArrayConverter<AreaFlag>))]
        public BitArray AreaFlags
        {
            get { return this.areaFlags_; }
            set { this.areaFlags_ = value; }
        }

        [Browsable(false), XmlIgnore]
        public override int IconIndex
        {
            get { return 0; }
        }

        [Browsable(false)]
        new public Sex Gender
        {
            get { return base.Gender; }
            set { base.Gender = value; }
        }

        [Browsable(false)]
        new public Positions Position
        {
            get { return base.Position; }
            set { base.Position = value; }
        }

        [Browsable(true)]
        new public Location Location
        {
            get { return base.Location; }
            set { base.Location = value; }
        }

        [Browsable(false)]
        public List<Reset> Resets
        {
            get
            {
                if (this.resets_ == null)
                    this.resets_ = new List<Reset>();
                return this.resets_;
            }
        }

        [Description("Can this area be closed?")]
        [Browsable(true)]
        public new bool Closeable
        {
            get { return base.Closeable; }
            set { base.Closeable = value; }
        }

        [Description("Is the entrance to this area closed?")]
        [Browsable(true)]
        public new bool Closed
        {
            get { return base.Closed; }
            set { base.Closed = value; }
        }

        [Description("Is the entrance to this area locked?")]
        [Browsable(true)]
        public new bool Locked
        {
            get { return base.Locked; }
            set { base.Locked = value; }
        }

        [Description("The item that is required to lock and unlock the entrance to this area.")]
        [Browsable(true)]
        public new Item Key
        {
            get { return base.Key; }
            set { base.Key = value; }
        }

        [Description("Various settings for the background image to use for this area in the area editor.")]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public BackgroundImageSettings BackgroundImageSettings
        {
            get
            {
                if (this.backgroundImageSettings_ == null)
                    this.backgroundImageSettings_ = new BackgroundImageSettings();
                return this.backgroundImageSettings_;
            }
            set { this.backgroundImageSettings_ = value; }
        }

        [DisplayName("Start")]
        [Description("The starting location of all things that first enter this area.")]
        public Point Start
        {
            get { return this.start_; }
            set { this.start_ = value; }
        }

        [Browsable(false)]
        public Location Destination
        {
            get { return new Location(this, this.start_); }
            set { }
        }

        [Description("The weather conditions of this area.")]
        public WeatherConditions WeatherConditions
        {
            get { return this.weather_; }
            set { this.weather_ = value; }
        }

        public String EnterMessage
        {
            get { return this.enterMessage_; }
            set { this.enterMessage_ = value; }
        }
        public String ExitMessage
        {
            get { return this.exitMessage_; }
            set { this.exitMessage_ = value; }
        }

        [Description("If true, any character that travels over the entrance to this area will automatically enter it. Otherwise, they must enter manually.")]
        [DefaultValue(false)]
        public bool AutoEnter
        {
            get { return this.autoEnter_; }
            set { this.autoEnter_ = value; }
        }

        [Category("Visual Effects")]
        [Description("Should the visible areas on the map be restricted by the observer's line of sight?")]
        [DefaultValue(false)]
        public bool LineOfSight
        {
            get { return this.los_; }
            set { this.los_ = value; }
        }

        [Category("Visual Effects")]
        [Description("If this is enabled, the areas that aren't in your line of sight will be gray. Otherwise, they will be blank.")]
        [DefaultValue(false)]
        public bool FogOfWar
        {
            get { return this.fow_; }
            set { this.fow_ = value; }
        }

        [Browsable(false)]
        public new int Size
        {
            get { return base.Size; }
            set { base.Size = value; }
        }

        [Browsable(false)]
        public new Damdai.Timer LifeSpan
        {
            get { return base.LifeSpan; }
            set { base.LifeSpan = value; }
        }

        [Browsable(false)]
        public new int Value
        {
            get { return base.Value; }
            set { base.Value = value; }
        }

        [Browsable(false)]
        public new Dictionary<Guid, Stat> Stats
        {
            get { return base.Stats; }
            set { base.Stats = value; }
        }

        [Browsable(false)]
        public new List<StatusEffect> StatusEffects
        {
            get { return base.StatusEffects; }
            set { base.StatusEffects = value; }
        }

        [Browsable(false)]
        public new bool CanRegen
        {
            get { return base.CanRegen; }
            set { base.CanRegen = value; }
        }

        [Browsable(false)]
        public new string DeathMessage
        {
            get { return base.DeathMessage; }
            set { base.DeathMessage = value; }
        }

        [DisplayName("StatusEffects")]
        [Description("These status effects will automatically be applied to objects that enter this area.")]
        [TypeConverter(typeof(ListConverter<StatusEffect>))]
        public List<StatusEffect> ApplyStatusEffects
        {
            get { return this.applystatuseffects_; }
            set { this.applystatuseffects_ = value; }
        }
        #endregion

        #region Methods
        public Area()
        {
            this.ObjectFlags.Set((int)ObjFlags.Indestructible, true);
            this.ObjectFlags.Set((int)ObjFlags.Immovable, true);
        }
        public Area(string name)
            : this()
        {
            this.Name = name;
            this.MapSymbol = "[]";
            this.MapColor = Telnet.Color.BrightBlue;
            lock (this.Stats)
            {
                this.Stats.Clear();
            }
        }

        public Point GetRandomLocation(Object o, Point origin, int distance)
        {
            List<Point> locations = new List<Point>();
            Point p = new Point(0, 0);
            for (int y = origin.Y - distance; y <= origin.Y + distance; ++y)
            {
                for (int x = origin.X - distance; x <= origin.X + distance; ++x)
                {
                    p.X = x;
                    p.Y = y;
                    if (p != origin && o.CanMove(p, false))
                        locations.Add(p);
                }
            }
            return locations[Server.RandomNumber.Next(locations.Count)];
        }

        public static Point GetNextPoint(Point start, Direction dir)
        {
            Point newPoint = start;
            switch (dir)
            {
                case Area.Direction.North: --newPoint.Y; break;
                case Area.Direction.South: ++newPoint.Y; break;
                case Area.Direction.West: --newPoint.X; break;
                case Area.Direction.East: ++newPoint.X; break;
                case Area.Direction.Northeast: --newPoint.Y; ++newPoint.X; break;
                case Area.Direction.Northwest: --newPoint.Y; --newPoint.X; break;
                case Area.Direction.Southeast: ++newPoint.Y; ++newPoint.X; break;
                case Area.Direction.Southwest: ++newPoint.Y; --newPoint.X; break;
            }

            return newPoint;
        }
        
        public List<Reset> GetResets(Point point)
        {
            List<Reset> resets = new List<Reset>();
            foreach (Reset reset in this.Resets)
            {
                if (reset.Location.Point == point)
                {
                    resets.Add(reset);
                }
            }
            return resets;
        }

        [Obfuscation(Exclude = false)]
        public List<T> GetChildren<T>(Point point, int range) where T : class, IObject
        {
            List<T> children = new List<T>();
            foreach (IObject child in this.GetChildren<T>())
            {
                if (range < 0 ||(child is Area.Layer ? (((Area.Layer)child).Contains(point)) :
                    (child.Location.Point == point)) || child.Distance(point) <= range)
                {
                    children.Add(child as T);
                }
            }
            return children;
        }
        
        [Obfuscation(Exclude = false)]
        public List<T> GetChildren<T>(Point point) where T : class, IObject
        {
            return this.GetChildren<T>(point, 0);
        }
        
        [Obfuscation(Exclude = false)]
        public T FindChild<T>(string name, Point location, int distance) where T : Object, new()
        {
            if (name != null)
            {
                name = name.ToLower();
                foreach (T t in this.GetChildren<T>(location, distance))
                    if (String.FindArgument(t.Name, name))
                        return t;
            }
            return null;
        }
        
        static public Direction ReverseDirection(Direction d)
        {
            switch (d)
            {
                case Direction.North: return Direction.South;
                case Direction.South: return Direction.North;
                case Direction.East: return Direction.West;
                case Direction.West: return Direction.East;
                case Direction.Northeast: return Direction.Southwest;
                case Direction.Northwest: return Direction.Southeast;
                case Direction.Southeast: return Direction.Northwest;
                case Direction.Southwest: return Direction.Northeast;
                //case Direction.Up:	return Direction.Down;
                //case Direction.Down:	return Direction.Up;
            }
            return d;
        }

        public override void Send(string str)
        {
            this.Send(str, false, false);
        }
        
        public void Send(string str, bool indoors, bool unconscious)
        {
            lock (this.Children)
            {
                foreach (Object o in this.Children)
                    if ((indoors || !o.IsInside) && (unconscious || !o.Incapacitated))
                        o.Send(str);
            }
        }
        
        [Obfuscation(Exclude = false)]
        public void Send(string str, Point p, int distance, IObject firstPerson, Damdai.Function<bool, IObject>[] requirements, params IObject[] ignore)
        {
            Debug.Assert(!string.IsNullOrEmpty(str), "str is null.");
            str = String.Format(String.Capitalize(str));
            List<IObject> ignoreList = new List<IObject>(ignore);
            List<Character> list = this.GetChildren<Character>(p, distance);
            foreach (Character ch in list)
                if (ch.Position > Positions.Sleeping && !ignoreList.Contains(ch) && this.CheckRequirements(requirements, ch))
                    ch.Send(str, firstPerson);
        }
        
        [Obfuscation(Exclude = false)]
        public void Send(string str, Location location, int distance, IObject firstPerson, Damdai.Function<bool, IObject>[] requirements, params IObject[] ignore)
        {
            this.Send(str, location.Point, distance, firstPerson, requirements, ignore);
        }
        
        [Obfuscation(Exclude = false)]
        public void Send(string str, Point p, IObject firstPerson, Damdai.Function<bool, IObject>[] requirements)
        {
            this.Send(str, p, 0, firstPerson, requirements);
        }
        
        [Obfuscation(Exclude = false)]
        public void Send(String str, IObject p1, IObject p2, int distance, Damdai.Function<bool, IObject>[] requirements, bool checkThirdPersonRequirements, Telnet.Color[] colors, string postfix)
        {
            if (str.Empty)
                return;

            if (colors == null)
            {
                colors = new Telnet.Color[3];
                colors[0] = colors[1] = colors[2] = Telnet.Color.Reset;
            }

            if (this.CheckRequirements(requirements, p1))
            {
                if (!string.IsNullOrEmpty(str.FirstPerson) && (p2 == null || p1 == p2 || !p1.Location.Equals(p2.Location)))
                    p1.Send(String.Format(str.ToFirstPerson(colors[0], p1, p2) + postfix), p1);
                else if (!string.IsNullOrEmpty(str.FirstPersonAtSecondPerson) && p2 != null && p1 != p2 && p1.Location.Equals(p2.Location))
                    p1.Send(String.Format(str.ToFirstPersonAtSecondPerson(colors[0], p1, p2) + postfix), p1);
            }

            if (p2 != null && p1 != p2 && (!(p2 is Character) || (p2 as Character).Position > Positions.Sleeping) && this.CheckRequirements(requirements, p2))
            {
                if (p2.Location.Equals(p1.Location) && !string.IsNullOrEmpty(str.SecondPersonAtFirstPerson))
                    p2.Send(String.Format(str.ToSecondPersonAtFirstPerson(colors[1], p1, p2) + postfix), p1);
                else if (!string.IsNullOrEmpty(str.SecondPerson) && !p2.Location.Equals(p1.Location))
                    p2.Send(String.Format(str.ToSecondPerson(colors[1], p1, p2) + postfix), p1);
            }

            if (!string.IsNullOrEmpty(str.ThirdPersonAtFirstPerson))
                this.Send(str.ToThirdPersonAtFirstPerson(colors[2], p1, p2), p1.Location, 0, p1, (checkThirdPersonRequirements ? requirements : null), p1, p2);
            if (p2 != null && !string.IsNullOrEmpty(str.ThirdPersonAtSecondPerson))
                this.Send(str.ToThirdPersonAtSecondPerson(colors[2], p1, p2), p2.Location, 0, p1, (checkThirdPersonRequirements ? requirements : null), p1, p2);

            if (!string.IsNullOrEmpty(str.ThirdPerson))
            {
                List<IObject> list = new List<IObject>(p1.Location.Area.GetChildren<IObject>(p1.Location.Point));
                list.Add(p1);
                if(p2 != null)
                    list.Add(p2);
                this.Send(str.ToThirdPerson(colors[2], p1, p2), p1.Location, distance, p1, (checkThirdPersonRequirements ? requirements : null), list.ToArray());
            }
        }
        
        [Obfuscation(Exclude = false)]
        public void Send(String str, IObject p1, IObject p2, int distance, Damdai.Function<bool, IObject>[] requirements, Telnet.Color[] colors)
        {
            this.Send(str, p1, p2, distance, requirements, true, colors, null);
        }
        
        [Obfuscation(Exclude = false)]
        public void Send(String str, IObject p1, IObject p2, int distance, Damdai.Function<bool, IObject>[] requirements, bool checkThirdPersonRequirements)
        {
            this.Send(str, p1, p2, distance, requirements, checkThirdPersonRequirements, null, null);
        }

        [Obfuscation(Exclude = false)]
        public void Send(String str, IObject p1, IObject p2, int distance, Damdai.Function<bool, IObject>[] requirements)
        {
            this.Send(str, p1, p2, distance, requirements, true, null, null);
        }
        
        public void Send(String str, IObject p1, IObject p2, int distance)
        {
            this.Send(str, p1, p2, distance, null, true, null, null);
        }

        [Obfuscation(Exclude = false)]
        bool CheckRequirements(Damdai.Function<bool, IObject>[] requirements, IObject o)
        {
            if (requirements != null)
                foreach (Damdai.Function<bool, IObject> fn in requirements)
                    if (!fn(o))
                        return false;
            return true;
        }

        [Obfuscation(Exclude = false)]
        internal override void Draw(Graphics g, AreaCanvas canvas, AreaCanvas.View view)
        {
            Point p = this.Location.Point;
            Point screenPoint = new Point((p.X - canvas.Offset.X) * canvas.Zoom, (p.Y - canvas.Offset.Y) * canvas.Zoom);
            //using(SolidBrush brush = new SolidBrush(Color.Blue))
            //    g.FillEllipse(brush, screenPoint.X, screenPoint.Y, canvas.Zoom, canvas.Zoom);
            //using(Pen pen = new Pen(Color.DarkBlue, 2))
            //    g.DrawEllipse(pen, screenPoint.X, screenPoint.Y, canvas.Zoom, canvas.Zoom);
            using (Bitmap bmp = new Bitmap(Ultimud.Properties.Resources.earth))
            {
                bmp.MakeTransparent(Color.Magenta);
                g.DrawImage(bmp, screenPoint.X, screenPoint.Y, canvas.Zoom, canvas.Zoom);
            }
        }
        
        [Obfuscation(Exclude = false)]
        public override string ToString()
        {
            return this.Name;
        }
        
        [Obfuscation(Exclude = false)]
        public override void Update()
        {
            base.Update();
            this.CheckResets();
            if (this.WeatherConditions != null && Server.RandomNumber.Next(300) == 1)
                this.WeatherConditions.Update(this);
        }
        
        [Obfuscation(Exclude = false)]
        void CheckResets()
        {
            foreach (Reset reset in this.Resets)
                reset.Update();
        }
        
        public override bool CanSee(Object o, bool fogOfWar)
        {
            return true;
        }
        
        [Obfuscation(Exclude = false)]
        protected override void OnChildAdded(Object child)
        {
            if(this.ApplyStatusEffects != null)
                foreach (StatusEffect status in this.ApplyStatusEffects)
                    status.Apply(child, null);
        }
        
        [Obfuscation(Exclude = false)]
        protected override void OnChildRemoved(Object child)
        {
            if (this.ApplyStatusEffects != null)
                foreach (StatusEffect status in this.ApplyStatusEffects)
                    child.RemoveStatusEffect(status.BlueprintId);
        }

        [OnDeserialized]
        [Obfuscation(Exclude = false)]
        new internal void OnDeserialized(StreamingContext context)
        {
            base.OnDeserialized(context);
            if (this.enterMessage_ == null)
                this.enterMessage_ = new String("You enter {2}.", "You enter {2}.", null, null, "{1} enters {2}.", null);
            if (this.exitMessage_ == null)
                this.exitMessage_ = new String(null, null, null, null, "{1} has arrived from {2}.", null);
            if (this.weather_ == null)
                this.weather_ = new WeatherConditions();
            if (this.applystatuseffects_ == null)
                this.applystatuseffects_ = new List<StatusEffect>();
        }
        #endregion
    }
}

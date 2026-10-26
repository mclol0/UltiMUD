using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Drawing.Design;
using System.Drawing;
using System.Reflection;
using System.Diagnostics;
using System.Runtime.Serialization;
using Damdai;

namespace Ultimud
{
    [Serializable]
    [Obfuscation(Exclude = true)]
    public abstract class Reset : ICloneable, ILocation
    {
        Location location_;
        Guid objectId_;
        int respawnTime_;
        string message_;
        DateTime lastUpdate_;
        protected List<Guid> objectIds_ = new List<Guid>();
#pragma warning disable 0169, 0414
        bool closed_; //Deprecated.
        bool closeable_; //Deprecated.
        bool locked_; //Deprecated.
        Guid keyId_; //Deprecated.
#pragma warning restore 0169, 0414
        Timer respawnTimer_ = new Timer(new TimeSpan(0, 0, 60));

        #region Properties
        [Description("The location of this reset.")]
        public Location Location
        {
            get { return this.location_; }
            set { this.location_ = value; }
        }

        public Guid ObjectId
        {
            get { return this.objectId_; }
            set { this.objectId_ = value; }
        }

        [Description("The amount of time before the object associated with this reset is respawned.")]
        public Timer RespawnTimer
        {
            get { return this.respawnTimer_; }
            set { this.respawnTimer_ = value; }
        }

        [Description("The message that is displayed when this object respawns.")]
        public string Message
        {
            get { return this.message_; }
            set { this.message_ = value; }
        }

        [Browsable(false)]
        public string ObjectName
        {
            get
            {
                return Server.Instance.GetObjectBlueprintName(this.ObjectId);
            }
        }

        [Browsable(false)]
        public bool Closeable
        {
            get { return this.closeable_; }
            set { }
        }

        [Browsable(false)]
        public bool Closed
        {
            get { return this.closed_; }
            set { }
        }

        [Browsable(false)]
        public bool Locked
        {
            get { return this.locked_; }
            set { }
        }

        [Browsable(false)]
        public Item Key
        {
            get { return null; }
            set { }
        }
        #endregion

        public Reset()
        {
        }
        public Reset(Location location)
        {
            this.Location = location;
        }
        
        [Obfuscation(Exclude = false)]
        public void Update()
        {
            if (this.RespawnTimer != null)
            {
                if (this.CheckConditions())
                {
                    if (!this.RespawnTimer.Update())
                        this.Respawn();
                }
                else
                {
                    this.RespawnTimer.Reset();
                }
            }
        }
        
        [Obfuscation(Exclude = false)]
        protected virtual Object Respawn()
        {
            Object o = Server.Instance.GetObjectBlueprint(this.ObjectId);
            if (o != null)
            {
                Object respawn = o.GetType().GetMethod("Create", BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { o }) as Object;
                respawn.Location = this.Location;
                String str = new String(null, null, null, null, this.Message, null);
                this.Location.Area.Send(str, respawn, null, 0);
                this.objectIds_.Add(respawn.Id);
                this.lastUpdate_ = DateTime.Now;
                return respawn;
            }
            return null;
        }
        
        [Obfuscation(Exclude = false)]
        protected abstract bool CheckConditions();
        
        [Obfuscation(Exclude = false)]
        public object Clone()
        {
            return this.MemberwiseClone();
        }
        
        [OnDeserialized]
        [Obfuscation(Exclude = false)]
        internal void OnDeserialized(StreamingContext context)
        {
            if (this.respawnTime_ > 0)
            {
                this.RespawnTimer = new Timer(new TimeSpan(0, 0, this.respawnTime_));
                this.respawnTime_ = 0;
            }
            else if (this.respawnTimer_ == null)
                this.RespawnTimer = new Timer(new TimeSpan(0, 0, 60));

            this.closeable_ = false;
            this.closed_ = false;
            this.locked_ = false;
            this.keyId_ = Guid.Empty;
        }
        
        public void Clear()
        {
            foreach (Guid oid in Damdai.Collections.Util.CopyList<Guid>(this.objectIds_))
            {
                Object o = Server.Instance.Find<Object>(oid);
                if (o != null) o.Die(null);
            }
            this.objectIds_.Clear();
        }
    }
}

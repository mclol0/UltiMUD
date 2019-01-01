using System;
using System.Collections.Generic;
using System.Reflection;
using System.Drawing;
using System.Drawing.Design;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Ultimud
{
    [Serializable]
    public class BackgroundImageSettings : MarshalByRefObject, ISerializable
    {
        Image image_;
        Rectangle rect_;

        [Obfuscation(Exclude = false)][NonSerialized] Image modifiedImage_;
        [Obfuscation(Exclude = false)][NonSerialized] bool visible_ = true;

        #region Properties
        public Image Image
        {
            get { return this.modifiedImage_; }
            set
            {
                this.image_ = value;
                this.modifiedImage_ = value;
                if (value != null)
                    this.Rect = new Rectangle(0, 0, value.Width, value.Height);
            }
        }
        public Rectangle Rect
        {
            get { return this.rect_; }
            set
            {
                this.rect_ = value;
                if (this.image_ != null)
                    this.modifiedImage_ = Damdai.Util.ResizeImage(this.image_, rect_.Size);
            }
        }
        public bool Visible
        {
            get { return this.visible_; }
            set { this.visible_ = value; }
        }
        #endregion

        public BackgroundImageSettings() { }
        internal BackgroundImageSettings(SerializationInfo info, StreamingContext context)
        {
            foreach (FieldInfo field in typeof(BackgroundImageSettings).GetFields(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.NonPublic))
                if (field.GetCustomAttributes(typeof(NonSerializedAttribute), false).Length == 0)
                    field.SetValue(this, info.GetValue(field.Name, field.FieldType));

            if (this.image_ != null)
                this.modifiedImage_ = Damdai.Util.ResizeImage(this.image_, this.Rect.Size);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            foreach (FieldInfo field in typeof(BackgroundImageSettings).GetFields(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.NonPublic))
                if (field.GetCustomAttributes(typeof(NonSerializedAttribute), false).Length == 0)
                    info.AddValue(field.Name, field.GetValue(this));
        }
        public override object InitializeLifetimeService()
        {
            return null;
        }

        public override string ToString()
        {
            return null;
        }
    }
}

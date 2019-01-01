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
    [Obfuscation(Exclude = true)]
    public class BackgroundImageSettings
    {
        Image image_;
        Rectangle rect_;

        [Obfuscation(Exclude = false)]
        [NonSerialized]
        Image modifiedImage_;

        [Obfuscation(Exclude = false)]
        [NonSerialized]
        bool visible_ = true;

        #region Properties
        [Category("Background Image")]
        public Image Image
        {
            get { return this.modifiedImage_; }
            set
            {
                this.image_ = value;
                this.modifiedImage_ = value;
                if (value != null)
                {
                    this.Rect = new Rectangle(0, 0, value.Width, value.Height);
                }
            }
        }

        [Category("Background Image")]
        [Description("The size and location of this image.")]
        public Rectangle Rect
        {
            get { return this.rect_; }
            set
            {
                this.rect_ = value;
                if (this.image_ != null)
                {
                    //this.modifiedImage_ = Damdai.Image.ResizeImage(this.image_, rect_.Size);
                }
            }
        }

        [DefaultValue(true)]
        public bool Visible
        {
            get { return this.visible_; }
            set { this.visible_ = value; }
        }
        #endregion

        [Obfuscation(Exclude = false)]
        public override string ToString()
        {
            return null;
        }

        [OnDeserialized]
        [Obfuscation(Exclude = false)]
        internal void OnDeserialized(StreamingContext context)
        {
            //if (this.image_ != null)
            //    this.modifiedImage_ = Damdai.Image.ResizeImage(this.image_, this.Rect.Size);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace Damdai
{
    public class Image
    {
        public static System.Drawing.Image ResizeImage(System.Drawing.Image poImage, System.Drawing.Size poSize)
        {
            //Detach image from its source
            System.Drawing.Image oImageOriginal = (System.Drawing.Image)poImage.Clone();

            //Resize new image
            System.Drawing.Image oResizedImage = new Bitmap(poSize.Width, poSize.Height, oImageOriginal.PixelFormat);
            Graphics oGraphic = Graphics.FromImage(oResizedImage);
            oGraphic.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
            oGraphic.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            oGraphic.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            Rectangle oRectangle = new Rectangle(0, 0, poSize.Width, poSize.Height);
            oGraphic.DrawImage(oImageOriginal, oRectangle);
            oGraphic.Dispose();
            oImageOriginal.Dispose();
            return oResizedImage;
        }
    }
}

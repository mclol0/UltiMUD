using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Damdai
{
    public static class Util
    {
        public static void Swap<T>(ref T t1, ref T t2)
        {
            T temp = t1;
            t1 = t2;
            t2 = temp;
        }

        public static Image ResizeImage(Image poImage, Size poSize)
        {
            //Detach image from its source
            Image oImageOriginal = (Image)poImage.Clone();

            //Resize new image
            Image oResizedImage = new Bitmap(poSize.Width, poSize.Height, oImageOriginal.PixelFormat);
            Graphics oGraphic = Graphics.FromImage(oResizedImage);
            oGraphic.CompositingQuality = CompositingQuality.HighQuality;
            oGraphic.SmoothingMode = SmoothingMode.HighQuality;
            oGraphic.InterpolationMode = InterpolationMode.HighQualityBicubic;
            Rectangle oRectangle = new Rectangle(0, 0, poSize.Width, poSize.Height);
            oGraphic.DrawImage(oImageOriginal, oRectangle);
            oGraphic.Dispose();
            oImageOriginal.Dispose();
            return oResizedImage;
        }
    }
}

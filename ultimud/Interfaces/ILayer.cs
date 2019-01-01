using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace Ultimud
{
    public interface ILayer : IObject, ICloneable
    {
        int ZIndex { get; set; }
        int Height { get; set; }
        char FillChar { get; set; }
        char FillChar2 { get; set; }
        int FillChar2Percent { get; set; }
        bool FillChar2Randomize { get; set; }
        bool IsInvis { get; set; }
        List<Point> Points { get; set; }
        Telnet.Color FillColor { get; set; }
        Telnet.Color FillColor2 { get; set; }
        Rectangle BoundingBox { get; }
        Dictionary<Point, bool> Overlay { get; }
        WeatherConditions WeatherConditions { get; set; }
        IScript OnObjectMove { get; set; }

        void AddPoint(Point p);
        void AddPoint(int index, Point p);
        void RemovePoint(int index);
        bool ContainsPoint(int x, int y);
        bool ContainsPoint(Point p);
        bool ContainsPoints(IEnumerable<Point> points);
    }
}

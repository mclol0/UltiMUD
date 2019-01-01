using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using Damdai;

namespace Ultimud
{
    public interface IArea : IObject, ICloseable
    {
        BackgroundImageSettings BackgroundImageSettings { get; set; }
        List<IReset> Resets { get; set; }
        Point Start { get; set; }
        Telnet.Color FillColor { get; set; }
        Telnet.Color FillColor2 { get; set; }
        char FillChar { get; set; }
        char FillChar2 { get; set; }
        int FillChar2Percent { get; set; }
        bool LineOfSight { get; set; }
        bool FogOfWar { get; set; }

        IShop GetShop(Point p);
        IShop NewShop(Point p);
        IReset NewReset(Point p);
        ILayer NewLayer();
        ILayer NewPath();
        ILayer NewExit();
        ILayer NewNpcRegion();

        void Send(string str, Function<bool, IObject> test);
        void Send(string str, Point p, int distance, IObject p1, Function<bool, IObject> conditional, params IObject[] ignore);
        void Send(string str, Location location, int distance, IObject p1, Function<bool, IObject> conditional, params IObject[] ignore);
        void Send(string str, Point p, IObject p1, Function<bool, IObject> conditional);
        void Send(string str, Point p, int distance, IObject p1, params IObject[] ignore);
        void Send(string str, Location location, int distance, IObject p1, params IObject[] ignore);
        void Send(string str, Point p, IObject p1);
        void Send(String str, IObject p1, IObject p2, int distance, Telnet.Color[] colors, string postfix, Function<bool, IObject> p1p2conditional, Function<bool, IObject> p3conditional);
        void Send(String str, IObject p1, IObject p2, int distance, Telnet.Color[] colors, Function<bool, IObject> p1p2conditional, Function<bool, IObject> p3conditional);
        void Send(String str, IObject p1, IObject p2, int distance, Function<bool, IObject> p1p2conditional, Function<bool, IObject> p3conditional);
        void Send(String str, IObject p1, IObject p2, int distance, Telnet.Color[] colors, string postfix);
        void Send(String str, IObject p1, IObject p2, int distance, Telnet.Color[] colors);
        void Send(String str, IObject p1, IObject p2, int distance);

        T GetLayer<T>(Point p, bool invis, params T[] ignore) where T : class, ILayer;
        List<T> GetLayers<T>(Point p, bool invis, params T[] ignore) where T : class, ILayer;
        List<IObject> GetNearbyObjects(Point p, int range);

        void AddReset(IReset reset);
        void RemoveReset(IReset reset);
    }
}

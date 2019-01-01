using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using System.Drawing;
using Damdai;

namespace Ultimud
{
    public interface IObject : IIdentifiable, ILocatable
    {
        IObject Parent { get; set; }
        IArea ContainingArea { get; }
        IAction CurrentAction { get; }
        Dictionary<Guid, IObject> Children { get; }
        List<IStatusEffect> StatusEffects { get; set; }
        List<Guid> Tags { get; set; }
        Dictionary<Guid, IStat> Stats { get; set; }
        Telnet.Color MapColor { get; set; }
        Position Position { get; set; }
        Timer LifeSpan { get; set; }
        string MapSymbol { get; set; }
        string Description { get; set; }
        int Value { get; set; }
        int Size { get; set; }
        int RemainingSpace { get; }
        bool IsIncapacitated { get; }
        object this[object var] { get; set; }

        IScript OnCreate { get; set; }
        IScript OnUpdate { get; set; }
        IScript OnDestroy { get; set; }

        bool CanSee(IObject o, bool fogofwar);
        bool CanSee(Point p);
        bool CanSee(int x, int y);
        bool CanMove(Point p, bool force);
        bool Move(Direction direction, String leavemsg, String arrivemsg, bool force);
        bool HasTag(Guid id);
        bool HasTag(List<Guid> tags);
        bool HasTags(List<Guid> tags);
        bool HasStat(IStatBlueprint statblueprint);
        void AddChild(IObject o);
        void RemoveChild(Guid id, bool destroy);
        void AddStat(IStatBlueprint statblueprint);
        void Send(string str, IObject actor);
        void Send(string str);
        void Draw(IMap map);
        void Destroy(IObject killer);
        void Update();
        void RemoveStatusEffect(IStatusEffect status);
        void EnqueueAction(IAction action);
        void StartNewAction(IAction action);
        void RemoveAction(Type type);
        void RemoveAction(IAction action);
        void ReplaceCurrentAction(IAction action);
        void Stop(bool silent);
        void Wait();
        void Go();
        void ApplyStatMod(Guid id, long current, long max, bool silent);
        void ApplyStatMod(string statname, long current, long max, bool silent);
        void ApplyStatMod(IStat mod, bool silent);
        void ApplyStatMods(List<IStat> mods, bool silent);
        IObject GetChild(Guid id);
        int GetDistance(Point p);
        int GetDistance(ILocatable o);
        long GetStatCurrent(string name);
        long GetStatMax(string name);
        IStat GetStat(IStatBlueprint blueprint);
        IStat GetStat(string name);
        IStatusEffect GetStatusEffect(IStatusEffectBlueprint blueprint);
        Direction GetDirection(IObject o);
        Direction GetDirection(Point p);
        T FindNearby<T>(string name, int rangemin, int rangemax, bool sightlimited, Function<bool, IObject> conditional) where T : class, IObject;
        T FindNearby<T>(string name, int rangemax, bool sightlimited, Function<bool, IObject> conditional) where T : class, IObject;
        T FindNearby<T>(string name, int rangemax, bool sightlimited) where T : class, IObject;
        T FindNearby<T>(string name, int[] range, bool sightlimited) where T : class, IObject;
        T FindChild<T>(T t, bool searchchildren) where T : class, IObject;
        T FindChild<T>(string name, bool searchchildren, Function<bool, IObject> conditional) where T : class, IObject;
        T FindChild<T>(string name, bool searchchildren) where T : class, IObject;
        List<IObject> GetChildList();
        object Clone();

        event Function_void<IObject, IObject> Destroyed;
    }
}

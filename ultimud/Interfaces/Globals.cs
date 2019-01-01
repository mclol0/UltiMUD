using System;
using System.Collections.Generic;
using System.Text;

namespace Ultimud
{
    public struct Globals
    {
#if DEBUG
        public static int SERVER_PORT_OFFSET = 3000;
        public static int GAME_PORT_OFFSET = 3001;
        public static int CLIENT_PORT = 3000;
#else
        public static int SERVER_PORT_OFFSET = 4000;
        public static int GAME_PORT_OFFSET = 4001;
        public static int CLIENT_PORT = 4000;
#endif

        public static IServer Server;
        public static Random Random = new Random((int)DateTime.Now.Ticks);
        public static Dictionary<string, List<T>> ConsolidateList<T>(List<T> list) where T : IObject
        {
            Dictionary<string, List<T>> dictionary = new Dictionary<string, List<T>>();
            foreach (T t in list)
            {
                if (string.IsNullOrEmpty(t.Name))
                    continue;

                if (!dictionary.ContainsKey(t.Name))
                    dictionary.Add(t.Name, new List<T>());

                dictionary[t.Name].Add(t);
            }
            return dictionary;
        }
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
        Random,
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

    public enum BodyLocation
    {
        None,
        Head,
        Face,
        Ears,
        Neck,
        Shoulders,
        Body,
        Back,
        Arms,
        Elbows,
        Wrists,
        Hands,
        Held,
        Fingers,
        Waist,
        Legs,
        Knees,
        Feet,
    }

    public enum Gender
    {
        None,
        Male,
        Female,
    }

    public enum Position
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

    public enum UserFlags
    {
        AutoMap,
        LoggedIn,
        Administrator,
        Spacing,
        Help,
        Creating,
        Silenced
    }

    public enum AbilityTargetType
    {
        Player,
        Npc,
        Item,
    }
    public enum AbilityStatus
    {
        Ok,
        //Done,
        Miss,
        Fail,
    }
    /// <exclude/>
    public enum AbilityState
    {
        Prepare,
        PerformWithAreaCheck,
        PerformWithoutAreaCheck,
        Miss,
        Fail,
        Success
    }
    public enum AbilityStyle
    {
        None,
        Defensive,
        Aggressive
    }
    public enum AbilityTarget
    {
        Self,
        Others,
        All,
    }

    public enum Perspective
    {
        FirstPerson,
        SecondPerson,
        ThirdPerson
    }

    public enum TextType
    {
        Help,
        Server,
        StatIncrease,
        StatDecrease,
        AbilityImprovements,
        AbilityPrepare1,
        AbilityPrepare2,
        AbilityPrepare3,
        AbilityPerform1,
        AbilityPerform2,
        AbilityPerform3,
        AbilitySuccess1,
        AbilitySuccess2,
        AbilitySuccess3,
        AbilityMiss1,
        AbilityMiss2,
        AbilityMiss3,
        AbilityFail1,
        AbilityFail2,
        AbilityFail3,
        MapColorSelf,
        MapColorPlayer,
    }

    public enum BroadcastEvent
    {
        Login,
        Logout,
        NewCharacter,
        Deleted,
        MessagePosted
        //Death,
        //LevelUp,
    }

    public enum ServerFlags
    {
        Newlocked,
    }

    public enum ChannelTarget
    {
        World,
        Player,
        Npc,
        PlayerOrNpc
    }
}

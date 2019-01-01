using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Data;
using System.Drawing;
using Damdai;

namespace Ultimud
{
    public partial interface IServer
    {
        List<IArea> Areas { get; }
        Dictionary<TextType, Telnet.Color> Colors { get; set; }
        IArea MainArea { get; }
        IFightSystem FightSystem { get; set; }
        IBlueprintManager BlueprintManager { get; }
        IWorldManager WorldManager { get; }
        IUserManager UserManager { get; }
        IInfoChannel InfoChannel { get; }
        IPvpSettings PvpSettings { get; }
        BitArray Flags { get; set; }
        string Name { get; set; }
        string Description { get; set; }
        string TitleScreen { get; set; }
        string MOTD { get; set; }
        int ClientCount { get; }
        int InventoryLimit { get; set; }
        int MapSize { get; set; }
        bool NumberCommas { get; set; }
        bool Running { get; }
        IScript Prompt { get; set; }
        IScript OnLogin { get; set; }
        IScript OnCommandEntered { get; set; }
        IScript OnAbilityPerformed { get; set; }

        string GetColorCode(TextType t);
        void SetColor(TextType text, Telnet.Color color);

        List<string> RegisterClient(string ip, int port);
        void UnregisterClient(string ip);
        void SendLogToClients(string category, string msg);
        void SendMessageToClients(string fromip, string msg);
        void DoClientFunction(Function_void<IClient> fn);

        void Start();
        void Stop();
        void Echo(string str, params IObject[] ignore);
        bool ProcessCommand(IUser user);
        bool ProcessCommand(IUser user, bool includescriptcommands);
        IHelp GetHelp(string arg);
        IArea GetArea(string areaname);

        IArea NewArea(string name);
        IStat NewStat(IStatBlueprint blueprint);
        IStat NewStat(IStatBlueprint blueprint, long current, long max);

        IIdentifiable NewCommand();
        IIdentifiable NewChannel();
        IIdentifiable NewMessageBoard();
        IIdentifiable NewSocial();
        IIdentifiable NewHelp();
        IIdentifiable NewStatBlueprint();
        IIdentifiable NewStatusEffect();
        IIdentifiable NewAbility();
        IIdentifiable NewItem();
        IIdentifiable NewNpc();
        IIdentifiable NewRace();
        IIdentifiable NewJob();
        IIdentifiable NewScript(string name, string script);
        IIdentifiable NewTag();
    }
}

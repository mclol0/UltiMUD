using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using Damdai;

namespace Ultimud
{
    public interface IUser
    {
        Guid Id { get; }
        ICharacter Character { get; set; }
        string Password { get; set; }
        string IpAddress { get; }
        string Input { get; set; }
        string Command { get; set; }
        string LastCommand { get; set; }
        bool Connected { get; }
        bool ProcessingInput { get; set; }
        object CurrentArg { get; set; }
        Socket Socket { get; set; }
        IMenu CurrentMenu { get; }
        List<object> CurrentCommandArgs { get; }
        Dictionary<string, string> Aliases { get; }
        Function<bool, IUser> CurrentInputHandler { get; }
        Function<bool, IUser> InputHandler { get; set; }
        Queue<IMenu> Menus { get; }
        BitArray Flags { get; set; }
        DateTime LastActivity { get; set; }
        Location StartLocation { get; set; }
        Dictionary<IChannel, bool> ChannelSettings { get; set; }
        int LoginStep { get; set; }

        event Function_void<IUser, string> InputReceivedEvent;
        void InputReceived(string str);

        void Init(IUser other);
        void Send(string str);
        void SendError(string str);
        void SendHelp(string str);
        void SendPrompt();
        void FlushOutput();
        void FlushAllOutput();
        void FinishInput();
        void SetChannel(IChannel channel, bool on, bool quiet);
        bool GetChannelSetting(IChannel channel);
        string GetArgument();
        string GetAlias(string str);
        object GetArg(int num);
        object GetCurrentArg();
        object GetLastCommandArg();
        T GetCommandArg<T>(int num);
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Ultimud
{
    public interface IClient
    {
        string Name { get; }
        string IpAddress { get; }
        
        void ReceiveMessage(string from, string msg);
        void ReceiveLog(string category, string msg);
        void ReceiveAlert(string str);
        void ClientConnected(string name);
        void ClientDisconnected(string name);
        void ServerStarted();
        void ServerStopped();
        void UserConnected(IUser u);
        void UserDisconnected(Guid id);
        void BlueprintAdded(IIdentifiable i);
        void BlueprintRemoved(Guid id);
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Ultimud
{
    public interface IUserManager
    {
        Dictionary<Guid, IUser> SavedUsers { get; }
        Dictionary<Guid, IUser> LoggedIn { get; }

        void SaveUser(IUser user);
        void DeleteUser(IUser user);
        void SetUserConnecting(IUser user);
        void SetUserLoggedIn(IUser user);
        void SetUserLoggedOut(IUser user);
        void DisconnectUser(IUser user);
        void DisconnectAllUsers();
        void UpdateUsers();
        void LogoutAllUsers();
        void FlushOutput();

        IUser GetSavedUser(string name);
        IUser GetLoggedInUser(string name);
        List<IUser> GetSavedUsersList();
        List<IUser> GetLoggedInUsersList();
        List<IUser> GetAllConnectedUsers();
    }
}

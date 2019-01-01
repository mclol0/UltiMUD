using System;
using System.Collections.Generic;
using System.Text;

namespace Ultimud
{
    public interface IMessageBoard : IIdentifiable
    {
        string Description { get; set; }
        long NextPostId { get; set; }
        Dictionary<long, IMessageBoardPost> Posts { get; }

        IMessageBoardPost GetLastPost();
        void PrintMessages(IUser user);
        void DeletePost(IMessageBoardPost post);
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Ultimud
{
    public interface IMessageBoardPost
    {
        long Id { get; }
        string From { get; }
        string Subject { get; }
        DateTime Date { get; }
        IUser User { get; }
    }
}

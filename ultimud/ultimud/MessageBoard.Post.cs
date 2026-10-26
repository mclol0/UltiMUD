using System;
using System.Collections.Generic;
using System.Reflection;
using System.Diagnostics;

namespace Ultimud
{
    public partial class MessageBoard
    {
        [Serializable]
        [Obfuscation(Exclude = true)]
        public class Post
        {
            long id_;
            DateTime date_;
            string from_;
            string to_;
            string subject_;
            string body_;
            Guid pid_;

            #region Properties
            public long Id
            {
                get { return this.id_; }
            }
            public DateTime Date
            {
                get { return this.date_; }
            }
            public string From
            {
                get { return this.from_; }
            }
            public string To
            {
                get { return this.to_; }
            }
            public string Subject
            {
                get { return this.subject_; }
            }
            public string Body
            {
                get { return this.body_; }
            }
            public Guid PlayerId
            {
                get { return this.pid_; }
            }
            #endregion

            public Post(long id, string from, string to, string subject, string body, Guid pid)
            {
                this.id_ = id;
                this.date_ = DateTime.Now;
                this.from_ = from;
                this.to_ = to;
                this.subject_ = subject;
                this.body_ = body;
                this.pid_ = pid;
            }

            [Obfuscation(Exclude = false)]
            static public bool GetTo(User user)
            {
                if (string.IsNullOrEmpty(user.Command))
                {
                    user.CurrentCommandArgs.Add("all");
                    return true;
                }
                string arg = user.GetArgument();
                user.CurrentCommandArgs.Add(string.IsNullOrEmpty(arg) ? "all" : arg);
                return true;
            }
            
            [Obfuscation(Exclude = false)]
            static public bool GetSubject(User user)
            {
                if (string.IsNullOrEmpty(user.Command))
                {
                    user.Menus.Enqueue(new Menu(GetSubject, "Subject: "));
                    return false;
                }
                else
                {
                    user.CurrentCommandArgs.Add(user.Command);
                    user.Command = null;
                    return true;
                }
            }
            
            [Obfuscation(Exclude = false)]
            static public bool GetBody(User user)
            {
                if (string.IsNullOrEmpty(user.Command))
                {
                    user.Menus.Enqueue(new Menu(GetBody, "Message: "));
                    return false;
                }
                else
                {
                    user.CurrentCommandArgs.Add(user.Command);
                    user.Command = null;
                    return true;
                }
            }
            
            [Obfuscation(Exclude = false)]
            public override string ToString()
            {
                return
                    "From: " + this.From + "\r\n" +
                    "To: " + this.to_ + "\r\n" +
                    "Date: " + this.date_.ToString() + "\r\n" +
                    "Subject: " + this.Subject + "\r\n\r\n" +
                    this.body_.TrimEnd('\r', '\n');
            }
        }
    }
}

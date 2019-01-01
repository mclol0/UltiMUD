using System;
using System.Collections.Generic;
using System.Reflection;
using System.ComponentModel;

namespace Ultimud
{
    [Serializable]
    [Obfuscation(Exclude = true)]
    internal partial class MessageBoard : IIdentifiable
    {
        Guid id_;
        string name_;
        string description_;
        Dictionary<long, Post> posts_ = new Dictionary<long, Post>();
        long nextPostId_;

        [Obfuscation(Exclude = false)]
        [NonSerialized]
        static Dictionary<string, Server.InputHandler> commands_ = new Dictionary<string, Server.InputHandler>();

        #region Properties
        [Browsable(false)]
        public Guid Id
        {
            get { return this.id_; }
            set { this.id_ = value; }
        }

        [Description("The name of this message board.")]
        public string Name
        {
            get { return this.name_; }
            set { this.name_ = Telnet.Decolorize(value); }
        }

        [Description("A description of this message board outlining its purpose.")]
        public string Description
        {
            get { return this.description_; }
            set { this.description_ = value; }
        }

        [Browsable(false)]
        public Dictionary<long, Post> Posts
        {
            get { return this.posts_; }
        }

        public static Dictionary<string, Server.InputHandler> Commands
        {
            get { return commands_; }
        }
        #endregion

        internal static void Init(string loadpath, string serverpath)
        {
            commands_.Clear();
            commands_.Add("list", List);
            commands_.Add("post", PostMessage);
            commands_.Add("read", ReadMessage);
            commands_.Add("delete", DeleteMessage);
            if (!Server.Instance.DataSets.ContainsKey(typeof(MessageBoard)))
                Server.Instance.DataSets.Add(typeof(MessageBoard), new Dictionary<Guid, IIdentifiable>());
        }
        
        //public static Server.InputHandler GetHandler(string str)
        //{
        //    Server.InputHandler handler = null;
        //    commands_.TryGetValue(str, out handler);
        //    return handler;
        //}

        [Obfuscation(Exclude = false)]
        public static bool List(User user)
        {
            string arg = user.GetArgument();
            if (string.IsNullOrEmpty(arg))
            {
                Menu menu = new Menu(List, null);
                menu.AddOption("all");
                Dictionary<Guid, IIdentifiable> messageBoards = Server.Instance.DataSets[typeof(MessageBoard)];
                lock (messageBoards)
                    foreach (MessageBoard m in messageBoards.Values)
                        menu.AddOption(m.Name);
                user.Menus.Enqueue(menu);
                return false;
            }
            else
            {
                MessageBoard mb = Server.Instance.Get<MessageBoard>(arg);
                if (mb == null)
                {
                    StringTable table = new StringTable("Board", "Posts", "Most Recent");
                    table.Columns[1].Alignment = StringTable.Column.Align.Right;
                    foreach (MessageBoard m in Server.Instance.DataSets[typeof(MessageBoard)].Values)
                    {
                        Post lastpost = m.GetLastPost();
                        table.AddRow(m.Name, m.Posts.Count.ToString(), (lastpost != null ? (lastpost.Date.ToString("MM/dd/yyyy HH:mm:ss") + " by " + lastpost.From) : ""));
                    }

                    table.Sort("board");
                    user.Send(table.ToString());
                }
                else
                    mb.PrintMessages(user);

                user.FinishInput();
                return true;
            }
        }
        
        [Obfuscation(Exclude = false)]
        public static bool PostMessage(User user)
        {
            user.InputHandler = PostMessage;
            if (Ultimud.Command.GetArguments(user, GetBoard, Post.GetSubject, Post.GetBody))
            {
                object cmd1 = user.GetArg(0);
                MessageBoard board = (cmd1 is MessageBoard ? cmd1 as MessageBoard: Server.Instance.Get<MessageBoard>((string)cmd1));
                if (board == null)
                {
                    user.Send("Invalid message board.");
                    throw new AbortCommandException();
                }

                Post post = new Post(
                    board.nextPostId_++,
                    user.Character.Name,
                    "all",
                    user.GetCommandArg<string>(1),
                    user.GetCommandArg<string>(2),
                    user.Id);
                lock (board.Posts)
                    board.Posts.Add(post.Id, post);
                user.Send("Message posted.");
                Server.Instance.Settings.InfoChannel.Broadcast(InfoChannel.Event.MessagePosted, 
                    user.Character.Name + " has posted a new message in the " + board.Name + " forum.");
                return true;
            }
            return false;
        }
        
        [Obfuscation(Exclude = false)]
        public static bool GetBoard(User user)
        {
            string arg = null;
            if (user.CurrentArg != null)
                user.CurrentCommandArgs.Add(user.GetCurrentArg());
            else if (!string.IsNullOrEmpty((arg = user.GetArgument())))
                user.CurrentCommandArgs.Add(arg);
            else
            {
                Menu menu = new Menu(GetBoard, null);
                Dictionary<Guid, IIdentifiable> messageBoards = Server.Instance.DataSets[typeof(MessageBoard)];
                lock (messageBoards)
                    foreach (MessageBoard mb in messageBoards.Values)
                        menu.AddOption(new Menu.Option(mb.Name, mb));

                if (menu.Options == null)
                {
                    user.Send("There are no message boards.");
                    throw new AbortCommandException();
                }

                user.Menus.Enqueue(menu);
                return false;
            }
            return true;
        }
        
        [Obfuscation(Exclude = false)]
        public static bool ReadMessage(User user)
        {
            user.InputHandler = ReadMessage;
            if (Ultimud.Command.GetArguments(user, GetBoard, SelectMessage))
            {
                object cmd1 = user.GetArg(0);
                MessageBoard board = (cmd1 is MessageBoard ? cmd1 as MessageBoard : Server.Instance.Get<MessageBoard>((string)cmd1));
                if (board == null)
                {
                    user.Send("Invalid message board.");
                    throw new AbortCommandException();
                }

                Post post = null;
                lock (board.Posts)
                {
                    string cmd2 = user.GetCommandArg<string>(1);
                    int id;
                    if(int.TryParse(cmd2, out id) && board.Posts.TryGetValue(id, out post))
                    {
                        user.Send(String.Format(post.ToString()));
                    }
                    else
                    {
                        user.Send("Message not found: " + cmd2);
                        return true;
                    }
                }

                return true;
            }
            return false;
        }
        
        [Obfuscation(Exclude = false)]
        public static bool DeleteMessage(User user)
        {
            user.InputHandler = DeleteMessage;
            if (Ultimud.Command.GetArguments(user, GetBoard, SelectMessage))
            {
                object cmd1 = user.GetArg(0);
                MessageBoard board = (cmd1 is MessageBoard ? cmd1 as MessageBoard : Server.Instance.Get<MessageBoard>((string)cmd1));
                if (board == null)
                {
                    user.Send("Invalid message board.");
                    throw new AbortCommandException();
                }

                Post post = null;
                lock (board.Posts)
                {
                    string cmd2 = user.GetCommandArg<string>(1);
                    int id;
                    if (int.TryParse(cmd2, out id) && board.Posts.TryGetValue(id, out post))
                    {
                        if (post.PlayerId == user.Id)
                        {
                            board.DeletePost(post);
                            user.Send("Post deleted.");
                        } else
                            user.Send("You can't delete that post.");
                    }
                    else
                    {
                        user.Send("Message not found: " + cmd2);
                        return true;
                    }
                }

                return true;
            }
            return false;
        }
        
        [Obfuscation(Exclude = false)]
        void PrintMessages(User user)
        {
            if (this.Posts.Count < 1)
                user.Send("There are no messages on this board.");
            else
            {
                StringTable table = new StringTable("Id", "Date", "From", "Subject");
                table.Columns[0].Alignment = StringTable.Column.Align.Right;
                lock (this.Posts)
                    foreach (Post post in this.Posts.Values)
                        table.AddRow(post.Id.ToString(), post.Date.ToString("MM/dd/yyyy HH:mm:ss"), post.From, String.Summarize(post.Subject, 30));
                user.Send(table.ToString());
            }
        }
        
        [Obfuscation(Exclude = false)]
        static bool SelectMessage(User user)
        {
            string arg = null;
            if (user.CurrentArg != null)
            {
                user.CurrentCommandArgs.Add(user.GetCurrentArg());
                return true;
            }

            object lastcmd = user.GetLastCommandArg();
            if (lastcmd == null)
            {
                user.Menus.Enqueue(new Menu(SelectMessage, null));
                return false;
            }

            MessageBoard board = (lastcmd is MessageBoard ? lastcmd as MessageBoard : Server.Instance.Get<MessageBoard>((string)lastcmd));
            if (board == null)
            {
                user.Send("Invalid message board.");
                throw new AbortCommandException();
            }
            if (board.Posts.Count < 1)
            {
                user.Send("There are no messages on this board.");
                throw new AbortCommandException();
            }

            if (!string.IsNullOrEmpty((arg = user.GetArgument())))
                user.CurrentCommandArgs.Add(arg);
            else 
            {
                board.PrintMessages(user);
                user.Menus.Enqueue(new Menu(SelectMessage, "Enter message id: "));
                return false;
            }
            return true;
        }
        
        [Obfuscation(Exclude = false)]
        void DeletePost(Post post)
        {
            lock(this.posts_)
                this.posts_.Remove(post.Id);
        }
        
        [Obfuscation(Exclude = false)]
        Post GetLastPost()
        {
            Post post = null;
            this.posts_.TryGetValue(this.nextPostId_ - 1, out post);
            return post;
        }
        
        [Obfuscation(Exclude = false)]
        public override string ToString()
        {
            if (string.IsNullOrEmpty(this.Name))
                return base.ToString();
            return this.Name;
        }
    }
}

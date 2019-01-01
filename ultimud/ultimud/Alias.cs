using System;
using System.Collections.Generic;
using System.Reflection;

namespace Ultimud
{
    [Obfuscation(Exclude = true)]
    internal static class Alias
    {
        [NonSerialized]
        [Obfuscation(Exclude = false)]
        static Dictionary<string, Server.InputHandler> commands_ = new Dictionary<string, Server.InputHandler>();

        [Obfuscation(Exclude = false)]
        internal static Dictionary<string, Server.InputHandler> Commands
        {
            get { return commands_; }
        }

        internal static void Init(string loadpath, string serverpath)
        {
            commands_.Clear();
            commands_.Add("list", Alias.List);
            commands_.Add("add", Alias.Add);
            commands_.Add("delete", Alias.Delete);
        }

        internal static Server.InputHandler GetHandler(string str)
        {
            Server.InputHandler handler = null;
            commands_.TryGetValue(str, out handler);
            return handler;
        }

        [Obfuscation(Exclude = false)]
        internal static bool List(User user)
        {
            if (user.Aliases.Count == 0)
            {
                user.Send("You have no aliases.");
                user.FinishInput();
                return true;
            }

            string str = string.Empty;
            foreach (KeyValuePair<string, string> alias in user.Aliases)
            {
                str += alias.Key + " > " + alias.Value + "\r\n";
            }

            user.Send(str.Trim('\r', '\n'));
            user.FinishInput();
            return true;
        }

        [Obfuscation(Exclude = false)]
        internal static bool Add(User user)
        {
            user.InputHandler = Add;
            if (Command.GetArguments(user, GetAlias, GetAliasedCommand))
            {
                string cmd1 = user.GetCommandArg<string>(0);
                string cmd2 = user.GetCommandArg<string>(1);

                if (user.Aliases.ContainsKey(cmd1))
                {
                    user.Aliases[cmd1] = cmd2;
                }
                else
                {
                    user.Aliases.Add(cmd1, cmd2);
                }

                user.Send("Alias added.");
                return true;
            }
            return false;
        }

        [Obfuscation(Exclude = false)]
        internal static bool Delete(User user)
        {
            user.InputHandler = Delete;
            if (Command.GetArguments(user, GetAlias))
            {
                string cmd = user.GetCommandArg<string>(0);
                if (user.Aliases.ContainsKey(cmd))
                {
                    user.Aliases.Remove(cmd);
                    user.Send("Alias deleted.");
                }
                else
                {
                    user.Send("No such alias: " + cmd);
                }

                return true;
            }
            return false;
        }

        [Obfuscation(Exclude = false)]
        internal static bool GetAlias(User user)
        {
            string arg = user.GetArgument();
            if (string.IsNullOrEmpty(arg))
            {
                Menu menu = new Menu(GetAlias, "Enter alias: ");
                user.Menus.Enqueue(menu);
                return false;
            }
            else
            {
                user.CurrentCommandArgs.Add(arg);
                return true;
            }
        }

        [Obfuscation(Exclude = false)]
        internal static bool GetAliasedCommand(User user)
        {
            string arg = user.Command.Trim('\'', '"');
            if (string.IsNullOrEmpty(arg))
            {
                Menu menu = new Menu(GetAliasedCommand, "Enter command: ");
                user.Menus.Enqueue(menu);
                return false;
            }
            else
            {
                user.CurrentCommandArgs.Add(arg);
                user.Command = null;
                return true;
            }
        }
    }
}

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Reflection;

namespace Ultimud
{
    public class Commands
    {
        static public void Init(string path)
        {
            //commands_.Clear();
            //Add(new Command("commands", DoCommands));
            //Add(new Command("quit", DoQuit));
            //Add(new Command("set", DoSet));
            //Add(new Command("look", DoLook, false));
            //Add(new Command("travel", DoMove, false));
            //Add(new Command("follow", DoFollow, false));
            //Add(new Command("stop", DoStop));
            //Add(new Command("wait", DoWait));
            //Add(new Command("go", DoGo));
            //Add(new Command("?", DoHelp));
            //Add(new Command("stats", DoStats));
            //Add(new Command("abilities", DoAbilities));
            ////Add(new Command("restore", DoRestore));
            //Add(new Command("enter", DoEnter, false));
            //Add(new Command("open", DoOpen, false));
            //Add(new Command("close", DoClose, false));
            //Add(new Command("lock", DoLock, false));
            //Add(new Command("unlock", DoUnlock, false));
            ////Add(new Command("exit", DoExit));
            ////Add(new Command("createItem", DoCreateItem, Server.SelectItemBlueprint)));
            //Add(new Command("destroy", DoDestroyItem, false));
            //Add(new Command("pickup", DoPickUp, false));
            //Add(new Command("drop", DoDrop, false));
            //Add(new Command("inventory", DoInventory));
            //Add(new Command("use", DoUse, false));
            //Add(new Command("remove", DoRemove, false));
            //Add(new Command("give", DoGive, false));
            //Add(new Command("givemoney", DoGiveMoney, false));
            //Add(new Command("equipment", DoEquipment));
            //Add(new Command("status", DoStatus));
            //Add(new Command("put", DoPut, false));
            //Add(new Command("get", DoGet, false));
            ////Add(new Command("slay", DoSlay, false));
            //Add(new Command("who", DoWho));
            //Add(new Command("buy", DoBuy, false));
            //Add(new Command("sell", DoSell, false));
            //Add(new Command("money", DoMoney));
            //Add(new Command("forums", DoForums));
            //Add(new Command("learn", DoLearn, false));
            //Add(new Command("alias", DoAlias));
            //Add(new Command("delete", DoDelete));
            //Add(new Command("socials", DoSocials));
            //Add(new Command("channels", DoChannels));
            //Add(new Command("order", DoOrder, false));
            ////Script Commands.
            //Add(new Command("teach", DoTeach, true, true));
            //Add(new Command("peace", DoPeace, true, true));
            //Add(new Command("force", DoForce, true, true));
            //Add(new Command("createitem", DoCreateItem, true, true));
            //Add(new Command("print", DoPrint, true, true));
            //Add(new Command("summonnpc", DoSummonNpc, true, true));
        }
        /*
        static public bool DoCommands(User user)
        {
            Menu menu = new Menu(null, null);
            foreach (Command c in commands_)
            {
                if (c.SystemCommand)
                    continue;

                string pre = null;
                string post = null;
                if (!c.CanUseIncapacitated && user.Character.Incapacitated)
                {
                    pre = "{color:gray}";
                    post = "{color:reset}";
                }
                menu.AddOption(new Menu.Option(pre, c.Name, post));
            }
            user.Send(menu.ToString());
            return true;
        }
        static public bool DoSet(User user)
        {
            if (GetArguments(user, User.SelectSetting))
            {
                string cmd = user.GetCommandArg<string>(0);
                Server.InputHandler handler = User.GetHandler(cmd);
                if (handler == null)
                {
                    user.Send("Invalid setting: " + cmd);
                }
                else
                {
                    return handler(user);
                }

                return true;
            }

            return false;
        }
        static public bool DoQuit(User user)
        {
            if (user.Character.FightSystem != null)
            {
                user.Send("You are still involved in a fight!");
                return true;
            }

            String str = new String("You fade out of existance.", null, null, null, "{1} fades out of existence.", null);
            user.Character.Area.Send(str, user.Character, null, 0);
            user.Send("Goodbye.");
            Server.Instance.Unload(user, true);
            Server.Instance.Disconnect(user);
            Trace.WriteLine(user.Character.Name + " has quit.");
            Server.Instance.Settings.InfoChannel.Broadcast(InfoChannel.Event.Logout, user.Character.Name + " has logged out.");
            return true;
        }
        static public bool DoLook(User user)
        {
            if (string.IsNullOrEmpty(user.Command))
                user.Character.Look(true);
            else
                user.Character.LookAt(user.Command);
            return true;
        }
        static public bool DoMove(User user)
        {
            if (user.Character.CurrentAction != null)
                user.Character.Stop(false);

            if (GetArguments(user, Object.SelectDestination, Object.SelectSpeed))
            {
                object arg1 = user.GetArg(0);
                string arg2 = user.GetCommandArg<string>(1);
                int speed = 5;
                Stat speedStat = user.Character.GetStat("speed");
                if (speedStat != null)
                    speed = (int)speedStat.Current;

                try
                {
                    speed = Math.Max(0, Math.Min(int.Parse(arg2), speed));
                }
                catch
                {
                    //Do nothing.
                }

                if (arg1 is string)
                {
                    string arg = arg1 as string;
                    List<string> dirs = new List<string>();
                    dirs.AddRange(Enum.GetNames(typeof(Area.Direction)));
                    dirs.AddRange(Enum.GetNames(typeof(Area.ShortDirection)));
                    for (int x = 0; x < dirs.Count; ++x)
                    {
                        string str = dirs[x];
                        if (str.StartsWith(arg, StringComparison.CurrentCultureIgnoreCase))
                        {
                            Area.Direction dir = (Area.Direction)((x > 8 ? x - 9 : x));
                            if (speed < 1)
                            {
                                if (!user.Character.Move(dir,
                                    new String("You move {dir}.", null, null, null, "{1} leaves {dir}.", null),
                                    new String(null, null, null, null, "{1} arrives from the {dir}.", null), false))
                                    user.Send("Unable to move " + dir.ToString() + ".");
                            }
                            else
                                user.Character.ReplaceCurrentAction(new TravelAction(user.Character, dir, speed));
                            return true;
                        }
                    }

                    Object target = user.Character.FindNearbyObject<Object>(arg, (int)user.Character.GetStat("sight").Current, true);
                    if (target != null && target.Distance(user.Character) > 0)
                    {
                        user.Character.ReplaceCurrentAction(new TravelAction(user.Character, target, Math.Max(1, speed)));
                        return true;
                    }
                }
                else if (arg1 is Area.Direction || arg1 is Area.ShortDirection)
                {
                    Area.Direction dir = (Area.Direction)arg1;
                    if (speed < 1)
                    {
                        if (!user.Character.Move(dir,
                            new String("You move {dir}.", null, null, null, "{1} leaves {dir}.", null),
                            new String(null, null, null, null, "{1} arrives from the {dir}.", null), false))
                            user.Send("Unable to move " + dir.ToString() + ".");
                    }
                    else
                        user.Character.ReplaceCurrentAction(new TravelAction(user.Character, dir, Math.Max(1, speed)));
                    return true;
                }
                else if (arg1 is Object)
                {
                    Object target = arg1 as Object;
                    if (target != null && target.Distance(user.Character) > 0)
                    {
                        user.Character.ReplaceCurrentAction(new TravelAction(user.Character, target, Math.Max(1, speed)));
                        return true;
                    }
                }

                user.Send("Invalid destination.");
                return true;
            }

            return false;
        }
        static public bool DoFollow(User user)
        {
            string usage = 
@"{color:cyan}Usage: follow <path name> <direction> <speed>
Usage: follow <item or character name>";
            string arg1 = user.GetArgument();
            if (string.IsNullOrEmpty(arg1))
            {
                user.Send(usage);
                return true;
            }
            else
            {
                Object o = user.Character.FindNearbyObject<Object>(arg1, 0, true);
                if (o == null)
                {
                    user.Send("Target not found: " + arg1);
                    return true;
                }
                else
                {
                    if (o is Area.Path)
                    {
                        string arg2 = user.GetArgument();
                        if (string.IsNullOrEmpty(arg2))
                        {
                            user.Send("Direction not specified.");
                            return true;
                        }
                        else
                        {
                            Area.Path path = o as Area.Path;
                            Area.Direction dir = Area.Direction.Random;
                            Point[] points = path.GetNearestPoints(user.Character.Location.Point);
                            foreach (Point point in points)
                            {
                                if (point != Point.Empty)
                                {
                                    Area.Direction direction = user.Character.GetDirection(point);
                                    if (direction.ToString().StartsWith(arg2, StringComparison.CurrentCultureIgnoreCase))
                                    {
                                        dir = direction;
                                        break;
                                    }
                                }
                            }

                            if (dir == Area.Direction.Random)
                            {
                                user.Send("Invalid direction: " + arg2);
                            }
                            else
                            {
                                string arg3 = user.GetArgument();
                                if (string.IsNullOrEmpty(arg3))
                                {
                                    arg3 = "10";
                                }

                                int speed = 5;
                                Stat speedStat = user.Character.GetStat("speed");
                                if (speedStat != null)
                                {
                                    speed = (int)speedStat.Current;
                                }

                                speed = Math.Max(1, Math.Min(int.Parse(arg3), speed));
                                user.Character.ReplaceCurrentAction(new FollowAction(user.Character, path, dir, speed));
                            }
                        }
                    }
                    else
                    {
                        int speed = 5;
                        Stat speedStat = user.Character.GetStat("speed");
                        if (speedStat != null)
                        {
                            speed = (int)speedStat.Current;
                        }
                        speed = Math.Max(1, speed);
                        user.Character.ReplaceCurrentAction(new FollowAction(user.Character, o, 0, speed));
                    }
                }
            }

            return true;
        }
        static public bool DoStop(User user)
        {
            user.Character.Stop(false);
            return true;
        }
        static public bool DoWait(User user)
        {
            user.Character.Wait();
            return true;
        }
        static public bool DoGo(User user)
        {
            user.Character.Go();
            return true;
        }
        static public bool DoHelp(User user)
        {
            if (GetArguments(user, Help.SelectHelp))
            {
                string arg = user.GetCommandArg<string>(0).ToLower();
                Help help = Help.Get(arg);

                if (help == null)
                    user.Send("No help for: " + arg);
                else
                    user.Send(String.Format(help.Print()).TrimEnd('\r', '\n'));

                return true;
            }

            return false;
        }
        static public bool DoStats(User user)
        {
            if (user.Character.Stats.Count < 1)
            {
                user.Send("You have no stats.");
                return true;
            }

            user.Send(Server.Instance.Settings.StatsFormat.ToString(user.GetArgument(), user));
            return true;
        }
        static public bool DoAbilities(User user)
        {
            if (user.Character.Abilities.Count < 1)
                user.Send("You have no abilities.");
            else
            {
                Menu menu = new Menu();
                foreach (KeyValuePair<Guid, int> pair in user.Character.Abilities)
                {
                    Ability ability = Server.Instance.Get<Ability>(pair.Key);
                    if (ability != null)
                        menu.AddOption(new Menu.Option((ability.Improvable ? string.Format("{0,-4}", pair.Value) : string.Format("{0,-4}", "--")), ability.Name, (string.IsNullOrEmpty(ability.Shortcut) ? null : "<" + ability.Shortcut + ">")));
                }
                user.Send(menu.ToString());
            }
            //user.Send(Server.Instance.Settings.AbilitiesFormat.ToString(user.GetArgument(), user));
            return true;
        }
        static public bool DoRestore(User user)
        {
            user.Character.Restore(false);
            return true;
        }
        static public bool DoEnter(User user)
        {
            if (GetArguments(user, Area.SelectPortal))
            {
                IPortal portal = user.GetCommandArg<IPortal>(0);

                if (portal != null)
                    user.Character.Enter(portal);
                else
                    user.Send("Invalid target.");
                
                return true;
            }

            return false;
        }
        static public bool DoOpen(User user)
        {
            if (GetArguments(user, Object.SelectCloseable))
            {
                ICloseable o = user.GetCommandArg<ICloseable>(0);
                if (o == null)
                    user.Send("Invalid target.");
                else if (!o.Closeable)
                    user.Send("You can't do that.");
                else
                    user.Character.Open(o);
                return true;
            }

            return false;
        }
        static public bool DoClose(User user)
        {
            if (GetArguments(user, Object.SelectCloseable))
            {
                ICloseable o = user.GetCommandArg<ICloseable>(0);

                if (o == null)
                    user.Send("Invalid target.");
                else if (!o.Closeable)
                    user.Send("You can't do that.");
                else
                    user.Character.Close(o);
                
                return true;
            }

            return false;
        }
        static public bool DoLock(User user)
        {
            if (GetArguments(user, Object.SelectCloseable))
            {
                ICloseable o = user.GetCommandArg<ICloseable>(0);
                if (o == null)
                    user.Send("Invalid target.");
                else if (!o.Closeable)
                    user.Send("You can't do that.");
                else
                    user.Character.Lock(o);
                return true;
            }

            return false;
        }
        static public bool DoUnlock(User user)
        {
            if (GetArguments(user, Object.SelectCloseable))
            {
                ICloseable o = user.GetCommandArg<ICloseable>(0);
                if (o == null)
                    user.Send("Invalid target.");
                else if (!o.Closeable)
                    user.Send("You can't do that.");
                else
                    user.Character.Unlock(o);
                return true;
            }

            return false;
        }
        //static public bool DoExit(User user)
        //{
        //    Area.Exit exit = user.Character.GetExit();
        //    if (exit == null)
        //    {
        //        user.Send("There is no exit here.");
        //    }
        //    else
        //    {
        //        user.Character.ExitArea(exit);
        //    }
        //    return true;
        //}

        //static public bool DoCreateItem(User user)
        //{
        //    string arg1 = user.CurrentCommandArgs[0];
        //    Item selectedItem = Server.Instance.Get<Item>(arg1);
        //    if (selectedItem != null)
        //    {
        //        Item item = Item.Create(selectedItem);
        //        item.Parent = user.Character.Area;
        //        item.Location = user.Character.Location;
        //        String str = new String(
        //            "You have created {2}!",
        //            null,
        //            "{1} has created {2}!");
        //        user.Character.Area.SendToRoom(str, user.Character, item);
        //    }
        //    else
        //    {
        //        user.Send("Invalid target: " + arg1);
        //    }

        //    return true;
        //}
        static public bool DoDestroyItem(User user)
        {
            if (GetArguments(user, Character.SelectItem))
            {
                Item selectedItem = user.GetCommandArg<Item>(0);
                if (selectedItem != null)
                    user.Character.Destroy(selectedItem);
                else
                    user.Send("Item not found in inventory.");
                return true;
            }

            return false;
        }
        static public bool DoPickUp(User user)
        {
            if (GetArguments(user, Item.Select))
            {
                Item selectedItem = user.GetCommandArg<Item>(0);
                if (selectedItem != null)
                    user.Character.PickUp(selectedItem);
                else
                    user.Send("Invalid target.");
                return true;
            }

            return false;
        }
        static public bool DoDrop(User user)
        {
            if (GetArguments(user, Character.SelectItem))
            {
                Item selectedItem = user.GetCommandArg<Item>(0);
                if (selectedItem != null)
                    user.Character.Drop(selectedItem, false);
                else
                    user.Send("Item not found.");
                return true;
            }

            return false;
        }
        static public bool DoInventory(User user)
        {
            List<Item> list = user.Character.Inventory;
            if (list.Count < 1)
            {
                user.Send("Your inventory is empty.\r\nYou are currently using " + user.Character.Equipment.Count + " item(s).");
                return true;
            }

            Dictionary<string, List<Item>> dictionary = Utils.ConsolidateList<Item>(list);
            Menu menu = new Menu("You are carrying:");
            foreach (List<Item> items in dictionary.Values)
                menu.AddOption(new Menu.Option(null, items[0].Name, (items.Count > 1 ? "(" + items.Count + ")" : null)));

            user.Send(menu.ToString());
            return true;
        }
        static public bool DoUse(User user)
        {
            if(GetArguments(user, Character.SelectItem))
            {
                Item selectedItem = user.GetCommandArg<Item>(0);
                if (selectedItem != null)
                    user.Character.Use(selectedItem, false);
                else
                    user.Send("Item not found.");
                return true;
            }

            return false;
        }
        static public bool DoRemove(User user)
        {
            if (GetArguments(user, Character.SelectEquippedItem))
            {
                Item selectedItem = user.GetCommandArg<Item>(0);
                if (selectedItem != null)
                    user.Character.Remove(selectedItem.EquipLocation);
                else
                    user.Send("Item not found.");
                return true;
            }

            return false;
        }
        static public bool DoGive(User user)
        {
            if (GetArguments(user, Character.SelectItem, Character.SelectLocalCharacter))
            {
                Item selectedItem = user.GetCommandArg<Item>(0);
                if (selectedItem == null)
                    user.Send("Item not found.");
                else
                {
                    Character target = user.GetCommandArg<Character>(1);
                    if (target == null || target == user.Character)
                        user.Send("Invalid target.");
                    else
                    {
                        user.Character.Give(selectedItem, target);
                        if (selectedItem.ObjectFlags.Get((int)Object.ObjFlags.Money))
                            user.Character.Value -= selectedItem.Value;
                    }
                }
                return true;
            }

            return false;
        }
        static public bool DoGiveMoney(User user)
        {
            if (GetArguments(user, Character.SelectLocalCharacter, Character.SelectMoneyAmount))
            {
                Character ch = user.GetCommandArg<Character>(0);
                if (ch == null)
                    user.Send("Target not found.");
                else
                {
                    string arg = user.GetCommandArg<string>(1);
                    int amount = 0;
                    if (!int.TryParse(arg, out amount))
                        user.Send("Amount must be a number between 0 and " + user.Character.Value + ".");
                    else if (amount > user.Character.Value)
                        user.Send("You can't give more than you have.");
                    else
                    {
                        user.Character.Value -= amount;
                        ch.Value += amount;
                        user.Character.Area.Send(new String(
                            "You give {2} $" + amount + ".", "You give {2} $" + amount + ".",
                            "{1} gives you $" + amount + ".", "{1} gives you $" + amount + ".",
                            "{1} gives {2} some money.", null), user.Character, ch, 0);
                    }
                }
                return true;
            }

            return false;
        }
        static public bool DoEquipment(User user)
        {
            if (user.Character.Equipment.Count == 0)
            {
                user.Send("You aren't using anything.");
                return true;
            }

            //Menu menu = new Menu("You are using:");

            //lock (user.Character.Equipment)
            //{
            //    foreach (KeyValuePair<Character.BodyParts, Item> pair in user.Character.Equipment)
            //    {
            //        menu.AddOption(new Menu.Option(null, "({color:yellow}" + pair.Key.ToString() + "{color:reset})", pair.Value.Name));
            //    }
            //}

            //user.Send(menu.ToString());
            user.Send(Server.Instance.Settings.EqFormat.ToString(user.GetArgument(), user.Character));
            return true;
        }
        static public bool DoStatus(User user)
        {
            bool found = false;
            string str = string.Format("{0,-20}  {1,10}\r\n", "Status", "Duration");
            str += "--------------------------------\r\n";
            foreach (StatusEffect status in user.Character.StatusEffects)
            {
                StatusEffectBlueprint bp = status.Blueprint;
                if (bp != null && bp.Visible)
                {
                    str += string.Format("{0,-20}  {1,10}\r\n", bp.Name,
                        (status.IsPermanent || status.Duration == null ? string.Empty : status.SecondsRemaining.ToString()));
                    found = true;
                }
            }

            if (found)
            {
                user.Send(str.TrimEnd('\r', '\n'));
            }
            else
            {
                user.Send("You aren't affected by any status effects.");
            }
            return true;
        }
        static public bool DoPut(User user)
        {
            if (GetArguments(user, Character.SelectItem, Character.SelectContainer))
            {
                Item selectedItem = user.GetCommandArg<Item>(0);
                if (selectedItem == null)
                    user.Send("Item not found.");
                else
                {
                    IContainer container = user.GetCommandArg<IContainer>(1);
                    if (container == null || container == selectedItem || !((Object)container).ObjectFlags.Get((int)Object.ObjFlags.Container))
                        user.Send("Invalid container.");
                    else if (container.Closed)
                        user.Send(String.Capitalize(container.Name) + " is closed.");
                    else if (container.Locked)
                        user.Send(String.Capitalize(container.Name) + " is locked.");
                    else
                        user.Character.Put(selectedItem, container);
                }
                return true;
            }

            return false;
        }
        static public bool DoGet(User user)
        {
            if (GetArguments(user, Character.SelectContainer, Character.SelectItemFromContainer))
            {
                IContainer container = user.GetCommandArg<IContainer>(0);
                if (container == null || !((Object)container).ObjectFlags.Get((int)Object.ObjFlags.Container))
                    user.Send("Invalid container.");
                else
                {
                    Item target = user.GetCommandArg<Item>(1);
                    if (target == null || target == container)
                        user.Send("Item not found in container.");
                    else
                        user.Character.Get(target, container);
                }
                return true;
            }

            return false;
        }
        static public bool DoSlay(User user)
        {
            if (GetArguments(user, Character.SelectLocalCharacter))
            {
                Character ch = user.GetCommandArg<Character>(0);
                if (ch != null)
                    user.Character.Destroy(ch);
                else
                    user.Send("Target not found.");
                return true;
            }

            return false;
        }
        static public bool DoWho(User user)
        {
            user.Send(Server.Instance.Settings.WhoFormat.ToString(user.GetArgument(), null));
            return true;
        }
        static public bool DoBuy(User user)
        {
            if (GetArguments(user, Shop.SelectItemToBuy))
            {
                string arg1 = user.GetCommandArg<string>(0);
                List<Shop> shops = user.Character.NearbyObjects<Shop>(0, true);
                if (shops.Count < 1)
                {
                    user.Send("There is nothing to buy here.");
                    return true;
                }

                Shop shop = shops[0];
                Item selectedItem = shop.FindItem(arg1);
                if (selectedItem != null)
                {
                    shop.Sell(selectedItem, user.Character);
                }
                else
                {
                    user.Send("This shop doesn't carry any items with that name.");
                }
                return true;
            }

            return false;
        }
        static public bool DoSell(User user)
        {
            if (GetArguments(user, Shop.SelectItemToSell))
            {
                string arg1 = user.GetCommandArg<string>(0);
                List<Shop> shops = user.Character.NearbyObjects<Shop>(0, true);
                if (shops.Count < 1)
                {
                    user.Send("There is no place to sell here.");
                    return true;
                }

                Shop shop = shops[0];
                Item selectedItem = user.Character.FindItemInInventory(arg1);
                if (selectedItem != null)
                    shop.Buy(selectedItem, user.Character);
                else
                    user.Send("Item not found in inventory: " + arg1);
                return true;
            }

            return false;
        }
        static public bool DoMoney(User user)
        {
            user.Send("You have $" + user.Character.Value.ToString(Server.Instance.Settings.NumberFormat) + ".");
            return true;
        }
        static public bool DoForums(User user)
        {
            if(GetArguments(user, MessageBoard.GetCommand))
            {
                string arg = user.GetCommandArg<string>(0);
                Server.InputHandler handler = MessageBoard.GetHandler(arg);
                if (handler == null)
                {
                    user.Send("Invalid option: " + arg);
                    return true;
                }
                else
                {
                    user.CurrentCommandArgs.Clear();
                    return handler(user);
                }
            }

            return false;
        }
        static public bool DoLearn(User user)
        {
            List<Npc> npcs = user.Character.NearbyObjects<Npc>(0, true, user.Character);
            if (npcs.Count < 1 || !npcs[0].CanTeach)
            {
                user.Send("No one is teaching anything here.");
                return true;
            }

            Npc npc = npcs[0];

            if (GetArguments(user, npc.SelectAbility))
            {
                Ability ability = user.GetCommandArg<Ability>(0);
                if (ability == null || !npc.Reset.TeachableAbilities.ContainsKey(ability.Id))
                {
                    user.Send("They don't teach that ability.");
                    return true;
                }

                if (user.Character.Abilities.ContainsKey(ability.Id))
                {
                    user.Send("You already know that ability.");
                    return true;
                }

                int cost = npc.Reset.TeachableAbilities[ability.Id];
                if (cost > user.Character.Value)
                {
                    user.Send("You don't have enough money.");
                    return true;
                }

                user.Character.Value -= cost;
                user.Character.Abilities.Add(ability.Id, 0);
                user.Send(Server.Instance.Settings.GetColorCode(Settings.Text.AbilityImprovements) + 
                    "You have learned a new ability! (" + ability.Name + ")");
                return true;
            }

            return false;
        }
        static public bool DoAlias(User user)
        {
            if (GetArguments(user, Alias.GetCommand))
            {
                string arg = user.GetCommandArg<string>(0);
                Server.InputHandler handler = Alias.GetHandler(arg);
                if (handler == null)
                {
                    user.Send("Invalid option: " + arg);
                    return true;
                }
                else
                {
                    user.CurrentCommandArgs.Clear();
                    return handler(user);
                }
            }

            return false;
        }
        static public bool DoDelete(User user)
        {
            if (GetArguments(user, User.EnterPassword, User.ConfirmPassword))
            {
                string arg1 = user.GetCommandArg<string>(0);
                string arg2 = user.GetCommandArg<string>(1);
                if (arg1 == arg2 && arg2 == user.Password)
                {
                    user.Send("Thank you for playing " + Server.Instance.Settings.Name + ".");
                    Server.Instance.Disconnect(user);
                    user.Character.DropItems(false);
                    user.Character.Area.Send(new String(null, null, null, null, "{1} disintegrates!", null), user.Character, null, 0);
                    Server.Instance.Settings.InfoChannel.Broadcast(InfoChannel.Event.Login, user.Character.Name + " has deleted.");
                    Server.Instance.Unload(user, false);
                    user.Delete();
                }
                else
                {
                    user.Send("Invalid password or passwords do not match.");
                }
                return true;
            }

            return false;
        }
        static public bool DoSocials(User user)
        {
            int maxLength = 0;
            SortedDictionary<string, Social> socials = new SortedDictionary<string, Social>();
            foreach (Social social in Server.Instance.DataSets[typeof(Social)].Values)
            {
                socials.Add(social.Name, social);
                if (social.Name.Length > maxLength)
                {
                    maxLength = social.Name.Length;
                }
            }

            if (socials.Count == 0)
            {
                user.Send("There are no socials.");
                return true;
            }

            int maxColumns = 70 / maxLength;
            string str = string.Empty;
            int count = 0;
            foreach (Social social in socials.Values)
            {
                str += string.Format("{0,-" + maxLength + "}  ", social.Name);
                if (++count % maxColumns == 0)
                {
                    str += "\r\n";
                }
            }
            user.Send(str.TrimEnd('\r', '\n'));
            return true;
        }
        static public bool DoChannels(User user)
        {
            Menu menu = new Menu(null, null);
            foreach (Channel channel in Server.Instance.DataSets[typeof(Channel)].Values)
                menu.AddOption(channel.Name);

            if (menu.Options != null)
                user.Send(menu.ToString());
            else
                user.Send("There are no channels.");

            return true;
        }
        static public bool DoOrder(User user)
        {
            if (GetArguments(user, Character.SelectLocalCharacter, User.EnterMessage))
            {
                Character target = user.GetCommandArg<Character>(0);
                string arg2 = user.GetCommandArg<string>(1);

                if (target == null || target == user.Character)
                    user.Send("Invalid target.");
                else if (target.Master != user.Character)
                    user.Send("They aren't under your control.");
                else
                {
                    User u = target.User;
                    if (u == null)
                        u = new User(target);
                    target.ReplaceCurrentAction(new CommandAction(arg2, 1, u));
                    user.Send("You order " + target.Name + " to '" + arg2 + "'.");
                }
                return true;
            }

            return false;
        }

        //Script Commands
        static public bool DoTeach(User user)
        {
            Npc npc = user.Character as Npc;
            if(npc == null) 
                return true;

            string arg1 = user.GetArgument();
            string arg2 = user.GetArgument();

            if (!string.IsNullOrEmpty(arg1) && !string.IsNullOrEmpty(arg2))
            {
                Character ch = npc.FindNearbyObject<Character>(arg1, 0, true);
                Ability ability = Server.Instance.Get<Ability>(arg2);
                if (ch != null &&
                    ability != null &&
                    !ch.Abilities.ContainsKey(ability.Id) &&
                    ch.CanUse(ability, false))
                {
                    ch.Abilities.Add(ability.Id, 0);
                    ch.Send(Server.Instance.Settings.GetColorCode(Settings.Text.AbilityImprovements) +
                        "You have learned a new ability! (" + ability.Name + ")");
                }
            }
            return true;
        }
        static public bool DoPeace(User user)
        {
            if (user.Character.FightSystem != null)
            {
                user.Character.FightSystem.Finish(null);
                if (user.Character.CurrentAction is AbilityAction)
                    user.Character.Stop(false);
            }
            return true;
        }
        static public bool DoForce(User user)
        {
            Npc npc = user.Character as Npc;
            if (npc == null)
                return true;

            string arg1 = user.GetArgument();
            string arg2 = user.GetArgument();

            if (!string.IsNullOrEmpty(arg1) && !string.IsNullOrEmpty(arg2))
            {
                Character ch = npc.FindNearbyObject<Character>(arg1, 0, false);
                if (ch.User != null)
                {
                    ch.User.Command = arg2;
                    Server.Instance.ProcessCommand(ch.User, true);
                    ch.User.FinishInput();
                }
            }
            return true;
        }
        static public bool DoCreateItem(User user)
        {
            string arg1 = user.Command;
            if (!string.IsNullOrEmpty(arg1))
            {
                Item bp = Server.Instance.Get<Item>(arg1);
                if (bp != null)
                {
                    Item item = bp.GetType().GetMethod("Create", 
                        BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { bp }) as Item;
                    if (item != null)
                    {
                        item.Parent = user.Character;
                    }
                }
            }
            return true;
        }
        static public bool DoPrint(User user)
        {
            user.Send(user.Command);
            return true;
        }
        static public bool DoSummonNpc(User user)
        {
            Npc bp = Server.Instance.Get<Npc>(user.Command);
            if (bp != null)
            {
                Npc npc = Npc.Create(bp);
                npc.Location = user.Character.Location;
                npc.Master = user.Character;
            }
            return true;
        }*/
    }
}

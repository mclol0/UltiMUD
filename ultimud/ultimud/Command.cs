using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Drawing;
using System.Drawing.Design;
using System.ComponentModel;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using System.Diagnostics;
using System.IO;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using LuaInterface;

namespace Ultimud
{
    [Serializable]
    [Obfuscation(Exclude = true)]
    public class Command : ISaveable
    {
        static List<Command> commands_ = new List<Command>();
        static Dictionary<Guid, Guid> lastTellFrom_ = new Dictionary<Guid, Guid>();
        string name_;
        string script_;
        bool incapacitated_ = true;
        bool system_ = false;
        List<MenuFunction> menufunctions_ = new List<MenuFunction>();
        uint priority_;
        bool hidden_;
        List<string> aliases_ = new List<string>();

        [Obfuscation(Exclude = false)]
        [NonSerialized]
        Lua lua_;

        [Obfuscation(Exclude = false)]
        [NonSerialized]
        Server.InputHandler[] menuhandlers_;

        #region Properties
        public static List<Command> Commands
        {
            get { return commands_; }
            set { commands_ = value; }
        }
        static string Path
        {
            get
            {
                if (Server.Instance != null)
                    return Server.Instance.ServerPath + "\\Commands";
                if (MainForm.Instance != null)
                    return MainForm.Instance.ServerPath + "\\Commands";
                return System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data\\muds\\Unnamed\\Commands");
            }
        }
        public string Name
        {
            get { return this.name_; }
            set 
            {
                if (!string.IsNullOrEmpty(this.name_) && this.name_ != value)
                    this.Delete();
                this.name_ = value;
            }
        }

        [Editor(typeof(StringEditor), typeof(UITypeEditor))]
        public string Script
        {
            get { return this.script_; }
            set { this.script_ = value; }
        }

        [DefaultValue(true)]
        [Description("Can this ability be used if unconscious or dead?")]
        public bool CanUseIncapacitated
        {
            get { return this.incapacitated_; }
            set { this.incapacitated_ = value; }
        }
        
        [DefaultValue(false)]
        [Description("If true, this command cannot be used by players. It can only be invoked through DoCommand().")]
        public bool SystemCommand
        {
            get { return this.system_; }
            set { this.system_ = value; }
        }
        
        [TypeConverter(typeof(Damdai.TypeConverters.ListToStringConverter))]
        [Description("These menu functions are called for each argument required for this command.")]
        public List<MenuFunction> MenuFunctions
        {
            get { return this.menufunctions_; }
            set { this.menufunctions_ = value; }
        }

        [DefaultValue(0)]
        [Description("The priority of this command when more than 1 command is matched by user input. (0 = no priority, 1 = top priority)")]
        public uint Priority
        {
            get { return this.priority_; }
            set { this.priority_ = value; }
        }
        
        [Description("This property can be used in your COMMANDS script to determine if it will appear in the command list.")]
        [DefaultValue(false)]
        public bool Hidden
        {
            get { return this.hidden_; }
            set { this.hidden_ = value; }
        }
        
        [Description("This is a list of aliases that a user can enter to invoke this command instead of the command name. (Example: name=northeast, alias=ne)")]
        [Editor("System.Windows.Forms.Design.StringCollectionEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(System.Drawing.Design.UITypeEditor))]
        [TypeConverter(typeof(Damdai.TypeConverters.ListToStringConverter))]
        public List<string> Aliases
        {
            get { return this.aliases_; }
            set { this.aliases_ = value; }
        }
        #endregion

        internal static void Init(string loadpath, string serverpath)
        {
            lock (commands_)
            {
                commands_.Clear();
                string cmdspath = !string.IsNullOrEmpty(loadpath) ? (loadpath + "\\Commands") : null;
                if (!string.IsNullOrEmpty(cmdspath) && Directory.Exists(cmdspath))
                {
                    foreach (FileInfo file in new DirectoryInfo(cmdspath).GetFiles("*.cmd"))
                    {
                        Command cmd = Damdai.Serialization.Xml.Deserialize<Command>(file.FullName);
                        commands_.Add(cmd);
                    }
                }
                else if (!string.IsNullOrEmpty(serverpath) && Directory.Exists(serverpath + "\\Commands"))
                {
                    foreach (FileInfo file in new DirectoryInfo(serverpath + "\\Commands").GetFiles("*.cmd"))
                    {
                        Command cmd = Damdai.Serialization.Xml.Deserialize<Command>(file.FullName);
                        commands_.Add(cmd);
                    }
                }

                SeedDefaultCommands(!string.IsNullOrEmpty(loadpath) ? loadpath : serverpath);

                foreach (Command existingCmd in commands_)
                {
                    if (existingCmd.Name != null && existingCmd.Name.Equals("chat", StringComparison.OrdinalIgnoreCase) && existingCmd.Aliases != null)
                        existingCmd.Aliases.RemoveAll(delegate(string a) { return a.Equals("gossip", StringComparison.OrdinalIgnoreCase); });
                }

                commands_.Sort(delegate(Command cmd1, Command cmd2)
                {
                    return cmd1.priority_.CompareTo(cmd2.priority_);
                });
            }
        }

        static bool IsLegacyStubScript(string script)
        {
            if (string.IsNullOrEmpty(script))
                return true;
            string trimmed = script.Trim();
            return trimmed.StartsWith("luanet.load_assembly('Ultimud')") &&
                   trimmed.Contains("finished = Command.Do") &&
                   !trimmed.Contains("--");
        }

        static string GetDefaultLuaScript(string name, string methodName)
        {
            switch (name.ToLowerInvariant())
            {
                case "withdraw":
                    return
@"-- ============================================================================
-- Command: withdraw
-- Syntax : withdraw <amount|all>
-- Customize REQUIRE_BANK_LOCATION, WITHDRAW_FEE_PERCENT, or messages below!
-- ============================================================================
local REQUIRE_BANK_LOCATION = false
local WITHDRAW_FEE_PERCENT = 0

local arg = user:GetArgument()
if arg == nil or arg == '' then
    user:Send('Usage: withdraw <amount|all>')
    return true
end

if REQUIRE_BANK_LOCATION and Command.GetShop(user) == nil then
    user:Send('You must be at a bank or shop to withdraw gold.')
    return true
end

local amount = 0
if string.lower(arg) == 'all' then
    amount = user.BankBalance
else
    amount = Command.ToInt(arg, -1)
    if amount <= 0 then
        user:Send('Please specify a valid positive amount to withdraw.')
        return true
    end
end

if amount <= 0 or amount > user.BankBalance then
    user:Send(""You don't have that much gold in your bank account."")
    return true
end

local fee = math.floor(amount * WITHDRAW_FEE_PERCENT / 100)
local received = amount - fee

user.BankBalance = user.BankBalance - amount
char.Value = char.Value + received

user:Send('You withdraw $' .. Command.FormatNumber(received) .. '. Remaining bank balance: $' .. Command.FormatNumber(user.BankBalance) .. '.')
return true";

                case "deposit":
                    return
@"-- ============================================================================
-- Command: deposit
-- Syntax : deposit <amount|all>
-- Customize REQUIRE_BANK_LOCATION, DEPOSIT_FEE_PERCENT, or messages below!
-- ============================================================================
local REQUIRE_BANK_LOCATION = false
local DEPOSIT_FEE_PERCENT = 0

local arg = user:GetArgument()
if arg == nil or arg == '' then
    user:Send('Usage: deposit <amount|all>')
    return true
end

if REQUIRE_BANK_LOCATION and Command.GetShop(user) == nil then
    user:Send('You must be at a bank or shop to deposit gold.')
    return true
end

local amount = 0
if string.lower(arg) == 'all' then
    amount = char.Value
else
    amount = Command.ToInt(arg, -1)
    if amount <= 0 then
        user:Send('Please specify a valid positive amount to deposit.')
        return true
    end
end

if amount <= 0 or amount > char.Value then
    user:Send(""You don't have that much gold on hand."")
    return true
end

local fee = math.floor(amount * DEPOSIT_FEE_PERCENT / 100)
local credited = amount - fee

char.Value = char.Value - amount
user.BankBalance = user.BankBalance + credited

user:Send('You deposit $' .. Command.FormatNumber(credited) .. '. New bank balance: $' .. Command.FormatNumber(user.BankBalance) .. '.')
return true";

                case "bank":
                    return
@"-- ============================================================================
-- Command: bank (alias: balance)
-- Syntax : bank [deposit|withdraw <amount>]
-- Customize BANK_HEADER or statement formatting below!
-- ============================================================================
local BANK_HEADER = '{color:brightyellow}=== Royal Bank Statement ==={color:reset}'

local sub = user:GetArgument()
if sub ~= nil and sub ~= '' then
    local lower = string.lower(sub)
    if lower == 'deposit' then
        return Command.DoDeposit(user)
    elseif lower == 'withdraw' then
        return Command.DoWithdraw(user)
    end
end

user:Send(BANK_HEADER .. '\r\n' ..
    'Gold on Hand: $' .. Command.FormatNumber(char.Value) .. '\r\n' ..
    'Bank Balance: $' .. Command.FormatNumber(user.BankBalance))
return true";

                case "money":
                    return
@"-- ============================================================================
-- Command: money (alias: gold)
-- Customize currency display below!
-- ============================================================================
local CURRENCY_SYMBOL = '$'
user:Send('You have ' .. CURRENCY_SYMBOL .. Command.FormatNumber(char.Value) .. '.')
return true";

                case "sacrifice":
                    return
@"-- ============================================================================
-- Command: sacrifice (alias: junk)
-- Syntax : sacrifice <item>
-- Customize MIN_REWARD, MAX_REWARD, VALUE_DIVISOR, and message below!
-- ============================================================================
local MIN_REWARD = 1
local MAX_REWARD = 10
local VALUE_DIVISOR = 10

local arg = user:GetArgument()
if arg == nil or arg == '' then
    user:Send('Sacrifice what?')
    return true
end

local item = Command.FindItemHereOrInv(user, arg)
if item == nil then
    user:Send(""You don't see that item here."")
    return true
end

local reward = math.floor(item.Value / VALUE_DIVISOR) + 1
if reward < MIN_REWARD then reward = MIN_REWARD end
if reward > MAX_REWARD then reward = MAX_REWARD end

local itemName = item.Name
item.Parent = nil
char.Value = char.Value + reward

user:Send('You sacrifice ' .. itemName .. ' and receive $' .. reward .. '.')
return true";

                case "north":
                case "south":
                case "east":
                case "west":
                case "northeast":
                case "northwest":
                case "southeast":
                case "southwest":
                    string capDir = char.ToUpper(name[0]) + name.Substring(1).ToLower();
                    return
@"-- ============================================================================
-- Command: " + name + @"
-- Customize movement messages below!
-- ============================================================================
local DIRECTION   = '" + capDir + @"'
local LEAVE_SELF  = 'You move {dir}.'
local LEAVE_ROOM  = '{1} leaves {dir}.'
local ARRIVE_ROOM = '{1} arrives from the {dir}.'

return Command.MoveDir(user, DIRECTION, LEAVE_SELF, LEAVE_ROOM, ARRIVE_ROOM)";

                case "stand":
                    return
@"-- ============================================================================
-- Command: stand
-- Customize position change messages below!
-- ============================================================================
return Command.SetPosition(user, 'Stopped',
    'You stand up.',
    '{1} stands up.',
    'You are already standing.')";

                case "sit":
                    return
@"-- ============================================================================
-- Command: sit
-- Customize position change messages below!
-- ============================================================================
return Command.SetPosition(user, 'Sitting',
    'You sit down.',
    '{1} sits down.',
    'You are already sitting.')";

                case "rest":
                    return
@"-- ============================================================================
-- Command: rest
-- Customize position change messages below!
-- ============================================================================
return Command.SetPosition(user, 'Resting',
    'You sit down and rest.',
    '{1} sits down and rests.',
    'You are already resting.')";

                case "sleep":
                    return
@"-- ============================================================================
-- Command: sleep
-- Customize position change messages below!
-- ============================================================================
return Command.SetPosition(user, 'Sleeping',
    'You lie down and go to sleep.',
    '{1} lies down and goes to sleep.',
    'You are already asleep.')";

                case "look":
                    return
@"-- ============================================================================
-- Command: look (alias: l)
-- Syntax : look [target]
-- ============================================================================
if user.Command == nil or user.Command == '' then
    user:Send(char:GenerateLookDescription(false))
    return true
end
return Command.DoExamine(user)";

                case "who":
                    return
@"-- ============================================================================
-- Command: who
-- Displays online players using Server Settings -> WhoFormat
-- ============================================================================
local filter = user:GetArgument()
user:Send(settings.WhoFormat:ToString(filter, user))
return true";

                case "stats":
                    return
@"-- ============================================================================
-- Command: stats (aliases: score, sc)
-- Displays character sheet using Server Settings -> StatsFormat
-- ============================================================================
local filter = user:GetArgument()
user:Send(settings.StatsFormat:ToString(filter, user))
return true";

                case "equipment":
                    return
@"-- ============================================================================
-- Command: equipment (alias: eq)
-- Displays equipped items using Server Settings -> EqFormat
-- ============================================================================
if char.Equipment.Count == 0 then
    user:Send(""You aren't using anything."")
    return true
end
user:Send(settings.EqFormat:ToString(user:GetArgument(), char))
return true";

                case "abilities":
                    return
@"-- ============================================================================
-- Command: abilities (aliases: skills, spells)
-- Displays learned abilities using Server Settings -> AbilitiesFormat
-- ============================================================================
if char.Abilities.Count < 1 then
    user:Send('You have no abilities.')
    return true
end
user:Send(settings.AbilitiesFormat:ToString(user:GetArgument(), user))
return true";

                case "say":
                    return
@"-- ============================================================================
-- Command: say (alias: ')
-- Syntax : say <message>
-- Customize SAY_COLOR and speech formatting below!
-- ============================================================================
local SAY_COLOR = '{color:brightwhite}'

local msg = user.Command
user.Command = nil
if msg == nil or msg == '' then
    user:Send('Say what?')
    return true
end

if Command.IsSilenced(user) then
    user:Send('You are silenced.')
    return true
end

Command.BroadcastRoom(user,
    SAY_COLOR .. 'You say, ""' .. msg .. '""{color:reset}',
    SAY_COLOR .. '{1} says, ""' .. msg .. '""{color:reset}',
    true)
return true";

                case "chat":
                    return
@"-- ============================================================================
-- Command: chat (alias: ooc)
-- Syntax : chat <message>
-- Customize CHAT_COLOR and TAG below!
-- ============================================================================
local CHAT_COLOR = '{color:brightcyan}'
local CHAT_TAG   = '[CHAT]'

local msg = user.Command
user.Command = nil
if msg == nil or msg == '' then
    user:Send('Chat what?')
    return true
end

if Command.IsSilenced(user) then
    user:Send('You are silenced.')
    return true
end

Command.BroadcastGlobal(user,
    CHAT_COLOR .. CHAT_TAG .. ' You: ' .. msg .. '{color:reset}',
    CHAT_COLOR .. CHAT_TAG .. ' ' .. char.Name .. ': ' .. msg .. '{color:reset}',
    true)
return true";

                case "tell":
                    return
@"-- ============================================================================
-- Command: tell
-- Syntax : tell <player> <message>
-- Customize TELL_COLOR and messages below!
-- ============================================================================
local TELL_COLOR = '{color:brightmagenta}'

local targetName = user:GetArgument()
local msg = user.Command
user.Command = nil
if targetName == nil or targetName == '' or msg == nil or msg == '' then
    user:Send('Usage: tell <player> <message>')
    return true
end

if Command.IsSilenced(user) then
    user:Send('You are silenced.')
    return true
end

local targetUser = Command.FindOnlineUser(targetName)
if targetUser == nil then
    user:Send('No player by that name is online.')
    return true
end

if targetUser:IsIgnoring(char.Name) then
    user:Send(targetUser.Character.Name .. ' is ignoring tells from you.')
    return true
end

Command.RecordLastTell(targetUser, user)
user:Send(TELL_COLOR .. 'You tell ' .. targetUser.Character.Name .. ', ""' .. msg .. '""{color:reset}')
if targetUser.IsAfk then
    local reason = ''
    if targetUser.AfkMessage ~= nil and targetUser.AfkMessage ~= '' then
        reason = ' (' .. targetUser.AfkMessage .. ')'
    end
    user:Send('{color:yellow}' .. targetUser.Character.Name .. ' is currently AFK' .. reason .. '.{color:reset}')
end
targetUser:Send(TELL_COLOR .. char.Name .. ' tells you, ""' .. msg .. '""{color:reset}')
return true";

                case "reply":
                    return
@"-- ============================================================================
-- Command: reply
-- Syntax : reply <message>
-- ============================================================================
local TELL_COLOR = '{color:brightmagenta}'

local msg = user.Command
user.Command = nil
if msg == nil or msg == '' then
    user:Send('Reply what?')
    return true
end

local targetUser = Command.GetLastTellSender(user)
if targetUser == nil then
    user:Send('No one has sent you a tell recently.')
    return true
end

if targetUser:IsIgnoring(char.Name) then
    user:Send(targetUser.Character.Name .. ' is ignoring tells from you.')
    return true
end

Command.RecordLastTell(targetUser, user)
user:Send(TELL_COLOR .. 'You tell ' .. targetUser.Character.Name .. ', ""' .. msg .. '""{color:reset}')
if targetUser.IsAfk then
    local reason = ''
    if targetUser.AfkMessage ~= nil and targetUser.AfkMessage ~= '' then
        reason = ' (' .. targetUser.AfkMessage .. ')'
    end
    user:Send('{color:yellow}' .. targetUser.Character.Name .. ' is currently AFK' .. reason .. '.{color:reset}')
end
targetUser:Send(TELL_COLOR .. char.Name .. ' tells you, ""' .. msg .. '""{color:reset}')
return true";

                case "whisper":
                    return
@"-- ============================================================================
-- Command: whisper
-- Syntax : whisper <target> <message>
-- ============================================================================
local WHISPER_COLOR = '{color:magenta}'

local targetName = user:GetArgument()
local msg = user.Command
user.Command = nil
if targetName == nil or targetName == '' or msg == nil or msg == '' then
    user:Send('Usage: whisper <target> <message>')
    return true
end

local target = Command.FindCharacterInRoom(user, targetName)
if target == nil then
    user:Send(""They aren't here."")
    return true
end

if target.User ~= nil and target.User:IsIgnoring(char.Name) then
    user:Send(target.Name .. ' is ignoring you.')
    return true
end

Command.BroadcastTargetRoom(user, target,
    WHISPER_COLOR .. 'You whisper to {2}, ""' .. msg .. '""{color:reset}',
    WHISPER_COLOR .. '{1} whispers to you, ""' .. msg .. '""{color:reset}',
    '{1} whispers something quietly to {2}.',
    true)
return true";

                case "emote":
                    return
@"-- ============================================================================
-- Command: emote (alias: :)
-- Syntax : emote <action>
-- ============================================================================
local msg = user.Command
user.Command = nil
if msg == nil or msg == '' then
    user:Send('Emote what?')
    return true
end

Command.BroadcastRoom(user, char.Name .. ' ' .. msg, '{1} ' .. msg, false)
return true";

                case "description":
                    return
@"-- ============================================================================
-- Command: description (alias: desc)
-- Syntax : description [text|clear]
-- ============================================================================
local desc = user.Command
user.Command = nil
if desc == nil or desc == '' then
    if char.Description == nil or char.Description == '' then
        user:Send('You have no character description set. Usage: description <text|clear>')
    else
        user:Send('Your description:\r\n' .. char.Description)
    end
    return true
end

local lower = string.lower(desc)
if lower == 'clear' or lower == 'none' then
    char.Description = ''
    user:Send('Your description has been cleared.')
else
    char.Description = desc
    user:Send('Your description has been updated.')
end
return true";

                case "title":
                    return
@"-- ============================================================================
-- Command: title
-- Syntax : title [text|clear]
-- ============================================================================
local title = user.Command
user.Command = nil
if title == nil or title == '' then
    if user.Title == nil or user.Title == '' then
        user:Send('You have no title set. Usage: title <text|clear>')
    else
        user:Send('Your current title is: ' .. char.Name .. ' ' .. user.Title)
    end
    return true
end

local lower = string.lower(title)
if lower == 'clear' or lower == 'none' then
    user.Title = ''
    user:Send('Your title has been cleared.')
else
    user.Title = title
    user:Send('Your title is now: ' .. char.Name .. ' ' .. user.Title)
end
return true";

                case "afk":
                    return
@"-- ============================================================================
-- Command: afk
-- Syntax : afk [reason]
-- ============================================================================
local msg = user.Command
user.Command = nil
if msg ~= nil and msg ~= '' then
    user.IsAfk = true
    user.AfkMessage = msg
    user:Send('{color:yellow}You are now AFK (' .. user.AfkMessage .. ').{color:reset}')
else
    user.IsAfk = not user.IsAfk
    if not user.IsAfk then
        user.AfkMessage = ''
        user:Send('{color:brightgreen}You are no longer AFK.{color:reset}')
    else
        user:Send('{color:yellow}You are now AFK.{color:reset}')
    end
end
return true";

                case "ignore":
                    return
@"-- ============================================================================
-- Command: ignore
-- Syntax : ignore [player]
-- ============================================================================
local target = user:GetArgument()
return Command.ToggleIgnore(user, target)";

                case "recall":
                    return
@"-- ============================================================================
-- Command: recall (alias: home)
-- Syntax : recall
-- Customize ALLOW_IN_COMBAT, RECALL_COST, and messages below!
-- ============================================================================
local ALLOW_IN_COMBAT = false
local RECALL_COST     = 0

if not ALLOW_IN_COMBAT and char.FightSystem ~= nil then
    user:Send(""You cannot recall while fighting! Use 'flee' instead."")
    return true
end

local destArea = server.MainArea or char.Area
if destArea == nil then
    user:Send('There is nowhere to recall to.')
    return true
end

if RECALL_COST > 0 then
    if char.Value < RECALL_COST then
        user:Send('You need $' .. RECALL_COST .. ' to recall.')
        return true
    end
    char.Value = char.Value - RECALL_COST
end

if char.CurrentAction ~= nil then
    char:Stop(true)
end

Command.BroadcastRoom(user, nil, '{1} disappears in a flash of bright light.', false)
char.Location = Location(destArea, destArea.Start)
user:Send('{color:brightcyan}You pray for recall and are transported back to safety!{color:reset}')
Command.BroadcastRoom(user, nil, '{1} appears in a flash of bright light.', false)
Command.DoLook(user)
return true";

                case "consider":
                    return
@"-- ============================================================================
-- Command: consider (alias: con)
-- Syntax : consider <target>
-- Customize power ratio thresholds and difficulty messages below!
-- ============================================================================
local targetName = user:GetArgument()
if targetName == nil or targetName == '' then
    user:Send('Consider whom?')
    return true
end

local target = Command.FindCharacterInRoom(user, targetName)
if target == nil or target == char then
    user:Send(""They aren't here."")
    return true
end

local myPower = Command.CalculatePower(char)
local theirPower = Command.CalculatePower(target)
local ratio = 1.0
if myPower > 0 then
    ratio = theirPower / myPower
end

local verdict = ''
if ratio < 0.5 then
    verdict = '{color:brightgreen}You could defeat ' .. target.Name .. ' effortlessly.{color:reset}'
elseif ratio < 0.85 then
    verdict = '{color:green}' .. target.Name .. ' looks weaker than you.{color:reset}'
elseif ratio <= 1.15 then
    verdict = '{color:yellow}' .. target.Name .. ' looks like a fair and even match.{color:reset}'
elseif ratio <= 1.6 then
    verdict = '{color:brightred}' .. target.Name .. ' looks tough! Be careful.{color:reset}'
else
    verdict = '{color:red}' .. target.Name .. ' would crush you completely!{color:reset}'
end

user:Send('Considering ' .. target.Name .. ':\r\n' .. verdict)
return true";

                case "time":
                    return
@"-- ============================================================================
-- Command: time
-- Displays server date/time and uptime
-- ============================================================================
user:Send(Command.GetUptimeString())
return true";

                case "weather":
                    return
@"-- ============================================================================
-- Command: weather
-- Displays current weather in the character's area
-- ============================================================================
user:Send(Command.GetWeatherString(user))
return true";

                case "save":
                    return
@"-- ============================================================================
-- Command: save
-- Saves player character data to disk
-- ============================================================================
user:Save()
user:Send('Character saved.')
return true";

                case "stop":
                    return
@"-- ============================================================================
-- Command: stop
-- Stops current movement or action
-- ============================================================================
char:Stop(false)
return true";

                case "announce":
                    return
@"-- ============================================================================
-- Command: announce (aliases: gecho, broadcast) [ADMIN]
-- Syntax : announce <message>
-- Customize BANNER_PREFIX below!
-- ============================================================================
local BANNER_PREFIX = '{color:brightyellow}[ANNOUNCEMENT] '

if not Command.RequireAdmin(user) then return true end

local msg = user.Command
user.Command = nil
if msg == nil or msg == '' then
    user:Send('Usage: announce <message>')
    return true
end

local broadcast = BANNER_PREFIX .. char.Name .. ': ' .. msg .. '{color:reset}'
user:Send(broadcast)
server:Echo(broadcast, char)
return true";

                case "slay":
                    return
@"-- ============================================================================
-- Command: slay (alias: smite) [ADMIN]
-- Syntax : slay <target>
-- Customize divine wrath messages below!
-- ============================================================================
if not Command.RequireAdmin(user) then return true end

local targetName = user:GetArgument()
if targetName == nil or targetName == '' then
    user:Send('Usage: slay <target>')
    return true
end

local target = Command.FindCharacterInRoom(user, targetName)
if target == nil or target == char then
    user:Send('Target not found here.')
    return true
end

Command.BroadcastTargetRoom(user, target,
    '{color:brightred}You call down divine wrath and slay {2} instantly!{color:reset}',
    '{color:brightred}{1} calls down divine wrath and slays you instantly!{color:reset}',
    '{color:brightred}{1} calls down divine wrath and slays {2} instantly!{color:reset}',
    false)
target:Die(char)
return true";

                case "silence":
                    return
@"-- ============================================================================
-- Command: silence [ADMIN]
-- Syntax : silence <player>
-- ============================================================================
if not Command.RequireAdmin(user) then return true end
return Command.ToggleSilence(user, user:GetArgument())";

                case "kick":
                    return
@"-- ============================================================================
-- Command: kick (alias: disconnect) [ADMIN]
-- Syntax : kick <player>
-- ============================================================================
if not Command.RequireAdmin(user) then return true end
return Command.DisconnectUser(user, user:GetArgument())";

                case "makeadmin":
                    return
@"-- ============================================================================
-- Command: makeadmin (alias: advance) [ADMIN]
-- Syntax : makeadmin <player>
-- ============================================================================
if not Command.RequireAdmin(user) then return true end
return Command.ToggleAdmin(user, user:GetArgument())";

                default:
                    return
@"-- ============================================================================
-- Command: " + name + @"
-- Pre-bound Lua globals:
--   user     : Ultimud.User (current connected user)
--   char     : Ultimud.Character (user.Character)
--   area     : Ultimud.Area (char.Area)
--   server   : Ultimud.Server (Server.Instance)
--   settings : Ultimud.Settings (server.Settings)
--   Command  : Ultimud.Command (built-in helper library)
-- You can customize this script freely or add pre/post hooks around Command." + methodName + @"!
-- ============================================================================
finished = Command." + methodName + @"(user)
return finished";
            }
        }

        static Command CreateDefaultCommand(string name, string methodName, bool canUseIncapacitated, uint priority, bool sysCmd, bool hidden, params string[] aliases)
        {
            Command cmd = new Command();
            cmd.name_ = name;
            cmd.incapacitated_ = canUseIncapacitated;
            cmd.priority_ = priority;
            cmd.system_ = sysCmd;
            cmd.hidden_ = hidden;
            if (aliases != null && aliases.Length > 0)
                cmd.aliases_.AddRange(aliases);
            cmd.script_ = GetDefaultLuaScript(name, methodName);
            cmd.Init();
            return cmd;
        }

        static void SeedDefaultCommands(string serverpath)
        {
            List<Command> list = new List<Command>();

            // Movement & Observation
            list.Add(CreateDefaultCommand("look", "DoLook", false, 1, false, false, "l"));
            list.Add(CreateDefaultCommand("north", "DoNorth", false, 1, false, false, "n"));
            list.Add(CreateDefaultCommand("south", "DoSouth", false, 1, false, false, "s"));
            list.Add(CreateDefaultCommand("east", "DoEast", false, 1, false, false, "e"));
            list.Add(CreateDefaultCommand("west", "DoWest", false, 1, false, false, "w"));
            list.Add(CreateDefaultCommand("northeast", "DoNortheast", false, 0, false, false, "ne"));
            list.Add(CreateDefaultCommand("northwest", "DoNorthwest", false, 0, false, false, "nw"));
            list.Add(CreateDefaultCommand("southeast", "DoSoutheast", false, 0, false, false, "se"));
            list.Add(CreateDefaultCommand("southwest", "DoSouthwest", false, 0, false, false, "sw"));
            list.Add(CreateDefaultCommand("travel", "DoTravel", false, 0, false, false, "move", "go"));
            list.Add(CreateDefaultCommand("stop", "DoStop", false, 0, false, false));
            list.Add(CreateDefaultCommand("enter", "DoEnter", false, 0, false, false));
            list.Add(CreateDefaultCommand("follow", "DoFollow", false, 0, false, false));
            list.Add(CreateDefaultCommand("ditch", "DoDitch", false, 0, false, false));
            list.Add(CreateDefaultCommand("group", "DoGroup", true, 0, false, false, "party"));
            list.Add(CreateDefaultCommand("order", "DoOrder", false, 0, false, false));

            // Positions
            list.Add(CreateDefaultCommand("position", "DoPosition", true, 0, false, false));
            list.Add(CreateDefaultCommand("stand", "DoStand", true, 0, false, false));
            list.Add(CreateDefaultCommand("sit", "DoSit", true, 0, false, false));
            list.Add(CreateDefaultCommand("rest", "DoRest", true, 0, false, false));
            list.Add(CreateDefaultCommand("sleep", "DoSleep", true, 0, false, false));

            // Communication
            list.Add(CreateDefaultCommand("who", "DoWho", true, 1, false, false));
            list.Add(CreateDefaultCommand("say", "DoSay", false, 1, false, false, "'"));
            list.Add(CreateDefaultCommand("chat", "DoChat", true, 0, false, false, "ooc"));
            list.Add(CreateDefaultCommand("tell", "DoTell", true, 0, false, false));
            list.Add(CreateDefaultCommand("reply", "DoReply", true, 0, false, false));
            list.Add(CreateDefaultCommand("whisper", "DoWhisper", false, 0, false, false));
            list.Add(CreateDefaultCommand("emote", "DoEmote", false, 0, false, false, ":"));
            list.Add(CreateDefaultCommand("gtell", "DoGroupTell", true, 0, false, false, "gsay"));
            list.Add(CreateDefaultCommand("channels", "DoChannels", true, 0, false, false));
            list.Add(CreateDefaultCommand("socials", "DoSocials", true, 0, false, false));
            list.Add(CreateDefaultCommand("forums", "DoForums", true, 0, false, false, "board", "boards"));
            list.Add(CreateDefaultCommand("afk", "DoAfk", true, 0, false, false));
            list.Add(CreateDefaultCommand("ignore", "DoIgnore", true, 0, false, false));

            // Character Info & Inventory
            list.Add(CreateDefaultCommand("stats", "DoStats", true, 1, false, false, "score", "sc"));
            list.Add(CreateDefaultCommand("inventory", "DoInventory", true, 1, false, false, "i", "inv"));
            list.Add(CreateDefaultCommand("equipment", "DoEquipment", true, 1, false, false, "eq"));
            list.Add(CreateDefaultCommand("abilities", "DoAbilities", true, 0, false, false, "skills", "spells"));
            list.Add(CreateDefaultCommand("status", "DoStatus", true, 0, false, false, "affects"));
            list.Add(CreateDefaultCommand("money", "DoMoney", true, 0, false, false, "gold"));
            list.Add(CreateDefaultCommand("description", "DoDescription", true, 0, false, false, "desc"));
            list.Add(CreateDefaultCommand("title", "DoTitle", true, 0, false, false));

            // Combat, Tactical & Exploration
            list.Add(CreateDefaultCommand("flee", "DoFlee", false, 0, false, false, "escape"));
            list.Add(CreateDefaultCommand("recall", "DoRecall", false, 0, false, false, "home"));
            list.Add(CreateDefaultCommand("consider", "DoConsider", false, 0, false, false, "con"));
            list.Add(CreateDefaultCommand("scan", "DoScan", false, 0, false, false, "where", "scout"));
            list.Add(CreateDefaultCommand("exits", "DoExits", true, 0, false, false));

            // Items, Containers & Economy
            list.Add(CreateDefaultCommand("get", "DoGet", false, 1, false, false, "take", "pickup"));
            list.Add(CreateDefaultCommand("put", "DoPut", false, 0, false, false));
            list.Add(CreateDefaultCommand("drop", "DoDrop", false, 0, false, false));
            list.Add(CreateDefaultCommand("use", "DoUse", false, 0, false, false, "wear", "wield", "equip", "eat", "quaff", "drink"));
            list.Add(CreateDefaultCommand("remove", "DoRemove", false, 0, false, false, "unequip"));
            list.Add(CreateDefaultCommand("give", "DoGive", false, 0, false, false));
            list.Add(CreateDefaultCommand("givemoney", "DoGiveMoney", false, 0, false, false));
            list.Add(CreateDefaultCommand("split", "DoSplit", false, 0, false, false));
            list.Add(CreateDefaultCommand("destroy", "DoDestroyItem", false, 0, false, false));
            list.Add(CreateDefaultCommand("sacrifice", "DoSacrifice", false, 0, false, false, "junk"));
            list.Add(CreateDefaultCommand("examine", "DoExamine", false, 0, false, false, "exa", "lookat"));
            list.Add(CreateDefaultCommand("open", "DoOpen", false, 0, false, false));
            list.Add(CreateDefaultCommand("close", "DoClose", false, 0, false, false));
            list.Add(CreateDefaultCommand("lock", "DoLock", false, 0, false, false));
            list.Add(CreateDefaultCommand("unlock", "DoUnlock", false, 0, false, false));
            list.Add(CreateDefaultCommand("bank", "DoBank", false, 0, false, false, "balance"));
            list.Add(CreateDefaultCommand("deposit", "DoDeposit", false, 0, false, false));
            list.Add(CreateDefaultCommand("withdraw", "DoWithdraw", false, 0, false, false));

            // Shops & Training
            list.Add(CreateDefaultCommand("list", "DoShopList", false, 0, false, false, "shop"));
            list.Add(CreateDefaultCommand("buy", "DoBuy", false, 0, false, false));
            list.Add(CreateDefaultCommand("sell", "DoSell", false, 0, false, false));
            list.Add(CreateDefaultCommand("evaluate", "DoEvaluate", false, 0, false, false, "value"));
            list.Add(CreateDefaultCommand("pay", "DoGiveMoney", false, 0, false, false));
            list.Add(CreateDefaultCommand("learn", "DoLearn", false, 0, false, false));
            list.Add(CreateDefaultCommand("teach", "DoTeachPlayer", false, 0, false, false));

            // World & System
            list.Add(CreateDefaultCommand("time", "DoTime", true, 0, false, false));
            list.Add(CreateDefaultCommand("weather", "DoWeather", true, 0, false, false));
            list.Add(CreateDefaultCommand("set", "DoSet", true, 0, false, false, "settings"));
            list.Add(CreateDefaultCommand("alias", "DoAlias", true, 0, false, false, "aliases"));
            list.Add(CreateDefaultCommand("commands", "DoCommands", true, 0, false, false));
            list.Add(CreateDefaultCommand("help", "DoHelp", true, 1, false, false, "?"));
            list.Add(CreateDefaultCommand("save", "DoSave", true, 0, false, false));
            list.Add(CreateDefaultCommand("password", "DoPassword", true, 0, false, false));
            list.Add(CreateDefaultCommand("delete", "DoDelete", true, 0, false, false));
            list.Add(CreateDefaultCommand("quit", "DoQuit", true, 0, false, false));

            // Administrator / Immortal Commands
            list.Add(CreateDefaultCommand("wizhelp", "DoWizHelp", true, 0, false, true, "immhelp"));
            list.Add(CreateDefaultCommand("goto", "DoGoto", true, 0, false, true, "teleport"));
            list.Add(CreateDefaultCommand("transfer", "DoTransferPlayer", true, 0, false, true, "summon"));
            list.Add(CreateDefaultCommand("slay", "DoSlay", true, 0, false, true, "smite"));
            list.Add(CreateDefaultCommand("restore", "DoRestoreAdmin", true, 0, false, true, "healall"));
            list.Add(CreateDefaultCommand("purge", "DoPurge", true, 0, false, true));
            list.Add(CreateDefaultCommand("spawn", "DoSpawn", true, 0, false, true, "mload", "oload"));
            list.Add(CreateDefaultCommand("announce", "DoAnnounce", true, 0, false, true, "gecho", "broadcast"));
            list.Add(CreateDefaultCommand("wizstat", "DoWizStat", true, 0, false, true, "inspect"));
            list.Add(CreateDefaultCommand("setstat", "DoSetStat", true, 0, false, true));
            list.Add(CreateDefaultCommand("silence", "DoSilencePlayer", true, 0, false, true));
            list.Add(CreateDefaultCommand("kick", "DoKickPlayer", true, 0, false, true, "disconnect"));
            list.Add(CreateDefaultCommand("makeadmin", "DoMakeAdmin", true, 0, false, true, "advance"));

            // Script Commands
            list.Add(CreateDefaultCommand("peace", "DoPeace", true, 0, true, true));
            list.Add(CreateDefaultCommand("force", "DoForce", true, 0, true, true));
            list.Add(CreateDefaultCommand("createitem", "DoCreateItem", true, 0, true, true));
            list.Add(CreateDefaultCommand("print", "DoPrint", true, 0, true, true));
            list.Add(CreateDefaultCommand("summonnpc", "DoSummonNpc", true, 0, true, true));

            string saveDir = !string.IsNullOrEmpty(serverpath) ? (serverpath + "\\Commands") : Path;
            try
            {
                if (!string.IsNullOrEmpty(saveDir))
                    Directory.CreateDirectory(saveDir);
            }
            catch { }

            foreach (Command c in list)
            {
                Command existing = commands_.Find(delegate(Command ex)
                {
                    return ex.Name != null && ex.Name.Equals(c.name_, StringComparison.OrdinalIgnoreCase);
                });
                if (existing == null)
                {
                    commands_.Add(c);
                    try
                    {
                        if (!string.IsNullOrEmpty(saveDir))
                            Damdai.Serialization.Xml.Serialize(c, saveDir + "\\" + c.name_ + ".cmd");
                    }
                    catch { }
                }
                else if (IsLegacyStubScript(existing.script_))
                {
                    existing.script_ = c.script_;
                    existing.Init();
                    try
                    {
                        if (!string.IsNullOrEmpty(saveDir))
                            Damdai.Serialization.Xml.Serialize(existing, saveDir + "\\" + existing.name_ + ".cmd");
                    }
                    catch { }
                }
            }
        }

        [Obfuscation(Exclude = false)]
        public static Command Get(string name, bool includesyscommands)
        {
            if (string.IsNullOrEmpty(name))
                return null;
            Command cmd = null;
            lock (commands_)
            {
                foreach (Command c in Command.Commands)
                {
                    if ((!c.SystemCommand || includesyscommands) && !string.IsNullOrEmpty(c.name_) &&
                        (c.Name.StartsWith(name, StringComparison.CurrentCultureIgnoreCase) ||
                         (c.aliases_ != null && c.aliases_.Exists(delegate(string a) { return a.Equals(name, StringComparison.CurrentCultureIgnoreCase); }))))
                    {
                        if (c.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase) ||
                            (c.aliases_ != null && c.aliases_.Exists(delegate(string a) { return a.Equals(name, StringComparison.CurrentCultureIgnoreCase); })))
                        {
                            return c;
                        }
                        if (c.priority_ == 1)
                            return c;
                        else if (cmd == null || cmd.priority_ == 0 || (c.priority_ > 0 && c.priority_ < cmd.priority_))
                            cmd = c;
                    }
                }
            }
            return cmd;
        }

        [Obfuscation(Exclude = false)]
        public static bool GetArguments(User user, params Server.InputHandler[] handlers)
        {
            if (user.CurrentCommandArgs.Count != handlers.Length)
                foreach (Server.InputHandler handler in handlers)
                    handler(user);

            if (user.CurrentCommandArgs.Count >= handlers.Length)
                return true;

            return false;
        }
        
        [Obfuscation(Exclude = false)]
        public bool DoScript(User user)
        {
            lock (this)
            {
                if (this.lua_ == null || this.menuhandlers_ == null)
                    this.Init();

                if (this.menuhandlers_.Length == 0 || Ultimud.Command.GetArguments(user, this.menuhandlers_))
                {
                    try
                    {
                        this.lua_["user"] = user;
                        this.lua_["char"] = user != null ? user.Character : null;
                        this.lua_["ch"] = user != null ? user.Character : null;
                        this.lua_["actor"] = user != null ? user.Character : null;
                        this.lua_["server"] = Server.Instance;
                        this.lua_["area"] = (user != null && user.Character != null) ? user.Character.Area : null;
                        this.lua_["settings"] = Server.Instance != null ? Server.Instance.Settings : null;
                        this.lua_["finished"] = null;
                        object[] rvals = this.lua_.DoString(this.script_);
                        object fin = this.lua_["finished"];
                        if (fin is bool)
                            return (bool)fin;
                        if (rvals != null && rvals.Length > 0 && rvals[0] is bool)
                            return (bool)rvals[0];
                        return (user.Menus.Count == 0 && (user.InputHandler == null || user.InputHandler == this.DoScript));
                    }
                    catch (AbortCommandException)
                    {
                        throw;
                    }
                    catch (Exception e)
                    {
                        Trace.WriteLine("Command: " + this.Name + ": " + e.Message, "LUA");
#if DEBUG
                        user.Send(e.Message);
#endif
                    }
                    return true;
                }
            }

            return false;
        }
        
        [Obfuscation(Exclude = false)]
        public void Save()
        {
            if (!string.IsNullOrEmpty(this.name_))
            {
                Directory.CreateDirectory(Path);
                Damdai.Serialization.Xml.Serialize(this, Path + "\\" + this.name_ + ".cmd");
                this.Init();
            }
        }
        
        [Obfuscation(Exclude = false)]
        public void Delete()
        {
            if (!string.IsNullOrEmpty(this.name_))
            {
                string file = Path + "\\" + this.name_ + ".cmd";
                if (File.Exists(file))
                    File.Delete(file);
            }
        }
        
        [Obfuscation(Exclude = false)]
        public override string ToString()
        {
            if(string.IsNullOrEmpty(this.name_))
                return base.ToString();
            else
                return this.name_;
        }

        [Obfuscation(Exclude = false)]
        void Init()
        {
            lock (this)
            {
                List<Server.InputHandler> list = new List<Server.InputHandler>();
                foreach (MenuFunction menufn in this.menufunctions_)
                    if (!string.IsNullOrEmpty(menufn.Function) && menufn.Handler != null)
                        list.Add(menufn.Handler);
                this.menuhandlers_ = list.ToArray();
                this.lua_ = new Lua();
                try
                {
                    this.lua_.DoString(
                        "luanet.load_assembly('System')\r\n" +
                        "luanet.load_assembly('System.Drawing')\r\n" +
                        "luanet.load_assembly('Damdai')\r\n" +
                        "luanet.load_assembly('Ultimud')\r\n" +
                        "Command = luanet.import_type('Ultimud.Command')\r\n" +
                        "Server = luanet.import_type('Ultimud.Server')\r\n" +
                        "String = luanet.import_type('Ultimud.String')\r\n" +
                        "StringTable = luanet.import_type('Ultimud.StringTable')\r\n" +
                        "Area = luanet.import_type('Ultimud.Area')\r\n" +
                        "Location = luanet.import_type('Ultimud.Location')\r\n" +
                        "Object = luanet.import_type('Ultimud.Object')\r\n" +
                        "Character = luanet.import_type('Ultimud.Character')\r\n" +
                        "Npc = luanet.import_type('Ultimud.Npc')\r\n" +
                        "Player = luanet.import_type('Ultimud.Player')\r\n" +
                        "Item = luanet.import_type('Ultimud.Item')\r\n" +
                        "Shop = luanet.import_type('Ultimud.Shop')\r\n" +
                        "Help = luanet.import_type('Ultimud.Help')\r\n" +
                        "Point = luanet.import_type('System.Drawing.Point')");
                }
                catch { }
            }
        }

        #region Lua Scripting Helper API
        public static int ToInt(string str, int defaultValue)
        {
            if (string.IsNullOrEmpty(str))
                return defaultValue;
            int val;
            if (int.TryParse(str.Trim().TrimStart('$'), out val))
                return val;
            return defaultValue;
        }

        public static string FormatNumber(long value)
        {
            string fmt = (Server.Instance != null && Server.Instance.Settings != null && !string.IsNullOrEmpty(Server.Instance.Settings.NumberFormat))
                ? Server.Instance.Settings.NumberFormat
                : "N0";
            return value.ToString(fmt);
        }

        public static Shop GetShop(User user)
        {
            if (user == null || user.Character == null)
                return null;
            List<Shop> shops = user.Character.NearbyObjects<Shop>(0, true);
            return (shops != null && shops.Count > 0) ? shops[0] : null;
        }

        public static Item FindItemHereOrInv(User user, string name)
        {
            if (user == null || user.Character == null || string.IsNullOrEmpty(name))
                return null;
            return user.Character.FindNearby<Item>(name, 0, true) ?? user.Character.FindItemInInventory(name);
        }

        public static Character FindCharacterInRoom(User user, string name)
        {
            if (user == null || user.Character == null || string.IsNullOrEmpty(name))
                return null;
            return user.Character.FindNearby<Character>(name, 0, true);
        }

        public static bool IsSilenced(User user)
        {
            return user != null && user.Flags.Get((int)User.UserFlags.Silenced);
        }

        public static bool MoveDir(User user, string dirName, string selfLeaveMsg, string roomLeaveMsg, string roomArriveMsg)
        {
            if (user == null || user.Character == null)
                return true;
            Area.Direction dir = (Area.Direction)Enum.Parse(typeof(Area.Direction), dirName, true);
            if (user.Character.Position != Object.Positions.Stopped &&
                user.Character.Position != Object.Positions.Moving)
            {
                user.Send("You must stand up first!");
                return true;
            }

            if (user.Character.CurrentAction != null)
                user.Character.Stop(false);

            bool moved = user.Character.Move(
                dir,
                new String(selfLeaveMsg ?? "You move {dir}.", null, null, null, roomLeaveMsg ?? "{1} leaves {dir}.", null),
                new String(null, null, null, null, roomArriveMsg ?? "{1} arrives from the {dir}.", null),
                false);

            if (!moved)
                user.Send("You cannot move " + dir.ToString().ToLower() + ".");
            else
                user.Send(user.Character.GenerateLookDescription(!user.Flags.Get((int)User.UserFlags.AutoMap)));
            return true;
        }

        public static bool SetPosition(User user, string posName, string selfMsg, string roomMsg, string alreadyMsg)
        {
            if (user == null || user.Character == null)
                return true;
            if (user.Character.Position < Object.Positions.Sleeping)
            {
                user.Send("You can't do that in your current state.");
                return true;
            }
            Object.Positions targetPos = (Object.Positions)Enum.Parse(typeof(Object.Positions), posName, true);
            if (user.Character.Position == targetPos)
            {
                user.Send(alreadyMsg);
                return true;
            }
            if (targetPos != Object.Positions.Stopped)
                user.Character.Stop(true);
            user.Character.Position = targetPos;
            if (user.Character.Area != null)
                user.Character.Area.Send(new String(selfMsg, null, null, null, roomMsg, null), user.Character, null, 0);
            return true;
        }

        public static void BroadcastRoom(User user, string selfMsg, string roomMsg, bool mustHear)
        {
            if (user == null || user.Character == null || user.Character.Area == null)
                return;
            String str = new String(selfMsg, null, null, null, roomMsg, null);
            if (mustHear)
                user.Character.Area.Send(str, user.Character, null, 0, new Damdai.Function<bool, IObject>[] { Object.CanHear });
            else
                user.Character.Area.Send(str, user.Character, null, 0);
        }

        public static void BroadcastTargetRoom(User user, Character target, string selfMsg, string targetMsg, string roomMsg, bool mustHear)
        {
            if (user == null || user.Character == null || user.Character.Area == null || target == null)
                return;
            String str = new String(selfMsg, selfMsg, targetMsg, targetMsg, roomMsg, null);
            if (mustHear)
                user.Character.Area.Send(str, user.Character, target, 0, new Damdai.Function<bool, IObject>[] { Object.CanHear });
            else
                user.Character.Area.Send(str, user.Character, target, 0);
        }

        public static void BroadcastGlobal(User user, string selfMsg, string globalMsg, bool respectIgnore)
        {
            if (user == null || Server.Instance == null)
                return;
            if (!string.IsNullOrEmpty(selfMsg))
                user.Send(selfMsg);
            foreach (User u in Damdai.Collections.Util.CopyList<User>(Server.Instance.Users))
            {
                if (u != null && u != user && u.Flags.Get((int)User.UserFlags.LoggedIn) && u.Character != null)
                {
                    if (respectIgnore && user.Character != null && u.IsIgnoring(user.Character.Name))
                        continue;
                    u.Send(globalMsg);
                }
            }
        }

        public static void RecordLastTell(User targetUser, User senderUser)
        {
            if (targetUser == null || senderUser == null)
                return;
            lock (lastTellFrom_)
                lastTellFrom_[targetUser.Id] = senderUser.Id;
        }

        public static User GetLastTellSender(User user)
        {
            if (user == null || Server.Instance == null)
                return null;
            Guid senderId = Guid.Empty;
            lock (lastTellFrom_)
                lastTellFrom_.TryGetValue(user.Id, out senderId);
            if (senderId == Guid.Empty)
                return null;
            foreach (User u in Damdai.Collections.Util.CopyList<User>(Server.Instance.Users))
            {
                if (u != null && u.Id == senderId && u.Flags.Get((int)User.UserFlags.LoggedIn) && u.Character != null)
                    return u;
            }
            return null;
        }

        public static long CalculatePower(Character ch)
        {
            if (ch == null || ch.Stats == null)
                return 0;
            long power = 0;
            foreach (Stat s in ch.Stats.Values)
                power += s.Max;
            return power;
        }

        public static string GetUptimeString()
        {
            TimeSpan uptime = DateTime.Now - (Server.Instance != null ? Server.Instance.StartTime : DateTime.Now);
            return "Server Time: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") +
                "\r\nUptime:      " + (int)uptime.TotalHours + "h " + uptime.Minutes + "m " + uptime.Seconds + "s";
        }

        public static string GetWeatherString(User user)
        {
            if (user == null || user.Character == null || user.Character.Area == null)
                return "You cannot tell the weather here.";
            Area area = user.Character.Area;
            Area.Layer layer = user.Character.OnLayer;
            string weather = (layer != null && !Damdai.Collections.Util.IsEmpty(layer.WeatherConditions.PossibleConditions)
                ? layer.WeatherConditions.ToString()
                : area.WeatherConditions.ToString());
            return "Current weather in " + area.Name + ": " + weather;
        }

        public static bool ToggleIgnore(User user, string target)
        {
            if (string.IsNullOrEmpty(target))
            {
                if (user.IgnoredUsers.Count == 0)
                    user.Send("You are not ignoring anyone.");
                else
                    user.Send("Ignored players: " + string.Join(", ", user.IgnoredUsers.ToArray()));
                return true;
            }
            user.Command = target;
            return DoIgnore(user);
        }

        public static bool ToggleSilence(User user, string targetName)
        {
            user.Command = targetName;
            return DoSilencePlayer(user);
        }

        public static bool DisconnectUser(User user, string targetName)
        {
            user.Command = targetName;
            return DoKickPlayer(user);
        }

        public static bool ToggleAdmin(User user, string targetName)
        {
            user.Command = targetName;
            return DoMakeAdmin(user);
        }

        public static long ModifyStat(Character ch, string statName, long delta)
        {
            if (ch == null || string.IsNullOrEmpty(statName))
                return 0;
            Stat stat = ch.GetStat(statName);
            if (stat == null)
                return 0;
            long nextVal = stat.Current + delta;
            if (nextVal < 0)
                nextVal = 0;
            if (nextVal > stat.Max)
                nextVal = stat.Max;
            stat.Current = nextVal;
            return stat.Current;
        }

        public static Item GiveItemBlueprint(Character ch, string itemName)
        {
            if (ch == null || string.IsNullOrEmpty(itemName) || Server.Instance == null)
                return null;
            Item bp = Server.Instance.Get<Item>(itemName);
            if (bp == null)
                return null;
            Item item = Item.Create(bp);
            if (item != null)
                item.Parent = ch;
            return item;
        }

        public static Npc SpawnNpcBlueprint(Character ch, string npcName)
        {
            if (ch == null || string.IsNullOrEmpty(npcName) || Server.Instance == null)
                return null;
            Npc bp = Server.Instance.Get<Npc>(npcName);
            if (bp == null)
                return null;
            Npc npc = Npc.Create(bp);
            if (npc != null)
                npc.Location = ch.Location;
            return npc;
        }
        #endregion

        #region Built-In Command Implementations
        public static bool DoLook(User user)
        {
            if (string.IsNullOrEmpty(user.Command))
            {
                user.Send(user.Character.GenerateLookDescription(false));
                return true;
            }
            return DoExamine(user);
        }

        public static bool DoExamine(User user)
        {
            string arg = user.GetArgument();
            if (string.IsNullOrEmpty(arg))
            {
                user.Send(user.Character.GenerateLookDescription(false));
                return true;
            }
            if (arg.Equals("in", StringComparison.OrdinalIgnoreCase))
            {
                string nextArg = user.GetArgument();
                if (!string.IsNullOrEmpty(nextArg))
                    arg = nextArg;
            }

            Object target = user.Character.FindItemInInventory(arg);
            if (target == null)
                target = user.Character.GetEquippedItem(arg);
            if (target == null)
                target = user.Character.FindNearby<Object>(arg, 0, true);
            if (target == null)
                target = user.Character.FindNearby<Object>(arg, 5, true);

            if (target == null)
            {
                user.Send("You don't see '" + arg + "' here.");
                return true;
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("{color:brightcyan}" + target.Name + "{color:reset}");
            if (!string.IsNullOrEmpty(target.Description))
                sb.AppendLine(target.Description);

            if (target is Character)
            {
                Character ch = (Character)target;
                sb.AppendLine(ch.Name + " is " + ch.Position.ToString().ToLower() + ".");
                if (ch.Equipment != null && ch.Equipment.Count > 0)
                {
                    sb.AppendLine("\r\nEquipment:");
                    sb.AppendLine(Server.Instance.Settings.EqFormat.ToString(null, ch));
                }
            }
            else if (target is IContainer && target.ObjectFlags.Get((int)Object.ObjFlags.Container))
            {
                IContainer container = (IContainer)target;
                if (container.Closed)
                {
                    sb.AppendLine("It is closed.");
                }
                else
                {
                    List<Item> contents = target.GetChildren<Item>();
                    if (contents.Count == 0)
                        sb.AppendLine("It is empty.");
                    else
                    {
                        sb.AppendLine("Contents:");
                        foreach (Item item in contents)
                            sb.AppendLine("  " + item.Name);
                    }
                }
            }

            user.Send(sb.ToString().TrimEnd('\r', '\n'));
            return true;
        }

        static bool MoveInDirection(User user, Area.Direction dir)
        {
            if (user.Character.Position != Object.Positions.Stopped &&
                user.Character.Position != Object.Positions.Moving)
            {
                user.Send("You must stand up first!");
                return true;
            }

            if (user.Character.CurrentAction != null)
                user.Character.Stop(false);

            bool moved = user.Character.Move(
                dir,
                new String("You move {dir}.", null, null, null, "{1} leaves {dir}.", null),
                new String(null, null, null, null, "{1} arrives from the {dir}.", null),
                false);

            if (!moved)
            {
                user.Send("You cannot move " + dir.ToString().ToLower() + ".");
            }
            else
            {
                user.Send(user.Character.GenerateLookDescription(!user.Flags.Get((int)User.UserFlags.AutoMap)));
            }
            return true;
        }

        public static bool DoNorth(User user) { return MoveInDirection(user, Area.Direction.North); }
        public static bool DoSouth(User user) { return MoveInDirection(user, Area.Direction.South); }
        public static bool DoEast(User user) { return MoveInDirection(user, Area.Direction.East); }
        public static bool DoWest(User user) { return MoveInDirection(user, Area.Direction.West); }
        public static bool DoNortheast(User user) { return MoveInDirection(user, Area.Direction.Northeast); }
        public static bool DoNorthwest(User user) { return MoveInDirection(user, Area.Direction.Northwest); }
        public static bool DoSoutheast(User user) { return MoveInDirection(user, Area.Direction.Southeast); }
        public static bool DoSouthwest(User user) { return MoveInDirection(user, Area.Direction.Southwest); }

        public static bool DoTravel(User user)
        {
            if (user.Character.CurrentAction != null)
                user.Character.Stop(false);

            if (GetArguments(user, User.SelectDestinationOrDirection, User.SelectSpeed))
            {
                object arg1 = user.GetArg(0);
                int speed = 1;
                if (user.CurrentCommandArgs.Count > 1 && user.GetArg(1) is int)
                    speed = (int)user.GetArg(1);

                Stat speedStat = (Server.Instance.Settings.SpeedStat != Guid.Empty ? user.Character.GetStat(Server.Instance.Settings.SpeedStat) : null) ?? user.Character.GetStat("speed");
                int maxSpeed = (speedStat != null ? (int)speedStat.Current : 5);
                speed = Math.Max(1, Math.Min(speed, Math.Max(1, maxSpeed)));

                if (arg1 is Area.Direction || arg1 is Area.ShortDirection)
                {
                    Area.Direction dir = (Area.Direction)arg1;
                    if (Server.Instance.Settings.SimpleMovement)
                        return MoveInDirection(user, dir);
                    user.Character.ReplaceCurrentAction(new TravelAction(user.Character, dir, speed));
                    return true;
                }
                else if (arg1 is Object)
                {
                    Object target = (Object)arg1;
                    if (target.Distance(user.Character) > 0)
                    {
                        user.Character.ReplaceCurrentAction(new TravelAction(user.Character, target, speed));
                        return true;
                    }
                }

                user.Send("Invalid destination.");
                return true;
            }
            return false;
        }

        public static bool DoStop(User user)
        {
            user.Character.Stop(false);
            return true;
        }

        public static bool DoEnter(User user)
        {
            if (user.Character.Parent is Vehicle && string.IsNullOrEmpty(user.Command))
            {
                user.Character.Enter((Vehicle)user.Character.Parent);
                user.Send(user.Character.GenerateLookDescription(!user.Flags.Get((int)User.UserFlags.AutoMap)));
                return true;
            }
            if (!string.IsNullOrEmpty(user.Command))
            {
                Vehicle veh = user.Character.FindNearby<Vehicle>(user.Command.Trim(), 0, true);
                if (veh != null)
                {
                    user.Command = null;
                    user.Character.Enter(veh);
                    return true;
                }
            }
            if (GetArguments(user, User.SelectPortal))
            {
                IPortal portal = user.GetCommandArg<IPortal>(0);
                if (portal != null)
                {
                    user.Character.Enter(portal);
                    user.Send(user.Character.GenerateLookDescription(!user.Flags.Get((int)User.UserFlags.AutoMap)));
                }
                else
                {
                    user.Send("Invalid entrance.");
                }
                return true;
            }
            return false;
        }

        public static bool DoFollow(User user)
        {
            string arg1 = user.GetArgument();
            if (string.IsNullOrEmpty(arg1))
            {
                if (user.Character.Master != null)
                {
                    user.Send("You stop following " + user.Character.Master.Name + ".");
                    user.Character.Master = null;
                    user.Character.Stop(false);
                }
                else
                {
                    user.Send("Follow whom? Usage: follow <target>");
                }
                return true;
            }

            if (arg1.Equals("self", StringComparison.OrdinalIgnoreCase) || arg1.Equals(user.Character.Name, StringComparison.OrdinalIgnoreCase))
            {
                if (user.Character.Master != null)
                {
                    user.Send("You stop following " + user.Character.Master.Name + ".");
                    user.Character.Master = null;
                }
                user.Character.Stop(false);
                return true;
            }

            Object o = user.Character.FindNearby<Object>(arg1, 0, true);
            if (o == null)
            {
                user.Send("Target not found: " + arg1);
                return true;
            }

            int speed = 5;
            Stat speedStat = user.Character.GetStat("speed");
            if (speedStat != null)
                speed = Math.Max(1, (int)speedStat.Current);

            if (o is Character)
            {
                Character targetChar = (Character)o;
                user.Character.Master = targetChar;
                user.Character.Area.Send(new String(
                    "You start following {2}.", "You start following {2}.",
                    "{1} starts following you.", "{1} starts following you.",
                    "{1} starts following {2}.", null), user.Character, targetChar, 0);
            }
            user.Character.ReplaceCurrentAction(new FollowAction(user.Character, o, 0, speed));
            return true;
        }

        public static bool DoDitch(User user)
        {
            string arg = user.GetArgument();
            if (string.IsNullOrEmpty(arg))
            {
                int count = 0;
                foreach (Character ch in user.Character.NearbyObjects<Character>(5, true, user.Character))
                {
                    if (ch.Master == user.Character)
                    {
                        ch.Master = null;
                        ch.Stop(false);
                        ch.Send(user.Character.Name + " ditches you.");
                        count++;
                    }
                }
                user.Send(count > 0 ? "You ditch your followers." : "No one is following you.");
                return true;
            }

            Character target = user.Character.FindNearby<Character>(arg, 5, true);
            if (target != null && target.Master == user.Character)
            {
                target.Master = null;
                target.Stop(false);
                user.Send("You ditch " + target.Name + ".");
                target.Send(user.Character.Name + " ditches you.");
            }
            else
            {
                user.Send("They aren't following you.");
            }
            return true;
        }

        public static bool DoGroup(User user)
        {
            Character leader = user.Character.Master ?? user.Character;
            StringTable table = new StringTable("Member", "Role", "Position");
            table.AddRow(leader.Name, "Leader", leader.Position.ToString());
            if (user.Character.Area != null)
            {
                foreach (Character ch in user.Character.Area.GetChildren<Character>())
                {
                    if (ch != leader && ch.Master == leader)
                        table.AddRow(ch.Name, "Follower", ch.Position.ToString());
                }
            }
            user.Send(table.ToString());
            return true;
        }

        public static bool DoOrder(User user)
        {
            if (GetArguments(user, User.SelectCharacterRoom, User.EnterMessage))
            {
                Character target = user.GetCommandArg<Character>(0);
                string cmd = user.GetCommandArg<string>(1);
                if (target == null || target == user.Character)
                    user.Send("Invalid target.");
                else if (target.Master != user.Character)
                    user.Send("They aren't under your control.");
                else
                {
                    user.Send("You order " + target.Name + " to '" + cmd + "'.");
                    target.DoCommand(cmd);
                }
                return true;
            }
            return false;
        }

        public static bool DoPosition(User user)
        {
            if (user.Character.Position < Object.Positions.Sleeping)
            {
                user.Send("You can't do that in your current state.");
                return true;
            }
            string arg = user.GetArgument();
            if (string.IsNullOrEmpty(arg))
            {
                user.Send("You are currently " + user.Character.Position.ToString().ToLower() + ". Usage: stand, sit, rest, sleep");
                return true;
            }
            if ("stand".StartsWith(arg, StringComparison.OrdinalIgnoreCase) || "standing".StartsWith(arg, StringComparison.OrdinalIgnoreCase))
                return DoStand(user);
            if ("sit".StartsWith(arg, StringComparison.OrdinalIgnoreCase) || "sitting".StartsWith(arg, StringComparison.OrdinalIgnoreCase))
                return DoSit(user);
            if ("rest".StartsWith(arg, StringComparison.OrdinalIgnoreCase) || "resting".StartsWith(arg, StringComparison.OrdinalIgnoreCase))
                return DoRest(user);
            if ("sleep".StartsWith(arg, StringComparison.OrdinalIgnoreCase) || "sleeping".StartsWith(arg, StringComparison.OrdinalIgnoreCase))
                return DoSleep(user);
            user.Send("Invalid position: " + arg);
            return true;
        }

        public static bool DoStand(User user)
        {
            if (user.Character.Position < Object.Positions.Sleeping)
            {
                user.Send("You can't do that in your current state.");
                return true;
            }
            if (user.Character.Position == Object.Positions.Stopped)
            {
                user.Send("You are already standing.");
                return true;
            }
            user.Character.Position = Object.Positions.Stopped;
            user.Character.Area.Send(new String("You stand up.", null, null, null, "{1} stands up.", null), user.Character, null, 0);
            return true;
        }

        public static bool DoSit(User user)
        {
            if (user.Character.Position < Object.Positions.Sleeping)
            {
                user.Send("You can't do that in your current state.");
                return true;
            }
            if (user.Character.Position == Object.Positions.Sitting)
            {
                user.Send("You are already sitting.");
                return true;
            }
            user.Character.Stop(true);
            user.Character.Position = Object.Positions.Sitting;
            user.Character.Area.Send(new String("You sit down.", null, null, null, "{1} sits down.", null), user.Character, null, 0);
            return true;
        }

        public static bool DoRest(User user)
        {
            if (user.Character.Position < Object.Positions.Sleeping)
            {
                user.Send("You can't do that in your current state.");
                return true;
            }
            if (user.Character.Position == Object.Positions.Resting)
            {
                user.Send("You are already resting.");
                return true;
            }
            user.Character.Stop(true);
            user.Character.Position = Object.Positions.Resting;
            user.Character.Area.Send(new String("You sit down and rest.", null, null, null, "{1} sits down and rests.", null), user.Character, null, 0);
            return true;
        }

        public static bool DoSleep(User user)
        {
            if (user.Character.Position < Object.Positions.Sleeping)
            {
                user.Send("You can't do that in your current state.");
                return true;
            }
            if (user.Character.Position == Object.Positions.Sleeping)
            {
                user.Send("You are already asleep.");
                return true;
            }
            user.Character.Stop(true);
            user.Character.Position = Object.Positions.Sleeping;
            user.Character.Area.Send(new String("You lie down and go to sleep.", null, null, null, "{1} lies down and goes to sleep.", null), user.Character, null, 0);
            return true;
        }

        public static bool DoWho(User user)
        {
            user.Send(Server.Instance.Settings.WhoFormat.ToString(user.GetArgument(), user));
            return true;
        }

        public static bool DoSay(User user)
        {
            string msg = user.Command;
            user.Command = null;
            if (string.IsNullOrEmpty(msg))
            {
                user.Send("Say what?");
                return true;
            }
            if (user.Flags.Get((int)User.UserFlags.Silenced))
            {
                user.Send("You are silenced.");
                return true;
            }
            String str = new String(
                "{color:brightwhite}You say, \"" + msg + "\"{color:reset}", null,
                null, null,
                "{color:brightwhite}{1} says, \"" + msg + "\"{color:reset}", null);
            user.Character.Area.Send(str, user.Character, null, 0, new Damdai.Function<bool, IObject>[] { Object.CanHear });
            return true;
        }

        public static bool DoChat(User user)
        {
            string msg = user.Command;
            user.Command = null;
            if (string.IsNullOrEmpty(msg))
            {
                user.Send("Chat what?");
                return true;
            }
            if (user.Flags.Get((int)User.UserFlags.Silenced))
            {
                user.Send("You are silenced.");
                return true;
            }
            user.Send("{color:brightcyan}[CHAT] You: " + msg + "{color:reset}");
            foreach (User u in Damdai.Collections.Util.CopyList<User>(Server.Instance.Users))
            {
                if (u != null && u != user && u.Flags.Get((int)User.UserFlags.LoggedIn) && u.Character != null && !u.IsIgnoring(user.Character.Name))
                    u.Send("{color:brightcyan}[CHAT] " + user.Character.Name + ": " + msg + "{color:reset}");
            }
            return true;
        }

        public static bool DoTell(User user)
        {
            string targetName = user.GetArgument();
            string msg = user.Command;
            user.Command = null;
            if (string.IsNullOrEmpty(targetName) || string.IsNullOrEmpty(msg))
            {
                user.Send("Usage: tell <player> <message>");
                return true;
            }
            if (user.Flags.Get((int)User.UserFlags.Silenced))
            {
                user.Send("You are silenced.");
                return true;
            }

            User targetUser = null;
            foreach (User u in Damdai.Collections.Util.CopyList<User>(Server.Instance.Users))
            {
                if (u != null && u.Flags.Get((int)User.UserFlags.LoggedIn) && u.Character != null &&
                    u.Character.Name.StartsWith(targetName, StringComparison.CurrentCultureIgnoreCase))
                {
                    targetUser = u;
                    break;
                }
            }

            if (targetUser == null)
            {
                user.Send("No player by that name is online.");
                return true;
            }
            if (targetUser.IsIgnoring(user.Character.Name))
            {
                user.Send(targetUser.Character.Name + " is ignoring tells from you.");
                return true;
            }

            lock (lastTellFrom_)
                lastTellFrom_[targetUser.Id] = user.Id;

            user.Send("{color:brightmagenta}You tell " + targetUser.Character.Name + ", \"" + msg + "\"{color:reset}");
            if (targetUser.IsAfk)
                user.Send("{color:yellow}" + targetUser.Character.Name + " is currently AFK" + (!string.IsNullOrEmpty(targetUser.AfkMessage) ? " (" + targetUser.AfkMessage + ")" : "") + ".{color:reset}");
            targetUser.Send("{color:brightmagenta}" + user.Character.Name + " tells you, \"" + msg + "\"{color:reset}");
            return true;
        }

        public static bool DoReply(User user)
        {
            string msg = user.Command;
            user.Command = null;
            if (string.IsNullOrEmpty(msg))
            {
                user.Send("Reply what?");
                return true;
            }

            Guid senderId = Guid.Empty;
            lock (lastTellFrom_)
                lastTellFrom_.TryGetValue(user.Id, out senderId);

            if (senderId == Guid.Empty)
            {
                user.Send("No one has sent you a tell recently.");
                return true;
            }

            User targetUser = null;
            foreach (User u in Damdai.Collections.Util.CopyList<User>(Server.Instance.Users))
            {
                if (u != null && u.Id == senderId && u.Flags.Get((int)User.UserFlags.LoggedIn) && u.Character != null)
                {
                    targetUser = u;
                    break;
                }
            }

            if (targetUser == null)
            {
                user.Send("They are no longer online.");
                return true;
            }
            if (targetUser.IsIgnoring(user.Character.Name))
            {
                user.Send(targetUser.Character.Name + " is ignoring tells from you.");
                return true;
            }

            lock (lastTellFrom_)
                lastTellFrom_[targetUser.Id] = user.Id;

            user.Send("{color:brightmagenta}You tell " + targetUser.Character.Name + ", \"" + msg + "\"{color:reset}");
            if (targetUser.IsAfk)
                user.Send("{color:yellow}" + targetUser.Character.Name + " is currently AFK" + (!string.IsNullOrEmpty(targetUser.AfkMessage) ? " (" + targetUser.AfkMessage + ")" : "") + ".{color:reset}");
            targetUser.Send("{color:brightmagenta}" + user.Character.Name + " tells you, \"" + msg + "\"{color:reset}");
            return true;
        }

        public static bool DoWhisper(User user)
        {
            string targetName = user.GetArgument();
            string msg = user.Command;
            user.Command = null;
            if (string.IsNullOrEmpty(targetName) || string.IsNullOrEmpty(msg))
            {
                user.Send("Usage: whisper <target> <message>");
                return true;
            }

            Character target = user.Character.FindNearby<Character>(targetName, 0, true);
            if (target == null)
            {
                user.Send("They aren't here.");
                return true;
            }
            if (target.User != null && target.User.IsIgnoring(user.Character.Name))
            {
                user.Send(target.Name + " is ignoring you.");
                return true;
            }

            String str = new String(
                "{color:magenta}You whisper to {2}, \"" + msg + "\"{color:reset}",
                "{color:magenta}You whisper to {2}, \"" + msg + "\"{color:reset}",
                "{color:magenta}{1} whispers to you, \"" + msg + "\"{color:reset}",
                "{color:magenta}{1} whispers to you, \"" + msg + "\"{color:reset}",
                "{1} whispers something quietly to {2}.", null);
            user.Character.Area.Send(str, user.Character, target, 0, new Damdai.Function<bool, IObject>[] { Object.CanHear });
            return true;
        }

        public static bool DoEmote(User user)
        {
            string msg = user.Command;
            user.Command = null;
            if (string.IsNullOrEmpty(msg))
            {
                user.Send("Emote what?");
                return true;
            }
            String str = new String(
                user.Character.Name + " " + msg, null,
                null, null,
                "{1} " + msg, null);
            user.Character.Area.Send(str, user.Character, null, 0);
            return true;
        }

        public static bool DoStats(User user)
        {
            user.Send(Server.Instance.Settings.StatsFormat.ToString(user.GetArgument(), user));
            return true;
        }

        public static bool DoInventory(User user)
        {
            List<Item> list = user.Character.Inventory;
            string numFmt = Server.Instance.Settings.NumberFormat;
            if (list.Count < 1)
            {
                user.Send("Your inventory is empty.\r\nGold: " + user.Character.Value.ToString(numFmt) + "\r\nYou are currently using " + user.Character.Equipment.Count + " item(s).");
                return true;
            }

            Dictionary<string, List<Item>> dictionary = Utils.ConsolidateList<Item>(list);
            Menu menu = new Menu("You are carrying (Gold: " + user.Character.Value.ToString(numFmt) + "):");
            foreach (List<Item> items in dictionary.Values)
                menu.AddOption(new Menu.Option(items[0].Name + (items.Count > 1 ? " (" + items.Count + ")" : ""), null));

            user.Send(menu.ToString());
            return true;
        }

        public static bool DoEquipment(User user)
        {
            if (user.Character.Equipment.Count == 0)
            {
                user.Send("You aren't using anything.");
                return true;
            }
            user.Send(Server.Instance.Settings.EqFormat.ToString(user.GetArgument(), user.Character));
            return true;
        }

        public static bool DoAbilities(User user)
        {
            if (user.Character.Abilities.Count < 1)
            {
                user.Send("You have no abilities.");
                return true;
            }
            user.Send(Server.Instance.Settings.AbilitiesFormat.ToString(user.GetArgument(), user));
            return true;
        }

        public static bool DoStatus(User user)
        {
            bool found = false;
            StringTable table = new StringTable("Status", "Duration");
            table.Columns[1].Alignment = StringTable.Column.Align.Right;
            foreach (StatusEffect status in user.Character.StatusEffects)
            {
                StatusEffectBlueprint bp = status.Blueprint;
                if (bp != null && bp.Visible)
                {
                    table.AddRow(bp.Name, (status.IsPermanent || status.Duration == null ? "Permanent" : status.SecondsRemaining.ToString() + "s"));
                    found = true;
                }
            }

            if (found)
                user.Send(table.ToString());
            else
                user.Send("You aren't affected by any status effects.");
            return true;
        }

        public static bool DoMoney(User user)
        {
            user.Send("You have $" + user.Character.Value.ToString(Server.Instance.Settings.NumberFormat) + ".");
            return true;
        }

        public static bool DoGet(User user)
        {
            if (user.CurrentCommandArgs.Count == 0 && !string.IsNullOrEmpty(user.Command))
            {
                string firstArg = user.GetArgument();
                string secondArg = user.GetArgument();
                if (firstArg.Equals("from", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(secondArg))
                {
                    user.Command = secondArg + " " + user.Command;
                    return DoGetFromContainer(user);
                }
                else if (!string.IsNullOrEmpty(secondArg))
                {
                    if (secondArg.Equals("from", StringComparison.OrdinalIgnoreCase))
                        secondArg = user.GetArgument();

                    IContainer container = user.Character.FindChild<IContainer>(secondArg, false, false) ?? user.Character.FindNearby<IContainer>(secondArg, 0, true);
                    if (container == null || !((Object)container).ObjectFlags.Get((int)Object.ObjFlags.Container))
                    {
                        user.Send("Invalid container: " + secondArg);
                        return true;
                    }
                    if (container.Closed)
                    {
                        user.Send(String.Capitalize(container.Name) + " is closed.");
                        return true;
                    }
                    if (container.Locked)
                    {
                        user.Send(String.Capitalize(container.Name) + " is locked.");
                        return true;
                    }
                    Item item = container.FindChild<Item>(firstArg, false, false);
                    if (item == null)
                    {
                        user.Send("Item not found in " + container.Name + ": " + firstArg);
                        return true;
                    }
                    user.Character.Get(item, container);
                    return true;
                }
                else
                {
                    Item roomItem = user.Character.FindNearby<Item>(firstArg, 0, true);
                    if (roomItem != null)
                    {
                        if (roomItem.ObjectFlags.Get((int)Object.ObjFlags.Money))
                        {
                            user.Character.Value += roomItem.Value;
                            user.Character.Area.Send(new String(
                                "You pick up $" + roomItem.Value + ".", null, null, null,
                                "{1} picks up some money.", null), user.Character, null, 0);
                            roomItem.Parent = null;
                        }
                        else
                        {
                            user.Character.PickUp(roomItem);
                        }
                        return true;
                    }
                    user.Send("Target not found: " + firstArg);
                    return true;
                }
            }

            if (GetArguments(user, User.SelectItemRoom))
            {
                Item selectedItem = user.GetCommandArg<Item>(0);
                if (selectedItem != null)
                {
                    if (selectedItem.ObjectFlags.Get((int)Object.ObjFlags.Money))
                    {
                        user.Character.Value += selectedItem.Value;
                        user.Character.Area.Send(new String(
                            "You pick up $" + selectedItem.Value + ".", null, null, null,
                            "{1} picks up some money.", null), user.Character, null, 0);
                        selectedItem.Parent = null;
                    }
                    else
                    {
                        user.Character.PickUp(selectedItem);
                    }
                }
                else
                    user.Send("Invalid target.");
                return true;
            }
            return false;
        }

        public static bool DoGetFromContainer(User user)
        {
            if (GetArguments(user, User.SelectContainer, User.SelectItemFromContainer))
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

        public static bool DoPut(User user)
        {
            if (GetArguments(user, User.SelectItemInInventory, User.SelectContainer))
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

        public static bool DoDrop(User user)
        {
            if (user.CurrentCommandArgs.Count == 0 && !string.IsNullOrEmpty(user.Command))
            {
                string trimmed = user.Command.Trim();
                int moneyAmt;
                if (trimmed.StartsWith("$") && int.TryParse(trimmed.Substring(1), out moneyAmt) && moneyAmt > 0)
                {
                    user.Command = null;
                    if (moneyAmt > user.Character.Value)
                        user.Send("You don't have that much money.");
                    else
                        user.Character.DropMoney(moneyAmt, false);
                    return true;
                }
            }

            if (GetArguments(user, User.SelectItemInInventory))
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

        public static bool DoUse(User user)
        {
            if (GetArguments(user, User.SelectItemInInventory))
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

        public static bool DoRemove(User user)
        {
            if (GetArguments(user, User.SelectEquippedItem))
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

        public static bool DoGive(User user)
        {
            if (GetArguments(user, User.SelectItemInInventory, User.SelectCharacterRoom))
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

        static bool SelectMoneyAmount(User user)
        {
            string arg = user.GetArgument();
            if (string.IsNullOrEmpty(arg))
            {
                user.Menus.Enqueue(new Menu(SelectMoneyAmount, "How much money? "));
                return false;
            }
            user.CurrentCommandArgs.Add(arg.TrimStart('$'));
            return true;
        }

        public static bool DoGiveMoney(User user)
        {
            if (user.CurrentCommandArgs.Count == 0 && !string.IsNullOrEmpty(user.Command))
            {
                string trimmed = user.Command.Trim();
                string[] parts = trimmed.Split(new char[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
                int testAmt;
                if (parts.Length == 2 && int.TryParse(parts[0].TrimStart('$'), out testAmt))
                    user.Command = parts[1] + " " + parts[0];
            }

            if (GetArguments(user, User.SelectCharacterRoom, SelectMoneyAmount))
            {
                Character ch = user.GetCommandArg<Character>(0);
                if (ch == null || ch == user.Character)
                    user.Send("Invalid target.");
                else
                {
                    string arg = user.GetCommandArg<string>(1);
                    int amount = 0;
                    if (!int.TryParse(arg, out amount) || amount <= 0)
                        user.Send("Amount must be a positive number.");
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

        public static bool DoDestroyItem(User user)
        {
            if (GetArguments(user, User.SelectItemInInventory))
            {
                Item selectedItem = user.GetCommandArg<Item>(0);
                if (selectedItem != null)
                {
                    user.Send("You destroy " + selectedItem.Name + ".");
                    selectedItem.Parent = null;
                }
                else
                    user.Send("Item not found in inventory.");
                return true;
            }
            return false;
        }

        public static bool DoOpen(User user)
        {
            if (GetArguments(user, User.SelectCloseableObject))
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

        public static bool DoClose(User user)
        {
            if (GetArguments(user, User.SelectCloseableObject))
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

        public static bool DoLock(User user)
        {
            if (GetArguments(user, User.SelectCloseableObject))
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

        public static bool DoUnlock(User user)
        {
            if (GetArguments(user, User.SelectCloseableObject))
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

        public static bool DoShopList(User user)
        {
            List<Shop> shops = user.Character.NearbyObjects<Shop>(0, true);
            if (shops.Count < 1)
            {
                user.Send("There is no shop here.");
                return true;
            }

            Shop shop = shops[0];
            StringTable table = new StringTable("Item", "Qty.", "Price");
            table.Columns[1].Alignment = StringTable.Column.Align.Right;
            table.Columns[2].Alignment = StringTable.Column.Align.Right;

            foreach (Guid id in shop.Items)
            {
                Item item = Server.Instance.Get<Item>(id);
                if (item != null)
                    table.AddRow(item.Name, "--", "$" + shop.SellPrice(item).ToString(Server.Instance.Settings.NumberFormat));
            }

            Dictionary<string, List<Item>> stock = Utils.ConsolidateList<Item>(shop.Stock);
            foreach (KeyValuePair<string, List<Item>> pair in stock)
                table.AddRow(pair.Value[0].Name, pair.Value.Count.ToString(), "$" + shop.SellPrice(pair.Value[0]).ToString(Server.Instance.Settings.NumberFormat));

            table.Sort("Item");
            user.Send(shop.Name + " Inventory:\r\n" + table.ToString());
            return true;
        }

        public static bool DoBuy(User user)
        {
            if (GetArguments(user, User.SelectItemToBuy))
            {
                Shop shop = user.GetCommandArg<Shop>(0);
                Item selectedItem = user.GetCommandArg<Item>(1);
                if (shop == null)
                {
                    user.Send("There is nothing to buy here.");
                }
                else if (selectedItem != null)
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

        public static bool DoSell(User user)
        {
            if (GetArguments(user, User.SelectItemToSell))
            {
                Shop shop = user.GetCommandArg<Shop>(0);
                Item selectedItem = user.GetCommandArg<Item>(1);
                if (shop == null)
                    user.Send("There is nowhere to sell here.");
                else if (selectedItem != null)
                    shop.Buy(selectedItem, user.Character);
                else
                    user.Send("Item not found in inventory.");
                return true;
            }
            return false;
        }

        public static bool DoEvaluate(User user)
        {
            List<Shop> shops = user.Character.NearbyObjects<Shop>(0, true);
            if (shops.Count < 1)
            {
                user.Send("There is no shopkeeper here to evaluate your item.");
                return true;
            }
            if (GetArguments(user, User.SelectItemInInventory))
            {
                Item item = user.GetCommandArg<Item>(0);
                if (item == null)
                    user.Send("Item not found.");
                else
                    user.Send(shops[0].Name + " will pay $" + shops[0].BuyPrice(item).ToString(Server.Instance.Settings.NumberFormat) + " for " + item.Name + ".");
                return true;
            }
            return false;
        }

        public static bool DoLearn(User user)
        {
            if (GetArguments(user, User.SelectNpcRoom, User.SelectAbility))
            {
                Npc npc = user.GetCommandArg<Npc>(0);
                Ability ability = user.GetCommandArg<Ability>(1);
                if (npc == null || !npc.CanTeach)
                {
                    user.Send("They have nothing to teach you.");
                    return true;
                }
                if (ability == null || !npc.TeachableAbilities.ContainsKey(ability.Id))
                {
                    user.Send("They don't teach that ability.");
                    return true;
                }
                if (user.Character.Abilities.ContainsKey(ability.Id))
                {
                    user.Send("You already know that ability.");
                    return true;
                }
                if (!user.Character.CanUse(ability, true))
                {
                    user.Send("You don't meet the requirements to learn that ability.");
                    return true;
                }

                int cost = npc.TeachableAbilities[ability.Id];
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

        public static bool DoTeachPlayer(User user)
        {
            string targetName = user.GetArgument();
            string abilityName = user.GetArgument();
            if (string.IsNullOrEmpty(targetName) || string.IsNullOrEmpty(abilityName))
            {
                user.Send("Usage: teach <player> <ability>");
                return true;
            }

            Character target = user.Character.FindNearby<Character>(targetName, 0, true);
            Ability ability = Server.Instance.Get<Ability>(abilityName);
            if (target == null)
            {
                user.Send("Target not found: " + targetName);
                return true;
            }
            if (ability == null || (! (user.Character is Npc) && !user.Character.Abilities.ContainsKey(ability.Id)))
            {
                user.Send("You don't know that ability.");
                return true;
            }
            if (target.Abilities.ContainsKey(ability.Id))
            {
                user.Send(target.Name + " already knows " + ability.Name + ".");
                return true;
            }
            if (!target.CanUse(ability, true))
            {
                user.Send(target.Name + " cannot learn " + ability.Name + ".");
                return true;
            }

            target.Abilities.Add(ability.Id, 0);
            user.Send("You teach " + ability.Name + " to " + target.Name + ".");
            target.Send(Server.Instance.Settings.GetColorCode(Settings.Text.AbilityImprovements) +
                user.Character.Name + " has taught you a new ability! (" + ability.Name + ")");
            return true;
        }

        public static bool DoTime(User user)
        {
            TimeSpan uptime = DateTime.Now - Server.Instance.StartTime;
            user.Send("Server Time: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") +
                "\r\nUptime:      " + (int)uptime.TotalHours + "h " + uptime.Minutes + "m " + uptime.Seconds + "s");
            return true;
        }

        public static bool DoWeather(User user)
        {
            Area area = user.Character.Area;
            if (area == null)
            {
                user.Send("You cannot tell the weather here.");
                return true;
            }
            Area.Layer layer = user.Character.OnLayer;
            string weather = (layer != null && !Damdai.Collections.Util.IsEmpty(layer.WeatherConditions.PossibleConditions)
                ? layer.WeatherConditions.ToString()
                : area.WeatherConditions.ToString());
            user.Send("Current weather in " + area.Name + ": " + weather);
            return true;
        }

        public static bool DoSet(User user)
        {
            if (GetArguments(user, User.SelectSetting))
            {
                string cmd = user.GetCommandArg<string>(0);
                Server.InputHandler handler = User.GetSettingHandler(cmd);
                if (handler == null)
                    user.Send("Invalid setting: " + cmd);
                else
                    return handler(user);
                return true;
            }
            return false;
        }

        public static bool DoAlias(User user)
        {
            if (user.CurrentCommandArgs.Count == 0 && !string.IsNullOrEmpty(user.Command))
            {
                string trimmed = user.Command.Trim();
                string[] parts = trimmed.Split(new char[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 2 &&
                    !parts[0].Equals("add", StringComparison.OrdinalIgnoreCase) &&
                    !parts[0].Equals("delete", StringComparison.OrdinalIgnoreCase) &&
                    !parts[0].Equals("list", StringComparison.OrdinalIgnoreCase))
                {
                    user.Command = "add " + trimmed;
                }
            }

            if (GetArguments(user, User.SelectAliasCommand))
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

        public static bool DoForums(User user)
        {
            if (GetArguments(user, User.SelectForumCommand))
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

        public static bool DoChannels(User user)
        {
            Menu menu = new Menu();
            foreach (Channel channel in Server.Instance.DataSets[typeof(Channel)].Values)
                menu.AddOption(channel.Name);

            if (menu.Options != null)
                user.Send(menu.ToString());
            else
                user.Send("There are no channels.");
            return true;
        }

        public static bool DoSocials(User user)
        {
            int maxLength = 0;
            SortedDictionary<string, Social> socials = new SortedDictionary<string, Social>();
            foreach (Social social in Server.Instance.DataSets[typeof(Social)].Values)
            {
                if (!string.IsNullOrEmpty(social.Name) && !socials.ContainsKey(social.Name))
                {
                    socials.Add(social.Name, social);
                    if (social.Name.Length > maxLength)
                        maxLength = social.Name.Length;
                }
            }

            if (socials.Count == 0)
            {
                user.Send("There are no socials.");
                return true;
            }

            int maxColumns = Math.Max(1, 70 / Math.Max(1, maxLength));
            string str = string.Empty;
            int count = 0;
            foreach (Social social in socials.Values)
            {
                str += string.Format("{0,-" + maxLength + "}  ", social.Name);
                if (++count % maxColumns == 0)
                    str += "\r\n";
            }
            user.Send(str.TrimEnd('\r', '\n'));
            return true;
        }

        public static bool DoCommands(User user)
        {
            Menu menu = new Menu();
            foreach (Command c in commands_)
            {
                if (c.SystemCommand || c.Hidden)
                    continue;

                string text = c.Name;
                if (!c.CanUseIncapacitated && user.Character.Incapacitated)
                    text = "{color:gray}" + c.Name + "{color:reset}";
                menu.AddOption(new Menu.Option(c.Name, text, null));
            }
            user.Send(menu.ToString());
            return true;
        }

        public static bool DoHelp(User user)
        {
            if (GetArguments(user, User.SelectHelp))
            {
                object arg = user.GetArg(0);
                Help help = arg as Help;
                if (help == null && arg is string)
                    help = Help.Get((string)arg);

                if (help == null)
                    user.Send("No help available for that topic.");
                else
                    user.Send(String.Format(help.Print()).TrimEnd('\r', '\n'));
                return true;
            }
            return false;
        }

        public static bool DoSave(User user)
        {
            user.Save();
            user.Send("Character saved.");
            return true;
        }

        public static bool DoPassword(User user)
        {
            if (GetArguments(user, User.EnterPassword, User.ConfirmPassword))
            {
                string pass1 = user.GetCommandArg<string>(0);
                string pass2 = user.GetCommandArg<string>(1);
                if (pass1 == pass2)
                {
                    user.SetPassword(pass1);
                    user.Save();
                    user.Send("Password changed.");
                }
                else
                {
                    user.SendError("Passwords do not match.");
                }
                return true;
            }
            return false;
        }

        public static bool DoDelete(User user)
        {
            if (GetArguments(user, User.EnterPassword, User.ConfirmPassword))
            {
                string arg1 = user.GetCommandArg<string>(0);
                string arg2 = user.GetCommandArg<string>(1);
                if (arg1 == arg2 && user.VerifyPassword(arg2))
                {
                    string name = user.Character.Name;
                    user.Send("Thank you for playing " + Server.Instance.Settings.Name + ".");
                    user.Character.DropItems(false);
                    if (user.Character.Area != null)
                        user.Character.Area.Send(new String(null, null, null, null, "{1} disintegrates!", null), user.Character, null, 0);
                    Server.Instance.Settings.InfoChannel.Broadcast(InfoChannel.Event.Deleted, name + " has deleted.");
                    user.Delete();
                    Server.Instance.Unload(user, false);
                    Server.Instance.Disconnect(user);
                }
                else
                {
                    user.Send("Invalid password or passwords do not match.");
                }
                return true;
            }
            return false;
        }

        public static bool DoQuit(User user)
        {
            if (user.Character.FightSystem != null)
            {
                user.Send("You are still involved in a fight!");
                return true;
            }

            string name = user.Character.Name;
            String str = new String("You fade out of existence.", null, null, null, "{1} fades out of existence.", null);
            if (user.Character.Area != null)
                user.Character.Area.Send(str, user.Character, null, 0);
            user.Send("Goodbye.");
            Server.Instance.Settings.InfoChannel.Broadcast(InfoChannel.Event.Logout, name + " has logged out.");
            Server.Instance.Unload(user, true);
            Server.Instance.Disconnect(user);
            Trace.WriteLine(name + " has quit.");
            return true;
        }

        // Script Commands
        public static bool DoPeace(User user)
        {
            if (user.Character.FightSystem != null)
            {
                user.Character.FightSystem.Finish(null);
                if (user.Character.CurrentAction is AbilityAction)
                    user.Character.Stop(false);
            }
            return true;
        }

        public static bool DoForce(User user)
        {
            Npc npc = user.Character as Npc;
            if (npc == null)
                return true;

            string arg1 = user.GetArgument();
            string arg2 = user.Command;
            user.Command = null;

            if (!string.IsNullOrEmpty(arg1) && !string.IsNullOrEmpty(arg2))
            {
                Character ch = npc.FindNearby<Character>(arg1, 0, false);
                if (ch != null && ch.User != null)
                {
                    ch.User.Command = arg2;
                    Server.Instance.ProcessCommand(ch.User, true);
                    ch.User.FinishInput();
                }
            }
            return true;
        }

        public static bool DoCreateItem(User user)
        {
            string arg1 = user.Command;
            user.Command = null;
            if (!string.IsNullOrEmpty(arg1))
            {
                Item bp = Server.Instance.Get<Item>(arg1);
                if (bp != null)
                {
                    Item item = Item.Create(bp);
                    if (item != null)
                        item.Parent = user.Character;
                }
            }
            return true;
        }

        public static bool DoPrint(User user)
        {
            string msg = user.Command;
            user.Command = null;
            if (!string.IsNullOrEmpty(msg) && user.Character.Area != null)
            {
                user.Character.Area.Send(new String(msg, msg, msg, msg, msg, msg), user.Character, null, 0);
            }
            return true;
        }

        public static bool DoSummonNpc(User user)
        {
            string arg1 = user.Command;
            user.Command = null;
            if (!string.IsNullOrEmpty(arg1))
            {
                Npc bp = Server.Instance.Get<Npc>(arg1);
                if (bp != null)
                {
                    Npc npc = Npc.Create(bp);
                    if (npc != null)
                        npc.Location = user.Character.Location;
                }
            }
            return true;
        }

        // Player QoL, Combat, Economy & Social Commands
        public static bool DoFlee(User user)
        {
            if (user.Character.FightSystem == null)
            {
                user.Send("You aren't fighting anyone!");
                return true;
            }

            List<Area.Direction> validDirs = new List<Area.Direction>();
            if (user.Character.Area != null)
            {
                for (int i = 0; i < 8; i++)
                {
                    Area.Direction dir = (Area.Direction)i;
                    System.Drawing.Point dest = Area.GetNextPoint(user.Character.Location.Point, dir);
                    if (user.Character.CanMove(dest, false))
                        validDirs.Add(dir);
                }
            }

            user.Character.FightSystem.Finish("Fled from combat.");
            if (user.Character.CurrentAction != null)
                user.Character.Stop(false);

            if (validDirs.Count > 0)
            {
                Area.Direction chosen = validDirs[Server.RandomNumber.Next(validDirs.Count)];
                string dirStr = chosen.ToString().ToLower();
                String leaveMsg = new String(
                    "{color:yellow}You flee " + dirStr + " from combat!{color:reset}", null,
                    null, null,
                    "{1} flees " + dirStr + " from combat!", null);
                String arriveMsg = new String(null, null, null, null, "{1} arrives in a panic.", null);
                user.Character.Move(chosen, leaveMsg, arriveMsg, false);
                DoLook(user);
            }
            else
            {
                user.Send("{color:yellow}You panic and disengage from combat!{color:reset}");
            }
            return true;
        }

        public static bool DoRecall(User user)
        {
            if (user.Character.FightSystem != null)
            {
                user.Send("You cannot recall while fighting! Use 'flee' instead.");
                return true;
            }

            Area destArea = Server.Instance.MainArea ?? user.Character.Area;
            if (destArea == null)
            {
                user.Send("There is nowhere to recall to.");
                return true;
            }

            if (user.Character.CurrentAction != null)
                user.Character.Stop(true);

            if (user.Character.Area != null)
            {
                user.Character.Area.Send(
                    new String(null, null, null, null, "{1} disappears in a flash of bright light.", null),
                    user.Character, null, 0);
            }

            user.Character.Location = new Location(destArea, destArea.Start);
            user.Send("{color:brightcyan}You pray for recall and are transported back to safety!{color:reset}");
            destArea.Send(
                new String(null, null, null, null, "{1} appears in a flash of bright light.", null),
                user.Character, null, 0);
            DoLook(user);
            return true;
        }

        public static bool DoConsider(User user)
        {
            string targetName = user.GetArgument();
            if (string.IsNullOrEmpty(targetName))
            {
                user.Send("Consider whom?");
                return true;
            }

            Character target = user.Character.FindNearby<Character>(targetName, 0, true);
            if (target == null || target == user.Character)
            {
                user.Send("They aren't here.");
                return true;
            }

            long myPower = 0;
            foreach (Stat s in user.Character.Stats.Values)
                myPower += s.Max;
            long theirPower = 0;
            foreach (Stat s in target.Stats.Values)
                theirPower += s.Max;

            double ratio = (myPower > 0) ? ((double)theirPower / (double)myPower) : 1.0;
            string verdict;
            if (ratio < 0.5)
                verdict = "{color:brightgreen}You could defeat " + target.Name + " effortlessly.{color:reset}";
            else if (ratio < 0.85)
                verdict = "{color:green}" + String.Capitalize(target.Name) + " looks weaker than you.{color:reset}";
            else if (ratio <= 1.15)
                verdict = "{color:yellow}" + String.Capitalize(target.Name) + " looks like a fair and even match.{color:reset}";
            else if (ratio <= 1.6)
                verdict = "{color:brightred}" + String.Capitalize(target.Name) + " looks tough! Be careful.{color:reset}";
            else
                verdict = "{color:red}" + String.Capitalize(target.Name) + " would crush you completely!{color:reset}";

            user.Send("Considering " + target.Name + ":\r\n" + verdict);
            return true;
        }

        public static bool DoScan(User user)
        {
            if (user.Character.Area == null)
            {
                user.Send("You see nothing nearby.");
                return true;
            }

            List<Object> nearby = user.Character.NearbyObjects<Object>((int)Area.Range.Nearby, true, user.Character);
            StringTable table = new StringTable("Object", "Type", "Direction", "Distance");
            table.Columns[3].Alignment = StringTable.Column.Align.Right;
            int count = 0;
            foreach (Object o in nearby)
            {
                if (o == null || o == user.Character || o is Area.Layer)
                    continue;
                int dist = user.Character.Distance(o);
                string dir = (dist == 0)
                    ? "here"
                    : user.Character.GetDirection(o.Location.Point).ToString().ToLower();
                string typeName = (o is Npc) ? "NPC" : (o is Character) ? "Player" : (o is IPortal) ? "Portal" : (o is Shop) ? "Shop" : (o is Item) ? "Item" : "Object";
                table.AddRow(o.Name, typeName, dir, dist.ToString());
                count++;
            }

            if (count == 0)
                user.Send("You scan the surroundings and see nothing of note nearby.");
            else
                user.Send("Nearby surroundings:\r\n" + table.ToString());
            return true;
        }

        public static bool DoExits(User user)
        {
            if (user.Character.Area == null)
            {
                user.Send("Obvious exits: none.");
                return true;
            }

            List<string> exits = new List<string>();
            for (int i = 0; i < 8; i++)
            {
                Area.Direction dir = (Area.Direction)i;
                System.Drawing.Point pt = Area.GetNextPoint(user.Character.Location.Point, dir);
                if (user.Character.CanMove(pt, false))
                {
                    Area.Layer layer = user.Character.Area.GetLayer<Area.Layer>(pt, false);
                    string terrain = (layer != null && !string.IsNullOrEmpty(layer.Name)) ? (" (" + layer.Name + ")") : string.Empty;
                    exits.Add(dir.ToString().ToLower() + terrain);
                }
            }

            List<IPortal> portals = user.Character.NearbyObjects<IPortal>(0, true);
            foreach (IPortal p in portals)
            {
                if (!string.IsNullOrEmpty(p.Name))
                    exits.Add("enter " + p.Name);
            }

            if (exits.Count == 0)
                user.Send("Obvious exits: none.");
            else
                user.Send("{color:brightcyan}Obvious exits:{color:reset} " + string.Join(", ", exits.ToArray()));
            return true;
        }

        public static bool DoGroupTell(User user)
        {
            string msg = user.Command;
            user.Command = null;
            if (string.IsNullOrEmpty(msg))
            {
                user.Send("Tell your group what?");
                return true;
            }

            Character leader = user.Character.Master ?? user.Character;
            List<Character> members = new List<Character>();
            members.Add(leader);
            if (user.Character.Area != null)
            {
                foreach (Character f in user.Character.Area.GetChildren<Character>())
                {
                    if (f != null && f != leader && f.Master == leader && !members.Contains(f))
                        members.Add(f);
                }
            }

            if (members.Count <= 1 && user.Character.Master == null)
            {
                user.Send("You are not in a group.");
                return true;
            }

            string formatted = "{color:brightgreen}[GROUP] " + user.Character.Name + ": " + msg + "{color:reset}";
            foreach (Character m in members)
                m.Send(formatted);
            return true;
        }

        public static bool DoSplit(User user)
        {
            string arg = user.GetArgument();
            int amount;
            if (string.IsNullOrEmpty(arg) || !int.TryParse(arg.TrimStart('$'), out amount) || amount <= 0)
            {
                user.Send("Usage: split <amount>");
                return true;
            }
            if (amount > user.Character.Value)
            {
                user.Send("You don't have that much gold.");
                return true;
            }

            Character leader = user.Character.Master ?? user.Character;
            List<Character> localMembers = new List<Character>();
            if (leader.Area == user.Character.Area && leader.Distance(user.Character) == 0)
                localMembers.Add(leader);
            if (user.Character.Area != null)
            {
                foreach (Character f in user.Character.Area.GetChildren<Character>())
                {
                    if (f != null && f != leader && f.Master == leader && f.Area == user.Character.Area && f.Distance(user.Character) == 0 && !localMembers.Contains(f))
                        localMembers.Add(f);
                }
            }

            if (localMembers.Count <= 1)
            {
                user.Send("There is no one in your group here to split money with.");
                return true;
            }

            int share = amount / localMembers.Count;
            int remainder = amount - (share * localMembers.Count);
            user.Character.Value -= amount;
            user.Character.Value += remainder;

            foreach (Character m in localMembers)
            {
                m.Value += share;
                if (m == user.Character)
                    m.Send("You split $" + amount + " among " + localMembers.Count + " group members (your share: $" + (share + remainder) + ").");
                else
                    m.Send(user.Character.Name + " splits $" + amount + "; your share is $" + share + ".");
            }
            return true;
        }

        public static bool DoBank(User user)
        {
            string sub = user.GetArgument();
            if (!string.IsNullOrEmpty(sub))
            {
                if (sub.Equals("deposit", StringComparison.OrdinalIgnoreCase))
                    return DoDeposit(user);
                if (sub.Equals("withdraw", StringComparison.OrdinalIgnoreCase))
                    return DoWithdraw(user);
            }

            string numFmt = Server.Instance.Settings.NumberFormat;
            user.Send("{color:brightyellow}=== Royal Bank Statement ==={color:reset}\r\n" +
                      "Gold on Hand: $" + user.Character.Value.ToString(numFmt) + "\r\n" +
                      "Bank Balance: $" + user.BankBalance.ToString(numFmt));
            return true;
        }

        public static bool DoDeposit(User user)
        {
            string arg = user.GetArgument();
            if (string.IsNullOrEmpty(arg))
            {
                user.Send("Usage: deposit <amount|all>");
                return true;
            }

            int amount;
            if (arg.Equals("all", StringComparison.OrdinalIgnoreCase))
                amount = user.Character.Value;
            else if (!int.TryParse(arg.TrimStart('$'), out amount) || amount <= 0)
            {
                user.Send("Please specify a valid positive amount to deposit.");
                return true;
            }

            if (amount <= 0 || amount > user.Character.Value)
            {
                user.Send("You don't have that much gold on hand.");
                return true;
            }

            user.Character.Value -= amount;
            user.BankBalance += amount;
            string numFmt = Server.Instance.Settings.NumberFormat;
            user.Send("You deposit $" + amount.ToString(numFmt) + ". New bank balance: $" + user.BankBalance.ToString(numFmt) + ".");
            return true;
        }

        public static bool DoWithdraw(User user)
        {
            string arg = user.GetArgument();
            if (string.IsNullOrEmpty(arg))
            {
                user.Send("Usage: withdraw <amount|all>");
                return true;
            }

            int amount;
            if (arg.Equals("all", StringComparison.OrdinalIgnoreCase))
                amount = user.BankBalance;
            else if (!int.TryParse(arg.TrimStart('$'), out amount) || amount <= 0)
            {
                user.Send("Please specify a valid positive amount to withdraw.");
                return true;
            }

            if (amount <= 0 || amount > user.BankBalance)
            {
                user.Send("You don't have that much gold in your bank account.");
                return true;
            }

            user.BankBalance -= amount;
            user.Character.Value += amount;
            string numFmt = Server.Instance.Settings.NumberFormat;
            user.Send("You withdraw $" + amount.ToString(numFmt) + ". Remaining bank balance: $" + user.BankBalance.ToString(numFmt) + ".");
            return true;
        }

        public static bool DoSacrifice(User user)
        {
            string arg = user.GetArgument();
            if (string.IsNullOrEmpty(arg))
            {
                user.Send("Sacrifice what?");
                return true;
            }

            Item item = user.Character.FindNearby<Item>(arg, 0, true) ?? user.Character.FindItemInInventory(arg);
            if (item == null)
            {
                user.Send("You don't see that item here.");
                return true;
            }

            int reward = Math.Max(1, Math.Min(10, item.Value / 10 + 1));
            string itemName = item.Name;
            item.Parent = null;
            user.Character.Value += reward;
            user.Send("You sacrifice " + itemName + " and receive $" + reward + ".");
            return true;
        }

        public static bool DoDescription(User user)
        {
            string desc = user.Command;
            user.Command = null;
            if (string.IsNullOrEmpty(desc))
            {
                if (string.IsNullOrEmpty(user.Character.Description))
                    user.Send("You have no character description set. Usage: description <text|clear>");
                else
                    user.Send("Your description:\r\n" + user.Character.Description);
                return true;
            }

            if (desc.Equals("clear", StringComparison.OrdinalIgnoreCase) || desc.Equals("none", StringComparison.OrdinalIgnoreCase))
            {
                user.Character.Description = string.Empty;
                user.Send("Your description has been cleared.");
            }
            else
            {
                user.Character.Description = desc;
                user.Send("Your description has been updated.");
            }
            return true;
        }

        public static bool DoTitle(User user)
        {
            string title = user.Command;
            user.Command = null;
            if (string.IsNullOrEmpty(title))
            {
                if (string.IsNullOrEmpty(user.Title))
                    user.Send("You have no title set. Usage: title <text|clear>");
                else
                    user.Send("Your current title is: " + user.Character.Name + " " + user.Title);
                return true;
            }

            if (title.Equals("clear", StringComparison.OrdinalIgnoreCase) || title.Equals("none", StringComparison.OrdinalIgnoreCase))
            {
                user.Title = string.Empty;
                user.Send("Your title has been cleared.");
            }
            else
            {
                user.Title = title.Trim();
                user.Send("Your title is now: " + user.Character.Name + " " + user.Title);
            }
            return true;
        }

        public static bool DoAfk(User user)
        {
            string msg = user.Command;
            user.Command = null;
            if (!string.IsNullOrEmpty(msg))
            {
                user.IsAfk = true;
                user.AfkMessage = msg.Trim();
                user.Send("{color:yellow}You are now AFK (" + user.AfkMessage + ").{color:reset}");
            }
            else
            {
                user.IsAfk = !user.IsAfk;
                if (!user.IsAfk)
                {
                    user.AfkMessage = string.Empty;
                    user.Send("{color:brightgreen}You are no longer AFK.{color:reset}");
                }
                else
                {
                    user.Send("{color:yellow}You are now AFK.{color:reset}");
                }
            }
            return true;
        }

        public static bool DoIgnore(User user)
        {
            string target = user.GetArgument();
            if (string.IsNullOrEmpty(target))
            {
                if (user.IgnoredUsers.Count == 0)
                    user.Send("You are not ignoring anyone.");
                else
                    user.Send("Ignored players: " + string.Join(", ", user.IgnoredUsers.ToArray()));
                return true;
            }

            int idx = user.IgnoredUsers.FindIndex(delegate(string s)
            {
                return s.Equals(target, StringComparison.OrdinalIgnoreCase);
            });

            if (idx >= 0)
            {
                string removed = user.IgnoredUsers[idx];
                user.IgnoredUsers.RemoveAt(idx);
                user.Send("You are no longer ignoring " + removed + ".");
            }
            else
            {
                user.IgnoredUsers.Add(target);
                user.Send("You are now ignoring " + target + ".");
            }
            return true;
        }

        // Administrator / Immortal Commands
        public static bool RequireAdmin(User user)
        {
            if (user == null || !user.Flags.Get((int)User.UserFlags.Administrator))
            {
                if (user != null)
                    user.Send("You do not have administrator privileges.");
                return false;
            }
            return true;
        }

        public static User FindOnlineUser(string name)
        {
            if (string.IsNullOrEmpty(name) || Server.Instance == null)
                return null;
            foreach (User u in Damdai.Collections.Util.CopyList<User>(Server.Instance.Users))
            {
                if (u != null && u.Character != null && u.Character.Name != null &&
                    u.Character.Name.StartsWith(name, StringComparison.OrdinalIgnoreCase))
                    return u;
            }
            return null;
        }

        public static bool DoWizHelp(User user)
        {
            if (!RequireAdmin(user))
                return true;

            StringTable table = new StringTable("Command", "Syntax", "Description");
            table.AddRow("wizhelp", "wizhelp", "List all administrator commands");
            table.AddRow("goto", "goto <target|x y>", "Teleport to a player, NPC, area, or coordinates");
            table.AddRow("transfer", "transfer <target>", "Summon a player or NPC to your location");
            table.AddRow("slay", "slay <target>", "Instantly slay a target character or NPC");
            table.AddRow("restore", "restore [target|all]", "Restore full stats and clear status effects");
            table.AddRow("purge", "purge [target]", "Remove non-player objects in the room");
            table.AddRow("spawn", "spawn <npc|item> <name>", "Spawn an NPC or Item from blueprint");
            table.AddRow("announce", "announce <message>", "Broadcast a global server announcement");
            table.AddRow("wizstat", "wizstat [target]", "Inspect detailed stats and flags of an object");
            table.AddRow("setstat", "setstat <target> <stat> <val>", "Set a target's stat, money, or bank balance");
            table.AddRow("silence", "silence <player>", "Toggle silence on a player");
            table.AddRow("kick", "kick <player>", "Disconnect a player from the server");
            table.AddRow("makeadmin", "makeadmin <player>", "Grant or revoke Administrator privileges");
            user.Send("{color:brightcyan}=== Immortal / Administrator Commands ==={color:reset}\r\n" + table.ToString());
            return true;
        }

        public static bool DoGoto(User user)
        {
            if (!RequireAdmin(user))
                return true;

            string arg1 = user.GetArgument();
            string arg2 = user.GetArgument();
            if (string.IsNullOrEmpty(arg1))
            {
                user.Send("Usage: goto <player|npc|area|x y>");
                return true;
            }

            int x, y;
            if (!string.IsNullOrEmpty(arg2) && int.TryParse(arg1, out x) && int.TryParse(arg2, out y))
            {
                Area area = user.Character.Area ?? Server.Instance.MainArea;
                user.Character.Location = new Location(area, new System.Drawing.Point(x, y));
                user.Send("You teleport to (" + x + ", " + y + ") in " + area.Name + ".");
                DoLook(user);
                return true;
            }

            User targetUser = FindOnlineUser(arg1);
            if (targetUser != null && targetUser.Character != null && targetUser.Character.Area != null)
            {
                user.Character.Location = targetUser.Character.Location;
                user.Send("You teleport to " + targetUser.Character.Name + ".");
                DoLook(user);
                return true;
            }

            Character targetChar = user.Character.FindNearby<Character>(arg1, 100, false);
            if (targetChar != null)
            {
                user.Character.Location = targetChar.Location;
                user.Send("You teleport to " + targetChar.Name + ".");
                DoLook(user);
                return true;
            }

            Area targetArea = Server.Instance.Get<Area>(arg1);
            if (targetArea != null)
            {
                user.Character.Location = new Location(targetArea, targetArea.Start);
                user.Send("You teleport to " + targetArea.Name + ".");
                DoLook(user);
                return true;
            }

            user.Send("Target not found: " + arg1);
            return true;
        }

        public static bool DoTransferPlayer(User user)
        {
            if (!RequireAdmin(user))
                return true;

            string targetName = user.GetArgument();
            if (string.IsNullOrEmpty(targetName))
            {
                user.Send("Usage: transfer <player|npc>");
                return true;
            }

            User targetUser = FindOnlineUser(targetName);
            if (targetUser != null && targetUser.Character != null)
            {
                targetUser.Character.Location = user.Character.Location;
                targetUser.Send("{color:brightcyan}" + user.Character.Name + " has transferred you!{color:reset}");
                user.Send("You transfer " + targetUser.Character.Name + " to your location.");
                return true;
            }

            Character targetChar = user.Character.FindNearby<Character>(targetName, 100, false);
            if (targetChar != null)
            {
                targetChar.Location = user.Character.Location;
                user.Send("You transfer " + targetChar.Name + " to your location.");
                return true;
            }

            user.Send("Target not found: " + targetName);
            return true;
        }

        public static bool DoSlay(User user)
        {
            if (!RequireAdmin(user))
                return true;

            string targetName = user.GetArgument();
            if (string.IsNullOrEmpty(targetName))
            {
                user.Send("Usage: slay <target>");
                return true;
            }

            Character target = user.Character.FindNearby<Character>(targetName, 0, true);
            if (target == null || target == user.Character)
            {
                user.Send("Target not found here.");
                return true;
            }

            user.Character.Area.Send(
                new String(
                    "{color:brightred}You call down divine wrath and slay {2} instantly!{color:reset}",
                    "{color:brightred}You call down divine wrath and slay {2} instantly!{color:reset}",
                    "{color:brightred}{1} calls down divine wrath and slays you instantly!{color:reset}",
                    "{color:brightred}{1} calls down divine wrath and slays you instantly!{color:reset}",
                    "{color:brightred}{1} calls down divine wrath and slays {2} instantly!{color:reset}", null),
                user.Character, target, 0);
            target.Die(user.Character);
            return true;
        }

        public static bool DoRestoreAdmin(User user)
        {
            if (!RequireAdmin(user))
                return true;

            string targetName = user.GetArgument();
            List<Character> targets = new List<Character>();

            if (string.IsNullOrEmpty(targetName) || targetName.Equals("self", StringComparison.OrdinalIgnoreCase) || targetName.Equals("me", StringComparison.OrdinalIgnoreCase))
            {
                targets.Add(user.Character);
            }
            else if (targetName.Equals("all", StringComparison.OrdinalIgnoreCase))
            {
                foreach (User u in Damdai.Collections.Util.CopyList<User>(Server.Instance.Users))
                    if (u != null && u.Character != null)
                        targets.Add(u.Character);
            }
            else
            {
                User targetUser = FindOnlineUser(targetName);
                if (targetUser != null && targetUser.Character != null)
                    targets.Add(targetUser.Character);
                else
                {
                    Character nearby = user.Character.FindNearby<Character>(targetName, 0, true);
                    if (nearby != null)
                        targets.Add(nearby);
                }
            }

            if (targets.Count == 0)
            {
                user.Send("Target not found: " + targetName);
                return true;
            }

            foreach (Character ch in targets)
            {
                foreach (Stat s in ch.Stats.Values)
                    s.Current = s.Max;
                ch.StatusEffects.Clear();
                if (ch.Position < Object.Positions.Stopped)
                    ch.Position = Object.Positions.Stopped;
                if (ch != user.Character)
                    ch.Send("{color:brightcyan}You have been fully restored by " + user.Character.Name + "!{color:reset}");
            }

            user.Send("Restored " + targets.Count + " character(s) to full health and stats.");
            return true;
        }

        public static bool DoPurge(User user)
        {
            if (!RequireAdmin(user))
                return true;

            string targetName = user.GetArgument();
            if (!string.IsNullOrEmpty(targetName))
            {
                Object target = user.Character.FindNearby<Object>(targetName, 0, true);
                if (target == null || target == user.Character || (target is Character && !(target is Npc)))
                {
                    user.Send("Purgeable object not found: " + targetName);
                    return true;
                }
                string removedName = target.Name;
                target.Parent = null;
                user.Send("Purged " + removedName + ".");
                return true;
            }

            List<Object> roomObjs = user.Character.NearbyObjects<Object>(0, true, user.Character);
            int purged = 0;
            foreach (Object o in roomObjs)
            {
                if (o is Npc || o is Item)
                {
                    o.Parent = null;
                    purged++;
                }
            }
            user.Send("Purged " + purged + " object(s) from the room.");
            return true;
        }

        public static bool DoSpawn(User user)
        {
            if (!RequireAdmin(user))
                return true;

            string first = user.GetArgument();
            string rest = user.Command;
            user.Command = null;
            if (string.IsNullOrEmpty(first))
            {
                user.Send("Usage: spawn <npc|item> <blueprint name>");
                return true;
            }

            string kind = string.Empty;
            string bpName = first;
            if ((first.Equals("npc", StringComparison.OrdinalIgnoreCase) || first.Equals("item", StringComparison.OrdinalIgnoreCase)) && !string.IsNullOrEmpty(rest))
            {
                kind = first.ToLower();
                bpName = rest.Trim();
            }
            else if (!string.IsNullOrEmpty(rest))
            {
                bpName = (first + " " + rest).Trim();
            }

            if (kind != "item")
            {
                Npc npcBp = Server.Instance.Get<Npc>(bpName);
                if (npcBp != null)
                {
                    Npc npc = Npc.Create(npcBp);
                    npc.Location = user.Character.Location;
                    user.Send("Spawned NPC '" + npc.Name + "' at your location.");
                    return true;
                }
            }

            if (kind != "npc")
            {
                Item itemBp = Server.Instance.Get<Item>(bpName);
                if (itemBp != null)
                {
                    Item item = Item.Create(itemBp);
                    item.Parent = user.Character;
                    user.Send("Spawned Item '" + item.Name + "' in your inventory.");
                    return true;
                }
            }

            user.Send("No NPC or Item blueprint found matching '" + bpName + "'.");
            return true;
        }

        public static bool DoAnnounce(User user)
        {
            if (!RequireAdmin(user))
                return true;

            string msg = user.Command;
            user.Command = null;
            if (string.IsNullOrEmpty(msg))
            {
                user.Send("Usage: announce <message>");
                return true;
            }

            string broadcast = "{color:brightyellow}[ANNOUNCEMENT] " + user.Character.Name + ": " + msg + "{color:reset}";
            user.Send(broadcast);
            Server.Instance.Echo(broadcast, user.Character);
            return true;
        }

        public static bool DoWizStat(User user)
        {
            if (!RequireAdmin(user))
                return true;

            string targetName = user.GetArgument();
            Object target = null;
            if (string.IsNullOrEmpty(targetName) || targetName.Equals("self", StringComparison.OrdinalIgnoreCase))
            {
                target = user.Character;
            }
            else
            {
                target = user.Character.FindNearby<Object>(targetName, 0, true) ?? user.Character.FindItemInInventory(targetName);
                if (target == null)
                {
                    User u = FindOnlineUser(targetName);
                    if (u != null)
                        target = u.Character;
                }
            }

            if (target == null)
            {
                user.Send("Object not found: " + targetName);
                return true;
            }

            string info = "{color:brightcyan}=== WizStat: " + target.Name + " ==={color:reset}\r\n" +
                          "Type:     " + target.GetType().Name + "\r\n" +
                          "ID:       " + target.Id + "\r\n" +
                          "Location: " + (target.Area != null ? (target.Area.Name + " (" + target.Location.Point.X + ", " + target.Location.Point.Y + ")") : "Inventory/None") + "\r\n" +
                          "Position: " + target.Position + "\r\n" +
                          "Value:    $" + target.Value;

            Character ch = target as Character;
            if (ch != null)
            {
                if (ch.User != null)
                    info += "\r\nBank:     $" + ch.User.BankBalance + "\r\nAdmin:    " + ch.User.Flags.Get((int)User.UserFlags.Administrator);
                info += "\r\nItems:    " + ch.Inventory.Count + " in inventory, " + ch.Equipment.Count + " equipped";
                foreach (Stat s in ch.Stats.Values)
                {
                    StatBlueprint bp = s.Blueprint;
                    if (bp != null)
                        info += "\r\nStat [" + bp.Name + "]: " + s.Current + " / " + s.Max;
                }
            }

            user.Send(info);
            return true;
        }

        public static bool DoSetStat(User user)
        {
            if (!RequireAdmin(user))
                return true;

            string targetName = user.GetArgument();
            string statName = user.GetArgument();
            string valStr = user.GetArgument();
            long val;
            if (string.IsNullOrEmpty(targetName) || string.IsNullOrEmpty(statName) || string.IsNullOrEmpty(valStr) || !long.TryParse(valStr, out val))
            {
                user.Send("Usage: setstat <target> <stat|money|bank> <value>");
                return true;
            }

            Character target = null;
            if (targetName.Equals("self", StringComparison.OrdinalIgnoreCase) || targetName.Equals("me", StringComparison.OrdinalIgnoreCase))
                target = user.Character;
            else
            {
                User u = FindOnlineUser(targetName);
                target = (u != null && u.Character != null) ? u.Character : user.Character.FindNearby<Character>(targetName, 0, true);
            }

            if (target == null)
            {
                user.Send("Target character not found: " + targetName);
                return true;
            }

            if (statName.Equals("money", StringComparison.OrdinalIgnoreCase) || statName.Equals("gold", StringComparison.OrdinalIgnoreCase))
            {
                target.Value = (int)val;
                user.Send("Set " + target.Name + "'s money to $" + target.Value + ".");
                return true;
            }
            if (statName.Equals("bank", StringComparison.OrdinalIgnoreCase) && target.User != null)
            {
                target.User.BankBalance = (int)val;
                user.Send("Set " + target.Name + "'s bank balance to $" + target.User.BankBalance + ".");
                return true;
            }

            Stat stat = target.GetStat(statName);
            if (stat == null)
            {
                StatBlueprint bp = Server.Instance.Get<StatBlueprint>(statName);
                if (bp != null)
                {
                    stat = new Stat(bp.Id);
                    target.Stats[bp.Id] = stat;
                }
            }

            if (stat == null)
            {
                user.Send("Stat not found: " + statName);
                return true;
            }

            stat.Max = val;
            stat.Current = val;
            user.Send("Set " + target.Name + "'s " + stat.Blueprint.Name + " to " + val + ".");
            return true;
        }

        public static bool DoSilencePlayer(User user)
        {
            if (!RequireAdmin(user))
                return true;

            string targetName = user.GetArgument();
            User targetUser = FindOnlineUser(targetName);
            if (targetUser == null)
            {
                user.Send("No online player found matching '" + targetName + "'.");
                return true;
            }

            bool silenced = !targetUser.Flags.Get((int)User.UserFlags.Silenced);
            targetUser.Flags.Set((int)User.UserFlags.Silenced, silenced);
            user.Send(targetUser.Character.Name + " is now " + (silenced ? "silenced" : "unsilenced") + ".");
            targetUser.Send(silenced ? "{color:red}You have been silenced by an administrator.{color:reset}" : "{color:green}Your silence has been lifted.{color:reset}");
            return true;
        }

        public static bool DoKickPlayer(User user)
        {
            if (!RequireAdmin(user))
                return true;

            string targetName = user.GetArgument();
            User targetUser = FindOnlineUser(targetName);
            if (targetUser == null || targetUser == user)
            {
                user.Send("Specify another online player to kick.");
                return true;
            }

            string kickedName = targetUser.Character.Name;
            targetUser.Send("{color:brightred}You have been disconnected by an administrator.{color:reset}");
            Server.Instance.Disconnect(targetUser);
            user.Send("Disconnected " + kickedName + ".");
            return true;
        }

        public static bool DoMakeAdmin(User user)
        {
            if (!RequireAdmin(user))
                return true;

            string targetName = user.GetArgument();
            User targetUser = FindOnlineUser(targetName);
            if (targetUser == null)
            {
                user.Send("No online player found matching '" + targetName + "'.");
                return true;
            }

            bool isAdmin = !targetUser.Flags.Get((int)User.UserFlags.Administrator);
            targetUser.Flags.Set((int)User.UserFlags.Administrator, isAdmin);
            user.Send(targetUser.Character.Name + " administrator status: " + (isAdmin ? "ENABLED" : "DISABLED") + ".");
            targetUser.Send(isAdmin
                ? "{color:brightcyan}You have been granted Administrator privileges! Type 'wizhelp' for commands.{color:reset}"
                : "{color:yellow}Your Administrator privileges have been revoked.{color:reset}");
            return true;
        }
        #endregion
    }

    internal class AbortCommandException : Exception
    {
    }

    [Obfuscation(Exclude = false)]
    internal class MenuFunctionConverter : TypeConverter
    {
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }
        public override System.ComponentModel.TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            List<string> list = new List<string>();
            MethodInfo[] methods = typeof(User).GetMethods(BindingFlags.DeclaredOnly | BindingFlags.NonPublic | BindingFlags.Static);
            foreach (MethodInfo method in methods)
            {
                ParameterInfo[] parameters = method.GetParameters();
                if (parameters.Length == 1 && parameters[0].ParameterType == typeof(User) && method.ReturnType == typeof(bool))
                    list.Add(method.Name);
            }
            list.Sort();
            return new StandardValuesCollection(list);
        }
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string))
                return true;
            return base.CanConvertFrom(context, sourceType);
        }
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return true;
        }
        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            return value;
        }
        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            return value;
        }
    }

    [Serializable]
    [Obfuscation(Exclude = true)]
    public class MenuFunction
    {
        string fnname_;
        Server.InputHandler handler_;

        [TypeConverter(typeof(MenuFunctionConverter))]
        public string Function
        {
            get { return this.fnname_; }
            set { this.fnname_ = value; this.handler_ = null; }
        }

        [XmlIgnore]
        [Browsable(false)]
        public Server.InputHandler Handler
        {
            get
            {
                if (this.handler_ == null && !string.IsNullOrEmpty(this.fnname_))
                {
                    MethodInfo method = typeof(User).GetMethod(this.fnname_, BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
                    if (method != null)
                        this.handler_ = Delegate.CreateDelegate(typeof(Server.InputHandler), method) as Server.InputHandler;
                }
                return this.handler_;
            }
        }

        [Obfuscation(Exclude = false)]
        public override string ToString()
        {
            if (string.IsNullOrEmpty(this.fnname_))
                return base.ToString();
            else
                return this.fnname_;
        }
    }
}

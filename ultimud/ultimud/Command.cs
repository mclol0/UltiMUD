using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Reflection;
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
            get { return MainForm.Instance.ServerPath + "\\Commands"; }
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
                string cmdspath = loadpath + "\\Commands";
                if (Directory.Exists(cmdspath))
                {
                    foreach (FileInfo file in new DirectoryInfo(cmdspath).GetFiles("*.cmd"))
                    {
                        Command cmd = Damdai.Serialization.Xml.Deserialize<Command>(file.FullName);
                        commands_.Add(cmd);
                    }
                }
                commands_.Sort(delegate(Command cmd1, Command cmd2)
                {
                    return cmd1.priority_.CompareTo(cmd2.priority_);
                });
            }
        }

        [Obfuscation(Exclude = false)]
        public static Command Get(string name, bool includesyscommands)
        {
            Command cmd = null;
            lock (commands_)
            {
                foreach (Command c in Command.Commands)
                {
                    if ((!c.SystemCommand || includesyscommands) && !string.IsNullOrEmpty(c.name_) && (c.Name.StartsWith(name, StringComparison.CurrentCultureIgnoreCase) || (c.aliases_ != null && c.aliases_.Contains(name))))
                    {
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
                if (this.lua_ == null)
                    this.Init();

                if (this.menuhandlers_.Length == 0 || Ultimud.Command.GetArguments(user, this.menuhandlers_))
                {
                    try
                    {
                        this.lua_["user"] = user;
                        this.lua_["finished"] = true;
                        this.lua_.DoString(this.script_);
                        return (bool)this.lua_["finished"];
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
                Damdai.Serialization.Xml.Serialize(this, Path + "\\" + this.name_ + ".cmd");
                this.Init();
            }
        }
        
        [Obfuscation(Exclude = false)]
        public void Delete()
        {
            if (!string.IsNullOrEmpty(this.name_))
                File.Delete(Path + "\\" + this.name_ + ".cmd");
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
                    if (!string.IsNullOrEmpty(menufn.Function))
                        list.Add(menufn.Handler);
                this.menuhandlers_ = list.ToArray();
                this.lua_ = new Lua();
            }
        }
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
                if (this.handler_ == null)
                {
                    MethodInfo method = typeof(User).GetMethod(this.fnname_, BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
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

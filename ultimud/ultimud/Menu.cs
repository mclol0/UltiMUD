using System;
using System.Collections.Generic;
using System.Reflection;

namespace Ultimud
{
    [Obfuscation(Exclude = true)]
    public class Menu
    {
        public enum Formats
        {
            Dynamic,
            List
        }

        [Obfuscation(Exclude = true)]
        public class Option : IComparable
        {
            [Obfuscation(Exclude = false)] string key_;
            [Obfuscation(Exclude = false)] string text_;
            [Obfuscation(Exclude = false)] object tag_;
            [Obfuscation(Exclude = false)] string[] columns_;

            #region Properties
            public string Key
            {
                get { return this.key_; }
                set { this.key_ = value; }
            }
            public string Text
            {
                get { return this.text_; }
            }
            public object Tag
            {
                get { return this.tag_; }
            }
            public string[] Columns
            {
                get { return this.columns_; }
            }
            #endregion

            public Option(string key, string text, object tag)
            {
                this.key_ = key;
                this.text_ = text;
                this.tag_ = tag;
            }
            public Option(string text, object tag) : this(text, text, tag) { }
            public Option(string key, string[] columns, object tag)
            {
                this.key_ = key;
                this.tag_ = tag;
                this.columns_ = columns;
            }
            public Option(string text) : this(text, text, null) { }

            [Obfuscation(Exclude = false)]
            public int CompareTo(object o)
            {
                Option option = o as Option;
                return this.Text.CompareTo(option.Text);
            }

            public override string ToString()
            {
                return this.text_;
            }
        }

        [Obfuscation(Exclude = false)] Server.InputHandler inputHandler_;
        [Obfuscation(Exclude = false)] List<Option> options_;
        [Obfuscation(Exclude = false)] string prompt_;
        [Obfuscation(Exclude = false)] bool cancellable_;
        [Obfuscation(Exclude = false)] string description_;
        [Obfuscation(Exclude = false)] Telnet.Color textColor_ = Telnet.Color.Cyan;
        [Obfuscation(Exclude = false)] Telnet.Color optionColor_ = Telnet.Color.Reset;
        [Obfuscation(Exclude = false)] Formats format_ = Formats.Dynamic;
        [Obfuscation(Exclude = false)] StringTable stringtable_;

        #region Properties
        public string Prompt
        {
            get
            {
                if (this.Options != null)
                {
                    if (this.prompt_ != null)
                    {
                        return this.prompt_;
                    }
                    else if (this.cancellable_)
                    {
                        return "Enter option or 'cancel': ";
                    }
                    else
                    {
                        return "Enter option: ";
                    }
                }
                else
                {
                    return this.prompt_;
                }
            }
            set { this.prompt_ = value; }
        }
        public List<Option> Options
        {
            get { return this.options_; }
            set { this.options_ = value; }
        }
        public Server.InputHandler InputHandler
        {
            get { return this.inputHandler_; }
        }
        public Telnet.Color TextColor
        {
            get { return this.textColor_; }
            set { this.textColor_ = value; }
        }
        public Telnet.Color OptionColor
        {
            get { return this.optionColor_; }
            set { this.optionColor_ = value; }
        }
        #endregion

        public Menu(StringTable stringtable)
        {
            if (stringtable != null)
            {
                this.stringtable_ = stringtable;
                this.format_ = Formats.List;
            }
        }
        public Menu(string description, StringTable stringtable)
        {
            this.description_ = description;
        }
        public Menu(Server.InputHandler inputHandler, string description, string prompt, bool cancellable, StringTable stringtable)
            : this(stringtable)
        {
            this.inputHandler_ = inputHandler;
            this.description_ = description;
            this.prompt_ = prompt;
            this.cancellable_ = cancellable;
        }
        public Menu(Server.InputHandler inputHandler, string prompt, bool cancellable, StringTable stringtable)
            : this(inputHandler, null, prompt, cancellable, stringtable)
        {
        }
        public Menu(Server.InputHandler inputHandler, string prompt, StringTable stringtable)
            : this(inputHandler, null, prompt, true, stringtable)
        {
        }

        public Menu()
        {
        }
        public Menu(string description)
            : this(description, null)
        {
        }
        public Menu(Server.InputHandler inputHandler, string description, string prompt, bool cancellable)
            : this(inputHandler, description, prompt, cancellable, null)
        {
        }
        public Menu(Server.InputHandler inputHandler, string prompt, bool cancellable)
            : this(inputHandler, null, prompt, cancellable, null)
        {
        }
        public Menu(Server.InputHandler inputHandler, string prompt)
            : this(inputHandler, null, prompt, true, null)
        {
        }

        public void AddOption(string str)
        {
            if (string.IsNullOrEmpty(str))
                return;
            this.AddOption(new Option(str));
        }
        public void AddOption(Option option)
        {
            if (this.Options == null)
                this.Options = new List<Option>();
            lock (this.Options)
                this.Options.Add(option);
        }
        public void AddOptions(params string[] options)
        {
            foreach (string str in options)
                if (!string.IsNullOrEmpty(str))
                    this.AddOption(str);
        }

        [Obfuscation(Exclude = false)]
        public bool ProcessCommand(User user)
        {
            //Debug.WriteLine("Processing menu: " + this.InputHandler.Method.Name);
            if (this.Options == null)
            {
                if (user.Menus.Count > 0)
                    user.Menus.Dequeue();
                return this.inputHandler_(user);
            }
            else if ("cancel".StartsWith(user.Command, StringComparison.CurrentCultureIgnoreCase) && this.cancellable_)
            {
                user.FinishInput();
                user.Send("Cancelled.");
                return false;
            }
            else
            {
                Option option = this.GetOption(user.Command);
                if (option == null)
                {
                    user.Send("Invalid option: " + user.Command);
                    return false;
                }
                else
                {
                    if (option.Tag != null)
                    {
                        user.CurrentArg = option.Tag;
                        user.Command = null;
                    }
                    else
                        user.Command = '"' + option.Key + '"';

                    if (user.Menus.Count > 0)
                        user.Menus.Dequeue();
                    
                    return this.inputHandler_(user);
                }
            }
        }

        [Obfuscation(Exclude = false)]
        Option GetOption(string str)
        {
            int num = 0;

            if (int.TryParse(str, out num) && num > 0 && num - 1 < this.Options.Count)
                return this.Options[num - 1];

            lock (this.Options)
            {
                foreach (Option option in this.Options)
                    if (String.FindArgument(option.Key, str))
                        return option;
            }
            return null;
        }
        
        public override string ToString()
        {
            switch (this.format_)
            {
                case Formats.Dynamic: return this.PrintDynamic();
                case Formats.List: return this.PrintList();
                default: throw new ApplicationException("Bad format: " + this.format_);
            }
        }

        [Obfuscation(Exclude = false)]
        string PrintDynamic()
        {
            string str = string.Empty;
            if (this.description_ != null)
            {
                str += Server.Instance.Settings.GetColorCode(Settings.Text.Help) + String.Format(String.Capitalize(this.description_)) + "\r\n\r\n" + Telnet.GetColorCode(this.OptionColor);
            }
            if (this.Options != null && this.Options.Count > 0)
            {
                this.Options.Sort();
                int maxLength = 0;
                List<Option> options = new List<Option>(this.Options);
                List<string> list = new List<string>();
                int count = 0;
                foreach (Option option in options)
                {
                    string temp = (this.InputHandler != null ? (++count + ". ") : null) + option.ToString();
                    list.Add(temp);
                    string temp2 = Telnet.Decolorize(temp);
                    maxLength = Math.Max(maxLength, temp2.Length);
                }

                int maxColumns = Math.Max(1, 70 / maxLength);
                count = 0;
                foreach (string option in list)
                {
                    //str += string.Format("{0,-" + maxLength + "}", option) + "  ";
                    str += option + (maxColumns > 1 ? String.PrintLine(' ', maxLength - String.Length(option) + 2) : null);
                    if (++count % maxColumns == 0)
                        str += "\r\n";
                }
            }
            return str.TrimEnd('\r', '\n');
        }

        [Obfuscation(Exclude = false)]
        string PrintList()
        {
            string str = string.Empty;
            if (this.description_ != null)
            {
                str += Server.Instance.Settings.GetColorCode(Settings.Text.Help) + String.Format(String.Capitalize(this.description_)) + "\r\n\r\n" + Telnet.GetColorCode(this.OptionColor);
            }
            if (this.Options != null && this.Options.Count > 0)
            {
                if (this.stringtable_.Columns[0].Name != "#")
                    this.stringtable_.Columns.Insert(0, new StringTable.Column(this.stringtable_, "#"));
                this.stringtable_.Rows.Clear();
                int count = 0;
                foreach (Option o in this.Options)
                {
                    string[] columns = new string[o.Columns.Length + 1];
                    columns[0] = (++count).ToString();
                    o.Columns.CopyTo(columns, 1);
                    this.stringtable_.AddRow(columns);
                }
                this.stringtable_.Sort("Item");
                str += this.stringtable_.ToString();
            }
            return str.TrimEnd('\r', '\n');
        }
    }
}

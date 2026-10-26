using System;
using System.Collections.Generic;
using System.Reflection;
using System.ComponentModel;
using System.Text;

namespace Ultimud
{
    [TypeConverter(typeof(ExpandableObjectConverter))]
    [Obfuscation(Exclude = true)]
    public class StringTable
    {
        [Serializable]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        [Obfuscation(Exclude = true)]
        public class Column
        {
            public enum Align
            {
                Left,
                Right
            }

            [NonSerialized]
            [Obfuscation(Exclude = false)]
            StringTable table_;

            [Obfuscation(Exclude = false)] string name_;
            [Obfuscation(Exclude = false)] int width_;
            [Obfuscation(Exclude = false)] Align alignment_ = Align.Left;

            #region Properties
            public string Name
            {
                get { return this.name_; }
                set
                {
                    this.name_ = value;
                    this.Width = Math.Max(this.Width, String.Length(value));
                }
            }
            public int Width
            {
                get { return this.width_; }
                set { this.width_ = value; }
            }
            
            [DefaultValue(Align.Left)]
            public Align Alignment
            {
                get { return this.alignment_; }
                set { this.alignment_ = value; }
            }

            [Browsable(false)]
            public StringTable Table
            {
                get { return this.table_; }
                set { this.table_ = value; }
            }
            #endregion

            public Column(StringTable table)
            {
                this.table_ = table;
            }
            public Column(StringTable table, string name)
                : this(table)
            {
                this.name_ = name;
                this.width_ = Math.Max(String.Length(name), this.width_);
            }
            public Column(StringTable table, string name, Align align)
                : this(table, name)
            {
                this.Alignment = align;
            }
            public Column(StringTable table, Column other)
                : this(table)
            {
                this.Name = other.Name;
                this.Alignment = other.Alignment;
                this.Width = other.Width;
            }
            
            [Obfuscation(Exclude = false)]
            public override string ToString()
            {
                return this.Format(this.Name);
            }
            
            [Obfuscation(Exclude = false)]
            public string Format(string val)
            {
                if (val == null)
                    val = string.Empty;
                else if (this.table_.sort_ == this)
                    val = "{color:brightwhite}" + val + "{color:reset}";

                string str = String.PrintLine(' ', this.width_ - String.Length(val));

                switch (this.Alignment)
                {
                    case Align.Left:
                        str = str.Insert(0, val);
                        break;
                    case Align.Right:
                        str += val;
                        break;
                }
                return str + "  ";
            }
            
            [Obfuscation(Exclude = false)]
            public static Column[] ToArray(params string[] names)
            {
                List<Column> list = new List<Column>();
                foreach (string name in names)
                    list.Add(new Column(null, name));
                return list.ToArray();
            }
        }

        [TypeConverter(typeof(ExpandableObjectConverter))]
        [Obfuscation(Exclude = true)]
        public class Row : IComparable
        {
            [Obfuscation(Exclude = false)] StringTable table_;
            [Obfuscation(Exclude = false)] Dictionary<Column, string> values_ = new Dictionary<Column, string>();

            public Row(StringTable table, params string[] values)
            {
                this.table_ = table;
                for (int x = 0; x < values.Length; ++x)
                {
                    string str = values[x];
                    if (str == null)
                    {
                        str = string.Empty;
                    }
                    Column col = table.Columns[x];
                    this.values_.Add(col, str);
                    col.Width = Math.Max(String.Length(str), col.Width);
                }
            }

            [Obfuscation(Exclude = false)]
            public int CompareTo(object o)
            {
                if (this.table_.sort_ == null)
                {
                    return 0;
                }
                Row row = o as Row;
                return this.values_[this.table_.sort_].CompareTo(row.values_[this.table_.sort_]);
            }
            
            [Obfuscation(Exclude = false)]
            public override string ToString()
            {
                string str = string.Empty;
                foreach (Column column in this.table_.Columns)
                {
                    string val = null;
                    if (this.values_.TryGetValue(column, out val))
                        str += column.Format(val);
                }
                return str;
            }
        }

        [Obfuscation(Exclude = false)] List<Column> columns_ = new List<Column>(); 
        [Obfuscation(Exclude = false)] List<Row> rows_ = new List<Row>();
        [Obfuscation(Exclude = false)] Column sort_;

        #region Properties
        public List<Column> Columns
        {
            get { return this.columns_; }
        }
        public List<Row> Rows
        {
            get { return this.rows_; }
        }
        #endregion

        public StringTable(params string[] columns)
        {
            foreach (string str in columns)
            {
                this.AddColumn(str);
            }
        }
        public StringTable(params Column[] columns)
        {
            foreach (Column column in columns)
            {
                Column col = new Column(this, column);
                this.columns_.Add(col);
            }
        }
        public void AddColumn(string str)
        {
            Column col = new Column(this, str);
            lock (this)
            {
                this.columns_.Add(col);
                if (this.sort_ == null)
                    this.Sort(col);
            }
        }
        public void AddRow(params string[] values)
        {
            try
            {
                lock (this)
                    this.rows_.Add(new Row(this, values));
            }
            catch
            {
            }
        }
        public void Sort(Column column)
        {
            this.sort_ = column;
            this.rows_.Sort();
        }
        public void Sort(string column)
        {
            this.Sort(this.GetColumn(column));
        }
        public Column GetColumn(string name)
        {
            foreach (Column column in this.Columns)
                if (!string.IsNullOrEmpty(column.Name) && column.Name.StartsWith(name, StringComparison.CurrentCultureIgnoreCase))
                    return column;

            return null;
        }
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            string header = string.Empty;
            lock (this)
            {
                foreach (Column col in this.Columns)
                    header += col.ToString();

                builder.AppendLine(header);
                builder.AppendLine(String.PrintLine('-', String.Length(header) - 2));

                foreach (Row row in this.rows_)
                {
                    try
                    {
                        builder.AppendLine(row.ToString());
                    }
                    catch
                    {
                    }
                }
            }

            return builder.ToString().Trim('\r', '\n');
        }
    }
}

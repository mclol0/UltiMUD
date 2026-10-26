using System;
using System.Collections.Generic;
using System.Reflection;
using System.ComponentModel;

namespace Ultimud
{
    [Serializable]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    [Obfuscation(Exclude = true)]
    public abstract class Formatter
    {
        List<StringTable.Column> columns_ = new List<StringTable.Column>();
        List<String.StringRef> fields_ = new List<String.StringRef>();

        #region Properties
        [Description("The columns displayed in this table.")]
        public List<StringTable.Column> Columns
        {
            get { return this.columns_; }
            set { this.columns_ = value; }
        }

        [Description("The fields displayed in this table's rows.")]
        public List<String.StringRef> Fields
        {
            get { return this.fields_; }
            set { this.fields_ = value; }
        }
        #endregion

        public abstract string ToString(string sortColumn, object o);
        public abstract string[] FormatFields(object o);

        [Obfuscation(Exclude = false)]
        public override string ToString()
        {
            return null;
        }
    }
}

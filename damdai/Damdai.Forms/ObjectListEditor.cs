using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using Damdai;

namespace Damdai.Forms
{
    public partial class ObjectListEditor<T> : UserControl where T: class, new()
    {
        List<T> addedObjects_ = new List<T>();
        List<T> removedObjects_ = new List<T>();
        List<T> modifiedObjects_ = new List<T>();

        public ICollection<object> Objects
        {
            get
            {
                List<object> list = new List<object>();
                foreach (object o in this.toolStripComboBox1.Items)
                {
                    list.Add(o);
                }
                return list;
            }

            set
            {
                this.toolStripComboBox1.Items.Clear();
                this.propertyGrid1.SelectedObject = null;
                if (value != null)
                {
                    foreach (object o in value)
                    {
                        this.toolStripComboBox1.Items.Add(o);
                    }
                }
            }
        }
        public List<T> AddedObjects
        {
            get { return this.addedObjects_; }
        }
        public List<T> RemovedObjects
        {
            get { return this.removedObjects_; }
        }
        public List<T> ModifiedObjects
        {
            get { return this.modifiedObjects_; }
        }

        public ObjectListEditor(ICollection<object> objects, Bitmap newObjectIcon, Dictionary<string, Function_void<object>> buttons, params Type[] types)
        {
            InitializeComponent();
            if (newObjectIcon != null)
                this.imageList1.Images.Add(newObjectIcon);
            this.toolStripButton1.Image = newObjectIcon;
            this.toolStripButton1.Tag = typeof(T);
            this.Objects = objects;

            foreach (Type type in types)
            {
                ToolStripButton button = new ToolStripButton(null, newObjectIcon, new EventHandler(this.toolStripButton1_Click));
                button.DisplayStyle = ToolStripItemDisplayStyle.Image;
                button.Tag = type;
                button.Text = "New " + type.Name;
                this.toolStrip1.Items.Insert(1, button);
            }

            if (buttons != null && buttons.Count > 0)
            {
                this.toolStrip1.Items.Add(new ToolStripSeparator());
                foreach (KeyValuePair<string, Function_void<object>> pair in buttons)
                {
                    ToolStripButton button = new ToolStripButton(pair.Key, null, new EventHandler(this.customButton_Click));
                    button.Tag = pair.Value;
                    button.DisplayStyle = ToolStripItemDisplayStyle.Text;
                    this.toolStrip1.Items.Add(button);
                }
            }
        }
        public ObjectListEditor(Bitmap newObjectIcon, params Type[] types)
            : this(null, newObjectIcon, null, types)
        {
        }

        //New object.
        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            this.CreateNewObject(((ToolStripButton)sender).Tag as Type);
        }
        //Delete object.
        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            this.DeleteObject();
        }
        private void customButton_Click(object sender, EventArgs e)
        {
            Function_void<object> fn = (Function_void<object>)((ToolStripItem)sender).Tag;
            if (fn != null && this.propertyGrid1.SelectedObject != null)
                fn(this.propertyGrid1.SelectedObject);
        }
        public virtual void CreateNewObject(Type type)
        {
            System.Diagnostics.Trace.Assert(type != null, "Type is null!");
            System.Diagnostics.Debug.WriteLine("Creating new object: " + type.Name);
            object o = Activator.CreateInstance(type);
            //object o = type.GetConstructor(BindingFlags.Static | BindingFlags.NonPublic, null, Type.EmptyTypes, null).Invoke(null);
            //T t = new T();
            this.toolStripComboBox1.Items.Add(o);
            this.toolStripComboBox1.SelectedItem = o;
            this.addedObjects_.Add(o as T);
        }
        void DeleteObject()
        {
            object o = this.toolStripComboBox1.SelectedItem;
            if (o != null)
            {
                T t = o as T;
                this.modifiedObjects_.Remove(t);
                if (this.AddedObjects.Contains(t))
                    this.AddedObjects.Remove(t);
                else
                    this.removedObjects_.Add(t);
                this.toolStripComboBox1.Items.Remove(o);
                this.propertyGrid1.SelectedObject = null;
            }
        }
        private void toolStripComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.propertyGrid1.SelectedObject = this.toolStripComboBox1.SelectedItem;
            T t = this.propertyGrid1.SelectedObject as T;
            if (t != null && !this.modifiedObjects_.Contains(t))
                this.modifiedObjects_.Add(t);
        }
        private void propertyGrid1_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            //T t = this.propertyGrid1.SelectedObject as T;
            //if (!this.modifiedObjects_.Contains(t))
            //    this.modifiedObjects_.Add(t);
        }
    }
}

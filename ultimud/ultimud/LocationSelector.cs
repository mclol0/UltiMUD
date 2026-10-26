using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Design;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using System.Reflection;

namespace Ultimud
{
    [Obfuscation(Exclude = false)]
    internal partial class LocationSelector : Ultimud.AreaCanvas
    {
        public LocationSelector()
        {
            InitializeComponent();
            this.LocationClickedEvent += this.SetLocation;
            this.toolStripComboBox1.Items.Add("Nowhere");
            this.Mode = EditModes.Manipulate;
        }

        void SetLocation(Point p)
        {
            this.Mark = p;
            this.Invalidate();
        }

        public void SetLocation(Area area, Point p)
        {
            if (area != null)
            {
                this.SetArea(area);
                this.GoTo(p);
                this.SetLocation(p);
            }
        }

        public class Editor : UITypeEditor
        {
            public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
            {
                return UITypeEditorEditStyle.DropDown;
            }

            public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
            {
                Location location = (Location)value;
                LocationSelector selector = new LocationSelector();
                if (location != null)
                    selector.SetLocation(location.Area, location.Point);
                IWindowsFormsEditorService service = provider.GetService(typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;
                service.DropDownControl(selector);
                return new Location(selector.Area, selector.Mark);
            }
        }
    }
}


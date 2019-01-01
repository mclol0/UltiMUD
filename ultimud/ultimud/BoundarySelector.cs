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
    internal partial class BoundarySelector : Ultimud.AreaCanvas
    {
        public Area.Region Boundary
        {
            get { return this.SelectedObject as Area.Region; }
            set { this.SelectedObject = value; }
        }

        new public object SelectedObject
        {
            get
            {
                if (base.SelectedObject == null)
                    base.SelectedObject = new Area.Region();
                return base.SelectedObject;
            }
            set { base.SelectedObject = value; }
        }

        public BoundarySelector()
        {
            InitializeComponent();
            this.SelectedObject = new Area.Region();
            this.Mode = EditModes.Manipulate;
            this.Editable = true;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (this.SelectedObject == null)
                this.SelectedObject = new Area.Region();
            base.OnMouseDown(e);
        }
        internal class Editor : UITypeEditor
        {
            public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
            {
                return UITypeEditorEditStyle.DropDown;
            }
            public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
            {
                NpcReset reset = value as NpcReset;
                Area.Region boundary = reset.Boundary;
                BoundarySelector selector = new BoundarySelector();
                selector.SetArea(reset.Location.Area);
                selector.Mark = reset.Location.Point;
                if (boundary != null)
                {
                    selector.Boundary = boundary;
                    selector.GoTo(boundary.Center);
                }
                else
                    selector.GoTo(reset.Location.Point);

                IWindowsFormsEditorService service = provider.GetService(typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;
                service.DropDownControl(selector);
                reset.Boundary = selector.Boundary;
                return null;
            }
            public override bool IsDropDownResizable
            {
                get { return true; }
            }
        }
    }
}


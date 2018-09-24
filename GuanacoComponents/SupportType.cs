using Grasshopper.Kernel.Special;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Guanaco.Components
{
    public class CheckboxAttributes : GH_ValueListAttributes
    {
        public SupportType ownerOfThisAttribute;

        public List<ToolStripMenuItem> collectionToolStripMenuItems;

        public CheckboxAttributes(SupportType owner) : base(owner)
        {
            this.ownerOfThisAttribute = owner;

            this.collectionToolStripMenuItems = new List<ToolStripMenuItem>();
        }
    }

    public class SupportType : GuanacoSupportCheckBox
    {
        public GH_ValueListAttributes instanceAttributes;

        private readonly Guid ID = new Guid("{014ce6ed-8cb5-4bf0-9b4d-60d6597e010a}");

        public SupportType()
        {
            this.ListMode = Grasshopper.Kernel.Special.GH_ValueListMode.CheckList;
            this.Description = "Support Type. FP is physically fixed support (all nodes of an element have fixed translation).";
            this.Name = "SupportType";
            this.Category = "Guanaco";
            this.SubCategory = "BoundaryConditions";
            this.NickName = "SupportType";
            this.SolutionExpired += new Grasshopper.Kernel.IGH_DocumentObject.SolutionExpiredEventHandler(this.ValueMenuItem_Click);

            this.ListItems.Clear();
            this.ListItems.Add(new GH_ValueListItem("DX", "1"));
            this.ListItems.Add(new GH_ValueListItem("DY", "2"));
            this.ListItems.Add(new GH_ValueListItem("DZ", "3"));
            this.ListItems.Add(new GH_ValueListItem("RX", "4"));
            this.ListItems.Add(new GH_ValueListItem("RY", "5"));
            this.ListItems.Add(new GH_ValueListItem("RZ", "6"));
            this.ListItems.Add(new GH_ValueListItem("FP", "100"));
        }

        public override void AppendAdditionalMenuItems(ToolStripDropDown menu)
        {
        }

        public override Guid ComponentGuid
        {
            get
            {
                return ID;
            }
        }

        public override void CreateAttributes()
        {
            CheckboxAttributes instanceAttributes = new CheckboxAttributes(this);

            this.instanceAttributes = instanceAttributes;
            base.m_attributes = this.instanceAttributes;
        }

        bool pf = false;
        private void ValueMenuItem_Click(object sender, EventArgs e)
        {
            if (pf)
            {
                base.ListItems[6].Selected = false;
                pf = false;
            }
            if (base.ListItems[6].Selected)
            {
                int i = 0;
                while (i < 6)
                {
                    base.ListItems[i].Selected = false;
                    i++;
                    pf = true;
                }
            }
        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Icons.DropDown;
            }
        }
    }
}
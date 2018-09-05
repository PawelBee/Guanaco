using Grasshopper.Kernel.Special;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Guanaco.Components
{
    public class Element2DShape : GuanacoList
    {
        public new List<GuanacoListItem> ListItems;

        public GuanacoListAttributes instanceAttributes;

        private readonly Guid ID = new Guid("{8ad66920-8503-4d19-a231-647fb12353fc}");

        public Element2DShape() : base()
        {

            this.ListMode = GH_ValueListMode.DropDown;
            this.Description = "Element shape.";
            this.Name = "Element2DShape";
            this.Category = "Guanaco";
            this.SubCategory = "Mesh";
            this.NickName = "Element2DShape";

            base.ListItems.Clear();

            this.ListItems = new List<GuanacoListItem>();

            base.ListItems.Add(new GuanacoListItem("Tri", Element2D.Element2DShape.Tri));
            base.ListItems.Add(new GuanacoListItem("Quad", Element2D.Element2DShape.Quad));
            this.SelectItem(0);
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
            GuanacoListAttributes instanceAttributes = new GuanacoListAttributes(this);

            foreach (GuanacoListItem item in base.ListItems)
            {
                ToolStripMenuItem ToolStripItem = new ToolStripMenuItem(item.Name);
                ToolStripItem.Click += new EventHandler(this.ValueMenuItem_Click);
                ToolStripItem.MouseLeave += new EventHandler(MouseLeave);
                ToolStripItem.MouseEnter += new EventHandler(MouseEnter);

                instanceAttributes.collectionToolStripMenuItems.Add(ToolStripItem);
            }
            instanceAttributes.collectionToolStripMenuItems[0].Checked = true;

            this.instanceAttributes = instanceAttributes;

            base.m_attributes = this.instanceAttributes;
        }

        private void ValueMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem item2 = sender as ToolStripMenuItem;
            ToolStripDropDownMenu menu = item2.Owner as ToolStripDropDownMenu;
            foreach (ToolStripMenuItem it in menu.Items)
            {
                it.Checked = false;
            }
            for (int i = 0; i < menu.Items.Count; i++)
            {
                if (menu.Items[i] == item2)
                {
                    item2.Checked = true;
                    this.SelectItem(i);
                    menu.Hide();
                    this.ExpireSolution(true);
                    return;
                }
            }
        }

        private void MouseLeave(object sender, EventArgs e)
        {
        }

        private void MouseEnter(object sender, EventArgs e)
        {
            ToolStripMenuItem item2 = sender as ToolStripMenuItem;
            ToolStripDropDownMenu menu = item2.Owner as ToolStripDropDownMenu;
            menu.Show();
        }
    }
}
using Grasshopper.Kernel.Special;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Guanaco.Components
{
    public class FE2DType : GuanacoList
    {
        public new List<GuanacoListItem> ListItems;

        public GuanacoListAttributes instanceAttributes;

        private readonly Guid ID = new Guid("{1359b971-2c35-4944-9ada-3ebbdf71532f}");

        public FE2DType() : base()
        {

            this.ListMode = GH_ValueListMode.DropDown;
            this.Description = "Finite element 2D type.";
            this.Name = "FE2DType";
            this.Category = "Guanaco";
            this.SubCategory = "Mesh";
            this.NickName = "FE2DType";

            base.ListItems.Clear();

            this.ListItems = new List<GuanacoListItem>();

            base.ListItems.Add(new GuanacoListItem("Shell", Element2D.Element2DType.Shell));
            base.ListItems.Add(new GuanacoListItem("Membrane", Element2D.Element2DType.Membrane));
            base.ListItems.Add(new GuanacoListItem("PlaneStress", Element2D.Element2DType.PlaneStress));
            base.ListItems.Add(new GuanacoListItem("PlaneStrain", Element2D.Element2DType.PlaneStrain));
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
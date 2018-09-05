using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Grasshopper.Kernel.Special;
using Guanaco;

namespace Guanaco.Components
{
    public class ResultType1D : GuanacoList
    {
        public new List<GuanacoListItem> ListItems;

        public GuanacoListAttributes instanceAttributes;

        private readonly Guid ID = new Guid("{e912e9f0-da51-4624-85df-64689b53e1f5}");

        public ResultType1D() : base()
        {

            this.ListMode = GH_ValueListMode.DropDown;
            this.Description = "Result type to be shown for 1-dimensional elements (bars).";
            this.Name = "1DResultType";
            this.Category = "Guanaco";
            this.SubCategory = "Results";
            this.NickName = "1DResultType";

            base.ListItems.Clear();

            this.ListItems = new List<GuanacoListItem>();
            for (int i = 0; i < ResultUtil.Results1D.Length; i++)
            {
                Type t = ResultUtil.Results1D[i];
                Array a = Enum.GetValues(t);
                for (int j = i == 0 ? 0 : 1; j < a.Length; j++)
                {
                    object v = a.GetValue(j);
                    base.ListItems.Add(new GuanacoListItem(v.ToString(), (ResultUtil.SectionForces1D)v));
                }
            }
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
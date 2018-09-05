using GH_IO.Serialization;
using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Special;
using Grasshopper.Kernel.Types;
using Rhino;
using Rhino.DocObjects;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Newtonsoft.Json;
using Grasshopper.Kernel;

namespace Guanaco
{
    /***************************************************/
    /****         Grasshopper utility class         ****/
    /***************************************************/

    public static class GHUtil
    {
        /***************************************************/

        // Determine the path to Calculix for given GH document.
        public static string CCXPath(GH_Document GrasshopperDocument)
        {
            string CCXPath = string.Empty;
            Grasshopper.Kernel.Expressions.GH_Variant var = new Grasshopper.Kernel.Expressions.GH_Variant();
            GrasshopperDocument.ConstantServer.TryGetValue("CCXPath", out var);
            if (var != null) CCXPath = var._String;
            if (CCXPath == string.Empty || CCXPath == null || CCXPath == "") CCXPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\AppData\\Roaming\\Grasshopper\\Libraries\\Guanaco\\CCX\\CCX.exe";
            return CCXPath;
        }

        /***************************************************/

        // Determine the path to Gmsh for given GH document.
        public static string GmshPath(GH_Document GrasshopperDocument)
        {
            string GmshPath = string.Empty;
            Grasshopper.Kernel.Expressions.GH_Variant var = new Grasshopper.Kernel.Expressions.GH_Variant();
            GrasshopperDocument.ConstantServer.TryGetValue("GmshPath", out var);
            if (var != null) GmshPath = var._String;
            if (GmshPath == string.Empty || GmshPath == null || GmshPath == "") GmshPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\AppData\\Roaming\\Grasshopper\\Libraries\\Guanaco\\Gmsh\\gmsh.exe";
            return GmshPath;
        }

        /***************************************************/
    }


    /***************************************************/
    /****         Guanaco Grasshopper list          ****/
    /***************************************************/

    public class GuanacoList : GH_Param<IGH_Goo>, IGH_PreviewObject, IGH_BakeAwareObject, IGH_StateAwareObject
    {
        private GH_ValueListMode m_listMode;

        private readonly List<GuanacoListItem> m_userItems;

        private bool m_hidden;

        protected override IGH_Goo InstantiateT()
        {
            return new GH_ObjectWrapper();
        }

        public override GH_Exposure Exposure
        {
            get
            {
                return GH_Exposure.secondary;
            }
        }

        public override System.Guid ComponentGuid
        {
            get
            {
                Guid result = new System.Guid("{1997a0df-aa76-4cf7-9f8f-b09b84672fe8}");
                return result;
            }
        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return null;
            }
        }

        public string DisplayName
        {
            get
            {
                if (string.IsNullOrWhiteSpace(this.NickName))
                {
                    return null;
                }
                if (this.NickName.Equals("List", System.StringComparison.OrdinalIgnoreCase))
                {
                    return null;
                }
                return this.NickName;
            }
        }

        public GH_ValueListMode ListMode
        {
            get
            {
                return this.m_listMode;
            }
            set
            {
                this.m_listMode = value;
                if (this.m_attributes != null)
                {
                    this.m_attributes.ExpireLayout();
                }
            }
        }

        public List<GuanacoListItem> ListItems
        {
            get
            {
                return this.m_userItems;
            }
        }

        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never), System.Obsolete("This property has been replaced by ListItems")]
        public List<GuanacoListItem> Values
        {
            get
            {
                return this.m_userItems;
            }
        }

        public List<GuanacoListItem> SelectedItems
        {
            get
            {
                List<GuanacoListItem> items = new List<GuanacoListItem>();
                if (this.m_userItems.Count == 0)
                {
                    return items;
                }
                GH_ValueListMode listMode = this.ListMode;
                if (listMode == GH_ValueListMode.CheckList)
                {
                    List<GuanacoListItem>.Enumerator enumerator = new List<GuanacoListItem>.Enumerator();
                    try
                    {
                        enumerator = this.m_userItems.GetEnumerator();
                        while (enumerator.MoveNext())
                        {
                            GuanacoListItem item = enumerator.Current;
                            if (item.Selected)
                            {
                                items.Add(item);
                            }
                        }
                        return items;
                    }
                    finally
                    {
                        enumerator.Dispose();
                    }
                }
                List<GuanacoListItem>.Enumerator enumerator2 = new List<GuanacoListItem>.Enumerator();
                try
                {
                    enumerator2 = this.m_userItems.GetEnumerator();
                    while (enumerator2.MoveNext())
                    {
                        GuanacoListItem item2 = enumerator2.Current;
                        if (item2.Selected)
                        {
                            items.Add(item2);
                            return items;
                        }
                    }
                }
                finally
                {
                    enumerator2.Dispose();
                }
                this.m_userItems[0].Selected = true;
                items.Add(this.m_userItems[0]);
                return items;
            }
        }

        public GuanacoListItem FirstSelectedItem
        {
            get
            {
                if (this.m_userItems.Count == 0)
                {
                    return null;
                }
                List<GuanacoListItem>.Enumerator enumerator = new List<GuanacoListItem>.Enumerator();
                try
                {
                    enumerator = this.m_userItems.GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        GuanacoListItem item = enumerator.Current;
                        if (item.Selected)
                        {
                            return item;
                        }
                    }
                }
                finally
                {
                    enumerator.Dispose();
                }
                return this.m_userItems[0];
            }
        }

        public bool Hidden
        {
            get
            {
                return this.m_hidden;
            }
            set
            {
                this.m_hidden = value;
            }
        }

        public bool IsPreviewCapable
        {
            get
            {
                return true;
            }
        }

        public BoundingBox ClippingBox
        {
            get
            {
                return this.Preview_ComputeClippingBox();
            }
        }

        public bool IsBakeCapable
        {
            get
            {
                return !this.m_data.IsEmpty;
            }
        }

        public GuanacoList() : base(new GH_InstanceDescription("Value List", "List", "Provides a list of preset values to choose from", "Params", "Input"))
        {
            this.m_listMode = GH_ValueListMode.DropDown;
            this.m_userItems = new List<GuanacoListItem>();
            this.m_hidden = false;
        }

        public override void CreateAttributes()
        {
            this.m_attributes = new GuanacoListAttributes(this);
        }

        public void ToggleItem(int index)
        {
            if (index < 0)
            {
                return;
            }
            if (index >= this.m_userItems.Count)
            {
                return;
            }
            this.RecordUndoEvent("Toggle: " + this.m_userItems[index].Name);
            this.m_userItems[index].Selected = !this.m_userItems[index].Selected;
            this.ExpireSolution(true);
        }

        public void SelectItem(int index)
        {
            if (index < 0)
            {
                return;
            }
            if (index >= this.m_userItems.Count)
            {
                return;
            }
            bool modify = false;
            int arg_25_0 = 0;
            int num = this.m_userItems.Count - 1;
            for (int i = arg_25_0; i <= num; i++)
            {
                if (i == index)
                {
                    if (!this.m_userItems[i].Selected)
                    {
                        modify = true;
                        break;
                    }
                }
                else if (this.m_userItems[i].Selected)
                {
                    modify = true;
                    break;
                }
            }
            if (!modify)
            {
                return;
            }
            this.RecordUndoEvent("Select: " + this.m_userItems[index].Name);
            int arg_98_0 = 0;
            int num2 = this.m_userItems.Count - 1;
            for (int j = arg_98_0; j <= num2; j++)
            {
                this.m_userItems[j].Selected = (j == index);
            }
            this.ExpireSolution(true);
        }

        protected override void CollectVolatileData_Custom()
        {
            List<GuanacoListItem>.Enumerator enumerator = new List<GuanacoListItem>.Enumerator();
            this.m_data.Clear();
            try
            {
                enumerator = this.SelectedItems.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    GuanacoListItem item = enumerator.Current;
                    this.m_data.Append(item.Value, new GH_Path(0));
                }
            }
            finally
            {
                enumerator.Dispose();
            }
        }

        public void DrawViewportMeshes(IGH_PreviewArgs args)
        {
            this.Preview_DrawMeshes(args);
        }

        public void DrawViewportWires(IGH_PreviewArgs args)
        {
            this.Preview_DrawWires(args);
        }

        public void BakeGeometry(RhinoDoc doc, List<System.Guid> obj_ids)
        {
            this.BakeGeometry(doc, null, obj_ids);
        }

        public void BakeGeometry(RhinoDoc doc, ObjectAttributes att, List<System.Guid> obj_ids)
        {
            System.Collections.IEnumerator enumerator = new List<GuanacoListItem>.Enumerator();
            if (att == null)
            {
                att = doc.CreateDefaultAttributes();
            }
            try
            {
                enumerator = this.m_data.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    object item = System.Runtime.CompilerServices.RuntimeHelpers.GetObjectValue(enumerator.Current);
                    if (item != null)
                    {
                        if (item is IGH_BakeAwareData)
                        {
                            IGH_BakeAwareData bake_item = (IGH_BakeAwareData)item;
                            System.Guid id;
                            if (bake_item.BakeGeometry(doc, att, out id))
                            {
                                obj_ids.Add(id);
                            }
                        }
                    }
                }
            }
            finally
            {
                ((System.IDisposable)enumerator).Dispose();
            }
        }

        public void LoadState(string state)
        {
            System.Collections.IEnumerator enumerator = new List<GuanacoListItem>.Enumerator();
            try
            {
                enumerator = this.m_userItems.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    GuanacoListItem item = enumerator.Current as GuanacoListItem;
                    item.Selected = false;
                }
            }
            finally
            {
                ((System.IDisposable)enumerator).Dispose();
            }
            int index;
            if (int.TryParse(state, out index))
            {
                if (index >= 0 && index < this.m_userItems.Count)
                {
                    this.m_userItems[index].Selected = true;
                }
            }
            else
            {
                int arg_81_0 = 0;
                int num = System.Math.Min(state.Length, this.m_userItems.Count) - 1;
                for (int i = arg_81_0; i <= num; i++)
                {
                    this.m_userItems[i].Selected = state[i].Equals('Y');
                }
            }
        }

        public string SaveState()
        {
            System.Text.StringBuilder state = new System.Text.StringBuilder(this.m_userItems.Count);
            List<GuanacoListItem>.Enumerator enumerator = new List<GuanacoListItem>.Enumerator();
            try
            {
                enumerator = this.m_userItems.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    GuanacoListItem item = enumerator.Current;
                    if (item.Selected)
                    {
                        state.Append('Y');
                    }
                    else
                    {
                        state.Append('N');
                    }
                }
            }
            finally
            {
                enumerator.Dispose();
            }
            return state.ToString();
        }

        public override bool Write(GH_IWriter writer)
        {
            writer.SetInt32("ListMode", (int)this.ListMode);
            writer.SetInt32("ListCount", this.m_userItems.Count);
            int arg_36_0 = 0;
            int num = this.m_userItems.Count - 1;
            for (int i = arg_36_0; i <= num; i++)
            {
                GH_IWriter chunk = writer.CreateChunk("ListItem", i);
                chunk.SetString("Name", this.m_userItems[i].Name);
                string serializedExpression = JsonConvert.SerializeObject(this.m_userItems[i].Expression);
                string serializedType = JsonConvert.SerializeObject(this.m_userItems[i].Expression.GetType(), typeof(Type), new JsonSerializerSettings());
                chunk.SetString("Expression", serializedExpression);
                chunk.SetString("Type", serializedType);
                chunk.SetBoolean("Selected", this.m_userItems[i].Selected);
            }
            return base.Write(writer);
        }

        public override bool Read(GH_IReader reader)
        {
            int mode = 1;
            reader.TryGetInt32("UIMode", ref mode);
            reader.TryGetInt32("ListMode", ref mode);
            this.ListMode = (GH_ValueListMode)mode;
            int count = reader.GetInt32("ListCount");
            int cache = 0;
            reader.TryGetInt32("CacheCount", ref cache);
            this.m_userItems.Clear();
            int arg_52_0 = 0;
            int num = count - 1;
            for (int i = arg_52_0; i <= num; i++)
            {
                GH_IReader chunk = reader.FindChunk("ListItem", i);
                if (chunk == null)
                {
                    reader.AddMessage("Missing chunk for List Value: " + i.ToString(), GH_Message_Type.error);
                }
                else
                {
                    string itemName = chunk.GetString("Name");
                    string itemExpression = chunk.GetString("Expression");
                    bool itemSelected = false;
                    chunk.TryGetBoolean("Selected", ref itemSelected);
                    Type t = JsonConvert.DeserializeObject<Type>(chunk.GetString("Type"));
                    object deserializedExpression = JsonConvert.DeserializeObject(itemExpression, t);
                    GuanacoListItem item = new GuanacoListItem(itemName, deserializedExpression);
                    item.Selected = itemSelected;
                    this.m_userItems.Add(item);
                }
            }
            if (reader.ItemExists("ListIndex"))
            {
                int idx = reader.GetInt32("ListIndex");
                if (idx >= 0 && idx < this.m_userItems.Count)
                {
                    this.m_userItems[idx].Selected = true;
                }
            }
            return base.Read(reader);
        }
    }


    /***************************************************/
    /****    Guanaco Grasshopper list attributes    ****/
    /***************************************************/

    public class GuanacoListAttributes : GH_Attributes<GuanacoList>
    {
        private RectangleF _NameBounds;

        private RectangleF _ItemBounds;

        public GuanacoList ownerOfThisAttribute;

        public List<ToolStripMenuItem> collectionToolStripMenuItems;

        public GuanacoListAttributes(GuanacoList owner) : base(owner)
        {
            this.ownerOfThisAttribute = owner;

            this.collectionToolStripMenuItems = new List<ToolStripMenuItem>();
        }

        public override bool AllowMessageBalloon
        {
            get
            {
                return false;
            }
        }

        public override bool HasInputGrip
        {
            get
            {
                return false;
            }
        }

        public override bool HasOutputGrip
        {
            get
            {
                return true;
            }
        }

        private RectangleF NameBounds
        {
            get { return this._NameBounds; }
            set { this._NameBounds = value; }
        }

        private RectangleF ItemBounds
        {
            get { return this._ItemBounds; }
            set { this._ItemBounds = value; }
        }

        private int ItemMaximumWidth()
        {
            int num2 = 20;
            foreach (GuanacoListItem item in this.Owner.ListItems)
            {
                int num3 = GH_FontServer.StringWidth(item.Name, GH_FontServer.Standard);
                num2 = Math.Max(num2, num3);
            }
            return (num2 + 10);
        }

        public override GH_ObjectResponse RespondToMouseDoubleClick(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
            return GH_ObjectResponse.Ignore;
        }

        public override GH_ObjectResponse RespondToMouseDown(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
            if (e.Button == MouseButtons.Left)
            {
                switch (this.Owner.ListMode)
                {
                    case GH_ValueListMode.CheckList:
                        {
                            int arg_4A_0 = 0;
                            int num = this.Owner.ListItems.Count - 1;
                            for (int i = arg_4A_0; i <= num; i++)
                            {
                                GuanacoListItem item = this.Owner.ListItems[i];
                                if (item.BoxLeft.Contains(e.CanvasLocation))
                                {
                                    this.Owner.ToggleItem(i);
                                    return GH_ObjectResponse.Handled;
                                }
                            }
                            break;
                        }
                    case GH_ValueListMode.DropDown:
                        {
                            GuanacoListItem item2 = this.Owner.FirstSelectedItem;
                            if (item2 != null)
                            {
                                if (item2.BoxRight.Contains(e.CanvasLocation))
                                {
                                    List<GuanacoListItem>.Enumerator enumerator;
                                    ToolStripDropDownMenu menu = new ToolStripDropDownMenu();
                                    GuanacoListItem activeItem = this.Owner.FirstSelectedItem;
                                    enumerator = this.Owner.ListItems.GetEnumerator();
                                    try
                                    {
                                        enumerator = this.Owner.ListItems.GetEnumerator();
                                        while (enumerator.MoveNext())
                                        {
                                            GuanacoListItem existingItem = enumerator.Current;
                                            ToolStripMenuItem menuItem = new ToolStripMenuItem(existingItem.Name);
                                            menuItem.Click += new EventHandler(this.ValueMenuItem_Click);
                                            if (existingItem == activeItem)
                                            {
                                                menuItem.Checked = true;
                                            }
                                            menuItem.Tag = existingItem;
                                            menu.Items.Add(menuItem);
                                        }
                                    }
                                    finally
                                    {
                                        enumerator.Dispose();
                                    }
                                    menu.Show(sender, e.ControlLocation);
                                    return GH_ObjectResponse.Handled;
                                }
                            }
                            break;
                        }
                }
            }
            return base.RespondToMouseDown(sender, e);
        }

        private void ValueMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem menuItem = (ToolStripMenuItem)sender;
            if (menuItem.Checked)
            {
                return;
            }
            GuanacoListItem item = menuItem.Tag as GuanacoListItem;
            if (item == null)
            {
                return;
            }
            this.Owner.SelectItem(this.Owner.ListItems.IndexOf(item));
        }

        protected override void Layout()
        {
            switch (this.Owner.ListMode)
            {
                case GH_ValueListMode.CheckList:
                    this.LayoutCheckList();
                    break;
                case GH_ValueListMode.DropDown:
                    this.LayoutDropDown();
                    break;
            }
            this.ItemBounds = this.Bounds;
            RectangleF bounds = this.Bounds;
            RectangleF bounds2 = new RectangleF(bounds.X, this.Bounds.Y, 0f, this.Bounds.Height);
            this.NameBounds = bounds2;
            if (this.Owner.DisplayName != null)
            {
                int nameWidth = GH_FontServer.StringWidth(this.Owner.DisplayName, GH_FontServer.Standard) + 10;
                bounds2 = this.Bounds;
                bounds = new RectangleF(bounds2.X - (float)nameWidth, this.Bounds.Y, (float)nameWidth, this.Bounds.Height);
                this.NameBounds = bounds;
                this.Bounds = RectangleF.Union(this.NameBounds, this.ItemBounds);
            }
        }

        private void LayoutDropDown()
        {
            List<GuanacoListItem>.Enumerator enumerator;
            int num2 = this.ItemMaximumWidth() + 0x16;
            int num = 0x16;
            this.Pivot = (PointF)GH_Convert.ToPoint(this.Pivot);
            RectangleF ef2 = new RectangleF(this.Pivot.X, this.Pivot.Y, (float)num2, (float)num);
            this.Bounds = ef2;
            GuanacoListItem firstSelectedItem = this.Owner.FirstSelectedItem;

            enumerator = this.Owner.ListItems.GetEnumerator();

            try
            {
                while (enumerator.MoveNext())
                {
                    GuanacoListItem current = enumerator.Current;

                    if (current == firstSelectedItem)
                    {
                        SetDropdownBounds(current, this.Bounds);
                    }
                    else
                    {
                        SetEmptyBounds(current, this.Bounds);
                    }
                }
            }
            finally
            {
                enumerator.Dispose();
            }
        }

        private void SetDropdownBounds(GuanacoListItem item, RectangleF bounds)
        {
            RectangleF rectangleF = new RectangleF(bounds.X, bounds.Y, 0f, bounds.Height);
            item.BoxLeft = rectangleF;
            rectangleF = new RectangleF(bounds.X, bounds.Y, bounds.Width - 22f, bounds.Height);
            item.BoxName = rectangleF;
            rectangleF = new RectangleF(bounds.Right - 22f, bounds.Y, 22f, bounds.Height);
            item.BoxRight = rectangleF;
        }

        private void SetEmptyBounds(GuanacoListItem item, RectangleF bounds)
        {
            RectangleF rectangleF = new RectangleF(bounds.X, bounds.Y, 0f, 0f);
            item.BoxLeft = rectangleF;
            rectangleF = new RectangleF(bounds.X, bounds.Y, 0f, 0f);
            item.BoxName = rectangleF;
            rectangleF = new RectangleF(bounds.X, bounds.Y, 0f, 0f);
            item.BoxRight = rectangleF;
        }

        private void RenderDropDown(GH_Canvas canvas, Graphics graphics, Color color)
        {
            GuanacoListItem firstSelectedItem = this.Owner.FirstSelectedItem;
            if (firstSelectedItem != null)
            {
                graphics.DrawString(this.Owner.SelectedItems[0].Name, GH_FontServer.Standard, Brushes.Black, firstSelectedItem.BoxName, GH_TextRenderingConstants.CenterCenter);
                RenderDownArrow(canvas, graphics, firstSelectedItem.BoxRight, color);
            }
        }

        private static void RenderDownArrow(GH_Canvas canvas, Graphics graphics, RectangleF bounds, Color color)
        {
            int num = Convert.ToInt32((float)(bounds.X + (0.5f * bounds.Width)));
            int num2 = Convert.ToInt32((float)(bounds.Y + (0.5f * bounds.Height)));
            PointF[] tfArray2 = new PointF[3];
            PointF tf = new PointF((float)num, (float)(num2 + 6));
            tfArray2[0] = tf;
            PointF tf2 = new PointF((float)(num + 6), (float)(num2 - 6));
            tfArray2[1] = tf2;
            PointF tf3 = new PointF((float)(num - 6), (float)(num2 - 6));
            tfArray2[2] = tf3;
            PointF[] points = tfArray2;
            RenderShape(canvas, graphics, points, color);
        }

        private void LayoutCheckList()
        {
            int width = this.ItemMaximumWidth() + 22;
            int height = 22 * Math.Max(1, this.Owner.ListItems.Count);
            this.Pivot = GH_Convert.ToPoint(this.Pivot);
            RectangleF bounds = new RectangleF(this.Pivot.X, this.Pivot.Y, (float)width, (float)height);
            this.Bounds = bounds;
            int arg_80_0 = 0;
            int num = this.Owner.ListItems.Count - 1;
            for (int i = arg_80_0; i <= num; i++)
            {
                float arg_B0_1 = this.Bounds.X;
                bounds = this.Bounds;
                RectangleF box = new RectangleF(arg_B0_1, bounds.Y + (float)(i * 22), (float)width, 22f);
                this.Owner.ListItems[i].SetCheckListBounds(box);
            }
        }

        protected override void Render(GH_Canvas canvas, Graphics graphics, GH_CanvasChannel channel)
        {
            if (channel == GH_CanvasChannel.Objects)
            {
                GH_Capsule capsule = GH_Capsule.CreateCapsule(this.Bounds, GH_Palette.Normal);
                capsule.AddOutputGrip(this.OutputGrip.Y);
                capsule.Render(canvas.Graphics, this.Selected, this.Owner.Locked, this.Owner.Hidden);
                capsule.Dispose();
                int alpha = GH_Canvas.ZoomFadeLow;
                if (alpha > 0)
                {
                    canvas.SetSmartTextRenderingHint();
                    GH_PaletteStyle style = GH_CapsuleRenderEngine.GetImpliedStyle(GH_Palette.Normal, this);
                    Color color = Color.FromArgb(alpha, style.Text);
                    if (this.NameBounds.Width > 0f)
                    {
                        SolidBrush nameFill = new SolidBrush(color);
                        graphics.DrawString(this.Owner.NickName, GH_FontServer.Standard, nameFill, this.NameBounds, GH_TextRenderingConstants.CenterCenter);
                        nameFill.Dispose();
                        int x = Convert.ToInt32(this.NameBounds.Right);
                        int y0 = Convert.ToInt32(this.NameBounds.Top);
                        int y = Convert.ToInt32(this.NameBounds.Bottom);
                        GH_GraphicsUtil.EtchFadingVertical(graphics, y0, y, x, Convert.ToInt32(0.8 * (double)alpha), Convert.ToInt32(0.3 * (double)alpha));
                    }
                    switch (this.Owner.ListMode)
                    {
                        case GH_ValueListMode.CheckList:
                            this.RenderCheckList(canvas, graphics, color);
                            break;
                        case GH_ValueListMode.DropDown:
                            this.RenderDropDown(canvas, graphics, color);
                            break;
                    }
                }
            }
        }

        private void RenderCheckList(GH_Canvas canvas, Graphics graphics, Color color)
        {
            List<GuanacoListItem>.Enumerator enumerator;
            if (this.Owner.ListItems.Count == 0)
            {
                return;
            }
            int y0 = Convert.ToInt32(this.Bounds.Y);
            int y = Convert.ToInt32(this.Bounds.Bottom);
            int x = Convert.ToInt32(this.Owner.ListItems[0].BoxLeft.Right);

            enumerator = this.Owner.ListItems.GetEnumerator();
            try
            {
                enumerator = this.Owner.ListItems.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    GuanacoListItem item = enumerator.Current;
                    graphics.DrawString(item.Name, GH_FontServer.Standard, Brushes.Black, item.BoxName, GH_TextRenderingConstants.CenterCenter);
                    if (item.Selected)
                    {
                        GuanacoListAttributes.RenderCheckMark(canvas, graphics, item.BoxLeft, color);
                    }
                }
            }
            finally
            {
                enumerator.Dispose();
            }
        }

        private static void RenderCheckMark(GH_Canvas canvas, Graphics graphics, RectangleF bounds, Color color)
        {
            int x = Convert.ToInt32(bounds.X + 0.5f * bounds.Width) - 2;
            int y = Convert.ToInt32(bounds.Y + 0.5f * bounds.Height);
            PointF[] array = new PointF[6];
            PointF[] arg_54_0_cp_0 = array;
            int arg_54_0_cp_1 = 0;
            PointF pointF = new PointF((float)x, (float)y);
            arg_54_0_cp_0[arg_54_0_cp_1] = pointF;
            PointF[] arg_7A_0_cp_0 = array;
            int arg_7A_0_cp_1 = 1;
            PointF pointF2 = new PointF((float)x - 3.5f, (float)y - 3.5f);
            arg_7A_0_cp_0[arg_7A_0_cp_1] = pointF2;
            PointF[] arg_A0_0_cp_0 = array;
            int arg_A0_0_cp_1 = 2;
            PointF pointF3 = new PointF((float)x - 6.5f, (float)y - 0.5f);
            arg_A0_0_cp_0[arg_A0_0_cp_1] = pointF3;
            PointF[] arg_C0_0_cp_0 = array;
            int arg_C0_0_cp_1 = 3;
            PointF pointF4 = new PointF((float)x, (float)y + 6f);
            arg_C0_0_cp_0[arg_C0_0_cp_1] = pointF4;
            PointF[] arg_E6_0_cp_0 = array;
            int arg_E6_0_cp_1 = 4;
            PointF pointF5 = new PointF((float)x + 9.5f, (float)y - 3.5f);
            arg_E6_0_cp_0[arg_E6_0_cp_1] = pointF5;
            PointF[] arg_10C_0_cp_0 = array;
            int arg_10C_0_cp_1 = 5;
            PointF pointF6 = new PointF((float)x + 6.5f, (float)y - 6.5f);
            arg_10C_0_cp_0[arg_10C_0_cp_1] = pointF6;
            PointF[] corners = array;
            GuanacoListAttributes.RenderShape(canvas, graphics, corners, color);
        }

        private static void RenderShape(GH_Canvas canvas, Graphics graphics, PointF[] points, Color color)
        {
            int alpha = GH_Canvas.ZoomFadeMedium;
            float x0 = points[0].X;
            float x = x0;
            float y0 = points[0].Y;
            float y = y0;
            int arg_32_0 = 1;
            int num = points.Length - 1;
            for (int i = arg_32_0; i <= num; i++)
            {
                x0 = Math.Min(x0, points[i].X);
                x = Math.Max(x, points[i].X);
                y0 = Math.Min(y0, points[i].Y);
                y = Math.Max(y, points[i].Y);
            }
            RectangleF bounds = RectangleF.FromLTRB(x0, y0, x, y);
            bounds.Inflate(1f, 1f);
            LinearGradientBrush fill = new LinearGradientBrush(bounds, color, GH_GraphicsUtil.OffsetColour(color, 50), LinearGradientMode.Vertical);
            fill.WrapMode = WrapMode.TileFlipXY;
            graphics.FillPolygon(fill, points);
            fill.Dispose();
            if (alpha > 0)
            {
                Color col0 = Color.FromArgb(Convert.ToInt32(0.5 * (double)alpha), Color.White);
                Color col = Color.FromArgb(0, Color.White);
                LinearGradientBrush highlightFill = new LinearGradientBrush(bounds, col0, col, LinearGradientMode.Vertical);
                highlightFill.WrapMode = WrapMode.TileFlipXY;
                Pen highlightEdge = new Pen(highlightFill, 3f);
                highlightEdge.LineJoin = LineJoin.Round;
                highlightEdge.CompoundArray = new float[]
                {
                    0f,
                    0.5f
                };
                graphics.DrawPolygon(highlightEdge, points);
                highlightFill.Dispose();
                highlightEdge.Dispose();
            }
            graphics.DrawPolygon(new Pen(color, 1f)
            {
                LineJoin = LineJoin.Round
            }, points);
        }
    }


    /***************************************************/
    /****      Guanaco Grasshopper list item        ****/
    /***************************************************/

    public class GuanacoListItem
    {
        private IGH_Goo m_value;

        [System.Runtime.CompilerServices.CompilerGenerated]
        private bool _Selected;

        [System.Runtime.CompilerServices.CompilerGenerated]
        private string _Name;

        [System.Runtime.CompilerServices.CompilerGenerated]
        private object _Expression;

        [System.Runtime.CompilerServices.CompilerGenerated]
        private System.Drawing.RectangleF _BoxName;

        [System.Runtime.CompilerServices.CompilerGenerated]
        private System.Drawing.RectangleF _BoxLeft;

        [System.Runtime.CompilerServices.CompilerGenerated]
        private System.Drawing.RectangleF _BoxRight;

        private const int BoxWidth = 22;

        public bool Selected
        {
            get
            {
                return this._Selected;
            }
            set
            {
                this._Selected = value;
            }
        }

        public string Name
        {
            get
            {
                return this._Name;
            }
            set
            {
                this._Name = value;
            }
        }

        public object Expression
        {
            get
            {
                return this._Expression;
            }
            set
            {
                this._Expression = value;
            }
        }

        public System.Drawing.RectangleF BoxName
        {
            get
            {
                return this._BoxName;
            }
            set
            {
                this._BoxName = value;
            }
        }

        public System.Drawing.RectangleF BoxLeft
        {
            get
            {
                return this._BoxLeft;
            }
            set
            {
                this._BoxLeft = value;
            }
        }

        public System.Drawing.RectangleF BoxRight
        {
            get
            {
                return this._BoxRight;
            }
            set
            {
                this._BoxRight = value;
            }
        }


        [System.ComponentModel.Browsable(false)]
        public IGH_Goo Value
        {
            get
            {
                if (this.m_value == null)
                {
                    this.m_value = new GH_ObjectWrapper(this.Expression);
                }
                return this.m_value;
            }
        }

        public bool IsVisible
        {
            get
            {
                return this.BoxName.Height > 0f;
            }
        }

        public GuanacoListItem()
        {
            this.Name = string.Empty;
            this.Expression = default(object);
        }

        public GuanacoListItem(string name, object expression)
        {
            this.Name = name;
            this.Expression = expression;
        }

        public void ExpireValue()
        {
            this.m_value = null;
        }

        internal void SetCheckListBounds(System.Drawing.RectangleF bounds)
        {
            System.Drawing.RectangleF rectangleF = new System.Drawing.RectangleF(bounds.X, bounds.Y, 22f, bounds.Height);
            this.BoxLeft = rectangleF;
            rectangleF = new System.Drawing.RectangleF(bounds.X + 22f, bounds.Y, bounds.Width - 22f, bounds.Height);
            this.BoxName = rectangleF;
            rectangleF = new System.Drawing.RectangleF(bounds.Right, bounds.Y, 0f, bounds.Height);
            this.BoxRight = rectangleF;
        }

        internal void SetDropdownBounds(System.Drawing.RectangleF bounds)
        {
            System.Drawing.RectangleF rectangleF = new System.Drawing.RectangleF(bounds.X, bounds.Y, 0f, bounds.Height);
            this.BoxLeft = rectangleF;
            rectangleF = new System.Drawing.RectangleF(bounds.X, bounds.Y, bounds.Width - 22f, bounds.Height);
            this.BoxName = rectangleF;
            rectangleF = new System.Drawing.RectangleF(bounds.Right - 22f, bounds.Y, 22f, bounds.Height);
            this.BoxRight = rectangleF;
        }

        internal void SetSequenceBounds(System.Drawing.RectangleF bounds)
        {
            System.Drawing.RectangleF rectangleF = new System.Drawing.RectangleF(bounds.X, bounds.Y, 22f, bounds.Height);
            this.BoxLeft = rectangleF;
            rectangleF = new System.Drawing.RectangleF(bounds.X + 22f, bounds.Y, bounds.Width - 44f, bounds.Height);
            this.BoxName = rectangleF;
            rectangleF = new System.Drawing.RectangleF(bounds.Right - 22f, bounds.Y, 22f, bounds.Height);
            this.BoxRight = rectangleF;
        }

        internal void SetEmptyBounds(System.Drawing.RectangleF bounds)
        {
            System.Drawing.RectangleF rectangleF = new System.Drawing.RectangleF(bounds.X, bounds.Y, 0f, 0f);
            this.BoxLeft = rectangleF;
            rectangleF = new System.Drawing.RectangleF(bounds.X, bounds.Y, 0f, 0f);
            this.BoxName = rectangleF;
            rectangleF = new System.Drawing.RectangleF(bounds.X, bounds.Y, 0f, 0f);
            this.BoxRight = rectangleF;
        }
    }


    /***************************************************/
    /****         Guanaco Support checkbox          ****/
    /***************************************************/

    public class GuanacoSupportCheckBox : GH_ValueList
    {
        public override System.Guid ComponentGuid
        {
            get
            {
                System.Guid result = new System.Guid("{e12a63d7-c349-44f7-8900-62534ff7a977}");
                return result;
            }
        }

        protected override void CollectVolatileData_Custom()
        {
            List<GH_ValueListItem>.Enumerator enumerator = new List<GH_ValueListItem>.Enumerator();
            this.m_data.Clear();
            try
            {
                List<int> DOFs = new List<int>();
                bool physical = false;

                enumerator = this.SelectedItems.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    GH_ValueListItem item = enumerator.Current;
                    int DOF;
                    item.Value.CastTo<int>(out DOF);
                    DOFs.Add(DOF);
                    if (DOF == 100)
                    {
                        physical = true;
                        DOFs = new List<int> { 1, 2, 3 };
                        break;
                    }
                }
                Guanaco.SupportType supportType = new SupportType(DOFs, physical);

                this.m_data.Append(GH_Convert.ToGoo(supportType), new GH_Path(0));
            }

            finally
            {
                enumerator.Dispose();
            }
        }

        /***************************************************/
    }
}

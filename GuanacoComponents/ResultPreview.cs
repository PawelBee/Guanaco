using Grasshopper.Kernel;
using System;
using System.Collections.Generic;

namespace Guanaco.Components
{
    public class ResultPreview : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the _TestPopulateElements class.
        /// </summary>
        public ResultPreview()
          : base("ResultPreview", "ResultPreview",
              "Preview the results.",
              "Guanaco", "Results")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Model", "Model", "Model", GH_ParamAccess.item);
            pManager.AddGenericParameter("Node result type", "NodeResultType", "Node result type - use predefined dropdown.", GH_ParamAccess.item);
            pManager.AddGenericParameter("Bar result type", "BarResultType", "Bar result type - use predefined dropdown.", GH_ParamAccess.item);
            pManager.AddGenericParameter("Panel result type", "PanelResultType", "Panel result type - use predefined dropdown.", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Preview values", "Values", "Preview values?", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("Preview nodes", "Nodes", "Show nodes?", GH_ParamAccess.item, true);
            pManager.AddBooleanParameter("Preview edges", "Edges", "Show mesh edges?", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("Deformed", "Deformed", "Deformed results", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("Preview LCS", "LCS", "Show local coordinate systems?", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("Preview loads", "Loads", "Show loads?", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("Preview supports", "Supports", "Show supports?", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("Preview component numbers", "ComponentNumbers", "Show component numbers?", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("Preview node numbers", "NodeNumbers", "Show node numbers?", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("Preview element numbers", "ElementNumbers", "Show element numbers?", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("Preview load values", "LoadValues", "Show load values?", GH_ParamAccess.item, false);
            pManager.AddNumberParameter("Text size factor", "TextFactor", "Custom scale of text.", GH_ParamAccess.item, 1);
            pManager.AddNumberParameter("Graphic size factor", "GraphicFactor", "Custom scale of graphics.", GH_ParamAccess.item, 1);
            pManager.AddNumberParameter("Deformation factor", "DeformationFactor", "Custom scale of deformation.", GH_ParamAccess.item, 1);
            pManager.AddIntegerParameter("Bars ids to preview", "BarIds", "All shown if empty.", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Panels ids to preview", "PanelIds", "All shown if empty.", GH_ParamAccess.list);
            pManager[18].Optional = true;
            pManager[19].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
        }

        Rhino.Display.CustomDisplay m_display;
        Rhino.RhinoDoc doc = Rhino.RhinoDoc.ActiveDoc;
        GH_Document ghdoc;
        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            ghdoc = this.OnPingDocument();
            ghdoc.ContextChanged += new GH_Document.ContextChangedEventHandler(CheckDisplay);
            
            Model input = null;
            DA.GetData(0, ref input);
            if (input.Mesh == null) return;
            Model model = input.Clone() as Model;

            Grasshopper.Kernel.Types.GH_ObjectWrapper type0D = null;
            Grasshopper.Kernel.Types.GH_ObjectWrapper type1D = null;
            Grasshopper.Kernel.Types.GH_ObjectWrapper type2D = null;
            DA.GetData(1, ref type0D);
            DA.GetData(2, ref type1D);
            DA.GetData(3, ref type2D);

            type0D.Value.CheckResultDimension(0);
            type1D.Value.CheckResultDimension(1);
            type2D.Value.CheckResultDimension(2);
            string nodeResultType = type0D.ToString();
            string barResType = type1D.ToString();
            string panelResType = type2D.ToString();

            bool showVals, showNds, showEdgs, deformed, showLCS, showLoads, showSupports, showCNs, showNNs, showENs, showLVals;
            showVals = showNds = showEdgs = deformed = showLCS = showLoads = showSupports = showCNs = showNNs = showENs = showLVals = false;
            double TFactor, GFactor, Dfactor;
            TFactor = GFactor = Dfactor = 0;
            DA.GetData(4, ref showVals);
            DA.GetData(5, ref showNds);
            DA.GetData(6, ref showEdgs);
            DA.GetData(7, ref deformed);
            DA.GetData(8, ref showLCS);
            DA.GetData(9, ref showLoads);
            DA.GetData(10, ref showSupports);
            DA.GetData(11, ref showCNs);
            DA.GetData(12, ref showNNs);
            DA.GetData(13, ref showENs);
            DA.GetData(14, ref showLVals);
            DA.GetData(15, ref TFactor);
            DA.GetData(16, ref GFactor);
            DA.GetData(17, ref Dfactor);

            List<int> panelIds = new List<int>();
            List<int> barIds = new List<int>();
            DA.GetDataList(18, barIds);
            DA.GetDataList(19, panelIds);

            double[] sizeFactors = model.sizeFactors();
            double EFactor = sizeFactors[0];
            double FFactor = sizeFactors[1];

            if (deformed) model.Mesh.Deform(Dfactor);

            m_display = new Rhino.Display.CustomDisplay(!this.Hidden);
            Grasshopper.GUI.Gradient.GH_Gradient gradient = PreviewUtil.CreateStandardGradient();

            HashSet<int> nodeIds = new HashSet<int>();
            HashSet<int> barNodeIds;
            HashSet<int> panelNodeIds;
            m_display.AddBarPreview(model, barIds, out barNodeIds, GFactor, EFactor, TFactor, showCNs, showENs, showLCS, false, barResType, showVals);
            m_display.AddPanelPreview(model, panelIds, out panelNodeIds, GFactor, EFactor, TFactor, FFactor, showCNs, showENs, showLCS, showLoads, showLVals, showEdgs, false, gradient, panelResType, showVals);

            nodeIds.UnionWith(panelNodeIds);
            nodeIds.UnionWith(barNodeIds);
            m_display.AddNodePreview(model, nodeIds, GFactor, EFactor, TFactor, FFactor, showNds, showNNs, showLoads, showLVals, gradient, nodeResultType, showVals);

            if (showSupports) m_display.AddSupportPreview(model, nodeIds, EFactor, GFactor);
        }

        public override void CreateAttributes()
        {
            base.CreateAttributes();
            this.ObjectChanged += new IGH_DocumentObject.ObjectChangedEventHandler(CheckDisplay);
            this.SolutionExpired += new IGH_DocumentObject.SolutionExpiredEventHandler(DisposeDisplay);
        }

        private void CheckDisplay(object sender, EventArgs e)
        {
            if (m_display != null)
            {
                if (ghdoc.Enabled && !this.Hidden)
                {
                    m_display.Enabled = true;
                }
                else
                {
                    m_display.Enabled = false;
                }
            }
        }

        private void DisposeDisplay(object sender, EventArgs e)
        {
            if (m_display != null)
            {
                m_display.Dispose();
            }
        }

        public override void RemovedFromDocument(GH_Document document)
        {
            if (m_display != null)
            {
                m_display.Dispose();
            }
            base.RemovedFromDocument(document);
        }

        public override bool Locked
        {
            get
            {
                return base.Locked;
            }
            set
            {
                base.Locked = value;
                if (Locked && m_display != null)
                {
                    m_display.Dispose();
                }
            }
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Icons.ResultPreview;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("3ea8fa65-1ddc-4558-88b4-18f49e0986e7"); }
        }
    }
}
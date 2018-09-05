using Grasshopper.Kernel;
using System;
using System.Collections.Generic;
using RG = Rhino.Geometry;

namespace Guanaco.Components
{
    public class ToGHMesh : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the ToGHMesh class.
        /// </summary>
        public ToGHMesh()
          : base("ToGHMesh", "ToGHMesh",
              "Translate Guanaco Mesh into Grasshopper mesh.",
              "Guanaco", "Mesh")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Guanaco mesh", "GCOMesh", "Guanaco mesh to be translated.", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGeometryParameter("GH mesh", "GHMesh", "Output Grasshopper mesh.", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Guanaco.Model model = null;
            DA.GetData(0, ref model);
            List<RG.GeometryBase> GHMesh = new List<RG.GeometryBase>();
            List<RG.LineCurve> barElements = new List<RG.LineCurve>();
            GHMesh.Add(MeshUtil.GetRhinoMesh(model.Mesh, out barElements));
            GHMesh.AddRange(barElements);
            DA.SetDataList(0, GHMesh);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Icons.ToGHMesh;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("c3723d58-9943-4268-95ee-c5a565b74699"); }
        }
    }
}
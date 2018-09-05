using Grasshopper.Kernel;
using Rhino.Geometry;
using System;

namespace Guanaco.Components
{
    public class GravityLoad : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the GravityLoad class.
        /// </summary>
        public GravityLoad()
          : base("GravityLoad", "GravityLoad",
              "Create a gravity load.",
              "Guanaco", "BoundaryConditions")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddVectorParameter("Load value", "Value", "Load value as a vector.", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Load", "Load", "Created point load.", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Vector3d factor = new Vector3d();
            DA.GetData(0, ref factor);

            Guanaco.GravityLoad load = new Guanaco.GravityLoad(factor);
            DA.SetData(0, load);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Icons.GravityLoad;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("bbe83bb5-38ec-4f5a-b6c0-362c0e0b4049"); }
        }
    }
}
using Grasshopper.Kernel;
using System;

namespace Guanaco.Components
{
    public class GmshInfo : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the GmshInfo class.
        /// </summary>
        public GmshInfo()
          : base("GmshInfo", "GmshInfo",
              "Info on Gmsh package.",
              "Guanaco", "Info")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Info", "Info", "Info on the package.", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            DA.SetData(0, "Version 3.01. http://gmsh.info/");
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Icons.GmshInfo;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("e96c465c-3fbb-4331-8613-93ca9a2c5dd9"); }
        }
    }
}
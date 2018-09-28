using Grasshopper.Kernel;
using System;

namespace Guanaco.Components
{
    public class CCXInfo : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the CalculiXInfo class.
        /// </summary>
        public CCXInfo()
          : base("CCXInfo", "CCXInfo",
              "Info on CalculiX package.",
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
            DA.SetData(0, "Version 2.14. http://www.calculix.de/");
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Icons.CCXInfo;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("e4a36d1c-d2cd-4c83-b747-fd13293f2aa0"); }
        }
    }
}
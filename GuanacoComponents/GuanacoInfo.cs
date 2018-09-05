using Grasshopper.Kernel;
using System;

namespace Guanaco.Components
{
    public class GuanacoInfo : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the Info class.
        /// </summary>
        public GuanacoInfo()
          : base("GuanacoInfo", "GuanacoInfo",
              "Info on Guanaco package.",
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
            DA.SetData(0, "Guanaco v. 1.0\n\nContact:\nP.Baran, Tentech B.V.\npawel@tentech.nl\npawel.baran@mail.com");
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Icons.GuanacoInfo;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("f255896e-1986-4342-99e2-147a3e96e2e1"); }
        }
    }
}
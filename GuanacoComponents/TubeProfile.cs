using Grasshopper.Kernel;
using System;

namespace Guanaco.Components
{
    public class TubeProfile : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the TubeProfile class.
        /// </summary>
        public TubeProfile()
          : base("TubeProfile", "TubeProfile",
              "Create a tube profile.",
              "Guanaco", "Elements")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Name", "Name", "Name of the profile.", GH_ParamAccess.item, string.Empty);
            pManager.AddNumberParameter("Diameter", "Diameter", "Diameter of the profile.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Thickness", "Thickness", "Wall thickness. Full profile in if the value set to zero", GH_ParamAccess.item, 0);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Profile", "Profile", "Created tube profile.", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string name = string.Empty;
            double diameter, thickness;
            diameter = thickness = 0;
            DA.GetData(0, ref name);
            DA.GetData(1, ref diameter);
            DA.GetData(2, ref thickness);

            if (thickness == 0) DA.SetData(0, new ProfileRound(diameter, name));
            else DA.SetData(0, new ProfilePipe(diameter, thickness, name));
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return Icons.TubeProfile;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("0bd7de59-c2c3-462b-ae07-aa52b6f89bf4"); }
        }
    }
}
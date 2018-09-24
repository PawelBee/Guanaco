using Grasshopper.Kernel;
using System;

namespace Guanaco.Components
{
    public class BoxProfile : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the BoxProfile class.
        /// </summary>
        public BoxProfile()
          : base("BoxProfile", "BoxProfile",
              "Create a box profile.",
              "Guanaco", "Elements")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Name", "Name", "Name of the profile.", GH_ParamAccess.item, string.Empty);
            pManager.AddNumberParameter("Height", "Height", "Height of the profile.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Width", "Width", "Width of the profile.", GH_ParamAccess.item);
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
            double height, width, thickness;
            height = width = thickness = 0;
            DA.GetData(0, ref name);
            DA.GetData(1, ref height);
            DA.GetData(2, ref width);
            DA.GetData(3, ref thickness);

            if (thickness == 0) DA.SetData(0, new ProfileRectangular(height, width, name));
            else DA.SetData(0, new ProfileBox(height, width, thickness, name));
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
                return Icons.BoxProfile;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("df1ee0cb-d456-4f84-9a9f-2b49b2144214"); }
        }
    }
}
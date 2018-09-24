using Grasshopper.Kernel;
using Rhino.Geometry;
using System;

namespace Guanaco.Components
{
    public class CreateBar : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the CreateBar class.
        /// </summary>
        public CreateBar()
          : base("CreateBar", "CreateBar",
              "Create a bar.",
              "Guanaco", "Elements")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddLineParameter("Curve", "Curve", "Bar curve.", GH_ParamAccess.item);
            pManager.AddGenericParameter("Profile", "Profile", "Bar profile.", GH_ParamAccess.item);
            pManager.AddGenericParameter("Material", "Material", "Bar material.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Rotation", "Rotation", "Rotation.", GH_ParamAccess.item, 0);
            pManager.AddNumberParameter("XOffset", "XOffset", "Offset along the local X axis.", GH_ParamAccess.item, 0);
            pManager.AddNumberParameter("YOffset", "YOffset", "Offset along the local Y axis.", GH_ParamAccess.item, 0);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Bar", "Bar", "Created bar.", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Line curve = new Line();
            Profile profile = null;
            Material material = null;
            double rotation, XOffset, YOffset;
            rotation = XOffset = YOffset = 0;
            DA.GetData(0, ref curve);
            DA.GetData(1, ref profile);
            DA.GetData(2, ref material);
            DA.GetData(3, ref rotation);
            DA.GetData(4, ref XOffset);
            DA.GetData(5, ref YOffset);

            DA.SetData(0, new Bar(new LineCurve(curve), profile, material, rotation, new Vector2d(XOffset, YOffset)));
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
                return Icons.CreateBar;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("ed7c22b1-2ede-4702-bdb4-90a02dbfa6b8"); }
        }
    }
}
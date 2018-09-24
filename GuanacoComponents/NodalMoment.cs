using Grasshopper.Kernel;
using Rhino.Geometry;
using System;

namespace Guanaco.Components
{
    public class NodalMoment : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the NodalMoment class.
        /// </summary>
        public NodalMoment()
          : base("NodalMoment", "NodalMoment",
              "Create a moment load at point.",
              "Guanaco", "BoundaryConditions")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Point", "Point", "Point to apply the load.", GH_ParamAccess.item);
            pManager.AddVectorParameter("Axis", "Axis", "Axis around which the moment is applied.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Moment", "Moment", "Value of the moment.", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Load", "Load", "Created moment.", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Point3d point = new Point3d();
            Vector3d axis = new Vector3d();
            double value = 0;
            DA.GetData(0, ref point);
            DA.GetData(1, ref axis);
            DA.GetData(2, ref value);
            axis.Unitize();

            DA.SetData(0, new Guanaco.NodalMoment(point, axis, value));
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
                return Icons.NodalMoment;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("abacfcdb-ec43-46dd-839f-10c31ff5df30"); }
        }
    }
}
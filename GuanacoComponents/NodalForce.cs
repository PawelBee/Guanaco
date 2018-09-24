using Grasshopper.Kernel;
using Rhino.Geometry;
using System;

namespace Guanaco.Components
{
    public class NodalForce : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the PointLoad class.
        /// </summary>
        public NodalForce()
          : base("NodalForce", "NodalForce",
              "Create a point load.",
              "Guanaco", "BoundaryConditions")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Point", "Point", "Point to apply the load.", GH_ParamAccess.item);
            pManager.AddVectorParameter("Force", "Force", "Force as vector.", GH_ParamAccess.item);
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
            Point3d point = new Point3d();
            Vector3d force = new Vector3d();
            DA.GetData(0, ref point);
            DA.GetData(1, ref force);
            
            DA.SetData(0, new Guanaco.NodalForce(point, force));
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Icons.NodalForce;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("0ac6aa2b-64cc-4ae1-9b90-9d2b34bc3cb7"); }
        }
    }
}
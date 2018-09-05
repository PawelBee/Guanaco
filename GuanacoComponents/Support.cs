using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

namespace Guanaco.Components
{
    public class Support : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the Support class.
        /// </summary>
        public Support()
          : base("Support", "Support",
              "Create a support.",
              "Guanaco", "BoundaryConditions")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGeometryParameter("Geometry", "Geometry", "Geometry of the support: point, curve or surface", GH_ParamAccess.list);
            pManager.AddGenericParameter("Support type", "SupportType", "Support type - use predefined drop-down.", GH_ParamAccess.item);
            pManager.AddTextParameter("Support name", "Name", "Support name - must be unique!", GH_ParamAccess.item);
            pManager[0].DataMapping = GH_DataMapping.Flatten;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Support", "support", "Created support.", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Object> geometry = new List<object>();
            Guanaco.SupportType type = null;
            string name = string.Empty;
            DA.GetDataList(0, geometry);
            DA.GetData(1, ref type);
            DA.GetData(2, ref name);

            List<GeometryBase> rhgeo = new List<GeometryBase>();
            foreach (object g in geometry)
            {
                GeometryBase gg = GH_Convert.ToGeometryBase(g);
                rhgeo.Add(gg);
            }

            Guanaco.Support support = new Guanaco.Support(rhgeo, type, name);
            DA.SetData(0, support);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Icons.Support;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("dff16e21-2194-491c-8f2f-34b950cf7239"); }
        }
    }
}
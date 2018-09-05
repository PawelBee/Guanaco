using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

namespace Guanaco.Components
{
    public class InfillLoad : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the PressureLoad class.
        /// </summary>
        public InfillLoad()
          : base("InfillLoad", "InfillLoad",
              "Create an infill load.",
              "Guanaco", "BoundaryConditions")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBrepParameter("Volumes", "Volumes", "Volumes applying the load.", GH_ParamAccess.list);
            pManager.AddTextParameter("Load function", "LoadFunction", "Load as a function of z (total height of the volume), x (distance of the evaluated point from the top of the volume), g (density of the infill material). Default is hydrostatic.", GH_ParamAccess.item, "g*x");
            pManager.AddNumberParameter("Infill density", "InfillDensity", "Infill density.", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Load", "Load", "Created infill load.", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Brep> volumes = new List<Brep>();
            string lFunction = string.Empty;
            double d = 0;
            DA.GetDataList(0, volumes);
            DA.GetData(1, ref lFunction);
            DA.GetData(2, ref d);

            Guanaco.InfillLoad load = new Guanaco.InfillLoad(volumes, lFunction, d);
            DA.SetData(0, load);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Icons.InfillLoad;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("c12bc283-9b1d-4e45-a55c-1690e043be9d"); }
        }
    }
}
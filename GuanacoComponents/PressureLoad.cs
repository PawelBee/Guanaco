using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

namespace Guanaco.Components
{
    public class PressureLoad : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the PressureLoad class.
        /// </summary>
        public PressureLoad()
          : base("PressureLoad", "PressureLoad",
              "Create a pressure load normal to the element.",
              "Guanaco", "BoundaryConditions")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBrepParameter("Surfaces", "Surfaces", "Surfaces applying the load.", GH_ParamAccess.list);
            pManager.AddNumberParameter("Load value", "Value", "Load value.", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Load", "Load", "Created pressure load.", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Brep> surfaces = new List<Brep>();
            double lvalue = 0;
            DA.GetDataList(0, surfaces);
            DA.GetData(1, ref lvalue);

            Guanaco.PressureLoad load = new Guanaco.PressureLoad(surfaces, lvalue);
            DA.SetData(0, load);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Icons.PressureLoad;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("76b84f82-c76a-4aeb-92e9-5041ff82e265"); }
        }
    }
}
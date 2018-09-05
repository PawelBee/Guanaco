using Grasshopper.Kernel;
using System;
using System.Collections.Generic;

namespace Guanaco.Components
{
    public class GetListOfProperties : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the GetListOfParameters class.
        /// </summary>
        public GetListOfProperties()
          : base("GetListOfProperties", "GetProperties",
              "Get list of object's properties.",
              "Guanaco", "Util")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Object", "Object", "Object to be processed.",GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Properties", "Properties", "Properties of the object.", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Grasshopper.Kernel.Types.GH_ObjectWrapper GHWrapper = null;
            DA.GetData(0, ref GHWrapper);

            List<string> props = new List<string>();
            foreach (System.Reflection.PropertyInfo prop in GHWrapper.Value.GetType().GetProperties())
            {
                props.Add(prop.Name);
            }

            DA.SetDataList(0, props);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Icons.GetListOfProperties;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("307063c3-2ef1-4393-93db-3db2b5c7552b"); }
        }
    }
}
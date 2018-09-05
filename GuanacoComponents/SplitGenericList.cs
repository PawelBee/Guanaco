using Grasshopper.Kernel;
using System;

namespace Guanaco.Components
{
    public class SplitGenericList : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the SplitGenericList class.
        /// </summary>
        public SplitGenericList()
          : base("SplitGenericList", "SplitGenericList",
              "Split generic list into its items.",
              "Guanaco", "Util")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Generic list", "GenericList", "Generic list to be split.", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("List items", "ListItems", "Items of the list.", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Grasshopper.Kernel.Types.GH_ObjectWrapper GHWrapper = null;
            DA.GetData(0, ref GHWrapper);

            if (GHWrapper.Value is System.Collections.IList && GHWrapper.Value.GetType().IsGenericType)
            {
                DA.SetDataList(0, (System.Collections.IList)GHWrapper.Value);
            }
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Icons.SplitGenericList;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("aab2eede-09e9-4e86-b007-541dec560c3d"); }
        }
    }
}
using Grasshopper.Kernel;
using System;

namespace Guanaco.Components
{
    public class SearchLibrary : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the SearchLibrary class.
        /// </summary>
        public SearchLibrary()
          : base("SearchLibrary", "SearchLibrary",
              "Search built-in library for an item.",
              "Guanaco", "Library")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("LibraryName", "LibraryName", "Name of the library to be searched.", GH_ParamAccess.item);
            pManager.AddTextParameter("Item", "Item", "Name of the item to be searched for.", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Item", "Item", "Found item.", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string library, item;
            library = item = string.Empty;
            DA.GetData(0, ref library);
            DA.GetData(1, ref item);
            DA.SetData(0, LibraryUtil.SearchLibrary(library, item));
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
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("0e1f3dd6-b4cc-475c-9e4b-21563b3bebc3"); }
        }
    }
}
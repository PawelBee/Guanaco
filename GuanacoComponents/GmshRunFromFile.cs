using Grasshopper.Kernel;
using System;

namespace Guanaco.Components
{
    public class GmshRunFromFile : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the GmshRunFile class.
        /// </summary>
        public GmshRunFromFile()
          : base("GmshRunFromFile", "GmshRunFromFile",
              "Meshing of a model based on a Gmsh input file.",
              "Guanaco", "Expert")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Run meshing process", "Run", "The model is meshed when true.", GH_ParamAccess.item, false);
            pManager.AddGenericParameter("Model", "Model", "Model to be meshed", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Reduced integration elements are created if true", "ReducedIntegration", "The choice is yours.", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Output model", "Model", "Result of meshing.", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool run = false;
            Model input = null;
            bool reducedIntegration = false;

            DA.GetData(0, ref run);
            DA.GetData(1, ref input);
            DA.GetData(2, ref reducedIntegration);

            if (run)
            {
                Model model = input.Clone() as Model;
                model.MeshModelFromFile(GHUtil.GmshPath(this.OnPingDocument()), reducedIntegration);
                DA.SetData(0, model);
            }
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
            get { return new Guid("4d6849f7-7ad8-462c-bed5-2749a00bfe77"); }
        }
    }
}
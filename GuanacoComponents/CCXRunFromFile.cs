using Grasshopper.Kernel;
using Rhino;
using System;

namespace Guanaco.Components
{
    public class CCXRunFromFile : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the CCXRunFromFile class.
        /// </summary>
        public CCXRunFromFile()
          : base("CCXRunFromFile", "CCXRunFromFile",
              "Solve Guanaco model CalculiX based on an input file.",
              "Guanaco", "Expert")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Run", "Run", "Runs the CalculiX input file if true.", GH_ParamAccess.item, false);
            pManager.AddGenericParameter("Model", "Model", "Built model.", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Threads", "Threads", "Number of threads to be used.", GH_ParamAccess.item, Environment.ProcessorCount);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Results", "Results", "Results.", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool run = false;
            int threads = 0;
            Model input = null;
            DA.GetData(0, ref run);
            DA.GetData(1, ref input);
            DA.GetData(2, ref threads);

            if (run)
            {
                Model model = input.Clone() as Model;
                model.SolveFromFile(threads, GHUtil.CCXPath(this.OnPingDocument()));
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
                return Icons.CCXRunFromFile;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("e13be38a-724d-4fa5-aac6-a4b6f54a0bcc"); }
        }
    }
}
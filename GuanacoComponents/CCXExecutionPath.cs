using Grasshopper.Kernel;
using Rhino;
using System;

namespace Guanaco.Components
{
    public class CCXExecutionPath : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the CCXExecutionPath class.
        /// </summary>
        public CCXExecutionPath()
          : base("CCXExecutionPath", "CCXExecutionPath",
              "Command that can be ran in shell to execute the CalculiX file (if it has already been created).",
              "Guanaco", "Expert")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Model", "Model", "Model aimed to be ran manually.", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Command", "Command", "Command to be executed in shell to run the model.", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Model model = null;
            DA.GetData(0, ref model);

            string CCXCommand = model.BuildCCXCommand(Environment.ProcessorCount, GHUtil.CCXPath(this.OnPingDocument()));
            DA.SetData(0, CCXCommand);
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
            get { return new Guid("a86c6ba8-f5e6-4e52-9c85-12001a192420"); }
        }
    }
}
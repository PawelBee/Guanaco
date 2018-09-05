using Grasshopper.Kernel;
using System;

namespace Guanaco.Components
{
    public class CCXBuildFile : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the CCXBuildFile class.
        /// </summary>
        public CCXBuildFile()
          : base("CCXBuildFile", "CCXBuildFile",
              "Build CalculiX input file based on the model.",
              "Guanaco", "Expert")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Run", "Run", "Generate the CalculiX input file in the model directory if true.", GH_ParamAccess.item, false);
            pManager.AddGenericParameter("Model", "Model", "Model to be processed.", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Nonlinear", "Nonlinear", "Nonlinear analysis if true.", GH_ParamAccess.item, false);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Model", "Model", "Model saved in the file.", GH_ParamAccess.item);
            pManager.AddTextParameter("File path", "FilePath", "CalculiX file path.", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool generate, nLinear;
            generate = nLinear = false;
            Model input = null;
            DA.GetData(0, ref generate);
            DA.GetData(1, ref input);
            DA.GetData(2, ref nLinear);

            if (generate)
            {
                Model model = input.Clone() as Model;
                model.BuildCCXFile(nLinear);
                DA.SetData(0, model);
                DA.SetData(1, model.Directory + "\\" + model.Name + ".inp");
            }
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Icons.CCXBuildFile;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("1d022f37-8928-4fb2-a562-ac0bc54c7bd5"); }
        }
    }
}
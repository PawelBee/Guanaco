using Grasshopper.Kernel;
using Rhino;
using System;


namespace Guanaco.Components
{
    public class GmshBuildFile : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the GmshBuildFile class.
        /// </summary>
        public GmshBuildFile()
          : base("GmshBuildFile", "GmshBuildFile",
              "Build Gmsh input file based on the model and meshing parameters.",
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
            pManager.AddGenericParameter("Gmsh parameters", "GmshParams", "Created mesh parameters.", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Model", "Model", "Model saved in the file.", GH_ParamAccess.item);
            pManager.AddTextParameter("File path", "FilePath", "Gmsh file path.", GH_ParamAccess.item);
        }

        public bool undoGH = false;
        public bool done = false;
        public Model model;

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool run = false;
            Model input = null;
            MeshUtil.GmshParams gmshParams = null;

            DA.GetData(0, ref run);
            DA.GetData(1, ref input);
            DA.GetData(2, ref gmshParams);

            RhinoDoc doc = RhinoDoc.ActiveDoc;
            GH_Document GrasshopperDocument = this.OnPingDocument();

            if (undoGH)
            {
                Rhino.RhinoApp.RunScript("_undo", false);
                undoGH = false;
                done = true;
            }

            if (run && !done)
            {
                model = input.Clone() as Model;
                model.BuildGmshFile(doc, gmshParams);

                undoGH = true;
                GrasshopperDocument.ScheduleSolution(0, ScheduleCallback);
            }

            else if (!run)
            {
                done = false;
            }

            DA.SetData(0, model);
            if (model != null)
                DA.SetData(1, model.GetModelFilePath(GuanacoUtil.FileType.geo));
        }

        private void ScheduleCallback(GH_Document GrasshopperDocument)
        {
            this.ExpireSolution(false);
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
            get { return new Guid("5f9b7586-c11a-40ee-a706-90cf6ede7c7d"); }
        }
    }
}
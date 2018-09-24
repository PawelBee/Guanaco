using Grasshopper.Kernel;
using Rhino;
using System;
using System.Collections.Generic;

namespace Guanaco.Components
{
    public class GmshModel : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the GmshElements class.
        /// </summary>
        public GmshModel()
          : base("GmshModel", "GmshModel",
              "Single-step meshing of a model with Gmsh.",
              "Guanaco", "Mesh")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Run meshing process", "Run", "The model is meshed when true.", GH_ParamAccess.item, false);
            pManager.AddGenericParameter("Model", "Model", "Model to be meshed", GH_ParamAccess.item);
            pManager.AddGenericParameter("Gmsh parameters", "GmshParams", "Created mesh parameters.", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Reduced integration elements are created if true", "ReducedIntegration", "The choice is yours.", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Output model", "Model", "Result of meshing.", GH_ParamAccess.item);
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
            bool reducedIntegration = false;

            DA.GetData(0, ref run);
            DA.GetData(1, ref input);
            DA.GetData(2, ref gmshParams);
            DA.GetData(3, ref reducedIntegration);

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
                model.MeshModel(doc, gmshParams, GHUtil.GmshPath(GrasshopperDocument), reducedIntegration);
                
                undoGH = true;
                GrasshopperDocument.ScheduleSolution(0, ScheduleCallback);
            }

            else if (!run)
            {
                done = false;
            }

            DA.SetData(0, model);
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
                return Icons.GmshModel;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("7122e29b-fe0e-4c81-9482-380d54e08b91"); }
        }
    }
}
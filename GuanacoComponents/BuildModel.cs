using Grasshopper.Kernel;
using System;
using System.Collections.Generic;

namespace Guanaco.Components
{
    public class BuildModel : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the BuildModel class.
        /// </summary>
        public BuildModel()
          : base("BuildModel", "BuildModel",
              "Build model based on separate components.",
              "Guanaco", "Model")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Name", "Name", "Name of the model.", GH_ParamAccess.item);
            pManager.AddTextParameter("Directory", "Directory", "Directory to be used - if empty then a Windows temp folder is used and the files are cleaned after finishing the job.", GH_ParamAccess.item, string.Empty);
            pManager.AddGenericParameter("Components", "Components", "Bars and panels.", GH_ParamAccess.list);
            pManager.AddNumberParameter("Tolerance", "Tolerance", "Dimensional tolerance used while building the model.", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Model", "Model", "Built model.", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string name = string.Empty;
            string dir = string.Empty;
            List<BuildingComponent> components = new List<BuildingComponent>();
            double tol = 0;
            DA.GetData(0, ref name);
            DA.GetData(1, ref dir);
            DA.GetDataList(2, components);
            DA.GetData(3, ref tol);

            for (int i = 0; i < components.Count; i++)
            {
                components[i] = components[i].Clone(false) as BuildingComponent;
            }

            Model model = new Model(name, dir, components, tol);
            DA.SetData(0, model);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Icons.BuildModel;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("cba380d6-411c-4e91-b55f-2b50aed8df03"); }
        }
    }
}
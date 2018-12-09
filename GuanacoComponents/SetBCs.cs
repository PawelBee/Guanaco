using Grasshopper.Kernel;
using System;
using System.Collections.Generic;

namespace Guanaco.Components
{
    public class SetBCs : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the SetBCs class.
        /// </summary>
        public SetBCs()
          : base("SetBCs", "SetBCs",
              "Set boundary conditions (loads, supports) to the model.",
              "Guanaco", "BoundaryConditions")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Model", "Model", "Model.", GH_ParamAccess.item);
            pManager.AddGenericParameter("Loads", "Loads", "Loads.", GH_ParamAccess.list);
            pManager.AddGenericParameter("Supports", "Supports", "Supports.", GH_ParamAccess.list);
            pManager[1].Optional = true;
            pManager[2].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Model", "Model", "Model after applying the boundary conditions.", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Model input = null;
            List<Load> loads = new List<Load>();
            List<Guanaco.Support> supports = new List<Guanaco.Support>();
            DA.GetData(0, ref input);
            DA.GetDataList(1, loads);
            DA.GetDataList(2, supports);
            if (input.Mesh == null) throw new Exception("The boundary conditions can be applied to a meshed model only.");

            Model model = input.Clone() as Model;
            foreach (Load l in loads)
            {
                model.AddLoad(l.Clone() as Load);
            }
            foreach (Guanaco.Support s in supports)
            {
                model.AddSupport(s.Clone() as Guanaco.Support);
            }
            DA.SetData(0, model);
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
                return Icons.SetBCs;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("fa97b7e8-185f-47c8-bd00-ecca9d1a432b"); }
        }
    }
}
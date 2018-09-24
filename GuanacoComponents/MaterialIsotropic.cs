using Grasshopper.Kernel;
using System;

namespace Guanaco.Components
{
    public class MaterialIsotropic : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MaterialIsotropic class.
        /// </summary>
        public MaterialIsotropic()
          : base("MaterialIsotropic", "MaterialIsotropic",
              "Create an isotropic material based on given properties.",
              "Guanaco", "Material")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Name", "Name", "Material name.", GH_ParamAccess.item);
            pManager.AddNumberParameter("E", "E", "Young's modulus.", GH_ParamAccess.item);
            pManager.AddNumberParameter("v", "v", "Poisson ratio.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Density", "Density", "Density.", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Material", "Material", "Created material.", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string name = string.Empty;
            double E, v, density;
            E = v = density = 0;
            DA.GetData(0, ref name);
            DA.GetData(1, ref E);
            DA.GetData(2, ref v);
            DA.GetData(3, ref density);

            DA.SetData(0, new Guanaco.MaterialIsotropic(name, E, v, density));
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
                return Icons.MaterialIsotropic;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("d9cf9324-973f-4757-ac0c-cd8ffadf8139"); }
        }
    }
}
using Grasshopper.Kernel;
using System;

namespace Guanaco.Components
{
    public class MaterialEngineeringConstants : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MaterialEngineeringConstants class.
        /// </summary>
        public MaterialEngineeringConstants()
          : base("MaterialEngineeringConstants", "MaterialEngineeringConstants",
              "Create an orthotropic material based on the engineering constants.",
              "Guanaco", "Material")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Name", "Name", "Material name.", GH_ParamAccess.item);
            pManager.AddNumberParameter("E1", "E1", "Young's modulus in X direction.", GH_ParamAccess.item);
            pManager.AddNumberParameter("E2", "E2", "Young's modulus in Y direction.", GH_ParamAccess.item);
            pManager.AddNumberParameter("E3", "E3", "Young's modulus in Z direction.", GH_ParamAccess.item);
            pManager.AddNumberParameter("v12", "v12", "Poisson ratio in xy.", GH_ParamAccess.item);
            pManager.AddNumberParameter("v13", "v13", "Poisson ratio in xz.", GH_ParamAccess.item);
            pManager.AddNumberParameter("v23", "v23", "Poisson ratio in yz.", GH_ParamAccess.item);
            pManager.AddNumberParameter("G12", "G12", "Shear modulus in xy.", GH_ParamAccess.item);
            pManager.AddNumberParameter("G13", "G13", "Shear modulus in xz.", GH_ParamAccess.item);
            pManager.AddNumberParameter("G23", "G23", "Shear modulus in yz.", GH_ParamAccess.item);
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
            double E1, E2, E3, v12, v13, v23, G12, G13, G23, density;
            E1 = E2 = E3 = v12 = v13 = v23 = G12 = G13 = G23 = density = 0;
            DA.GetData(0, ref name);
            DA.GetData(1, ref E1);
            DA.GetData(2, ref E2);
            DA.GetData(3, ref E3);
            DA.GetData(4, ref v12);
            DA.GetData(5, ref v13);
            DA.GetData(6, ref v23);
            DA.GetData(7, ref G12);
            DA.GetData(8, ref G13);
            DA.GetData(9, ref G23);
            DA.GetData(10, ref density);

            DA.SetData(0, new Guanaco.MaterialEngineeringConstants(name, E1, E2, E3, v12, v13, v23, G12, G13, G23, density));
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
            get { return new Guid("3d1978fa-d08d-4bff-a28a-1870e91c0abc"); }
        }
    }
}
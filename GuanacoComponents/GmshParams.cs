using Grasshopper.Kernel;
using System;


namespace Guanaco.Components
{
    public class GmshParams : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the GmshParamscs class.
        /// </summary>
        public GmshParams()
          : base("GmshParams", "GmshParams",
              "Create meshing parameters to be used by Gmsh.",
              "Guanaco", "Mesh")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Element shape", "ElementShape", "Tri or quad, use predefined dropdown.", GH_ParamAccess.item);
            pManager.AddGenericParameter("Meshing algorithm", "MeshAlgorithm", "Meshing algorithm, use predefined dropdown.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Maximum characteristic edge length", "MaxCharLength", "The choice is yours.", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Second order elements", "SecondOrder", "The choice is yours.", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Number of smoothing steps", "SmoothingSteps", "The choice is yours.", GH_ParamAccess.item);
            pManager.AddBooleanParameter("High order optimization if true", "HighOrderOptimization", "The choice is yours.", GH_ParamAccess.item, false);
            pManager.AddNumberParameter("Optimization threshold", "OptimizationThreshold", "The choice is yours.", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Unify face normals if true", "UnifyFaces", "The choice is yours.", GH_ParamAccess.item, true);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Gmsh parameters", "GmshParams", "Created mesh parameters.", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            double maxCharLength = 0;
            double optimizationThreshold = 0;
            int smoothingSteps = 0;
            bool highOrderOptimization, secondOrder, unifyFaces;
            highOrderOptimization = secondOrder = unifyFaces = false;
            Element2D.Element2DShape elementShape = Element2D.Element2DShape.Tri;
            MeshUtil.GmshAlgorithm meshingAlgorithm = MeshUtil.GmshAlgorithm.Automatic;
            
            DA.GetData(0, ref elementShape);
            DA.GetData(1, ref meshingAlgorithm);
            DA.GetData(2, ref maxCharLength);
            DA.GetData(3, ref secondOrder);
            DA.GetData(4, ref smoothingSteps);
            DA.GetData(5, ref highOrderOptimization);
            DA.GetData(6, ref optimizationThreshold);
            DA.GetData(7, ref unifyFaces);

            DA.SetData(0, new MeshUtil.GmshParams(elementShape, meshingAlgorithm, maxCharLength, secondOrder, smoothingSteps, highOrderOptimization, optimizationThreshold, unifyFaces));
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
                return Icons.GmshParams;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("08dce95f-6c9a-4b73-95fb-cdf4ab153603"); }
        }
    }
}
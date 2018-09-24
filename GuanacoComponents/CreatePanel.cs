using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

namespace Guanaco.Components
{
    public class CreatePanel : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the CreatePanel class.
        /// </summary>
        public CreatePanel()
          : base("CreatePanel", "CreatePanel",
              "Create a panel.",
              "Guanaco", "Elements")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBrepParameter("Surface", "Surface", "Panel surface.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Thickness", "Thickness", "Panel thickness.", GH_ParamAccess.item);
            pManager.AddGenericParameter("Material", "Material", "Panel material.", GH_ParamAccess.item);
            pManager.AddGenericParameter("FEType", "FEType", "Finite element 2D type, use predefined dropdown.", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Panel", "Panel", "Created panel.", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Brep surface = null;
            double thickness = double.NaN;
            Material material = null;
            Element2D.Element2DType feType = Element2D.Element2DType.Unknown;
            DA.GetData(0, ref surface);
            DA.GetData(1, ref thickness);
            DA.GetData(2, ref material);
            DA.GetData(3, ref feType);

            List<Panel> panels = new List<Panel>();
            for (int i = 0; i < surface.Faces.Count; i++)
            {
                panels.Add(new Panel(surface.Faces.ExtractFace(i), thickness, material, -1, feType));
            }

            DA.SetDataList(0, panels);
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
                return Icons.CreatePanel;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("642f47cf-2684-48da-a9b8-e616b390ab31"); }
        }
    }
}
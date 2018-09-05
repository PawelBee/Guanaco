using Grasshopper.Kernel;
using System;

namespace Guanaco.Components
{
    public class GetPropertyByName : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the GetParameterByName class.
        /// </summary>
        public GetPropertyByName()
          : base("GetPropertyByName", "GetPropertyValue",
              "Get object's property by name.",
              "Guanaco", "Util")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Object", "Object", "Object to be processed.", GH_ParamAccess.item);
            pManager.AddTextParameter("Property name", "PropertyName", "Name of the property to be checked for value.", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Property value", "PropertyValue", "Value of the property.", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GuanacoObject input = null;
            string prop = string.Empty;
            DA.GetData(0, ref input);
            DA.GetData(1, ref prop);

            try
            {
                object value = input.GetType().GetProperty(prop).GetValue(input);
                DA.SetData(0, value);
            }
            catch (NullReferenceException)
            {
            }
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Icons.GetPropertyByName;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("def94d5d-4e85-48ed-af34-77c12eb47406"); }
        }
    }
}
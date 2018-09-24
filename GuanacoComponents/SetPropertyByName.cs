using Grasshopper.Kernel;
using System;

namespace Guanaco.Components
{
    public class SetPropertyByName : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the GetParameterByName class.
        /// </summary>
        public SetPropertyByName()
          : base("SetPropertyByName", "SetPropertyValue",
              "Set object's property by name.",
              "Guanaco", "Util")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Object", "Object", "Object to be processed.", GH_ParamAccess.item);
            pManager.AddTextParameter("Property name", "PropertyName", "Name of the property to be changed.", GH_ParamAccess.item);
            pManager.AddTextParameter("Property value", "PropertyValue", "Value to be assigned to the property.", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Object", "Object", "Object to after change.", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GuanacoObject input = null;
            string prop = string.Empty;
            object value = null;
            DA.GetData(0, ref input);
            DA.GetData(1, ref prop);
            DA.GetData(2, ref value);

            try
            {
                GuanacoObject newObject = input.Clone();
                newObject.GetType().GetProperty(prop).SetValue(newObject, value);
                DA.SetData(0, newObject);
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
                return Icons.SetPropertyByName;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("ba217094-8a70-41e6-8caa-20c5a7d5e831"); }
        }
    }
}
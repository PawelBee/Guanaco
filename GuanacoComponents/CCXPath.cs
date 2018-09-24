using Grasshopper.Kernel;
using System;
using System.IO;

namespace Guanaco.Components
{
    public class CCXPath : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the CCXPath class.
        /// </summary>
        public CCXPath()
          : base("CCXPath", "CCXPath",
              "Manually set CCX path.",
              "Guanaco", "Util")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Path", "Path", "Path to the executable of CCX.", GH_ParamAccess.item, string.Empty);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddBooleanParameter("Success", "Success", "True if a custom CCX path successfully added.", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        bool onlyInstance = true;
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_Document GrasshopperDocument = this.OnPingDocument();

            if (this.Params.Input[0].VolatileData.DataCount > 1)
            {
                throw new Exception("CCXPath component receives more than one input. Maximum one input is allowed.");
            }
            string path = string.Empty;
            DA.GetData(0, ref path);
            
            path = path.Replace(@"\", @"\\");
            if (path == string.Empty || path == "")
            {
                DA.SetData(0, false);
            }
            else if (File.Exists(path))
            {
                if (GrasshopperDocument.ConstantServer.ContainsKey("CCXPath")) GrasshopperDocument.ConstantServer["CCXPath"] = new Grasshopper.Kernel.Expressions.GH_Variant(path);
                else GrasshopperDocument.ConstantServer.Add("CCXPath", new Grasshopper.Kernel.Expressions.GH_Variant(path));
                DA.SetData(0, true);
            }
            else
            {
                throw new Exception("The file under given path does not exist.");
            }
        }

        public override void RemovedFromDocument(GH_Document document)
        {
            if (onlyInstance && document.ConstantServer.ContainsKey("CCXPath")) document.ConstantServer.Remove("CCXPath");
            base.RemovedFromDocument(document);
        }

        public override void CreateAttributes()
        {
            base.CreateAttributes();
            this.ObjectChanged += new IGH_DocumentObject.ObjectChangedEventHandler(CheckIfEnabled);
        }

        private void CheckIfEnabled(object sender, EventArgs e)
        {
            GH_Document GrasshopperDocument = this.OnPingDocument();
            if (this.Locked && onlyInstance && GrasshopperDocument.ConstantServer.ContainsKey("CCXPath")) GrasshopperDocument.ConstantServer.Remove("CCXPath");
        }
        
        protected override void BeforeSolveInstance()
        {
            GH_Document GrasshopperDocument = this.OnPingDocument();
            foreach (IGH_DocumentObject obj in GrasshopperDocument.Objects)
            {
                if (obj.Name == "CCXPath")
                {
                    if (obj != this)
                    {
                        onlyInstance = false;
                        throw new Exception("A CCXPath component already exists on the canvas. Use that one to set the path.");
                    }
                    else onlyInstance = true;
                    break;
                }
            }
            if (GrasshopperDocument.ConstantServer.ContainsKey("CCXPath")) GrasshopperDocument.ConstantServer.Remove("CCXPath");
            base.BeforeSolveInstance();
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
                return Icons.CCXPath;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("31f99947-a3d8-4a3b-81c7-b3a29180f1c4"); }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using Rhino.Geometry;

namespace Guanaco
{
    /***************************************************/
    /****                   Model                   ****/
    /***************************************************/

    public class Model : GuanacoObject
    {
        /***************************************************/

        // Fields & properties.
        string _name;
        public string Name
        {
            get
            {
                return this._name;
            }
        }

        string _directory;
        public string Directory
        {
            get
            {
                return this._directory;
            }
        }

        Mesh _mesh;
        public Mesh Mesh
        {
            get
            {
                return this._mesh;
            }
        }

        double _tolerance;
        public double Tolerance
        {
            get
            {
                return this._tolerance;
            }
        }

        List<Bar> _bars;
        public List<Bar> Bars
        {
            get
            {
                return this._bars;
            }
        }

        List<Panel> _panels;
        public List<Panel> Panels
        {
            get
            {
                return this._panels;
            }
        }

        List<Load> _loads;
        public List<Load> Loads
        {
            get
            {
                return this._loads;
            }
        }

        List<Support> _supports;
        public List<Support> Supports
        {
            get
            {
                return this._supports;
            }
        }

        Vector3d _gravity;
        public Vector3d Gravity
        {
            get
            {
                return this._gravity;
            }
        }

        MaterialCollection _materials;
        public MaterialCollection Materials
        {
            get
            {
                return this._materials;
            }
        }

        bool _CCXFileBuilt;
        bool _GmshFileBuilt;

        /***************************************************/

        // Constructors.
        public Model(string name, string directory, List<BuildingComponent> components, double tolerance)
        {
            // Set the directory: if null then temp dir to be used
            if (directory == null || directory == string.Empty || directory == "")
                this._directory = GuanacoUtil.TempDir;
            else
            {
                System.IO.Directory.CreateDirectory(directory);
                this._directory = directory;
            }

            // Declare properties.
            this._name = name;
            this._bars = new List<Bar>();
            this._panels = new List<Panel>();
            this._materials = new MaterialCollection();

            // Add components to relevant lists.
            foreach (BuildingComponent bc in components)
            {
                if (bc is Bar)
                {
                    AddBar(bc as Bar);
                }
                else if (bc is Panel)
                {
                    AddPanel(bc as Panel);
                }
                else throw new Exception("Unrecognized component type. Currently only bars and panels are accepted.");
            }
            
            // Assign other properties.
            this._loads = new List<Load>();
            this._supports = new List<Support>();
            this._tolerance = tolerance;
            this._gravity = new Vector3d();
        }

        /***************************************************/

        // Destructor - delete temporary files.
        ~Model()
        {
            if (Directory == GuanacoUtil.TempDir)
            {
                foreach (string file in System.IO.Directory.GetFiles(System.IO.Path.GetTempPath()))
                {
                    if (file.StartsWith(Name + GetTempSuffix()))
                    {
                        System.IO.File.Delete(file);
                    }
                }
            }
        }

        /***************************************************/

        // Set mesh of the model.
        public void SetMesh(Mesh mesh)
        {
            this._mesh = mesh;
        }

        /***************************************************/

        // Return the next free bar Id.
        public int NextBarId()
        {
            return this._bars.Count;
        }

        /***************************************************/

        // Return the next free bar Id.
        public int NextPanelId()
        {
            return this._panels.Count;
        }

        /***************************************************/

        // Add bar to the model.
        public void AddBar(Bar bar)
        {
            bar.AddToModel(this);
        }

        /***************************************************/

        // Add panel to the model.
        public void AddPanel(Panel panel)
        {
            panel.AddToModel(this);
        }

        /***************************************************/

        // Add material to the model.
        public void AddMaterial(Material material)
        {
            this._materials.AddMaterial(material);
        }

        /***************************************************/

        // Add support to the model.
        public void AddSupport(Support support)
        {
            Support s = support.Clone() as Support;
            this._supports.Add(s);
            this._mesh.ApplySupport(s, Tolerance);
        }

        /***************************************************/

        // Guid used to create suffix for a temporary file.
        public string GetTempSuffix()
        {
            return Convert.ToBase64String(this.UniqueId.ToByteArray()).Replace("=", "").Replace("+", "").Replace("/","");
        }

        /***************************************************/

        // Get model file path.
        public string GetModelFilePath(GuanacoUtil.FileType? fileType = null)
        {
            string path = this._directory + "\\" + Name;
            if (this._directory == GuanacoUtil.TempDir) path += GetTempSuffix();
            if (fileType != null)
                path += "." + fileType;
            return path;
        }

        /***************************************************/

        // Build CalculiX input file.
        public virtual void BuildGmshFile(Rhino.RhinoDoc doc, MeshUtil.GmshParams gmshParams)
        {
            doc.Views.RedrawEnabled = false;
            Rhino.DocObjects.ObjectEnumeratorSettings all;
            all = new Rhino.DocObjects.ObjectEnumeratorSettings();
            all.ObjectTypeFilter = Rhino.DocObjects.ObjectType.AnyObject;
            foreach (Rhino.DocObjects.RhinoObject ro in doc.Objects.GetObjectList(all))
            {
                doc.Objects.Hide(ro.Id, true);
            }

            // Add bar curves to Rhino.
            for (int i = this._bars.Count - 1; i >= 0; i--)
            {
                doc.Objects.Add(this._bars[i].Curve);
            }

            // Add panel surfaces to Rhino.
            for (int i = this._panels.Count - 1; i >= 0; i--)
            {
                doc.Objects.Add(this._panels[i].Surface);
            }
            
            // Export the geometry to .iges files.
            string exportCrvCom = "-_export _selcrv _enter \"" + this.GetModelFilePath() + "_Curves.iges\" _enter";
            string exportSrfCom = "-_export _selsrf _enter \"" + this.GetModelFilePath() + "_Surfaces.iges\" _enter";
            Rhino.RhinoApp.RunScript(exportCrvCom, false);
            Rhino.RhinoApp.RunScript(exportSrfCom, false);
            Rhino.RhinoDoc.ActiveDoc.Views.RedrawEnabled = true;

            string name = this._name;
            if (this._directory == GuanacoUtil.TempDir) name += this.GetTempSuffix();

            System.IO.TextWriter tw = new System.IO.StreamWriter(this.GetModelFilePath(GuanacoUtil.FileType.geo));
            foreach (String s in MeshUtil.GmshInput(name, doc.ModelAbsoluteTolerance, gmshParams))
            {
                tw.WriteLine(s);
            }
            tw.Close();
            
            this._GmshFileBuilt = true;
        }

        /***************************************************/

        // Run the input file in Gmsh.
        public void RunGmshFile(string gmshpath)
        {
            string GmshCmd = "/C " + gmshpath + " -m \"" + this.GetModelFilePath(GuanacoUtil.FileType.geo) + "\" -o \"" + this.GetModelFilePath() + "_Mesh.inp\" -0";
            System.Diagnostics.Process cmd = System.Diagnostics.Process.Start("CMD.exe", GmshCmd);
            cmd.WaitForExit();
        }

        /***************************************************/

        // Mesh the model from file.
        public void MeshModelFromFile(string gmshPath, bool reducedIntegration)
        {
            this.RunGmshFile(gmshPath);
            MeshUtil.ReadMesh(this, reducedIntegration);
        }

        /***************************************************/

            // Mesh the model.
        public void MeshModel(Rhino.RhinoDoc doc, MeshUtil.GmshParams gmshParams, string gmshPath, bool reducedIntegration)
        {
            if (this._GmshFileBuilt)
                throw new Exception("A file has been created for this model. Run the analysis with GmshRunFromFile to execute the alrady created file.");
            this.BuildGmshFile(doc, gmshParams);
            this.MeshModelFromFile(gmshPath, reducedIntegration);
        }

        /***************************************************/

        // Build CalculiX input file.
        public virtual void BuildCCXFile(bool nonLinear)
        {
            List<string> outputLines = new List<string>();

            // Write mesh topology information.
            outputLines.AddRange(this._mesh.ToCCX());

            // Write support information.
            foreach (Support s in this._supports)
            {
                outputLines.AddRange(s.ToCCX());
            }

            // Write material information.
            foreach (Material m in this._materials.Items)
            {
                outputLines.AddRange((m as dynamic).ToCCX());
            }

            // Write bar information.
            foreach (Bar bar in this._bars)
            {
                outputLines.AddRange(bar.ToCCX());
            }

            // Write panel information.
            foreach (Panel panel in this._panels)
            {
                outputLines.AddRange(panel.ToCCX());
            }

            // Write information about the analysis mode.
            if (nonLinear) outputLines.Add("*STEP,NLGEOM");
            else outputLines.Add("*STEP");
            outputLines.Add("*STATIC");

            // Set pressure and gravity loads (element loads).
            outputLines.Add("*DLOAD");
            foreach (Element element in this._mesh.Elements)
            {
                if (element is Element2D)
                {
                    Element2D e = element as Element2D;
                    if (e.Pressure != 0) outputLines.Add(e.PressureToCCX());
                }
            }

            if (this._gravity.Length > 0)
            {
                outputLines.Add(String.Format("Eall, GRAV,{0},{1},{2},{3}", Gravity.Length, Gravity.X / Gravity.Length, Gravity.Y / Gravity.Length, Gravity.Z / Gravity.Length));
            }

            // Set nodal loads.
            outputLines.Add("*CLOAD");
            foreach (Node n in this._mesh.Nodes)
            {
                outputLines.AddRange(n.LoadToCCX());
            }

            // Set sections in which the bar results are to be calculated.
            foreach (Element e in this._mesh.Elements)
            {
                if (e is Element1D) outputLines.AddRange((e as Element1D).CCXSectionResults());
            }

            // Set output format.
            outputLines.AddRange(new List<string> { "*EL FILE,GLOBAL=NO", "S, E", "*NODE PRINT,NSET=Nall,GLOBAL=YES", "U", "*END STEP" });

            // Write the information to file.
            System.IO.TextWriter tw = new System.IO.StreamWriter(this.GetModelFilePath(GuanacoUtil.FileType.inp));
            foreach (String s in outputLines)
            {
                tw.WriteLine(s);
            }
            tw.Close();

            // Set the info about building the file.
            this._CCXFileBuilt = true;
        }

        /***************************************************/

        // Run the input file in Calxulix.
        public void RunCCXFile(int threads, string CCXpath)
        {
            string CCXCmd = "/C " + "set OMP_NUM_THREADS=" + threads.ToString() + " & pushd " + this._directory + " & \"" + CCXpath + "\" -i " + this._name;
            if (this._directory == GuanacoUtil.TempDir)
                CCXCmd += this.GetTempSuffix();
            System.Diagnostics.Process cmd = System.Diagnostics.Process.Start("CMD.exe", CCXCmd);
            cmd.WaitForExit();
        }

        /***************************************************/

        // Run the model from file.
        public void SolveFromFile(int threads, string CCXpath)
        {
            this.RunCCXFile(threads, CCXpath);
            ResultUtil.ReadNodeDisplacements(this);
            ResultUtil.Read1DElementForces(this);
            ResultUtil.Read2DElementResults(this, ResultUtil.ResultGroup.Strain);
            ResultUtil.Read2DElementResults(this, ResultUtil.ResultGroup.Stress);
        }

        /***************************************************/

        // Run a solution and read results from the output file.
        public void Solve(bool nonlinear, int threads, string CCXpath)
        {
            if (this._CCXFileBuilt)
                throw new Exception("A file has been created for this model. Run the analysis with CCXRunFromFile to execute the alrady created file.");
            this.BuildCCXFile(nonlinear);
            this.SolveFromFile(threads, CCXpath);
        }

        /***************************************************/

        // Clone.
        public override GuanacoObject Clone(bool newID = false)
        {
            Model m = this.ShallowClone(newID) as Model;
            m._bars = this._bars.Select(b => b.Clone(newID) as Bar).ToList();
            m._bars.ForEach(b => b.SetParentModel(m));
            m._panels = this._panels.Select(p => p.Clone(newID) as Panel).ToList();
            m._panels.ForEach(p => p.SetParentModel(m));
            if (this._mesh != null)
            {
                m._mesh = this._mesh.Clone(newID) as Mesh;
                m._bars.ForEach(b => b.AssignElements(b.Elements));
                m._panels.ForEach(p => p.AssignElements(p.Elements));
            }
            m._loads = this._loads.Select(l => l.Clone(newID) as Load).ToList();
            m._supports = this._supports.Select(s => s.Clone(newID) as Support).ToList();
            m._gravity = new Vector3d(this._gravity);
            m._materials = this._materials.Clone(newID) as MaterialCollection;
            return m;
        }

        /***************************************************/
    }
}

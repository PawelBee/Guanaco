using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Guanaco
{
    /***************************************************/
    /****                   Model                   ****/
    /***************************************************/

    public class Model : GuanacoObject , IDisposable
    {
        /***************************************************/

        // Fields & properties.
        private string _name;
        public string Name
        {
            get
            {
                return this._name;
            }
        }

        private string _directory;
        public string Directory
        {
            get
            {
                return this._directory;
            }
        }

        private Mesh _mesh;
        public Mesh Mesh
        {
            get
            {
                return this._mesh;
            }
        }

        private double _tolerance;
        public double Tolerance
        {
            get
            {
                return this._tolerance;
            }
        }

        private IndexedCollection<Bar> _bars;
        public IndexedCollection<Bar> Bars
        {
            get
            {
                return this._bars;
            }
        }

        private IndexedCollection<Panel> _panels;
        public IndexedCollection<Panel> Panels
        {
            get
            {
                return this._panels;
            }
        }

        private List<Load> _loads;
        public IEnumerable<Load> Loads
        {
            get
            {
                return this._loads.AsReadOnly();
            }
        }

        private List<Support> _supports;
        public IEnumerable<Support> Supports
        {
            get
            {
                return this._supports;
            }
        }

        private Vector3d _gravity;
        public Vector3d Gravity
        {
            get
            {
                return this._gravity;
            }
        }

        private MaterialCollection _materials;
        public MaterialCollection Materials
        {
            get
            {
                return this._materials;
            }
        }

        private bool _CCXFileBuilt;
        private bool _GmshFileBuilt;

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
            this._bars = new IndexedCollection<Bar>();
            this._panels = new IndexedCollection<Panel>();
            this._materials = new MaterialCollection();

            // Add components to relevant lists.
            foreach (BuildingComponent bc in components)
            {
                if (bc is Bar)
                    AddBar(bc as Bar);
                else if (bc is Panel)
                    AddPanel(bc as Panel);
                else
                    throw new Exception("Unrecognized component type. Currently only bars and panels are accepted.");
            }
            
            // Assign other properties.
            this._loads = new List<Load>();
            this._supports = new List<Support>();
            this._tolerance = tolerance;
            this._gravity = new Vector3d();
        }

        /***************************************************/
        
        // Dispose the object.
        private bool isDisposed = false;

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (!this.isDisposed)
            {
                if (disposing)
                {

                }

                if (this._directory == GuanacoUtil.TempDir)
                {
                    string fileNamePattern = "*" + this._name + GetTempSuffix() + "*";
                    foreach (string file in System.IO.Directory.GetFiles(this._directory, fileNamePattern))
                    {
                        File.Delete(file);
                    }
                }
            }

            this.isDisposed = true;
        }

        ~Model()
        {
            this.Dispose(false);
        }

        /***************************************************/

        // Add bar to the model.
        public void AddBar(Bar bar, int id = -1)
        {
            this._bars.Add(bar, id);
            this._materials.Add(bar.Material);
        }

        /***************************************************/

        // Add panel to the model.
        public void AddPanel(Panel panel, int id = -1)
        {
            this._panels.Add(panel, id);
            this._materials.Add(panel.Material);
        }

        /***************************************************/

        // Add material to the model.
        public void AddMaterial(Material material)
        {
            this._materials.Add(material);
        }

        /***************************************************/

        // Apply load to mesh components (nodes/elements) within tolerance.
        public void AddLoad(Load load)
        {
            this._loads.Add(load);
            this.ApplyLoad(load as dynamic);
        }

        /***************************************************/

        // Add support to the model.
        public void AddSupport(Support support)
        {
            this._supports.Add(support);
            this.ApplySupport(support);
        }

        /***************************************************/

        // Guid used to create suffix for a temporary file.
        public string GetTempSuffix()
        {
            return Convert.ToBase64String(this._uniqueId.ToByteArray()).Replace("=", "").Replace("+", "").Replace("/","");
        }

        /***************************************************/

        // Get model file path.
        public string GetModelFilePath(GuanacoUtil.FileType? fileType = null)
        {
            string path = this._directory + "\\" + Name;

            if (this._directory == GuanacoUtil.TempDir)
                path += GetTempSuffix();

            if (fileType != null)
                path += "." + fileType;

            return path;
        }

        /***************************************************/

        // Build CalculiX input file.
        public virtual void BuildGmshFile(Rhino.RhinoDoc doc, MeshUtil.GmshParams gmshParams)
        {
            doc.Views.RedrawEnabled = false;

            // Hide all existing Rhino objects.
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
            string curveFilePath = this.GetModelFilePath() + "_Curves.iges";
            string surfaceFilePath = this.GetModelFilePath() + "_Surfaces.iges";

            if (File.Exists(curveFilePath))
                File.Delete(curveFilePath);
            if (File.Exists(surfaceFilePath))
                File.Delete(surfaceFilePath);

            string exportCrvCom = "-_export _selcrv _enter \"" + curveFilePath + "\" _enter";
            string exportSrfCom = "-_export _selsrf _enter \"" + surfaceFilePath + "\" _enter";
            Rhino.RhinoApp.RunScript(exportCrvCom, false);
            Rhino.RhinoApp.RunScript(exportSrfCom, false);
            Rhino.RhinoDoc.ActiveDoc.Views.RedrawEnabled = true;

            string name = this._name;
            if (this._directory == GuanacoUtil.TempDir)
                name += this.GetTempSuffix();

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
            this._mesh = new Mesh();
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
            outputLines.Add(nonLinear ? "*STEP,NLGEOM" : "*STEP");
            outputLines.Add("*STATIC");

            // Set pressure and gravity loads (element loads).
            outputLines.Add("*DLOAD");
            foreach (Element element in this._mesh.Elements)
            {
                if (element is Element2D)
                {
                    Element2D e = element as Element2D;
                    if (e.Pressure != 0)
                        outputLines.Add(e.PressureToCCX());
                }
            }

            if (this._gravity.Length > 0)
                outputLines.Add(String.Format("Eall, GRAV,{0},{1},{2},{3}", Gravity.Length, Gravity.X / Gravity.Length, Gravity.Y / Gravity.Length, Gravity.Z / Gravity.Length));

            // Set nodal loads.
            outputLines.Add("*CLOAD");
            foreach (Node n in this._mesh.Nodes)
            {
                outputLines.AddRange(n.LoadToCCX());
            }

            // Set sections in which the bar results are to be calculated.
            foreach (Element e in this._mesh.Elements)
            {
                if (e is Element1D)
                    outputLines.AddRange((e as Element1D).CCXSectionResults());
            }

            // Set output format.
            outputLines.AddRange(new List<string> { "*EL FILE,GLOBAL=NO", "S, E", "*NODE PRINT,NSET=Nall,GLOBAL=YES", "U", "*END STEP" });

            // Write the information to file.
            TextWriter tw = new System.IO.StreamWriter(this.GetModelFilePath(GuanacoUtil.FileType.inp));
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
        public string BuildCCXCommand(int threads, string CCXpath)
        {
            string CCXCmd = "/C " + "set OMP_NUM_THREADS=" + threads.ToString() + " & pushd " + this._directory + " & \"" + CCXpath + "\" -i " + this._name;
            if (this._directory == GuanacoUtil.TempDir)
                CCXCmd += this.GetTempSuffix();

            return CCXCmd;
        }

        /***************************************************/

        // Run the input file in Calxulix.
        public void RunCCXFile(int threads, string CCXpath)
        {
            string CCXCmd = this.BuildCCXCommand(threads, CCXpath);
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

            m._bars = new IndexedCollection<Bar>();
            foreach (Bar bar in this._bars)
            {
                Bar b = bar.Clone(newID) as Bar;
                m.AddBar(b, bar.Id.AsInteger);
            }

            m._panels = new IndexedCollection<Panel>();
            foreach (Panel panel in this._panels)
            {
                Panel p = panel.Clone(newID) as Panel;
                m.AddPanel(p, panel.Id.AsInteger);
            }

            if (this._mesh != null)
            {
                m._mesh = this._mesh.Clone(newID) as Mesh;
                foreach (Bar bar in m._bars)
                {
                    bar.AssignElements(bar.Elements.Select(e => m._mesh.Elements[e.Id.AsInteger]).ToList());
                }
                foreach (Panel panel in m._panels)
                {
                    panel.AssignElements(panel.Elements.Select(e => m._mesh.Elements[e.Id.AsInteger]).ToList());
                }
            }

            m._loads = this._loads.Select(l => l.Clone(newID) as Load).ToList();
            m._supports = this._supports.Select(s => s.Clone(newID) as Support).ToList();
            m._gravity = new Vector3d(this._gravity);
            m._materials = this._materials.Clone(newID) as MaterialCollection;
            return m;
        }
        

        /***************************************************/
        /****              Private methods              ****/
        /***************************************************/

        //Find the closest node to a point lying in within the model tolerance.
        private Node ClosestNode(Point3d point)
        {
            Node closestNode = null;
            double minDist = this._tolerance;
            foreach (Node n in this._mesh.Nodes)
            {
                if (n.Primary)
                {
                    double dist = n.Location.DistanceTo(point);
                    if (dist <= minDist)
                    {
                        minDist = dist;
                        closestNode = n;
                    }
                }
            }
            return closestNode;
        }

        /***************************************************/

        // Apply point load to the closest point that lies within tolerance.
        private void ApplyLoad(NodalForce load)
        {
            Node nodeToLoad = this.ClosestNode(load.Point);
            if (nodeToLoad != null)
                nodeToLoad.AddForceLoad(load.LoadValue);
        }

        /***************************************************/

        // Apply moment to the closest point that lies within tolerance.
        private void ApplyLoad(NodalMoment load)
        {
            Node nodeToLoad = this.ClosestNode(load.Point);
            if (nodeToLoad != null)
                nodeToLoad.AddMomentLoad(load.Axis * load.LoadValue);
        }

        /***************************************************/

        // Apply infill load to the adjoining elements.
        private void ApplyLoad(InfillLoad load)
        {
            foreach (Element e in this._mesh.Elements)
            {
                // Check if the element is 2D.
                Element2D el = e as Element2D;
                if (el == null)
                    continue;

                Point3d elC = el.GetCentroid();
                Vector3d elN = el.GetNormal();
                foreach (Infill i in load.Infills)
                {
                    // Check if the element is adjoining to the infill (if all its vertices lie within tolerance).
                    bool broken = false;
                    foreach (Point3d v in el.GetVertices())
                    {
                        if (v.DistanceTo(i.Volume.ClosestPoint(v)) > this._tolerance)
                        {
                            broken = true;
                            break;
                        }
                    }

                    // If the element is adjoining to the infill, apply the load based on location of the element and load function of the infill.
                    if (!broken)
                    {
                        // Flip normal of the element if it points outside the infill.
                        Point3d cpt = Point3d.Add(elC, elN * this._tolerance);
                        if (!i.Volume.IsPointInside(cpt, Rhino.RhinoMath.SqrtEpsilon, true))
                            el.FlipNormal();

                        // Check if the element is not surrounded by the infill from both sides - if it is then do nothing (no hydrostatic pressure).
                        else
                        {
                            cpt = Point3d.Add(elC, -elN * this._tolerance);
                            if (i.Volume.IsPointInside(cpt, Rhino.RhinoMath.SqrtEpsilon, true))
                                continue;
                        }

                        // Apply the load based on location of the element and load function of the infill.
                        string g = load.InfillDensity.ToString(GuanacoUtil.Invariant);
                        string x = (i.MaxZ - elC.Z).ToString(GuanacoUtil.Invariant);
                        string z = (i.MaxZ - i.MinZ).ToString(GuanacoUtil.Invariant);
                        string f = load.LoadFunction.Replace("g", g).Replace("x", x).Replace("z", z);
                        double p = GuanacoUtil.Evaluate(f);
                        el.AddPressure(-p);
                    }
                }
            }
        }

        /***************************************************/

        // Apply pressure load to the elements laying within tolerance from the pressure area.
        private void ApplyLoad(PressureLoad load)
        {
            foreach (Element e in this._mesh.Elements)
            {
                // Check if the element is 2D.
                Element2D el = e as Element2D;
                if (el == null)
                    continue;

                // Check if the element is adjoining to the pressure area (if all its vertices lie within tolerance) - if yes then apply the load.
                Point3d elC = el.GetCentroid();
                foreach (Brep s in load.Surfaces)
                {
                    bool broken = false;
                    foreach (Point3d v in el.GetVertices())
                    {
                        if (v.DistanceTo(s.ClosestPoint(v)) > this._tolerance)
                        {
                            broken = true;
                            break;
                        }
                    }

                    if (!broken)
                        el.AddPressure(load.LoadValue * 1);
                }
            }
        }

        /***************************************************/

        // Apply support to the nodes laying within tolerance from the support geometry.
        private void ApplySupport(Support support)
        {
            foreach (GeometryBase g in support.Geometry)
            {
                if (g is Point)
                {
                    Point p = g as Point;
                    Point3d pt = p.Location;
                    foreach (Node node in this._mesh.Nodes)
                    {
                        if (node.Primary && node.Location.DistanceTo(pt) <= this._tolerance)
                            support.Nodes.Add(node);
                    }
                }
                else if (g is Curve)
                {
                    Curve c = g as Curve;
                    double t;
                    foreach (Node node in this._mesh.Nodes)
                    {
                        if (node.Primary)
                        {
                            c.ClosestPoint(node.Location, out t);
                            if (c.PointAt(t).DistanceTo(node.Location) <= this._tolerance)
                                support.Nodes.Add(node);
                        }
                    }
                }
                else if (g is Brep)
                {
                    Brep b = g as Brep;
                    foreach (Node node in this._mesh.Nodes)
                    {
                        if (node.Primary)
                        {
                            Point3d bpt = b.ClosestPoint(node.Location);
                            if (node.Location.DistanceTo(bpt) <= this._tolerance)
                                support.Nodes.Add(node);
                        }
                    }
                }
                else
                {
                    throw new Exception("Unknown support geometry type! " + support.Name);
                }
            }

            // If the physical fixing of the support is chosen, then instead of fixing rotational stiffness within chosen nodes, fix translation in all nodes of the supported elements.
            if (support.SupportType.Physical)
            {
                List<Node> baseNodes = support.Nodes.ToList();
                foreach (Element e in this._mesh.Elements)
                {
                    Element2D el = e as Element2D;
                    if (e == null)
                        continue;

                    HashSet<Node> elNodes = new HashSet<Node>(el.Nodes.Take(el.PrimaryNodeCount));
                    foreach (Node node in baseNodes)
                    {
                        if (elNodes.Any(n => n == node))
                        {
                            support.Nodes.UnionWith(elNodes);
                            break;
                        }
                    }
                }
            }
        }

        /***************************************************/

        // Apply gravity load to the model.
        private void ApplyLoad(GravityLoad load)
        {
            this._gravity += load.GravityFactor;
        }

        /***************************************************/
    }
}

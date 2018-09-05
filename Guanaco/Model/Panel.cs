using System;
using System.Collections.Generic;
using System.Linq;
using Rhino.Geometry;

namespace Guanaco
{
    /***************************************************/
    /****                   Panel                   ****/
    /***************************************************/

    public class Panel : BuildingComponent
    {
        /***************************************************/

        // Fields & properties.
        int _id;
        public override int Id
        {
            get
            {
                return this._id;
            }
        }

        List<int> _elements;
        public override List<int> Elements
        {
            get
            {
                return this._elements;
            }
        }

        Material _material;
        public override Material Material
        {
            get
            {
                return this._material;
            }
            set
            {
                this._material = value;
            }
        }

        Model _parentModel;
        public override Model ParentModel
        {
            get
            {
                return this._parentModel;
            }
        }

        Brep _surface;
        public Brep Surface
        {
            get
            {
                return this._surface;
            }
        }
        
        double _thickness;
        public double Thickness
        {
            get
            {
                return this._thickness;
            }
        }

        Element2D.Element2DType _feType;
        Element2D.Element2DType FEType
        {
            get
            {
                return this._feType;
            }
        }

        private Plane _lcs;
        public Plane LCS
        {
            get
            {
                return this._lcs;
            }
        }
        
        /***************************************************/

        // Constructors.
        public Panel(Brep surface, double thickness, Material material, int id = -1, Element2D.Element2DType feType = Element2D.Element2DType.Unknown)
        {
            this._id = id;
            this._surface = surface;
            this._thickness = thickness;
            this._feType = feType;
            this._material = material;
            this._elements = new List<int>();

            // Set the local coordinate system.
            IEnumerable<Point3d> vertices = surface.Vertices.Select(p => p.Location);
            PlaneFitResult pfr = Plane.FitPlaneToPoints(vertices, out this._lcs);
            if (pfr != PlaneFitResult.Success)
                throw new Exception("Geometry of the panel seems to be faulty!");
            this._lcs.Origin = GeometryUtil.Average(vertices);
        }

        /***************************************************/

        // Add panel to a model.
        public override void AddToModel(Model model)
        {
            this._id = model.NextPanelId();
            this._parentModel = model;
            model.AddMaterial(this._material);
            model.Panels.Add(this);
        }

        /***************************************************/

        // Set parent model of the panel.
        public override void SetParentModel(Model model)
        {
            this._parentModel = model;
        }

        /***************************************************/

        // Assign elements to the component after meshing.
        public override void AssignElements(List<int> elementIds)
        {
            this._elements = elementIds;
            foreach (int id in elementIds)
            {
                Element2D e = this._parentModel.Mesh.Elements[id] as Element2D;
                e.SetParentComponent(this);
                e.FEType = this._feType;
            }
        }

        /***************************************************/

        // Convert Id to CCX Id (zero is not allowed).
        public string CCXId()
        {
            return (this._id + 1).ToString();
        }

        /***************************************************/

        // Convert panel information to CCX format.
        public override List<string> ToCCX()
        {
            List<string> CCXFormat = new List<string> { "*ORIENTATION,NAME=ORPANEL" + this.CCXId() };
            CCXFormat.Add(GuanacoUtil.RhinoVectorToString(this._lcs.XAxis) + "," + GuanacoUtil.RhinoVectorToString(this._lcs.YAxis));
            CCXFormat.Add("*ELSET,ELSET=PANEL" + this.CCXId());
            CCXFormat.AddRange(GuanacoUtil.IntsToCCX(this._elements, true));
            CCXFormat.Add("*SHELL SECTION,MATERIAL=" + this._material.Name + ",ELSET=PANEL" + this.CCXId() + ",ORIENTATION=ORPANEL" + this.CCXId());
            CCXFormat.Add(this._thickness.ToString(GuanacoUtil.Invariant));
            return CCXFormat;
        }

        /***************************************************/

        // Clone.
        public override GuanacoObject Clone(bool newID = false)
        {
            Panel p = this.ShallowClone(newID) as Panel;
            p._lcs = new Plane(this._lcs);
            p._surface = this._surface.DuplicateBrep();
            p._elements = new List<int>(this._elements);
            return p;
        }

        /***************************************************/

        // Panel type enum.
        public enum PanelType
        {
            Shell,
            Membrane,
            PlaneStress,
            PlaneStrain
        }

        /***************************************************/
    }
}

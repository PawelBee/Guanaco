using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Guanaco
{
    /***************************************************/
    /****                   Panel                   ****/
    /***************************************************/

    public class Panel : BuildingComponent
    {
        /***************************************************/

        // Fields & properties.
        private Brep _surface;
        public Brep Surface
        {
            get
            {
                return this._surface;
            }
        }

        private double _thickness;
        public double Thickness
        {
            get
            {
                return this._thickness;
            }
        }

        private Element2D.Element2DType _feType;
        public Element2D.Element2DType FEType
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
            this._surface = surface;
            this._thickness = thickness;
            this._feType = feType;
            this._material = material;
            this._elements = new List<Element>();

            // Set the local coordinate system.
            IEnumerable<Point3d> vertices = surface.Vertices.Select(p => p.Location);
            PlaneFitResult pfr = Plane.FitPlaneToPoints(vertices, out this._lcs);
            if (pfr != PlaneFitResult.Success)
                throw new Exception("Geometry of the panel seems to be faulty!");
            this._lcs.Origin = GeometryUtil.Average(vertices);
        }

        /***************************************************/

        // Assign elements to the component after meshing.
        public override void AssignElements(List<Element> elements)
        {
            this._elements = elements;
            foreach (Element element in elements)
            {
                Element2D e = element as Element2D;
                e.FEType = this._feType;
            }
        }

        /***************************************************/

        // Convert panel information to CCX format.
        public override List<string> ToCCX()
        {
            string panelType;
            switch (this._feType)
            {
                case Element2D.Element2DType.Shell:
                    panelType = "*SHELL";
                    break;
                case Element2D.Element2DType.Membrane:
                    panelType = "*MEMBRANE";
                    break;
                default:
                    throw new Exception(String.Format("{0} is not supported in the current implementation of Guanaco.", this._feType));
            }
            List<string> CCXFormat = new List<string> { "*ORIENTATION,NAME=ORPANEL" + this.CCXId() };
            CCXFormat.Add(GuanacoUtil.RhinoVectorToString(this._lcs.XAxis) + "," + GuanacoUtil.RhinoVectorToString(this._lcs.YAxis));
            CCXFormat.Add("*ELSET,ELSET=PANEL" + this.CCXId());
            CCXFormat.AddRange(GuanacoUtil.IntsToCCX(this._elements.Select(e => e.Id.AsInteger), true));
            CCXFormat.Add(panelType + " SECTION,MATERIAL=" + this._material.Name + ",ELSET=PANEL" + this.CCXId() + ",ORIENTATION=ORPANEL" + this.CCXId());
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
            p._elements = new List<Element>(this._elements);
            p._parentCollection = null;
            p._id = null;
            return p;
        }

        // Clone to a target model.
        public virtual GuanacoObject Clone(Model targetModel, bool newID = false)
        {
            Panel p = this.Clone(newID) as Panel;
            if (targetModel != null)
                targetModel.AddPanel(p, this._id.AsInteger);

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

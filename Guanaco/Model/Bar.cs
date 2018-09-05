using System;
using System.Collections.Generic;
using Rhino.Geometry;

namespace Guanaco
{
    /***************************************************/
    /****                    Bar                    ****/
    /***************************************************/

    public class Bar : BuildingComponent
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

        LineCurve _curve;
        public LineCurve Curve
        {
            get
            {
                return this._curve;
            }
        }

        Profile _profile;
        public Profile Profile
        {
            get
            {
                return this._profile;
            }
        }

        double _rotation;
        public double Rotation
        {
            get
            {
                return this._rotation;
            }
        }

        Vector2d _offset;
        public Vector2d Offset
        {
            get
            {
                return this._offset;
            }
        }

        Plane _lcs;
        public Plane LCS
        {
            get
            {
                return this._lcs;
            }
        }

        /***************************************************/

        // Constructors.
        public Bar(LineCurve curve, Profile profile, Material material, double rotation = 0, Vector2d offset = new Vector2d(), int id = -1)
        {
            this._id = id;
            this._elements = new List<int>();
            this._material = material;
            this._curve = curve;
            this._profile = profile;
            this._rotation = rotation;
            this._offset = offset;
            
            // Set the local coordinate system.
            Vector3d direction = curve.Line.UnitTangent;
            Vector3d localX;
            if (direction.IsParallelTo(Vector3d.ZAxis, 1e-3) == 0)
            {
                localX = new Vector3d(direction.X, direction.Y, direction.Z);
                localX.Rotate(Math.PI * 0.5, Vector3d.CrossProduct(direction, Vector3d.ZAxis));
            }
            else localX = Vector3d.XAxis;
            Vector3d localY = new Vector3d(localX.X, localX.Y, localX.Z);
            localY.Rotate(Math.PI * 0.5, direction);
            this._lcs = new Plane((curve.PointAtStart + curve.PointAtEnd) * 0.5, localX, localY);
        }

        /***************************************************/

        // Add bar to a model.
        public override void AddToModel(Model model)
        {
            this._id = model.NextBarId();
            this._parentModel = model;
            model.AddMaterial(this._material);
            model.Bars.Add(this);
        }

        /***************************************************/

        // Set parent model of the bar.
        public override void SetParentModel(Model model)
        {
            this._parentModel = model;
        }

        /***************************************************/

        // Assign elements to the component after meshing.
        public override void AssignElements(List<int> elementIds)
        {
            this._elements = elementIds;
            if ((this.Profile is ProfilePipe || this.Profile is ProfileBox) && this._parentModel.Mesh.Elements[elementIds[0]].Order != 2)
                throw new Exception("Using beam general section requires 2nd order 1D elements. Refer to CalculiX manual.");
            foreach (int id in elementIds)
            {
                Element1D e = this._parentModel.Mesh.Elements[id] as Element1D;
                e.SetParentComponent(this);
            }
        }

        /***************************************************/

        // Convert Id to CCX Id (zero is not allowed).
        public string CCXId()
        {
            return (this._id + 1).ToString();
        }

        /***************************************************/

        // Convert bar information to CCX format.
        public override List<string> ToCCX()
        {
            Plane orLCS = new Plane(this._lcs);
            orLCS.Rotate(this._rotation, orLCS.ZAxis);

            List<string> CCXFormat = new List<string> { "*ORIENTATION,NAME=ORBAR" + this.CCXId() };
            CCXFormat.Add(GuanacoUtil.RhinoVectorToString(orLCS.XAxis) + "," + GuanacoUtil.RhinoVectorToString(orLCS.YAxis));
            CCXFormat.Add("*ELSET,ELSET=BAR" + this.CCXId());
            CCXFormat.AddRange(GuanacoUtil.IntsToCCX(this._elements, true));
            string[] profileInfo = this._profile.ToCCXFormat();
            string barInfo = profileInfo[0] + ",MATERIAL=" + this._material.Name + ",ELSET=BAR" + this.CCXId() + ",ORIENTATION=ORBAR" + this.CCXId() + ",";

            if (this._offset.X != 0) barInfo += "OFFSET1=" + (this._offset.X / this._profile.GetHeight()).ToString(GuanacoUtil.Invariant) + ",";
            if (this._offset.Y != 0) barInfo += "OFFSET2=" + (this._offset.Y / this._profile.GetWidth()).ToString(GuanacoUtil.Invariant) + ",";
            barInfo += profileInfo[1];
            
            CCXFormat.Add(barInfo);
            CCXFormat.Add(profileInfo[2]);
            CCXFormat.Add(GuanacoUtil.RhinoVectorToString(orLCS.XAxis));

            return CCXFormat;
        }

        /***************************************************/

        // Clone.
        public override GuanacoObject Clone(bool newID = false)
        {
            Bar b = this.ShallowClone(newID) as Bar;
            b._curve = this._curve.DuplicateCurve() as LineCurve;
            b._offset = new Vector2d(this._offset.X, this._offset.Y);
            b._lcs = new Plane(this._lcs);
            b._elements = new List<int>(this._elements);
            return b;
        }

        /***************************************************/
    }
}

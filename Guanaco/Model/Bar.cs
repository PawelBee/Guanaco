using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Guanaco
{
    /***************************************************/
    /****                    Bar                    ****/
    /***************************************************/

    public class Bar : BuildingComponent
    {
        /***************************************************/

        // Fields & properties.
        private LineCurve _curve;
        public LineCurve Curve
        {
            get
            {
                return this._curve;
            }
        }

        private Profile _profile;
        public Profile Profile
        {
            get
            {
                return this._profile;
            }
        }

        private double _rotation;
        public double Rotation
        {
            get
            {
                return this._rotation;
            }
        }

        private Vector2d _offset;
        public Vector2d Offset
        {
            get
            {
                return this._offset;
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
        public Bar(LineCurve curve, Profile profile, Material material, double rotation = 0, Vector2d offset = new Vector2d(), int id = -1)
        {
            this._elements = new List<Element>();
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
            else
                localX = Vector3d.XAxis;

            Vector3d localY = new Vector3d(localX.X, localX.Y, localX.Z);
            localY.Rotate(Math.PI * 0.5, direction);
            this._lcs = new Plane((curve.PointAtStart + curve.PointAtEnd) * 0.5, localX, localY);
        }

        /***************************************************/

        //Assign elements to the component after meshing.
        public override void AssignElements(List<Element> elements)
        {
            if ((this._profile is ProfilePipe || this._profile is ProfileBox) && elements.Any(e => e.Order != 2))
                throw new Exception("Using beam general section requires 2nd order 1D elements. Refer to CalculiX manual.");

            this._elements = elements;
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
            CCXFormat.AddRange(GuanacoUtil.IntsToCCX(this._elements.Select(e => e.Id.AsInteger), true));
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
            b._elements = new List<Element>(this._elements);
            b._parentCollection = null;
            b._id = null;
            return b;
        }

        // Clone to a target model.
        public GuanacoObject Clone(Model targetModel, bool newID = false)
        {
            Bar b = this.Clone(newID) as Bar;
            if (targetModel != null)
                targetModel.AddBar(b, this._id.AsInteger);

            return b;
        }

        /***************************************************/
    }
}

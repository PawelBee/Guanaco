using Rhino.Geometry;
using System.Collections.Generic;
using System.Linq;

namespace Guanaco
{
    /***************************************************/
    /****     Abstract class for all load types     ****/
    /***************************************************/

    public abstract class Load : GuanacoObject
    {

    }


    /***************************************************/
    /****             Point load class              ****/
    /***************************************************/

    public class NodalForce : Load
    {
        /***************************************************/

        // Fields & properties.
        private Point3d _point;
        public Point3d Point
        {
            get
            {
                return this._point;
            }
        }

        private Vector3d _loadValue;
        public Vector3d LoadValue
        {
            get
            {
                return this._loadValue;
            }
        }

        /***************************************************/

        // Constructors.
        public NodalForce(Point3d geo, Vector3d lValue)
        {
            this._point = geo;
            this._loadValue = lValue;
        }

        /***************************************************/

        // Clone.
        public override GuanacoObject Clone(bool newID = false)
        {
            NodalForce nf = this.ShallowClone(newID) as NodalForce;
            nf._loadValue = new Vector3d(this._loadValue);
            nf._point = new Point3d(this._point);
            return nf;
        }

        /***************************************************/
    }


    /***************************************************/
    /****            Nodal moment class             ****/
    /***************************************************/

    public class NodalMoment : Load
    {
        /***************************************************/

        // Fields & properties.
        private Point3d _point;
        public Point3d Point
        {
            get
            {
                return this._point;
            }
        }

        private Vector3d _axis;
        public Vector3d Axis
        {
            get
            {
                return this._axis;
            }
        }

        private double _loadValue;
        public double LoadValue
        {
            get
            {
                return this._loadValue;
            }
        }

        /***************************************************/

        // Constructors.
        public NodalMoment(Point3d geo, Vector3d axis, double lValue)
        {
            this._point = geo;
            this._axis = axis;
            this._loadValue = lValue;
        }

        /***************************************************/

        // Clone.
        public override GuanacoObject Clone(bool newID = false)
        {
            NodalMoment nm = this.ShallowClone(newID) as NodalMoment;
            nm._axis = new Vector3d(this._axis);
            nm._point = new Point3d(this._point);
            return nm;
        }

        /***************************************************/
    }


    /***************************************************/
    /****             Infill load class             ****/
    /***************************************************/

    public class InfillLoad : Load
    {
        /***************************************************/

        // Fields & properties.
        private List<Infill> _infills;
        public IEnumerable<Infill> Infills
        {
            get
            {
                return this._infills;
            }
        }

        private string _loadFunction;
        public string LoadFunction
        {
            get
            {
                return this._loadFunction;
            }
        }

        private double _infillDensity;
        public double InfillDensity
        {
            get
            {
                return this._infillDensity;
            }
        }

        /***************************************************/

        // Constructors.
        public InfillLoad(List<Brep> geometry, string loadFunction, double infillDensity)
        {
            this._infills = new List<Infill>();
            foreach (Brep b in geometry)
            {
                this._infills.Add(new Infill(b));
            }
            this._loadFunction = loadFunction;
            this._infillDensity = infillDensity;
        }

        /***************************************************/

        // Clone.
        public override GuanacoObject Clone(bool newID = false)
        {
            InfillLoad il = this.ShallowClone(newID) as InfillLoad;
            il._infills = this._infills.Select(i => i.Clone(newID) as Infill).ToList();
            return il;
        }

        /***************************************************/
    }


    /***************************************************/
    /****            Pressure load class            ****/
    /***************************************************/

    public class PressureLoad : Load
    {
        /***************************************************/

        // Properties.
        private List<Brep> _surfaces;
        public IEnumerable<Brep> Surfaces
        {
            get
            {
                return this._surfaces;
            }
        }

        private double _loadValue;
        public double LoadValue
        {
            get
            {
                return this._loadValue;
            }
        }

        /***************************************************/

        // Constructors.
        public PressureLoad(List<Brep> surfaces, double loadValue)
        {
            this._surfaces = surfaces;
            this._loadValue = loadValue;
        }

        /***************************************************/

        // Clone.
        public override GuanacoObject Clone(bool newID = false)
        {
            PressureLoad pl = this.ShallowClone(newID) as PressureLoad;
            pl._surfaces = this._surfaces.Select(s => s.DuplicateBrep()).ToList();
            return pl;
        }

        /***************************************************/
    }


    /***************************************************/
    /****            Gravity load class             ****/
    /***************************************************/

    public class GravityLoad : Load
    {
        /***************************************************/

        // Fields & properties.
        private Vector3d _gravityFactor;
        public Vector3d GravityFactor
        {
            get
            {
                return this._gravityFactor;
            }
        }

        /***************************************************/

        // Constructors.
        public GravityLoad(Vector3d factor)
        {
            this._gravityFactor = factor;
        }
        
        /***************************************************/

        // Clone.
        public override GuanacoObject Clone(bool newID = false)
        {
            GravityLoad gl = this.ShallowClone(newID) as GravityLoad;
            gl._gravityFactor = new Vector3d(this._gravityFactor);
            return gl;
        }

        /***************************************************/
    }


    /***************************************************/
    /****            Infill volume class            ****/
    /***************************************************/

    public class Infill : GuanacoObject
    {
        /***************************************************/

        // Fields & properties.
        private Brep _volume;
        public Brep Volume
        {
            get
            {
                return this._volume;
            }
        }

        private double _maxZ;
        public double MaxZ
        {
            get
            {
                return this._maxZ;
            }
        }

        private double _minZ;
        public double MinZ
        {
            get
            {
                return this._minZ;
            }
        }

        /***************************************************/

        // Constructors.
        public Infill(Brep volume)
        {
            this._volume = volume;
            BoundingBox bBox = Volume.GetBoundingBox(false);
            this._maxZ = bBox.Max.Z;
            this._minZ = bBox.Min.Z;
        }

        /***************************************************/

        // Clone.
        public override GuanacoObject Clone(bool newID = false)
        {
            Infill i = this.ShallowClone(newID) as Infill;
            i._volume = this._volume.DuplicateBrep();
            return i;
        }

        /***************************************************/
    }
}

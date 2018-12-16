using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Guanaco
{
    /***************************************************/
    /****            Generic element class          ****/
    /***************************************************/

    public abstract class Element : GuanacoIndexable
    {
        /***************************************************/

        // Fields & properties.
        protected Dictionary<string, double[]> _results;
        public virtual ReadOnlyDictionary<string, double[]> Results
        {
            get
            {
                return new ReadOnlyDictionary<string, double[]>(this._results);
            }
        }

        protected int _order;
        public virtual int Order
        {
            get
            {
                return this._order;
            }
            set
            {
                this._order = value;
            }
        }

        protected bool _reducedIntegration;
        public bool ReducedIntegration
        {
            get
            {
                return this._reducedIntegration;
            }
            set
            {
                this._reducedIntegration = value;
            }
        }

        protected List<Node> _nodes;
        public IEnumerable<Node> Nodes
        {
            get
            {
                return this._nodes;
            }
        }

        /***************************************************/

        // Methods to be implemented.
        public abstract List<string> ToCCX();
        public abstract string CCXType();
        public abstract IEnumerable<Point3d> GetVertices();
        public abstract GuanacoObject Clone(Mesh targetMesh, bool newID);

        /***************************************************/

        // Add result to the element.
        internal void AddResult(string resultType, double[] resultValue)
        {
            this._results.Add(resultType, resultValue);
        }

        /***************************************************/

        // Get results from the element.
        public virtual double[] GetResults(string resultType)
        {
            return this._results.ContainsKey(resultType) ? this._results[resultType] : null;
        }

        /***************************************************/
    }


    /***************************************************/
    /****                 1D element                ****/
    /***************************************************/

    public class Element1D : Element
    {
        /***************************************************/
        
        // Fields & properties.
        public override int Order
        {
            get
            {
                return this._order;
            }
            set
            {
                if (value == 1 || value == 2)
                    this._order = value;
                else
                    throw new Exception("1D elements can be order 1 or 2. Change order of the element.");
            }
        }

        /***************************************************/

        // Constructors.
        public Element1D(List<Node> nodes, int order, bool reducedIntegration)
        {
            this._nodes = nodes;
            this._order = order;
            this._reducedIntegration = reducedIntegration;
            this._results = new Dictionary<string, double[]>();
        }

        /***************************************************/

        // Get location of element's primary nodes.
        public override IEnumerable<Point3d> GetVertices()
        {
            yield return this._nodes[0].Location;
            yield return this._nodes[this._nodes.Count - 1].Location;
        }

        /***************************************************/

        // Convert element information to CCX format.
        public override List<string> ToCCX()
        {
            string s = this.CCXId() + ",";
            foreach (Node n in this._nodes)
            {
                s += (n.CCXId() + ",");
            }
            s = s.TrimEnd(',');
            return new List<string> { s };
        }

        /***************************************************/

        // Convert the element type to CCX format.
        public override string CCXType()
        {
            string CCXFormat = "B";

            if (this._order == 1)
                CCXFormat += "31";
            else if (this._order == 2)
                CCXFormat += "32";
            else
                throw new Exception("Unknown element type! Make sure the element is 1st or 2nd order beam element.");

            if (this._reducedIntegration)
                CCXFormat += "R";

            return CCXFormat;
        }

        /***************************************************/

        // Get the surfaces in which section forces are to be calculated
        public string[] CCXSectionResults()
        {
            string[] CCXFormat = new string[8];
            string SName = "S" + this.CCXId();
            CCXFormat[0] = "*SURFACE,NAME = " + SName + "S";
            CCXFormat[1] = this.CCXId() + ",S6";
            CCXFormat[2] = "*SURFACE,NAME = " + SName + "E";
            CCXFormat[3] = this.CCXId() + ",S4";
            CCXFormat[4] = "*SECTION PRINT,SURFACE = " + SName + "S,NAME = " + SName + "S";
            CCXFormat[5] = "SOF";
            CCXFormat[6] = "*SECTION PRINT,SURFACE = " + SName + "E,NAME = " + SName + "E";
            CCXFormat[7] = "SOF";
            return CCXFormat;
        }

        /***************************************************/

        //Clone.
        public override GuanacoObject Clone(bool newID = false)
        {
            Element1D e = this.ShallowClone(newID) as Element1D;
            e._nodes = this._nodes.Select(n => n.Clone(newID) as Node).ToList();

            e._results = new Dictionary<string, double[]>(this._results);
            foreach (string r in this._results.Keys)
            {
                e._results[r] = this._results[r].ToArray();
            }

            e._parentCollection = null;
            e._id = null;
            return e;
        }

        // Clone to a target mesh.
        public override GuanacoObject Clone(Mesh targetMesh, bool newID = false)
        {
            Element1D e = this.Clone(newID) as Element1D;
            if (targetMesh != null)
            {
                targetMesh.AddElement(e, this._id.AsInteger);
                e._nodes = this._nodes.Select(n => targetMesh.Nodes[n.Id.AsInteger]).ToList();
            }

            return e;
        }

        /***************************************************/
    }


    /***************************************************/
    /****                 2D element                ****/
    /***************************************************/

    public class Element2D : Element
    {
        /***************************************************/

        // Fields & properties.
        private Vector3d[] _orientation;
        public Vector3d[] Orientation
        {
            get
            {
                return this._orientation;
            }
        }

        public override int Order
        {
            get
            {
                return this._order;
            }
            set
            {
                if (value == 1 || value == 2)
                    this._order = value;
                else
                    throw new Exception("2D elements can be order 1 or 2. Change order of the element.");
            }
        }

        private int _primaryNodeCount;
        public int PrimaryNodeCount
        {
            get
            {
                return this._primaryNodeCount;
            }
        }

        private double _pressure;
        public double Pressure
        {
            get
            {
                return this._pressure;
            }
        }

        private Element2DType _feType;
        public Element2DType FEType
        {
            get
            {
                return this._feType;
            }
            set
            {
                this._feType = value;
            }
        }

        private bool _composite;
        public bool Composite
        {
            get
            {
                return this._composite;
            }
        }

        /***************************************************/

        // Constructors.
        public Element2D(List<Node> nodes, int order, bool reducedIntegration, bool composite = false)
        {
            this._nodes = nodes;
            this._order = order;
            this._primaryNodeCount = nodes.Count / order;
            this._feType = Element2DType.Unknown;
            this._reducedIntegration = reducedIntegration;
            this._composite = composite;
            this._pressure = 0;
            this._orientation = new Vector3d[] { new Vector3d(1, 0, 0), new Vector3d(0, 1, 0) };
            this._results = new Dictionary<string, double[]>();
        }

        /***************************************************/

        // Set orientation of the element.
        public void SetOrientation(Vector3d[] orientation)
        {
            this._orientation = orientation;
        }

        /***************************************************/

        // Add pressure to the element.
        public void AddPressure(double pressure)
        {
            this._pressure += pressure;
        }

        /***************************************************/

        // Get location of element's primary nodes.
        public override IEnumerable<Point3d> GetVertices()
        {
            for (int i = 0; i < PrimaryNodeCount; i++)
            {
                yield return this._nodes[i].Location;
            }
        }

        /***************************************************/

        // Get centroid of an element.
        public Point3d GetCentroid()
        {
            return GeometryUtil.Average(this.GetVertices());
        }

        /***************************************************/

        // Get normal of the element acc. to convention used in CalculiX.
        public Vector3d GetNormal(bool unitize = true)
        {
            Vector3d v1 = this._nodes[0].Location - this._nodes[1].Location;
            Vector3d v2 = this._nodes[2].Location - this._nodes[1].Location;
            Vector3d CP = Vector3d.CrossProduct(v2, v1);

            if (unitize)
                CP.Unitize();

            return CP;
        }

        /***************************************************/

        // Flip normal of the element.
        public void FlipNormal()
        {
            if (Order == 1)
                this._nodes.Reverse();
            else
            {
                List<Node> stNodes, ndNodes;
                stNodes = this._nodes.GetRange(0, PrimaryNodeCount);
                stNodes.Reverse();
                ndNodes = this._nodes.GetRange(PrimaryNodeCount, PrimaryNodeCount);
                this._nodes = stNodes.Concat(ndNodes).ToList();
            }

            this._pressure *= -1;
        }

        /***************************************************/

        // Convert the element type to CCX format.
        public override string CCXType()
        {
            string CCXFormat = string.Empty;
            switch (this._feType)
            {
                case Element2DType.Shell:
                    {
                        CCXFormat = "S";
                        break;
                    }
                case Element2DType.Membrane:
                    {
                        CCXFormat = "M3D";
                        break;
                    }
                case Element2DType.PlaneStress:
                    {
                        CCXFormat = "CPS";
                        break;
                    }
                case Element2DType.PlaneStrain:
                    {
                        CCXFormat = "CPE";
                        break;
                    }
            }

            if (this._primaryNodeCount == 3 && this._order == 1)
                CCXFormat += "3";
            else if (this._primaryNodeCount == 3 && this._order == 2)
                CCXFormat += "6";
            else if (this._primaryNodeCount == 4 && this._order == 1 && !this._reducedIntegration)
                CCXFormat += "4";
            else if (this._primaryNodeCount == 4 && this._order == 1 && this._reducedIntegration)
                CCXFormat += "4R";
            else if (this._primaryNodeCount == 4 && this._order == 2 && !this._reducedIntegration)
                CCXFormat += "8";
            else if (this._primaryNodeCount == 4 && this._order == 2 && this._reducedIntegration)
                CCXFormat += "8R";
            else
                throw new Exception("Unknown element type! Make sure the element is either tri or quad, 1st or 2nd order.");

            return CCXFormat;
        }

        /***************************************************/

        // Convert element information to CCX format.
        public override List<string> ToCCX()
        {
            List<string> ss = new List<string>();
            string s = this.CCXId() + ",";

            foreach (List<int> chnk in GuanacoUtil.Chunks(this._nodes.Select(n => n.Id.AsInteger).ToList(), 15))
            {
                foreach (int n in chnk)
                {
                    s += ((n + 1).ToString() + ",");
                }
                ss.Add(s);
                s = string.Empty;
            }

            ss[ss.Count - 1] = ss.Last().TrimEnd(',');
            return ss;
        }

        /***************************************************/

        // Convert the element orientation to CCX format.
        public string[] OrientationToCCX()
        {
            if (this._orientation == null)
                return new string[0];

            string[] ss = new string[2];
            {
                ss[0] = "*ORIENTATION,NAME=OR" + this.CCXId();
                ss[1] = (this._orientation[0].ToString() + "," + this._orientation[1].ToString()).Replace("}", "").Replace("{", "");
            }

            return ss;
        }

        /***************************************************/

        // Save pressure to CCX format.
        public string PressureToCCX()
        {
            return this.CCXId() + ",P," + this._pressure.ToString(GuanacoUtil.Invariant);
        }

        /***************************************************/

        //Clone.
        public override GuanacoObject Clone(bool newID = false)
        {
            Element2D e = this.ShallowClone(newID) as Element2D;
            e._nodes = this._nodes.Select(n => n.Clone(newID) as Node).ToList();
            e._orientation = new Vector3d[] { new Vector3d(this._orientation[0]), new Vector3d(this._orientation[1]) };
            e._results = new Dictionary<string, double[]>(this._results);
            foreach (string r in this._results.Keys)
            {
                e._results[r] = this._results[r].ToArray();
            }
            e._parentCollection = null;
            e._id = null;
            return e;
        }

        // Clone to a target mesh.
        public override GuanacoObject Clone(Mesh targetMesh, bool newID = false)
        {
            Element2D e = this.Clone(newID) as Element2D;
            if (targetMesh != null)
            {
                targetMesh.AddElement(e, this._id.AsInteger);
                e._nodes = this._nodes.Select(n => targetMesh.Nodes[n.Id.AsInteger]).ToList();
            }

            return e;
        }

        /***************************************************/

        // Element type enum.
        public enum Element2DType
        {
            Shell,
            Membrane,
            PlaneStress,
            PlaneStrain,
            Unknown
        }

        /***************************************************/

        // Element shape.
        public enum Element2DShape
        {
            Tri,
            Quad
        }

        /***************************************************/
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Rhino.Geometry;

namespace Guanaco
{
    /***************************************************/
    /****            Generic element class          ****/
    /***************************************************/

    public abstract class Element : GuanacoObject
    {
        /***************************************************/

        // Fields & properties.
        public abstract int Id { get; }
        public abstract List<int> Nodes { get; }
        public abstract bool ReducedIntegration { get; set; }
        public abstract Dictionary<string, double[]> Results { get; }
        public abstract int Order { get; set; }

        Mesh _parentMesh;
        public virtual Mesh ParentMesh
        {
            get
            {
                return this._parentMesh;
            }
        }

        BuildingComponent _parentComponent;
        public virtual BuildingComponent ParentComponent
        {
            get
            {
                return this._parentComponent;
            }
        }

        /***************************************************/

        // Convert Id to CCX Id (zero is not allowed).
        public string CCXId()
        {
            return (this.Id + 1).ToString();
        }

        /***************************************************/

        // Methods to be implemented.
        public abstract List<string> ToCCX();
        public abstract string CCXType();
        public abstract IEnumerable<Point3d> GetVertices();

        public virtual double[] GetResults(string resultType)
        {
            return Results.ContainsKey(resultType) ? Results[resultType] : null;
        }

        public virtual void SetParentMesh(Mesh parentMesh)
        {
            this._parentMesh = parentMesh;
        }

        public virtual void SetParentComponent(BuildingComponent parentComponent)
        {
            this._parentComponent = parentComponent;
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
        int _id;
        public override int Id
        {
            get
            {
                return this._id;
            }
        }

        List<int> _nodes;
        public override List<int> Nodes
        {
            get
            {
                return this._nodes;
            }
        }

        bool _reducedIntegration;
        public override bool ReducedIntegration
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

        Dictionary<string, double[]> _results;
        public override Dictionary<string, double[]> Results
        {
            get
            {
                return this._results;
            }
        }

        int _order;
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
        public Element1D(Mesh parentMesh, int id, List<int> nodes, int order, bool reducedIntegration)
        {
            this.SetParentMesh(parentMesh);
            this._id = id;
            this._nodes = nodes;
            this._order = order;
            this._reducedIntegration = reducedIntegration;
            this._results = new Dictionary<string, double[]>();
        }

        /***************************************************/

        // Get location of element's primary nodes.
        public override IEnumerable<Point3d> GetVertices()
        {
            yield return this.ParentMesh.Nodes[Nodes[0]].Location;
            yield return this.ParentMesh.Nodes[Nodes[Nodes.Count - 1]].Location;
        }

        /***************************************************/

        // Convert element information to CCX format.
        public override List<string> ToCCX()
        {
            string s = this.CCXId() + ",";
            foreach (int n in this._nodes)
            {
                s += ((n + 1).ToString() + ",");
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

        // Clone.
        public override GuanacoObject Clone(bool newID = false)
        {
            Element1D e = this.ShallowClone(newID) as Element1D;
            e._nodes = new List<int>(this._nodes);
            e._results = new Dictionary<string, double[]>(this._results);
            foreach (string r in this._results.Keys)
            {
                e.Results[r] = this._results[r].ToArray();
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
        int _id;
        public override int Id
        {
            get
            {
                return this._id;
            }
        }

        List<int> _nodes;
        public override List<int> Nodes
        {
            get
            {
                return this._nodes;
            }
        }

        bool _reducedIntegration;
        public override bool ReducedIntegration
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

        Dictionary<string, double[]> _results;
        public override Dictionary<string, double[]> Results
        {
            get
            {
                return this._results;
            }
        }

        Vector3d[] _orientation;
        public Vector3d[] Orientation
        {
            get
            {
                return this._orientation;
            }
        }

        int _order;
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

        int _primaryNodeCount;
        public int PrimaryNodeCount
        {
            get
            {
                return this._primaryNodeCount;
            }
        }

        double _pressure;
        public double Pressure
        {
            get
            {
                return this._pressure;
            }
        }

        Element2DType _feType;
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

        bool _composite;
        public bool Composite
        {
            get
            {
                return this._composite;
            }
        }

        /***************************************************/

        // Constructors.
        public Element2D(Mesh parentMesh, int id, List<int> nodes, int order, bool reducedIntegration, bool composite = false)
        {
            this.SetParentMesh(parentMesh);
            this._id = id;
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
                yield return this.ParentMesh.Nodes[Nodes[i]].Location;
            }
        }

        /***************************************************/

        // Get centroid of an element.
        public Point3d GetCentroid()
        {
            List<Point3d> vertices = new List<Point3d>();
            for (int i = 0; i < this._primaryNodeCount; i++)
            {
                vertices.Add(this.ParentMesh.Nodes[Nodes[i]].Location);
            }
            return GeometryUtil.Average(vertices);
        }

        /***************************************************/

        // Get normal of the element acc. to convention used in CalculiX.
        public Vector3d GetNormal(bool unitize = true)
        {
            Vector3d v1 = this.ParentMesh.Nodes[Nodes[0]].Location - this.ParentMesh.Nodes[Nodes[1]].Location;
            Vector3d v2 = this.ParentMesh.Nodes[Nodes[2]].Location - this.ParentMesh.Nodes[Nodes[1]].Location;
            Vector3d CP = Vector3d.CrossProduct(v2, v1);
            if (unitize) CP.Unitize();
            return CP;
        }

        /***************************************************/

        // Flip normal of the element.
        public void FlipNormal()
        {
            if (Order == 1)
            {
                this._nodes.Reverse();
            }
            else
            {
                List<int> stNodes, ndNodes;
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

            foreach (List<int> chnk in GuanacoUtil.Chunks(this._nodes, 15))
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
            if (this._orientation == null) return new string[0];
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

        // Clone.
        public override GuanacoObject Clone(bool newID = false)
        {
            Element2D e = this.ShallowClone(newID) as Element2D;
            e._nodes = new List<int>(this._nodes);
            e._orientation = new Vector3d[] { new Vector3d(this._orientation[0]), new Vector3d(this._orientation[1]) };
            e._results = new Dictionary<string, double[]>(this._results);
            foreach (string r in this._results.Keys)
            {
                e.Results[r] = this._results[r].ToArray();
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

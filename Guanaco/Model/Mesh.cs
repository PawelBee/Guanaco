using System;
using System.Collections.Generic;
using System.Linq;
using Rhino.Geometry;

namespace Guanaco
{
    /***************************************************/
    /****                   Mesh                    ****/
    /***************************************************/

    public class Mesh : GuanacoObject
    {
        /***************************************************/

        // Fields & properties.
        List<Node> _nodes;
        public List<Node> Nodes
        {
            get
            {
                return this._nodes;
            }
        }

        List<Element> _elements;
        public List<Element> Elements
        {
            get
            {
                return this._elements;
            }
        }

        /***************************************************/

        // Constructors.
        public Mesh()
        {
            this._nodes = new List<Node>();
            this._elements = new List<Element>();
        }

        /***************************************************/

        // Apply load to mesh components (nodes/elements) within tolerance.
        public void ApplyLoad(Load load, double tolerance)
        {
            // Apply point load to the closest point that lies within tolerance.
            if (load is NodalForce)
            {
                NodalForce nf = load as NodalForce;
                int loadId = -1;
                double minDist = tolerance;
                foreach (Node n in this._nodes)
                {
                    if (n.Primary)
                    {
                        double dist = n.Location.DistanceTo(nf.Point);
                        if (dist <= minDist)
                        {
                            minDist = dist;
                            loadId = n.Id;
                        }
                    }
                }
                if (loadId >= 0)
                {
                    foreach (Node n in this._nodes)
                    {
                        if (n.Id == loadId)
                        {
                            n.AddForceLoad(nf.LoadValue);
                            break;
                        }
                    }
                }
            }

            // Apply moment to the closest point that lies within tolerance.
            else if (load is NodalMoment)
            {
                NodalMoment nm = load as NodalMoment;
                int loadId = -1;
                double minDist = tolerance;
                foreach (Node n in this._nodes)
                {
                    if (n.Primary)
                    {
                        double dist = n.Location.DistanceTo(nm.Point);
                        if (dist <= minDist)
                        {
                            minDist = dist;
                            loadId = n.Id;
                        }
                    }
                }
                if (loadId >= 0)
                {
                    foreach (Node n in this._nodes)
                    {
                        if (n.Id == loadId)
                        {
                            n.AddMomentLoad(nm.Axis * nm.LoadValue);
                            break;
                        }
                    }
                }
            }

            // Apply infill load to the adjoining elements.
            else if (load is InfillLoad)
            {
                InfillLoad il = load as InfillLoad;
                foreach (Element e in this._elements)
                {
                    // Check if the element is 2D.
                    if (!(e is Element2D)) continue;
                    Element2D el = e as Element2D;

                    Point3d elC = el.GetCentroid();
                    Vector3d elN = el.GetNormal();
                    foreach (Infill i in il.Infills)
                    {
                        // Check if the element is adjoining to the infill (if all its vertices lie within tolerance).
                        bool broken = false;
                        foreach (Point3d v in el.GetVertices())
                        {
                            if (v.DistanceTo(i.Volume.ClosestPoint(v)) > tolerance)
                            {
                                broken = true;
                                break;
                            }
                        }

                        // If the element is adjoining to the infill, apply the load based on location of the element and load function of the infill.
                        if (!broken)
                        {
                            // Flip normal of the element if it points outside the infill.
                            Point3d cpt = Point3d.Add(elC, elN * tolerance);
                            if (!i.Volume.IsPointInside(cpt, Rhino.RhinoMath.SqrtEpsilon, true))
                            {
                                el.FlipNormal();
                            }

                            // Check if the element is not surrounded by the infill from both sides - if it is then do nothing (no hydrostatic pressure).
                            else
                            {
                                cpt = Point3d.Add(elC, -elN * tolerance);
                                if (i.Volume.IsPointInside(cpt, Rhino.RhinoMath.SqrtEpsilon, true))
                                {
                                    continue;
                                }
                            }

                            // Apply the load based on location of the element and load function of the infill.
                            string g = il.InfillDensity.ToString(GuanacoUtil.Invariant);
                            string x = (i.maxZ - elC.Z).ToString(GuanacoUtil.Invariant);
                            string z = i.maxZ.ToString(GuanacoUtil.Invariant);
                            string f = il.LoadFunction.Replace("g", g).Replace("x", x).Replace("z", z);
                            double p = GuanacoUtil.Evaluate(f);
                            el.AddPressure(-p);
                        }
                    }
                }
            }

            // Apply pressure load to the elements laying within tolerance from the pressure area.
            else if (load is PressureLoad)
            {
                PressureLoad pl = load as PressureLoad;
                foreach (Element e in this._elements)
                {
                    // Check if the element is 2D.
                    if (!(e is Element2D)) continue;
                    Element2D el = e as Element2D;

                    // Check if the element is adjoining to the pressure area (if all its vertices lie within tolerance) - if yes then apply the load.
                    Point3d elC = el.GetCentroid();
                    foreach (Brep s in pl.Surfaces)
                    {
                        bool broken = false;
                        foreach (Point3d v in el.GetVertices())
                        {
                            if (v.DistanceTo(s.ClosestPoint(v)) > tolerance)
                            {
                                broken = true;
                                break;
                            }
                        }
                        if (!broken)
                        {
                            el.AddPressure(pl.LoadValue * 1);
                        }
                    }
                }
            }
        }

        /***************************************************/

        // Apply support to the nodes laying within tolerance from the supporting geometry.
        public void ApplySupport(Support s, double tolerance)
        {
            foreach (GeometryBase g in s.Geometry)
            {
                if (g is Point)
                {
                    Point p = g as Point;
                    Point3d pt = p.Location;
                    foreach (Node node in this._nodes)
                    {
                        if (node.Primary && node.Location.DistanceTo(pt) <= tolerance)
                        {
                            s.Nodes.Add(node.Id);
                        }
                    }
                }
                else if (g is Curve)
                {
                    Curve c = g as Curve;
                    double t;
                    foreach (Node node in this._nodes)
                    {
                        if (node.Primary)
                        {
                            c.ClosestPoint(node.Location, out t);
                            if (c.PointAt(t).DistanceTo(node.Location) <= tolerance)
                            {
                                s.Nodes.Add(node.Id);
                            }
                        }
                    }
                }
                else if (g is Brep)
                {
                    Brep b = g as Brep;
                    foreach (Node node in this._nodes)
                    {
                        if (node.Primary)
                        {
                            Point3d bpt = b.ClosestPoint(node.Location);
                            if (node.Location.DistanceTo(bpt) <= tolerance)
                            {
                                s.Nodes.Add(node.Id);
                            }
                        }
                    }
                }
                else
                {
                    throw new Exception("Unknown support geometry type! " + s.Name);
                }
            }

            // If the physical fixing of the support is chosen, then instead of fixing rotational stiffness within chosen nodes, fix translation in all nodes of the supported elements.
            if (s.SupportType.Physical)
            {
                List<int> baseNodes = s.Nodes.ToList();
                foreach (Element e in this._elements)
                {
                    if (!(e is Element2D)) continue;
                    Element2D el = e as Element2D;

                    HashSet<int> ids = new HashSet<int>(el.Nodes.GetRange(0, el.PrimaryNodeCount));
                    foreach (int node in baseNodes)
                    {
                        if (ids.Any(id => id == node))
                        {
                            s.Nodes.UnionWith(ids);
                            break;
                        }
                    }
                }
            }
        }

        /***************************************************/

        // Get node results stored within each instance of the node class.
        public List<double> GetNodeDisplacement(string resultType)
        {
            List<double> results = new List<double>();
            foreach (Node n in this._nodes)
            {
                switch (resultType)
                {
                    case "dX":
                        results.Add(n.Displacement.X);
                        break;
                    case "dY":
                        results.Add(n.Displacement.Y);
                        break;
                    case "dZ":
                        results.Add(n.Displacement.Z);
                        break;
                    case "dTotal":
                        results.Add(n.Displacement.Length);
                        break;
                    default:
                        results.Add(double.NaN);
                        break;
                }
            }
            return results;
        }

        /***************************************************/

        // Retrieve the results that are stored in elements.
        public Dictionary<int, double[]> GetElementResults(string resultType)
        {
            Dictionary<int, double[]> results = new Dictionary<int, double[]>();
            for (int i = 0; i < this._elements.Count; i++)
            {
                double[] result = this._elements[i].GetResults(resultType);
                if (result != null) results.Add(i, result);
            }
            return results;
        }

        /***************************************************/

        // Deform the mesh.
        public void Deform(double factor = 1.0)
        {
            foreach (Node n in this._nodes)
            {
                n.SetLocation(n.Location + n.Displacement * factor);
            }
        }

        /***************************************************/

        // Write topology information to CCX format.
        public List<string> ToCCX()
        {
            List<string> CCXFormat = new List<string>();

            // Write node information.
            CCXFormat.Add("*NODE, NSET = Nall");
            foreach (Node n in this._nodes)
            {
                CCXFormat.Add(n.ToCCX());
            }

            // Order elements by their type.
            List<Tuple<string, List<int>>> elementsByTypes = new List<Tuple<string, List<int>>>();
            for (int i = 0; i < this._elements.Count; i++)
            {
                Element e = this._elements[i];
                string elementType = e.CCXType();
                bool newGroup = true;
                foreach (Tuple<string, List<int>> t in elementsByTypes)
                {
                    if (t.Item1 == elementType)
                    {
                        newGroup = false;
                        t.Item2.Add(i);
                        break;
                    }
                }
                if (newGroup) elementsByTypes.Add(new Tuple<string, List<int>>(elementType, new List<int> { i }));
            }

            // Write elements.
            foreach (Tuple<string, List<int>> element in elementsByTypes)
            {
                string componentType = this._elements[element.Item2[0]] is Element1D ? "BARS" : "PANELS";
                CCXFormat.Add("*ELEMENT, TYPE = " + element.Item1 + ", ELSET = " + componentType);
                foreach (int i in element.Item2)
                {
                    CCXFormat.AddRange(this._elements[i].ToCCX());
                }
            }

            return CCXFormat;
        }

        /***************************************************/

        // Clone.
        public override GuanacoObject Clone(bool newID = false)
        {
            Mesh m = this.ShallowClone(newID) as Mesh;
            m._nodes = new List<Node>(this._nodes);
            m._elements = new List<Element>(this._elements);
            for (int i = 0; i < m.Nodes.Count; i++)
            {
                Node n = m.Nodes[i].Clone(newID) as Node;
                n.SetParentMesh(m);
                m.Nodes[i] = n;
            }
            for (int i = 0; i < m.Elements.Count; i++)
            {
                Element e = m.Elements[i].Clone(newID) as Element;
                e.SetParentMesh(m);
                m.Elements[i] = e;
            }
            return m;
        }

        /***************************************************/
    }
}

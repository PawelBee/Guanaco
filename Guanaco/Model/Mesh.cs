using System;
using System.Collections.Generic;
using System.Linq;

namespace Guanaco
{
    /***************************************************/
    /****                   Mesh                    ****/
    /***************************************************/

    public class Mesh : GuanacoObject
    {
        /***************************************************/

        // Fields & properties.
        private IndexedCollection<Node> _nodes;
        public IndexedCollection<Node> Nodes
        {
            get
            {
                return this._nodes;
            }
        }

        private IndexedCollection<Element> _elements;
        public IndexedCollection<Element> Elements
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
            this._nodes = new IndexedCollection<Node>();
            this._elements = new IndexedCollection<Element>();
        }

        /***************************************************/

        // Add a node to mesh.
        public void AddNode(Node node, int id = -1)
        {
            this._nodes.Add(node, id);
        }

        /***************************************************/

        // Add an element to mesh.
        public void AddElement(Element element, int id = -1)
        {
            this._elements.Add(element, id);
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
                if (result != null)
                    results.Add(i, result);
            }

            return results;
        }

        /***************************************************/

        // Deform the mesh.
        public void Deform(double factor = 1.0)
        {
            foreach (Node n in this._nodes)
            {
                n.Location += n.Displacement * factor;
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

                if (newGroup)
                    elementsByTypes.Add(new Tuple<string, List<int>>(elementType, new List<int> { i }));
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
            m._nodes = new IndexedCollection<Node>();
            m._elements = new IndexedCollection<Element>();
            foreach (Node node in this._nodes)
            {
                Node n = node.Clone(m, newID) as Node;
            }
            foreach (Element element in this._elements)
            {
                Element e = element.Clone(m, newID) as Element;
            }
            return m;
        }

        /***************************************************/
    }
}

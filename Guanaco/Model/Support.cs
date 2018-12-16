using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Guanaco
{
    /***************************************************/
    /****                 Support                   ****/
    /***************************************************/

    public class Support : GuanacoObject
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

        private List<GeometryBase> _geometry;
        public IEnumerable<GeometryBase> Geometry
        {
            get
            {
                return this._geometry;
            }
        }

        private SupportType _supportType;
        public SupportType SupportType
        {
            get
            {
                return this._supportType;
            }
        }

        private HashSet<Node> _nodes;
        public HashSet<Node> Nodes
        {
            get
            {
                return this._nodes;
            }
        }

        /***************************************************/

        // Constructors - format conforming to CalculiX convention.
        public Support(List<GeometryBase> geometry, SupportType support_Type, string name)
        {
            this._name = name;
            this._geometry = geometry;
            this._supportType = support_Type;
            this._nodes = new HashSet<Node>();
        }

        /***************************************************/

        // Add nodes to the support definition.
        public void AddNodes(IEnumerable<Node> nodes)
        {
            this._nodes.UnionWith(nodes);
        }

        /***************************************************/

        // Convert support information to CCX format.
        public List<string> ToCCX()
        {
            List<string> CCXFormat = new List<string>();
            CCXFormat.Add("*NSET,NSET=Support_" + this._name);

            CCXFormat.AddRange(GuanacoUtil.IntsToCCX(this._nodes.Select(n => n.Id.AsInteger).ToList(), true));

            CCXFormat.Add("*BOUNDARY");

            foreach (int dof in this._supportType.DOFs)
            {
                CCXFormat.Add("Support_" + this._name + ", " + dof.ToString(GuanacoUtil.Invariant));
            }

            return CCXFormat;
        }

        /***************************************************/

        // Clone.
        public override GuanacoObject Clone(bool newID = false)
        {
            Support s = this.ShallowClone(newID) as Support;
            s._geometry = new List<GeometryBase>(this._geometry);
            for (int i = 0; i < s._geometry.Count; i++)
            {
                GeometryBase g = s._geometry[i].Duplicate();
                s._geometry[i] = g;
            }
            s._supportType = this._supportType.Clone(newID) as SupportType;
            s._nodes = new HashSet<Node>(this._nodes);
            return s;
        }
    }


    /***************************************************/
    /****               Support type                ****/
    /***************************************************/

    public class SupportType: GuanacoObject
    {
        /***************************************************/

        // Fields & properties.
        private List<int> _dofs;
        public List<int> DOFs
        {
            get
            {
                return this._dofs;
            }
        }

        private bool _physical;
        public bool Physical
        {
            get
            {
                return this._physical;
            }
        }

        /***************************************************/

        // Constructors.
        public SupportType(List<int> dofs, bool physical = false)
        {
            this.SetDOFs(dofs, physical);
        }

        /***************************************************/

        // Set the degrees of freedom blocked by the support.
        private void SetDOFs(List<int> dofs, bool physical = false)
        {
            if (physical)
            {
                if (dofs.Count != 3 || !dofs.Contains(1) || !dofs.Contains(2) || !dofs.Contains(3))
                    throw new Exception("The physical fixed support is possible only when DOFs 1, 2, 3 are fixed.");

                this._physical = true;
            }
            this._dofs = dofs;
        }

        /***************************************************/

        // Clone.
        public override GuanacoObject Clone(bool newID = false)
        {
            SupportType st = this.ShallowClone(newID) as SupportType;
            st.SetDOFs(new List<int>(this._dofs), this._physical);
            return st;
        }

        /***************************************************/
    }
}

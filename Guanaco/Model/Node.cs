using Rhino.Geometry;
using System.Collections.Generic;

namespace Guanaco
{
    /***************************************************/
    /****                   Node                    ****/
    /***************************************************/

    public class Node : GuanacoIndexable
    {
        /***************************************************/

        // Fields & properties.
        private Point3d _location;
        public Point3d Location
        {
            get
            {
                return this._location;
            }
            set
            {
                this._location = value;
            }
        }

        private bool _primary;
        public bool Primary
        {
            get
            {
                return this._primary;
            }
            set
            {
                this._primary = value;
            }
        }

        private Vector3d _forceLoad;
        public Vector3d ForceLoad
        {
            get
            {
                return this._forceLoad;
            }
        }

        private Vector3d _momentLoad;
        public Vector3d MomentLoad
        {
            get
            {
                return this._momentLoad;
            }
        }

        private Vector3d _displacement;
        public Vector3d Displacement
        {
            get
            {
                return this._displacement;
            }
        }

        /***************************************************/

        // Constructors.
        public Node()
        {
            this._primary = false;
            this._forceLoad = new Vector3d();
            this._momentLoad = new Vector3d();
            this._displacement = new Vector3d();
        }

        public Node(Point3d location): this()
        {
            this._location = location;
        }

        public Node(double x, double y, double z) : this()
        {
            this._location = new Point3d(x, y, z);
        }

        /***************************************************/

        // Set displacement of the node.
        public void ResetDisplacement()
        {
            this._displacement = new Vector3d();
        }

        /***************************************************/

        // Add displacement to the node.
        public void AddDisplacement(Vector3d displacement)
        {
            this._displacement += displacement;
        }

        /***************************************************/

        // Add force load to the node.
        public void AddForceLoad(Vector3d forceLoad)
        {
            this._forceLoad += forceLoad;
        }

        /***************************************************/

        // Add moment load to the node.
        public void AddMomentLoad(Vector3d momentLoad)
        {
            this._momentLoad += momentLoad;
        }

        /***************************************************/

        // Convert the node basic info and location to CCX format.
        public string ToCCX()
        {
            string s = this.CCXId();
            int i = 0;
            while (i < 3)
            {
                s += ("," + this._location[i].ToString(GuanacoUtil.Invariant));
                i++;
            }
            return s;
        }

        /***************************************************/

        // Convert the node load to CCX format.
        public List<string> LoadToCCX()
        {
            List<string> CCXFormat = new List<string>();
            if (this._forceLoad.X != 0)
                CCXFormat.Add(this.CCXId() + ",1," + this._forceLoad.X.ToString(GuanacoUtil.Invariant));
            if (this._forceLoad.Y != 0)
                CCXFormat.Add(this.CCXId() + ",2," + this._forceLoad.Y.ToString(GuanacoUtil.Invariant));
            if (this._forceLoad.Z != 0)
                CCXFormat.Add(this.CCXId() + ",3," + this._forceLoad.Z.ToString(GuanacoUtil.Invariant));
            if (this._momentLoad.X != 0)
                CCXFormat.Add(this.CCXId() + ",4," + this._momentLoad.X.ToString(GuanacoUtil.Invariant));
            if (this._momentLoad.Y != 0)
                CCXFormat.Add(this.CCXId() + ",5," + this._momentLoad.Y.ToString(GuanacoUtil.Invariant));
            if (this._momentLoad.Z != 0)
                CCXFormat.Add(this.CCXId() + ",6," + this._momentLoad.Z.ToString(GuanacoUtil.Invariant));
            return CCXFormat;
        }

        /***************************************************/

        // Clone.
        public override GuanacoObject Clone(bool newID = false)
        {
            Node n = this.ShallowClone(newID) as Node;
            n._location = new Point3d(this._location);
            n._displacement = new Vector3d(this._displacement);
            n._forceLoad = new Vector3d(this._forceLoad);
            n._momentLoad = new Vector3d(this._momentLoad);
            n._parentCollection = null;
            n._id = null;
            return n;
        }

        // Clone to a target mesh.
        public GuanacoObject Clone(Mesh targetMesh, bool newID = false)
        {
            Node n = this.Clone(newID) as Node;
            if (targetMesh != null)
                targetMesh.AddNode(n, this._id.AsInteger);

            return n;
        }

        /***************************************************/
    }
}

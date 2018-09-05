using System;
using System.Collections.Generic;

namespace Guanaco
{
    /***************************************************/
    /****           Generic material class          ****/
    /***************************************************/

    public abstract class Material : GuanacoObject, ILibraryObject
    {
        /***************************************************/

        // Properties.
        public abstract string Name { get; }
        public abstract double Density { get; }
        
        /***************************************************/

        // Methods to be implemented.
        public abstract List<string> ToCCX();

        /***************************************************/
    }


    /***************************************************/
    /****          Isotropic material class         ****/
    /***************************************************/

    public class MaterialIsotropic : Material
    {
        /***************************************************/

        // Fields & properties.
        string _name;
        public override string Name
        {
            get
            {
                return this._name;
            }
        }
        
        double _density;
        public override double Density
        {
            get
            {
                return this._density;
            }
        }
        
        double _e;
        public double E
        {
            get
            {
                return this._e;
            }
        }
        
        double _v;
        public double v
        {
            get
            {
                return this._v;
            }
        }

        /***************************************************/

        // Constructors.
        public MaterialIsotropic(string name, double young, double poisson, double density)
        {
            this._name = name;
            this._e = young;
            this._v = poisson;
            this._density = density;
        }

        /***************************************************/

        // Convert the material information to CCX format.
        public override List<string> ToCCX()
        {
            List<string> CCXformat = new List<string>();
            CCXformat.Add("*MATERIAL, NAME = " + this._name);
            if (this._density != 0)
            {
                CCXformat.Add("*DENSITY");
                CCXformat.Add(this._density.ToString(GuanacoUtil.Invariant));
            }
            CCXformat.Add("*ELASTIC,TYPE = ISO");
            CCXformat.Add(String.Format("{0},{1}", this._e, this._v));
            return CCXformat;
        }

        /***************************************************/
    }


    /***************************************************/
    /****         Orthotropic material class        ****/
    /***************************************************/

    public class MaterialEngineeringConstants : Material
    {
        /***************************************************/

        // Fields & properties.
        string _name;
        public override string Name
        {
            get
            {
                return this._name;
            }
        }

        double _density;
        public override double Density
        {
            get
            {
                return this._density;
            }
        }

        double _e1;
        public double E1
        {
            get
            {
                return this._e1;
            }
        }

        double _e2;
        public double E2
        {
            get
            {
                return this._e2;
            }
        }

        double _e3;
        public double E3
        {
            get
            {
                return this._e3;
            }
        }

        double _v12;
        public double v12
        {
            get
            {
                return this._v12;
            }
        }

        double _v13;
        public double v13
        {
            get
            {
                return this._v13;
            }
        }

        double _v23;
        public double v23
        {
            get
            {
                return this._v23;
            }
        }

        double _g12;
        public double G12
        {
            get
            {
                return this._g12;
            }
        }

        double _g13;
        public double G13
        {
            get
            {
                return this._g13;
            }
        }

        double _g23;
        public double G23
        {
            get
            {
                return this._g23;
            }
        }

        /***************************************************/

        // Constructors.
        public MaterialEngineeringConstants(string name, double young1, double young2, double young3, double poisson12, double poisson13, double poisson23, double shear12, double shear13, double shear23, double density)
        {
            this._name = name;
            this._e1 = young1;
            this._e2 = young2;
            this._e3 = young3;
            this._v12 = poisson12;
            this._v13 = poisson13;
            this._v23 = poisson23;
            this._g12 = shear12;
            this._g13 = shear13;
            this._g23 = shear23;
            this._density = density;
        }
        
        /***************************************************/

        // Convert the material information to CCX format.
        public override List<string> ToCCX()
        {
            List<string> CCXformat = new List<string>();
            CCXformat.Add("* MATERIAL, NAME = " + this._name);
            if (this._density != 0)
            {
                CCXformat.Add("*DENSITY");
                CCXformat.Add(this._density.ToString(GuanacoUtil.Invariant));
            }
            CCXformat.Add("* ELASTIC,TYPE = ENGINEERING CONSTANTS");
            CCXformat.Add(String.Format("{0},{1},{2},{3},{4},{5},{6},{7}", this._e1, this._e2, this._e3, this._v12, this._v13, this._v23, this._g12, this._g13));
            CCXformat.Add(this._g23.ToString());
            return CCXformat;
        }

        /***************************************************/
    }
}

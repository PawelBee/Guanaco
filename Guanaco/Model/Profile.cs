using Rhino.Geometry;
using System;

namespace Guanaco
{
    /***************************************************/
    /****         Generic bar profile class         ****/
    /***************************************************/

    public abstract class Profile : GuanacoObject
    {
        /***************************************************/

        // Properties.
        public virtual string Name { get; }

        /***************************************************/

        // Methods to be implemented.
        public abstract Curve[] ToRhinoProfile();
        public abstract string[] ToCCXFormat();
        public abstract double GetHeight();
        public abstract double GetWidth();

        /***************************************************/
    }


    /***************************************************/
    /****          Rectangular bar profile          ****/
    /***************************************************/

    public class ProfileRectangular : Profile
    {
        /***************************************************/

        // Fields & properties.
        private string _name;
        public override string Name
        {
            get
            {
                return this._name;
            }
        }

        private double _height;
        public double Height
        {
            get
            {
                return this._height;
            }
        }

        private double _width;
        public double Width
        {
            get
            {
                return this._width;
            }
        }

        /***************************************************/

        // Constructors.
        public ProfileRectangular(double height, double width, string name = null)
        {
            this._height = height;
            this._width = width;
            this._name = name != null && name != string.Empty ? name : "RECTANGLE_" + height.ToString(GuanacoUtil.Invariant) + "x" + width.ToString(GuanacoUtil.Invariant);
        }

        /***************************************************/

        // Get height of the profile.
        public override double GetHeight()
        {
            return this._height;
        }

        /***************************************************/

        // Get width of the profile.
        public override double GetWidth()
        {
            return this._width;
        }

        /***************************************************/

        // Get profile curves in Rhino format.
        public override Curve[] ToRhinoProfile()
        {
            Point3d[] cornerPoints = new Point3d[5];
            cornerPoints[0] = cornerPoints[4] = new Point3d(-this._height * 0.5, -this._width * 0.5, 0);
            cornerPoints[1] = new Point3d(-this._height * 0.5, this._width * 0.5, 0);
            cornerPoints[2] = new Point3d(this._height * 0.5, this._width * 0.5, 0);
            cornerPoints[3] = new Point3d(this._height * 0.5, -this._width * 0.5, 0);
            return new Curve[] { new PolylineCurve(cornerPoints) };
        }

        /***************************************************/

        // Convert profile information to CCX format.
        public override string[] ToCCXFormat()
        {
            string[] CCXFormat = new string[3];
            CCXFormat[0] = "*BEAM SECTION";
            CCXFormat[1] = "SECTION=RECT";
            CCXFormat[2] = string.Format("{0},{1}", this._height, this._width);
            return CCXFormat;
        }

        /***************************************************/
    }


    /***************************************************/
    /****             Round bar profile             ****/
    /***************************************************/

    public class ProfileRound : Profile
    {
        /***************************************************/

        // Fields & properties.
        private string _name;
        public override string Name
        {
            get
            {
                return this._name;
            }
        }

        private double _diameter;
        public double Diameter
        {
            get
            {
                return this._diameter;
            }
        }

        /***************************************************/

        // Constructors.
        public ProfileRound(double diameter, string name)
        {
            this._diameter = diameter;
            this._name = name != null && name != string.Empty ? name : "CIRCLE_" + diameter.ToString(GuanacoUtil.Invariant);
        }

        /***************************************************/

        // Get height of the profile.
        public override double GetHeight()
        {
            return this._diameter;
        }

        /***************************************************/

        // Get width of the profile.
        public override double GetWidth()
        {
            return this._diameter;
        }

        /***************************************************/

        // Get profile curves in Rhino format.
        public override Curve[] ToRhinoProfile()
        {
            return new Curve[] { new ArcCurve(new Circle(this._diameter)) };
        }

        /***************************************************/

        // Convert profile information to CCX format.
        public override string[] ToCCXFormat()
        {
            string[] CCXFormat = new string[3];
            CCXFormat[0] = "*BEAM SECTION";
            CCXFormat[1] = "SECTION=CIRC";
            CCXFormat[2] = string.Format("{0},{0}", this._diameter * 0.5);
            return CCXFormat;
        }

        /***************************************************/
    }


    /***************************************************/
    /****            Tubular bar profile            ****/
    /***************************************************/

    public class ProfilePipe : Profile
    {
        /***************************************************/

        // Fields & properties.
        private string _name;
        public override string Name
        {
            get
            {
                return this._name;
            }
        }

        private double _diameter;
        public double Diameter
        {
            get
            {
                return this._diameter;
            }
        }

        private double _thickness;
        public double Thickness
        {
            get
            {
                return this._thickness;
            }
        }

        /***************************************************/

        // Constructors.
        public ProfilePipe(double diameter, double thickness, string name)
        {
            if (thickness * 2 >= diameter)
                throw new Exception("Wall thickness is larger or equal to half of the diameter.");

            this._diameter = diameter;
            this._thickness = thickness;
            this._name = name != null && name != string.Empty ? name : "PIPE_" + diameter.ToString(GuanacoUtil.Invariant) + "x" + thickness.ToString(GuanacoUtil.Invariant);
        }

        /***************************************************/

        // Get height of the profile.
        public override double GetHeight()
        {
            return this._diameter;
        }

        /***************************************************/

        // Get width of the profile.
        public override double GetWidth()
        {
            return this._diameter;
        }

        /***************************************************/

        // Get profile curves in Rhino format.
        public override Curve[] ToRhinoProfile()
        {
            Curve[] curves = new Curve[2];
            curves[0] = new ArcCurve(new Circle(this._diameter * 0.5));
            curves[1] = new ArcCurve(new Circle(this._diameter * 0.5 - this._thickness));
            return curves;
        }

        /***************************************************/

        // Convert profile information to CCX format.
        public override string[] ToCCXFormat()
        {
            string[] CCXFormat = new string[3];
            CCXFormat[0] = "*BEAM GENERAL SECTION";
            CCXFormat[1] = "SECTION=PIPE";
            CCXFormat[2] = string.Format("{0},{1}", this._diameter * 0.5, this._thickness);
            return CCXFormat;
        }

        /***************************************************/
    }


    /***************************************************/
    /****              Box bar profile              ****/
    /***************************************************/

    public class ProfileBox : Profile
    {
        /***************************************************/

        // Fields & properties.
        private string _name;
        public override string Name
        {
            get
            {
                return this._name;
            }
        }

        private double _height;
        public double Height
        {
            get
            {
                return this._height;
            }
        }

        private double _width;
        public double Width
        {
            get
            {
                return this._width;
            }
        }

        private double _thickness;
        public double Thickness
        {
            get
            {
                return this._thickness;
            }
        }

        /***************************************************/

        // Constructors.
        public ProfileBox(double height, double width, double thickness, string name)
        {
            if (thickness * 2 >= Math.Min(height, width))
                throw new Exception("Wall thickness is larger or equal to half of the height/width.");

            this._height = height;
            this._width = width;
            this._thickness = thickness;
            this._name = name != null && name != string.Empty ? name : "BOX_" + height.ToString(GuanacoUtil.Invariant) + "x" + width.ToString(GuanacoUtil.Invariant) + "x" + thickness.ToString(GuanacoUtil.Invariant);
        }

        /***************************************************/

        // Get height of the profile.
        public override double GetHeight()
        {
            return this._height;
        }

        /***************************************************/

        // Get width of the profile.
        public override double GetWidth()
        {
            return this._width;
        }

        /***************************************************/

        // Get profile curves in Rhino format.
        public override Curve[] ToRhinoProfile()
        {
            Curve[] curves = new Curve[2];
            Point3d[] outerCornerPoints = new Point3d[5];
            Point3d[] innerCornerPoints = new Point3d[5];
            outerCornerPoints[0] = outerCornerPoints[4] = new Point3d(-this._height * 0.5, -this._width * 0.5, 0);
            outerCornerPoints[1] = new Point3d(-this._height * 0.5, this._width * 0.5, 0);
            outerCornerPoints[2] = new Point3d(this._height * 0.5, this._width * 0.5, 0);
            outerCornerPoints[3] = new Point3d(this._height * 0.5, -this._width * 0.5, 0);
            innerCornerPoints[0] = innerCornerPoints[4] = new Point3d(-this._height * 0.5 + this._thickness, -this._width * 0.5 + this._thickness, 0);
            innerCornerPoints[1] = new Point3d(-this._height * 0.5 + this._thickness, this._width * 0.5 - this._thickness, 0);
            innerCornerPoints[2] = new Point3d(this._height * 0.5 - this._thickness, this._width * 0.5 - this._thickness, 0);
            innerCornerPoints[3] = new Point3d(this._height * 0.5 - this._thickness, -this._width * 0.5 + this._thickness, 0);
            curves[0] = new PolylineCurve(outerCornerPoints);
            curves[1] = new PolylineCurve(innerCornerPoints);
            return curves;
        }

        /***************************************************/

        // Convert profile information to CCX format.
        public override string[] ToCCXFormat()
        {
            string[] CCXFormat = new string[3];
            CCXFormat[0] = "*BEAM GENERAL SECTION";
            CCXFormat[1] = "SECTION=BOX";
            CCXFormat[2] = string.Format("{0},{1},{2},{2},{2},{2}", this._height, this._width, this._thickness);
            return CCXFormat;
        }

        /***************************************************/
    }
}

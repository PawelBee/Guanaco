using Rhino.Geometry;
using System.Collections.Generic;
using System.Linq;

namespace Guanaco
{
    /***************************************************/
    /****           Geometry utility class          ****/
    /***************************************************/

    public static class GeometryUtil
    {
        /***************************************************/

        // Compute an average of a list of points.
        public static Point3d Average(IEnumerable<Point3d> points)
        {
            double X = 0;
            double Y = 0;
            double Z = 0;
            foreach (Point3d point in points)
            {
                X += point.X;
                Y += point.Y;
                Z += point.Z;
            }

            return new Point3d(X / points.Count(), Y / points.Count(), Z / points.Count());
        }

        /***************************************************/
    }
}

using Rhino.Geometry;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Xml.Serialization;

namespace Guanaco
{
    /***************************************************/
    /****           General utility class           ****/
    /***************************************************/

    public static class GuanacoUtil
    {
        /***************************************************/

        // General variables.
        public static readonly System.Globalization.CultureInfo Invariant = System.Globalization.CultureInfo.InvariantCulture;
        public static readonly System.Globalization.NumberStyles FloatNum = System.Globalization.NumberStyles.Float;
        public static readonly string[] Space = new string[] { " " };
        public static readonly string TempDir = System.IO.Path.GetTempPath();

        /***************************************************/

        // Chunk a list into a list of lists of given length.
        public static List<List<int>> Chunks(List<int> l, int n)
        {
            List<List<int>> list = new List<List<int>>();
            for (int i = 0; i < l.Count; i += n)
            {
                if (l.Count < i + n)
                    list.Add(l.GetRange(i, l.Count - i));
                else
                    list.Add(l.GetRange(i, n));
            }
            return list;
        }

        /***************************************************/

        // Convert list of integers to CCX format.
        public static List<string> IntsToCCX(IEnumerable<int> ints, bool toCCXIds = false)
        {
            List<int> data = new List<int>(ints);
            if (toCCXIds)
            {
                for (int i = 0; i < data.Count; i++)
                {
                    data[i]++;
                }
            }

            List<string> CCXFormat = new List<string>();
            foreach (List<int> chnk in GuanacoUtil.Chunks(data, 16))
            {
                CCXFormat.Add(string.Join(",", chnk) + ",");
            }

            if (CCXFormat.Count != 0)
                CCXFormat[CCXFormat.Count - 1] = CCXFormat.Last().TrimEnd(',');

            return CCXFormat;
        }

        /***************************************************/

        // Convert Rhino vector to a string.
        public static string RhinoVectorToString(Vector3d vector)
        {
            return string.Format("{0},{1},{2}", vector.X, vector.Y, vector.Z);
        }

        /***************************************************/

        // Evaluate string as an equation.
        public static double Evaluate(string expression)
        {
            var loDataTable = new DataTable();
            var loDataColumn = new DataColumn("Eval", typeof(double), expression);
            loDataTable.Columns.Add(loDataColumn);
            loDataTable.Rows.Add(0);
            return (double)(loDataTable.Rows[0]["Eval"]);
        }

        /***************************************************/

        // File types.
        public enum FileType
        {
            inp,
            frd,
            dat,
            geo
        }

        /***************************************************/
        
        // Serialize generic objects.
        public static T Deserialize<T>(this string toDeserialize)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
            using (System.IO.StringReader textReader = new System.IO.StringReader(toDeserialize))
            {
                return (T)xmlSerializer.Deserialize(textReader);
            }
        }

        /***************************************************/

        // Deserialize generic objects.
        public static string Serialize<T>(this T toSerialize)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
            using (System.IO.StringWriter textWriter = new System.IO.StringWriter())
            {
                xmlSerializer.Serialize(textWriter, toSerialize);
                return textWriter.ToString();
            }
        }

        /***************************************************/
    }
}

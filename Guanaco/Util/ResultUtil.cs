using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Rhino.Geometry;

namespace Guanaco
{
    /***************************************************/
    /****            Result utility class           ****/
    /***************************************************/

    public static class ResultUtil
    {
        /***************************************************/

        // Find first line containing results in a result file.
        public static int GetFirstResultLine(string[] resLines, ResultGroup resultGroup, GuanacoUtil.FileType fileType, int step = 0, double totalTime = 1.0)
        {
            int lineNo = -1;
            int stepCount = 1;
            double time = 0;

            switch (fileType)
            {
                case GuanacoUtil.FileType.dat:
                    {
                        string resultName = string.Empty;
                        switch (resultGroup)
                        {
                            case ResultGroup.Displacement:
                                {
                                    resultName = "displacements";
                                    break;
                                }
                            case ResultGroup.Strain:
                                {
                                    resultName = "strains";
                                    break;
                                }
                            case ResultGroup.Stress:
                                {
                                    resultName = "stresses";
                                    break;
                                }
                        }
                        for (int i = 0; i < resLines.Length; i++)
                        {
                            if (resLines[i].TrimStart(' ').StartsWith(resultName))
                            {
                                {
                                    string[] splt = resLines[i].Split(GuanacoUtil.Space, StringSplitOptions.RemoveEmptyEntries);
                                    time = double.Parse(splt[splt.Length - 1], GuanacoUtil.FloatNum);
                                    lineNo = i + 2;
                                    if (stepCount == step) return lineNo;
                                    stepCount++;
                                }
                            }
                        }
                        break;
                    }
                case GuanacoUtil.FileType.frd:
                    {
                        string resultName = string.Empty;
                        switch (resultGroup)
                        {
                            case ResultGroup.Stress:
                                {
                                    resultName = "STRESS";
                                    break;
                                }
                            case ResultGroup.Strain:
                                {
                                    resultName = "TOSTRAIN";
                                    break;
                                }
                        }
                        for (int i = 0; i < resLines.Length; i++)
                        {
                            int minLength = 5 + resultName.Length;
                            if (resLines[i].Length > minLength && resLines[i].Substring(5, resultName.Length) == resultName)
                            {
                                {
                                    string[] splt = resLines[i - 1].Split(GuanacoUtil.Space, StringSplitOptions.RemoveEmptyEntries);
                                    time = double.Parse(splt[2], GuanacoUtil.FloatNum);
                                    int j = i;
                                    while (j < resLines.Length)
                                    {
                                        if (resLines[j].Substring(1, 2) == "-1")
                                        {
                                            lineNo = j;
                                            break;
                                        }
                                        j++;
                                    }
                                    if (stepCount == step) return lineNo;
                                    stepCount++;
                                }
                            }
                        }
                        break;
                    }
            }

            if (time != totalTime)
            {
                throw new Exception("Oooops, the iteration did not converge!");
            }

            return lineNo;
        }

        /***************************************************/

        // Read node displacements from the output file.
        public static void ReadNodeDisplacements(Model model, int step = 0, double totalTime = 1.0)
        {
            string[] resLines = File.ReadAllLines(model.GetModelFilePath(GuanacoUtil.FileType.dat));

            int resid = GetFirstResultLine(resLines, ResultGroup.Displacement, GuanacoUtil.FileType.dat, step, totalTime);
            for (int i = 0; i < model.Mesh.Nodes.Count; i++)
            {
                double[] disp = resLines[resid + i].Split(GuanacoUtil.Space, StringSplitOptions.RemoveEmptyEntries).Select(x => double.Parse(x, GuanacoUtil.FloatNum)).ToArray();
                model.Mesh.Nodes[i].SetDisplacement(new Vector3d(disp[1], disp[2], disp[3]));
            }
        }

        /***************************************************/

        // Read results for 1D elements from the output file.
        public static void Read1DElementForces(Model model, int step = 0, double totalTime = 1.0)
        {
            if (model.Bars.Count == 0) return;

            List<int> el1DIds = new List<int>();
            foreach (Element e in model.Mesh.Elements)
            {
                if (e is Element1D) el1DIds.Add(e.Id);
            }

            string[] resLines = File.ReadAllLines(model.GetModelFilePath(GuanacoUtil.FileType.dat));

            bool stepFound = false;
            int lineNo = -1;
            double time = 0;
            int stepCount = 1;
            for (int i = 0; i < resLines.Length; i++)
            {
                string line = resLines[i];
                if (line.TrimStart(' ').StartsWith("statistics"))
                {
                    string[] splt = line.Split(GuanacoUtil.Space, StringSplitOptions.RemoveEmptyEntries);
                    time = double.Parse(splt[splt.Length - 1], GuanacoUtil.FloatNum);
                    lineNo = i + 4;
                    if (stepCount == step)
                    {
                        stepFound = true;
                        break;
                    }
                    stepCount++;
                    i += el1DIds.Count * 2 * 20 - 1;
                }
            }

            if (!stepFound && time != totalTime)
            {
                throw new Exception("Oooops, the iteration did not converge!");
            }

            for (int i = 0; i < el1DIds.Count; i++)
            {
                Element1D e = model.Mesh.Elements[el1DIds[i]] as Element1D;

                foreach (string s in Enum.GetNames(typeof(SectionForces1D)).Skip(1))
                {
                    if (!e.Results.ContainsKey(s))
                        e.Results.Add(s, new double[2]);
                }

                double multiplier = 1;
                for (int j = 0; j < 2; j++)
                {
                    Bar parentBar = e.ParentComponent as Bar;
                    Plane LCS = new Plane(parentBar.LCS);
                    LCS.Rotate(parentBar.Rotation, LCS.ZAxis);
                    Transform t = Transform.PlaneToPlane(Plane.WorldXY, LCS);

                    double[] force = resLines[lineNo].Split(GuanacoUtil.Space, StringSplitOptions.RemoveEmptyEntries).Select(x => double.Parse(x, GuanacoUtil.FloatNum)).Take(3).ToArray();
                    Vector3d forceV = new Vector3d(force[0], force[1], force[2]) * multiplier;
                    forceV.Transform(t);
                    e.Results["Fx"][j] = forceV.X;
                    e.Results["Fy"][j] = forceV.Y;
                    e.Results["Fz"][j] = forceV.Z;
                    lineNo += 8;
                    double[] moment = resLines[lineNo].Split(GuanacoUtil.Space, StringSplitOptions.RemoveEmptyEntries).Select(x => double.Parse(x, GuanacoUtil.FloatNum)).ToArray();
                    Vector3d momentV = new Vector3d(moment[0], moment[1], moment[2]) * multiplier;
                    momentV.Transform(t);
                    e.Results["Mxx"][j] = momentV[0];
                    e.Results["Myy"][j] = momentV[1];
                    e.Results["Mzz"][j] = momentV[2];
                    lineNo += 12;
                    multiplier *= -1;
                }
            }
        }

        /***************************************************/

        // Read results for 2D elements from the output file.
        public static void Read2DElementResults(Model model, ResultGroup resultType, int step = 0, double totalTime = 1.0)
        {
            if (model.Panels.Count == 0) return;

            // Find Ids of 2D elements.
            List<int> el2DIds = new List<int>();
            foreach (Element e in model.Mesh.Elements)
            {
                if (e is Element2D)
                    el2DIds.Add(e.Id);
            }

            // Read result lines.
            string[] resLines = File.ReadAllLines(model.GetModelFilePath(GuanacoUtil.FileType.frd));
            string line;

            // Find ids of vertices of each element.
            List<int[]> vertIds = new List<int[]>();
            int elCount = 0;
            bool elInfo = false;
            for (int i = 0; i < resLines.Length; i++)
            {
                line = resLines[i].TrimStart(' ');
                if (!elInfo && line.StartsWith("3C"))
                {
                    elInfo = true;
                    continue;
                }
                else if (elInfo && line.StartsWith("-1"))
                {
                    int elId = int.Parse(line.Split(GuanacoUtil.Space, StringSplitOptions.RemoveEmptyEntries)[1]) - 1;
                    if (elId == el2DIds[elCount])
                    {
                        Element2D e = model.Mesh.Elements[elId] as Element2D;
                        i++;
                        line = resLines[i].TrimStart(' ');
                        int[] splt = line.Split(GuanacoUtil.Space, StringSplitOptions.RemoveEmptyEntries).Skip(1).Select(s => int.Parse(s)).ToArray();
                        vertIds.Add(splt.Take(e.PrimaryNodeCount * 2).ToArray());

                        elCount++;
                        if (elCount == el2DIds.Count)
                            break;
                    }
                }
            }

            // Read the results per each vertex.
            Dictionary<int, double[]> nodeValues = new Dictionary<int, double[]>();
            int resid = GetFirstResultLine(resLines, resultType, GuanacoUtil.FileType.frd, step, totalTime);
            line = resLines[resid];
            string start = "-1";
            while (start == "-1")
            {
                int nodeId = int.Parse(line.Substring(4, 9));
                double[] nodeValue = new double[6];
                int i = 13;
                for (int j = 0; j < 6; j++)
                {
                    string resultString = line.Substring(i, 12);
                    double resultValue = double.NaN;
                    if (double.TryParse(resultString, GuanacoUtil.FloatNum, GuanacoUtil.Invariant, out resultValue))
                        nodeValue[j] = resultValue;
                    else if (resultString.Substring(9) == "INF")
                    {
                        nodeValue[j] = resultString[8] == '-' ? double.NegativeInfinity : double.PositiveInfinity;
                    }
                    else
                        throw new Exception("The result format is incorrect, please inspect the result .frd file.");
                    i += 12;
                }
                nodeValues.Add(nodeId, nodeValue);
                resid++;
                line = resLines[resid];
                start = line.Substring(1, 2);
            }

            // Assign the values to the elements.
            for (int i = 0; i < el2DIds.Count; i++)
            {
                Element2D e = model.Mesh.Elements[el2DIds[i]] as Element2D;
                int vertCount = vertIds[i].Length;
                Type type = null;
                switch (resultType)
                {
                    case ResultGroup.Stress:
                        type = typeof(Stress2D);
                        break;
                    case ResultGroup.Strain:
                        type = typeof(Strain2D);
                        break;
                }

                string[] names = Enum.GetNames(type).Skip(1).ToArray();
                foreach (string name in names)
                {
                    if (!e.Results.ContainsKey(name)) e.Results.Add(name, new double[vertCount]);
                }

                for (int j = 0; j < vertCount; j++)
                {
                    double[] vertValues = nodeValues[vertIds[i][j]];
                    for (int k = 0; k < names.Length; k++)
                    {
                        e.Results[names[k]][j] = vertValues[k];
                    }
                }
            }
        }

        /***************************************************/

        // Result groups.
        public enum ResultGroup
        {
            Displacement,
            SectionForces,
            Stress,
            Strain
        }

        /***************************************************/

        // Nodal result types.
        public static Type[] Results0D = new Type[] { typeof(Displacement0D) };
        public enum Displacement0D
        {
            None,
            dX,
            dY,
            dZ,
            dTotal
        }

        /***************************************************/

        // Result types for 1D elements.
        public static Type[] Results1D = new Type[] { typeof(SectionForces1D) };
        public enum SectionForces1D
        {
            None,
            Fx,
            Fy,
            Fz,
            Mxx,
            Myy,
            Mzz
        }

        /***************************************************/

        // Result types for 2D elements.
        public static Type[] Results2D = new Type[] { typeof(Stress2D), typeof(Strain2D) };
        public enum Stress2D
        {
            None,
            Sxx,
            Syy,
            Szz,
            Sxy,
            Sxz,
            Syz
        }

        public enum Strain2D
        {
            None,
            Exx,
            Eyy,
            Ezz,
            Exy,
            Exz,
            Eyz
        }

        /***************************************************/

        // Result types for 3D elements.
        public static Type[] Results3D = new Type[] { };

        /***************************************************/

        // Compile-time result dimension mapper.
        public static Dictionary<int, Type[]> resultDimension = new Dictionary<int, Type[]>
        {
            {0, Results0D },
            {1, Results1D },
            {2, Results2D },
            {3, Results3D }
        };

        /***************************************************/

        // Check if the result type refers to the elements of relevant dimension.
        public static void CheckResultDimension(this object resultType, int dimension)
        {
            Type t1 = resultType.GetType();
            foreach (Type t2 in resultDimension[dimension])
            {
                if (t1 == t2) return;
            }
            throw new Exception(string.Format("The input result {0} is not applicable to {1}-dimensional objects.", resultType, dimension));
        }

        /***************************************************/
    }
}

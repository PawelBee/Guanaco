using System.Collections.Generic;
using System.Linq;
using Rhino.Geometry;
using System;
using System.Drawing;

namespace Guanaco
{
    /***************************************************/
    /****           Preview utility class           ****/
    /***************************************************/

    public static class PreviewUtil
    {
        /***************************************************/

        // Create the preview of panels.
        public static void AddPanelPreview(this Rhino.Display.CustomDisplay display, Model model, IEnumerable<int> ids, out HashSet<int> nodeIds, double graphicFactor, double elementFactor, double textFactor, double forceFactor, bool showComponentNumbers, bool showElementNumbers, bool showLCS, bool showLoads, bool showLoadValues, bool showEdges, bool showThk = false, Grasshopper.GUI.Gradient.GH_Gradient gradient = null, string panelResultType = "None", bool showResultValues = false)
        {
            nodeIds = new HashSet<int>();
            if (model.Panels.Count == 0)
                return;

            // Take all results out first to determine min & max, otherwise could be done inside the loop.
            Dictionary<int, double> panelTopResults = new Dictionary<int, double>();
            Dictionary<int, double> panelBottomResults = new Dictionary<int, double>();
            double panelMax = 0;
            double panelMin = 0;
            if (panelResultType != "None")
            {
                Dictionary<int, double[]> panelResults = model.Mesh.GetElementResults(panelResultType);
                foreach (KeyValuePair<int, double[]> elResult in panelResults)
                {
                    int id = elResult.Key;
                    double[] results = elResult.Value;
                    int halfResultCount = results.Length / 2;

                    panelTopResults.Add(id, results.Skip(halfResultCount).Average());
                    panelBottomResults.Add(id, results.Take(halfResultCount).Average());
                }

                panelMax = Math.Max(panelTopResults.Values.Max(), panelBottomResults.Values.Max());
                panelMin = Math.Min(panelTopResults.Values.Min(), panelBottomResults.Values.Min());
            }

            ids = ids.Count() == 0 ? Enumerable.Range(0, model.Panels.Count) : ids.Select(i => i - 1);

            foreach (int i in ids)
            {
                Panel panel = model.Panels[i];

                if (showComponentNumbers)
                {
                    Vector3d fOffset = panel.LCS.ZAxis * 0.002;
                    if (showThk)
                    {
                        fOffset += panel.LCS.ZAxis * 0.5 * panel.Thickness;
                    }
                    Plane fp = new Plane(panel.LCS.Origin + fOffset, panel.LCS.ZAxis);
                    Plane bp = new Plane(panel.LCS.Origin - fOffset, -panel.LCS.ZAxis);
                    Rhino.Display.Text3d ft = new Rhino.Display.Text3d("Panel " + panel.CCXId(), fp, elementFactor * 0.6 * textFactor);
                    Rhino.Display.Text3d bt = new Rhino.Display.Text3d("Panel " + panel.CCXId(), bp, elementFactor * 0.6 * textFactor);
                    ft.Bold = true;
                    bt.Bold = true;
                    display.AddText(ft, Color.DarkRed);
                    display.AddText(bt, Color.DarkRed);
                }

                foreach (int eId in panel.Elements)
                {
                    Element2D e = model.Mesh.Elements[eId] as Element2D;
                    for (int j = 0; j < e.PrimaryNodeCount; j++)
                    {
                        nodeIds.Add(e.Nodes[j]);
                    }

                    Color bottomColor;
                    Color topColor;
                    if (panelResultType == "None") bottomColor = topColor = Color.Aqua;
                    else
                    {
                        bottomColor = gradient.ColourAt((panelBottomResults[eId] - panelMin) / (panelMax - panelMin));
                        topColor = gradient.ColourAt((panelTopResults[eId] - panelMin) / (panelMax - panelMin));
                    }

                    Point3d centroid = e.GetCentroid();
                    Vector3d normal = e.GetNormal();
                    Vector3d minOffset = normal * 0.001;
                    Vector3d offset = showThk ? normal * 0.5 * panel.Thickness : minOffset;

                    List<Point3d> front = new List<Point3d>();
                    List<Point3d> back = new List<Point3d>();

                    foreach (Point3d v in e.GetVertices())
                    {
                        front.Add(v + offset);
                        back.Add(v - offset);
                    }
                    display.AddPolygon(front, topColor, Color.Black, true, showEdges);
                    display.AddPolygon(back, bottomColor, Color.Black, true, showEdges);

                    if (showThk)
                    {
                        front.Add(front[0]);
                        back.Add(back[0]);
                        for (int j = 0; j < front.Count - 1; j++)
                        {
                            List<Point3d> fv = front.GetRange(j, 2);
                            List<Point3d> bv = back.GetRange(j, 2);
                            bv.Reverse();
                            fv.AddRange(bv);
                            display.AddPolygon(fv, Color.Aqua, Color.Black, true, true);
                        }
                    }

                    Plane tp = new Plane(panel.LCS);
                    tp.Origin = centroid + offset + minOffset;
                    tp.XAxis *= -1;
                    tp.ZAxis *= -1;
                    Plane bp = new Plane(panel.LCS);
                    bp.Origin = centroid - offset - minOffset;

                    if (showResultValues && panelTopResults.Count != 0)
                    {
                        Rhino.Display.Text3d t1 = new Rhino.Display.Text3d(panelTopResults[eId].ToString("G2"), tp, elementFactor * 0.4 * textFactor);
                        t1.Bold = true;
                        display.AddText(t1, Color.Black);
                        Rhino.Display.Text3d t2 = new Rhino.Display.Text3d(panelBottomResults[eId].ToString("G2"), bp, elementFactor * 0.4 * textFactor);
                        t2.Bold = true;
                        display.AddText(t2, Color.Black);
                    }

                    if (showLCS)
                    {
                        display.AddVector(centroid, e.Orientation[0] * elementFactor * graphicFactor, Color.Red);
                        display.AddVector(centroid, e.Orientation[1] * elementFactor * graphicFactor, Color.Green);
                        display.AddVector(centroid, normal * elementFactor * graphicFactor, Color.Blue);
                    }

                    if (showLoads && e.Pressure != 0)
                    {
                        Vector3d pv = normal * e.Pressure / forceFactor * elementFactor * 5 * graphicFactor;
                        List<Point3d> pressurePoints = new List<Point3d>();
                        foreach (Point3d pt in e.PopulateWithPoints())
                        {
                            if (e.Pressure > 0)
                            {
                                display.AddVector(pt - pv - offset, pv);
                            }
                            else
                            {
                                display.AddVector(pt - pv + offset, pv);
                            }
                        }

                        if (showLoadValues)
                        {
                            Plane p = new Plane();
                            if (e.Pressure > 0)
                            {
                                p = new Plane(e.GetCentroid() - pv * 1.1 - offset, Vector3d.XAxis, Vector3d.ZAxis);
                            }
                            else
                            {
                                p = new Plane(e.GetCentroid() - pv * 1.1 + offset, Vector3d.XAxis, Vector3d.ZAxis);
                            }
                            Rhino.Display.Text3d t = new Rhino.Display.Text3d(e.Pressure.ToString("G2"), p, elementFactor * 0.4 * textFactor);
                            t.Bold = true;
                            display.AddText(t, Color.Magenta);
                        }
                    }

                    if (showElementNumbers)
                    {
                        Rhino.Display.Text3d tt = new Rhino.Display.Text3d(e.CCXId(), tp, elementFactor * 0.4 * textFactor);
                        Rhino.Display.Text3d bt = new Rhino.Display.Text3d(e.CCXId(), bp, elementFactor * 0.4 * textFactor);
                        tt.Bold = true;
                        bt.Bold = true;
                        display.AddText(tt, Color.Black);
                        display.AddText(bt, Color.Black);
                    }
                }
            }
        }

        /***************************************************/

        // Create the preview of bars.
        public static void AddBarPreview(this Rhino.Display.CustomDisplay display, Model model, IEnumerable<int> ids, out HashSet<int> nodeIds, double graphicFactor, double elementFactor, double textFactor, bool showComponentNumbers, bool showElementNumbers, bool showLCS, bool showThk = false, string barResultType = "None", bool showResultValues = false)
        {
            nodeIds = new HashSet<int>();
            if (model.Bars.Count == 0)
                return;

            // Take all results out first to determine min & max, otherwise could be done inside the loop.
            Dictionary<int, double[]> barResults = new Dictionary<int, double[]>();
            double BFactor = 0;
            if (barResultType != "None")
            {
                barResults = model.Mesh.GetElementResults(barResultType);
                double barExtreme = barResults.Values.Select(r => Math.Max(Math.Abs(r[0]), Math.Abs(r[1]))).Max();
                BFactor = elementFactor / barExtreme * 10;
            }

            ids = ids.Count() == 0 ? Enumerable.Range(0, model.Bars.Count) : ids.Select(i => i - 1);

            foreach (int i in ids)
            {
                Bar bar = model.Bars[i];
                Plane locLCS = new Plane(bar.LCS);
                locLCS.Rotate(bar.Rotation, locLCS.ZAxis);
                locLCS.Translate(locLCS.XAxis * bar.Offset.X);
                locLCS.Translate(locLCS.YAxis * bar.Offset.Y);

                Vector3d hOffset = locLCS.XAxis * (bar.Profile.GetHeight() * 0.5);
                if (showThk) hOffset *= 2;

                Plane tp = new Plane(locLCS.Origin, bar.LCS.YAxis, Vector3d.ZAxis);
                tp.Translate(hOffset);
                Curve[] profileCurves = bar.Profile.ToRhinoProfile();

                if (showComponentNumbers)
                {
                    Rhino.Display.Text3d t = new Rhino.Display.Text3d("Bar " + bar.CCXId(), tp, elementFactor * 0.6 * textFactor);
                    t.Bold = true;
                    display.AddText(t, Color.DarkRed);
                }

                foreach (int eId in bar.Elements)
                {
                    Element1D e = model.Mesh.Elements[eId] as Element1D;
                    nodeIds.Add(e.Nodes[0]);
                    nodeIds.Add(e.Nodes.Last());

                    Point3d[] endPts = e.GetVertices().ToArray();
                    Point3d c = (endPts[0] + endPts[1]) * 0.5;

                    display.AddLine(new Line(endPts[0], endPts[1]), Color.Aqua);

                    if (showThk)
                    {
                        Plane eP = new Plane(locLCS);
                        List<Point3d> elementVertices = e.GetVertices().ToList();
                        eP.Translate(elementVertices[0] - bar.LCS.Origin);
                        Transform ptp = Transform.PlaneToPlane(Plane.WorldXY, eP);

                        // Use DisplayPipeline to get surface preview! Now a temporary solution given.
                        foreach (Curve pc in profileCurves)
                        {
                            Curve opc = pc.DuplicateCurve();
                            opc.Transform(ptp);
                            display.AddCurve(opc);
                            opc.Translate(elementVertices[1] - elementVertices[0]);
                            display.AddCurve(opc);
                        }
                    }

                    if (barResultType != "None")
                    {
                        Vector3d graphDir;
                        switch (barResultType)
                        {
                            case "Myy":
                                graphDir = locLCS.XAxis;
                                break;
                            case "Mxx":
                                graphDir = locLCS.YAxis;
                                break;
                            default:
                                string barResDir = barResultType.ToString().Substring(1, 1);
                                graphDir = barResDir == "y" ? locLCS.YAxis : locLCS.XAxis;
                                break;
                        }
                        graphDir *= BFactor * graphicFactor;
                        Point3d[] cPts = new Point3d[] { endPts[0], endPts[1], endPts[1] + graphDir * barResults[eId][1], endPts[0] + graphDir * barResults[eId][0] };
                        display.AddPolygon(cPts, Color.CornflowerBlue, Color.Black, true, true);

                        if (showResultValues && barResults.Count != 0)
                        {
                            Plane tp1 = new Plane(tp);
                            tp1.Translate(endPts[0] - tp1.Origin + graphDir * barResults[eId][0]);
                            Rhino.Display.Text3d t1 = new Rhino.Display.Text3d(barResults[eId][0].ToString("G2"), tp1, elementFactor * 0.4 * textFactor);
                            t1.Bold = true;
                            display.AddText(t1, Color.Black);
                            Plane tp2 = new Plane(tp);
                            tp2.Translate(endPts[1] - tp2.Origin + graphDir * barResults[eId][1]);
                            Rhino.Display.Text3d t2 = new Rhino.Display.Text3d(barResults[eId][1].ToString("G2"), tp2, elementFactor * 0.4 * textFactor);
                            t2.Bold = true;
                            display.AddText(t2, Color.Black);
                        }
                    }

                    if (showLCS)
                    {
                        display.AddVector(c, locLCS.XAxis * elementFactor * graphicFactor, Color.Red);
                        display.AddVector(c, locLCS.YAxis * elementFactor * graphicFactor, Color.Green);
                        display.AddVector(c, locLCS.ZAxis * elementFactor * graphicFactor, Color.Blue);
                    }

                    if (showElementNumbers)
                    {
                        Plane etp = new Plane(tp);
                        etp.Translate(c - bar.LCS.Origin);
                        Rhino.Display.Text3d t = new Rhino.Display.Text3d(e.CCXId(), etp, elementFactor * 0.4 * textFactor);
                        t.Bold = true;
                        display.AddText(t, Color.Black);
                    }
                }
            }
        }

        /***************************************************/

        // Create the preview of nodes.
        public static void AddNodePreview(this Rhino.Display.CustomDisplay display, Model model, IEnumerable<int> ids, double graphicFactor, double elementFactor, double textFactor, double forceFactor, bool showNodes, bool showNodeNumbers, bool showLoads, bool showLoadValues, Grasshopper.GUI.Gradient.GH_Gradient gradient = null, string nodeResultType = "None", bool showResultValues = false)
        {
            if (model.Mesh == null || model.Mesh.Nodes.Count == 0)
                return;

            List<double> nodeResults = new List<double>();
            double nodeMax = 0;
            double nodeMin = 0;
            if (nodeResultType != "None")
            {
                nodeResults = model.Mesh.GetNodeDisplacement(nodeResultType);
                nodeMax = nodeResults.Max();
                nodeMin = nodeResults.Min();
            }

            foreach (int i in ids)
            {
                Node n = model.Mesh.Nodes[i];
                if (!n.Primary) continue;

                if (showNodes)
                {
                    if (nodeResults.Count > 0)
                    {
                        display.AddPoint(n.Location, gradient.ColourAt((nodeResults[i] - nodeMin) / (nodeMax - nodeMin)), Rhino.Display.PointStyle.Simple, Math.Max(1, Convert.ToInt32(5 * elementFactor * graphicFactor)));
                    }
                    else display.AddPoint(n.Location, Color.DarkRed, Rhino.Display.PointStyle.Simple, Math.Max(1, Convert.ToInt32(5 * elementFactor * graphicFactor)));
                }

                if (showResultValues && nodeResults.Count > 0)
                {
                    Plane p = new Plane(n.Location + Vector3d.ZAxis * elementFactor, Vector3d.XAxis, Vector3d.ZAxis);
                    Rhino.Display.Text3d t = new Rhino.Display.Text3d(nodeResults[i].ToString("G2"), p, elementFactor * 0.4 * textFactor);
                    t.Bold = true;
                    display.AddText(t, Color.Black);
                }

                if (showLoads && n.ForceLoad.Length != 0)
                {
                    Vector3d dLoad = n.ForceLoad / forceFactor * elementFactor * 5 * graphicFactor;
                    display.AddVector(n.Location - dLoad, dLoad);

                    if (showLoadValues)
                    {
                        Plane p = new Plane(n.Location - dLoad * 1.1, Vector3d.XAxis, Vector3d.ZAxis);
                        Rhino.Display.Text3d t = new Rhino.Display.Text3d(n.ForceLoad.Length.ToString("G2"), p, elementFactor * 0.4 * textFactor);
                        t.Bold = true;
                        display.AddText(t, Color.Magenta);
                    }
                }

                if (showLoads && n.MomentLoad.Length != 0)
                {
                    Vector3d mLoad = n.MomentLoad;
                    Vector3d axis = new Vector3d(mLoad.X, mLoad.Y, mLoad.Z);
                    Plane ap = new Plane(n.Location, axis);
                    Arc mArc = new Arc(new Circle(ap, 2.5 * elementFactor * graphicFactor), Math.PI * 1.5);
                    display.AddArc(mArc);
                    display.AddVector(mArc.EndPoint, mArc.TangentAt(Math.PI * 1.5));

                    if (showLoadValues)
                    {
                        Plane p = new Plane(n.Location - n.MomentLoad / forceFactor * elementFactor * 5 * graphicFactor * 1.1, Vector3d.XAxis, Vector3d.ZAxis);
                        Rhino.Display.Text3d t = new Rhino.Display.Text3d(n.MomentLoad.Length.ToString("G2"), p, elementFactor * 0.4 * textFactor);
                        t.Bold = true;
                        display.AddText(t, Color.Magenta);
                    }
                }

                if (showNodeNumbers)
                {
                    Plane p = new Plane(n.Location, Vector3d.XAxis, Vector3d.ZAxis);
                    Rhino.Display.Text3d t = new Rhino.Display.Text3d(n.CCXId(), p, elementFactor * 0.4 * textFactor);
                    t.Bold = true;
                    display.AddText(t, Color.Purple);
                }
            }
        }

        /***************************************************/

        // Create the preview of supports.
        public static void AddSupportPreview(this Rhino.Display.CustomDisplay display, Model model, HashSet<int> ids, double elementFactor, double graphicFactor)
        {
            double translationSupportFactor = 0.7 * elementFactor * graphicFactor;
            double rotationSupportFactor = 0.35 * elementFactor * graphicFactor;

            foreach (Support s in model.Supports)
            {
                foreach (int id in ids.Intersect(s.Nodes))
                {
                    Node n = model.Mesh.Nodes[id];
                    foreach (int dof in s.SupportType.DOFs)
                    {
                        if (dof == 1)
                            display.PreviewTanslationSupport(n, Vector3d.XAxis, translationSupportFactor);
                        if (dof == 2)
                            display.PreviewTanslationSupport(n, Vector3d.YAxis, translationSupportFactor);
                        if (dof == 3)
                            display.PreviewTanslationSupport(n, Vector3d.ZAxis, translationSupportFactor);
                        if (dof == 4)
                            display.PreviewRotationSupport(n, Vector3d.XAxis, rotationSupportFactor);
                        if (dof == 5)
                            display.PreviewRotationSupport(n, Vector3d.YAxis, rotationSupportFactor);
                        if (dof == 6)
                            display.PreviewRotationSupport(n, Vector3d.ZAxis, rotationSupportFactor);
                    }
                }
            }
        }

        /***************************************************/

        // Calculate the display factors dependent on the size of the model and its elements.
        public static double[] sizeFactors(this Model model)
        {
            BoundingBox bBox = new BoundingBox(model.Mesh.Nodes.Select(x => x.Location));

            double forceFactor = 0;
            double elementFactor = bBox.Diagonal.Length;

            foreach (Panel panel in model.Panels)
            {
                foreach (int eId in panel.Elements)
                {
                    Element2D e = model.Mesh.Elements[eId] as Element2D;
                    List<Point3d> eVertices = e.GetVertices().ToList();
                    if (e.PrimaryNodeCount == 3)
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            double edgeL = eVertices[i].DistanceTo(eVertices[(i + 1) % 3]);
                            if (edgeL <= elementFactor)
                            {
                                elementFactor = edgeL;
                            }
                        }
                    }
                    else
                    {
                        for (int i = 0; i < 2; i++)
                        {
                            double diagL = eVertices[i].DistanceTo(eVertices[i + 2]);
                            if (diagL <= elementFactor)
                            {
                                elementFactor = diagL;
                            }
                        }
                    }
                    if (e.Pressure != 0)
                    {
                        if (Math.Abs(e.Pressure) >= forceFactor)
                        {
                            forceFactor = Math.Abs(e.Pressure);
                        }
                    }
                }
            }

            foreach (Bar bar in model.Bars)
            {
                foreach (int eId in bar.Elements)
                {
                    Element1D e = model.Mesh.Elements[eId] as Element1D;
                    Point3d[] eVertices = e.GetVertices().ToArray();
                    double l = eVertices[0].DistanceTo(eVertices[1]);
                    if (l <= elementFactor)
                    {
                        elementFactor = l;
                    }

                }
            }

            foreach (Node n in model.Mesh.Nodes)
            {
                if (n.ForceLoad.Length != 0)
                {
                    if (Math.Abs(n.ForceLoad.Length) >= forceFactor)
                    {
                        forceFactor = Math.Abs(n.ForceLoad.Length);
                    }
                }
            }

            elementFactor *= 0.3;

            return new double[] { elementFactor, forceFactor };
        }
        
        /***************************************************/

        // Create standard colour gradient for showing the results.
        public static Grasshopper.GUI.Gradient.GH_Gradient CreateStandardGradient()
        {
            Grasshopper.GUI.Gradient.GH_Gradient gradient = new Grasshopper.GUI.Gradient.GH_Gradient();
            gradient.AddGrip(0.000, System.Drawing.Color.Blue);
            gradient.AddGrip(0.125, System.Drawing.Color.SkyBlue);
            gradient.AddGrip(0.250, System.Drawing.Color.Cyan);
            gradient.AddGrip(0.375, System.Drawing.Color.SpringGreen);
            gradient.AddGrip(0.500, System.Drawing.Color.GreenYellow);
            gradient.AddGrip(0.625, System.Drawing.Color.Yellow);
            gradient.AddGrip(0.750, System.Drawing.Color.Orange);
            gradient.AddGrip(0.875, System.Drawing.Color.Red);
            gradient.AddGrip(1.000, System.Drawing.Color.Brown);
            return gradient;
        }

        /***************************************************/

        // Create the preview of a translation support.
        public static void PreviewTanslationSupport(this Rhino.Display.CustomDisplay display, Node n, Vector3d axis, double displayFactor)
        {
            Point3d ptc = n.Location - axis * displayFactor;
            Point3d pbc = n.Location - axis * 1.1 * displayFactor;
            Vector3d localX = new Vector3d(axis.Z, axis.X, axis.Y);
            Vector3d localY = new Vector3d(axis.Y, axis.Z, axis.X);
            Vector3d Xt = localX * 0.5 * displayFactor;
            Vector3d Yt = localY * 0.5 * displayFactor;
            Vector3d Xb = localX * 0.55 * displayFactor;
            Vector3d Yb = localY * 0.55 * displayFactor;
            List<Point3d> pts = new List<Point3d> { ptc + Xt + Yt, ptc + Xt - Yt, ptc - Xt - Yt, ptc - Xt + Yt };
            List<Point3d> pbs = new List<Point3d> { pbc + Xb + Yb, pbc + Xb - Yb, pbc - Xb - Yb, pbc - Xb + Yb };
            display.AddPolygon(pbs, Color.Brown, Color.Black, true, true);
            display.AddPolygon(pts, Color.Brown, Color.Black, true, true);
            for (int i = 0; i < 4; i++)
            {
                display.AddPolygon(new Point3d[] { pts[i], pts[(i + 1) % 4], n.Location }, Color.Brown, Color.Black, true, true);
            }
        }

        /***************************************************/

        // Create the preview of a rotation support.
        public static void PreviewRotationSupport(this Rhino.Display.CustomDisplay display, Node node, Vector3d axis, double displayFactor)
        {
            double circRadius = Math.Sqrt(0.5) * displayFactor;
            Plane p = new Plane(node.Location, axis);
            Circle c = new Circle(p, displayFactor);
            Vector3d v1 = (new Vector3d(1, 1, 1) - axis) * circRadius;
            Vector3d v2 = v1.X == 0 ? new Vector3d(0, -v1.Y, v1.Z) : new Vector3d(-v1.X, v1.Y, v1.Z);
            Point3d s1 = node.Location - v1;
            Point3d s2 = node.Location - v2;
            Line l1 = new Line(s1, v1 * 2);
            Line l2 = new Line(s2, v2 * 2);
            display.AddLine(l1);
            display.AddLine(l2);
            display.AddCircle(c);
        }

        /***************************************************/

        // Populate the element with points - used for preview purposes to show pressure loads.
        public static List<Point3d> PopulateWithPoints(this Element2D element)
        {
            List<Point3d> corners = element.Nodes.Take(element.PrimaryNodeCount).Select(n => element.ParentMesh.Nodes[n].Location).ToList();
            Surface srf;
            if (corners.Count == 3) srf = NurbsSurface.CreateFromCorners(corners[0], corners[1], corners[2]);
            else srf = NurbsSurface.CreateFromCorners(corners[0], corners[1], corners[2], corners[3]);

            corners.Add(corners[0]);
            Polyline perim = new Polyline(corners);
            double minDist = perim.Length * 0.01;

            List<Point3d> srfPts = new List<Point3d>();

            Interval uDomain = srf.Domain(0);
            Interval vDomain = srf.Domain(1);
            double uMin = uDomain.Min;
            double uMax = uDomain.Max;
            double vMin = vDomain.Min;
            double vMax = vDomain.Max;
            double uStep = (uMax - uMin) / 3;
            double vStep = (vMax - vMin) / 3;

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    srfPts.Add(srf.PointAt(uMin + i * uStep, vMin + j * vStep));
                }
            }

            Brep srfBrep = srf.ToBrep();
            List<Point3d> pts = new List<Point3d>();

            foreach (Point3d pt in srfPts)
            {
                if (srfBrep.ClosestPoint(pt).DistanceTo(pt) <= minDist && perim.ClosestPoint(pt).DistanceTo(pt) > minDist) pts.Add(pt);
            }

            return pts;
        }

        /***************************************************/
    }
}

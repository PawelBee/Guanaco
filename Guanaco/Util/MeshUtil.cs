using Rhino;
using System;
using System.Collections.Generic;
using System.Linq;
using Rhino.Geometry;

namespace Guanaco
{
    /***************************************************/
    /****             Mesh utility class            ****/
    /***************************************************/

    public static class MeshUtil
    {
        /***************************************************/

        // Gmsh parameter class.
        public class GmshParams
        {
            Element2D.Element2DShape _elementShape;
            public Element2D.Element2DShape ElementShape
            {
                get
                {
                    return this._elementShape;
                }
            }
            
            GmshAlgorithm _meshingAlgorithm;
            public GmshAlgorithm MeshingAlgorithm
            {
                get
                {
                    return this._meshingAlgorithm;
                }
            }

            double _maxCharLength;
            public double MaxCharLength
            {
                get
                {
                    return this._maxCharLength;
                }
            }

            bool _secondOrder;
            public bool SecondOrder
            {
                get
                {
                    return this._secondOrder;
                }
            }

            int _smoothingSteps;
            public int SmoothingSteps
            {
                get
                {
                    return this._smoothingSteps;
                }
            }

            bool _highOrderOptimization;
            public bool HighOrderOptimization
            {
                get
                {
                    return this._highOrderOptimization;
                }
            }

            double _optimizationThreshold;
            public double OptimizationThreshold
            {
                get
                {
                    return this._optimizationThreshold;
                }
            }

            bool _unifyFaces;
            public bool UnifyFaces
            {
                get
                {
                    return this._unifyFaces;
                }
            }

            public GmshParams(Element2D.Element2DShape elementShape, GmshAlgorithm meshingAlgorithm, double maxCharLength, bool secondOrder, int smoothingSteps, bool highOrderOptimization, double optimizationThreshold, bool unifyFaces)
            {
                this._elementShape = elementShape;
                this._meshingAlgorithm = meshingAlgorithm;
                this._maxCharLength = maxCharLength;
                this._secondOrder = secondOrder;
                this._smoothingSteps = smoothingSteps;
                this._highOrderOptimization = highOrderOptimization;
                this._optimizationThreshold = optimizationThreshold;
                this._unifyFaces = unifyFaces;
            }
        }

        // Generate an input for Gmsh based on the parameters.
        public static List<string> GmshInput(string name, double tol, GmshParams gmshParams)
        {
            string RA = ((int)gmshParams.ElementShape).ToString();
            string MA = ((int)gmshParams.MeshingAlgorithm).ToString();
            string MCL = gmshParams.MaxCharLength.ToString(GuanacoUtil.Invariant);
            string SO = gmshParams.SecondOrder ? "2" : "1";
            string SS = gmshParams.SmoothingSteps.ToString();
            string HOO = gmshParams.HighOrderOptimization ? "1" : "0";
            string OT = gmshParams.OptimizationThreshold.ToString(GuanacoUtil.Invariant);
            string UF = gmshParams.UnifyFaces ? "1" : "0";
            
            // Generate the input file for Gmsh.
            List<string> lines = new List<string>{ "SetFactory(\"OpenCASCADE\");",
            "Geometry.Tolerance=" + tol.ToString(GuanacoUtil.Invariant) + ";",
            "Geometry.OCCFixDegenerated=1;",
            "Geometry.OCCFixSmallEdges=1;",
            "Geometry.OCCFixSmallFaces=1;",
            "Geometry.OCCParallel=1;",
            "Merge \"" + name + "_Curves" + ".iges\";",
            "lineList[] = Line \"*\";",
            "initLineCount = #lineList[];",
            "Merge \"" + name + "_Surfaces" + ".iges\";",
            "lineList[] = Line \"*\";",
            "initSurfaceLineCount = #lineList[];",
            "surfaceList[] = Surface \"*\";",
            "initSurfaceCount = #surfaceList[];",
            "surfaceCount = #surfaceList[];",
            "linesToDelete = {};",
            "surfacesToDelete = {};",
            "For i In {1:initLineCount}",
            "lineCountBefore = #lineList[];",
            "BooleanIntersection { Line{i}; }{ Line{1:initLineCount};",
            "Surface{1:initSurfaceCount}; }",
            "lineList[] = Line \"*\";",
            "lineCountAfter = #lineList[];",
            "If(lineCountAfter>lineCountBefore)",
            "Physical Line(i) = {lineCountBefore+1:lineCountAfter};",
            "linesToDelete+={i};",
            "Else",
            "Physical Line(i) = {i};",
            "EndIf",
            "EndFor",
            "For i In {1:initSurfaceCount}",
            "surfaceCountBefore = #surfaceList[];",
            "BooleanIntersection { Surface{i}; }{ Line{1:initLineCount}; ",
            "Surface{1:initSurfaceCount}; }",
            "surfaceList[] = Surface \"*\";",
            "surfaceCountAfter = #surfaceList[];",
            "If(surfaceCountAfter>surfaceCountBefore)",
            "Physical Surface(i) = {surfaceCountBefore+1:surfaceCountAfter};",
            "surfacesToDelete+={i};",
            "Else",
            "Physical Surface(i) = {i};",
            "EndIf",
            "EndFor",
            "For i In {0:#linesToDelete[]-1}",
            "Delete{Line{linesToDelete[i]};}",
            "EndFor",
            "For i In {0:#surfacesToDelete[]-1}",
            "Delete{Surface{surfacesToDelete[i]};}",
            "EndFor",
            "Geometry.Tolerance=1e-8;",
            "Mesh.Algorithm = " + MA + ";",
            "Mesh.CharacteristicLengthMax = " + MCL + ";",
            "Mesh.ElementOrder = " + SO + ";",
            "Mesh.SecondOrderIncomplete = 1;",
            "Mesh.Smoothing = " + SS + ";",
            "Mesh.RecombineAll = " + RA + ";",
            "Mesh.SubdivisionAlgorithm = 0;",
            "Mesh.HighOrderThresholdMin = " + OT + ";",
            "Mesh.HighOrderOptPrimSurfMesh = " + UF + ";",
            "Mesh.HighOrderOptimize = " + HOO + ";",
            "Mesh 2;",
            "Coherence Mesh;",
            "Mesh.SurfaceFaces = 1;"
            };

            return lines;
        }

        /***************************************************/

        // Read mesh file and convert it to Guanaco mesh format.
        public static void ReadMesh(Model model, bool reducedIntegration)
        {
            // Create empty mesh, initiate local variables.
            Mesh mesh = new Mesh();
            model.SetMesh(mesh);
            int barCounter = 0;
            int panelCounter = 0;

            int nodeID, elementID;
            nodeID = elementID = 0;
            bool nodeLines, elementLines, componentLines;
            nodeLines = elementLines = componentLines = false;
            string[] itemSep = new string[] { "," };

            // Read the mesh file.
            string line;
            int elementOrder = -1;
            int elementDimension = -1;
            List<int> elementList = new List<int>();

            Dictionary<int, int> elementPairs = new Dictionary<int, int>();

            System.IO.StreamReader file = new System.IO.StreamReader(model.GetModelFilePath() + "_Mesh.inp");
            while ((line = file.ReadLine()) != null)
            {
                if (line.StartsWith("*NODE") && !line.StartsWith("*NODE "))
                {
                    nodeLines = true;
                    continue;
                }
                else if (nodeLines && line.StartsWith("*"))
                {
                    nodeLines = false;
                }

                // Create Guanaco node based on the given line of input file.
                else if (nodeLines)
                {
                    string[] nodeInfo = line.Replace("\n", "").Split(itemSep, StringSplitOptions.None);
                    double x = Double.Parse(nodeInfo[1], GuanacoUtil.FloatNum);
                    double y = Double.Parse(nodeInfo[2], GuanacoUtil.FloatNum);
                    double z = Double.Parse(nodeInfo[3], GuanacoUtil.FloatNum);
                    mesh.Nodes.Add(new Node(mesh, nodeID, new Rhino.Geometry.Point3d(x, y, z)));
                    nodeID++;
                }

                if (line.StartsWith("*ELEMENT, type="))
                {
                    elementLines = true;
                    string elementShape = line.Replace(" ", "").Substring(14).Split(',')[0];
                    switch (elementShape)
                    {
                        case "T3D2":
                            elementOrder = 1;
                            elementDimension = 1;
                            continue;
                        case "T3D3":
                            elementOrder = 2;
                            elementDimension = 1;
                            continue;
                        case "CPS3":
                            elementOrder = 1;
                            elementDimension = 2;
                            continue;
                        case "CPS4":
                            elementOrder = 1;
                            elementDimension = 2;
                            continue;
                        case "CPS6":
                            elementOrder = 2;
                            elementDimension = 2;
                            continue;
                        case "CPS8":
                            elementOrder = 2;
                            elementDimension = 2;
                            continue;
                        default:
                            throw new Exception("Incorrect element type! Check if all elements in the .inp file are supported by Guanaco.");
                    }
                }

                else if (elementLines && line.StartsWith("*"))
                {
                    elementLines = false;
                }

                // Create Guanaco element based on the given line of input file.
                else if (elementLines)
                {
                    string[] elInfo = line.Replace("\n", "").Split(itemSep, StringSplitOptions.None);
                    List<int> elementNodes = new List<int>();
                    for (int i = 1; i < elInfo.Length; i++)
                    {
                        int id = int.Parse(elInfo[i]) - 1;
                        elementNodes.Add(id);
                    }

                    Element el;
                    if (elementDimension == 1)
                    {
                        // Add element.
                        el = new Element1D(mesh, elementID, elementNodes, elementOrder, reducedIntegration);

                        // Set primary nodes of an element.
                        mesh.Nodes[elementNodes[0]].Primary = true;
                        mesh.Nodes[elementNodes.Last()].Primary = true;
                    }
                    else if (elementDimension == 2)
                    {
                        // Add element.
                        el = new Element2D(mesh, elementID, elementNodes, elementOrder, reducedIntegration);

                        // Set primary nodes of an element.
                        for (int i = 0; i < (el as Element2D).PrimaryNodeCount; i++)
                        {
                            mesh.Nodes[elementNodes[i]].Primary = true;
                        }
                    }
                    else throw new Exception("Unsupported element found!");
                    mesh.Elements.Add(el);
                    elementPairs.Add(int.Parse(elInfo[0]), elementID);

                    elementID++;
                }

                if (componentLines && !Char.IsDigit(line[0]))
                {
                    if (elementDimension == 1)
                    {
                        model.Bars[barCounter].AssignElements(elementList);
                        barCounter++;
                    }
                    else if (elementDimension == 2)
                    {
                        model.Panels[panelCounter].AssignElements(elementList);
                        panelCounter++;
                    }
                    elementList = new List<int>();
                }

                if (line.StartsWith("*ELSET,"))
                {
                    componentLines = true;
                    if (line.Contains("PhysicalLine"))
                    {
                        elementDimension = 1;
                    }
                    else if (line.Contains("PhysicalSurface"))
                    {
                        elementDimension = 2;
                    }
                    else throw new Exception("Gmsh physical entity type not known.");
                }

                else if (componentLines && line.StartsWith("*"))
                {
                    componentLines = false;
                }

                // Assign the mesh elements to relevant components.
                else if (componentLines)
                {
                    string[] sa = line.Replace("\n", "").Split(itemSep, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string s in sa)
                    {
                        int id;
                        if (Int32.TryParse(s, out id))
                        {
                            elementList.Add(elementPairs[id]);
                        }
                    }
                }
            }

            if (elementList.Count > 0)
            {
                if (elementDimension == 1)
                    model.Bars[barCounter].AssignElements(elementList);
                else if (elementDimension == 2)
                    model.Panels[panelCounter].AssignElements(elementList);
            }

            file.Close();
        }

        /***************************************************/

        // Convert Guanaco mesh to Rhino mesh.
        public static Rhino.Geometry.Mesh GetRhinoMesh(Mesh mesh, out List<LineCurve> barElements)
        {
            barElements = new List<LineCurve>();

            Rhino.Geometry.Mesh rhinoMesh = new Rhino.Geometry.Mesh();
            foreach (Node node in mesh.Nodes)
            {
                rhinoMesh.Vertices.Add(node.Location);
            }

            foreach (Element element in mesh.Elements)
            {
                if (element is Element2D)
                {
                    Element2D e = element as Element2D;
                    if (e.PrimaryNodeCount == 4)
                    {
                        rhinoMesh.Faces.AddFace(e.Nodes[0], e.Nodes[1], e.Nodes[2], e.Nodes[3]);
                    }
                    else if (e.PrimaryNodeCount == 3)
                    {
                        rhinoMesh.Faces.AddFace(e.Nodes[0], e.Nodes[1], e.Nodes[2]);
                    }
                }
                else if (element is Element1D)
                {
                    Element1D e = element as Element1D;
                    barElements.Add(new LineCurve(rhinoMesh.Vertices[e.Nodes[0]], rhinoMesh.Vertices[e.Nodes[e.Nodes.Count - 1]]));
                }
            }

            return rhinoMesh;
        }

        /***************************************************/

        // Gmsh mesh algorithms.
        public enum GmshAlgorithm
        {
            MeshAdapt = 1,
            Automatic = 2,
            Delaunay = 5,
            Frontal = 6,
            Bamg = 7,
            DelQuad = 8,
            Pack = 9
        }

        /***************************************************/
    }
}
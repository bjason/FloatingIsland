using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ConcaveHull
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class MeshGenerator : MonoBehaviour
    {
        public MeshFilter walls;
        public MeshFilter horizontalWall;
        List<Line> wallLines = new List<Line>();
        List<Vector3> wallVertices = new List<Vector3>();
        List<int> wallTriangle = new List<int>();


        public class Triangle
        {
            public Node[] nodes = new Node[3];
            public Triangle(Node n1, Node n2, Node n3)
            {
                nodes[0] = n1;
                nodes[1] = n2;
                nodes[2] = n3;
            }

            public Node this[int i]
            {
                get
                {
                    return nodes[i];
                }
            }
        }

        public void drawHorizontalMesh(int height)
        {
            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();

            vertices.Add(new Vector3(0, 0, 0));

            for (int i = 0; i < Hull.hull_nodes[0].Count - 1; i++)
            {
                int startIndex = vertices.Count;

                vertices.Add(Hull.hull_nodes[0][i].getVector());
                vertices.Add(Hull.hull_nodes[0][i + 1].getVector());

                triangles.Add(0);
                triangles.Add(startIndex);
                triangles.Add(startIndex + 1);
            }

            triangles.Add(vertices.Count - 1);
            triangles.Add(0);
            triangles.Add(1);

            int start = vertices.Count;
            vertices.Add(new Vector3(0, (height - 1) * 50, 0));

            for (int i = 0; i < Hull.hull_nodes[height - 1].Count - 1; i++)
            {
                int startIndex = vertices.Count;

                vertices.Add(Hull.hull_nodes[height - 1][i].getVector());
                vertices.Add(Hull.hull_nodes[height - 1][i + 1].getVector());

                triangles.Add(start);
                triangles.Add(startIndex + 1);
                triangles.Add(startIndex);
            }
            triangles.Add(vertices.Count - 1);
            triangles.Add(start);
            triangles.Add(start + 1);

            Mesh mesh = new Mesh();
            horizontalWall.mesh = mesh;
            // GetComponent<MeshFilter>().mesh = mesh;

            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.RecalculateNormals();
        }

        public void drawWallMesh()
        {
            List<Vector2> uv = new List<Vector2>();
            List<Vector4> tangents = new List<Vector4>();
            Vector4 tangent = new Vector4(1f, 0f, 0f, -1f);
            int numLayer = Hull.hull_edges.Count;

            for (int i = 0; i < numLayer - 1; i++)
            {
                // lower layer
                List<Node> unusedNodes1 = Hull.hull_nodes[i];
                unusedNodes1.Add(unusedNodes1[0]); // to make the nodes create a circle
                // upper layer
                List<Node> unusedNodes2 = Hull.hull_nodes[i + 1];
                unusedNodes2.Add(unusedNodes2[0]);

                int p1 = 0, p2 = 0;
                while (p1 < unusedNodes1.Count - 1 && p2 < unusedNodes2.Count - 1)
                {
                    int startIndex = wallVertices.Count;

                    double diagnal1 = Line.getLength(unusedNodes1[p1], unusedNodes2[p2 + 1]); // |
                    double diagnal2 = Line.getLength(unusedNodes1[p1 + 1], unusedNodes2[p2]); // \

                    if (diagnal1 > diagnal2)
                    {
                        // left two nodes
                        wallVertices.Add(unusedNodes2[p2].getVector());
                        uv.Add(new Vector2((float)p2 / unusedNodes2.Count, (float)i / numLayer));
                        tangents.Add(tangent);
                        wallVertices.Add(unusedNodes1[p1].getVector());
                        uv.Add(new Vector2((float)p1 / unusedNodes1.Count, (float)i / numLayer));
                        tangents.Add(tangent);

                        double baseEdge = Line.getLength(unusedNodes1[p1], unusedNodes1[p1 + 1]);
                        double hypotenuse = Line.getLength(unusedNodes1[p1], unusedNodes2[p2]);

                        // if the shorter diagnal also constructs a obtuse triangle
                        while ((baseEdge * baseEdge + diagnal2 * diagnal2 > hypotenuse * hypotenuse) && p1 < unusedNodes1.Count - 1)
                        {
                            p1++;

                            wallVertices.Add(unusedNodes1[p1].getVector());
                            uv.Add(new Vector2((float)p1 / unusedNodes1.Count, (float)i / numLayer));
                            tangents.Add(tangent);

                            // add left triangle with two directions
                            wallTriangle.Add(startIndex);
                            wallTriangle.Add(wallVertices.Count - 1);
                            wallTriangle.Add(wallVertices.Count - 2);
                            wallTriangle.Add(startIndex);
                            wallTriangle.Add(wallVertices.Count - 2);
                            wallTriangle.Add(wallVertices.Count - 1);

                            baseEdge = Line.getLength(unusedNodes1[p1], unusedNodes1[p1 + 1]);
                            hypotenuse = Line.getLength(unusedNodes1[p1 + 1], unusedNodes2[p2]);
                            // diagnal1 = Line.getLength(unusedNodes1[p1], unusedNodes2[p2 + 1]); // |
                            // diagnal2 = Line.getLength(unusedNodes1[p1 + 1], unusedNodes2[p2]); // \
                        }
                    }
                    else
                    {
                        // left two nodes
                        wallVertices.Add(unusedNodes1[p1].getVector());
                        uv.Add(new Vector2((float)p1 / unusedNodes1.Count, (float)i / numLayer));
                        tangents.Add(tangent);
                        wallVertices.Add(unusedNodes2[p2].getVector());
                        uv.Add(new Vector2((float)p2 / unusedNodes2.Count, (float)i / numLayer));
                        tangents.Add(tangent);

                        double baseEdge = Line.getLength(unusedNodes2[p2], unusedNodes2[p2 + 1]);
                        double hypotenuse = Line.getLength(unusedNodes2[p2], unusedNodes1[p1]);

                        // if the shorter diagnal also constructs a obtuse triangle
                        while ((baseEdge * baseEdge + diagnal1 * diagnal1 > hypotenuse * hypotenuse) && p2 < unusedNodes2.Count - 1)
                        {
                            p2++;

                            wallVertices.Add(unusedNodes2[p2].getVector());
                            uv.Add(new Vector2((float)p2 / unusedNodes2.Count, (float)i / numLayer));
                            tangents.Add(tangent);

                            // add left triangle with two directions
                            wallTriangle.Add(startIndex);
                            wallTriangle.Add(wallVertices.Count - 1);
                            wallTriangle.Add(wallVertices.Count - 2);
                            wallTriangle.Add(startIndex);
                            wallTriangle.Add(wallVertices.Count - 2);
                            wallTriangle.Add(wallVertices.Count - 1);

                            baseEdge = Line.getLength(unusedNodes2[p2], unusedNodes2[p2 + 1]);
                            hypotenuse = Line.getLength(unusedNodes2[p2 + 1], unusedNodes1[p1]);
                        }

                    }

                    diagnal1 = Line.getLength(unusedNodes1[p1], unusedNodes2[p2 + 1]); // |
                    diagnal2 = Line.getLength(unusedNodes1[p1 + 1], unusedNodes2[p2]); // \

                    startIndex = wallVertices.Count;

                    // left two nodes
                    wallVertices.Add(unusedNodes1[p1].getVector());
                    uv.Add(new Vector2((float)p1 / unusedNodes1.Count, (float)i / numLayer));
                    tangents.Add(tangent);
                    wallVertices.Add(unusedNodes2[p2].getVector());
                    uv.Add(new Vector2((float)p2 / unusedNodes2.Count, (float)i / numLayer));
                    tangents.Add(tangent);

                    if (diagnal1 > diagnal2)
                    {
                        wallVertices.Add(unusedNodes1[p1 + 1].getVector());
                        uv.Add(new Vector2((float)(p1 + 1) / unusedNodes1.Count, (float)i / numLayer));
                        tangents.Add(tangent);
                        wallVertices.Add(unusedNodes2[p2 + 1].getVector());
                        uv.Add(new Vector2((float)(p2 + 1) / unusedNodes2.Count, (float)i / numLayer));
                        tangents.Add(tangent);

                        // add left triangle with two directions
                        wallTriangle.Add(startIndex);
                        wallTriangle.Add(startIndex + 1);
                        wallTriangle.Add(startIndex + 2);
                        wallTriangle.Add(startIndex);
                        wallTriangle.Add(startIndex + 2);
                        wallTriangle.Add(startIndex + 1);

                        // right triangle with 2 directions
                        wallTriangle.Add(startIndex + 2);
                        wallTriangle.Add(startIndex + 3);
                        wallTriangle.Add(startIndex + 1);
                        wallTriangle.Add(startIndex + 2);
                        wallTriangle.Add(startIndex + 1);
                        wallTriangle.Add(startIndex + 3);
                    }
                    else
                    {
                        wallVertices.Add(unusedNodes2[p2 + 1].getVector());
                        uv.Add(new Vector2((float)(p2 + 1) / unusedNodes2.Count, (float)i / numLayer));
                        tangents.Add(tangent);
                        wallVertices.Add(unusedNodes1[p1 + 1].getVector());
                        uv.Add(new Vector2((float)(p1 + 1) / unusedNodes1.Count, (float)i / numLayer));
                        tangents.Add(tangent);

                        // add left triangle with two directions
                        wallTriangle.Add(startIndex);
                        wallTriangle.Add(startIndex + 1);
                        wallTriangle.Add(startIndex + 2);
                        wallTriangle.Add(startIndex);
                        wallTriangle.Add(startIndex + 2);
                        wallTriangle.Add(startIndex + 1);

                        // right triangle with 2 directions
                        wallTriangle.Add(startIndex + 2);
                        wallTriangle.Add(startIndex + 3);
                        wallTriangle.Add(startIndex);
                        wallTriangle.Add(startIndex + 3);
                        wallTriangle.Add(startIndex + 2);
                        wallTriangle.Add(startIndex);
                    }

                    p1++;
                    p2++;
                }

                // the nodes that left, directly connect them with the last node of the other layer
                int l2 = wallVertices.Count - 1;
                bool flag = true;
                while (p1 < unusedNodes1.Count)
                {
                    wallVertices.Add(unusedNodes1[p1].getVector());
                    uv.Add(new Vector2((float)p1 / unusedNodes1.Count, (float)i / numLayer));
                    tangents.Add(tangent);

                    wallTriangle.Add(l2);
                    wallTriangle.Add(wallVertices.Count - 1);
                    if (flag)
                    {
                        wallTriangle.Add(wallVertices.Count - 3);
                        flag = false;

                        wallTriangle.Add(l2);
                        wallTriangle.Add(wallVertices.Count - 3);
                        wallTriangle.Add(wallVertices.Count - 1);
                    }
                    else
                    {
                        wallTriangle.Add(wallVertices.Count - 2);

                        wallTriangle.Add(l2);
                        wallTriangle.Add(wallVertices.Count - 2);
                        wallTriangle.Add(wallVertices.Count - 1);
                    }

                    p1++;
                }

                int l1 = wallVertices.Count - 1;
                flag = true;
                while (p2 < unusedNodes2.Count)
                {
                    // Line line = new Line(unusedNodes1[last1], unusedNodes2[p2]);
                    // wallLines.Add(line);
                    wallVertices.Add(unusedNodes2[p2].getVector());
                    uv.Add(new Vector2((float)p2 / unusedNodes2.Count, (float)i / numLayer));
                    tangents.Add(tangent);

                    wallTriangle.Add(l1);
                    wallTriangle.Add(wallVertices.Count - 1);
                    // wallTriangle.Add(wallVertices.Count - 2);

                    if (flag)
                    {
                        wallTriangle.Add(wallVertices.Count - 3);
                        flag = false;

                        wallTriangle.Add(l1);
                        wallTriangle.Add(wallVertices.Count - 3);
                        wallTriangle.Add(wallVertices.Count - 1);
                    }
                    else
                    {
                        wallTriangle.Add(wallVertices.Count - 2);

                        wallTriangle.Add(l1);
                        wallTriangle.Add(wallVertices.Count - 2);
                        wallTriangle.Add(wallVertices.Count - 1);
                    }

                    p2++;
                }
            }

            Mesh wallMesh = new Mesh();
            wallMesh.vertices = wallVertices.ToArray();
            wallMesh.triangles = wallTriangle.ToArray();
            wallMesh.uv = uv.ToArray();
            wallMesh.tangents = tangents.ToArray();

            walls.mesh = wallMesh;
        }

        public void drawWall()
        {
            for (int i = 0; i < Hull.hull_edges.Count - 1; i++)
            {
                // List<Line> edge1 = Hull.hull_edges[i];
                // List<Line> edge2 = Hull.hull_edges[i + 1];
                List<Node> unusedNodes1 = Hull.hull_nodes[i];
                List<Node> unusedNodes2 = Hull.hull_nodes[i + 1];

                // add all node from edge to unusedNodes
                // foreach (var line in edge1)
                // {
                //     if (!unusedNodes1.Contains(line[0])) unusedNodes1.Add(line[0]);
                //     if (!unusedNodes1.Contains(line[1])) unusedNodes1.Add(line[1]);
                // }
                // foreach (var line in edge2)
                // {
                //     if (!unusedNodes2.Contains(line[0])) unusedNodes2.Add(line[0]);
                //     if (!unusedNodes2.Contains(line[1])) unusedNodes2.Add(line[1]);
                // }

                // connect every pair of nodes together
                int p1 = 0, p2 = 0;
                while (p1 < unusedNodes1.Count && p2 < unusedNodes2.Count)
                {
                    Line line = new Line(unusedNodes1[p1], unusedNodes2[p2]);
                    wallLines.Add(line);
                    p1++;
                    p2++;
                }

                int last1 = p1 - 1, last2 = p2 - 1;
                // connect all rest nodes of one edge to the last node of the other edge
                while (p1 < unusedNodes1.Count)
                {
                    Line line = new Line(unusedNodes1[p1], unusedNodes2[last2]);
                    wallLines.Add(line);
                    p1++;
                }

                while (p2 < unusedNodes2.Count)
                {
                    Line line = new Line(unusedNodes1[last1], unusedNodes2[p2]);
                    wallLines.Add(line);
                    p2++;
                }

                if (p1 > p2)
                    connectBackwards(unusedNodes1, unusedNodes2, ref last1, ref last2);
                else connectBackwards(unusedNodes2, unusedNodes1, ref last2, ref last1);
            }
        }

        private void connectBackwards(List<Node> unusedNodes1, List<Node> unusedNodes2, ref int last1, ref int last2)
        {
            int len2 = last2;
            while (last1 > 0 && last2 > 0)
            {
                Line line = new Line(unusedNodes1[last1], unusedNodes2[--last2]);
                wallLines.Add(line);
                last1--;
            }
            wallLines.Add(new Line(unusedNodes1[0], unusedNodes2[len2]));
        }

        public void drawWalls()
        {
            for (int i = 0; i < Hull.hull_edges.Count - 1; i++)
            {
                List<Line> edge1 = Hull.hull_edges[i];
                List<Line> edge2 = Hull.hull_edges[i + 1];
                List<Node> unusedNodes1 = new List<Node>();
                List<Node> unusedNodes2 = new List<Node>();

                // add all node from edge to unusedNodes
                foreach (var Line in edge1)
                {
                    if (!unusedNodes1.Contains(Line[0])) unusedNodes1.Add(Line[0]);
                    if (!unusedNodes1.Contains(Line[1])) unusedNodes1.Add(Line[1]);
                }
                foreach (var Line in edge2)
                {
                    if (!unusedNodes1.Contains(Line[0])) unusedNodes1.Add(Line[0]);
                    if (!unusedNodes1.Contains(Line[1])) unusedNodes1.Add(Line[1]);
                }

                int p1 = 0, p2 = 0;
                while (unusedNodes1.Count > 1 || unusedNodes2.Count > 1)
                {
                    // get square
                    Node leftTop = unusedNodes1[p1 + 1];
                }

                // connect the rest of unused nodes to 
                while (unusedNodes1.Count == 1)
                {

                }
                while (unusedNodes2.Count == 1)
                {

                }
            }
        }
    }
}

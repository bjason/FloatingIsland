using UnityEngine;
using System.Collections.Generic;
using System;

/* TODO 
    issue: concave hull cannot be formed when there is only 1/2 nodes per layer
 */

namespace ConcaveHull
{
    public class Init : MonoBehaviour
    {

        List<List<Node>> dot_lists = new List<List<Node>>(); //Used only for the demo

        public string seed;
        // public int scaleFactor;
        public int numberOfDots;
        // public double concavity;
        // public bool isSquareGrid;
        public int height;

        void Start()
        {
            setDots(numberOfDots); //Used only for the demo
            generateHull();
        }

        public void generateHull()
        {
            foreach (var dot_list in dot_lists)
            {
                Hull.setConvHull(dot_list);
                // Hull.setConcaveHull(Math.Round(System.Convert.ToDecimal(concavity), 2), 40, isSquareGrid);
            }
            // drawWalls();
            MeshGenerator mg = GetComponent<MeshGenerator>();
            mg.drawWallMesh();
            mg.drawHorizontalMesh(height);
        }

        public void setDots(int numberOfDots)
        {
            seed = DateTime.Now.ToString();
            System.Random pseudorandom = new System.Random(seed.GetHashCode());

            int index = 0;
            for (int i = 0; i < height; i++)
            {
                List<Node> dot_list = new List<Node>();
                // To make pile rounded
                int numPerLayer = (int)(numberOfDots * (1f / height) * (i + 1));
                int widthPerLayera = (int)(100 * (1f / height) * (i + 1));

                for (int x = 0; x < numPerLayer; x++)
                {
                    int xpos, ypos;
                    do
                    {
                        xpos = pseudorandom.Next(-widthPerLayera, widthPerLayera);
                        ypos = pseudorandom.Next(-widthPerLayera, widthPerLayera);
                    } while (xpos * xpos + ypos * ypos > widthPerLayera * widthPerLayera);

                    Node curr = new Node(xpos, ypos, pseudorandom.Next(-8, 8) + i * 50, index++);
                    bool alreadyContains = false;
                    dot_list.ForEach(node => alreadyContains = alreadyContains | node.isSame(curr));

                    if (alreadyContains) continue;
                    else dot_list.Add(curr);
                }

                dot_lists.Add(dot_list);
            }

            //Delete repeated nodes
            // for (int pivot_position = 0; pivot_position < dot_list.Count; pivot_position++)
            // {
            //     for (int position = 0; position < dot_list.Count; position++)
            //         if (dot_list[pivot_position].x == dot_list[position].x && dot_list[pivot_position].y == dot_list[position].y
            //             && dot_list[pivot_position].z == dot_list[position].z && dot_list[pivot_position].id != dot_list[position].id)
            //         {
            //             dot_list.RemoveAt(position);
            //             position--;
            //         }
            // }
        }

        void OnDrawGizmos()
        {
            //Convex hull
            Gizmos.color = Color.yellow;
            foreach (var edges in Hull.hull_edges)
            {
                for (int i = 0; i < edges.Count; i++)
                {
                    // Vector3 left = new Vector3((float)edges[i].nodes[0].x, (float)edges[i].nodes[0].z, (float)edges[i].nodes[0].y);
                    // Vector3 right = new Vector3((float)edges[i].nodes[1].x, (float)edges[i].nodes[1].z, (float)edges[i].nodes[1].y);
                    Gizmos.DrawLine(edges[i].nodes[0].getVector(), edges[i].nodes[1].getVector());
                }
            }

            // foreach (var line in wallLines)
            // {
            //     Gizmos.DrawLine(line[0].getVector(), line[1].getVector());
            // }

            //Concave hull
            // Gizmos.color = Color.blue;
            // for (int i = 0; i < Hull.hull_concave_edges.Count; i++)
            // { }

            Gizmos.color = Color.red;
            foreach (var dot_list in dot_lists)
                for (int i = 0; i < dot_list.Count; i++)
                {
                    Gizmos.DrawSphere(dot_list[i].getVector(), 0.5f);
                }
        }
    }

}

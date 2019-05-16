using UnityEngine;

namespace ConcaveHull
{
    public class Node
    {
        public int id;
        public double x;
        public double y;
        public double z;
        public Node(double x, double y, double z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
        public Node(double x, double y, double z, int id)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.id = id;
        }

        public bool isSame(Node node)
        {
            return node.x == this.x && node.y == this.y && node.z == this.z;
        }

        public Vector3 getVector()
        {
            Vector3 res = new Vector3((float)this.x, (float)this.z, (float)this.y);
            return res;
        }
    }
}
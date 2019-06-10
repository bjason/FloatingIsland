using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Network
{
    public Link[] links;
    public GraphProperties graph_properties;
    public Node[] nodes;
}

[System.Serializable]
public class Link
{
    public int source;
    public int target;
    public float value;
}

[System.Serializable]
public class GraphProperties
{
    public string description;
    public string readme;
}

[System.Serializable]
public class Node
{
    public int id;
    public string label;
    public string color;
    public int group;
    public float x;
    public float y;
    public float z;
}

public class LinkObject
{
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 scale;

    public LinkObject(Vector3 p, Quaternion r, Vector3 s)
    {
        position = p;
        rotation = r;
        scale = s;
    }
}
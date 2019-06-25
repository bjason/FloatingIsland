using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class NetworkLoader : MonoBehaviour
{

    public string networkFolder;
    public string networkName;
    public GameObject nodeObject;
    public GameObject linkObject;
    public Transform nodeParent;
    public Transform linkParent;
    public float nodeSize;
    public float linkSize;
    public float threshold = 0.0f;
    public bool useThreshold;
    public bool optimizeMeshes = false;
    public Vector3 dimensions;

    public List<Vector3> nodePositions;
    public List<GameObject> nodes;
    public List<List<LinkObject>> linkPositions;
    public List<GameObject> links;

    void Start()
    {
        LoadNetwork();
    }

    public void LoadNetwork()
    {
        // Debug.Log("Loading Network: " + networkName);

        Network n = readFile();
        List<int> removedNodes = new List<int>();
        InstantiateObjects(n, removedNodes);

        // if (GetComponent<ManipulateNetwork>() != null)
        // {
        //     var mn = GetComponent<ManipulateNetwork>();
        //     mn.nodes = nodePositions;
        //     mn.nodeScale = nodeSize;
        //     mn.links = linkPositions;
        // }
        // else if (GetComponent<ManipulateNetwork2D>() != null)
        // {
        //     var mn = GetComponent<ManipulateNetwork2D>();
        //     mn.nodes = nodePositions;
        //     mn.nodeScale = nodeSize;
        //     mn.links = linkPositions;
        // }
        // else
        // {
        //     Debug.LogError("No suitable network manipulation script found.");
        // }
    }

    Network readFile()
    {
        string filename = "./Assets/" + networkFolder + "/" + networkName;
        return JsonUtility.FromJson<Network>(File.ReadAllText(filename));
    }

    void InstantiateObjects(Network n, List<int> removedNodes)
    {
        //Clean up existing children first
        int i = 0;
        while (transform.childCount > i)
        {
            // if (transform.GetChild(i).CompareTag("NoDestroy"))
            // {
            //     i++;
            // }
            // else
            {
                DestroyImmediate(transform.GetChild(i).gameObject);
            }
        }

        if (nodeParent == null)
        {
            var NodesGO = new GameObject("Nodes");
            NodesGO.transform.SetParent(transform);
            NodesGO.transform.localPosition = Vector3.zero;
            NodesGO.transform.localRotation = Quaternion.identity;
            NodesGO.transform.localScale = Vector3.one;
            nodeParent = NodesGO.transform;
        }
        if (linkParent == null)
        {
            var LinksGO = new GameObject("Links");
            LinksGO.transform.SetParent(transform);
            LinksGO.transform.localPosition = Vector3.zero;
            LinksGO.transform.localRotation = Quaternion.identity;
            LinksGO.transform.localScale = Vector3.one;
            linkParent = LinksGO.transform;
        }

        nodePositions = new List<Vector3>();
        nodes = new List<GameObject>();
        linkPositions = new List<List<LinkObject>>();
        links = new List<GameObject>();

        foreach (Node node in n.nodes)
        {
            if (removedNodes.Contains(nodes.Count))
            {
                nodes.Add(null);
                continue;
            }
            var pos = new Vector3(node.x, node.y, node.z);
            var newNode = Instantiate(nodeObject, nodeParent, false);
            newNode.transform.localPosition = pos;
            newNode.transform.localScale = new Vector3(nodeSize, nodeSize, nodeSize);
            newNode.name = "Node " + nodes.Count;

            if (node.color != null && !optimizeMeshes)
            {
                newNode.GetComponent<MeshRenderer>().material.SetColor("_Color", RGBStringToColor(node.color));
            }

            nodes.Add(newNode);
            nodePositions.Add(newNode.transform.localPosition);
            linkPositions.Add(new List<LinkObject>());
        }

        links = InstantiateLinks(n.links, removedNodes);

        removedNodes.Sort();
        removedNodes.Reverse();
        foreach (int r in removedNodes)
        {
            nodes.RemoveAt(r);
        }

        if (optimizeMeshes)
        {
            OptimizeMeshes(nodeParent, nodeObject);
            OptimizeMeshes(linkParent, linkObject);
        }
    }

    //Instantiate Links with mesh (cube or cylinder)
    List<GameObject> InstantiateLinks(Link[] links, List<int> removedNodes)
    {
        List<GameObject> linkList = new List<GameObject>();

        foreach (Link l in links)
        {
            if (useThreshold && l.value < threshold)
            {
                continue;
            }

            if (removedNodes.Contains(l.source) || removedNodes.Contains(l.target))
            {
                continue;
            }

            var link = Instantiate(linkObject, linkParent, false);
            link.name = "Link " + l.source + " to " + l.target;

            var p1 = nodes[l.source].transform.localPosition;
            var p2 = nodes[l.target].transform.localPosition;

            LinkObject lo = new LinkObject((p1 + p2) / 2, Quaternion.FromToRotation(Vector3.up, p2 - p1), new Vector3(linkSize, Vector3.Distance(p1, p2) * 0.5f, linkSize));

            link.transform.localPosition = lo.position;
            link.transform.localRotation = lo.rotation;
            link.transform.localScale = lo.scale;

            if (removedNodes == null || removedNodes.Count == 0)
            {
                linkList.Add(link);
                linkPositions[l.source].Add(lo);
                linkPositions[l.target].Add(lo);
            }
        }
        return linkList;
    }

    void OptimizeMeshes(Transform parent, GameObject parentObj)
    {
        MeshFilter[] filters = parent.GetComponentsInChildren<MeshFilter>();
        Material mat = parentObj.GetComponentInChildren<MeshRenderer>().sharedMaterial;
        List<List<CombineInstance>> combiners = new List<List<CombineInstance>>();

        int verts = 0;

        foreach (var filter in filters)
        {
            verts += filter.sharedMesh.vertexCount;

            if (verts / 65000 >= combiners.Count)
            {
                combiners.Add(new List<CombineInstance>());
            }

            CombineInstance ci = new CombineInstance();
            ci.subMeshIndex = 0;
            ci.mesh = filter.sharedMesh;
            ci.transform = Matrix4x4.TRS(filter.transform.localPosition, filter.transform.localRotation, filter.transform.localScale);
            combiners[verts / 65000].Add(ci);

        }


        //Delete Children
        while (parent.childCount != 0)
        {
            DestroyImmediate(parent.GetChild(0).gameObject);
        }

        for (int i = 0; i < combiners.Count; i++)
        {
            GameObject submesh = new GameObject();
            submesh.name = parent.name + " " + i;
            submesh.transform.parent = parent;
            submesh.transform.localPosition = Vector3.zero;
            submesh.transform.localRotation = Quaternion.identity;
            submesh.transform.localScale = Vector3.one;

            Mesh mesh = new Mesh();
            MeshFilter meshfilter = submesh.AddComponent<MeshFilter>();
            mesh.CombineMeshes(combiners[i].ToArray());
            meshfilter.sharedMesh = mesh;

            MeshRenderer meshrenderer = submesh.AddComponent<MeshRenderer>();
            meshrenderer.sharedMaterial = mat;

            //submesh.AddComponent<MeshCollider>();
        }
    }

    void ResizeArea(Vector3[] bounds)
    {

    }

    Color RGBStringToColor(string s)
    {
        float r = Convert.ToInt32(s.Substring(1, 2), 16) / 255f;
        float g = Convert.ToInt32(s.Substring(3, 2), 16) / 255f;
        float b = Convert.ToInt32(s.Substring(5, 2), 16) / 255f;

        return new Color(r, g, b);
    }

}

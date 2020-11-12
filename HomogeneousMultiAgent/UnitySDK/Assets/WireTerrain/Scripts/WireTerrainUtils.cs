using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

public static class WireTerrainUtils
{
    private static readonly string meshPartName = "WireTerrainPart";

    public static void GenerateMesh(Transform root, List<Vector3> segments, Material material, float terrainHeight, bool generateUV, bool flatten, Action<List<GameObject>> cleanupAction)
    {
        int maxVerticesCount = 65512;
        int meshPartsCount = Mathf.CeilToInt((float)(segments.Count) / maxVerticesCount);

        MeshFilter[] meshes = GenerateMeshPartsObjects(root, material, meshPartsCount, cleanupAction);
        for (int meshPartIndex = 0; meshPartIndex < meshPartsCount; meshPartIndex++)
        {
            if (meshes[meshPartIndex].sharedMesh == null)
            {
                meshes[meshPartIndex].sharedMesh = new Mesh();
            }

            var segmentsPart = segments.GetRange(meshPartIndex * maxVerticesCount, Math.Min(maxVerticesCount, segments.Count - (meshPartIndex * maxVerticesCount)));

            Vector2[] uv = null;
            if (generateUV)
            {
                uv = new Vector2[segmentsPart.Count];
                for (int i = 0; i < segmentsPart.Count; i++)
                {
                    uv[i] = new Vector2(segmentsPart[i].y / terrainHeight, 0f);
                }
            }

            var indices = new int[segmentsPart.Count];
            for (int i = 0; i < segmentsPart.Count; i++)
            {
                indices[i] = i;
                if (flatten)
                {
                    Vector3 p = segmentsPart[i];
                    p.y = 0;
                    segmentsPart[i] = p;
                }
            }

            meshes[meshPartIndex].sharedMesh.Clear();
            meshes[meshPartIndex].sharedMesh.vertices = segmentsPart.ToArray();
            meshes[meshPartIndex].sharedMesh.SetIndices(indices, MeshTopology.Lines, 0);
            if (generateUV)
            {
                meshes[meshPartIndex].sharedMesh.uv = uv;
            }
            else
            {
                meshes[meshPartIndex].sharedMesh.uv = null;
            }

            meshes[meshPartIndex].sharedMesh.RecalculateBounds();
        }
    }

    private static MeshFilter[] GenerateMeshPartsObjects(Transform root, Material material, int meshPartsCount, Action<List<GameObject>> cleanupAction)
    {
        List<GameObject> toDestroy = new List<GameObject>();
        MeshFilter[] mfs = new MeshFilter[meshPartsCount];
        int existedPartsCount = 0;
        foreach (Transform child in root)
        {
            if (child.name == meshPartName)
            {
                if (existedPartsCount < meshPartsCount)
                {
                    mfs[existedPartsCount] = child.GetComponent<MeshFilter>();

                    MeshRenderer mr = child.GetComponent<MeshRenderer>();
                    mr.sharedMaterial = material;
                }
                else
                {
                    toDestroy.Add(child.gameObject);
                }
                existedPartsCount++;
            }
        }

        if (existedPartsCount < meshPartsCount)
        {
            int createCount = meshPartsCount - existedPartsCount;
            for (int i = 0; i < createCount; i++)
            {
                GameObject newMeshPart = new GameObject(meshPartName);
                newMeshPart.transform.SetParent(root, false);
                MeshFilter mf = newMeshPart.AddComponent<MeshFilter>();
                MeshRenderer mr = newMeshPart.AddComponent<MeshRenderer>();
                mr.sharedMaterial = material;
                mfs[existedPartsCount + i] = mf;
            }
        }

        if (0 < toDestroy.Count)
        {
            cleanupAction(toDestroy);
        }

        return mfs;
    }

    internal static void GenerateMesh(Transform root, Vector3[] vertices, int[] indices, Material material, float terrainHeight, bool generateUV, bool flatten, Action<List<GameObject>> cleanupAction)
    {
        int maxVerticesCount = 65512;
        int partIndex = 0;
        HashSet<int> checkIndices = new HashSet<int>();
        List<Vector3> partVertices = new List<Vector3>();
        List<int> partIndices = new List<int>();

        int[] oldToNew = new int[indices.Length];
        int newIndex = 0;
        for (int segmentIndex = 0; segmentIndex < indices.Length; segmentIndex += 2)
        {
            if (!checkIndices.Contains(indices[segmentIndex]))
            {
                checkIndices.Add(indices[segmentIndex]);
                partVertices.Add(vertices[indices[segmentIndex]]);
                oldToNew[indices[segmentIndex]] = partVertices.Count - 1;
                newIndex++;
            }

            if (!checkIndices.Contains(indices[segmentIndex + 1]))
            {
                checkIndices.Add(indices[segmentIndex + 1]);
                partVertices.Add(vertices[indices[segmentIndex + 1]]);
                oldToNew[indices[segmentIndex + 1]] = partVertices.Count - 1;
                newIndex++;
            }
            partIndices.Add(oldToNew[indices[segmentIndex]]);
            partIndices.Add(oldToNew[indices[segmentIndex + 1]]);

            if (maxVerticesCount < partVertices.Count || indices.Length <= segmentIndex + 2)
            {                
                MeshFilter meshFilter = GetMeshPartObject(root, material, partIndex);
                if (meshFilter.sharedMesh == null)
                {
                    meshFilter.sharedMesh = new Mesh();
                }

                Vector2[] uv = null;
                if (generateUV)
                {
                    uv = new Vector2[partVertices.Count];
                    for (int i = 0; i < partVertices.Count; i++)
                    {
                        uv[i] = new Vector2(partVertices[i].y / terrainHeight, 0f);
                    }                    
                }

                if (flatten)
                {
                    for (int i = 0; i < partVertices.Count; i++)
                    {
                        Vector3 p = partVertices[i];
                        p.y = 0;
                        partVertices[i] = p;
                    }
                }

                meshFilter.sharedMesh.Clear();
                meshFilter.sharedMesh.vertices = partVertices.ToArray();
                if(uv != null){ meshFilter.sharedMesh.uv = uv; }
                meshFilter.sharedMesh.SetIndices(partIndices.ToArray(), MeshTopology.Lines, 0);
                meshFilter.sharedMesh.RecalculateBounds();

                partIndex++;
                partVertices.Clear();
                partIndices.Clear();
                checkIndices.Clear();
                newIndex = 0;
            }
        }

        ///Cleanup
        List<GameObject> toDestroy = new List<GameObject>();
        int existedPartsCount = 0;
        foreach (Transform child in root)
        {
            if (child.name == meshPartName)
            {
                if (partIndex <= existedPartsCount)
                {
                    toDestroy.Add(child.gameObject);
                }
                existedPartsCount++;
            }
        }

        if (0 < toDestroy.Count)
        {
            cleanupAction(toDestroy);
        }
    }

    private static MeshFilter GetMeshPartObject(Transform root, Material material, int index)
    {
        MeshFilter mf = null;
        int existedPartsCount = 0;
        foreach (Transform child in root)
        {
            if (child.name == meshPartName)
            {
                if (existedPartsCount == index)
                {
                    MeshRenderer mr = child.GetComponent<MeshRenderer>();
                    mr.sharedMaterial = material;
                    mf = child.GetComponent<MeshFilter>();
                }
                existedPartsCount++;
            }
        }

        if (mf == null)
        {
            GameObject newMeshPart = new GameObject(meshPartName);
            newMeshPart.transform.SetParent(root, false);
            mf = newMeshPart.AddComponent<MeshFilter>();
            MeshRenderer mr = newMeshPart.AddComponent<MeshRenderer>();
            mr.sharedMaterial = material;
        }

        return mf;
    }

    public static void Optimize(List<Vector3> lineSegments, float xStep, float zStep, Vector3 size, out Vector3[] vertices, out int[] indices)
    {
        float threshold = (xStep + zStep) / 1000f;
        float bucketStepX = xStep * 5.5f;
        float bucketStepY = zStep * 5.5f;

        AutoWeld(lineSegments, threshold, bucketStepX, bucketStepY, size, out vertices, out indices);
    }

    private static void AutoWeld(List<Vector3> lineSegments, float threshold, float bucketStepX, float bucketStepZ, Vector3 size, out Vector3[] vertices, out int[] indices)
    {
        Vector3[] newVertices = new Vector3[lineSegments.Count];
        int[] old2new = new int[lineSegments.Count];
        int newSize = 0;

        // Find AABB
        Vector3 min = Vector3.zero;
        Vector3 max = size;

        // Make rectangular buckets, each with dimensions "bucketStep"
        int bucketSizeX = Mathf.FloorToInt((max.x - min.x) / bucketStepX) + 1;
        int bucketSizeZ = Mathf.FloorToInt((max.z - min.z) / bucketStepZ) + 1;
        List<int>[,] buckets = new List<int>[bucketSizeX, bucketSizeZ];

        // Make new vertices
        for (int i = 0; i < lineSegments.Count; i++)
        {
            // Determine which bucket it belongs to
            int x = Mathf.FloorToInt((lineSegments[i].x - min.x) / bucketStepX);
            int z = Mathf.FloorToInt((lineSegments[i].z - min.z) / bucketStepZ);

            // Check to see if it's already been added
            if (buckets[x, z] == null)
            {
                buckets[x, z] = new List<int>(); // Make buckets lazily
            }

            for (int j = 0; j < buckets[x, z].Count; j++)
            {
                Vector3 to = newVertices[buckets[x, z][j]] - lineSegments[i];
                if (Vector3.SqrMagnitude(to) < threshold)
                {
                    old2new[i] = buckets[x, z][j];
                    goto skip; // Skip to next old vertex if this one is already there
                }
            }

            // Add new vertex
            newVertices[newSize] = lineSegments[i];
            buckets[x, z].Add(newSize);
            old2new[i] = newSize;
            newSize++;

            skip:;
        }

        // Make new lines
        var oldIndices = new int[lineSegments.Count];
        for (int i = 0; i < lineSegments.Count; i++)
        {
            oldIndices[i] = i;
        }
        indices = new int[oldIndices.Length];
        for (int i = 0; i < oldIndices.Length; i++)
        {
            indices[i] = old2new[oldIndices[i]];
        }

        vertices = new Vector3[newSize];
        for (int i = 0; i < newSize; i++)
        {
            vertices[i] = newVertices[i];
        }
    }

    public static void AddBounds(Vector3 terrainSize, ref Vector3[] vertices, ref int[] indices)
    {
        var verticesWithBound = new Vector3[vertices.Length + 4];
        Array.Copy(vertices, verticesWithBound, vertices.Length);

        verticesWithBound[vertices.Length + 0] = Vector3.zero;
        verticesWithBound[vertices.Length + 1] = new Vector3(terrainSize.x, 0, 0);
        verticesWithBound[vertices.Length + 2] = new Vector3(terrainSize.x, 0, terrainSize.z);
        verticesWithBound[vertices.Length + 3] = new Vector3(0, 0, terrainSize.z);

        int verticesLength = vertices.Length;
        var indicesWithBound = new int[indices.Length + 8];
        Array.Copy(indices, indicesWithBound, indices.Length);
        indicesWithBound[indices.Length + 0] = verticesLength;
        indicesWithBound[indices.Length + 1] = verticesLength + 1;
        indicesWithBound[indices.Length + 2] = verticesLength + 1;
        indicesWithBound[indices.Length + 3] = verticesLength + 2;
        indicesWithBound[indices.Length + 4] = verticesLength + 2;
        indicesWithBound[indices.Length + 5] = verticesLength + 3;
        indicesWithBound[indices.Length + 6] = verticesLength + 3;
        indicesWithBound[indices.Length + 7] = verticesLength;


        vertices = verticesWithBound;
        indices = indicesWithBound;
    }
}

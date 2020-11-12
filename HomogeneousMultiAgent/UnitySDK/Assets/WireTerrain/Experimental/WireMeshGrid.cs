using UnityEngine;
using System.Collections;
using System;

namespace WireTerrain
{
    public class WireMeshGrid : MonoBehaviour
    {
        /// <summary>
        /// Incoming triangle meshes. 
        /// </summary>
        [SerializeField]
        public MeshFilter sourceTerrain;        

        /// <summary>
        /// Number of cells in first direction 
        /// </summary>
        [SerializeField]
        private int cellCountX;

        /// <summary>
        /// Number of cells in second direction 
        /// </summary>
        [SerializeField]
        private int cellCountZ;

        /// <summary>
        /// Does we need UV coordinates 
        /// U coordinate calculated along Y (up) direction
        /// </summary>
        [SerializeField]
        protected bool generateUV;

        /// <summary>
        /// If "True" generate two submeshes one with Lines and other with Triangles
        /// </summary>
        [SerializeField]
        private bool addPolygonMesh;

        /// <summary>
        /// Resulting line mesh. 
        /// </summary>
        [SerializeField]
        public MeshFilter targetMesh;

        /// <summary>
        /// Vertices for mesh
        /// </summary>
        protected Vector3[] vertices;

        /// <summary>
        /// Lines indices for mesh
        /// </summary>
        protected int[] indices;

        /// <summary>
        /// UV coordinates (only "U" component is used, it's mapped to height)
        /// </summary>
        protected Vector2[] uv;

        /// <summary>
        /// Mesh of the primitive
        /// </summary>
        protected Mesh mesh;

        public int MeshVertexCount { get { return (cellCountX + 1) * (cellCountZ + 1); } }

        public int CellCountX
        {
            get { return cellCountX; }
            set
            {
                if (cellCountX != value)
                {
                    cellCountX = value;
                    OnValidate();
                }
            }
        }

        public int CellCountZ
        {
            get { return cellCountZ; }
            set
            {
                if (cellCountZ != value)
                {
                    cellCountZ = value;
                    OnValidate();
                }
            }
        }

        void CreateMesh()
        {
            mesh = new Mesh();

            if (targetMesh == null)
            {
                targetMesh = GetComponent<MeshFilter>();
            }

            if (targetMesh != null)
            {
                targetMesh.sharedMesh = mesh;
            }
        }

        void OnValidate()
        {
            if (MeshVertexCount < 65000)
            {
                ComputeWireTerrainMesh();
            }
            else
            {
                Debug.Log("Wire terrain mesh must not exceed 65000 vertices");
            }
        }
        private void ComputeWireTerrainMesh()
        {
            if (mesh == null)
            {
                CreateMesh();
            }

            if (sourceTerrain != null && sourceTerrain.sharedMesh != null && targetMesh != null && 0 < cellCountX && 0 < cellCountZ)
            {
                int vertexCountX = cellCountX + 1;
                int vertexCountZ = cellCountZ + 1;

                int meshVertexCount = vertexCountX * vertexCountZ;
                vertices = new Vector3[meshVertexCount];

                int linesCountX = cellCountX * vertexCountZ;
                int linesCountZ = cellCountZ * vertexCountX;
                int linesCount = linesCountX + linesCountZ;

                indices = new int[linesCount * 2];
                int lineFirstVertexIndex = 0;
                int index = 0;
                for (int i = 0; i < vertexCountZ; i++)
                {
                    for (int j = 0; j < cellCountX; j++)
                    {
                        indices[index] = lineFirstVertexIndex;
                        indices[index + 1] = lineFirstVertexIndex + 1;
                        lineFirstVertexIndex++;
                        index += 2;
                    }
                    lineFirstVertexIndex++;
                }

                lineFirstVertexIndex = 0;
                for (int i = 0; i < cellCountZ; i++)
                {
                    for (int j = 0; j < vertexCountX; j++)
                    {
                        indices[index] = lineFirstVertexIndex;
                        indices[index + 1] = lineFirstVertexIndex + vertexCountX;
                        lineFirstVertexIndex++;
                        index += 2;
                    }
                }

                Bounds bb = GetWorldBounds(sourceTerrain);
                float normalizedX = 0;
                float normalizedZ = 0;
                float normalizedXStep = 1f / (float)cellCountX;
                float normalizedZStep = 1f / (float)cellCountZ;        

                Vector3 terrainSize = bb.size;

                for (int i = 0; i < vertexCountZ; i++)
                {
                    normalizedX = 0;
                    for (int j = 0; j < vertexCountX; j++)
                    {
                        Vector3 p = RaycastPoint(sourceTerrain, normalizedX, normalizedZ, bb);
                        vertices[i * vertexCountX + j] = p;
                        normalizedX += normalizedXStep;
                    }
                    normalizedZ += normalizedZStep;
                }

                if (generateUV)
                {
                    var terrainHeight = terrainSize.y;
                    uv = new Vector2[vertices.Length];
                    for (int i = 0; i < vertices.Length; i++)
                    {
                        uv[i] = new Vector2(vertices[i].y / terrainHeight, 0f);
                    }
                }

                mesh.Clear();
                mesh.vertices = vertices;
                if (generateUV)
                {
                    mesh.uv = uv;
                }
                else
                {
                    mesh.uv = null;
                }

                if (addPolygonMesh)
                {
                    mesh.subMeshCount = 2;
                    var indicesPolygons = ComputTriangles(cellCountX, cellCountZ);
                    mesh.SetIndices(indicesPolygons, MeshTopology.Triangles, 0);
                    mesh.SetIndices(indices, MeshTopology.Lines, 1);
                }
                else
                {
                    mesh.subMeshCount = 1;
                    mesh.SetIndices(indices, MeshTopology.Lines, 0);
                }

                mesh.RecalculateBounds();
                targetMesh.sharedMesh = mesh;
            }
        }

        public static Vector3 RaycastPoint(MeshFilter sourceTerrain, float x, float z, Bounds worldBB)
        {
            int layerMask = 1 << sourceTerrain.gameObject.layer;
            x = x * worldBB.size.x;
            z = z * worldBB.size.z;
            Vector3 p = worldBB.min + new Vector3(x, worldBB.size.y * 1.1f, z);
            Vector3 shift = sourceTerrain.transform.position - worldBB.min;
            RaycastHit hit;
            if (Physics.Raycast(p, Vector3.down, out hit, worldBB.size.y * 2, layerMask))
            {
                p = hit.point + shift;
            }
            else
            {
                p = new Vector3(x, worldBB.min.y, z);
            }

            return p;
        }

        public static int ComputeLayerMask(MeshFilter[] mfs)
        {
            int layerMask = 0;
            foreach (var mf in mfs)
            {
                layerMask = layerMask | (1 << mf.gameObject.layer);
            }
            return layerMask;
        }
        public static Vector3 RaycastPoint(MeshFilter[] sourceTerrain, float x, float z, Bounds worldBB, int layerMask)
        {
            x = x * worldBB.size.x;
            z = z * worldBB.size.z;
            Vector3 p = worldBB.min + new Vector3(x, worldBB.size.y * 1.1f, z);
            RaycastHit hit;
            if (Physics.Raycast(p, Vector3.down, out hit, worldBB.size.y * 2, layerMask))
            {
                p = hit.point - worldBB.min;
            }
            else
            {
                p = new Vector3(x, 0, z);
            }

            return p;
        }

        public static Bounds GetWorldBounds(MeshFilter mf)
        {
            Bounds localBB = mf.sharedMesh.bounds;
            Bounds bb = new Bounds();
            bb.SetMinMax(mf.transform.TransformPoint(localBB.min), mf.transform.TransformPoint(localBB.max));
            return bb;
        }

        public static Bounds GetWorldBounds(MeshFilter[] mfs)
        {
            Bounds combinedBB = new Bounds();
            bool first = true;
            foreach (var mf in mfs)
            {
                if (first)
                {
                    combinedBB = GetWorldBounds(mf);
                    first = false;
                }
                else
                {
                    combinedBB.Encapsulate(GetWorldBounds(mf));
                }
            }
            return combinedBB;
        }

        private int[] ComputTriangles(int cellCountX, int vertexCountZ)
        {
            var indicesPolygons = new int[cellCountX * cellCountZ * 6];
            int indicesIndex = 0;
            int LLIndex;
            int TLIndex;
            for (int i = 0; i < cellCountZ; i++)
            {
                for (int j = 0; j < cellCountX; j++)
                {
                    LLIndex = i * (cellCountX + 1) + j;
                    TLIndex = LLIndex + (cellCountX + 1);
                    indicesPolygons[indicesIndex + 0] = LLIndex;
                    indicesPolygons[indicesIndex + 1] = TLIndex + 1;
                    indicesPolygons[indicesIndex + 2] = LLIndex + 1;

                    indicesPolygons[indicesIndex + 3] = LLIndex;
                    indicesPolygons[indicesIndex + 4] = TLIndex;
                    indicesPolygons[indicesIndex + 5] = TLIndex + 1;
                    indicesIndex += 6;
                }
            }
            return indicesPolygons;
        }
    }
}

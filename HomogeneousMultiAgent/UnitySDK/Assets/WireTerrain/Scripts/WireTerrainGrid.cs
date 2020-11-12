using UnityEngine;
using System.Collections;
using System;

namespace WireTerrain
{
    [AddComponentMenu("WireTerrain/Wire Grid")]
    public class WireTerrainGrid : MonoBehaviour
    {
        /// <summary>
        /// Incoming triangle meshes. 
        /// </summary>
        public Terrain sourceTerrain;
        //Terrain t = new Terrain();
        //t.terrainData.

        /// <summary>
        /// Resulting line mesh. 
        /// </summary>
        public MeshFilter targetMesh;

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
        /// Vertices for mesh
        /// </summary>
        protected Vector3[] vertices;

        /// <summary>
        /// Lines indices for mesh
        /// </summary>
        protected int[] indices;

        /// <summary>
        /// UV coordinates (only "U" component is used)
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

            if (sourceTerrain != null && targetMesh != null && 0 < cellCountX && 0 < cellCountZ)
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

                var terrainData = sourceTerrain.terrainData;
                float normalizedX = 0;
                float normalizedZ = 0;
                float normalizedXStep = 1f / (float)cellCountX;
                float normalizedZStep = 1f / (float)cellCountZ;                

                Vector3 terrainSize = terrainData.size;
                float h;
                for (int i = 0; i < vertexCountZ; i++)
                {
                    normalizedX = 0;
                    for (int j = 0; j < vertexCountX; j++)
                    {
                        h = terrainData.GetInterpolatedHeight(normalizedX, normalizedZ);
                        vertices[i * vertexCountX + j] = new Vector3(normalizedX * terrainSize.x, h, normalizedZ * terrainSize.z);
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

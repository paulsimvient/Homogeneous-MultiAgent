using UnityEngine;
using System.Collections;
using System;
using OxyPlot;
using System.Collections.Generic;

namespace WireTerrain
{
    public class WireMeshContours : MonoBehaviour
    {
        /// <summary>
        /// Terrain from which we extract data 
        /// </summary>
        [SerializeField]
        private MeshFilter[] sourceMeshes;        

        /// <summary>
        /// Start Y value for contours (first contours would lie on this height)
        /// </summary>
        [SerializeField]
        private float heightStart = 0.1f;

        /// <summary>
        /// Distance between planes that dissects terrain to produce contours
        /// </summary>
        [SerializeField]
        private float heightStep;

        /// <summary>
        /// Number of cells in X direction 
        /// </summary>
        [SerializeField]
        private int cellCountX;

        /// <summary>
        /// Number of cells in Z direction 
        /// </summary>
        [SerializeField]
        private int cellCountZ;

        /// <summary>
        /// If True all contours will have zero level (y coordinate == 0)
        /// </summary>
        [SerializeField]
        private bool flatten;

        /// <summary>
        ///  If "true" draws a rectangular frame with size of Terrain on zero height
        /// </summary>
        [SerializeField]
        private bool drawBound;

        /// <summary>
        /// Does we need UV coordinates 
        /// U coordinate calculated along Y (up) direction
        /// </summary>
        [SerializeField]
        protected bool generateUV;

        ///// <summary>
        ///// Resulting wire mesh. 
        ///// </summary>
        //[SerializeField]
        //private MeshFilter targetMesh;

        /// <summary>
        /// Mesh optimization (duplicate vertices removing)
        /// </summary>
        [SerializeField]
        protected bool optimize = true;

        /// <summary>
        /// Resulting wire mesh. 
        /// </summary>
        [SerializeField]
        private Material material;

        /// <summary>
        /// Vertices for mesh
        /// </summary>
        protected Vector3[] vertices;

        /// <summary>
        /// Lines indices for mesh
        /// </summary>
        protected int[] indices;

        /// <summary>
        /// UV coordinates (only "U" component is used). U coordinate mapped to vertex height, V coordinate is zero
        /// </summary>
        protected Vector2[] uv;

        /// <summary>
        /// Mesh of the primitive
        /// </summary>
        protected Mesh mesh;

        protected List<Vector3> lineSegments = new List<Vector3>();
        private int contoursLevelsCount;

        /// <summary>
        /// Start Y value for contours (first contours would lie on this height)
        /// </summary>
        public float HeightStart
        {
            get
            {
                return heightStart;
            }

            set
            {
                if (heightStart != value)
                {
                    heightStart = value;
                    ComputeWireTerrainContoursMesh();
                }
            }
        }

        /// <summary>
        /// Distance between planes that dissects terrain to produce contours
        /// </summary>
        public float HeightStep
        {
            get
            {
                return heightStep;
            }

            set
            {
                if (heightStep != value)
                {
                    heightStep = value;
                    ComputeWireTerrainContoursMesh();
                }
            }
        }

        /// <summary>
        /// Number of cells in X direction 
        /// </summary>
        public int CellCountX
        {
            get
            {
                return cellCountX;
            }

            set
            {
                if (cellCountX != value)
                {
                    cellCountX = value;
                    ComputeWireTerrainContoursMesh();
                }
            }
        }

        /// <summary>
        /// Number of cells in Z direction 
        /// </summary>
        public int CellCountZ
        {
            get
            {
                return cellCountZ;
            }

            set
            {
                if (cellCountZ != value)
                {
                    cellCountZ = value;
                    ComputeWireTerrainContoursMesh();
                }
            }
        }

        public void CollectLineSegment(double x1, double y1, double x2, double y2, double z)
        {
            lineSegments.Add(new Vector3((float)x1, (float)z, (float)y1));
            lineSegments.Add(new Vector3((float)x2, (float)z, (float)y2));
        }

        void OnValidate()
        {
            ComputeWireTerrainContoursMesh();
        }

        private void ComputeWireTerrainContoursMesh()
        {
            if (sourceMeshes != null && 0 < cellCountX && 0 < cellCountZ && 0 < heightStep)
            {
                lineSegments.Clear();
                int vertexCountX = cellCountX + 1;
                int vertexCountZ = cellCountZ + 1;

                float normalizedX = 0;
                float normalizedZ = 0;
                float normalizedXStep = 1f / (float)cellCountX;
                float normalizedZStep = 1f / (float)cellCountZ;

                double[,] heightsData = new double[vertexCountX, vertexCountZ];
                double[] xs = new double[vertexCountX];
                double[] ys = new double[vertexCountZ];
                
                Bounds bb = WireMeshGrid.GetWorldBounds(sourceMeshes);
                Vector3 terrainSize = bb.size;
                int layerMask = WireMeshGrid.ComputeLayerMask(sourceMeshes);

                for (int i = 0; i < vertexCountZ; i++)
                {
                    normalizedX = 0;
                    Vector3 p;
                    for (int j = 0; j < vertexCountX; j++)
                    {
                        p = WireMeshGrid.RaycastPoint(sourceMeshes, normalizedX, normalizedZ, bb, layerMask);
                        heightsData[j, i] = p.y;// h;
                        xs[j] = p.x;// normalizedX * terrainSize.x;

                        normalizedX += normalizedXStep;
                    }
                    ys[i] = normalizedZ * terrainSize.z;
                    normalizedZ += normalizedZStep;
                }

                var terrainHeight = terrainSize.y;
                contoursLevelsCount = (int)(terrainHeight / heightStep) + 1;
                double[] zs = new double[contoursLevelsCount];
                for (int i = 0; i < contoursLevelsCount; i++)
                {
                    zs[i] = heightStart + i * heightStep;
                }

                Conrec.Contour(heightsData, xs, ys, zs, CollectLineSegment);

                if (optimize)
                {
                    Vector3[] vertices;
                    int[] indices;
                    WireTerrainUtils.Optimize(lineSegments, terrainSize.x / cellCountX, terrainSize.z / cellCountZ, terrainSize, out vertices, out indices);

                    if (drawBound)
                    {
                        WireTerrainUtils.AddBounds(terrainSize, ref vertices, ref indices);
                    }

                    WireTerrainUtils.GenerateMesh(transform, vertices, indices, material, terrainHeight, generateUV, flatten, InvokeCleanup);
                }
                else
                {
                    if (drawBound)
                    {
                        lineSegments.Add(Vector3.zero);
                        lineSegments.Add(new Vector3(terrainSize.x, 0, 0));
                        lineSegments.Add(new Vector3(terrainSize.x, 0, 0));
                        lineSegments.Add(new Vector3(terrainSize.x, 0, terrainSize.z));
                        lineSegments.Add(new Vector3(terrainSize.x, 0, terrainSize.z));
                        lineSegments.Add(new Vector3(0, 0, terrainSize.z));
                        lineSegments.Add(new Vector3(0, 0, terrainSize.z));
                        lineSegments.Add(Vector3.zero);
                    }

                    WireTerrainUtils.GenerateMesh(transform, lineSegments, material, terrainHeight, generateUV, flatten, InvokeCleanup);
                }
            }
        }

        private void InvokeCleanup(List<GameObject> toDestroy)
        {
            StartCoroutine(Cleanup(toDestroy));
        }

        private IEnumerator Cleanup(List<GameObject> toDestroy)
        {
            yield return null;
            foreach (GameObject go in toDestroy)
            {
#if UNITY_EDITOR
                GameObject.DestroyImmediate(go);
#else
                GameObject.Destroy(go);
#endif
            }
        }
    }
}

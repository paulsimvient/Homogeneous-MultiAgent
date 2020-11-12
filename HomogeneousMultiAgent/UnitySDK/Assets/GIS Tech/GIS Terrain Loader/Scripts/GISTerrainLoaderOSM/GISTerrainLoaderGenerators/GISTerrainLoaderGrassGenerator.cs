using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace GISTech.GISTerrainLoader
{
    public class GISTerrainLoaderGrassGenerator
    {
        private static List<DetailPrototype> DetailPrototypes;
        private static List<GISTerrainLoaderSO_Grass> GrassPrefabs;
        private static TerrainContainerObject container;
        private static float GrassScaleFactor;
        private static float DetailDistance;
        private static List<int[,]> details;
        private static List<string> alreadyCreated;

        public static void GenerateGrass(TerrainContainerObject m_container, List<GISTerrainLoaderSO_Grass> m_GrassPrefabs, float m_GrassDensity, float m_GrassScaleFactor,float m_DetailDistance, Dictionary<string, OSMNode> nodes, Dictionary<string, OSMWay> ways, List<OSMMapMembers> relations)
        {
            GrassPrefabs = m_GrassPrefabs;
            container = m_container;
            GrassScaleFactor = m_GrassScaleFactor;
            DetailDistance = m_DetailDistance;

            AddDetailsLayersToTerrains();

            TerrainData tdata = container.terrains[0, 0].terrainData;
            int detailResolution = tdata.detailResolution;


            alreadyCreated = new List<string>();

            details = new List<int[,]>(container.terrains.Length);

            foreach (var item in container.terrains)
            {
                for (int i = 0; i < GrassPrefabs.Count; i++)
                    details.Add(new int[detailResolution, detailResolution]);
            }

            var detailsInPoint = new float[GrassPrefabs.Count];

            var grassWays = new List<OSMWay>();

            foreach (KeyValuePair<string, OSMWay> pair in ways)
            {

                OSMWay w = pair.Value;
                if (w.HasTags("landuse", "grass", "farmland", "forest", "meadow", "park", "pasture", "recreation_ground") ||
                    w.HasTags("leisure", "park", "golf_course") || w.HasTags("natural", "scrub", "wood"))

                    grassWays.Add(w);
            }

            var totalCount = grassWays.Count + container.terrainCount.x;

            float density = m_GrassDensity / 100f;

            if (density > 1) density = 1;

            density *= 64;
 
            for (int i = 0; i < grassWays.Count; i++)
            {
                OSMWay way = grassWays[i];

                if (alreadyCreated.Contains(way.id)) continue;
                alreadyCreated.Add(way.id);

                if (way.nodeRefs.Count == 0) continue;

                List<Vector3> Points = new List<Vector3>();

                float pxmin = float.MaxValue, pxmax = float.MinValue, pymin = float.MaxValue, pymax = float.MinValue;

                for (int m = 0; m < way.nodeRefs.Count; m++)
                {
                    string nodeRef = way.nodeRefs[m];

                    OSMNode node;
                    if (!nodes.TryGetValue(nodeRef, out node)) continue;


                    var NodeP_Merc = GeoRefConversion.LatLongToMercat(node.Longitude, node.Latitude);

                    Vector3 wspostion = GeoRefConversion.MercatCoordsToWorld(NodeP_Merc.x, 0, NodeP_Merc.y, container) - container.transform.position;

                    wspostion = new Vector3(wspostion.x / tdata.size.x * detailResolution, 0, wspostion.z / tdata.size.z * detailResolution);

                    if (wspostion.x < pxmin) pxmin = wspostion.x;
                    if (wspostion.x > pxmax) pxmax = wspostion.x;
                    if (wspostion.z < pymin) pymin = wspostion.z;
                    if (wspostion.z > pymax) pymax = wspostion.z;

                    Points.Add(wspostion);

                }

                if (Points.Count < 3) continue;

                Vector3[] points = Points.ToArray();
                for (int x = (int)pxmin; x < pxmax; x++)
                {
                    int tix = Mathf.FloorToInt(x / (float)detailResolution);
                    if (tix < 0 || tix >= container.terrainCount.x) continue;

                    int tx = x - tix * detailResolution;

                    for (int y = (int)pymin; y < pymax; y++)
                    {
                        int tiy = Mathf.FloorToInt(y / (float)detailResolution);
                        if (tiy >= container.terrainCount.y || tiy < 0) continue;

                        int tIndex = tiy * container.terrainCount.x + tix;
                        if (tIndex < 0 || tIndex >= container.terrains.Length) continue;

                        bool intersect = Extensions.IsPointInPolygon(points, x + 0.5f, y - 0.5f);
                        if (!intersect) continue;

                        int ty = y - tiy * detailResolution;

                        if (GrassPrefabs.Count == 1) details[tIndex][ty, tx] = (int)density;
                        else
                        {
                            float totalInPoint = 0;

                            int tIndex2 = tIndex * GrassPrefabs.Count;

                            for (int k = 0; k < GrassPrefabs.Count; k++)
                            {
                                float v = Random.Range(0f, 1f);
                                detailsInPoint[k] = v;
                                totalInPoint += v;
                            }

                            for (int k = 0; k < GrassPrefabs.Count; k++)
                            {
                                int v = (int)(detailsInPoint[k] / totalInPoint * density);
                                if (v > 255) v = 255;
                                details[tIndex2 + k][ty, tx] = v;
                            }
                        }
                    }
                }


            }


            for (int x = 0; x < container.terrainCount.x; x++)
            {
                for (int y = 0; y < container.terrainCount.y; y++)
                {
                    for (int prefabIndex = 0; prefabIndex < GrassPrefabs.Count; prefabIndex++)
                    {

                        int tIndex = y * container.terrainCount.x + x;

                        container.terrains[x, y].terrainData.SetDetailLayer(0, 0, prefabIndex,
                             details[tIndex * GrassPrefabs.Count + prefabIndex]);

                    }
                }
            }




        }
        private static void AddDetailsLayersToTerrains()
        {
            //Add Details To Terrains 
            DetailPrototypes = new List<DetailPrototype>();

            foreach (var element in GrassPrefabs)
            {
                if (element.EnableModelUsing)
                {
                    DetailPrototypes.Add(CopyDetailPrototype(element));
                }
            }

            foreach (var terrain in container.terrains)
            {
                terrain.terrainData.detailPrototypes = DetailPrototypes.ToArray();
                terrain.terrain.detailObjectDistance = DetailDistance;
            }
        }

        private static DetailPrototype CopyDetailPrototype(GISTerrainLoaderSO_Grass Source_item)
        {
            var detailPrototype = new DetailPrototype();

            detailPrototype.renderMode = DetailRenderMode.GrassBillboard;

            detailPrototype.prototypeTexture = Source_item.DetailTexture;
            detailPrototype.minWidth = Source_item.MinWidth;
            detailPrototype.maxWidth = Source_item.MaxWidth * GrassScaleFactor;
            detailPrototype.minHeight = Source_item.MinHeight;
            detailPrototype.maxHeight = Source_item.MaxHeight * GrassScaleFactor; detailPrototype.noiseSpread = Source_item.Noise;
            detailPrototype.healthyColor = Source_item.HealthyColor;
            detailPrototype.dryColor = Source_item.DryColor;

            if (Source_item.BillBoard)
                detailPrototype.renderMode = DetailRenderMode.GrassBillboard;
            else detailPrototype.renderMode = DetailRenderMode.Grass;

            return detailPrototype;
        }
    }
}
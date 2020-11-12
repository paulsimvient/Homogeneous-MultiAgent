using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace GISTech.GISTerrainLoader
{


    public class GISTerrainLoaderTreeGenerator
    {
        private static List<OSMNode> treeNodes;
        private static List<OSMWay> woodWays;
        private static List<OSMWay> treeRowWays;

        private static int totalTreeCount = 0;

        private static string m_currentWayID;

        private static HashSet<string> alreadyCreated;
        private static float TreeDistance;
        private static float BillBoardStartDistance;

        public  static  void GenerateTrees(TerrainContainerObject container, List<GameObject> m_treesPrefabs, float m_treeDensity, float TreeScaleFactor, float TreeRandomScaleFactor,float m_TreeDistance, float m_BillBoardStartDistance, Dictionary<string, OSMNode> nodes, Dictionary<string, OSMWay> ways, List<OSMMapMembers> relations)
        {
            TreeDistance = m_TreeDistance;
            BillBoardStartDistance = m_BillBoardStartDistance;

            AddTreesToTerrains(container, m_treesPrefabs);
 
            treeNodes = new List<OSMNode>();
            woodWays = new List<OSMWay>();
            treeRowWays = new List<OSMWay>();

            foreach (KeyValuePair<string, OSMNode> pair in nodes)
            {
                OSMNode n = pair.Value;
                if (n.HasTag("natural", "tree")) treeNodes.Add(n);
            }

            foreach (KeyValuePair<string, OSMWay> pair in ways)
            {
                OSMWay w = pair.Value;
                if (w.HasTag("natural", "wood") || w.HasTags("landuse", "forest", "park")) woodWays.Add(w);
                else if (w.HasTag("natural", "tree_row")) treeRowWays.Add(w);
            }


            totalTreeCount = treeNodes.Count + treeRowWays.Count + woodWays.Count;

            if (totalTreeCount == 0)
            {
                return;
            }

            alreadyCreated = new HashSet<string>();

            var treeDensity = m_treeDensity;
 
           var TLPMercator_X = container.TLPointMercator.x;
           var TLPMercator_Y = container.TLPointMercator.y;

           var DRPMercator_X = container.DRPointMercator.x;
           var DRPMercator_Y = container.DRPointMercator.y;


            for (int i = 0; i < treeNodes.Count; i++)
            {
                OSMNode node = treeNodes[i];

                if (alreadyCreated.Contains(node.id)) continue;

                alreadyCreated.Add(node.id);              

                var NodeP_Merc = GeoRefConversion.LatLongToMercat(node.Longitude, node.Latitude);

                double Offest_x = (NodeP_Merc.x - TLPMercator_X) / (DRPMercator_X - TLPMercator_X);

                double Offest_y = 1 - (NodeP_Merc.y - TLPMercator_Y) / (DRPMercator_Y - TLPMercator_Y);

                Vector3 WSPos = new Vector3((float)(container.transform.position.x + container.size.x * Offest_x), 0, (float)(container.size.z + container.size.z * Offest_y));

                SetTreeToTerrain(TreeScaleFactor, TreeRandomScaleFactor, container, WSPos);
            }

            for (int index = 0; index < treeRowWays.Count; index++)
            {
                OSMWay way = treeRowWays[index];
                if (alreadyCreated.Contains(way.id)) continue;
                alreadyCreated.Add(way.id);
                List<Vector3> points = OSMWay.GetGlobalPointsFromWay(way, nodes);

                for (int i = 0; i < points.Count; i++)
                {
                    Vector3 WSPos = points[i];

                    var WSPos_Merc = GeoRefConversion.LatLongToMercat(WSPos.x, WSPos.z);

                    double Offest_x = (WSPos_Merc.x - TLPMercator_X) / (DRPMercator_X - TLPMercator_X);
                    double Offest_y = 1 - (WSPos_Merc.y - TLPMercator_Y) / (DRPMercator_Y - TLPMercator_Y);

                    WSPos.x = (float)(container.transform.position.x + container.size.x * Offest_x);
                    WSPos.z = (float)(container.transform.position.z + container.size.z * Offest_y);

                    points[i] = WSPos;
                }

                for (int i = 0; i < points.Count - 1; i++)
                {
                    int len = Mathf.RoundToInt((points[i] - points[i + 1]).magnitude / m_treeDensity);
                    if (len > 0)
                    {
                        for (int j = 0; j <= len; j++) SetTreeToTerrain(TreeScaleFactor, TreeRandomScaleFactor, container, Vector3.Lerp(points[i], points[i + 1], j / (float)len));
                    }
                    else SetTreeToTerrain(TreeScaleFactor, TreeRandomScaleFactor, container, points[i]);
                }

            }

            for (int index = 0; index < woodWays.Count; index++)
            {
                OSMWay way = woodWays[index];
                if (alreadyCreated.Contains(way.id)) continue;
                alreadyCreated.Add(way.id);
                List<Vector3> points = OSMWay.GetGlobalPointsFromWay(way, nodes);

                for (int i = 0; i < points.Count; i++)
                {
                    Vector3 p = points[i];
 
                    var sp = GeoRefConversion.LatLongToMercat(p.x, p.z);

                    double rx = (sp.x - TLPMercator_X) / (DRPMercator_X - TLPMercator_X);
                    double ry = 1 - (sp.y - TLPMercator_Y) / (DRPMercator_Y - TLPMercator_Y);

                    p.x = (float)(container.transform.position.x + container.size.x * rx);
                    p.z = (float)(container.transform.position.z + container.size.z * ry);

                    points[i] = p;
                }

                Rect rect = Extensions.GetRectFromPoints(points);
                int lx = Mathf.RoundToInt(rect.width / m_treeDensity);
                int ly = Mathf.RoundToInt(rect.height / m_treeDensity);

                if (lx > 0 && ly > 0)
                {
                    m_currentWayID = way.id;

                    GenerateTerrainsTrees(TreeScaleFactor, TreeRandomScaleFactor,container, treeDensity, lx, ly, rect, points);

                }

            }
        }
        private static void SetTreeToTerrain(float TreeScaleFactor,float RandomScaleFactor,TerrainContainerObject container, Vector3 pos)
        {
            for (int x = 0; x < container.terrainCount.x; x++)
            {
                for (int y = 0; y < container.terrainCount.y; y++)
                {
                    TerrainObject item = container.terrains[x, y];
                    Terrain terrain = item.terrain;
                    terrain.treeBillboardDistance = BillBoardStartDistance;
                    terrain.treeDistance = TreeDistance;
                    TerrainData tData = terrain.terrainData;
                    Vector3 terPos = terrain.transform.position;
                    Vector3 localPos = pos - terPos;
                    float heightmapWidth = (tData.heightmapWidth - 1) * tData.heightmapScale.x;
                    float heightmapHeight = (tData.heightmapHeight - 1) * tData.heightmapScale.z;
                    if (localPos.x > 0 && localPos.z > 0 && localPos.x < heightmapWidth && localPos.z < heightmapHeight)
                    {
                        terrain.AddTreeInstance(new TreeInstance
                        {
                            color = Color.white,
                            heightScale = TreeScaleFactor + UnityEngine.Random.Range(-RandomScaleFactor, RandomScaleFactor),
                            lightmapColor = Color.white,
                            position = new Vector3(localPos.x / heightmapWidth, 0, localPos.z / heightmapHeight),
                            prototypeIndex = UnityEngine.Random.Range(0, tData.treePrototypes.Length),
                            widthScale = TreeScaleFactor + UnityEngine.Random.Range(-RandomScaleFactor, RandomScaleFactor)
                        });
                        break;
                    }
                }
            }
        }
        private static void GenerateTerrainsTrees(float TreeScaleFactor,float TreeRandomScaleFactor,TerrainContainerObject container,float treeDensity, int factorX, int factorY, Rect rect, List<Vector3> points)
        {
            Bounds bounds = container.GlobalTerrainBounds;
 
            Vector3 Bmin = bounds.min;
            Vector3 Bmax = bounds.max;

            float TreeValue = 400f / treeDensity;

            float rectx = (rect.xMax - rect.xMin) / factorX;
            float recty = (rect.yMax - rect.yMin) / factorY;

            int counter = 0;

            Vector3[] ps = points.ToArray();

            int Max_S_x = Mathf.Max(Mathf.FloorToInt((Bmin.x - rect.xMin) / rectx + 1), 0);
            int Min_E_x = Mathf.Min(Mathf.FloorToInt((Bmax.x - rect.xMin) / rectx), factorX);

            int Max_S_y = Mathf.Max(Mathf.FloorToInt((Bmin.z - rect.yMin) / recty + 1), 0);
            int Min_E_y = Mathf.Min(Mathf.FloorToInt((Bmax.z - rect.yMin) / recty), factorY);

            for (int x = Max_S_x; x < Min_E_x; x++)
            {

                float rx = x * rectx + rect.xMin;

                for (int y = Max_S_y; y < Min_E_y; y++)
                {
                    float ry = y * recty + rect.yMin;

                    float px = rx + UnityEngine.Random.Range(-TreeValue, TreeValue);
                    float pz = ry + UnityEngine.Random.Range(-TreeValue, TreeValue);

                    if (Extensions.IsPointInPolygon(ps, px, pz))
                    {
                        SetTreeToTerrain(TreeScaleFactor, TreeRandomScaleFactor, container, new Vector3(px, 0, pz));
                        counter++;
                    }
                }
            }

        }
        private static void AddTreesToTerrains(TerrainContainerObject container,List<GameObject> m_treesPrefabs)
        {
            TreePrototype[] prototypes = new TreePrototype[m_treesPrefabs.Count];

            for (int i = 0; i < prototypes.Length; i++)
            {
                if (m_treesPrefabs[i] != null)
                {
                    prototypes[i] = new TreePrototype
                    {
                        prefab = m_treesPrefabs[i]
                    };
                }

            }

            foreach (var item in container.terrains)
            {
                item.terrainData.treePrototypes = prototypes;
                item.terrainData.treeInstances = new TreeInstance[0];
            }
        }

    }
}
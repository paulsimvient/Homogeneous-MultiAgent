using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEngine;


namespace GISTech.GISTerrainLoader
{
    public class GISTerrainLoaderOSMFileLoader 
    {

        public static void LoadOSMFile(string OsmFilepath,out Dictionary<string, OSMNode> nodes,out Dictionary<string, OSMWay> ways,out List<OSMMapMembers> relations)
        {
            nodes = new Dictionary<string, OSMNode>();
            ways = new Dictionary<string, OSMWay>();
            relations = new List<OSMMapMembers>();
            if (File.Exists(OsmFilepath))
            {
                var xmlText = File.ReadAllText(OsmFilepath);

                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xmlText);
                var nodesDoc = doc.DocumentElement.ChildNodes;

                foreach (XmlNode node in nodesDoc)
                {
                    if (node.Name == "node")
                    {
                        OSMNode n = new OSMNode(node);
                        if (!nodes.ContainsKey(n.id)) nodes.Add(n.id, n);
                    }
                    else if (node.Name == "way")
                    {
                        OSMWay way = new OSMWay(node);
                        if (!ways.ContainsKey(way.id))
                            ways.Add(way.id, way);
                    }
                    else if (node.Name == "relation")
                        relations.Add(new OSMMapMembers(node));
                }

            }
           
        }


    }
}
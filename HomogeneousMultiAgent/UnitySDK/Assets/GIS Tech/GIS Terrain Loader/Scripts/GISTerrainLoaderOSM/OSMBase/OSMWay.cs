using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEngine;


namespace GISTech.GISTerrainLoader
{
    public class OSMWay : OSMDataElement

    {

        /// <summary>
        /// List of OSM Node ID.
        /// </summary>
        public List<string> nodeRefs;

        public List<OSMWay> holes;

        public OSMWay()
        {
        }

        public OSMWay(BinaryReader br)
        {
            id = br.ReadInt64().ToString();
            nodeRefs = new List<string>();
            tags = new List<OSMTag>();
            int refCount = br.ReadInt32();
            for (int i = 0; i < refCount; i++) nodeRefs.Add(br.ReadInt64().ToString());
            int tagCount = br.ReadInt32();
            for (int i = 0; i < tagCount; i++) tags.Add(new OSMTag(br));
        }

        public OSMWay(XmlNode node)
        {
            id = node.Attributes["id"].Value;
            nodeRefs = new List<string>();
            tags = new List<OSMTag>();

            foreach (XmlNode subNode in node.ChildNodes)
            {
                if (subNode.Name == "nd") nodeRefs.Add(subNode.Attributes["ref"].Value);
                else if (subNode.Name == "tag") tags.Add(new OSMTag(subNode));
            }
        }

        public static List<Vector3> GetGlobalPointsFromWay(OSMWay way, Dictionary<string, OSMNode> _nodes)
        {
            List<Vector3> points = new List<Vector3>();
            if (way.nodeRefs.Count == 0) return points;

            foreach (string nodeRef in way.nodeRefs)
            {
                OSMNode node;
                if (_nodes.TryGetValue(nodeRef, out node)) points.Add(new Vector3((float)node.Longitude, 0, (float)node.Latitude));
            }
            return points;
        }

    }
}
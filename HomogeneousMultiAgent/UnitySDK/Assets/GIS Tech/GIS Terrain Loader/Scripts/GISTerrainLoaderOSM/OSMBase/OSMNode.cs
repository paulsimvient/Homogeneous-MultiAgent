using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using UnityEngine;

namespace GISTech.GISTerrainLoader
{
    /// <summary>
    /// Class for OSM Node.
    /// </summary>
    public class OSMNode : OSMDataElement
    {
        /// <summary>
        /// Node ID
        /// </summary>
        public ulong ID { get; private set; }

        public readonly double Latitude;

        public readonly double Longitude;

        /// <summary>
        /// X coordinate in Unity space
        /// </summary>
        public float XCoord { get; private set; }
        /// <summary>
        /// Y coordinate in Unity space
        /// </summary>
        public float YCoord { get; private set; }

        public Vector3 WorldCoord { get; private set; }


        public OSMNode(BinaryReader br)
        {
            id = br.ReadInt64().ToString();
            Latitude = br.ReadSingle();
            Longitude = br.ReadSingle();

            tags = new List<OSMTag>();

            while(br.PeekChar()>-1)
            {
                try
                {
                    int tagCount = br.ReadInt32();
                    for (int i = 0; i < tagCount; i++) tags.Add(new OSMTag(br));
                }
                catch(Exception ex)
                {
                    Debug.LogError(ex);
                }
            }


        }

        public OSMNode(XmlNode node)
        {
            id = node.Attributes["id"].Value;

            Latitude = (float)Extensions.ConvertToDouble(node.Attributes["lat"].Value);
            Longitude = (float)Extensions.ConvertToDouble(node.Attributes["lon"].Value);

            tags = new List<OSMTag>();

            foreach (XmlNode subNode in node.ChildNodes) tags.Add(new OSMTag(subNode));
        }


    }
 
}
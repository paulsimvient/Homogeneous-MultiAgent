using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEngine;


namespace GISTech.GISTerrainLoader
{
    public class OSMMapNodeMembres
    {
        /// <summary>
        /// ID of OSM Way.
        /// </summary>
        public readonly string reference;

        /// <summary>
        /// Role of member.
        /// </summary>
        public readonly string role;

        /// <summary>
        /// Type of member.
        /// </summary>
        public readonly string type;

        public OSMMapNodeMembres(BinaryReader br)
        {
            type = br.ReadString();
            reference = br.ReadInt64().ToString();
            role = br.ReadString();
        }

        public OSMMapNodeMembres(XmlNode node)
        {
            type = node.Attributes["type"].Value;
            reference = node.Attributes["ref"].Value;
            role = node.Attributes["role"].Value;
        }
    }
}
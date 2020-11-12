using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEngine;
namespace GISTech.GISTerrainLoader
{
    public class OSMMapMembers : OSMDataElement
    {
        /// <summary>
        /// List of relation members.
        /// </summary>
        public readonly List<OSMMapNodeMembres> members;

        public OSMMapMembers(BinaryReader br)
        {
            id = br.ReadInt64().ToString();
            members = new List<OSMMapNodeMembres>();
            tags = new List<OSMTag>();

            int memberCount = br.ReadInt32();
            for (int i = 0; i < memberCount; i++) members.Add(new OSMMapNodeMembres(br));
            int tagCount = br.ReadInt32();
            for (int i = 0; i < tagCount; i++) tags.Add(new OSMTag(br));
        }

        public OSMMapMembers(XmlNode node)
        {
            id = node.Attributes["id"].Value;
            members = new List<OSMMapNodeMembres>();
            tags = new List<OSMTag>();

            foreach (XmlNode subNode in node.ChildNodes)
            {
                if (subNode.Name == "member") members.Add(new OSMMapNodeMembres(subNode));
                else if (subNode.Name == "tag") tags.Add(new OSMTag(subNode));
            }
        }

    }
}
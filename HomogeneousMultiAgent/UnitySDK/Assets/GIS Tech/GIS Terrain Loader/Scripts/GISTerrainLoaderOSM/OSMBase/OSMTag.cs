using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEngine;
namespace GISTech.GISTerrainLoader
{
    public class OSMTag 
    {
        /// <summary>
        /// Tag key.
        /// </summary>
        public readonly string key;

        /// <summary>
        /// Tag value.
        /// </summary>
        public readonly string value;

        public OSMTag(BinaryReader br)
        {
            key = br.ReadString();

            if (!string.IsNullOrEmpty(key))
            {

                var s = br.ReadString();
                if (!string.IsNullOrEmpty(s))
                {

                    value = br.ReadString();

                }
            }

        }

        public OSMTag(XmlNode node)
        {
            key = node.Attributes["k"].Value;
            value = node.Attributes["v"].Value;
        }

    }
}
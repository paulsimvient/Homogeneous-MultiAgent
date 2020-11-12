/*     Unity GIS Tech 2019-2020      */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GISTech.GISTerrainLoader
{
    public class TerrainContainerObject : MonoBehaviour
    {
        public DVector2 TLPointLatLong;
        public DVector2 DRPointLatLong;

        [HideInInspector]
        public DVector2 TLPointMercator;
        [HideInInspector]
        public DVector2 DRPointMercator;
        [HideInInspector]
        public Bounds GlobalTerrainBounds;

        [HideInInspector]
        public string GeneratedTerrainfolder;
        [HideInInspector]
        public Vector2Int terrainCount;

        private TerrainObject[,] _terrains;

        [HideInInspector]
        public Vector3 scale;
        [HideInInspector]
        public Vector3 size;
        public TerrainObject[,] terrains
        {
            get
            {
                if (_terrains == null)
                {
                    _terrains = new TerrainObject[terrainCount.x, terrainCount.y];
                    TerrainObject[] items = GetComponentsInChildren<TerrainObject>();
                    foreach (TerrainObject item in items) _terrains[item.Number.x, item.Number.y] = item;
                }
                return _terrains;
            }
            set
            {
                _terrains = value;
            }
        }
    }
}
/*     Unity GIS Tech 2019-2020      */
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GISTech.GISTerrainLoader
{
    public class GeoRef : MonoBehaviour
    {
        public Text LatLonText;
        public Text ElevationText;

        //Use Mask after adding Terrain to layers list 
        public LayerMask TerrainLayer;

        private DVector2 m_Origin = new DVector2(0, 0);

        private float MinElevation;
        private float MaxElevation;
        private float factor = 10f;
        private float Scale = 1;

        private Terrain m_terrain;

        public Terrain terrain
        {
            get { return m_terrain; }
            set
            {
                if (m_terrain != value)
                {
                    m_terrain = value;

                }
            }
        }


        void Start()
        {
            RuntimeTerrainGenerator.SendTerrainOrigine += UpdateOrigin;
 
        }
        
        /// <summary>
        /// Update terrain Origin for GeoRefrence 
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="minelevation"></param>
        /// <param name="maxelevation"></param>
        private void UpdateOrigin(DVector2 origin, float minelevation, float maxelevation)
        {
            m_Origin.x = origin.x;
            m_Origin.y = origin.y;

            GeoRefConversion.SetLocalOrigin(origin);

            MinElevation = minelevation;
            MaxElevation = maxelevation;

            Scale = 100 / TerrainPrefs.Get.terrainScale.y;

        }

        void Update()
        {
            RayCastMousePosition();
        }

        private RaycastHit hitInfo;
        private Ray ray;
        private void RayCastMousePosition()
        {
            hitInfo = new RaycastHit();

            if (Camera.main)
            {
                ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo, Mathf.Infinity, TerrainLayer))
                {
                    if (terrain == null)
                    {
                        terrain = hitInfo.collider.transform.gameObject.GetComponent<Terrain>();
                        ElevationText.text = GetHeight(terrain, hitInfo.point).ToString() + " m ";
                    }

                    if (!string.Equals(hitInfo.collider.transform.name, terrain.name))
                    {
                        terrain = hitInfo.collider.transform.gameObject.GetComponent<Terrain>();
                        ElevationText.text = GetHeight(terrain, hitInfo.point).ToString() + " m ";
                    }

                    var mousePos = new Vector3(hitInfo.point.x, hitInfo.point.y, hitInfo.point.z);

                    if (terrain != null)
                    {
                        ElevationText.text = (GetHeight(terrain, hitInfo.point) * factor + MinElevation ) + " m ";
                    }

                    LatLonText.text = GeoRefConversion.UWSToLatLog(mousePos, Scale).ToString();
              
                }
            }
 
        }
        public float GetHeight(Terrain terrain, Vector3 position)
        {
            TerrainData t = terrain.terrainData;
            float height = terrain.SampleHeight(position);
            return height;
        }
        void OnDisable()
        {
            RuntimeTerrainGenerator.SendTerrainOrigine -= UpdateOrigin;
        }


    }
}
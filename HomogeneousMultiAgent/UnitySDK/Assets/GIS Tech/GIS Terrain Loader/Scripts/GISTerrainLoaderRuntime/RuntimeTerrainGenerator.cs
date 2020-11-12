/*     Unity GIS Tech 2019-2020      */
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace GISTech.GISTerrainLoader
{
    public delegate void RuntimeTerrainGeneratorEvents();
    public delegate void RuntimeTerrainGeneratorProgression(string phasename, float value);
    public delegate void RuntimeTerrainGeneratorOrigine(DVector2 _origine, float minEle, float maxEle);
    public class RuntimeTerrainGenerator : MonoSingleton<RuntimeTerrainGenerator>
    {
        public static event RuntimeTerrainGeneratorProgression OnProgression;
        //public static event RuntimeTerrainGeneratorEvents OnGenerationFinished;
        //public static event RuntimeTerrainGeneratorEvents OnGenerationError;

        public static event RuntimeTerrainGeneratorOrigine SendTerrainOrigine;

        private FloatReader floatReader;
        private TerrainPrefs prefs;
        public TerrainObject[,] terrains;
        private List<TerrainObject> ListTerrainObjects;
        private TerrainContainerObject Terrainscontainer;
        [HideInInspector]
        public GeneratingTerrainPhase phase;
        public string FilePath;
        float ElevationScaleValue = 1112.0f;
        [HideInInspector]
        public bool RemovePrevTerrain;

        private int progress;

        void Start()
        {
            prefs = TerrainPrefs.Get;
            phase = GeneratingTerrainPhase.idle;
            FloatReader.OnReadError += OnError;
        }

        private void Update()
        {
            if (phase == GeneratingTerrainPhase.idle) return;
            if (phase == GeneratingTerrainPhase.CheckFile) CheckForFile();
            if (phase == GeneratingTerrainPhase.LoadElevation) LoadFloatFile(FilePath);
            else if (phase == GeneratingTerrainPhase.generateTerrains) GenerateTerrains();
            else if (phase == GeneratingTerrainPhase.generateHeightmaps) GenerateHeightmap();
            else if (phase == GeneratingTerrainPhase.RepareTerrains) RepareTerrains();
            else if (phase == GeneratingTerrainPhase.generateTextures) GenerateTextures();
            else if (phase == GeneratingTerrainPhase.finish) Finish();

        }
        private void CheckForFile()
        {
            if (File.Exists(FilePath))
            {
                phase = GeneratingTerrainPhase.LoadElevation;
            }
            else
                phase = GeneratingTerrainPhase.idle;
        }
        public void LoadFloatFile(string filepath)
        {
            try
            {
                if (File.Exists(filepath))
                {
                    floatReader = new FloatReader();

                    floatReader.LoadFloatGrid(filepath);

                    if (floatReader.LoadComplet)
                    {
                        phase = GeneratingTerrainPhase.generateTerrains;

                        floatReader.LoadComplet = false;

                        if (floatReader.Terrain_Dimension.x == 0 || floatReader.Terrain_Dimension.y == 0)
                        {
                            Debug.LogError("Can't detecte terrain dimension please try againe .");
                            return;
                        }
                        else
                            prefs.terrainDimensions = new Vector2((float)floatReader.Terrain_Dimension.x, (float)floatReader.Terrain_Dimension.y);

                        
                        if (floatReader.Tiles.x !=0 || floatReader.Tiles.y != 0)
                        {
                            prefs.terrainCount = new Vector2Int((int)floatReader.Tiles.x, (int)floatReader.Tiles.y);
                        }
                        else
                        {
                            Debug.LogError(floatReader.Tiles +"Terrain textures Tiles Count not set in Hdr file ... try againe");

                            phase = GeneratingTerrainPhase.idle;
                        }

                        if (OnProgression != null)
                        {
                            OnProgression("Loading Elevation File", 1);
                        }
                    }

                }

            }
            catch (Exception ex)
            {
                Debug.Log("Couldn't convert Terrain file:");
                Debug.Log(ex.Message + Environment.NewLine);
                phase = GeneratingTerrainPhase.idle;
            };
        }
        public void GenerateTerrains()
        {
            ListTerrainObjects = new List<TerrainObject>();

            const string containerName = "Terrains";
            string cName = containerName;
            //Destroy prv created terrain
            if (RemovePrevTerrain)
            {
                Destroy(GameObject.Find(cName));
            }
            else
            {
                int index = 1;
                while (GameObject.Find(cName) != null)
                {
                    cName = containerName + " " + index.ToString();
                    index++;
                }
            }


            var container = new GameObject(cName);
            container.transform.position = new Vector3(0, 0, 0);

            progress = 0;

            Vector2 TlimiteFrom = new Vector2(prefs.terrainDimensions.x, 0);
            Vector2 TlimiteTo = new Vector2(prefs.terrainDimensions.y, 0);

            Vector2Int tCount = new Vector2Int((int)prefs.terrainCount.x, (int)prefs.terrainCount.y);

            float maxElevation = floatReader.MaxElevation;
            float minElevation = floatReader.MinElevation;
            float ElevationRange = maxElevation - minElevation;

            var sizeX = Mathf.Floor(prefs.terrainDimensions.x * prefs.terrainScale.x) / prefs.terrainCount.x;
            var sizeZ = Mathf.Floor(prefs.terrainDimensions.y * prefs.terrainScale.z) / prefs.terrainCount.y;
            var sizeY = (ElevationRange) / ElevationScaleValue * prefs.TerrainExaggeration * 100 * prefs.terrainScale.y;

            Vector3 size;

            if (prefs.TerrainElevation == TerrainElevation.RealWorldElevation)
            {
                sizeY = ((ElevationRange)) * prefs.terrainScale.y/100;
                size = new Vector3(sizeX, sizeY, sizeZ);
            }
            else
            {
                size = new Vector3(sizeX, sizeY, sizeZ);

            }

            terrains = new TerrainObject[tCount.x, tCount.y];

            container.AddComponent<TerrainContainerObject>();

            var terrainContainer = container.GetComponent<TerrainContainerObject>();

            Terrainscontainer = terrainContainer;

            terrainContainer.terrainCount = prefs.terrainCount;

            terrainContainer.scale = prefs.terrainScale;

            terrainContainer.size = new Vector3(size.x * tCount.x, size.y, size.z * tCount.y);

            //Set Terrain Coordinates to the container TerrainContainer script (Lat/lon) + Mercator
            terrainContainer.TLPointLatLong = floatReader.TopLeftPoint;
            terrainContainer.DRPointLatLong = floatReader.DownRightPoint;

            terrainContainer.TLPointMercator = GeoRefConversion.LatLongToMercat(terrainContainer.TLPointLatLong.x, terrainContainer.TLPointLatLong.y);
            terrainContainer.DRPointMercator = GeoRefConversion.LatLongToMercat(terrainContainer.DRPointLatLong.x, terrainContainer.DRPointLatLong.y);




            progress = 0;

            for (int x = 0; x < tCount.x; x++)
            {
                for (int y = 0; y < tCount.y; y++)
                {
                    terrains[x, y] = CreateTerrain(container.transform, x, y, size, prefs.terrainScale);
                    terrains[x, y].container = terrainContainer;

                    ListTerrainObjects.Add(terrains[x, y]);
                }
            }

            terrainContainer.terrains = terrains;

            phase = GeneratingTerrainPhase.generateHeightmaps;

        }
        private TerrainObject CreateTerrain(Transform parent, int x, int y, Vector3 size, Vector3 scale)
        {
            TerrainData tdata = new TerrainData
            {
                baseMapResolution = 32,
                heightmapResolution = 32
            };

            tdata.SetDetailResolution(prefs.detailResolution, prefs.resolutionPerPatch);
            tdata.size = size;
            

            GameObject GO = Terrain.CreateTerrainGameObject(tdata);
            GO.gameObject.SetActive(true);
            GO.name = string.Format("{0}-{1}", x, y);
            GO.transform.parent = parent;
            GO.transform.position = new Vector3(size.x * x, 0, size.z * y);

            TerrainObject item = GO.AddComponent<TerrainObject>();
            item.Number = new Vector2Int(x, y);
            item.size = size;
            item.ElevationFilePath = FilePath;
            item.prefs = prefs;

            item.terrain.basemapDistance = prefs.BaseMapDistance;


            float prog = ((terrains.GetLength(0) * terrains.GetLength(1) * 100f) / (prefs.terrainCount.x * prefs.terrainCount.y)) / 100f;

            if (OnProgression != null)
            {
                OnProgression("Generating Terrains", prog);
            }


            return item;
        }
        private void GenerateHeightmap()
        {
            int index = 0;

            foreach(var Tile in ListTerrainObjects)
            {

                if (index >= terrains.Length-1)
                {
                    Debug.Log("GeneratingTerrainPhase.RepareTerrains : " + index);
                    phase = GeneratingTerrainPhase.RepareTerrains;
                }
                var terrainCount = new Vector2Int(prefs.terrainCount.x, prefs.terrainCount.y);

                int x = index % terrainCount.x;
                int y = index / terrainCount.x;

                float prog = ((index) * 100 / (terrains.GetLength(0) * terrains.GetLength(1))) / 100f;

                floatReader.GenerateHeightMap(Terrainscontainer, terrains[x, y]);
                index++;
                if (floatReader.generateComplete)
                {
                    progress = 0;
                    floatReader.generateComplete = false;
                }

                if (OnProgression != null)
                {
                    OnProgression("Generating Heightmap", (float)prog);
                }

            }


        }
        public void RepareTerrains()
        {
            GISTerrainLoaderBlendTerrainEdge.StitchTerrain(ListTerrainObjects, 50f, 20);

            GISTerrainLoaderBlendTerrainEdge.StitchTerrain(ListTerrainObjects, 20f, 20);

            phase = GeneratingTerrainPhase.generateTextures;

        }
        private void Finish()
        {
            if (OnProgression != null)
            {
                OnProgression("Finalization", progress);
            }

            foreach (TerrainObject item in terrains)
                item.terrain.Flush();
            phase = GeneratingTerrainPhase.idle;


            if (Camera.main.GetComponent<Camera3D>())
            {
                var cam = Camera.main.GetComponent<Camera3D>();
                cam.cameraTarget = new Vector3(75f * prefs.terrainScale.x, 30f * prefs.terrainScale.y, 75f * prefs.terrainScale.z);
                cam.enabled = true;

            }


            if (OnProgression != null)
            {
                SendTerrainOrigine(floatReader.Origine, floatReader.MinElevation, floatReader.MaxElevation);
            }
        }
        private void GenerateTextures()
        {
            if (prefs.textureMode == TextureMode.WithTexture)
            {
                int index = 0;

                foreach (var Tile in ListTerrainObjects)
                {
                    GISTerrainLoaderTextureGenerator.AddTextures(FilePath, Tile,new Vector2(prefs.textureWidth,prefs.textureHeight));

                    float prog = ((index) * 100 / (terrains.GetLength(0) * terrains.GetLength(1))) / 100f;

                    index++;

                    if (OnProgression != null)
                    {
                        OnProgression("Generate Textures", prog);
                    }
                }
                phase = GeneratingTerrainPhase.finish;

            }
            else
                phase = GeneratingTerrainPhase.finish;

        }


        void OnError()
        {
            phase = GeneratingTerrainPhase.idle;
        }
        void OnDisable()
        {
            if (Camera.main) Camera.main.GetComponent<Camera3D>().enabled = false;
        }
    }
}
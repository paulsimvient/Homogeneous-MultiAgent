/*     Unity GIS Tech 2019-2020      */
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using GISTech.GISTerrainLoader;

namespace GISTech.GISTerrainLoader
{
    public class EditorTerrainGenerator : EditorWindow
    {
        //Terrain Prefs //
        private bool ShowSetTerrainPref = true;
        private bool ShowTerrainPref = true;
        private bool ShowOSMVectorData = true;

        private UnityEngine.Object TerrainFile;
        public string TerrainFileName;
        public string TerrainFilePath;
        public float TerrainExaggeration;

        public Vector2Int terrainCount = Vector2Int.one;
        public Vector3 terrainScale = Vector3.one;
        private Vector2 m_terrainDimensions;

        public bool RemovePrvTerrain;

        public int heightmapResolution = 1025;
        public int detailResolution = 2048;
        public int resolutionPerPatch = 16;
        public int baseMapResolution = 513;


        public int heightmapResolution_index = 5;
        public int detailResolution_index = 5;
        public int resolutionPerPatch_index = 1;
        public int baseMapResolution_index = 5;

        private int[] heightmapResolutions = { 33, 65, 129, 257, 513, 1025, 2049 };
        public string[] heightmapResolutionsSrt = new string[] { "33", "65", "129", "257", "513", "1025", "2049" };


        private int[] availableHeights = { 32, 64, 129, 256, 512, 1024, 2048, 4096 };
        public string[] availableHeightSrt = new string[] { "32", "64", "128", "256", "512", "1024", "2048" };

        private int[] availableHeightsResolutionPrePec = { 4, 8, 16, 32 };
        public string[] availableHeightsResolutionPrePectSrt = new string[] { "4", "8", "16", "32"};


        const string heightmapTooltip = "Pixel resolution of the terrains heightmap.";
        private bool ShowTexturePref = true;

        public TerrainElevation terrainElevation = TerrainElevation.RealWorldElevation;
        public TextureMode textureMode = TextureMode.WithTexture;
        public int textureHeight = 1024;
        public int textureWidth = 1024;
        public Color textureEmptyColor = Color.white;

        private float tileWidth;
        private float tileLenght;


        private FloatReader floatReader;
        private Vector2 scrollPos = Vector2.zero;

        private static GeneratingTerrainPhase phase;
        private int CurrentTerrainIndex;
        private float ElevationScaleValue = 1112.0f;
        public TerrainObject[,] terrains;
        private TerrainContainerObject GeneratedContainer;

        public bool UseTerrainHeightSmoother;
        public bool UseTerrainSurfaceSmoother;

        [Range(0.2f, 0.3f)]
        private static float TerrainHeightSmoothFactor = 0.05f;
        [Range(0, 5)]
        private static int TerrainSurfaceSmoothFactor = 4;


        private string OSMFilePath;

        private bool EnableTreeGeneration;
        private float TreeDistance = 4000f;
        private float BillBoardStartDistance = 300;
        private float TreeDensity = 70f;
        private float TreeScaleFactor = 1.5f;
        private float TreeRandomScaleFactor = 0.5f;
        [SerializeField]
        List<GameObject> TreePrefabs = new List<GameObject>();


        private bool EnableGrassGeneration;
        private float DetailDistance = 300;
        private float GrassDensity = 10f;
        private float GrassScaleFactor = 1.5f;

        [SerializeField]
        List<GISTerrainLoaderSO_Grass> GrassPrefabs = new List<GISTerrainLoaderSO_Grass>();


        [MenuItem("Tools/GIS Terrain Loader/Terrain Loader", false, 2)]
        static void  Init()
        {
            EditorTerrainGenerator window = (EditorTerrainGenerator)EditorWindow.GetWindow(typeof(EditorTerrainGenerator), false, "GIS Terrain Loader (Editor)");
            window.Show();

           FloatReader.OnReadError += OnError;
        }
        void OnInspectorUpdate() { Repaint(); }
        void OnGUI()
        {
            GUI();
        }
        void GUI()
        {
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);


            ShowTerrainPref = EditorGUILayout.Foldout(ShowTerrainPref, " Terrain Prefs: ");

            if (ShowTerrainPref)
            {

                GUILayout.BeginHorizontal();
                GUILayout.Label("Heightmap Resolution     ", GUILayout.ExpandWidth(false));
                heightmapResolution_index = EditorGUILayout.Popup(heightmapResolution_index, heightmapResolutionsSrt);
                GUILayout.EndHorizontal();


                GUILayout.BeginHorizontal();
                GUILayout.Label("Detail Resolution            ", GUILayout.ExpandWidth(false));
                detailResolution_index = EditorGUILayout.Popup(detailResolution_index, availableHeightSrt);
                GUILayout.EndHorizontal();


                GUILayout.BeginHorizontal();
                GUILayout.Label("Resolution Per Patch       ", GUILayout.ExpandWidth(false));
                resolutionPerPatch_index = EditorGUILayout.Popup(resolutionPerPatch_index, availableHeightsResolutionPrePectSrt);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Base Map Resolution       ", GUILayout.ExpandWidth(false));
                baseMapResolution_index = EditorGUILayout.Popup(baseMapResolution_index, availableHeightSrt);
                GUILayout.EndHorizontal();
            }

            ShowTexturePref = EditorGUILayout.Foldout(ShowTexturePref, " Texture Prefs : ");

            if (ShowTexturePref)
            {
                textureMode = (TextureMode)EditorGUILayout.EnumPopup(" Texturing Mode : ", textureMode);


                if (textureMode == TextureMode.WithTexture)
                {
                    textureWidth = EditorGUILayout.IntField("Texture Width", textureWidth, GUILayout.ExpandWidth(false));
                    textureHeight = EditorGUILayout.IntField("Texture Height", textureHeight, GUILayout.ExpandWidth(false));
                    textureEmptyColor = EditorGUILayout.ColorField("Texture Empty Color", textureEmptyColor, GUILayout.ExpandWidth(false));
                }

            }

            ShowSetTerrainPref = EditorGUILayout.Foldout(ShowSetTerrainPref, " Terrain Heightmap / Parametres: ");

            if (ShowSetTerrainPref)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Set Terrain File (*.Flt) :", GUILayout.ExpandWidth(false));
                TerrainFile = EditorGUILayout.ObjectField(TerrainFile, typeof(UnityEngine.Object), GUILayout.ExpandWidth(true));
                GUILayout.EndHorizontal();

                terrainElevation = (TerrainElevation)EditorGUILayout.EnumPopup("Terrain Elevation Mode : ", terrainElevation);

                if (terrainElevation == TerrainElevation.RealWorldElevation)
                {

                }
                else
                if (terrainElevation == TerrainElevation.ExaggerationTerrain)
                {
                    TerrainExaggeration = EditorGUILayout.Slider(TerrainExaggeration, 0, 1);
                }

                GUILayout.BeginHorizontal();
                GUILayout.Label("Terrain Scale X: ", GUILayout.ExpandWidth(false));
                terrainScale.x = EditorGUILayout.FloatField(terrainScale.x, GUILayout.ExpandWidth(false));
                GUILayout.Label("Y: ", GUILayout.ExpandWidth(false));
                terrainScale.y = EditorGUILayout.FloatField(terrainScale.y, GUILayout.ExpandWidth(false));
                GUILayout.Label("Z: ", GUILayout.ExpandWidth(false));
                terrainScale.z = EditorGUILayout.FloatField(terrainScale.z, GUILayout.ExpandWidth(false));
                GUILayout.EndHorizontal();

                RemovePrvTerrain = EditorGUILayout.Toggle("Remove Previous Terrain :", RemovePrvTerrain);


                GUILayout.BeginHorizontal();
                GUILayout.Label("Enable Terrain Height Smoother  : ", GUILayout.ExpandWidth(false));
                UseTerrainHeightSmoother = EditorGUILayout.Toggle("", UseTerrainHeightSmoother);
                GUILayout.EndHorizontal();



                if (UseTerrainHeightSmoother)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Terrain Height Smooth Factor   :", GUILayout.ExpandWidth(false));
                    TerrainHeightSmoothFactor = EditorGUILayout.Slider(TerrainHeightSmoothFactor, 0.0f, 0.3f, GUILayout.ExpandWidth(true));
                    GUILayout.EndHorizontal();

                }

                GUILayout.BeginHorizontal();
                GUILayout.Label("Enable Terrain Surface Smoother : ", GUILayout.ExpandWidth(false));
                UseTerrainSurfaceSmoother = EditorGUILayout.Toggle("", UseTerrainSurfaceSmoother);
                GUILayout.EndHorizontal();


                if (UseTerrainSurfaceSmoother)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Terrain Surface Smooth Factor :", GUILayout.ExpandWidth(false));
                    TerrainSurfaceSmoothFactor = EditorGUILayout.IntSlider(TerrainSurfaceSmoothFactor, 1, 15, GUILayout.ExpandWidth(true));
                    GUILayout.EndHorizontal();

                }

            }



            ShowOSMVectorData = EditorGUILayout.Foldout(ShowOSMVectorData, " Generate OSM Vector Data : ");

            if (ShowOSMVectorData)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Enable Tree Generation :  ", GUILayout.ExpandWidth(false));
                EnableTreeGeneration = EditorGUILayout.Toggle("", EnableTreeGeneration);
                GUILayout.EndHorizontal();

                if (EnableTreeGeneration)
                {
                    //Tree Density
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Tree Density :", GUILayout.ExpandWidth(false));
                    TreeDensity = EditorGUILayout.Slider(TreeDensity, 1, 100, GUILayout.ExpandWidth(true));
                    GUILayout.EndHorizontal();

                    //Tree Scale Factor
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Tree Scale Factor :", GUILayout.ExpandWidth(false));
                    TreeScaleFactor = EditorGUILayout.Slider(TreeScaleFactor, 1, 10, GUILayout.ExpandWidth(true));
                    GUILayout.EndHorizontal();

                    //Tree Random Scale Factor
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Tree Random Scale Factor :", GUILayout.ExpandWidth(false));
                    TreeRandomScaleFactor = EditorGUILayout.Slider(TreeRandomScaleFactor, 0.1f, 1, GUILayout.ExpandWidth(true));
                    GUILayout.EndHorizontal();
                    //Tree Distance
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Tree Distance :", GUILayout.ExpandWidth(false));
                    TreeDistance = EditorGUILayout.Slider(TreeDistance, 1, 5000, GUILayout.ExpandWidth(true));
                    GUILayout.EndHorizontal();

                    //Tree BillBoard Start Distance
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Tree BillBoard Start Distance :", GUILayout.ExpandWidth(false));
                    BillBoardStartDistance = EditorGUILayout.Slider(BillBoardStartDistance, 1, 2000, GUILayout.ExpandWidth(true));
                    GUILayout.EndHorizontal();

                    //Tree Prefabs List
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Trees : ", GUILayout.ExpandWidth(false));

                    ScriptableObject target = this;
                    SerializedObject so = new SerializedObject(target);
                    SerializedProperty stringsProperty = so.FindProperty("TreePrefabs");

                    EditorGUILayout.PropertyField(stringsProperty, true);
                    so.ApplyModifiedProperties();

                    GUILayout.EndHorizontal();

                }

                GUILayout.BeginHorizontal();
                GUILayout.Label("Enable Grass Generation : ", GUILayout.ExpandWidth(false));
                EnableGrassGeneration = EditorGUILayout.Toggle("", EnableGrassGeneration);
                GUILayout.EndHorizontal();

                if (EnableGrassGeneration)
                {
                    //Grass Density
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Grass Density :", GUILayout.ExpandWidth(false));
                    GrassDensity = EditorGUILayout.Slider(GrassDensity, 1, 100, GUILayout.ExpandWidth(true));
                    GUILayout.EndHorizontal();

                    //Grass Scale Factor
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Grass Scale Factor :", GUILayout.ExpandWidth(false));
                    GrassScaleFactor = EditorGUILayout.Slider(GrassScaleFactor, 0.1f, 3, GUILayout.ExpandWidth(true));
                    GUILayout.EndHorizontal();

                    //Detail Distance
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Detail Distance :", GUILayout.ExpandWidth(false));
                    DetailDistance = EditorGUILayout.Slider(DetailDistance, 10f, 400, GUILayout.ExpandWidth(true));
                    GUILayout.EndHorizontal();

                    //Tree Prefabs List
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Grass : ", GUILayout.ExpandWidth(false));

                    ScriptableObject target = this;
                    SerializedObject so = new SerializedObject(target);
                    SerializedProperty stringsProperty = so.FindProperty("GrassPrefabs");

                    EditorGUILayout.PropertyField(stringsProperty, true);
                    so.ApplyModifiedProperties();

                    GUILayout.EndHorizontal();




                }


            }



            if (GUILayout.Button("Generate Terrain"))
            {
                heightmapResolution = heightmapResolutions[heightmapResolution_index];
                detailResolution = availableHeights[detailResolution_index];
                resolutionPerPatch = availableHeightsResolutionPrePec[resolutionPerPatch_index];
                baseMapResolution = availableHeights[baseMapResolution_index];


                TerrainFilePath = Path.Combine(Path.GetDirectoryName(Application.dataPath), AssetDatabase.GetAssetPath(TerrainFile));
                Initialization();
            }

            EditorGUILayout.EndScrollView();
        }
        private void Initialization()
        {
            if (!string.IsNullOrEmpty(TerrainFilePath) && File.Exists(TerrainFilePath))
            {
                phase = GeneratingTerrainPhase.CheckFile;
            }
            else
                Debug.LogError("File Not Exist : " + TerrainFilePath + " Please set (*.flt) File.. Try againe");
        }
        private void Update()
        {
            if (phase == GeneratingTerrainPhase.idle) return;
            if (phase == GeneratingTerrainPhase.CheckFile)                 CheckForFile();
            if (phase == GeneratingTerrainPhase.LoadElevation)             LoadFloatFile(TerrainFilePath);
            else if (phase == GeneratingTerrainPhase.generateTerrains)     GenerateTerrains();
            else if (phase == GeneratingTerrainPhase.generateHeightmaps)   GenerateHeightmap(CurrentTerrainIndex);
            else if (phase == GeneratingTerrainPhase.RepareTerrains)       RepareTerrains();
            else if (phase == GeneratingTerrainPhase.generateTextures)     GenerateTextures(CurrentTerrainIndex);
            else if (phase == GeneratingTerrainPhase.generateEnvironement) GenerateEnvironment();
            else if (phase == GeneratingTerrainPhase.finish)               Finish();

        }
        private void CheckForFile()
        {


            if (File.Exists(TerrainFilePath))
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

                    if (floatReader.Terrain_Dimension.x ==0 || floatReader.Terrain_Dimension.y == 0)
                    {
                        Debug.LogError("Can't detecte terrain dimension please try againe .");
                        return;
                    }else
                    if (floatReader.Terrain_Dimension != new DVector2(0, 0))
                    {
                        m_terrainDimensions = new Vector2((float)floatReader.Terrain_Dimension.x, (float)floatReader.Terrain_Dimension.y) * 10;
                    }

                    if (floatReader.Tiles != Vector2.zero)
                    {
                        terrainCount = new Vector2Int((int)floatReader.Tiles.x, (int)floatReader.Tiles.y);
                    }
                    else
                    {
                        Debug.LogError("Terrain textures Tiles Count not set in Hdr file ... try again");

                        phase = GeneratingTerrainPhase.idle;
                    }

                    if (floatReader.LoadComplet)
                    {
                        phase = GeneratingTerrainPhase.generateTerrains;
                        floatReader.LoadComplet = false;

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
            const string containerName = "Terrains";
            string cName = containerName;
            //Destroy prv created terrain
            if (RemovePrvTerrain)
            {
                DestroyImmediate(GameObject.Find(cName));
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
            CurrentTerrainIndex = 0;

            Vector2Int tCount = new Vector2Int(terrainCount.x, terrainCount.y);

            float maxElevation = floatReader.MaxElevation;
            float minElevation = floatReader.MinElevation;
            float ElevationRange = maxElevation - minElevation;

            var sizeX = Mathf.Floor(m_terrainDimensions.x * terrainScale.x*10) / terrainCount.x;
            var sizeZ = Mathf.Floor(m_terrainDimensions.y * terrainScale.z*10) / terrainCount.y;
            var sizeY = (ElevationRange) / ElevationScaleValue * TerrainExaggeration * 100 * terrainScale.y*10;

            Vector3 size;

            if (terrainElevation == TerrainElevation.RealWorldElevation)
            {
                sizeY = ((ElevationRange)) * terrainScale.y ;
                size = new Vector3(sizeX, sizeY, sizeZ);
            }
            else
            {
                sizeY = sizeY * 10;
                size = new Vector3(sizeX, sizeY, sizeZ);
            }

            string resultFolder = "Assets/Generated GIS Terrains";
            string resultFullPath = Path.Combine(Application.dataPath, "Generated GIS Terrains");
            if (!Directory.Exists(resultFullPath)) Directory.CreateDirectory(resultFullPath);
            string dateStr = DateTime.Now.ToString("yyyy-MM-dd HH-mm-") + DateTime.Now.Second.ToString();
                resultFolder += "/" + dateStr;
                resultFullPath = Path.Combine(resultFullPath, dateStr);
            if (!Directory.Exists(resultFullPath)) Directory.CreateDirectory(resultFullPath);




            terrains = new TerrainObject[tCount.x, tCount.y];

            container.AddComponent<TerrainContainerObject>();

            var terrainContainer = container.GetComponent<TerrainContainerObject>();

            terrainContainer.terrainCount = new Vector2Int(terrainCount.x, terrainCount.y);

            terrainContainer.GeneratedTerrainfolder = resultFolder;

            terrainContainer.scale = terrainScale;

            terrainContainer.size = new Vector3(size.x * tCount.x, size.y, size.z * tCount.y);

            //Set Terrain Coordinates to the container TerrainContainer script (Lat/lon) + Mercator
            terrainContainer.TLPointLatLong = floatReader.TopLeftPoint;
            terrainContainer.DRPointLatLong = floatReader.DownRightPoint;

            terrainContainer.TLPointMercator = GeoRefConversion.LatLongToMercat(terrainContainer.TLPointLatLong.x, terrainContainer.TLPointLatLong.y);
            terrainContainer.DRPointMercator = GeoRefConversion.LatLongToMercat(terrainContainer.DRPointLatLong.x, terrainContainer.DRPointLatLong.y);
 
            //Terrain Size Bounds 
            var centre = new Vector3(terrainContainer.size.x / 2, 0, terrainContainer.size.z / 2);
            terrainContainer.GlobalTerrainBounds = new Bounds(centre, new Vector3(centre.x + terrainContainer.size.x / 2, 0, centre.z + terrainContainer.size.z / 2));



            for (int x = 0; x < tCount.x; x++)
            {
                for (int y = 0; y < tCount.y; y++)
                {
                    terrains[x, y] = CreateTerrain(terrainContainer, x, y, size, terrainScale);
                    terrains[x, y].container = terrainContainer;
                }
            }

            terrainContainer.terrains = terrains;

            GeneratedContainer = terrainContainer;

            phase = GeneratingTerrainPhase.generateHeightmaps;



        }
        private TerrainObject CreateTerrain(TerrainContainerObject parent, int x, int y, Vector3 size, Vector3 scale)
        {

            TerrainData tdata = new TerrainData
            {
                baseMapResolution = 32,
                heightmapResolution = 32
            };

            tdata.heightmapResolution = heightmapResolution;
            tdata.baseMapResolution = baseMapResolution;
            tdata.SetDetailResolution(detailResolution, resolutionPerPatch);
            tdata.size = size;

            GameObject GO = Terrain.CreateTerrainGameObject(tdata);
            GO.gameObject.SetActive(true);
            GO.name = string.Format("{0}-{1}", x, y);
            GO.transform.parent = parent.gameObject.transform;
            GO.transform.position = new Vector3(size.x * x, 0, size.z * y);
            GO.isStatic = false;
            TerrainObject item = GO.AddComponent<TerrainObject>();
            item.Number = new Vector2Int(x, y);
            item.size = size;
            item.ElevationFilePath = TerrainFilePath;

            string filename = Path.Combine(parent.GeneratedTerrainfolder, GO.name) + ".asset";

            AssetDatabase.CreateAsset(tdata, filename);

            AssetDatabase.SaveAssets();

            return item;
        }
        private void GenerateHeightmap(int index)
        {
            if (index >= terrains.Length)
            {
                phase = GeneratingTerrainPhase.RepareTerrains;
                return;
            }

            int x = index % terrainCount.x;
            int y = index / terrainCount.x;

            Prefs prefs = new Prefs(detailResolution, resolutionPerPatch, baseMapResolution, heightmapResolution, terrainCount);

            floatReader.GenerateHeightMap(prefs, terrains[x, y]);

            if (floatReader.generateComplete)
            {
                CurrentTerrainIndex++;

                floatReader.generateComplete = false;
            }
        }
        public void RepareTerrains()
        {
            List<TerrainObject> List_terrainsObj = new List<TerrainObject>();

            foreach (var item in terrains)
            {
                if (item != null)
                {
                    List_terrainsObj.Add(item);
                }
 
            }

            if (UseTerrainHeightSmoother)
                GISTerrainLoaderTerrainSmoother.SmoothTerrainHeights(List_terrainsObj,1- TerrainHeightSmoothFactor);

            if (UseTerrainSurfaceSmoother)
                GISTerrainLoaderTerrainSmoother.SmoothTerrainSurface(List_terrainsObj, TerrainSurfaceSmoothFactor);

            GISTerrainLoaderBlendTerrainEdge.StitchTerrain(List_terrainsObj,50f,20);

            GISTerrainLoaderBlendTerrainEdge.StitchTerrain(List_terrainsObj, 20f, 20);

            CurrentTerrainIndex = 0;

            phase = GeneratingTerrainPhase.generateTextures;

        }
        private void GenerateTextures(int index)
        {
            if (index >= terrains.Length || textureMode == TextureMode.WithoutTexture)
            {
                phase = GeneratingTerrainPhase.generateEnvironement;
                return;

            }

            int x = index % terrainCount.x;
            int y = index / terrainCount.x;


            DirectoryInfo di = new DirectoryInfo(TerrainFilePath);

            var TextureFolderPath = TerrainFile.name + "_Textures";

            for (int i = 0; i<=5;i++)
            {
                di = di.Parent;
                TextureFolderPath = di.Name + "/" +TextureFolderPath;
                if (di.Name == "GIS Terrains") break;

                if(i==5)
                {
                    Debug.LogError("Texture folder not found! : Please put your terrain in GIS Terrain Loader/Recources/GIS Terrains/");
                }

            }

            AddTextureToTerrain(TextureFolderPath, terrains[x, y]);

            CurrentTerrainIndex++;
        }
        public void AddTextureToTerrain(string terrainPath, TerrainObject terrainItem)
        {
            var terrain = terrainItem.terrain;

            bool texExist;

            var texPath = Extensions.CheckForTexture(terrainPath, terrainItem, out texExist);

            if (texExist)
            {

#if UNITY_2018_3 || UNITY_2019
                string path = Path.Combine(terrainItem.container.GeneratedTerrainfolder, terrainItem.name + ".terrainlayer");
                TerrainLayer terrainLayer = new TerrainLayer();
                AssetDatabase.CreateAsset(terrainLayer, path);

                TerrainData terrainData = terrainItem.terrainData;
                TerrainLayer[] terrainLayers = terrainData.terrainLayers;
                for (int i = 0; i < terrainLayers.Length; i++)
                {
                    if ((UnityEngine.Object)terrainLayers[i] == (UnityEngine.Object)terrainLayer)
                    {
                        return;
                    }
                }
                int num = terrainLayers.Length;
                TerrainLayer[] array = new TerrainLayer[num + 1];
                Array.Copy(terrainLayers, 0, array, 0, num);
                array[num] = terrainLayer;
                terrainData.terrainLayers = array;

                terrainLayer.diffuseTexture = (Texture2D)Resources.Load(texPath);// tex;
                terrainLayer.tileSize = new Vector2(terrainItem.size.x, terrainItem.size.z);
                terrainLayer.tileOffset = Vector2.zero;
                terrainItem.terrainData.terrainLayers = array;
#else

            SplatPrototype sp = new SplatPrototype
            {
                texture = (Texture2D)Resources.Load(texPath),
                tileSize = new Vector2(terrainItem.size.x, terrainItem.size.z),
                tileOffset = Vector2.zero
            };
            terrain.terrainData.splatPrototypes = new[] { sp };

#endif
            }
            else
            {
                Debug.Log("Texture not found : " + texPath);
            }
        }
        private void GenerateEnvironment()
        {
            var nodes = new Dictionary<string, OSMNode>();
            var ways = new Dictionary<string, OSMWay>();
            var relations = new List<OSMMapMembers>();

            //Load OSM File
            if (EnableTreeGeneration || EnableGrassGeneration)
            {
                bool OSMExist;

                var OSMFilePath = Extensions.CheckForOSMFile(TerrainFilePath, TerrainFile.name, out OSMExist);

                if (OSMExist)
                {
                    GISTerrainLoaderOSMFileLoader.LoadOSMFile(OSMFilePath, out nodes, out ways, out relations);
                }
                else
                {
                    Debug.LogError("OSM File Not Found : please set your osm file into 'GIS Terrains'\'TerrainName'\'TerrainName_VectorData'");
                    phase = GeneratingTerrainPhase.finish;
                }

            }
            //Generate Trees
            if (EnableTreeGeneration)
            {
                if (TreePrefabs.Count > 0)
                {
                    var m_TreeDensity = 100 - TreeDensity;
                    GISTerrainLoaderTreeGenerator.GenerateTrees(GeneratedContainer, TreePrefabs, m_TreeDensity, TreeScaleFactor, TreeRandomScaleFactor, TreeDistance, BillBoardStartDistance, nodes, ways, relations);
                }
                else
                    Debug.LogError("Error : Tree Prefabs List is empty ");


            }

            //Generate Grass
            if (EnableGrassGeneration)
            {
                if (GrassPrefabs.Count > 0)
                {
                    var m_GrassDensity = 100 - GrassDensity;

                    GISTerrainLoaderGrassGenerator.GenerateGrass(GeneratedContainer, GrassPrefabs, GrassDensity, GrassScaleFactor, DetailDistance, nodes, ways, relations);
                }
                else
                    Debug.LogError("Error : Grass Prefabs List is empty ");

            }

            phase = GeneratingTerrainPhase.finish;
        }
        private void Finish()
        {
            foreach (TerrainObject item in terrains)
                item.terrain.Flush();
            phase = GeneratingTerrainPhase.idle;

        }
        static void OnError()
        {
            phase = GeneratingTerrainPhase.idle;
        }
        void OnDisable()
        {
            FloatReader.OnReadError -= OnError;
        }
    }
}
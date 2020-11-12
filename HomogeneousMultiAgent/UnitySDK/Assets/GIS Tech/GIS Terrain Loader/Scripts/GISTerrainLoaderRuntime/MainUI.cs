/*     Unity GIS Tech 2019-2020      */

using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using SimpleFileBrowser;

namespace GISTech.GISTerrainLoader
{
    public class MainUI : MonoBehaviour
    {
        public delegate void OnSuccess(string path);
        public delegate void OnCancel();
        [Header("Loading Terrain Mode : Single File / Infinite Tiles")]
        public ReadingTerrainMode readingTerrainMode;

        public Button LoadTerrain;

        public FileBrowser fileBrowserDiag;


        public Text TerrainPathText;

        public Scrollbar Terrain_Exaggeration;

        public Text Terrain_Exaggeration_value;

        public Dropdown ElevationMode;

        public Dropdown HeightMapResolution;

        public Toggle ClearLastTerrain;

        public Button GenerateTerrainBtn;

        private RuntimeTerrainGenerator runTimeTerrainGenerator;

        public const string version = "1.1";

        private TerrainPrefs terrainPrefs;

        private Camera3D camera3d;

        public Scrollbar GenerationProgress;

        public Text Phasename;

        public Text progressValue;
 
        void Start()
        {
            RuntimeTerrainGenerator.OnProgression += OnGeneratingTerrainProg;

            FileBrowser.SetFilters(true, new FileBrowser.Filter("Terrain File", ".flt"));
            FileBrowser.SetDefaultFilter(".flt");
            FileBrowser.SetExcludedExtensions(".lnk", ".tmp", ".zip", ".rar", ".exe");
            FileBrowser.AddQuickLink("Data Path", Application.dataPath, null);

            camera3d = Camera.main.GetComponent<Camera3D>();

            camera3d.enabled = false;

            LoadTerrain.onClick.AddListener(OnLoadBtnClicked);

            GenerateTerrainBtn.onClick.AddListener(OnGenerateTerrainbtnClicked);

            terrainPrefs = TerrainPrefs.Get;

            runTimeTerrainGenerator = RuntimeTerrainGenerator.Get;

            ElevationMode.onValueChanged.AddListener(OnElevationModeChanged);

            Terrain_Exaggeration.onValueChanged.AddListener(OnTerrainExaggerationChanged);

        }
        private void OnGenerateTerrainbtnClicked()
        {
            // ----- Case Of loading Single file  ---------//
            if (readingTerrainMode == ReadingTerrainMode.SingleTerrainFile)
            {
                runTimeTerrainGenerator.enabled = true;

                var TerrainPath = TerrainPathText.text;

                if (!string.IsNullOrEmpty(TerrainPath) && System.IO.File.Exists(TerrainPath))
                {
                    var elevationmode = ElevationMode.value;

                    int heightmapRes = int.Parse(HeightMapResolution.captionText.text.ToString());

                    float terrainexaggeration = Terrain_Exaggeration.value;

                    terrainPrefs.TerrainElevation = (TerrainElevation)elevationmode;

                    terrainPrefs.TerrainExaggeration = terrainexaggeration;

                    terrainPrefs.heightmapResolution = heightmapRes;

                    runTimeTerrainGenerator.RemovePrevTerrain = true;

                    runTimeTerrainGenerator.FilePath = TerrainPath;

                    runTimeTerrainGenerator.phase = GeneratingTerrainPhase.CheckFile;
                }
                else
                    Debug.LogError("Please set (*.flt) File.. Try againe");
            }

        }
        private void OnTerrainExaggerationChanged(float value)
        {
            Terrain_Exaggeration_value.text = value.ToString();
        }
        private void OnElevationModeChanged(int value)
        {

            switch (value)
            {
                case (int)TerrainElevation.RealWorldElevation:
                    Terrain_Exaggeration.transform.parent.gameObject.SetActive(false);

                    break;
                case (int)TerrainElevation.ExaggerationTerrain:
                    Terrain_Exaggeration.transform.parent.gameObject.SetActive(true);
                    break;
            }

        }
        private void OnLoadBtnClicked()
        {
            StartCoroutine(ShowLoadDialogCoroutine());
        }
        IEnumerator ShowLoadDialogCoroutine()
        {
            yield return FileBrowser.WaitForLoadDialog(false, null, "Load Terrain File", "Load");
            TerrainPathText.text = FileBrowser.Result;
        }
        private void OnGeneratingTerrainProg(string phase, float progress)
        {
            if (!phase.Equals("Finalization"))
            {
                GenerationProgress.transform.parent.gameObject.SetActive(true);

                Phasename.text = phase.ToString();

                GenerationProgress.value = progress;

                progressValue.text = (progress * 100).ToString() + "%";
            }
            else
            {
                camera3d.enabled = true;
                GenerationProgress.transform.parent.gameObject.SetActive(false);
            }
        }
        private void OnInfiniteStartNavigation()
        {

        }
    }
}
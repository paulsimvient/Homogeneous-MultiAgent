/*     Unity GIS Tech 2019-2020      */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEngine;

namespace GISTech.GISTerrainLoader
{
    public delegate void FloatReaderEvents();
    public class FloatReader
    {
        public static event FloatReaderEvents OnReadError;

        public float MaxElevation = -5000;
        public float MinElevation = 5000;

        public int mapSize_row_y;
        public int mapSize_col_x;

        public float[,] _floatheightData;
        public float[,] tdataHeightmap;

        public Vector2 Tiles = new Vector2(0, 0);

        public DVector2 Origine = new DVector2(0, 0);
        public DVector2 TopRightPoint = new DVector2(0, 0);

        public DVector2 TopLeftPoint = new DVector2(0, 0);
        public DVector2 DownRightPoint = new DVector2(0, 0);

        public DVector2 dim = new DVector2(0, 0);

        public DVector2 Terrain_Dimension = new DVector2(0, 0);


        private TerrainData tdata;

        private int lastX;

        public bool LoadComplet;
        public bool generateComplete;


        /// <summary>
        /// Load Elevation from .*flt file
        /// </summary>
        /// <param name="prefs"></param>
        /// <param name="item"></param>
        public void LoadFloatGrid(string filepath)
        {
            var hdrpath = Path.ChangeExtension(filepath, ".hdr");
            if (!File.Exists(filepath))
            {
                Debug.LogError("Please select a FloatGrid (.flt) file.");
                return;
            }

            if (File.Exists(hdrpath))
            {
                StreamReader hdrReader = new StreamReader(hdrpath);
                string hdrTemp = null;
                hdrTemp = hdrReader.ReadLine();
                while (hdrTemp != null)
                {
                    int spaceStart = hdrTemp.IndexOf(" ");
                    int spaceEnd = hdrTemp.LastIndexOf(" ");

                    hdrTemp = hdrTemp.Remove(spaceStart, spaceEnd - spaceStart);

                    string[] lineTemp = hdrTemp.Split(" "[0]);

                    switch (lineTemp[0])
                    {
                        case "nrows":
                            mapSize_row_y = Int32.Parse(lineTemp[1]);
                            break;
                        case "ncols":
                            mapSize_col_x = Int32.Parse(lineTemp[1]);
                            break;
                        case "xllcorner":
                            Origine.x = Extensions.ConvertToDouble(lineTemp[1]);
                            break;
                        case "yllcorner":
                            Origine.y = Extensions.ConvertToDouble(lineTemp[1]);
                            break;
                        case "xdim":
                            dim.x = Extensions.ConvertToDouble(lineTemp[1]);
                            break;
                        case "ydim":
                            dim.y = Extensions.ConvertToDouble(lineTemp[1]);
                            break;
                    }
                    hdrTemp = hdrReader.ReadLine();
                }
 
                TopRightPoint.x = Origine.x + (dim.x * mapSize_col_x);
                TopRightPoint.y = Origine.y + (dim.y * mapSize_row_y);

                var p1 = new DVector2(TopRightPoint.x, Origine.y);
                var p2 = new DVector2(Origine.x, TopRightPoint.y);

                TopLeftPoint = new DVector2(Origine.x, TopRightPoint.y);
                DownRightPoint = new DVector2(TopRightPoint.x, Origine.y);


                Terrain_Dimension.x = GeoRefConversion.Getdistance(Origine.y, Origine.x, p1.y, p1.x) * 10;
                Terrain_Dimension.y = GeoRefConversion.Getdistance(Origine.y, Origine.x, p2.y, p2.x) * 10;


                //DownRightPoint = new DVector2(, Origine.y);
                //var pos = GeoRefConversion.UWSToLatLog(mousePos, ).ToString();


                Debug.Log("Terrain Dimensions : " + Terrain_Dimension.x / 10 + " X " + Terrain_Dimension.y / 10 + " Km ");

                GetTilesNumberInTextureFolder(filepath);

            }
            else
            {
                Debug.LogError("The header (HDR) file is missing.");

                if (OnReadError != null)
                {
                    OnReadError();
                }
                return;
            }
            if (File.Exists(filepath))
            {
                var bytes = File.ReadAllBytes(filepath);
                _floatheightData = new float[mapSize_col_x, mapSize_row_y];

                for (int i = 0; i < mapSize_row_y; i++)
                {
                    for (int j = 0; j < mapSize_col_x; j++)
                    {
                        var el = BitConverter.ToSingle(bytes, i * mapSize_col_x * 4 + j * 4);

                        _floatheightData[j, mapSize_row_y - i - 1] = el;
                        if (el > -9900)
                        {

                            if (el < MinElevation)
                            {

                                MinElevation = el;
                            }
                            if (el > MaxElevation)
                            {
                                MaxElevation = el;
                            }
                        }


                    }
                }

                LoadComplet = true;
            }
            else
            {
                Debug.Log("File not found!");
                return;
            }

        }

        /// <summary>
        /// Generate HeightMaps by spliting Single terrain elevation file to tiles
        /// </summary>
        /// <param name="prefs"></param>
        /// <param name="item"></param>
        public void GenerateHeightMap(TerrainContainerObject container, TerrainObject item)
        {
            tdata = item.terrain.terrainData;
            if (tdataHeightmap == null)
                tdataHeightmap = new float[tdata.heightmapHeight, tdata.heightmapWidth];

            //if (tdata == null)
            //{

            //    tdata.baseMapResolution = prefs.baseMapResolution;
            //    tdata.SetDetailResolution(prefs.detailResolution, prefs.resolutionPerPatch);
            //    tdata.heightmapResolution = prefs.heightmapResolution;
            //    tdata.size = item.size;

            //    if (tdataHeightmap == null)
            //        tdataHeightmap = new float[tdata.heightmapHeight, tdata.heightmapWidth];
            //}

            float elevationRange = MaxElevation - MinElevation;

            long startTime = DateTime.Now.Ticks;

            float thx = tdata.heightmapWidth - 1;
            float thy = tdata.heightmapHeight - 1;


            var y_Terrain_Col_num = (mapSize_row_y / container.terrainCount.x);
            var x_Terrain_row_num = (mapSize_col_x / container.terrainCount.y);

            // heightmap rotation
            int tw = tdata.heightmapWidth;
            int th = tdata.heightmapHeight;
            for (int x = lastX; x < tw; x++)
            {
                for (int y = 0; y < th; y++)
                {

                    var x_from = item.Number.x * x_Terrain_row_num;
                    var x_To = item.Number.x * x_Terrain_row_num + x_Terrain_row_num - 1;

                    var y_from = item.Number.y * y_Terrain_Col_num;
                    var y_To = item.Number.y * y_Terrain_Col_num + y_Terrain_Col_num - 1;

                    float px = Mathf.Lerp(x_from, x_To, x / thx);
                    float py = Mathf.Lerp(y_from, y_To, y / thy);

                    var el = _floatheightData[(int)((px)), (int)(py)];

                    tdataHeightmap[y, x] = (el - MinElevation) / elevationRange;

                }
                lastX = x;
                //progress = hx / (float)tdata.heightmapWidth;
                if (new TimeSpan(DateTime.Now.Ticks - startTime).TotalSeconds > 1) return;
            }

            lastX = 0;
            tdata.SetHeights(0, 0, tdataHeightmap);

            tdata = null;
            generateComplete = true;

        }
        /// <summary>
        /// Generate HeightMaps by spliting Single terrain file to tiles
        /// </summary>
        /// <param name="prefs"></param>
        /// <param name="item"></param>
        public void GenerateHeightMap(Prefs prefs, TerrainObject item)
        {
            tdata = item.terrain.terrainData;

            if (tdataHeightmap == null)
                tdataHeightmap = new float[tdata.heightmapHeight, tdata.heightmapWidth];
            //
            if (tdata == null)
            {
                tdata = item.terrain.terrainData;
                tdata.baseMapResolution = prefs.baseMapResolution;
                tdata.SetDetailResolution(prefs.detailResolution, prefs.resolutionPerPatch);
                tdata.size = item.size;

                if (tdataHeightmap == null)
                    tdataHeightmap = new float[tdata.heightmapHeight, tdata.heightmapWidth];
            }

            float elevationRange = MaxElevation - MinElevation;

            long startTime = DateTime.Now.Ticks;

            float thx = tdata.heightmapWidth - 1;
            float thy = tdata.heightmapHeight - 1;


            var y_Terrain_Col_num = (mapSize_row_y / prefs.terrainCount.x);
            var x_Terrain_row_num = (mapSize_col_x / prefs.terrainCount.y);

            // heightmap rotation
            int tw = tdata.heightmapWidth;
            int th = tdata.heightmapHeight;
            for (int x = lastX; x < tw; x++)
            {
                for (int y = 0; y < th; y++)
                {

                    var x_from = item.Number.x * x_Terrain_row_num;
                    var x_To = item.Number.x * x_Terrain_row_num + x_Terrain_row_num - 1;

                    var y_from = item.Number.y * y_Terrain_Col_num;
                    var y_To = item.Number.y * y_Terrain_Col_num + y_Terrain_Col_num - 1;

                    float px = Mathf.Lerp(x_from, x_To, x / thx);
                    float py = Mathf.Lerp(y_from, y_To, y / thy);

                    var el = _floatheightData[(int)((px)), (int)(py)];

                    tdataHeightmap[y, x] = (el - MinElevation) / elevationRange;

                }
                lastX = x;
                if (new TimeSpan(DateTime.Now.Ticks - startTime).TotalSeconds > 1) return;
            }

            lastX = 0;
            tdata.SetHeights(0, 0, tdataHeightmap);

            tdata = null;
            generateComplete = true;

        }
        
        /// <summary>
        /// Get The Number of Tiles exiting in texture folder of terrain 
        /// </summary>
        /// <param name="terrainPath"></param>
        private void GetTilesNumberInTextureFolder(string terrainPath)
        {
            var folderPath = Path.GetDirectoryName(terrainPath);
            var TerrainFilename = Path.GetFileNameWithoutExtension(terrainPath);
            var TextureFolder = Path.Combine(folderPath, TerrainFilename + "_Textures");
            var TextureFile = Path.Combine(folderPath, TerrainFilename);

            if (Directory.Exists(TextureFolder))
            {
                var supportedExtensions = new HashSet<string> { ".png", ".jpg" };
                var tiles = Directory.GetFiles(TextureFolder, "*.*", SearchOption.AllDirectories).Where(f => supportedExtensions.Contains(new FileInfo(f).Extension, StringComparer.OrdinalIgnoreCase)).ToArray();
                var tilestotalcount = tiles.Length;

                int xtilecount = 0; int ytilecount = 0;

                for (int i = 0; i < tilestotalcount; i++)
                {
                    var fileName = Path.GetFileName(tiles[i]);

                    string[] multiArray = fileName.Split(new Char[] { '_' });

                    for (int s = 0; s < multiArray.Length; s++)
                    {
                        if (int.Parse(multiArray[2]) >= ytilecount)
                        {
                            ytilecount = int.Parse(multiArray[2]);
                        }

                    }

                }

                ytilecount = ytilecount + 1;
                xtilecount = tilestotalcount / ytilecount;

                Tiles = new Vector2(xtilecount, ytilecount);

            }
            else
                Debug.LogError("Textures folder of this terrain not exist .....");

        }

            

        
    }
}
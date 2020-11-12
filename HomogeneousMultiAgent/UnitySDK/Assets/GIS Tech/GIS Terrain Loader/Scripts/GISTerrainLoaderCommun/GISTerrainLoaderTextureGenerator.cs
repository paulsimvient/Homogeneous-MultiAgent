using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace GISTech.GISTerrainLoader
{
    public class GISTerrainLoaderTextureGenerator
    {
        public static void AddTextures(string terrainPath,TerrainObject terrainItem, Vector2 texturedim)
        {
            var terrain = terrainItem.terrain;

            bool texExist;
            var texPath = CheckForTexture(terrainPath, terrainItem, out texExist);
            if (texExist)
            {

                var textureWidth =(int) texturedim.x; 
                var textureHeight =(int) texturedim.y; 

                if (textureWidth <= 128 || textureHeight <= 128) return;
                textureWidth /= 2;
                textureHeight /= 2;

                Texture2D tex = new Texture2D(textureWidth, textureHeight);
                tex = LoadedTextureTile(texPath);


#if UNITY_2018_1_OR_NEWER
                TerrainLayer[] terrainTexture = new TerrainLayer[1];
                terrainTexture[0] = new TerrainLayer();
                terrainTexture[0].diffuseTexture = tex;
                terrainTexture[0].tileSize = new Vector2(terrainItem.size.x, terrainItem.size.z);
                terrainTexture[0].tileOffset = Vector2.zero;
                terrain.terrainData.terrainLayers = terrainTexture;
#else
            SplatPrototype sp = new SplatPrototype
            {
                texture = tex,
                tileSize = new Vector2(terrainItem.size.x, terrainItem.size.z),
                tileOffset = Vector2.zero
            };
            terrain.terrainData.splatPrototypes = new[] { sp };
#endif

            }
            else
                Debug.Log("Texture not found : " + texPath);


        }
        private static string CheckForTexture(string terrainPath, TerrainObject terrain, out bool exist)
        {
            string terrainTexture = "";
            exist = false;
            var folderPath = Path.GetDirectoryName(terrainPath);
            var TexturesFolder = Path.Combine(folderPath, Path.GetFileNameWithoutExtension(terrainPath) + "_Textures");
            var texturePath = Path.Combine(TexturesFolder, "Tile__" + terrain.Number.x.ToString() + "__" + terrain.Number.y.ToString());

            if (Directory.Exists(TexturesFolder))
            {

                if (File.Exists(texturePath + ".png"))
                {
                    texturePath = texturePath + ".png";
                    terrainTexture = texturePath;
                    exist = true;
                }
                else
                {
                    texturePath = texturePath + ".jpg";
                    terrainTexture = texturePath;
                    exist = true;
                }
            }
            return terrainTexture;
        }
        static Texture2D LoadedTextureTileAsync(string terrainPath)
        {
            Texture2D tex = new Texture2D(2, 2);

            var folderPath = Path.GetDirectoryName(terrainPath);
            var TerrainFilename = Path.GetFileNameWithoutExtension(terrainPath);
            var TextureFolder = Path.Combine(folderPath, TerrainFilename + "_Texture");
            var TextureFile = Path.Combine(folderPath, TerrainFilename);

            if (!Directory.Exists(TextureFolder))
            {
                Directory.CreateDirectory(TextureFolder);
            }

            if (File.Exists(TextureFile + ".png") || File.Exists(TextureFile + ".jpg"))
            {
                string texturePath;

                if (File.Exists(TextureFile + ".png"))
                    texturePath = TextureFile + ".png";
                else
                    texturePath = TextureFile + ".jpg";

                Debug.Log(texturePath);

                byte[] imgData;
                imgData = File.ReadAllBytes(texturePath);
                //Load raw Data into Texture2D 
                tex.LoadImage(imgData);
            }
            return tex;
        }
        static Texture2D LoadedTextureTile(string TexturePath)
        {
            Texture2D tex = new Texture2D(2, 2);
            if (File.Exists(TexturePath))
            {
                byte[] imgData;
                tex.wrapMode = TextureWrapMode.Clamp;
                imgData = File.ReadAllBytes(TexturePath);
                //Load raw Data into Texture2D 
                tex.LoadImage(imgData);
            }
            return tex;
        }
    }
}
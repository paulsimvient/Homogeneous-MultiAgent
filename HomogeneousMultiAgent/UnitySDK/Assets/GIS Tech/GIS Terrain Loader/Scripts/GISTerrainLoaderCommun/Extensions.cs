/*     Unity GIS Tech 2019-2020      */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using UnityEngine;

namespace GISTech.GISTerrainLoader
{
    public class Extensions
    {
        public static string CheckForTexture(string TexturePath, TerrainObject terrain, out bool exist)
        {
            string terrainTexture = "";
            exist = false;
            var textureFilePath = Path.Combine(TexturePath, "Tile__" + terrain.Number.x.ToString() + "__" + terrain.Number.y.ToString());

            if (Resources.Load(textureFilePath) as Texture2D)
            {
                terrainTexture = textureFilePath;
                exist = true;
            }
            return terrainTexture;
        }
        public static string CheckForOSMFile(string TerrainFilePath, string TerrainFileName,out bool exist)
        {
            exist = false;
            string osmfile = "";

            DirectoryInfo di = new DirectoryInfo(TerrainFilePath);

            var VectorFolderPath = TerrainFileName + "_VectorData";

            for (int i = 0; i <= 5; i++)
            {
                di = di.Parent;

                VectorFolderPath = di.Name + "/" + VectorFolderPath;

                //If Directory GIS Terrains Exist
                if (di.Name == "GIS Terrains")
                {
                    var MainfolderPath = Path.GetDirectoryName(TerrainFilePath);
                    var VectorDataFolder = Path.Combine(MainfolderPath, TerrainFileName + "_VectorData");

                    osmfile = VectorDataFolder + "/"+ TerrainFileName + ".osm";

                    if (File.Exists(osmfile))
                    {
                        exist = true;
                    }
                    else
                        Debug.LogError("Osm File Not Found : Please put your terrain in GIS Terrain Loader/Recources/GIS Terrains/TerrainFileName_VectorData/TerrainFileName.osm  " + osmfile);

                    break;
                }
              

                if (i == 5)
                {
                    exist = false;
                    Debug.LogError("Vector folder not found! : Please put your terrain in GIS Terrain Loader/Recources/GIS Terrains/");
                }

            }
            return osmfile;
        }
        public static double ConvertToDouble(string s)
        {
            char systemSeparator = Thread.CurrentThread.CurrentCulture.NumberFormat.CurrencyDecimalSeparator[0];
            double result = 0;
            try
            {
                if (s != null)
                    if (!s.Contains(","))
                        result = double.Parse(s, CultureInfo.InvariantCulture);
                    else
                        result = Convert.ToDouble(s.Replace(".", systemSeparator.ToString()).Replace(",", systemSeparator.ToString()));
            }
            catch (Exception e)
            {
                try
                {
                    result = Convert.ToDouble(s);
                }
                catch
                {
                    try
                    {
                        result = Convert.ToDouble(s.Replace(",", ";").Replace(".", ",").Replace(";", "."));
                    }
                    catch
                    {
                        throw new Exception("Wrong string-to-double format  :" + e.Message);
                    }
                }
            }
            return result;
        }


        public static bool IsPointInPolygon(Vector3[] poly, float x, float y)
        {
            int i, j;
            bool c = false;
            for (i = 0, j = poly.Length - 1; i < poly.Length; j = i++)
            {
                if ((poly[i].z <= y && y < poly[j].z ||
                     poly[j].z <= y && y < poly[i].z) &&
                    x < (poly[j].x - poly[i].x) * (y - poly[i].z) / (poly[j].z - poly[i].z) + poly[i].x)
                {
                    c = !c;
                }
            }
            return c;
        }
        public static Rect GetRectFromPoints(List<Vector3> points)
        {
            return new Rect
            {
                x = points.Min(p => p.x),
                y = points.Min(p => p.z),
                xMax = points.Max(p => p.x),
                yMax = points.Max(p => p.z)
            };
        }
    }
    public static class TransformExtensions
    {
        /// <summary>
        /// Updates the local eulerAngles to a new vector3, if a value is omitted then the old value will be used.
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public static void SetLocalEulerAngles(this Transform transform, float? x = null, float? y = null, float? z = null)
        {
            var vector = new Vector3();
            if (x != null) { vector.x = x.Value; } else { vector.x = transform.localEulerAngles.x; }
            if (y != null) { vector.y = y.Value; } else { vector.y = transform.localEulerAngles.y; }
            if (z != null) { vector.z = z.Value; } else { vector.z = transform.localEulerAngles.z; }
            transform.localEulerAngles = vector;
        }

        /// <summary>
        /// Updates the position to a new vector3, if a value is omitted then the old value will be used.
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public static void SetPosition(this Transform transform, float? x = null, float? y = null, float? z = null)
        {
            var vector = new Vector3();
            if (x != null) { vector.x = x.Value; } else { vector.x = transform.position.x; }
            if (y != null) { vector.y = y.Value; } else { vector.y = transform.position.y; }
            if (z != null) { vector.z = z.Value; } else { vector.z = transform.position.z; }
            transform.position = vector;
        }
    }

    [Serializable]
    public class DVector2
    {
        public static DVector2 Zero = new DVector2(0, 0);

        public double x;
        public double y;

        private static System.Random _random = new System.Random();

        public DVector2(double x, double y)
        {
            this.x = x;
            this.y = y;
        }

        public void Reset()
        {
            x = 0;
            y = 0;
        }

        public void Normalize()
        {
            double length = Length();

            x /= length;
            y /= length;
        }

        public DVector2 Normalized()
        {
            return Clone() / Length();
        }

        public void Negate()
        {
            x = -x;
            y = -y;
        }

        public DVector2 Clone()
        {
            return new DVector2(x, y);
        }

        public static DVector2 operator +(DVector2 a, DVector2 b)
        {
            return new DVector2(a.x + b.x, a.y + b.y);
        }

        public static DVector2 operator -(DVector2 a, DVector2 b)
        {
            return new DVector2(a.x - b.x, a.y - b.y);
        }

        public static DVector2 operator *(DVector2 a, double b)
        {
            return new DVector2(a.x * b, a.y * b);
        }

        public static DVector2 operator /(DVector2 a, DVector2 b)
        {
            return new DVector2(a.x / b.x, a.y / b.y);
        }

        public static DVector2 operator /(DVector2 a, double b)
        {
            return new DVector2(a.x / b, a.y / b);
        }

        public void Accumulate(DVector2 other)
        {
            x += other.x;
            y += other.y;
        }

        public DVector2 Divide(float scalar)
        {
            return new DVector2(x / scalar, y / scalar);
        }

        public DVector2 Divide(double scalar)
        {
            return new DVector2(x / scalar, y / scalar);
        }

        public double Dot(DVector2 v)
        {
            return x * v.x + y * v.y;
        }

        public double Cross(DVector2 v)
        {
            return x * v.y - y * v.x;
        }

        public double Length()
        {
            return Math.Sqrt(x * x + y * y);
        }

        public double LengthSquared()
        {
            return x * x + y * y;
        }

        public double Angle()
        {
            return Math.Atan2(y, x);
        }

        public static DVector2 Lerp(DVector2 from, DVector2 to, double t)
        {
            return new DVector2(from.x + t * (to.x - from.x),
                               from.y + t * (to.y - from.y));
        }

        public static DVector2 FromAngle(double angle)
        {
            return new DVector2(Math.Cos(angle), Math.Sin(angle));
        }

        public static double Distance(DVector2 v1, DVector2 v2)
        {
            return (v2 - v1).Length();
        }

        public static DVector2 RandomUnitVector()
        {
            double angle = _random.NextDouble() * Math.PI * 2;

            return new DVector2(Math.Cos(angle), Math.Sin(angle));
        }

        public override string ToString()
        {
            return "{" + Math.Round(x, 5) + "," + Math.Round(y, 5) + "}";
        }
    }
}
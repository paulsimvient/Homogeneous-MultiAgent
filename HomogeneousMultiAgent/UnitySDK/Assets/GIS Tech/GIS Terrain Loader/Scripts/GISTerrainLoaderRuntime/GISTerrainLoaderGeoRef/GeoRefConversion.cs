/*     Unity GIS Tech 2019-2020      */

using System;
using UnityEngine;

namespace GISTech.GISTerrainLoader
{
    public class GeoRefConversion 
    {
        /// <summary>
        /// Convert Unity World space (X,Y,Z) coordinates to (Lat, Lon) coordinates
        /// </summary>
        /// <returns>
        /// Returns DVector2 containing Latitude and Longitude
        /// </returns>
        /// <param name='position'>
        /// (X,Y,Z) Position Parameter
        /// </param>
        public static DVector2 UWSToLatLog(Vector3 position, float scale)
        {
            FindMetersPerLat(_LatOrigin);
            DVector2 geoLocation = new DVector2(0, 0);
            geoLocation.y = (_LatOrigin + (position.z * scale) / metersPerLat); 
            geoLocation.x = (_LonOrigin + (position.x * scale) / metersPerLon); 
            return geoLocation;
        }

        /// <summary>
        /// Convert (Lat, Lon) coordinates to Unity World space (X,Y,Z) coordinates
        /// </summary>
        /// <returns>
        /// Returns a Vector3 containing (X, Y, Z)
        /// </returns>
        /// <param name='latlon'>
        /// (Lat, Lon) as Vector2
        /// </param>
        public static Vector3 LatLogToUWS(DVector2 latlon, int scale)
        {
            FindMetersPerLat(_LatOrigin);
            double zPosition = metersPerLat * (latlon.y - _LatOrigin);
            double xPosition = metersPerLon * (latlon.x - _LonOrigin);
            return new Vector3((float)zPosition / scale, 0, (float)xPosition / scale);
        }
        /// <summary>
        /// Convert (Lat, Lon) coordinates to Unity World space (X,Y,Z) coordinates
        /// </summary>
        /// <returns>
        /// Returns a Vector3 containing (X, Y, Z)
        /// </returns>
        /// <param name='latlon'>
        /// (Lat, Lon) as Vector2
        /// </param>
        public static Vector3 LatLogToUWS(DVector2 latlon, int scale, DVector2 origin)
        {
            SetLocalOrigin(origin);
               FindMetersPerLat(_LatOrigin);
            double zPosition = metersPerLat * (latlon.y - _LatOrigin);
            double xPosition = metersPerLon * (latlon.x - _LonOrigin);
            return new Vector3((float)zPosition / scale, 0, (float)xPosition / scale);
        }
        /// <summary>
        /// Change the relative origin offset (Lat, Lon), the Default is (0,0), 
        /// used to bring a local area to (0,0,0) in UCS coordinate system
        /// </summary>
        /// <param name='localOrigin'>
        /// Referance point.
        /// </param>
        public static void SetLocalOrigin(DVector2 origine)
        {
            Origine.x = origine.x;

            Origine.y = origine.y;
        }
 
 
        private static DVector2 Origine = new DVector2(0, 0);

        private static double _LatOrigin { get { return Origine.y; } }
        private static double _LonOrigin { get { return Origine.x; } }

        private static float metersPerLat;
        private static float metersPerLon;
 
        private static void FindMetersPerLat(double lat)
        {
            // Compute lengths of degrees
            // Set up "Constants"
            float m1 = 111132.92f;     
            float m2 = -559.82f;        
            float m3 = 1.175f;       
            float m4 = -0.0023f;         

            float p1 = 111412.84f;     
            float p2 = -93.5f;      
            float p3 = 0.118f;       

            lat = lat * Mathf.Deg2Rad;

            // Calculate the length of a degree of latitude and longitude in meters
            metersPerLat = m1 + (m2 * Mathf.Cos(2 * (float)lat)) + (m3 * Mathf.Cos(4 * (float)lat)) + (m4 * Mathf.Cos(6 * (float)lat));

            metersPerLon = (p1 * Mathf.Cos((float)lat)) + (p2 * Mathf.Cos(3 * (float)lat)) + (p3 * Mathf.Cos(5 * (float)lat));
        }
 
        /// <summary>
        /// Calculate the distance between two Lat/Log Points.
        /// </summary>
        /// <param name="lon1"></param>
        /// <param name="lat1"></param>
        /// <param name="lon2"></param>
        /// <param name="lat2"></param>
        /// <returns></returns>
        public static double Getdistance(double lat1, double lon1, double lat2, double lon2, char unit = 'K')
        {
            if ((lat1 == lat2) && (lon1 == lon2))
            {
                return 0;
            }
            else
            {
                var radlat1 = Math.PI * lat1 / 180;
                var radlat2 = Math.PI * lat2 / 180;
                var theta = lon1 - lon2;
                var radtheta = Math.PI * theta / 180;
                var dist = Math.Sin(radlat1) * Math.Sin(radlat2) + Math.Cos(radlat1) * Math.Cos(radlat2) * Math.Cos(radtheta);
                if (dist > 1)
                {
                    dist = 1;
                }
                dist = Math.Acos(dist);
                dist = dist * 180 / Math.PI;
                dist = dist * 60 * 1.1515;
                if (unit == 'K') { dist = dist * 1.609344; }
                if (unit == 'N') { dist = dist * 0.8684; }
                return dist;
            }
        }

        public const double DEG2RAD = Math.PI / 180;
        public static DVector2 LatLongToMercat(double x, double y)
        {
            double sy = Math.Sin(y * DEG2RAD);
            var mx = (x + 180) / 360;
            var my = 0.5 - Math.Log((1 + sy) / (1 - sy)) / (Math.PI * 4);

            return new DVector2(mx, my);
        }
        public static Vector3 MercatCoordsToWorld(double mx, float y, double mz, TerrainContainerObject container)
        {
            var sx = (mx - container.TLPointMercator.x) / (container.DRPointMercator.x - container.TLPointMercator.x) * container.size.x;
            var sz = (1 - (mz - container.TLPointMercator.y) / (container.DRPointMercator.y - container.TLPointMercator.y)) * container.size.z;
            return new Vector3((float)sx, y * container.scale.y, (float)sz);
        }

    }
}
﻿/*     Unity GIS Tech 2019-2020      */
using UnityEngine;

namespace GISTech.GISTerrainLoader
{
    public abstract class Singleton<T> where T : class, new()
    {
        private static T _instance = new T();
        public static T Get { get { return _instance; } }
    }
}

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;
using System;

namespace WireTerrain
{
    public class DensitySliderGrid : DensitySlider
    {
        [SerializeField]
        private WireTerrainGrid target1;
        [SerializeField]
        private WireTerrainGrid target2;

        protected override void UpdateDensity(float val)
        {
            target1.CellCountX = (int)val;
            target1.CellCountZ = (int)val;
            target2.CellCountX = (int)val;
            target2.CellCountZ = (int)val;
        }
    }
}

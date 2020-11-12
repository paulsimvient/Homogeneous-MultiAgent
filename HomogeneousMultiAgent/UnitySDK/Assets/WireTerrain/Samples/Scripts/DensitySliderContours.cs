using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;
using System;

namespace WireTerrain
{
    public class DensitySliderContours : DensitySlider
    {
        [SerializeField]
        private WireTerrainContours target1;
        [SerializeField]
        private WireTerrainContours target2;

        protected override void UpdateDensity(float val)
        {
            target1.HeightStep = val;

            target2.HeightStep = val;
        }
    }
}

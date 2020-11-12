using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;
using System;

namespace WireTerrain
{
    public abstract class DensitySlider : MonoBehaviour
    {
        [SerializeField]
        private Slider slider;
        [SerializeField]
        private float initialVal;
        [SerializeField]
        private float minVal;
        [SerializeField]
        private float maxVal;

        void Start()
        {            
            slider.minValue = minVal;
            slider.maxValue = maxVal;
            slider.value = initialVal;
            slider.onValueChanged.AddListener(UpdateDensity);
        }

        protected abstract void UpdateDensity(float val);
    }
}

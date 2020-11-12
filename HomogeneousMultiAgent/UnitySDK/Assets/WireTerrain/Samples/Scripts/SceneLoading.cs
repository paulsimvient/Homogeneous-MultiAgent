using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

namespace WireTerrain
{
    public class SceneLoading : MonoBehaviour
    {

        [SerializeField]
        private Toggle scene1Toggle;
        [SerializeField]
        private Toggle scene2Toggle;
        private string scene1Name = "WireContoursTerrain";
        private string scene2Name = "WireGridTerrain";

        void Start()
        {
            if (SceneManager.GetActiveScene().name == scene1Name)
            {
                scene1Toggle.isOn = true;
            }
            else
            {
                scene2Toggle.isOn = true;
            }
            scene1Toggle.onValueChanged.AddListener(ToggleSceneSelection);
            scene2Toggle.onValueChanged.AddListener(ToggleSceneGimbal);
        }

        void ToggleSceneSelection(bool isOn)
        {
            if (isOn)
            {
                SceneManager.LoadScene(scene1Name);
            }

        }

        void ToggleSceneGimbal(bool isOn)
        {
            if (isOn)
            {
                SceneManager.LoadScene(scene2Name);
            }

        }
    }
}

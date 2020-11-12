using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Automatic camera setting to frame objects inside the TargetPanel area.
/// OBS.: Not stable for object follow or continuously moving camera
/// </summary>
public class CameraFraming : MonoBehaviour
{
    //Renderer[] targets;
    public Camera mainCamera;
    RectTransform targetPanel;
    Canvas canvas;

    public enum WrappingMode
    {
        FitInside, Width, Height
    }
    public WrappingMode wrappingMode;
    public int maxIterations;
    public float positionPrecision;

    public int cycles;

    Vector3 newCamPosition;
    float newFieldOfView;

    void Start()
    {
        canvas = transform.GetComponentInChildren<Canvas>();
        foreach(Transform canvasChild in canvas.transform){
            if(canvasChild.name == "TargetPanel"){
                targetPanel = canvasChild.GetComponent<RectTransform>();
            }
        }

        newCamPosition = mainCamera.transform.position;
        newFieldOfView = mainCamera.fieldOfView;
    }

    public BEController beController;

    void Update()
    {
        canvas.scaleFactor = beController.beUIController.uiScale;
    }

    /// <summary>
    /// Get max and min points of the object according to each axis 
    /// </summary>
    /// <param name="obj">Renderer of the object</param>
    /// <returns>Return a Vector3[]{maxX, minX, maxY, minY, maxZ, minZ}</returns>
    Vector3[] GetMaxMinBoundaryPoints(Renderer obj)
    {
        Vector3 resultPointMaxX = mainCamera.WorldToScreenPoint(obj.bounds.max);
        Vector3 resultPointMinX = mainCamera.WorldToScreenPoint(obj.bounds.max);
        Vector3 resultPointMaxY = mainCamera.WorldToScreenPoint(obj.bounds.max);
        Vector3 resultPointMinY = mainCamera.WorldToScreenPoint(obj.bounds.max);
        Vector3 resultPointMaxZ = mainCamera.WorldToScreenPoint(obj.bounds.max);
        Vector3 resultPointMinZ = mainCamera.WorldToScreenPoint(obj.bounds.max);

        for (int x = 0; x < 2; x++)
        {
            for (int y = 0; y < 2; y++)
            {
                for (int z = 0; z < 2; z++)
                {
                    Vector3 testPoint = mainCamera.WorldToScreenPoint(obj.bounds.max - new Vector3(obj.transform.lossyScale.x * x,
                                                                     obj.transform.lossyScale.y * y,
                                                                     obj.transform.lossyScale.z * z));

                    if (resultPointMaxX.x < testPoint.x)
                    {
                        resultPointMaxX = testPoint;
                    }
                    if (resultPointMinX.x > testPoint.x)
                    {
                        resultPointMinX = testPoint;
                    }
                    if (resultPointMaxY.y < testPoint.y)
                    {
                        resultPointMaxY = testPoint;
                    }
                    if (resultPointMinY.y > testPoint.y)
                    {
                        resultPointMinY = testPoint;
                    }
                    if (resultPointMaxZ.z < testPoint.z)
                    {
                        resultPointMaxZ = testPoint;
                    }
                    if (resultPointMinZ.z > testPoint.z)
                    {
                        resultPointMinZ = testPoint;
                    }
                }
            }
        }
        return new Vector3[] { resultPointMaxX, resultPointMinX, resultPointMaxY, resultPointMinY, resultPointMaxZ, resultPointMinZ };
    }

    /// <summary>
    /// Get max and min points of the scene objects according to each axis 
    /// </summary>
    /// <param name="targets">Renderer list of the scene objects</param>
    /// <returns>Return a Vector3[]{maxX, minX, maxY, minY, maxZ, minZ}</returns>
    Vector3[] GetSceneMaxMinBoundaryPoints(Renderer[] targets)
    {
        var smaxX = mainCamera.WorldToScreenPoint(targets[0].bounds.center);
        var sminX = mainCamera.WorldToScreenPoint(targets[0].bounds.center);
        var smaxY = mainCamera.WorldToScreenPoint(targets[0].bounds.center);
        var sminY = mainCamera.WorldToScreenPoint(targets[0].bounds.center);
        var smaxZ = mainCamera.WorldToScreenPoint(targets[0].bounds.center);
        var sminZ = mainCamera.WorldToScreenPoint(targets[0].bounds.center);

        foreach (Renderer bound in targets)
        {
            Vector3[] bounds_ = GetMaxMinBoundaryPoints(bound);
            var smaxX_ = bounds_[0];
            var sminX_ = bounds_[1];
            var smaxY_ = bounds_[2];
            var sminY_ = bounds_[3];
            var smaxZ_ = bounds_[4];
            var sminZ_ = bounds_[5];

            if (smaxX_.x > smaxX.x)
            {
                smaxX = smaxX_;
            }
            if (sminX_.x < sminX.x)
            {
                sminX = sminX_;
            }
            if (smaxY_.y > smaxY.y)
            {
                smaxY = smaxY_;
            }
            if (sminY_.y < sminY.y)
            {
                sminY = sminY_;
            }
            if (smaxZ_.z > smaxZ.z)
            {
                smaxZ = smaxZ_;
            }
            if (sminZ_.z < sminZ.z)
            {
                sminZ = sminZ_;
            }
        }

        return new Vector3[] { smaxX, sminX, smaxY, sminY, smaxZ, sminZ };
    }

    /// <summary>
    /// Adjust camera position and field of view to frame objects inside screen area set by the TargetPanel rect
    /// </summary>
    /// <param name="targets">Renderer list of the scene objects to be framed</param>
    public void CentralizeCamera(Renderer[] targets)
    {
        cycles = 0;

        Renderer maxXObject = targets[0];
        Renderer minXObject = targets[0];
        Renderer maxYObject = targets[0];
        Renderer minYObject = targets[0];
        Renderer maxZObject = targets[0];
        Renderer minZObject = targets[0];

        float panelRight = targetPanel.position.x + (targetPanel.rect.width / 2);
        float panelLeft = targetPanel.position.x - (targetPanel.rect.width / 2);
        float panelTop = targetPanel.position.y + (targetPanel.rect.height / 2);
        float panelBottom = targetPanel.position.y - (targetPanel.rect.height / 2);

        for (int i = 0; i < maxIterations; i++)
        {
            Vector3[] bounds_ = GetSceneMaxMinBoundaryPoints(targets);

            // max and min objects screen points 
            var smaxX = bounds_[0];
            var sminX = bounds_[1];
            var smaxY = bounds_[2];
            var sminY = bounds_[3];
            var smaxZ = bounds_[4];
            var sminZ = bounds_[5];

            float sizeX = smaxX.x - sminX.x;
            float sizeY = smaxY.y - sminY.y;

            Vector3 wmaxX = mainCamera.ScreenToWorldPoint(smaxX);
            Vector3 wminX = mainCamera.ScreenToWorldPoint(sminX);
            Vector3 wmaxY = mainCamera.ScreenToWorldPoint(smaxY);
            Vector3 wminY = mainCamera.ScreenToWorldPoint(sminY);

            Vector3 center = mainCamera.ScreenToWorldPoint(new Vector3(((smaxX + sminX) / 2).x, ((smaxZ + sminZ) / 2).y, ((smaxZ + sminZ) / 2).z));

            float wsizeX = wmaxX.x - wminX.x;
            float wsizeY = wmaxY.y - wminY.y;

            float distance = Vector3.Dot(center - mainCamera.transform.position, mainCamera.transform.forward);
            var frustumHeight = 2.0f * distance * Mathf.Tan(mainCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
            var frustumWidth = frustumHeight * mainCamera.aspect;

            if (distance < 0)
            {
                //negative distance
                mainCamera.transform.LookAt(center, Vector3.up);
            }

            float panelWidth = Mathf.Abs(panelLeft - panelRight);
            float panelHeight = Mathf.Abs(panelTop - panelBottom);

            float wpanelHeight = (wsizeY / sizeY) * panelHeight;
            float wpanelWidth = (wsizeX / sizeX) * panelWidth;

            if (wrappingMode == WrappingMode.Width)
            {
                float newFrustumWidth = (wsizeX / wpanelWidth) * frustumWidth;
                var newFrustumHeight = newFrustumWidth / mainCamera.aspect;
                newFieldOfView = Mathf.Atan(newFrustumHeight / (2.0f * distance)) / (0.5f * Mathf.Deg2Rad);
            }
            if (wrappingMode == WrappingMode.Height)
            {
                float newFrustumHeight = (wsizeY / wpanelHeight) * frustumHeight;
                newFieldOfView = Mathf.Atan(newFrustumHeight / (2.0f * distance)) / (0.5f * Mathf.Deg2Rad);
            }
            if (wrappingMode == WrappingMode.FitInside)
            {
                float panelAspect = panelWidth / panelHeight;
                float targetsAspect = sizeX / sizeY;

                if (panelAspect >= targetsAspect)
                {
                    float newFrustumHeight = (wsizeY / wpanelHeight) * frustumHeight;
                    newFieldOfView = Mathf.Atan(newFrustumHeight / (2.0f * distance)) / (0.5f * Mathf.Deg2Rad) / canvas.scaleFactor;
                }
                else
                {
                    float newFrustumWidth = (wsizeX / wpanelWidth) * frustumWidth;
                    var newFrustumHeight = newFrustumWidth / mainCamera.aspect;
                    newFieldOfView = Mathf.Atan(newFrustumHeight / (2.0f * distance)) / (0.5f * Mathf.Deg2Rad) / canvas.scaleFactor;
                }
            }
            mainCamera.fieldOfView = newFieldOfView;

            if (mainCamera.fieldOfView < 0)
            {
                mainCamera.fieldOfView = 1;
            }

            var wpanelCenter = mainCamera.ScreenToWorldPoint(new Vector3(targetPanel.position.x, targetPanel.position.y, mainCamera.WorldToScreenPoint(center).z));

            var xX = Vector3.Dot(center - mainCamera.transform.position, mainCamera.transform.right) -
                                    Vector3.Dot(wpanelCenter - mainCamera.transform.position, mainCamera.transform.right);
            var yY = Vector3.Dot(center - mainCamera.transform.position, mainCamera.transform.up) -
                        Vector3.Dot(wpanelCenter - mainCamera.transform.position, mainCamera.transform.up);

            newCamPosition += (mainCamera.transform.right * xX) + (mainCamera.transform.up * yY);
            mainCamera.transform.position = newCamPosition;

            cycles++;
        }

    }

}

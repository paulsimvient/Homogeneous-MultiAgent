//===========================================================================//
//                      LinePathTracing (Version 1.0)                        //
//                        (c) 2019 Sergey Stafeyev                           //
//===========================================================================//

using System;
using UnityEditor;
using UnityEngine;

[AddComponentMenu("Tools/Line Path Tracing")]
public class LinePathTracing : MonoBehaviour
{
    #region UI

    [SerializeField]
    private EDrawQuality _drawQualityPreset = EDrawQuality.Middle;

    [SerializeField]
    private Color _lineColor = Color.yellow;

    [SerializeField]
    private float _lineThickness = 0.025f;

    [SerializeField]
    private float _minLineLength = 3f;

    [SerializeField]
    private float _minLineAngle = 0.5f;

    [SerializeField]
    private bool _showLinesConnections = false;

    [SerializeField]
    private Color _connectionColor = Color.red;

    [SerializeField]
    private float _connectionSize = 0.1f;

    [SerializeField]
    private bool _destroyTimer = false;

    [SerializeField]
    private float _lifeTime = 15;

    #endregion UI

    private static int _lineNum = -1;

    private Transform _linesContainer;
    private GameObject _lineInstance;
    private GameObject _lineConnectionInstance;
    private GameObject _lastLineConnection;

    private Vector3? _prevPos;
    private Vector3? _prevDirect;
    private Vector3? _prevPointPos;

    private bool _isQuiting;


    private void Start()
    {
        _prevPointPos = transform.position;
        _lineNum++;

        // Create objects container
        GameObject go = new GameObject();
        go.name = "Line_" + _lineNum;
        _linesContainer = go.transform;

        if (_destroyTimer)
        {
            var destroyTimer = _linesContainer.gameObject.AddComponent<LPTDestroyTimer>();
            destroyTimer.Run(_lifeTime);
        }

        _lineInstance = CreateLineInstance(_lineThickness, _lineColor);
        _lineInstance.transform.SetParent(_linesContainer);

        if (_showLinesConnections)
        {
            _lineConnectionInstance = CreateLineConnectionInstance(_connectionSize, _connectionColor);
            _lineConnectionInstance.transform.SetParent(_linesContainer);
        }
    }

    private void OnEnable()
    {
        _prevPos = null;
        _prevDirect = null;
        _prevPointPos = transform.position;
    }

    private GameObject CreateLineInstance(float thickness, Color color)
    {
        Vector3[] verts = new Vector3[8];

        float halfLineThickness = thickness / 2f;

        verts[0] = new Vector3(0, halfLineThickness, 1);
        verts[1] = new Vector3(0, halfLineThickness, 0);
        verts[2] = new Vector3(0, -halfLineThickness, 1);
        verts[3] = new Vector3(0, -halfLineThickness, 0);
        verts[4] = new Vector3(halfLineThickness, 0, 1);
        verts[5] = new Vector3(halfLineThickness, 0, 0);
        verts[6] = new Vector3(-halfLineThickness, 0, 1);
        verts[7] = new Vector3(-halfLineThickness, 0, 0);

        int[] tris = new int[24] {
            0, 1, 2,
            2, 1, 3,
            1, 0, 2,
            3, 1, 2,
            4, 5, 6,
            6, 5, 7,
            5, 4, 6,
            7, 5, 6
        };

        Vector2[] uvs = new Vector2[8];

        uvs[0] = new Vector2(0, 1);
        uvs[1] = new Vector2(1, 1);
        uvs[2] = new Vector2(0, 0);
        uvs[3] = new Vector2(1, 0);
        uvs[4] = new Vector2(0, 1);
        uvs[5] = new Vector2(1, 1);
        uvs[6] = new Vector2(0, 0);
        uvs[7] = new Vector2(1, 0);

        Mesh mesh = new Mesh();

        mesh.vertices = verts;
        mesh.triangles = tris;
        mesh.uv = uvs;

        mesh.RecalculateNormals();

        // Instantiate new GameObject
        GameObject go = new GameObject();
        go.SetActive(false);
        go.name = "Line";
        go.transform.position = Vector3.zero;

        go.AddComponent<MeshFilter>().mesh = mesh;

        MeshRenderer renderer = go.AddComponent<MeshRenderer>();
        renderer.material.shader = Shader.Find("Unlit/Color");
        renderer.material.color = color;

        return go;
    }

    private GameObject CreateLineConnectionInstance(float size, Color color)
    {
        Vector3[] verts = new Vector3[8];

        float halfConnectionSize = size / 2f;
        float halfDiagonalSize = halfConnectionSize * 0.8f;

        verts[0] = new Vector3(-halfDiagonalSize, halfDiagonalSize, 0);
        verts[1] = new Vector3(0, 0, -halfConnectionSize);
        verts[2] = new Vector3(halfDiagonalSize, -halfDiagonalSize, 0);
        verts[3] = new Vector3(0, 0, halfConnectionSize);
        verts[4] = new Vector3(halfDiagonalSize, halfDiagonalSize, 0);
        verts[5] = new Vector3(0, 0, -halfConnectionSize);
        verts[6] = new Vector3(-halfDiagonalSize, -halfDiagonalSize, 0);
        verts[7] = new Vector3(0, 0, halfConnectionSize);

        int[] tris = new int[24] {
            0, 1, 2,
            2, 3, 0,
            0, 3, 2,
            2, 1, 0,
            4, 5, 6,
            6, 7, 4,
            4, 7, 6,
            6, 5, 4
        };

        Vector2[] uvs = new Vector2[8];

        uvs[0] = new Vector2(0, 1);
        uvs[1] = new Vector2(1, 1);
        uvs[2] = new Vector2(0, 0);
        uvs[3] = new Vector2(1, 0);
        uvs[4] = new Vector2(0, 1);
        uvs[5] = new Vector2(1, 1);
        uvs[6] = new Vector2(0, 0);
        uvs[7] = new Vector2(1, 0);

        Mesh mesh = new Mesh();

        mesh.vertices = verts;
        mesh.triangles = tris;
        mesh.uv = uvs;

        mesh.RecalculateNormals();

        // Instantiate new GameObject
        GameObject go = new GameObject();
        go.SetActive(false);
        go.name = "Connection";
        go.transform.position = Vector3.zero;

        go.AddComponent<MeshFilter>().mesh = mesh;

        MeshRenderer renderer = go.AddComponent<MeshRenderer>();
        renderer.material.shader = Shader.Find("Unlit/Color");
        renderer.material.color = color;

        return go;
    }

    private GameObject DrawLineAB(Vector3 a, Vector3 b, string name = null)
    {
        float distance = Vector3.Distance(a, b);

        GameObject line = GameObject.Instantiate(_lineInstance);
        line.SetActive(true);
        line.transform.localScale = new Vector3(1f, 1f, distance);
        line.transform.position = a;
        line.transform.LookAt(b);

        if (name != null)
            line.name = name;

        return line;
    }

    private GameObject DrawConnection(Vector3 pos, Vector3 direct, string name = null)
    {
        GameObject go = GameObject.Instantiate(_lineConnectionInstance);
        go.SetActive(true);
        go.transform.localScale = Vector3.one;
        go.transform.position = pos;
        go.transform.LookAt(pos + direct);

        if (name != null)
            go.name = name;

        return go;
    }

    private void DrawWayLine()
    {
        if (_prevPos == null)
        {
            _prevPos = transform.position;
            return;
        }

        if (_prevDirect == null)
        {
            _prevDirect = transform.position - _prevPos;
            _prevPos = transform.position;
            return;
        }

        Vector3 _newDirect = transform.position - (Vector3)_prevPos;

        float angle = Vector3.Angle((Vector3)_prevDirect, _newDirect);

        if (angle > _minLineAngle && (Vector3)_prevPos != (Vector3)_prevPointPos)
        {
            GameObject line = DrawLineAB((Vector3)_prevPointPos, (Vector3)_prevPos, "Line_" + _lineNum);
            line.transform.SetParent(_linesContainer);

            line = DrawLineAB((Vector3)_prevPos, transform.position, "Line_" + _lineNum);
            line.transform.SetParent(_linesContainer);

            if (_showLinesConnections)
            {
                GameObject connection = DrawConnection((Vector3)_prevPos, (Vector3)_prevPointPos);
                connection.transform.SetParent(_linesContainer);

                connection = DrawConnection(transform.position, (Vector3)_prevPos);
                connection.transform.SetParent(_linesContainer);

                _lastLineConnection = connection;
            }

            _prevPointPos = transform.position;
        }
        else
        {
            if (Vector3.Distance((Vector3)_prevPointPos, transform.position) > _minLineLength)
            {
                GameObject line = DrawLineAB((Vector3)_prevPointPos, transform.position, "Line_" + _lineNum);
                line.transform.SetParent(_linesContainer);

                _prevPointPos = transform.position;
                
                if (_showLinesConnections)
                {
                    GameObject connection = DrawConnection(transform.position, (Vector3)_prevPointPos);
                    connection.transform.SetParent(_linesContainer);

                    _lastLineConnection = connection;
                }
            }
        }

        _prevDirect = _newDirect;
        _prevPos = transform.position;
    }

    private void FixedUpdate()
    {
        if (_linesContainer == null)
            return;

        DrawWayLine();
    }

    private void OnDisable()
    {
        if (_isQuiting)
            return;

        _minLineLength = 0; // Complete line

        DrawWayLine();

        Destroy(_lastLineConnection);
    }

    private void OnApplicationQuit()
    {
        _isQuiting = true;
    }

    public enum EDrawQuality
    {
        Custom = 0,
        Low = 1,
        Middle = 2,
        High = 3
    }
}

[CustomEditor(typeof(LinePathTracing))]
[CanEditMultipleObjects]
public class LinePathTracingEditor : Editor
{
    SerializedProperty _drawQualityPreset;
    SerializedProperty _lineColor;
    SerializedProperty _lineThickness;
    SerializedProperty _minLineLength;
    SerializedProperty _minLineAngle;
    SerializedProperty _showLinesConnections;
    SerializedProperty _connectionColor;
    SerializedProperty _connectionSize;
    SerializedProperty _destroyTimer;
    SerializedProperty _lifeTime;

    // Temp
    private int _drawQualityPresetCurrent;


    protected virtual void OnEnable()
    {
        _drawQualityPreset = serializedObject.FindProperty("_drawQualityPreset");
        _lineColor = serializedObject.FindProperty("_lineColor");
        _lineThickness = serializedObject.FindProperty("_lineThickness");
        _minLineLength = serializedObject.FindProperty("_minLineLength");
        _minLineAngle = serializedObject.FindProperty("_minLineAngle");
        _showLinesConnections = serializedObject.FindProperty("_showLinesConnections");
        _connectionColor = serializedObject.FindProperty("_connectionColor");
        _connectionSize = serializedObject.FindProperty("_connectionSize");
        _destroyTimer = serializedObject.FindProperty("_destroyTimer");
        _lifeTime = serializedObject.FindProperty("_lifeTime");

        _drawQualityPresetCurrent = _drawQualityPreset.enumValueIndex;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        GUILayout.Space(10);

        _lineColor.colorValue = EditorGUILayout.ColorField("Line Color", _lineColor.colorValue);
        _lineThickness.floatValue = EditorGUILayout.Slider("Line Thickness", _lineThickness.floatValue, 0.01f, 0.8f);

        GUILayout.Space(5);

        DrawQualityPreset();

        if ((LinePathTracing.EDrawQuality)_drawQualityPreset.enumValueIndex != LinePathTracing.EDrawQuality.Custom)
            GUI.enabled = false;

        _minLineLength.floatValue = EditorGUILayout.FloatField("Min Line Length", _minLineLength.floatValue);
        _minLineAngle.floatValue = EditorGUILayout.FloatField("Min Line Angle", _minLineAngle.floatValue);

        GUI.enabled = true;

        GUILayout.Space(10);

        _showLinesConnections.boolValue = EditorGUILayout.Toggle("Show Lines Connections", _showLinesConnections.boolValue);
        _connectionColor.colorValue = EditorGUILayout.ColorField("Connection Color", _connectionColor.colorValue);
        _connectionSize.floatValue = EditorGUILayout.Slider("Connection Size", _connectionSize.floatValue, 0.02f, 1f);

        GUILayout.Space(10);

        _destroyTimer.boolValue = EditorGUILayout.Toggle("Destroy Timer", _destroyTimer.boolValue);

        if (_destroyTimer.boolValue)
            _lifeTime.floatValue = EditorGUILayout.FloatField("Life Time", _lifeTime.floatValue);

        GUILayout.Space(5);

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawQualityPreset()
    {
        _drawQualityPreset.enumValueIndex = 
            (int)(LinePathTracing.EDrawQuality)EditorGUILayout.EnumPopup("Draw Quality Preset", 
            (LinePathTracing.EDrawQuality)Enum.GetValues(typeof(LinePathTracing.EDrawQuality)).
            GetValue(_drawQualityPreset.enumValueIndex));

        if (_drawQualityPreset.enumValueIndex == _drawQualityPresetCurrent)
            return;

        _drawQualityPresetCurrent = _drawQualityPreset.enumValueIndex;

        switch ((LinePathTracing.EDrawQuality)_drawQualityPreset.enumValueIndex)
        {
            case LinePathTracing.EDrawQuality.High:
                _minLineAngle.floatValue = 0.3f;
                _minLineLength.floatValue = 1f;
                break;
            case LinePathTracing.EDrawQuality.Middle:
                _minLineAngle.floatValue = 0.5f;
                _minLineLength.floatValue = 3f;
                break;
            case LinePathTracing.EDrawQuality.Low:
                _minLineAngle.floatValue = 1.5f;
                _minLineLength.floatValue = 5f;
                break;
        }
    }
}
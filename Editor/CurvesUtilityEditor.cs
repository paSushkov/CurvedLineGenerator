using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System;


[CustomEditor(typeof(CurvesUtility))]
[CanEditMultipleObjects]
[Serializable]
public class CurvesUtilityEditor : Editor
{
    private Curves.LineTypes _type;
    private bool _cuttable;
    private bool _dynamicCut;

    private Vector3 _start;
    private Vector3 _end;

    private Vector3 _control1Vect;
    private Vector3 _control2Vect;

    private Transform _startObj;
    private Transform _endObj;

    private Transform _control1;
    private Transform _control2;

    public Vector3 _direction;
    public float _distance;
    public bool _allowMoveObjects;

    private int _detail;
    private int _amplitude;
    private float _wavePower;
    private float _noise;
    private bool _renderLine;
    private bool _hideStart;
    private bool _AutoRebuild;
    private float _RebuildFreq;

    CurvesUtility lineEditor;


    private String[] PointOptions = Enum.GetNames(typeof(CurvesUtility.PointType));

    private CurvesUtility.PointType _startSelection;
    private CurvesUtility.PointType _endSelection;
    private CurvesUtility.PointType _CTRL_1Selection;
    private CurvesUtility.PointType _CTRL_2Selection;
    public static void DrawUILine(Color color, int thickness = 2, int padding = 10)
    {
        Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(padding + thickness));
        r.height = thickness;
        r.y += padding / 2;
        r.x -= 2;
        r.width += 6;
        EditorGUI.DrawRect(r, color);
    }
    private void PointControl(string Label,
                                ref CurvesUtility.PointType editorPointType,
                                ref CurvesUtility.PointType currentPointType,
                                ref Transform editorObj, ref Transform currentObj,
                                ref Vector3 editorVector, ref Vector3 currentVector)
    {
        GUILayout.BeginHorizontal();

        EditorGUILayout.PrefixLabel(Label, EditorStyles.boldLabel);
        EditorGUI.BeginChangeCheck();
        editorPointType = (CurvesUtility.PointType)GUILayout.SelectionGrid((int)currentPointType, PointOptions, 2);
        GUILayout.EndHorizontal();
        if (EditorGUI.EndChangeCheck())
        {
            if (currentPointType == CurvesUtility.PointType.Object && editorPointType == CurvesUtility.PointType.Vector3 && currentObj != null)
                currentVector = lineEditor.transform.InverseTransformPoint(currentObj.position);
            currentPointType = editorPointType;
        }
        switch (currentPointType)
        {
            case (CurvesUtility.PointType.Object):
                {
                    EditorGUI.BeginChangeCheck();
                    editorObj = (Transform)EditorGUILayout.ObjectField(currentObj, typeof(Transform), true);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(target, "Changed the object");
                        currentObj = editorObj;
                        lineEditor.BuildNewLine();
                    }
                    break;
                }
            case (CurvesUtility.PointType.Vector3):
                {

                    EditorGUI.BeginChangeCheck();
                    editorVector = EditorGUILayout.Vector3Field("Coordinates", currentVector);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(target, "Changed Vector3 coordinates");
                        currentVector = editorVector;
                        lineEditor.BuildNewLine();
                    }
                    break;
                }
        }
        EditorGUILayout.Space(5f);
    }


    private void handlePoint(ref Vector3 originalPoint, Transform editorTransform, Quaternion editorRotation)
    {
        Vector3 point = editorTransform.TransformPoint(originalPoint);
        Quaternion handleRotation = Tools.pivotRotation == PivotRotation.Local ?
            editorTransform.rotation : Quaternion.identity;
        Handles.color = Color.white;


        EditorGUI.BeginChangeCheck();
        point = Handles.DoPositionHandle(point, handleRotation);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject((CurvesUtility)target, "Move Point");
            originalPoint = editorTransform.InverseTransformPoint(point);
            lineEditor.BuildNewLine();
        }
    }



    public override void OnInspectorGUI()
    {
        // base.OnInspectorGUI();
        lineEditor = (CurvesUtility)target;

        GUILayout.Label("Lenght: " + lineEditor.lenght.ToString("F2"), EditorStyles.boldLabel);
        EditorGUI.BeginChangeCheck();
        _type = (Curves.LineTypes)EditorGUILayout.EnumPopup("Type of the line::", lineEditor.LineType);

        EditorGUI.BeginChangeCheck();
        if (EditorGUI.EndChangeCheck())
        { 
        }
            _cuttable = EditorGUILayout.Toggle("End on collision:", lineEditor.CutLine);
        
        if (_cuttable)
        { 
        _dynamicCut = EditorGUILayout.Toggle("Dynamic cutting:", lineEditor.DynamicCutting);
        }

        _detail = EditorGUILayout.IntSlider("Dots: ", lineEditor.detail, 2, 500);
        _noise = EditorGUILayout.Slider("Noise: ", lineEditor.Noise, 0f, 10f);
        //DrawUILine(Color.gray);
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// START-END POINTS
        {
            PointControl("Start", ref _startSelection, ref lineEditor.startPointType, ref _startObj, ref lineEditor.startObj, ref _start, ref lineEditor.startVect);
            PointControl("End", ref _endSelection, ref lineEditor.endPointType, ref _endObj, ref lineEditor.endObj, ref _end, ref lineEditor.endVect);
        }
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// MANUAL DISTANCE
        {
            //DrawUILine(Color.gray,1);
            _direction = (lineEditor.end - lineEditor.start).normalized;

            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Distance: " + lineEditor.distance.ToString("F2"));

            if (lineEditor.endPointType == CurvesUtility.PointType.Object)
            {
                EditorGUI.BeginChangeCheck();
                _allowMoveObjects = EditorGUILayout.Toggle(lineEditor.allowMoveObjects);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(target, "Allowed to move objects");
                    lineEditor.allowMoveObjects = _allowMoveObjects;
                }
            }
            EditorGUI.BeginChangeCheck();

            EditorGUI.BeginDisabledGroup(lineEditor.endPointType == CurvesUtility.PointType.Object &&
                                         (_allowMoveObjects == false || lineEditor.endObj == null));
            _distance = EditorGUILayout.Slider(lineEditor.distance, 1f, 100f);
            EditorGUI.EndDisabledGroup();

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Allowed to move objects");
                lineEditor.distance = _distance;
                if (lineEditor.endPointType == CurvesUtility.PointType.Object)
                { lineEditor.endObj.position = lineEditor.start + _direction * _distance; }

                if (lineEditor.endPointType == CurvesUtility.PointType.Vector3)
                { lineEditor.endVect = lineEditor.startVect + _direction * _distance; }
            }
            GUILayout.EndHorizontal();
            DrawUILine(Color.gray);
        }
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// BEZIER CONTROLS 
        if (_type == Curves.LineTypes.QuadraticBezier || _type == Curves.LineTypes.CubicBezier)
        {
            PointControl("Control #1", ref _CTRL_1Selection, ref lineEditor.Control_1Type, ref _control1, ref lineEditor.Control_1Obj, ref _control1Vect, ref lineEditor.Control1Vect);
            if (_type == Curves.LineTypes.CubicBezier)
            {
                PointControl("Control #2", ref _CTRL_2Selection, ref lineEditor.Control_2Type, ref _control2, ref lineEditor.Control_2Obj, ref _control2Vect, ref lineEditor.Control2Vect);
            }
        }
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// CURL CONTROL 

        if (_type == Curves.LineTypes.ConeLeft || _type == Curves.LineTypes.ConeRight ||
            _type == Curves.LineTypes.SpiralLeft || _type == Curves.LineTypes.SpiralRight ||
            _type == Curves.LineTypes.SpringLeft || _type == Curves.LineTypes.SpringRight ||
            _type == Curves.LineTypes.SineLine)
        {

            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Amplitude");
            _amplitude = EditorGUILayout.IntSlider(lineEditor.Amplitude, 0, 50);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Wave power");
            _wavePower = EditorGUILayout.Slider(lineEditor.WavePower, 0f, 10f);
            GUILayout.EndHorizontal();
        }
        GUILayout.BeginHorizontal();
        _renderLine = EditorGUILayout.Toggle("RenderLine", lineEditor.RenderLine);
        _hideStart = EditorGUILayout.Toggle("Hide on Start", lineEditor.HideOnStart);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        _AutoRebuild = EditorGUILayout.Toggle("Rebuild every Х sec:", lineEditor.AutoRebuild);
        EditorGUI.BeginDisabledGroup(!_AutoRebuild);
        _RebuildFreq = EditorGUILayout.Slider(lineEditor.RebuildFreq, 0f, 2f);
        EditorGUI.EndDisabledGroup();

        GUILayout.EndHorizontal();


        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(target, "Properties were changed");
            lineEditor.LineType = _type;
            lineEditor.CutLine = _cuttable;
            lineEditor.DynamicCutting = _dynamicCut;
            lineEditor.detail = _detail;
            lineEditor.Noise = _noise;

            lineEditor.Amplitude = _amplitude;
            lineEditor.WavePower = _wavePower;

            lineEditor.RenderLine = _renderLine;
            lineEditor.HideOnStart = _hideStart;

            lineEditor.AutoRebuild = _AutoRebuild;
            lineEditor.RebuildFreq = _RebuildFreq;

            lineEditor.BuildNewLine();
        }


        if (GUILayout.Button("Rebuild"))
        {
            lineEditor.BuildNewLine();
        }
    
    }
    public void OnSceneGUI()
    {
        lineEditor = (CurvesUtility)target;
        Transform editorTransform = lineEditor.transform;
        Quaternion editorRotation = lineEditor.transform.rotation;

        Handles.DrawAAPolyLine(5f, lineEditor.GetOriginalDots);


        /*
        for (int i = 1; i< lineEditor.GetPath.Length; i++)
        {
            Handles.color = Color.cyan;
            Handles.DrawLine(lineEditor.GetPath[i - 1], lineEditor.GetPath[i]);
        }
        */


        if (lineEditor.startPointType == CurvesUtility.PointType.Vector3)
            handlePoint(ref lineEditor.startVect, editorTransform, editorRotation);
        if (lineEditor.endPointType == CurvesUtility.PointType.Vector3)
            handlePoint(ref lineEditor.endVect, editorTransform, editorRotation);


        if (_type == Curves.LineTypes.QuadraticBezier || _type == Curves.LineTypes.CubicBezier)
        {
            if (lineEditor.Control_1Type == CurvesUtility.PointType.Vector3)
                handlePoint(ref lineEditor.Control1Vect, editorTransform, editorRotation);
            if (_type == Curves.LineTypes.CubicBezier)
            {
                if (lineEditor.Control_2Type == CurvesUtility.PointType.Vector3)
                    handlePoint(ref lineEditor.Control2Vect, editorTransform, editorRotation);
            }
        }
    }

}



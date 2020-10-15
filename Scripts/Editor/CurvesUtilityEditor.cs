using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using UnityEditorInternal;

//TODO: REDO ALL! 

namespace Sushkov.LinesUtility
{
    [CustomEditor(typeof(CurvesUtility))]
    [CanEditMultipleObjects]
    public class CurvesUtilityEditor : Editor
    {
        private LineType _type;
        private bool _cuttable;
        private bool _dynamicCut;
        private LayerMask _layerMask;
        private List<int> layerNumbers = new List<int>();

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
        private QueryTriggerInteraction _cutByTriggers;
        CurvesUtility lineEditor;
        SerializedProperty m_RendererProp;


        private string[] PointOptions = Enum.GetNames(typeof(CurvesUtility.PointType));

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
            editorPointType =
                (CurvesUtility.PointType) GUILayout.SelectionGrid((int) currentPointType, PointOptions, 2);
            GUILayout.EndHorizontal();
            if (EditorGUI.EndChangeCheck())
            {
                if (currentPointType == CurvesUtility.PointType.Object &&
                    editorPointType == CurvesUtility.PointType.Vector3 && currentObj != null)
                    currentVector = lineEditor.transform.InverseTransformPoint(currentObj.position);
                currentPointType = editorPointType;
            }

            switch (currentPointType)
            {
                case (CurvesUtility.PointType.Object):
                {
                    EditorGUI.BeginChangeCheck();
                    editorObj = (Transform) EditorGUILayout.ObjectField(currentObj, typeof(Transform), true);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(target, "Changed the object");
                        if (editorObj is null)
                            return;

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


        private void HandlePoint(ref Vector3 originalPoint, Transform editorTransform)
        {
            Vector3 point = editorTransform.TransformPoint(originalPoint);
            Quaternion handleRotation = Tools.pivotRotation == PivotRotation.Local
                ? editorTransform.rotation
                : Quaternion.identity;
            Handles.color = Color.white;


            EditorGUI.BeginChangeCheck();
            point = Handles.DoPositionHandle(point, handleRotation);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject((CurvesUtility) target, "Move Point");
                originalPoint = editorTransform.InverseTransformPoint(point);
                lineEditor.BuildNewLine();
            }
        }

        void OnEnable()
        {
            m_RendererProp = serializedObject.FindProperty("lineRenderer");
        }

        private void OnSceneGUI()
        {
            if (lineEditor == null) return;
            if (lineEditor.transform.hasChanged)
            {
                lineEditor.BuildNewLine();
                lineEditor.transform.hasChanged = false;
            }
            if (lineEditor.startPointType == CurvesUtility.PointType.Vector3)
                HandlePoint(ref lineEditor.startLocal, lineEditor.transform);
            if (lineEditor.endPointType == CurvesUtility.PointType.Vector3)
                HandlePoint(ref lineEditor.endLocal, lineEditor.transform);
            switch (lineEditor.lineType)
            {
                case LineType.QuadraticBezier:
                    if (lineEditor.control1Type == CurvesUtility.PointType.Vector3)
                        HandlePoint(ref lineEditor.control1Local, lineEditor.transform);
                    break;
                case LineType.CubicBezier:
                    if (lineEditor.control1Type == CurvesUtility.PointType.Vector3)
                        HandlePoint(ref lineEditor.control1Local, lineEditor.transform);
                    if (lineEditor.control2Type == CurvesUtility.PointType.Vector3)
                        HandlePoint(ref lineEditor.control2Local, lineEditor.transform);
                    break;
            }
        }

        public override void OnInspectorGUI()
        {
            // base.OnInspectorGUI();
            EditorGUILayout.PropertyField(m_RendererProp, new GUIContent("Line renderer"));
            lineEditor = (CurvesUtility) target;

            GUILayout.Label("Lenght: " + lineEditor.lenght.ToString("F2"), EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();
            _type = (LineType) EditorGUILayout.EnumPopup("Type of the line::", lineEditor.lineType);

            _cuttable = EditorGUILayout.Toggle("End on collision:", lineEditor.cutLine);

            if (_cuttable)
            {
                _dynamicCut = EditorGUILayout.Toggle("Dynamic cutting:", lineEditor.dynamicCutting);
                _layerMask = LayerMaskField("Cutted by layers", lineEditor.cuttingMask);

                _cutByTriggers =
                    (QueryTriggerInteraction) EditorGUILayout.EnumPopup("Trigger interaction",
                        lineEditor.cutByTriggers);
            }

            _detail = EditorGUILayout.IntSlider("Dots: ", lineEditor.Detail, 2, 500);
            _noise = EditorGUILayout.Slider("Noise: ", lineEditor.noise, 0f, 10f);
            //DrawUILine(Color.gray);
            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// START-END POINTS
            {
                PointControl("Start", ref _startSelection, ref lineEditor.startPointType, ref _startObj,
                    ref lineEditor.startObj, ref _start, ref lineEditor.startLocal);
                PointControl("End", ref _endSelection, ref lineEditor.endPointType, ref _endObj, ref lineEditor.endObj,
                    ref _end, ref lineEditor.endLocal);
            }
            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// MANUAL DISTANCE
            {
                //DrawUILine(Color.gray,1);
                _direction = (lineEditor.endWorld - lineEditor.startWorld).normalized;

                GUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Distance: " + lineEditor.distance.ToString("F2"));

                if (lineEditor.endPointType == CurvesUtility.PointType.Object)
                {
                    EditorGUI.BeginChangeCheck();
                    _allowMoveObjects = EditorGUILayout.Toggle("Allow move objects", lineEditor.allowMoveObjects);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(target, "Allowed to move objects");
                        lineEditor.allowMoveObjects = _allowMoveObjects;
                    }
                }

                GUILayout.EndHorizontal();

                EditorGUI.BeginChangeCheck();

                EditorGUI.BeginDisabledGroup( lineEditor.endPointType == CurvesUtility.PointType.Object && (!lineEditor.endObj || !lineEditor.allowMoveObjects));
                _distance = EditorGUILayout.Slider(lineEditor.distance, 1f, 100f);
                EditorGUI.EndDisabledGroup();

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(target, "Allowed to move objects by setting distance");
                    
                    if (lineEditor.endPointType == CurvesUtility.PointType.Object)
                        lineEditor.endObj.position = lineEditor.startWorld + _direction * _distance;
                    else if (lineEditor.endPointType == CurvesUtility.PointType.Vector3)
                        lineEditor.endLocal = lineEditor.endLocal.normalized * _distance;

                    lineEditor.BuildNewLine();
                }

                DrawUILine(Color.gray);
            }
            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// BEZIER CONTROLS 
            if (_type == LineType.QuadraticBezier || _type == LineType.CubicBezier)
            {
                PointControl("Control #1", ref _CTRL_1Selection, ref lineEditor.control1Type, ref _control1,
                    ref lineEditor.controlObj1, ref _control1Vect, ref lineEditor.control1Local);
                if (_type == LineType.CubicBezier)
                {
                    PointControl("Control #2", ref _CTRL_2Selection, ref lineEditor.control2Type, ref _control2,
                        ref lineEditor.controlObj2, ref _control2Vect, ref lineEditor.control2Local);
                }
            }
            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// CURL CONTROL 

            if (_type == LineType.ConeLeft || _type == LineType.ConeRight ||
                _type == LineType.SpiralLeft || _type == LineType.SpiralRight ||
                _type == LineType.SpringLeft || _type == LineType.SpringRight ||
                _type == LineType.SineLine)
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Amplitude");
                _amplitude = EditorGUILayout.IntSlider(lineEditor.amplitude, 0, 50);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Wave power");
                _wavePower = EditorGUILayout.Slider(lineEditor.wavePower, 0f, 10f);
                GUILayout.EndHorizontal();
            }

            GUILayout.BeginHorizontal();
            _renderLine = EditorGUILayout.Toggle("RenderLine", lineEditor.useLineRenderer);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            _AutoRebuild = EditorGUILayout.Toggle("Rebuild every Х sec:", lineEditor.autoRebuild);
            EditorGUI.BeginDisabledGroup(!_AutoRebuild);
            _RebuildFreq = EditorGUILayout.Slider(lineEditor.rebuildTime, 0f, 2f);
            EditorGUI.EndDisabledGroup();

            GUILayout.EndHorizontal();


            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Properties were changed");

                lineEditor.lineType = _type;
                lineEditor.cutLine = _cuttable;
                lineEditor.dynamicCutting = _dynamicCut;
                lineEditor.cutByTriggers = _cutByTriggers;
                lineEditor.cuttingMask = _layerMask;
                lineEditor.Detail = _detail;
                lineEditor.noise = _noise;

                lineEditor.amplitude = _amplitude;
                lineEditor.wavePower = _wavePower;

                lineEditor.useLineRenderer = _renderLine;

                lineEditor.autoRebuild = _AutoRebuild;
                lineEditor.rebuildTime = _RebuildFreq;

                lineEditor.BuildNewLine();
            }


            if (GUILayout.Button("Rebuild"))
            {
                lineEditor.BuildNewLine();
            }

            serializedObject.ApplyModifiedProperties();
        }

        private LayerMask LayerMaskField(string label, LayerMask layerMask)
        {
            var layers = InternalEditorUtility.layers;

            layerNumbers.Clear();

            for (int i = 0; i < layers.Length; i++)
                layerNumbers.Add(LayerMask.NameToLayer(layers[i]));

            int maskWithoutEmpty = 0;
            for (int i = 0; i < layerNumbers.Count; i++)
            {
                if (((1 << layerNumbers[i]) & layerMask.value) > 0)
                    maskWithoutEmpty |= (1 << i);
            }

            maskWithoutEmpty = UnityEditor.EditorGUILayout.MaskField(label, maskWithoutEmpty, layers);

            var mask = 0;
            for (var i = 0; i < layerNumbers.Count; i++)
            {
                if ((maskWithoutEmpty & (1 << i)) > 0)
                    mask |= (1 << layerNumbers[i]);
            }

            layerMask.value = mask;

            return layerMask;
        }
    }
}
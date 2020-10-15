using System.Collections.Generic;
using UnityEngine;


namespace Sushkov.LinesUtility
{
    [ExecuteAlways]
    public class CurvesUtility : MonoBehaviour
    {
        public enum PointType
        {
            Object,
            Vector3
        }

        #region Fields

        public LineRenderer lineRenderer;
        public LineType lineType;
        public bool cutLine; // Should the line be cutted by raytraceble obstacles?
        public LayerMask cuttingMask; // Which layers may block the line?
        public QueryTriggerInteraction cutByTriggers; // Shall it be cutted by triggers?
        public bool dynamicCutting; // Should the line be cutted in rintime?
        public bool useLineRenderer; // True to use <LineRenderer> for visualization.
        public bool autoRebuild; // True to enable rebuilding every x sec   
        public float rebuildTime; // In case AutoRebuild - how long it takes to call rebuild method
        public int detail; // How many dots the line have
        public int amplitude; // How many "waves" in case the line is Sin-wave or curly (Spiral, spring, etc)
        public float wavePower; // How strong "waves" in case the line is Sin-wave or curly (Spiral, spring, etc)
        public float noise;

        // Actual values that goes for calculation. start / end / bezier control //  GLOBAL COORDINATES
        public Vector3 startWorld;
        public Vector3 endWorld;
        public Vector3 control1World;
        public Vector3 control2World;

        // Variables to store values of parameters. start / end / bezier control //  LOCAL COORDINATES
        public Vector3 startLocal = Vector3.zero;
        public Vector3 endLocal = -Vector3.up * 2f;
        public Vector3 control1Local = Vector3.zero;
        public Vector3 control2Local = Vector3.up * 2f;

        // Variables to store link in case GameObject is used as part of the line 
        public Transform startObj;
        public Transform endObj;
        public Transform controlObj1;
        public Transform controlObj2;

        public PointType startPointType;
        public PointType endPointType;
        public PointType control1Type = PointType.Vector3;
        public PointType control2Type = PointType.Vector3;

        public Vector3 direction;
        public float distance;
        public bool allowMoveObjects;
        public float lenght;

        private List<Vector3>
            _originalDots = new List<Vector3>(); // Actual storage of the generated dots - GLOBAL world

        private List<Vector3> _dotsInUse = new List<Vector3>(); // Store dots, which in use at the moment
        private Vector3[] _dotsArray;
        private RaycastHit _hit;
        private float _rebuildTimer;
        private Vector3 _orthogonal = new Vector3();

        #endregion


        #region Properties

        public List<Vector3> GetOriginalDots => _originalDots;
        public List<Vector3> GetDotsInuse => _dotsInUse;
        public bool IsCutted { get; private set; }

        public int Detail
        {
            get => detail;
            set
            {
                detail = value;
                if (detail <= _originalDots.Capacity) return;
                _originalDots.Capacity = detail;
                _dotsInUse.Capacity = detail;
                _dotsArray = new Vector3[detail];
            }
        }

        public Vector3 Direction
        {
            get => direction;
            set
            {
                if (!Mathf.Approximately(Vector3.Dot(direction, value), 1f))
                    _orthogonal = Curves.RandomOrthogonal(direction.normalized);
                direction = value;
            }
        }

        #endregion

        public CurvesUtility()
        {
            lineType = LineType.Linear;
            startLocal = Vector3.zero;
            endLocal = Vector3.up;
            rebuildTime = 0.1f;
            startObj = null;
            endObj = null;
            controlObj1 = null;
            controlObj2 = null;
            startPointType = PointType.Vector3;
            endPointType = PointType.Vector3;
            _dotsArray = new Vector3 [15];
            Detail = 15;
            amplitude = 2;
            wavePower = 2f;
            noise = 0f;
            useLineRenderer = true;
            direction = (endWorld - startWorld).normalized;
            distance = (endWorld - startWorld).magnitude;
        }


        #region UnityEvents

        private void Awake()
        {
            _dotsArray = new Vector3[detail];
            BuildNewLine();
        }


        private void Update()
        {
            if (!Application.isPlaying)
            {
                if (startPointType == PointType.Object && startObj && startObj.hasChanged)
                {
                    BuildNewLine();
                    startObj.hasChanged = false;
                }

                if (endPointType == PointType.Object && endObj && endObj.hasChanged)
                {
                    BuildNewLine();
                    endObj.hasChanged = false;
                }

                if (control1Type == PointType.Object && controlObj1 && controlObj1.hasChanged)
                {
                    BuildNewLine();
                    controlObj1.hasChanged = false;
                }

                if (control2Type == PointType.Object && controlObj2 && controlObj2.hasChanged)
                {
                    BuildNewLine();
                    controlObj2.hasChanged = false;
                }
            }
        }

        private void FixedUpdate()
        {
            if (!Application.isPlaying) return;
            if (!autoRebuild && (!cutLine || !dynamicCutting)) return;

            _rebuildTimer += Time.fixedDeltaTime;

            if (cutLine && dynamicCutting)
            {
                UpdateCutting();
                if (useLineRenderer)
                    UpdateLineRenderer(ref _dotsInUse, lineRenderer);
            }

            if (_rebuildTimer > rebuildTime)
            {
                _rebuildTimer = 0f;
                if (autoRebuild)
                    GetDots(ref _originalDots, lineType);
                if (useLineRenderer)
                    UpdateLineRenderer(ref _dotsInUse, lineRenderer);
            }
        }

        #endregion


        #region Methods

        public float GetLenghtOf(ref List<Vector3> path)
        {
            return Curves.GetLenght(ref path);
        }

        public void BuildNewLine()
        {
            GetDots(ref _originalDots, lineType);
            if (useLineRenderer)
                UpdateLineRenderer(ref _dotsInUse, lineRenderer);
        }

        private void GetDots(ref List<Vector3> points, LineType newLineType)
        {
            if (startPointType == PointType.Object && startObj)
                startWorld = startObj.position;
            else
                startWorld = transform.position + transform.TransformVector(startLocal);
            if (endPointType == PointType.Object && endObj)
                endWorld = endObj.position;
            else endWorld = transform.position + transform.TransformVector(endLocal);
            if (control1Type == PointType.Object && controlObj1)
                control1World = controlObj1.position;
            else control1World = transform.position + transform.TransformVector(control1Local);
            if (control2Type == PointType.Object && controlObj2)
                control2World = controlObj2.position;
            else control2World = transform.position + transform.TransformVector(control2Local);
            Direction = (endWorld - startWorld).normalized;
            distance = (endWorld - startWorld).magnitude;
            switch (newLineType)
            {
                case LineType.Linear:
                    Curves.GetSimpleLineDots(ref points, startWorld, endWorld, Detail, noise);
                    break;
                case LineType.QuadraticBezier:
                    Curves.GetQuadraticBezierDots(ref points, startWorld, endWorld, control1World, Detail, noise);
                    break;
                case LineType.CubicBezier:
                    Curves.GetCubicBezierDots(ref points, startWorld, endWorld, control1World, control2World, Detail,
                        noise);
                    break;
                case LineType.SineLine:
                    Curves.GetSineLine(ref points, startWorld, endWorld, Detail, amplitude, wavePower, noise,
                        ref _orthogonal);
                    break;
                case LineType.SpiralRight:
                    Curves.GetSpiralRight(ref points, startWorld, endWorld, Detail, amplitude, wavePower, noise,
                        ref _orthogonal);
                    break;
                case LineType.SpiralLeft:
                    Curves.GetSpiralLeft(ref points, startWorld, endWorld, Detail, amplitude, wavePower, noise,
                        ref _orthogonal);
                    break;
                case LineType.SpringRight:
                    Curves.GetSpringRight(ref points, startWorld, endWorld, Detail, amplitude, wavePower, noise,
                        ref _orthogonal);
                    break;
                case LineType.SpringLeft:
                    Curves.GetSpringLeft(ref points, startWorld, endWorld, Detail, amplitude, wavePower, noise,
                        ref _orthogonal);
                    break;
                case LineType.ConeRight:
                    Curves.GetConeRight(ref points, startWorld, endWorld, Detail, amplitude, wavePower, noise,
                        ref _orthogonal);
                    break;
                case LineType.ConeLeft:
                    Curves.GetConeLeft(ref points, startWorld, endWorld, Detail, amplitude, wavePower, noise,
                        ref _orthogonal);
                    break;
            }

            if (cutLine)
                LineCutter(ref _originalDots, ref _dotsInUse);
            else

            {
                _dotsInUse.Clear();
                foreach (var point in _originalDots)
                {
                    _dotsInUse.Add(point);
                }
            }

            lenght = GetLenghtOf(ref _dotsInUse);
        }

        private void LineCutter(ref List<Vector3> originalLine, ref List<Vector3> cuttedLine)
        {
            cuttedLine.Clear();
            Curves.CheckCapacity(ref cuttedLine, originalLine.Count);
            cuttedLine.Add(originalLine[0]);
            for (var i = 1; i < originalLine.Count; i++)
            {
                if (Physics.Linecast(originalLine[i - 1], originalLine[i], out _hit, cuttingMask, cutByTriggers))
                {
                    cuttedLine.Add(_hit.point);
                    IsCutted = true;
                    return;
                }

                cuttedLine.Add(originalLine[i]);
            }

            IsCutted = false;
        }

        private void UpdateCutting()
        {
            LineCutter(ref _originalDots, ref _dotsInUse);
        }

        private void UpdateLineRenderer(ref List<Vector3> dots, LineRenderer lineRenderer)
        {
            if (!lineRenderer || dots is null)
                return;
            if (_dotsArray.Length < dots.Capacity)
                _dotsArray = new Vector3[dots.Capacity];
            for (var i = 0;
                i < dots.Count;
                i++)
                _dotsArray[i] = dots[i];
            lineRenderer.positionCount = dots.Count;
            lineRenderer.SetPositions(_dotsArray);
        }

        #endregion

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            if (startPointType == PointType.Vector3)
                Gizmos.DrawSphere(startWorld, 0.5f);
            if (endPointType == PointType.Vector3)
                Gizmos.DrawSphere(endWorld, 0.5f);
            Gizmos.color = Color.blue;

            switch (lineType)
            {
                case LineType.QuadraticBezier:
                    if (control1Type == PointType.Vector3)
                        Gizmos.DrawSphere(control1World, 0.5f);
                    break;
                case LineType.CubicBezier:
                    if (control1Type == PointType.Vector3)
                        Gizmos.DrawSphere(control1World, 0.5f);
                    if (control2Type == PointType.Vector3)
                        Gizmos.DrawSphere(control2World, 0.5f);
                    break;
            }
        }
    }
}
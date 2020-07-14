using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[ExecuteInEditMode]
public class CurvesUtility : MonoBehaviour
{
    private Vector3[] originalDots;                     // Actual storage of the generated dots. // GLOBAL COORDINATES
    private Vector3[] dotsInUse;                        // Store dots, which in use at the moment
    public Vector3[] GetOriginalDots { get => originalDots; }
    public Vector3[] GetDotsInuse { get => dotsInUse; }
    public bool IsCutted
    {
        get
        {
            return originalDots.Length > dotsInUse.Length ? true : false; }
    }
    public int DesiredDotsNumber { get => originalDots.Length + 1; }

    public Curves.LineTypes LineType;
    public bool CutLine;                 // Should the line be cutted by raytraceble obstacles?
    public bool DynamicCutting;          // Should the line be cutted by raytraceble obstacles?

    public bool RenderLine;             // True to use <LineRenderer> for visualization.
    public bool HideOnStart;            // True to disable <LineRenderer> on "Play" in case 
    public bool AutoRebuild;            // 
    public float RebuildFreq;    // In case AutoRebuild - how long it takes to call rebuild method

    // Actual values that goes for calculation. start / end / bezier control //  GLOBAL COORDINATES
    public Vector3 start;
    public Vector3 end;
    public Vector3 Control1;
    public Vector3 Control2;
    // Variables to store values of parameters. start / end / bezier control //  LOCAL COORDINATES
    public Vector3 startVect;
    public Vector3 endVect;
    public Vector3 Control1Vect;
    public Vector3 Control2Vect;
    // Variables to store link in case GameObject is used as part of the line 
    public Transform startObj;
    public Transform endObj;
    public Transform Control_1Obj;
    public Transform Control_2Obj;

    // Which way is used to operate line
    public enum PointType : int { Object = 0, Vector3 = 1 };
    public PointType startPointType;
    public PointType endPointType;
    public PointType Control_1Type;
    public PointType Control_2Type;

    // Parameters:
    public int detail;          // How many dots the line have
    public int Amplitude;       // How many "waves" in case the line is Sin-wave or curly (Spiral, spring, etc)
    public float WavePower;     // How strong "waves" in case the line is Sin-wave or curly (Spiral, spring, etc)
    public float Noise;

    LineRenderer lineRenderer;
    public Vector3 direction;
    public float distance;
    public bool allowMoveObjects;
    public float lenght;

    public List<GameObject> fellows;

    private Coroutine rebuildCoroutine;

    public CurvesUtility()
    {
        LineType = Curves.LineTypes.Linear;
        startVect = Vector3.zero;
        endVect = Vector3.up;
        RebuildFreq = 0.1f;
        startObj = null;
        endObj = null;
        Control_1Obj = null;
        Control_2Obj = null;
        startPointType = PointType.Vector3;
        endPointType = PointType.Vector3;
        detail = 15;
        Amplitude = 2;
        WavePower = 2f;
        Noise = 0f;
        RenderLine = true;
        HideOnStart = true;
        direction = (end - start).normalized;
        distance = (end - start).magnitude;
        fellows = new List<GameObject>();
    }

    public Vector3[] GetDots(Curves.LineTypes Type)
    {
        if ((startPointType == PointType.Object) && (startObj != null)) start = startObj.position;
        else if (startPointType == PointType.Vector3)
            start = startVect + transform.position;

        if ((endPointType == PointType.Object) && (endObj != null)) end = endObj.position;
        else if (endPointType == PointType.Vector3)
            end = endVect + transform.position;

        if ((Control_1Type == PointType.Object) && (Control_1Obj != null)) Control1 = Control_1Obj.position;
        else if (Control_1Type == PointType.Vector3)
            Control1 = Control1Vect + transform.position;

        if ((Control_2Type == PointType.Object) && (Control_2Obj != null)) Control2 = Control_2Obj.position;
        else if (Control_2Type == PointType.Vector3)
            Control2 = Control2Vect + transform.position;

        direction = (end - start).normalized;
        distance = (end - start).magnitude;

        switch (Type)
        {
            case (Curves.LineTypes.Linear):
                {
                    originalDots = Curves.GetSimpleLineDots(start, end, detail, Noise);
                    break;
                }
            case (Curves.LineTypes.QuadraticBezier):
                {
                    originalDots = Curves.GetQuadraticBezierDots(start, end, Control1, detail, Noise);
                    break;
                }
            case (Curves.LineTypes.CubicBezier):
                {
                    originalDots = Curves.GetCubicBezierDots(start, end, Control1, Control2, detail, Noise);
                    break;
                }
            case (Curves.LineTypes.SineLine):
                {
                    originalDots = Curves.GetSineLine(start, end, detail, Amplitude, WavePower, Noise);
                    break;
                }
            case (Curves.LineTypes.SpiralRight):
                {

                    originalDots = Curves.GetSpiralRight(start, end, detail, Amplitude, WavePower, Noise);
                    break;
                }
            case (Curves.LineTypes.SpiralLeft):
                {

                    originalDots = Curves.GetSpiralLeft(start, end, detail, Amplitude, WavePower, Noise);
                    break;
                }
            case (Curves.LineTypes.SpringRight):
                {
                    originalDots = Curves.GetSpringRight(start, end, detail, Amplitude, WavePower, Noise);
                    break;
                }
            case (Curves.LineTypes.SpringLeft):
                {
                    originalDots = Curves.GetSpringLeft(start, end, detail, Amplitude, WavePower, Noise);
                    break;
                }
            case (Curves.LineTypes.ConeRight):
                {
                    originalDots = Curves.GetConeRight(start, end, detail, Amplitude, WavePower, Noise);
                    break;
                }
            case (Curves.LineTypes.ConeLeft):
                {
                    originalDots = Curves.GetConeLeft(start, end, detail, Amplitude, WavePower, Noise);
                    break;
                }

        }
        if (CutLine)
            dotsInUse = cutter(originalDots);
        lenght = GetLenghtOf(originalDots);
        return originalDots;
    }
    public float GetLenghtOf(Vector3[] path)
    {
        return Curves.GetLenght(path);
    }
    private void previewTool()
    {
        originalDots = GetDots(LineType);
        if (GetComponent<LineRenderer>() == null) { gameObject.AddComponent<LineRenderer>(); }
        if (lineRenderer == null) { lineRenderer = GetComponent<LineRenderer>(); }

        lineRenderer.positionCount = originalDots.Length;
        lineRenderer.SetPositions(originalDots);
        if (HideOnStart && Application.isPlaying)
        {
            lineRenderer.enabled = false;
        }
    }
    // Update is called once per frame
    private void Awake()
    {
        BuildNewLine();
    }

    private IEnumerator rebuildUpdate()
    {
        BuildNewLine();
        yield return new WaitForSecondsRealtime(RebuildFreq);
        rebuildCoroutine = null;
    }

    private GameObject recursiveParentFinder(GameObject obj)
    {
        return (obj.transform.parent == null) ? obj : recursiveParentFinder(obj.transform.parent.gameObject);
    }

    void Update()
    {

        if (AutoRebuild)
        {
            if (rebuildCoroutine == null)
            {
                rebuildCoroutine = StartCoroutine(rebuildUpdate());
            }
        }
        else if (!AutoRebuild && rebuildCoroutine != null)
        {
            StopCoroutine(rebuildCoroutine);
            rebuildCoroutine = null;
        }
        if (CutLine && DynamicCutting)
        {
            UpdateCutting();
            UpdateLineRenderer(dotsInUse, lineRenderer);
        }


    }
    // Method to "cut" the line. !!! IMPORTANT!!! : Obstacle should be raytraceble
    private Vector3[] cutter(Vector3[] original)
    {
        RaycastHit hit;
        for (int i = 1; i < original.Length; i++)
        {
            Debug.DrawLine(original[i - 1], original[i], Color.red);
            if (Physics.Linecast(original[i - 1], original[i], out hit))
            {
                //Debug.Log((recursiveParentFinder(hit.collider.gameObject)));
                Vector3[] cutted = new Vector3[i + 1];
                cutted[0] = original[0];
                for (int k = 1; k < cutted.Length; k++)
                {
                    cutted[k] = original[k];
                }
                cutted[i] = hit.point;
                //From last point before collision - to collisio point;
                return cutted;
            }
        }
        return original;
    }


    public void UpdateCutting()
    {
        dotsInUse = cutter(originalDots);
    }
    public void UpdateLineRenderer(Vector3[] Dots, LineRenderer LineRenderer)
    {
        if (LineRenderer != null && Dots != null)
        {
            LineRenderer.positionCount = Dots.Length;
            LineRenderer.SetPositions(Dots);
        }
    }


    public void BuildNewLine()
    {
        originalDots = GetDots(LineType);
        if (CutLine)
        { dotsInUse = cutter(originalDots); }
        else
        { dotsInUse = originalDots; }
        if (RenderLine)
        {
            if (GetComponent<LineRenderer>() == null) { gameObject.AddComponent<LineRenderer>(); }
            if (lineRenderer == null) { lineRenderer = GetComponent<LineRenderer>(); }

            UpdateLineRenderer(dotsInUse, lineRenderer);

            if (HideOnStart && Application.isPlaying)
            {
                lineRenderer.enabled = false;
            }
        }

    }


   
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(start, 1f);
        Gizmos.DrawSphere(end, 1f);
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(Control1, 1f);
        Gizmos.DrawSphere(Control2, 1f);
    }
   
}

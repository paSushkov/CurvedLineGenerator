using UnityEngine;

/// <summary>
/// Library of methods for calculating points
/// </summary>
public static class Curves
{
    public enum LineTypes { Linear, QuadraticBezier, CubicBezier, SpiralRight, SpiralLeft, SpringRight, SpringLeft, ConeRight, ConeLeft, SineLine}

    // Methods for position calculation of every point of line
    private static Vector3 LinearBezierPoint(float t, Vector3 start, Vector3 end)
    {
        float _t = 1f - t;
        return _t * start + t * end;
    }
    private static Vector3 QuadraticBezierPoint(float t, Vector3 start, Vector3 end, Vector3 control1)
    {
        float _t = 1f - t;
        return Mathf.Pow(_t, 2) * start + 2 * t * _t * control1 + Mathf.Pow(t, 2) * end;
    }
    private static Vector3 CubeBezierPoint(float t, Vector3 start, Vector3 end, Vector3 control1, Vector3 control2)
    {
        float _t = 1f - t;
        Vector3 result;
        result = Mathf.Pow(_t, 3) * start;
        result += 3 * t * Mathf.Pow(_t, 2) * control1;
        result += 3 * t * t * _t * control2;
        result += Mathf.Pow(t, 3) * end;
        return result;
    }

    //Returns the array of positions for every point on requested amount
    public static Vector3[] GetSimpleLineDots(Vector3 StartPoint, Vector3 EndPoint, int amount, float Noise)
    {
        Vector3[] result = new Vector3[amount];
        result[0] = StartPoint;
        result[result.Length - 1] = EndPoint;
        for (int i = 1; i < result.Length - 1; i++)
        {
            float t = i / (float)(result.Length - 1);
            result[i] = LinearBezierPoint(t, StartPoint, EndPoint);
        }
        if (Noise > 0) { result = AddNoise(result, StartPoint, EndPoint, Noise); }
        return result;
    }
    public static Vector3[] GetQuadraticBezierDots(Vector3 StartPoint, Vector3 EndPoint, Vector3 ControlPoint, int amount, float Noise)
    {
        Vector3[] result = new Vector3[amount];
        result[0] = StartPoint;
        result[result.Length - 1] = EndPoint;
        for (int i = 1; i < result.Length - 1; i++)
        {
            float t = i / (float)(result.Length - 1);
            result[i] = QuadraticBezierPoint(t, StartPoint, EndPoint, ControlPoint);
        }
        if (Noise > 0) { result = AddNoise(result, StartPoint, EndPoint, Noise); }
        return result;
    }
    public static Vector3[] GetCubicBezierDots(Vector3 StartPoint, Vector3 EndPoint, Vector3 ControlPoint1, Vector3 ControlPoint2, int amount, float Noise)
    {
        Vector3[] result = new Vector3[amount];
        result[0] = StartPoint;
        result[result.Length - 1] = EndPoint;
        for (int i = 1; i < result.Length - 1; i++)
        {
            float t = i / (float)(result.Length - 1);
            result[i] = CubeBezierPoint(t, StartPoint, EndPoint, ControlPoint1, ControlPoint2);
        }
        if (Noise > 0) { result = AddNoise(result, StartPoint, EndPoint, Noise); }
        return result;
    }
    public static Vector3[] GetSineLine(Vector3 StartPoint, Vector3 EndPoint, int amount, int amplitude, float WavePower, float Noise) 
    {
        Vector3[] result = new Vector3[amount];
        float step = (EndPoint - StartPoint).magnitude / (amount - 1);

        Vector3 direction = (EndPoint - StartPoint).normalized;
        Vector3 orthogonal = RandomOrthogonal(direction).normalized;
        
        for (int i = 0; i < result.Length; i++)
        {
            result[i] = StartPoint + direction * step*i;
            result[i] += orthogonal * Mathf.Sin(amplitude * Mathf.PI * (i / (float)(amount - 1))) * WavePower;
        }
        if (Noise > 0) { result = AddNoise(result, StartPoint, EndPoint, Noise); }
        return result;
    }
    public static Vector3[] GetSpiralRight(Vector3 StartPoint, Vector3 EndPoint, int amount, int amplitude, float Radius, float Noise)
    {
        Vector3[] result = new Vector3[amount];
        Vector3 direction = (EndPoint - StartPoint).normalized;
        Vector3 orthogonal = RandomOrthogonal(direction).normalized;
        Vector3 orthogonal1 = Vector3.Cross(direction, orthogonal);
        float step = (EndPoint - StartPoint).magnitude / (float)(amount - 1);
        for (int i = 0; i < result.Length; i++)
        {
            Vector3 add = orthogonal * Mathf.Sin(amplitude * Mathf.PI * (i / (float)(amount - 1)));
            add += orthogonal1 * Mathf.Cos(amplitude * Mathf.PI * (i / (float)(amount - 1)));
            result[i] = StartPoint + direction * step * i + add * Radius;
        }
        if (Noise > 0) { result = AddNoise(result, StartPoint, EndPoint, Noise); }
        return result;
    }
    public static Vector3[] GetSpiralLeft(Vector3 StartPoint, Vector3 EndPoint, int amount, int amplitude, float Radius, float Noise)
    {
        Vector3[] result = new Vector3[amount];
        result[0] = StartPoint;
        Vector3 direction = (EndPoint - StartPoint).normalized;
        Vector3 orthogonal = RandomOrthogonal(direction).normalized;
        Vector3 orthogonal1 = Vector3.Cross(direction, orthogonal);
        float step = (EndPoint - StartPoint).magnitude / (float)(amount - 1);
        for (int i = 0; i < result.Length; i++)
        {
            Vector3 add = orthogonal * Mathf.Sin(amplitude * Mathf.PI * (i / (float)(amount - 1)));
            add -= orthogonal1 * Mathf.Cos(amplitude * Mathf.PI * (i / (float)(amount - 1)));
            result[i] = StartPoint + direction * step * i + add * Radius;
        }
        if (Noise > 0) { result = AddNoise(result, StartPoint, EndPoint, Noise); }
        return result;
    }
    public static Vector3[] GetSpringRight(Vector3 StartPoint, Vector3 EndPoint, int amount, int amplitude, float Radius, float Noise)
    {
        Vector3[] result = new Vector3[amount];
        Vector3 direction = (EndPoint - StartPoint).normalized;
        Vector3 orthogonal = RandomOrthogonal(direction).normalized;
        Vector3 orthogonal1 = Vector3.Cross(direction, orthogonal);
        float step = (EndPoint - StartPoint).magnitude / (float)(amount - 1);
        float currentRadius = 0f;
        for (int i = 0; i < result.Length; i++)
        {
            Vector3 add = orthogonal * Mathf.Sin(amplitude * Mathf.PI * (i / (float)(amount - 1)));
            add += orthogonal1 * Mathf.Cos(amplitude * Mathf.PI * (i / (float)(amount - 1)));
            result[i] = StartPoint + direction * step * i + add * currentRadius;

            if (i < result.Length / 2)
            { currentRadius += 2* Radius / ((float)(amount - 1)); }
            else
            {
                currentRadius -= 2*Radius / ((float)(amount - 1));
                currentRadius = Mathf.Clamp(currentRadius, 0f, Radius);
            }
        }
        if (Noise > 0) { result = AddNoise(result, StartPoint, EndPoint, Noise); }
        return result;
    }
    public static Vector3[] GetSpringLeft(Vector3 StartPoint, Vector3 EndPoint, int amount, int amplitude, float Radius, float Noise)
    {
        Vector3[] result = new Vector3[amount];
        Vector3 direction = (EndPoint - StartPoint).normalized;
        Vector3 orthogonal = RandomOrthogonal(direction).normalized;
        Vector3 orthogonal1 = Vector3.Cross(direction, orthogonal);
        float step = (EndPoint - StartPoint).magnitude / (float)(amount - 1);
        float currentRadius = 0f;
        for (int i = 0; i < result.Length; i++)
        {
            Vector3 add = orthogonal * Mathf.Sin(amplitude * Mathf.PI * (i / (float)(amount - 1)));
            add -= orthogonal1 * Mathf.Cos(amplitude * Mathf.PI * (i / (float)(amount - 1)));
            result[i] = StartPoint + direction * step * i + add * currentRadius;

            if (i < result.Length / 2)
            { currentRadius += 2*Radius / ((float)(amount - 1)); }
            else
            {
                currentRadius -= 2*Radius / ((float)(amount - 1));
                currentRadius = Mathf.Clamp(currentRadius, 0f, Radius);
            }
        }
        if (Noise > 0) { result = AddNoise(result, StartPoint, EndPoint, Noise); }
        return result;
    }
    public static Vector3[] GetConeRight(Vector3 StartPoint, Vector3 EndPoint, int amount, int amplitude, float Radius, float Noise)
    {

        Vector3[] result = new Vector3[amount];
        Vector3 direction = (EndPoint - StartPoint).normalized;
        Vector3 orthogonal = RandomOrthogonal(direction).normalized;
        Vector3 orthogonal1 = Vector3.Cross(direction, orthogonal);
        float step = (EndPoint - StartPoint).magnitude / (float)(amount - 1);
        float currentRadius = 0f;
        for (int i = 0; i < result.Length; i++)
        {
            Vector3 add = orthogonal * Mathf.Sin(amplitude * Mathf.PI * (i / (float)(amount - 1)));
            add += orthogonal1 * Mathf.Cos(amplitude * Mathf.PI * (i / (float)(amount - 1)));
            result[i] = StartPoint + direction * step * i + add * currentRadius;
            currentRadius = 2*Radius / amount * i;
        }
        if (Noise > 0) { result = AddNoise(result, StartPoint, EndPoint, Noise); }
        return result;
    }
    public static Vector3[] GetConeLeft(Vector3 StartPoint, Vector3 EndPoint, int amount, int amplitude, float Radius, float Noise)
    {
        Vector3[] result = new Vector3[amount];
        Vector3 direction = (EndPoint - StartPoint).normalized;
        Vector3 orthogonal = RandomOrthogonal(direction).normalized;
        Vector3 orthogonal1 = Vector3.Cross(direction, orthogonal);
        float step = (EndPoint - StartPoint).magnitude / (float)(amount - 1);
        float currentRadius = 0f;
        for (int i = 0; i < result.Length; i++)
        {
            Vector3 add = orthogonal * Mathf.Sin(amplitude * Mathf.PI * (i / (float)(amount - 1)));
            add -= orthogonal1 * Mathf.Cos(amplitude * Mathf.PI * (i / (float)(amount - 1)));
            result[i] = StartPoint + direction * step * i + add * currentRadius;
            currentRadius = Radius / amount * i*2;
        }
        if (Noise > 0) { result = AddNoise(result, StartPoint, EndPoint, Noise); }
        return result;
    }
        
    // INTERNAL TOOLS
    // Checks every single 
    
    // Calls an overload of itself
    private static Vector3[] AddNoise(Vector3[] LineArray, Vector3 StartPoint, Vector3 EndPoint, float NoisePower)
    {
        Vector3 direction = EndPoint - StartPoint;
        return AddNoise(LineArray, direction, NoisePower);
    }
    // Adding noise in 2 orthogonal directions
    private static Vector3[] AddNoise(Vector3[] LineArray, Vector3 Direction, float NoisePower)
    {
        Vector3 orthogonal1 = RandomOrthogonal(Direction).normalized;
        Vector3 orthogonal2 = Vector3.Cross(Direction, orthogonal1).normalized;
        for (int i = 1; i < LineArray.Length-1; i++)
        {
            LineArray[i] += orthogonal1 * Random.Range(-NoisePower, NoisePower);
            LineArray[i] += orthogonal2 * Random.Range(-NoisePower, NoisePower);
        }


        return LineArray;
    }
    // Returns a random orthogonal Vector3
    private static Vector3 RandomOrthogonal(Vector3 v)
    {
        Vector3 vPerpendicular = Vector3.one;
        if (v.x != 0)
        {
            vPerpendicular.x = -(v.y * vPerpendicular.y + v.z * vPerpendicular.z) / v.x;
        }
        else
        {
            vPerpendicular.x = Random.Range(0.0f, 1.0f);
        }
        if (v.y != 0)
        {
            vPerpendicular.y = -(v.x * vPerpendicular.x + v.z * vPerpendicular.z) / v.y;
        }
        else
        {
            vPerpendicular.y = Random.Range(0.0f, 1.0f);
        }
        if (v.z != 0)
        {
            vPerpendicular.z = -(v.y * vPerpendicular.y + v.x * vPerpendicular.x) / v.z;
        }
        else
        {
            vPerpendicular.z = Random.Range(0.0f, 1.0f);
        }
        return vPerpendicular;

    }
    // Returns summary magnitude between all Vectror[]3 elements in straight order
    public static float GetLenght(Vector3[] path)
    {
        float lenght = 0f;
        if (path!= null && path.Length > 1)
        {
            for (int i = 1; i < path.Length; i++)
            {
                lenght += (path[i] - path[i - 1]).magnitude;
            }
        }
        return lenght;
    }


}
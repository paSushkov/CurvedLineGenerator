using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Sushkov.LinesUtility
{
    public static class Curves
    {
        #region Line calculations

        public static void GetSimpleLineDots(ref List<Vector3> points, Vector3 startPoint, Vector3 endPoint, int amount,
            float noise)
        {
            points.Clear();
            CheckCapacity(ref points, amount);
            points.Add(startPoint);
            for (var i = 1; i < amount - 1; i++)
            {
                var t = i / (float) (amount - 1);
                points.Add(LinearBezierPoint(t, startPoint, endPoint));
            }

            points.Add(endPoint);

            if (noise > 0)
                AddNoise(ref points, startPoint, endPoint, noise);
        }

        public static void GetQuadraticBezierDots(ref List<Vector3> points, Vector3 startPoint, Vector3 endPoint,
            Vector3 controlPoint,
            int amount, float noise)
        {
            points.Clear();
            CheckCapacity(ref points, amount);

            points.Add(startPoint);
            for (var i = 1; i < amount - 1; i++)
            {
                var t = i / (float) (amount - 1);
                points.Add(QuadraticBezierPoint(t, startPoint, endPoint, controlPoint));
            }

            points.Add(endPoint);

            if (noise > 0)
                AddNoise(ref points, startPoint, endPoint, noise);
        }

        public static void GetCubicBezierDots(ref List<Vector3> points, Vector3 startPoint, Vector3 endPoint,
            Vector3 controlPoint1, Vector3 controlPoint2, int amount, float noise)
        {
            points.Clear();
            CheckCapacity(ref points, amount);
            points.Add(startPoint);
            for (var i = 1; i < amount - 1; i++)
            {
                var t = i / (float) (amount - 1);
                points.Add(CubeBezierPoint(t, startPoint, endPoint, controlPoint1, controlPoint2));
            }

            points.Add(endPoint);

            if (noise > 0)
                AddNoise(ref points, startPoint, endPoint, noise);
        }

        public static void GetSineLine(ref List<Vector3> points, Vector3 startPoint, Vector3 endPoint, int amount,
            int amplitude, float wavePower, float noise, ref Vector3 orthogonal)
        {
            points.Clear();
            CheckCapacity(ref points, amount);

            var step = (endPoint - startPoint).magnitude / (amount - 1);
            var direction = (endPoint - startPoint).normalized;
            orthogonal = VectorCompare(orthogonal, Vector3.zero) ? RandomOrthogonal(direction).normalized : orthogonal;

            for (var i = 0; i < amount; i++)
            {
                points.Add(startPoint + direction * (step * i));
                points[i] += orthogonal * (Mathf.Sin(amplitude * Mathf.PI * (i / (float) (amount - 1))) * wavePower);
            }

            if (noise > 0)
                AddNoise(ref points, startPoint, endPoint, noise);
        }

        public static void GetSpiralRight(ref List<Vector3> points, Vector3 startPoint, Vector3 endPoint, int amount,
            int amplitude, float radius, float noise, ref Vector3 orthogonal)
        {
            points.Clear();
            CheckCapacity(ref points, amount);
            var direction = (endPoint - startPoint).normalized;
            orthogonal = VectorCompare(orthogonal, Vector3.zero) ? RandomOrthogonal(direction).normalized : orthogonal;
            var orthogonal1 = Vector3.Cross(direction, orthogonal);
            var step = (endPoint - startPoint).magnitude / (amount - 1);
            for (var i = 0; i < amount; i++)
            {
                var add = orthogonal * Mathf.Sin(amplitude * Mathf.PI * (i / (float) (amount - 1)));
                add += orthogonal1 * Mathf.Cos(amplitude * Mathf.PI * (i / (float) (amount - 1)));
                points.Add(startPoint + direction * (step * i) + add * radius);
            }

            if (noise > 0)
                AddNoise(ref points, startPoint, endPoint, noise);
        }

        public static void GetSpiralLeft(ref List<Vector3> points, Vector3 startPoint, Vector3 endPoint, int amount,
            int amplitude, float radius, float noise, ref Vector3 orthogonal)
        {
            points.Clear();
            CheckCapacity(ref points, amount);
            //points.Add(startPoint);
            var direction = (endPoint - startPoint).normalized;
            orthogonal = VectorCompare(orthogonal, Vector3.zero) ? RandomOrthogonal(direction).normalized : orthogonal;
            var orthogonal1 = Vector3.Cross(direction, orthogonal);
            var step = (endPoint - startPoint).magnitude / (amount - 1);
            for (var i = 0; i < amount; i++)
            {
                var add = orthogonal * Mathf.Sin(amplitude * Mathf.PI * (i / (float) (amount - 1)));
                add -= orthogonal1 * Mathf.Cos(amplitude * Mathf.PI * (i / (float) (amount - 1)));
                points.Add(startPoint + direction * (step * i) + add * radius);
            }

            if (noise > 0)
                AddNoise(ref points, startPoint, endPoint, noise);
        }

        public static void GetSpringRight(ref List<Vector3> points, Vector3 startPoint, Vector3 endPoint, int amount,
            int amplitude, float radius, float noise, ref Vector3 orthogonal)
        {
            points.Clear();
            CheckCapacity(ref points, amount);

            var direction = (endPoint - startPoint).normalized;
            orthogonal = VectorCompare(orthogonal, Vector3.zero) ? RandomOrthogonal(direction).normalized : orthogonal;
            var orthogonal1 = Vector3.Cross(direction, orthogonal);
            var step = (endPoint - startPoint).magnitude / (amount - 1);
            var currentRadius = 0f;
            for (var i = 0; i < amount; i++)
            {
                var add = orthogonal * Mathf.Sin(amplitude * Mathf.PI * (i / (float) (amount - 1)));
                add += orthogonal1 * Mathf.Cos(amplitude * Mathf.PI * (i / (float) (amount - 1)));

                points.Add(startPoint + direction * (step * i) + add * currentRadius);

                if (i < amount / 2)
                    currentRadius += 2 * radius / (amount - 1);
                else
                {
                    currentRadius -= 2 * radius / (amount - 1);
                    currentRadius = Mathf.Clamp(currentRadius, 0f, radius);
                }
            }

            if (noise > 0)
                AddNoise(ref points, startPoint, endPoint, noise);
        }

        public static void GetSpringLeft(ref List<Vector3> points, Vector3 startPoint, Vector3 endPoint, int amount,
            int amplitude, float radius, float noise, ref Vector3 orthogonal)
        {
            points.Clear();
            CheckCapacity(ref points, amount);

            var direction = (endPoint - startPoint).normalized;
            orthogonal = VectorCompare(orthogonal, Vector3.zero) ? RandomOrthogonal(direction).normalized : orthogonal;
            var orthogonal1 = Vector3.Cross(direction, orthogonal);
            var step = (endPoint - startPoint).magnitude / (amount - 1);
            var currentRadius = 0f;
            for (var i = 0; i < amount; i++)
            {
                var add = orthogonal * Mathf.Sin(amplitude * Mathf.PI * (i / (float) (amount - 1)));
                add -= orthogonal1 * Mathf.Cos(amplitude * Mathf.PI * (i / (float) (amount - 1)));
                points.Add(startPoint + direction * (step * i) + add * currentRadius);

                if (i < amount / 2f)
                    currentRadius += 2f * radius / (amount - 1);
                else
                {
                    currentRadius -= 2f * radius / (amount - 1);
                    currentRadius = Mathf.Clamp(currentRadius, 0f, radius);
                }
            }

            if (noise > 0)
                AddNoise(ref points, startPoint, endPoint, noise);
        }

        public static void GetConeRight(ref List<Vector3> points, Vector3 startPoint, Vector3 endPoint, int amount,
            int amplitude, float radius, float noise, ref Vector3 orthogonal)
        {
            points.Clear();
            CheckCapacity(ref points, amount);

            var direction = (endPoint - startPoint).normalized;
            orthogonal = VectorCompare(orthogonal, Vector3.zero) ? RandomOrthogonal(direction).normalized : orthogonal;
            var orthogonal1 = Vector3.Cross(direction, orthogonal);
            var step = (endPoint - startPoint).magnitude / (amount - 1);
            var currentRadius = 0f;
            for (var i = 0; i < amount; i++)
            {
                var add = orthogonal * Mathf.Sin(amplitude * Mathf.PI * (i / (float) (amount - 1)));
                add += orthogonal1 * Mathf.Cos(amplitude * Mathf.PI * (i / (float) (amount - 1)));
                points.Add(startPoint + direction * (step * i) + add * currentRadius);
                currentRadius = 2 * radius / amount * i;
            }

            if (noise > 0)
                AddNoise(ref points, startPoint, endPoint, noise);
        }

        public static void GetConeLeft(ref List<Vector3> points, Vector3 startPoint, Vector3 endPoint, int amount,
            int amplitude, float radius, float noise, ref Vector3 orthogonal)
        {
            points.Clear();
            CheckCapacity(ref points, amount);

            var direction = (endPoint - startPoint).normalized;
            orthogonal = VectorCompare(orthogonal, Vector3.zero) ? RandomOrthogonal(direction).normalized : orthogonal;
            var orthogonal1 = Vector3.Cross(direction, orthogonal);
            var step = (endPoint - startPoint).magnitude / (amount - 1);
            var currentRadius = 0f;
            for (var i = 0; i < amount; i++)
            {
                var add = orthogonal * Mathf.Sin(amplitude * Mathf.PI * (i / (float) (amount - 1)));
                add -= orthogonal1 * Mathf.Cos(amplitude * Mathf.PI * (i / (float) (amount - 1)));
                points.Add(startPoint + direction * (step * i) + add * currentRadius);
                currentRadius = radius / amount * i * 2;
            }

            if (noise > 0)
                AddNoise(ref points, startPoint, endPoint, noise);
        }

        #endregion


        #region Tools for external usage

        public static float GetLenght(ref List<Vector3> path)
        {
            var lenght = 0f;
            if (path == null || path.Count <= 1) return lenght;
            for (var i = 1; i < path.Count; i++)
            {
                lenght += (path[i] - path[i - 1]).magnitude;
            }

            return lenght;
        }

        public static void CheckCapacity(ref List<Vector3> points, int size)
        {
            if (points.Capacity < size)
                points.Capacity = size;
        }

        #endregion


        #region Obsolete Line calculations

        [Obsolete]
        public static Vector3[] GetSimpleLineDots(Vector3 startPoint, Vector3 endPoint, int amount, float noise)
        {
            var result = new Vector3[amount];
            result[0] = startPoint;
            result[result.Length - 1] = endPoint;
            for (var i = 1; i < result.Length - 1; i++)
            {
                var t = i / (float) (result.Length - 1);
                result[i] = LinearBezierPoint(t, startPoint, endPoint);
            }

            if (noise > 0)
            {
                result = AddNoise(result, startPoint, endPoint, noise);
            }

            return result;
        }

        [Obsolete]
        public static Vector3[] GetQuadraticBezierDots(Vector3 startPoint, Vector3 endPoint, Vector3 controlPoint,
            int amount, float noise)
        {
            var result = new Vector3[amount];
            result[0] = startPoint;
            result[result.Length - 1] = endPoint;
            for (var i = 1; i < result.Length - 1; i++)
            {
                var t = i / (float) (result.Length - 1);
                result[i] = QuadraticBezierPoint(t, startPoint, endPoint, controlPoint);
            }

            if (noise > 0)
                result = AddNoise(result, startPoint, endPoint, noise);

            return result;
        }

        [Obsolete]
        public static Vector3[] GetCubicBezierDots(Vector3 startPoint, Vector3 endPoint, Vector3 controlPoint1,
            Vector3 controlPoint2, int amount, float noise)
        {
            var result = new Vector3[amount];
            result[0] = startPoint;
            result[result.Length - 1] = endPoint;
            for (var i = 1; i < result.Length - 1; i++)
            {
                var t = i / (float) (result.Length - 1);
                result[i] = CubeBezierPoint(t, startPoint, endPoint, controlPoint1, controlPoint2);
            }

            if (noise > 0)
                result = AddNoise(result, startPoint, endPoint, noise);

            return result;
        }

        [Obsolete]
        public static Vector3[] GetSineLine(Vector3 startPoint, Vector3 endPoint, int amount, int amplitude,
            float wavePower, float noise)
        {
            var result = new Vector3[amount];
            var step = (endPoint - startPoint).magnitude / (amount - 1);

            var direction = (endPoint - startPoint).normalized;
            var orthogonal = RandomOrthogonal(direction).normalized;

            for (var i = 0; i < result.Length; i++)
            {
                result[i] = startPoint + direction * step * i;
                result[i] += orthogonal * Mathf.Sin(amplitude * Mathf.PI * (i / (float) (amount - 1))) * wavePower;
            }

            if (noise > 0)
                result = AddNoise(result, startPoint, endPoint, noise);

            return result;
        }

        [Obsolete]
        public static Vector3[] GetSpiralRight(Vector3 startPoint, Vector3 endPoint, int amount, int amplitude,
            float radius, float noise)
        {
            var result = new Vector3[amount];
            var direction = (endPoint - startPoint).normalized;
            var orthogonal = RandomOrthogonal(direction).normalized;
            var orthogonal1 = Vector3.Cross(direction, orthogonal);
            var step = (endPoint - startPoint).magnitude / (amount - 1);
            for (var i = 0; i < result.Length; i++)
            {
                var add = orthogonal * Mathf.Sin(amplitude * Mathf.PI * (i / (float) (amount - 1)));
                add += orthogonal1 * Mathf.Cos(amplitude * Mathf.PI * (i / (float) (amount - 1)));
                result[i] = startPoint + direction * step * i + add * radius;
            }

            if (noise > 0)
                result = AddNoise(result, startPoint, endPoint, noise);

            return result;
        }

        [Obsolete]
        public static Vector3[] GetSpiralLeft(Vector3 startPoint, Vector3 endPoint, int amount, int amplitude,
            float radius, float noise)
        {
            var result = new Vector3[amount];
            result[0] = startPoint;
            var direction = (endPoint - startPoint).normalized;
            var orthogonal = RandomOrthogonal(direction).normalized;
            var orthogonal1 = Vector3.Cross(direction, orthogonal);
            var step = (endPoint - startPoint).magnitude / (amount - 1);
            for (var i = 0; i < result.Length; i++)
            {
                var add = orthogonal * Mathf.Sin(amplitude * Mathf.PI * (i / (float) (amount - 1)));
                add -= orthogonal1 * Mathf.Cos(amplitude * Mathf.PI * (i / (float) (amount - 1)));
                result[i] = startPoint + direction * step * i + add * radius;
            }

            if (noise > 0)
                result = AddNoise(result, startPoint, endPoint, noise);

            return result;
        }

        [Obsolete]
        public static Vector3[] GetSpringRight(Vector3 startPoint, Vector3 endPoint, int amount, int amplitude,
            float radius, float noise)
        {
            var result = new Vector3[amount];
            var direction = (endPoint - startPoint).normalized;
            var orthogonal = RandomOrthogonal(direction).normalized;
            var orthogonal1 = Vector3.Cross(direction, orthogonal);
            var step = (endPoint - startPoint).magnitude / (amount - 1);
            var currentRadius = 0f;
            for (var i = 0; i < result.Length; i++)
            {
                var add = orthogonal * Mathf.Sin(amplitude * Mathf.PI * (i / (float) (amount - 1)));
                add += orthogonal1 * Mathf.Cos(amplitude * Mathf.PI * (i / (float) (amount - 1)));
                result[i] = startPoint + direction * step * i + add * currentRadius;

                if (i < result.Length / 2)
                {
                    currentRadius += 2 * radius / (amount - 1);
                }
                else
                {
                    currentRadius -= 2 * radius / (amount - 1);
                    currentRadius = Mathf.Clamp(currentRadius, 0f, radius);
                }
            }

            if (noise > 0)
                result = AddNoise(result, startPoint, endPoint, noise);

            return result;
        }

        [Obsolete]
        public static Vector3[] GetSpringLeft(Vector3 startPoint, Vector3 endPoint, int amount, int amplitude,
            float radius, float noise)
        {
            var result = new Vector3[amount];
            var direction = (endPoint - startPoint).normalized;
            var orthogonal = RandomOrthogonal(direction).normalized;
            var orthogonal1 = Vector3.Cross(direction, orthogonal);
            var step = (endPoint - startPoint).magnitude / (amount - 1);
            var currentRadius = 0f;
            for (var i = 0; i < result.Length; i++)
            {
                var add = orthogonal * Mathf.Sin(amplitude * Mathf.PI * (i / (float) (amount - 1)));
                add -= orthogonal1 * Mathf.Cos(amplitude * Mathf.PI * (i / (float) (amount - 1)));
                result[i] = startPoint + direction * step * i + add * currentRadius;

                if (i < result.Length / 2)
                    currentRadius += 2 * radius / (amount - 1);
                else
                {
                    currentRadius -= 2 * radius / (amount - 1);
                    currentRadius = Mathf.Clamp(currentRadius, 0f, radius);
                }
            }

            if (noise > 0)
                result = AddNoise(result, startPoint, endPoint, noise);

            return result;
        }

        [Obsolete]
        public static Vector3[] GetConeRight(Vector3 startPoint, Vector3 endPoint, int amount, int amplitude,
            float radius, float noise)
        {
            var result = new Vector3[amount];
            var direction = (endPoint - startPoint).normalized;
            var orthogonal = RandomOrthogonal(direction).normalized;
            var orthogonal1 = Vector3.Cross(direction, orthogonal);
            var step = (endPoint - startPoint).magnitude / (amount - 1);
            var currentRadius = 0f;
            for (var i = 0; i < result.Length; i++)
            {
                var add = orthogonal * Mathf.Sin(amplitude * Mathf.PI * (i / (float) (amount - 1)));
                add += orthogonal1 * Mathf.Cos(amplitude * Mathf.PI * (i / (float) (amount - 1)));
                result[i] = startPoint + direction * step * i + add * currentRadius;
                currentRadius = 2 * radius / amount * i;
            }

            if (noise > 0)
                result = AddNoise(result, startPoint, endPoint, noise);

            return result;
        }

        [Obsolete]
        public static Vector3[] GetConeLeft(Vector3 startPoint, Vector3 endPoint, int amount, int amplitude,
            float radius, float noise)
        {
            var result = new Vector3[amount];
            var direction = (endPoint - startPoint).normalized;
            var orthogonal = RandomOrthogonal(direction).normalized;
            var orthogonal1 = Vector3.Cross(direction, orthogonal);
            var step = (endPoint - startPoint).magnitude / (amount - 1);
            var currentRadius = 0f;
            for (var i = 0; i < result.Length; i++)
            {
                var add = orthogonal * Mathf.Sin(amplitude * Mathf.PI * (i / (float) (amount - 1)));
                add -= orthogonal1 * Mathf.Cos(amplitude * Mathf.PI * (i / (float) (amount - 1)));
                result[i] = startPoint + direction * step * i + add * currentRadius;
                currentRadius = radius / amount * i * 2;
            }

            if (noise > 0)
                result = AddNoise(result, startPoint, endPoint, noise);

            return result;
        }

        #endregion


        #region Single point calculations

        private static Vector3 LinearBezierPoint(float tF, Vector3 start, Vector3 end)
        {
            var t = 1f - tF;
            return t * start + tF * end;
        }

        private static Vector3 QuadraticBezierPoint(float tF, Vector3 start, Vector3 end, Vector3 control1)
        {
            var t = 1f - tF;
            return Mathf.Pow(t, 2) * start + 2 * tF * t * control1 + Mathf.Pow(tF, 2) * end;
        }

        private static Vector3 CubeBezierPoint(float tF, Vector3 start, Vector3 end, Vector3 control1, Vector3 control2)
        {
            var t = 1f - tF;
            var result = Mathf.Pow(t, 3) * start;
            result += 3 * tF * Mathf.Pow(t, 2) * control1;
            result += 3 * tF * tF * t * control2;
            result += Mathf.Pow(tF, 3) * end;
            return result;
        }

        #endregion


        #region Interal tools

        private static void AddNoise(ref List<Vector3> points, Vector3 startPoint, Vector3 endPoint, float noisePower)
        {
            var direction = endPoint - startPoint;
            AddNoise(ref points, direction, noisePower);
        }

        private static void AddNoise(ref List<Vector3> points, Vector3 direction, float noisePower)
        {
            var orthogonal1 = RandomOrthogonal(direction).normalized;
            var orthogonal2 = Vector3.Cross(direction, orthogonal1).normalized;
            for (var i = 1; i < points.Count - 1; i++)
            {
                points[i] += orthogonal1 * Random.Range(-noisePower, noisePower);
                points[i] += orthogonal2 * Random.Range(-noisePower, noisePower);
            }
        }

        private static bool VectorCompare(Vector3 a, Vector3 b)
        {
            a.Normalize();
            b.Normalize();
            var e =  Mathf.Approximately(a.x, b.x) && Mathf.Approximately(a.y, b.y) && Mathf.Approximately(a.z, b.z);
            return e;
        }

        public static Vector3 RandomOrthogonal(Vector3 v)
        {
            var vPerpendicular = Vector3.one;
            if (v.x != 0)
                vPerpendicular.x = -(v.y * vPerpendicular.y + v.z * vPerpendicular.z) / v.x;
            else
                vPerpendicular.x = Random.Range(0.0f, 1.0f);
            if (v.y != 0)
                vPerpendicular.y = -(v.x * vPerpendicular.x + v.z * vPerpendicular.z) / v.y;
            else
                vPerpendicular.y = Random.Range(0.0f, 1.0f);

            if (v.z != 0)
                vPerpendicular.z = -(v.y * vPerpendicular.y + v.x * vPerpendicular.x) / v.z;
            else
                vPerpendicular.z = Random.Range(0.0f, 1.0f);

            return vPerpendicular.normalized;
        }

        #endregion


        #region Obolete tools

        [Obsolete]
        public static float GetLenght(Vector3[] path)
        {
            var lenght = 0f;
            if (path == null || path.Length <= 1) return lenght;
            for (var i = 1; i < path.Length; i++)
            {
                lenght += (path[i] - path[i - 1]).magnitude;
            }

            return lenght;
        }

        [Obsolete]
        private static Vector3[] AddNoise(Vector3[] lineArray, Vector3 startPoint, Vector3 endPoint, float noisePower)
        {
            Vector3 direction = endPoint - startPoint;
            return AddNoise(lineArray, direction, noisePower);
        }

        [Obsolete]
        private static Vector3[] AddNoise(Vector3[] lineArray, Vector3 direction, float noisePower)
        {
            var orthogonal1 = RandomOrthogonal(direction).normalized;
            var orthogonal2 = Vector3.Cross(direction, orthogonal1).normalized;
            for (var i = 1; i < lineArray.Length - 1; i++)
            {
                lineArray[i] += orthogonal1 * Random.Range(-noisePower, noisePower);
                lineArray[i] += orthogonal2 * Random.Range(-noisePower, noisePower);
            }

            return lineArray;
        }

        #endregion
    }
}
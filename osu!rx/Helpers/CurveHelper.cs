using System;
using System.Collections.Generic;
using System.Numerics;

namespace osu_rx.Helpers
{
    //https://github.com/ppy/osu-framework/blob/master/osu.Framework/MathUtils/PathApproximator.cs
    public class CurveHelper
    {
        public static List<Vector2> ApproximateCircularArc(List<Vector2> points)
        {
            Vector2 a = points[0];
            Vector2 b = points[1];
            Vector2 c = points[2];

            float aSq = (b - c).LengthSquared();
            float bSq = (a - c).LengthSquared();
            float cSq = (a - b).LengthSquared();

            if (aSq.AlmostEquals(0, 1e-3f) || bSq.AlmostEquals(0, 1e-3f) || cSq.AlmostEquals(0, 1e-3f))
                return new List<Vector2>();

            float s = aSq * (bSq + cSq - aSq);
            float t = bSq * (aSq + cSq - bSq);
            float u = cSq * (aSq + bSq - cSq);

            float sum = s + t + u;

            if (sum.AlmostEquals(0, 1e-3f))
                return new List<Vector2>();

            Vector2 centre = (s * a + t * b + u * c) / sum;
            Vector2 dA = a - centre;
            Vector2 dC = c - centre;

            float r = dA.Length();

            double thetaStart = Math.Atan2(dA.Y, dA.X);
            double thetaEnd = Math.Atan2(dC.Y, dC.X);

            while (thetaEnd < thetaStart)
                thetaEnd += 2 * Math.PI;

            double dir = 1;
            double thetaRange = thetaEnd - thetaStart;

            Vector2 orthoAtoC = c - a;
            orthoAtoC = new Vector2(orthoAtoC.Y, -orthoAtoC.X);

            if (Vector2.Dot(orthoAtoC, b - a) < 0)
            {
                dir = -dir;
                thetaRange = 2 * Math.PI - thetaRange;
            }

            int amountPoints = 2 * r <= 0.1f ? 2 : Math.Max(2, (int)Math.Ceiling(thetaRange / (2 * Math.Acos(1 - 0.1f / r))));

            List<Vector2> output = new List<Vector2>(amountPoints);

            for (int i = 0; i < amountPoints; ++i)
            {
                double fract = (double)i / (amountPoints - 1);
                double theta = thetaStart + dir * fract * thetaRange;
                Vector2 o = new Vector2((float)Math.Cos(theta), (float)Math.Sin(theta)) * r;
                output.Add(centre + o);
            }

            return output;
        }

        public static List<Vector2> ApproximateCatmull(List<Vector2> points)
        {
            var result = new List<Vector2>();

            for (int i = 0; i < points.Count - 1; i++)
            {
                var v1 = i > 0 ? points[i - 1] : points[i];
                var v2 = points[i];
                var v3 = i < points.Count - 1 ? points[i + 1] : v2 + v2 - v1;
                var v4 = i < points.Count - 2 ? points[i + 2] : v3 + v3 - v2;

                for (int c = 0; c < 50; c++)
                {
                    result.Add(catmullFindPoint(ref v1, ref v2, ref v3, ref v4, (float)c / 50));
                    result.Add(catmullFindPoint(ref v1, ref v2, ref v3, ref v4, (float)(c + 1) / 50));
                }
            }

            return result;
        }

        public static List<Vector2> ApproximateBezier(List<Vector2> points)
        {
            List<Vector2> output = new List<Vector2>();

            if (points.Count == 0)
                return output;

            var subdivisionBuffer1 = new Vector2[points.Count];
            var subdivisionBuffer2 = new Vector2[points.Count * 2 - 1];

            Stack<Vector2[]> toFlatten = new Stack<Vector2[]>();
            Stack<Vector2[]> freeBuffers = new Stack<Vector2[]>();

            toFlatten.Push(points.ToArray());

            Vector2[] leftChild = subdivisionBuffer2;

            while (toFlatten.Count > 0)
            {
                Vector2[] parent = toFlatten.Pop();

                if (bezierIsFlatEnough(parent))
                {
                    bezierApproximate(parent, output, subdivisionBuffer1, subdivisionBuffer2, points.Count);

                    freeBuffers.Push(parent);
                    continue;
                }

                Vector2[] rightChild = freeBuffers.Count > 0 ? freeBuffers.Pop() : new Vector2[points.Count];
                bezierSubdivide(parent, leftChild, rightChild, subdivisionBuffer1, points.Count);

                for (int i = 0; i < points.Count; ++i)
                    parent[i] = leftChild[i];

                toFlatten.Push(rightChild);
                toFlatten.Push(parent);
            }

            output.Add(points[points.Count - 1]);
            return output;
        }

        private static Vector2 catmullFindPoint(ref Vector2 vec1, ref Vector2 vec2, ref Vector2 vec3, ref Vector2 vec4, float t)
        {
            float t2 = t * t;
            float t3 = t * t2;

            Vector2 result;
            result.X = 0.5f * (2f * vec2.X + (-vec1.X + vec3.X) * t + (2f * vec1.X - 5f * vec2.X + 4f * vec3.X - vec4.X) * t2 + (-vec1.X + 3f * vec2.X - 3f * vec3.X + vec4.X) * t3);
            result.Y = 0.5f * (2f * vec2.Y + (-vec1.Y + vec3.Y) * t + (2f * vec1.Y - 5f * vec2.Y + 4f * vec3.Y - vec4.Y) * t2 + (-vec1.Y + 3f * vec2.Y - 3f * vec3.Y + vec4.Y) * t3);

            return result;
        }

        private static void bezierApproximate(Vector2[] points, List<Vector2> output, Vector2[] subdivisionBuffer1, Vector2[] subdivisionBuffer2, int count)
        {
            Vector2[] l = subdivisionBuffer2;
            Vector2[] r = subdivisionBuffer1;

            bezierSubdivide(points, l, r, subdivisionBuffer1, count);

            for (int i = 0; i < count - 1; ++i)
                l[count + i] = r[i + 1];

            output.Add(points[0]);

            for (int i = 1; i < count - 1; ++i)
            {
                int index = 2 * i;
                Vector2 p = 0.25f * (l[index - 1] + 2 * l[index] + l[index + 1]);
                output.Add(p);
            }
        }

        private static void bezierSubdivide(Vector2[] points, Vector2[] l, Vector2[] r, Vector2[] subdivisionBuffer, int count)
        {
            Vector2[] midpoints = subdivisionBuffer;

            for (int i = 0; i < count; ++i)
                midpoints[i] = points[i];

            for (int i = 0; i < count; i++)
            {
                l[i] = midpoints[0];
                r[count - i - 1] = midpoints[count - i - 1];

                for (int j = 0; j < count - i - 1; j++)
                    midpoints[j] = (midpoints[j] + midpoints[j + 1]) / 2;
            }
        }

        private static bool bezierIsFlatEnough(Vector2[] points)
        {
            for (int i = 1; i < points.Length - 1; i++)
            {
                if ((points[i - 1] - 2 * points[i] + points[i + 1]).LengthSquared() > 0.25f * 0.25f * 4)
                    return false;
            }

            return true;
        }
    }
}

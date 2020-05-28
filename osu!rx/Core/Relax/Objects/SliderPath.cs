using osu_rx.Helpers;
using OsuParsers.Beatmaps.Objects;
using OsuParsers.Enums.Beatmaps;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace osu_rx.Core.Relax.Objects
{
    //https://github.com/ppy/osu/blob/master/osu.Game/Rulesets/Objects/SliderPath.cs
    public class SliderPath
    {
        public readonly double PixelLength;

        public readonly CurveType CurveType;

        public double Distance
        {
            get => cumulativeLength.Count == 0 ? 0 : cumulativeLength[cumulativeLength.Count - 1];
        }

        private Slider slider;

        private Vector2[] sliderPoints;

        private List<Vector2> calculatedPath = new List<Vector2>();
        private List<double> cumulativeLength = new List<double>();

        public SliderPath(Slider slider)
        {
            this.slider = slider;

            sliderPoints = slider.SliderPoints.ToArray();
            CurveType = slider.CurveType;
            PixelLength = slider.PixelLength;

            calculatePath();
            calculateCumulativeLength();
        }

        public double ProgressAt(double progress)
        {
            double p = progress * slider.Repeats % 1;
            if (progress * slider.Repeats % 2 == 1)
                p = 1 - p;

            return p;
        }

        public Vector2 PositionAt(double progress)
        {
            double d = progressToDistance(progress);
            return interpolateVertices(indexOfDistance(d), d);
        }

        public Vector2 PositionAtTime(int time)
        {
            float sliderDuration = slider.EndTime - slider.StartTime;
            float currentSliderTime = time - slider.StartTime;
            double progress = (currentSliderTime / sliderDuration).Clamp(0, 1) * slider.Repeats % 1;
            progress = ProgressAt(progress);

            return PositionAt(progress);
        }

        private List<Vector2> calculateSubpath(List<Vector2> points)
        {
            switch (CurveType)
            {
                case CurveType.Linear:
                    return points;

                case CurveType.PerfectCurve:
                    if (sliderPoints.Length != 3 || points.ToArray().Length != 3)
                        break;

                    List<Vector2> subpath = CurveHelper.ApproximateCircularArc(points);

                    if (subpath.Count == 0)
                        break;

                    return subpath;

                case CurveType.Catmull:
                    return CurveHelper.ApproximateCatmull(points);
            }

            return CurveHelper.ApproximateBezier(points);
        }

        private void calculatePath()
        {
            int start = 0;
            int end = 0;

            for (int i = 0; i < sliderPoints.Length; ++i)
            {
                end++;

                if (i == sliderPoints.Length - 1 || sliderPoints[i] == sliderPoints[i + 1])
                {
                    var points = sliderPoints.Skip(start).Take(end - start).ToList();

                    foreach (Vector2 t in calculateSubpath(points))
                    {
                        if (calculatedPath.Count == 0 || calculatedPath.Last() != t)
                            calculatedPath.Add(t);
                    }

                    start = end;
                }
            }
        }

        private void calculateCumulativeLength()
        {
            double l = 0;

            cumulativeLength.Clear();
            cumulativeLength.Add(l);

            for (int i = 0; i < calculatedPath.Count - 1; ++i)
            {
                Vector2 diff = calculatedPath[i + 1] - calculatedPath[i];
                double d = diff.Length();

                if (PixelLength - l < d)
                {
                    calculatedPath[i + 1] = calculatedPath[i] + diff * (float)((PixelLength - l) / d);
                    calculatedPath.RemoveRange(i + 2, calculatedPath.Count - 2 - i);

                    l = PixelLength;
                    cumulativeLength.Add(l);
                    break;
                }

                l += d;
                cumulativeLength.Add(l);
            }

            if (l < PixelLength && calculatedPath.Count > 1)
            {
                Vector2 diff = calculatedPath[calculatedPath.Count - 1] - calculatedPath[calculatedPath.Count - 2];
                double d = diff.Length();

                if (d <= 0)
                    return;

                calculatedPath[calculatedPath.Count - 1] += diff * (float)((PixelLength - l) / d);
                cumulativeLength[calculatedPath.Count - 1] = PixelLength;
            }
        }

        private int indexOfDistance(double d)
        {
            int i = cumulativeLength.BinarySearch(d);
            if (i < 0) i = ~i;

            return i;
        }

        private double progressToDistance(double progress)
        {
            return progress * Distance;
        }

        private Vector2 interpolateVertices(int i, double d)
        {
            if (calculatedPath.Count == 0)
                return Vector2.Zero;

            if (i <= 0)
                return calculatedPath.First();
            if (i >= calculatedPath.Count)
                return calculatedPath.Last();

            Vector2 p0 = calculatedPath[i - 1];
            Vector2 p1 = calculatedPath[i];

            double d0 = cumulativeLength[i - 1];
            double d1 = cumulativeLength[i];

            if (d0.AlmostEquals(d1, 1e-7))
                return p0;

            double w = (d - d0) / (d1 - d0);
            return p0 + (p1 - p0) * (float)w;
        }
    }
}

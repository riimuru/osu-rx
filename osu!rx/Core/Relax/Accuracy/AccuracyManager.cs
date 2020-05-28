using osu_rx.Configuration;
using osu_rx.Core.Relax.Objects;
using osu_rx.Dependencies;
using osu_rx.Helpers;
using osu_rx.osu;
using OsuParsers.Beatmaps;
using OsuParsers.Beatmaps.Objects;
using System;
using System.Numerics;

namespace osu_rx.Core.Relax.Accuracy
{
    public class AccuracyManager
    {
        private OsuManager osuManager;
        private ConfigManager configManager;

        private Beatmap currentBeatmap;

        private int hitWindow50;
        private int hitWindow100;
        private int hitWindow300;

        private int lastHitScanIndex;
        private Vector2? lastOnNotePosition;
        private SliderPath lastSliderPath;

        private Random random = new Random();

        public AccuracyManager()
        {
            osuManager = DependencyContainer.Get<OsuManager>();
            configManager = DependencyContainer.Get<ConfigManager>();
        }

        public void Reset(Beatmap beatmap)
        {
            currentBeatmap = beatmap;

            hitWindow50 = osuManager.HitWindow50(currentBeatmap.DifficultySection.OverallDifficulty);
            hitWindow100 = osuManager.HitWindow100(currentBeatmap.DifficultySection.OverallDifficulty);
            hitWindow300 = osuManager.HitWindow300(currentBeatmap.DifficultySection.OverallDifficulty);

            lastHitScanIndex = -1;
            lastOnNotePosition = null;
            lastSliderPath = null;
        }

        public HitScanResult GetHitScanResult(int index)
        {
            var hitObject = currentBeatmap.HitObjects[index];

            if (!configManager.EnableHitScan || hitObject is Spinner)
                return HitScanResult.CanHit;

            if (lastHitScanIndex != index)
            {
                lastHitScanIndex = index;
                lastOnNotePosition = null;
                lastSliderPath = hitObject is Slider slider ? new SliderPath(slider) : null;
            }

            float hitObjectRadius = osuManager.HitObjectRadius(currentBeatmap.DifficultySection.CircleSize);

            Vector2 hitObjectPosition = hitObject.Position;
            if (hitObject is Slider && osuManager.CurrentTime > hitObject.StartTime)
                hitObjectPosition += lastSliderPath.PositionAtTime(osuManager.CurrentTime);

            float distanceToObject = Vector2.Distance(osuManager.CursorPosition, hitObjectPosition * osuManager.OsuWindow.PlayfieldRatio);
            float distanceToLastPos = Vector2.Distance(osuManager.CursorPosition, lastOnNotePosition ?? Vector2.Zero);

            if (osuManager.CurrentTime > hitObject.EndTime + hitWindow50)
            {
                if (configManager.HitScanMissAfterHitWindow50)
                    if (distanceToObject <= hitObjectRadius + configManager.HitScanRadiusAdditional && !intersectsWithOtherHitObjects(index + 1))
                        return HitScanResult.ShouldHit;

                return HitScanResult.MoveToNextObject;
            }

            if (configManager.EnableHitScanPrediction)
            {
                if (distanceToObject > hitObjectRadius * configManager.HitScanRadiusMultiplier)
                {
                    if (lastOnNotePosition != null && distanceToLastPos <= configManager.HitScanMaxDistance)
                        return HitScanResult.ShouldHit;
                }
                else if (distanceToObject <= hitObjectRadius)
                    lastOnNotePosition = osuManager.CursorPosition;
            }

            if (distanceToObject <= hitObjectRadius)
                return HitScanResult.CanHit;

            if (configManager.HitScanMissChance != 0)
                if (distanceToObject <= hitObjectRadius + configManager.HitScanRadiusAdditional && random.Next(1, 101) <= configManager.HitScanMissChance && !intersectsWithOtherHitObjects(index + 1))
                    return HitScanResult.CanHit;

            return HitScanResult.Wait;
        }

        public HitObjectTimings GetHitObjectTimings(int index, bool alternating, bool allowHit100)
        {
            var result = new HitObjectTimings();

            float acc = alternating ? random.NextFloat(1.2f, 1.7f) : 2;

            if (allowHit100)
                result.StartOffset = random.Next(-hitWindow100 / 2, hitWindow100 / 2);
            else
                result.StartOffset = random.Next((int)(-hitWindow300 / acc), (int)(hitWindow300 / acc));

            if (currentBeatmap.HitObjects[index] is Slider)
            {
                int sliderDuration = currentBeatmap.HitObjects[index].EndTime - currentBeatmap.HitObjects[index].StartTime;
                result.HoldTime = random.Next(sliderDuration >= 72 ? -26 : sliderDuration / 2 - 10, hitWindow300 * 2);
            }
            else
                result.HoldTime = random.Next(hitWindow300, hitWindow300 * 2);

            return result;
        }

        private bool intersectsWithOtherHitObjects(int startIndex)
        {
            int time = osuManager.CurrentTime;
            Vector2 cursorPosition = osuManager.CursorPosition;

            for (int i = startIndex; i < currentBeatmap.HitObjects.Count; i++)
            {
                var hitObject = currentBeatmap.HitObjects[i];
                double preEmpt = osuManager.DifficultyRange(currentBeatmap.DifficultySection.ApproachRate, 1800, 1200, 450);
                double startTime = hitObject.StartTime - preEmpt;
                if (startTime > time)
                    break;

                float distanceToObject = Vector2.Distance(cursorPosition, hitObject.Position * osuManager.OsuWindow.PlayfieldRatio);
                if (distanceToObject <= osuManager.HitObjectRadius(currentBeatmap.DifficultySection.CircleSize))
                    return true;
            }

            return false;
        }
    }
}
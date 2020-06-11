using osu;
using osu.Memory.Objects.Player.Beatmaps;
using osu.Memory.Objects.Player.Beatmaps.Objects;
using osu_rx.Configuration;
using osu_rx.Helpers;
using SimpleDependencyInjection;
using System;
using System.Numerics;

namespace osu_rx.Core.Relax.Accuracy
{
    public class AccuracyManager
    {
        private OsuManager osuManager;
        private ConfigManager configManager;

        private OsuBeatmap beatmap;

        private int hitWindow50;
        private int hitWindow100;
        private int hitWindow300;

        private int lastHitScanIndex;
        private Vector2? lastOnNotePosition;

        private Random random = new Random();

        public AccuracyManager()
        {
            osuManager = DependencyContainer.Get<OsuManager>();
            configManager = DependencyContainer.Get<ConfigManager>();
        }

        public void Reset(OsuBeatmap beatmap)
        {
            this.beatmap = beatmap;

            hitWindow50 = osuManager.HitWindow50(beatmap.OverallDifficulty);
            hitWindow100 = osuManager.HitWindow100(beatmap.OverallDifficulty);
            hitWindow300 = osuManager.HitWindow300(beatmap.OverallDifficulty);

            lastHitScanIndex = -1;
            lastOnNotePosition = null;
        }

        public HitScanResult GetHitScanResult(int index)
        {
            var hitObject = beatmap.HitObjects[index];

            if (!configManager.EnableHitScan || hitObject is OsuSpinner)
                return HitScanResult.CanHit;

            if (lastHitScanIndex != index)
            {
                lastHitScanIndex = index;
                lastOnNotePosition = null;
            }

            float hitObjectRadius = osuManager.HitObjectRadius(beatmap.CircleSize);

            Vector2 hitObjectPosition = hitObject is OsuSlider ? (hitObject as OsuSlider).PositionAtTime(osuManager.CurrentTime) : hitObject.Position;

            float distanceToObject = Vector2.Distance(osuManager.CursorPosition, hitObjectPosition * osuManager.WindowManager.PlayfieldRatio);
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

            if (beatmap.HitObjects[index] is OsuSlider)
            {
                int sliderDuration = beatmap.HitObjects[index].EndTime - beatmap.HitObjects[index].StartTime;
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

            for (int i = startIndex; i < beatmap.HitObjects.Count; i++)
            {
                var hitObject = beatmap.HitObjects[i];
                double preEmpt = osuManager.DifficultyRange(beatmap.ApproachRate, 1800, 1200, 450);
                double startTime = hitObject.StartTime - preEmpt;
                if (startTime > time)
                    break;

                float distanceToObject = Vector2.Distance(cursorPosition, hitObject.Position * osuManager.WindowManager.PlayfieldRatio);
                if (distanceToObject <= osuManager.HitObjectRadius(beatmap.CircleSize))
                    return true;
            }

            return false;
        }
    }
}
using osu;
using osu.Enums;
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

        private float audioRate;

        //hittimings
        private int minOffset;
        private int maxOffset;
        private int minAlternateOffset;
        private int maxAlternateOffset;

        //hitscan
        private bool canMiss;
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

            var mods = osuManager.Player.HitObjectManager.CurrentMods;
            audioRate = mods.HasFlag(Mods.HalfTime) ? 0.75f : mods.HasFlag(Mods.DoubleTime) ? 1.5f : 1;

            minOffset = calculateTimingOffset(configManager.HitTimingsMinOffset);
            maxOffset = calculateTimingOffset(configManager.HitTimingsMaxOffset);
            minAlternateOffset = calculateTimingOffset(configManager.HitTimingsAlternateMinOffset);
            maxAlternateOffset = calculateTimingOffset(configManager.HitTimingsAlternateMaxOffset);

            canMiss = false;
            lastHitScanIndex = -1;
            lastOnNotePosition = null;
        }

        public HitObjectTimings GetHitObjectTimings(int index, bool alternating, bool doubleDelay)
        {
            var result = new HitObjectTimings();

            int startOffsetMin = (int)((alternating ? minAlternateOffset : minOffset) * (doubleDelay ? configManager.HitTimingsDoubleDelayFactor : 1f));
            int startOffsetMax = (int)((alternating ? maxAlternateOffset : maxOffset) * (doubleDelay ? configManager.HitTimingsDoubleDelayFactor : 1f));

            result.StartOffset = MathHelper.Clamp(random.Next(startOffsetMin, startOffsetMax), -hitWindow50, hitWindow50);

            if (beatmap.HitObjects[index] is OsuSlider)
            {
                int sliderDuration = beatmap.HitObjects[index].EndTime - beatmap.HitObjects[index].StartTime;
                int maxHoldTime = (int)(configManager.HitTimingsMaxSliderHoldTime * audioRate);
                int holdTime = random.Next(configManager.HitTimingsMinSliderHoldTime, maxHoldTime);

                result.HoldTime = MathHelper.Clamp(holdTime, sliderDuration >= 72 ? -26 : sliderDuration / 2 - 10, maxHoldTime);
            }
            else
            {
                int maxHoldTime = (int)(configManager.HitTimingsMaxSliderHoldTime * audioRate);
                int holdTime = random.Next(configManager.HitTimingsMinHoldTime, maxHoldTime);

                result.HoldTime = MathHelper.Clamp(holdTime, 0, maxHoldTime);
            }

            return result;
        }

        private int calculateTimingOffset(int percentage)
        {
            float multiplier = Math.Abs(percentage) / 100f;

            int hitWindowStartTime = multiplier <= 1 ? 0 : multiplier <= 2 ? hitWindow300 + 1 : hitWindow100 + 1;
            int hitWindowEndTime = multiplier <= 1 ? hitWindow300 : multiplier <= 2 ? hitWindow100 : hitWindow50;
            int hitWindowTime = hitWindowEndTime - hitWindowStartTime;

            if (multiplier != 0 && multiplier % 1 == 0) //kinda dirty
                multiplier = 1;
            else
                multiplier %= 1;

            return (int)(hitWindowStartTime + (hitWindowTime * multiplier)) * (percentage < 0 ? -1 : 1);
        }

        public HitScanResult GetHitScanResult(int index)
        {
            var hitObject = beatmap.HitObjects[index];

            if (!configManager.EnableHitScan || hitObject is OsuSpinner)
                return HitScanResult.CanHit;

            if (lastHitScanIndex != index)
            {
                canMiss = configManager.HitScanMissChance != 0 && random.Next(1, 101) <= configManager.HitScanMissChance;
                lastHitScanIndex = index;
                lastOnNotePosition = null;
            }

            float hitObjectRadius = osuManager.HitObjectRadius(beatmap.CircleSize) * osuManager.WindowManager.PlayfieldRatio;
            float additionalRadius = configManager.HitScanRadiusAdditional * osuManager.WindowManager.PlayfieldRatio;

            Vector2 hitObjectPosition = hitObject is OsuSlider ? (hitObject as OsuSlider).PositionAtTime(osuManager.CurrentTime) : hitObject.Position;
            hitObjectPosition = osuManager.WindowManager.PlayfieldToScreen(hitObjectPosition);

            Vector2 cursorPosition = osuManager.Player.Ruleset.MousePosition;

            float distanceToObject = Vector2.Distance(cursorPosition, hitObjectPosition);
            float distanceToLastPos = Vector2.Distance(cursorPosition, lastOnNotePosition ?? Vector2.Zero);

            if (osuManager.CurrentTime > hitObject.EndTime + hitWindow50)
            {
                if (configManager.HitScanMissAfterHitWindow50)
                    if (distanceToObject <= hitObjectRadius + additionalRadius && !intersectsWithOtherHitObjects(index + 1))
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
                    lastOnNotePosition = cursorPosition;
            }

            if (distanceToObject <= hitObjectRadius)
                return HitScanResult.CanHit;

            if (canMiss && distanceToObject <= hitObjectRadius + additionalRadius && !intersectsWithOtherHitObjects(index + 1))
                return HitScanResult.CanHit;

            return HitScanResult.Wait;
        }

        private bool intersectsWithOtherHitObjects(int startIndex)
        {
            int time = osuManager.CurrentTime;
            double preEmpt = osuManager.DifficultyRange(beatmap.ApproachRate, 1800, 1200, 450);

            float hitObjectRadius = osuManager.HitObjectRadius(beatmap.CircleSize) * osuManager.WindowManager.PlayfieldRatio;
            Vector2 cursorPosition = osuManager.Player.Ruleset.MousePosition;

            for (int i = startIndex; i < beatmap.HitObjects.Count; i++)
            {
                var hitObject = beatmap.HitObjects[i];

                double startTime = hitObject.StartTime - preEmpt;
                if (startTime > time)
                    break;

                Vector2 hitObjectPosition = osuManager.WindowManager.PlayfieldToScreen(hitObject.Position);
                float distanceToObject = Vector2.Distance(cursorPosition, hitObjectPosition);
                if (distanceToObject <= hitObjectRadius)
                    return true;
            }

            return false;
        }
    }
}
using osu_rx.Configuration;
using osu_rx.Core.Relax.Accuracy;
using osu_rx.Dependencies;
using osu_rx.osu;
using OsuParsers.Beatmaps;
using OsuParsers.Beatmaps.Objects;
using OsuParsers.Enums;
using System.Threading;
using WindowsInput;
using WindowsInput.Native;

namespace osu_rx.Core.Relax
{
    public class Relax
    {
        private OsuManager osuManager;
        private ConfigManager configManager;
        private InputSimulator inputSimulator;
        private AccuracyManager accuracyManager;

        private Beatmap currentBeatmap;
        private bool shouldStop;

        private int hitWindow50;

        public Relax()
        {
            osuManager = DependencyContainer.Get<OsuManager>();
            configManager = DependencyContainer.Get<ConfigManager>();
            inputSimulator = new InputSimulator();
            accuracyManager = new AccuracyManager();
        }

        public void Start(Beatmap beatmap)
        {
            shouldStop = false;
            currentBeatmap = postProcessBeatmap(beatmap);

            hitWindow50 = osuManager.HitWindow50(currentBeatmap.DifficultySection.OverallDifficulty);

            float audioRate = (osuManager.Player.HitObjectManager.CurrentMods.HasFlag(Mods.DoubleTime) || osuManager.Player.HitObjectManager.CurrentMods.HasFlag(Mods.Nightcore)) ? 1.5f : osuManager.Player.HitObjectManager.CurrentMods.HasFlag(Mods.HalfTime) ? 0.75f : 1f;
            float maxBPM = configManager.MaxSingletapBPM / (audioRate / 2);

            int index, hitTime = 0;
            bool isHit, shouldStartAlternating, shouldAlternate;
            VirtualKeyCode currentKey;
            HitObject currentHitObject;
            HitObjectTimings currentHitTimings;

            reset();

            while (osuManager.CanPlay && index < currentBeatmap.HitObjects.Count && !shouldStop)
            {
                Thread.Sleep(1);

                if (osuManager.IsPaused)
                {
                    if (isHit)
                    {
                        isHit = false;
                        releaseAllKeys();
                    }

                    continue;
                }

                int currentTime = osuManager.CurrentTime + configManager.AudioOffset;
                if (currentTime >= currentHitObject.StartTime - hitWindow50)
                {
                    if (!isHit)
                    {
                        var hitScanResult = accuracyManager.GetHitScanResult(index);
                        switch (hitScanResult)
                        {
                            case HitScanResult.CanHit when currentTime >= currentHitObject.StartTime + currentHitTimings.StartOffset:
                            case HitScanResult.ShouldHit:
                                {
                                    isHit = true;
                                    hitTime = currentTime;

                                    switch (configManager.PlayStyle)
                                    {
                                        case PlayStyles.MouseOnly when currentKey == configManager.PrimaryKey:
                                            inputSimulator.Mouse.LeftButtonDown();
                                            break;
                                        case PlayStyles.MouseOnly:
                                            inputSimulator.Mouse.RightButtonDown();
                                            break;
                                        case PlayStyles.TapX when !shouldAlternate && !shouldStartAlternating:
                                            inputSimulator.Mouse.LeftButtonDown();
                                            currentKey = configManager.PrimaryKey;
                                            break;
                                        default:
                                            inputSimulator.Keyboard.KeyDown(currentKey);
                                            break;
                                    }
                                }
                                break;
                            case HitScanResult.MoveToNextObject:
                                moveToNextObject();
                                break;
                        }
                    }
                    else if (currentTime >= (currentHitObject is HitCircle ? hitTime : currentHitObject.EndTime) + currentHitTimings.HoldTime)
                    {
                        moveToNextObject();

                        if (currentHitObject is Spinner && currentHitObject.StartTime - currentBeatmap.HitObjects[index - 1].EndTime <= configManager.HoldBeforeSpinnerTime)
                            continue;

                        isHit = false;
                        releaseAllKeys();
                    }
                }
            }

            releaseAllKeys();

            while (osuManager.CanPlay && index >= currentBeatmap.HitObjects.Count && !shouldStop)
                Thread.Sleep(5);

            void reset()
            {
                accuracyManager.Reset(currentBeatmap);
                index = closestHitObjectIndex;
                isHit = false;
                currentKey = configManager.PrimaryKey;
                currentHitObject = currentBeatmap.HitObjects[index];
                updateAlternate();
                currentHitTimings = accuracyManager.GetHitObjectTimings(index, shouldAlternate, false);
            }

            void updateAlternate()
            {
                var lastHitObject = index > 0 ? currentBeatmap.HitObjects[index - 1] : null;
                var nextHitObject = index + 1 < currentBeatmap.HitObjects.Count ? currentBeatmap.HitObjects[index + 1] : null;

                shouldStartAlternating = nextHitObject != null ? 60000 / (nextHitObject.StartTime - currentHitObject.EndTime) >= maxBPM : false;
                shouldAlternate = lastHitObject != null ? 60000 / (currentHitObject.StartTime - lastHitObject.EndTime) >= maxBPM : false;
                if (shouldAlternate || configManager.PlayStyle == PlayStyles.Alternate)
                    currentKey = (currentKey == configManager.PrimaryKey) ? configManager.SecondaryKey : configManager.PrimaryKey;
                else
                    currentKey = configManager.PrimaryKey;
            }

            void moveToNextObject()
            {
                index++;
                if (index < currentBeatmap.HitObjects.Count)
                {
                    currentHitObject = currentBeatmap.HitObjects[index];

                    updateAlternate();
                    currentHitTimings = accuracyManager.GetHitObjectTimings(index, shouldAlternate, inputSimulator.InputDeviceState.IsKeyDown(configManager.HitWindow100Key));
                }
            }
        }

        public void Stop() => shouldStop = true;

        private Beatmap postProcessBeatmap(Beatmap beatmap)
        {
            foreach (var hitObject in beatmap.HitObjects)
                if (hitObject is Slider slider)
                    for (int i = 0; i < slider.SliderPoints.Count; i++)
                        slider.SliderPoints[i] -= slider.Position;

            return beatmap;
        }

        private int closestHitObjectIndex
        {
            get
            {
                int time = osuManager.CurrentTime;
                for (int i = 0; i < currentBeatmap.HitObjects.Count; i++)
                    if (currentBeatmap.HitObjects[i].StartTime >= time)
                        return i;

                return currentBeatmap.HitObjects.Count;
            }
        }

        private void releaseAllKeys()
        {
            inputSimulator.Keyboard.KeyUp(configManager.PrimaryKey);
            inputSimulator.Keyboard.KeyUp(configManager.SecondaryKey);
            inputSimulator.Mouse.LeftButtonUp();
            inputSimulator.Mouse.RightButtonUp();
        }
    }
}
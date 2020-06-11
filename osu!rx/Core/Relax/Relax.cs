using osu;
using osu.Enums;
using osu.Memory.Objects.Bindings;
using osu.Memory.Objects.Player.Beatmaps;
using osu.Memory.Objects.Player.Beatmaps.Objects;
using osu_rx.Configuration;
using osu_rx.Core.Relax.Accuracy;
using SimpleDependencyInjection;
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

        private bool shouldStop;

        private int hitWindow50;

        private VirtualKeyCode leftClick;
        private VirtualKeyCode rightClick;

        public Relax()
        {
            osuManager = DependencyContainer.Get<OsuManager>();
            configManager = DependencyContainer.Get<ConfigManager>();
            inputSimulator = new InputSimulator();
            accuracyManager = new AccuracyManager();
        }

        public void Start(OsuBeatmap beatmap)
        {
            shouldStop = false;

            hitWindow50 = osuManager.HitWindow50(beatmap.OverallDifficulty);

            leftClick = (VirtualKeyCode)osuManager.BindingManager.GetKeyCode(Bindings.OsuLeft);
            rightClick = (VirtualKeyCode)osuManager.BindingManager.GetKeyCode(Bindings.OsuRight);

            float audioRate = osuManager.Player.HitObjectManager.CurrentMods.HasFlag(Mods.DoubleTime) ? 1.5f : osuManager.Player.HitObjectManager.CurrentMods.HasFlag(Mods.HalfTime) ? 0.75f : 1f;
            float maxBPM = configManager.MaxSingletapBPM / (audioRate / 2);

            int index, hitTime = 0;
            bool isHit, shouldStartAlternating, shouldAlternate;
            OsuKeys currentKey;
            OsuHitObject currentHitObject;
            HitObjectTimings currentHitTimings;

            reset();

            while (osuManager.CanPlay && index < beatmap.HitObjects.Count && !shouldStop)
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
                                            inputSimulator.Keyboard.KeyDown(currentKey == OsuKeys.K1M1 ? leftClick : rightClick);
                                            break;
                                    }
                                }
                                break;
                            case HitScanResult.MoveToNextObject:
                                moveToNextObject();
                                break;
                        }
                    }
                    else if (currentTime >= (currentHitObject is OsuHitCircle ? hitTime : currentHitObject.EndTime) + currentHitTimings.HoldTime)
                    {
                        moveToNextObject();

                        if (currentHitObject is OsuSpinner && currentHitObject.StartTime - beatmap.HitObjects[index - 1].EndTime <= configManager.HoldBeforeSpinnerTime)
                            continue;

                        isHit = false;
                        releaseAllKeys();
                    }
                }
            }

            releaseAllKeys();

            while (osuManager.CanPlay && index >= beatmap.HitObjects.Count && !shouldStop)
                Thread.Sleep(5);

            void reset()
            {
                accuracyManager.Reset(beatmap);
                index = osuManager.Player.HitObjectManager.CurrentHitObjectIndex;
                isHit = false;
                currentKey = configManager.PrimaryKey;
                currentHitObject = beatmap.HitObjects[index];
                updateAlternate();
                currentHitTimings = accuracyManager.GetHitObjectTimings(index, shouldAlternate, false);
            }

            void updateAlternate()
            {
                var lastHitObject = index > 0 ? beatmap.HitObjects[index - 1] : null;
                var nextHitObject = index + 1 < beatmap.HitObjects.Count ? beatmap.HitObjects[index + 1] : null;

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
                if (index < beatmap.HitObjects.Count)
                {
                    currentHitObject = beatmap.HitObjects[index];

                    updateAlternate();
                    currentHitTimings = accuracyManager.GetHitObjectTimings(index, shouldAlternate, inputSimulator.InputDeviceState.IsKeyDown(configManager.HitWindow100Key));
                }
            }
        }

        public void Stop() => shouldStop = true;

        private void releaseAllKeys()
        {
            inputSimulator.Keyboard.KeyUp(leftClick);
            inputSimulator.Keyboard.KeyUp(rightClick);
            inputSimulator.Mouse.LeftButtonUp();
            inputSimulator.Mouse.RightButtonUp();
        }
    }
}
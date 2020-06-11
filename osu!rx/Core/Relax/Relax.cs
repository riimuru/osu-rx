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

        private OsuBeatmap beatmap;

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
            this.beatmap = beatmap;

            shouldStop = false;

            hitWindow50 = osuManager.HitWindow50(beatmap.OverallDifficulty);

            leftClick = (VirtualKeyCode)osuManager.BindingManager.GetKeyCode(Bindings.OsuLeft);
            rightClick = (VirtualKeyCode)osuManager.BindingManager.GetKeyCode(Bindings.OsuRight);

            accuracyManager.Reset(beatmap);

            bool isHit = false;
            int hitTime = 0;

            int index = osuManager.Player.HitObjectManager.CurrentHitObjectIndex;
            OsuHitObject currentHitObject = beatmap.HitObjects[index];

            var alternateResult = AlternateResult.None;
            OsuKeys currentKey = configManager.PrimaryKey;

            HitObjectTimings currentHitTimings = accuracyManager.GetHitObjectTimings(index, false, false);

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
                                        case PlayStyles.TapX when alternateResult == AlternateResult.None:
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

            void moveToNextObject()
            {
                index++;

                if (index < beatmap.HitObjects.Count)
                {
                    currentHitObject = beatmap.HitObjects[index];

                    alternateResult = getAlternateResult(index);
                    if (alternateResult.HasFlag(AlternateResult.AlternateThisNote))
                        currentKey = currentKey == configManager.PrimaryKey ? configManager.SecondaryKey : configManager.PrimaryKey;
                    else
                        currentKey = configManager.PrimaryKey;

                    currentHitTimings = accuracyManager.GetHitObjectTimings(index, alternateResult.HasFlag(AlternateResult.AlternateThisNote), inputSimulator.InputDeviceState.IsKeyDown(configManager.HitWindow100Key));
                }
            }
        }

        public void Stop() => shouldStop = true;

        private AlternateResult getAlternateResult(int index)
        {
            if (configManager.PlayStyle == PlayStyles.Alternate)
                return AlternateResult.AlternateThisNote;

            var result = AlternateResult.None;

            float audioRate = osuManager.Player.HitObjectManager.CurrentMods.HasFlag(Mods.DoubleTime) ? 1.5f : osuManager.Player.HitObjectManager.CurrentMods.HasFlag(Mods.HalfTime) ? 0.75f : 1f;
            float maxBPM = configManager.MaxSingletapBPM / (audioRate / 2);

            var currentHitObject = beatmap.HitObjects[index];
            var lastHitObject = index > 0 ? beatmap.HitObjects[index - 1] : null;
            var nextHitObject = index + 1 < beatmap.HitObjects.Count ? beatmap.HitObjects[index + 1] : null;

            if (lastHitObject != null && 60000 / (currentHitObject.StartTime - lastHitObject.EndTime) >= maxBPM)
                result += (int)AlternateResult.AlternateThisNote;

            if (nextHitObject != null && 60000 / (nextHitObject.StartTime - currentHitObject.EndTime) >= maxBPM)
                result += (int)AlternateResult.AlternateNextNote;

            return result;
        }

        private void releaseAllKeys()
        {
            inputSimulator.Keyboard.KeyUp(leftClick);
            inputSimulator.Keyboard.KeyUp(rightClick);
            inputSimulator.Mouse.LeftButtonUp();
            inputSimulator.Mouse.RightButtonUp();
        }
    }
}
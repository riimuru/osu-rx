using osu.Enums;
using osu.Memory;
using osu.Memory.Objects.Bindings;
using osu.Memory.Objects.Player;
using osu.Memory.Objects.Window;
using SimpleDependencyInjection;
using System;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Threading;

namespace osu
{
    public class OsuManager
    {
        public string DebugInfo { get; private set; }

        public OsuProcess OsuProcess { get; private set; }

        public OsuWindowManager WindowManager { get; private set; }

        public BindingManager BindingManager { get; private set; }

        public OsuPlayer Player { get; private set; }

        public int CurrentTime => OsuProcess.ReadInt32(timeAddress);

        public bool IsPaused => !OsuProcess.ReadBool(timeAddress + Signatures.IsAudioPlayingOffset);

        public OsuModes CurrentMode => (OsuModes)OsuProcess.ReadInt32(modeAddress);

        public Vector2 CursorPosition => Player.Ruleset.MousePosition - WindowManager.PlayfieldPosition;

        public bool CanPlay => CurrentMode == OsuModes.Play && Player.IsLoaded && !Player.ReplayMode;

        public float HitObjectScalingFactor(float circleSize) => 1f - 0.7f * (float)AdjustDifficulty(circleSize);

        public float HitObjectRadius(float circleSize)
        {
            float size = (float)(WindowManager.PlayfieldSize.X / 8f * HitObjectScalingFactor(circleSize));
            float radius = size / 2f / WindowManager.PlayfieldRatio * 1.00041f;

            return radius;
        }

        public int HitWindow300(double od) => (int)DifficultyRange(od, 80, 50, 20);
        public int HitWindow100(double od) => (int)DifficultyRange(od, 140, 100, 60);
        public int HitWindow50(double od) => (int)DifficultyRange(od, 200, 150, 100);

        public double AdjustDifficulty(double difficulty) => (ApplyModsToDifficulty(difficulty, 1.3) - 5) / 5;

        public double ApplyModsToDifficulty(double difficulty, double hardrockFactor)
        {
            if (Player.HitObjectManager.CurrentMods.HasFlag(Mods.Easy))
                difficulty = Math.Max(0, difficulty / 2);
            if (Player.HitObjectManager.CurrentMods.HasFlag(Mods.HardRock))
                difficulty = Math.Min(10, difficulty * hardrockFactor);

            return difficulty;
        }

        public double DifficultyRange(double difficulty, double min, double mid, double max)
        {
            difficulty = ApplyModsToDifficulty(difficulty, 1.4);

            if (difficulty > 5)
                return mid + (max - mid) * (difficulty - 5) / 5;
            if (difficulty < 5)
                return mid - (mid - min) * (5 - difficulty) / 5;
            return mid;
        }

        public bool Initialize()
        {
            Console.WriteLine("Initializing...");

            var osuProcess = Process.GetProcessesByName("osu!").FirstOrDefault();
            if (osuProcess == default)
            {
                Console.WriteLine("\nWaiting for osu!...");

                while (osuProcess == default)
                {
                    osuProcess = Process.GetProcessesByName("osu!").FirstOrDefault();
                    Thread.Sleep(500);
                }
            }

            osuProcess.EnableRaisingEvents = true;
            osuProcess.Exited += (o, e) => Environment.Exit(0);

            OsuProcess = new OsuProcess(osuProcess);
            DependencyContainer.Cache(OsuProcess);

            return scanMemory();
        }

        private UIntPtr timeAddress;
        private UIntPtr modeAddress;
        private bool scanMemory()
        {
            bool timeResult = false, modeResult = false, viewportResult = false, bindingManagerResult = false, playerResult = false;
            try
            {
                Console.WriteLine("\nScanning for memory addresses (this may take a while)...");

                timeResult = OsuProcess.FindPattern(Signatures.Time.Pattern, out UIntPtr timePointer);
                modeResult = OsuProcess.FindPattern(Signatures.Mode.Pattern, out UIntPtr modePointer);
                viewportResult = OsuProcess.FindPattern(Signatures.Viewport.Pattern, out UIntPtr viewportPointer);
                bindingManagerResult = OsuProcess.FindPattern(Signatures.BindingManager.Pattern, out UIntPtr bindingManagerPointer);
                playerResult = OsuProcess.FindPattern(Signatures.Player.Pattern, out UIntPtr playerPointer);

                if (timeResult && modeResult && viewportResult && bindingManagerResult && playerResult)
                {
                    timeAddress = (UIntPtr)OsuProcess.ReadUInt32(timePointer + Signatures.Time.Offset);
                    modeAddress = (UIntPtr)OsuProcess.ReadUInt32(modePointer + Signatures.Mode.Offset);
                    WindowManager = new OsuWindowManager((UIntPtr)OsuProcess.ReadUInt32(viewportPointer + Signatures.Viewport.Offset));
                    BindingManager = new BindingManager((UIntPtr)OsuProcess.ReadUInt32(bindingManagerPointer + Signatures.BindingManager.Offset));
                    Player = new OsuPlayer((UIntPtr)OsuProcess.ReadUInt32(playerPointer + Signatures.Player.Offset));
                }
            }
            catch
            {
            }

            if (timeAddress == UIntPtr.Zero || modeAddress == UIntPtr.Zero || WindowManager == null || BindingManager == null || Player == null)
            {
                DebugInfo = $"Time result: {(timeResult ? "success" : "fail")}\n" +
                    $"Mode result: {(modeResult ? "success" : "fail")}\n" +
                    $"Viewport result: {(viewportResult ? "success" : "fail")}\n" +
                    $"BindingManager result: {(bindingManagerResult ? "success" : "fail")}\n" +
                    $"Player result: {(playerResult ? "success" : "fail")}";

                return false;
            }

            return true;
        }
    }
}
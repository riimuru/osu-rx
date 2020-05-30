using osu_rx.Dependencies;
using osu_rx.osu.Memory;
using osu_rx.osu.Memory.Objects;
using osu_rx.osu.Memory.Objects.Bindings;
using osu_rx.osu.Memory.Objects.Player;
using osu_rx.osu.Memory.Objects.Window;
using OsuParsers.Enums;
using System;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Threading;

namespace osu_rx.osu
{
    public class OsuManager
    {
        public OsuProcess OsuProcess { get; private set; }

        public OsuWindowManager WindowManager { get; private set; }

        public BindingManager BindingManager { get; private set; }

        public OsuPlayer Player { get; private set; }

        public bool LoadedSuccessfully { get; private set; } = true;

        public int CurrentTime => OsuProcess.ReadInt32(timeAddress);

        public bool IsPaused => !OsuProcess.ReadBool(timeAddress + Signatures.IsAudioPlayingOffset);

        public OsuStates CurrentState => (OsuStates)OsuProcess.ReadInt32(stateAddress);

        public Vector2 CursorPosition => Player.Ruleset.MousePosition - WindowManager.PlayfieldPosition;

        public bool CanPlay => CurrentState == OsuStates.Play && Player.IsLoaded && !Player.ReplayMode;

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
                Console.WriteLine("\nosu! process not found! Please launch osu! first!");
                return false;
            }

            osuProcess.EnableRaisingEvents = true;
            osuProcess.Exited += (o, e) => Environment.Exit(0);
            OsuProcess = new OsuProcess(osuProcess);
            DependencyContainer.Cache(OsuProcess);

            scanMemory();

            return true;
        }

        private UIntPtr timeAddress;
        private UIntPtr stateAddress;
        private void scanMemory()
        {
            bool timeResult = false, stateResult = false, viewportResult = false, bindingManagerResult = false, playerResult = false;

            try
            {
                Console.WriteLine("\nScanning for memory addresses (this may take a while)...");

                timeResult = OsuProcess.FindPattern(Signatures.Time.Pattern, out UIntPtr timePointer);
                stateResult = OsuProcess.FindPattern(Signatures.State.Pattern, out UIntPtr statePointer);
                viewportResult = OsuProcess.FindPattern(Signatures.Viewport.Pattern, out UIntPtr viewportPointer);
                bindingManagerResult = OsuProcess.FindPattern(Signatures.BindingManager.Pattern, out UIntPtr bindingManagerPointer);
                playerResult = OsuProcess.FindPattern(Signatures.Player.Pattern, out UIntPtr playerPointer);

                if (timeResult && stateResult && viewportResult && bindingManagerResult && playerResult)
                {
                    timeAddress = (UIntPtr)OsuProcess.ReadUInt32(timePointer + Signatures.Time.Offset);
                    stateAddress = (UIntPtr)OsuProcess.ReadUInt32(statePointer + Signatures.State.Offset);
                    WindowManager = new OsuWindowManager((UIntPtr)OsuProcess.ReadUInt32(viewportPointer + Signatures.Viewport.Offset));
                    BindingManager = new BindingManager((UIntPtr)OsuProcess.ReadUInt32(bindingManagerPointer + Signatures.BindingManager.Offset));
                    Player = new OsuPlayer((UIntPtr)OsuProcess.ReadUInt32(playerPointer + Signatures.Player.Offset));
                }
            }
            catch { }
            finally
            {
                if (timeAddress == UIntPtr.Zero || stateAddress == UIntPtr.Zero || WindowManager == null || BindingManager == null || Player == null)
                {
                    Console.Clear();
                    Console.WriteLine("osu!rx failed to initialize:\n");
                    Console.WriteLine("Memory scanning failed! Please report this on GitHub/MPGH.");
                    Console.WriteLine("Please include as much info as possible (OS version, hack version, build source, debug info, etc.).");
                    Console.WriteLine($"\n\nDebug Info:\n");
                    Console.WriteLine($"Time result: {(timeResult ? "success" : "fail")}");
                    Console.WriteLine($"State result: {(stateResult ? "success" : "fail")}");
                    Console.WriteLine($"Viewport result: {(viewportResult ? "success" : "fail")}");
                    Console.WriteLine($"BindingManager result: {(bindingManagerResult ? "success" : "fail")}");
                    Console.WriteLine($"Player result: {(playerResult ? "success" : "fail")}");

                    while (true)
                        Thread.Sleep(1000);
                }
            }
        }
    }
}
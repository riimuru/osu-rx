using osu;
using osu.Enums;
using osu_rx.Configuration;
using osu_rx.Core.Relax;
using osu_rx.Core.Timewarp;
using SimpleDependencyInjection;
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using WindowsInput.Native;

namespace osu_rx
{
    class Program
    {
        private static OsuManager osuManager;
        private static ConfigManager configManager;
        private static Relax relax;
        private static Timewarp timewarp;
        private static string defaultConsoleTitle;

        static void Main(string[] args)
        {
            osuManager = new OsuManager();

            if (!osuManager.Initialize())
            {
                Console.Clear();
                Console.WriteLine("osu!rx failed to initialize:\n");
                Console.WriteLine("Memory scanning failed! Please report this on GitHub/MPGH.");
                Console.WriteLine("Please include as much info as possible (OS version, hack version, build source, debug info, etc.).");
                Console.WriteLine($"\n\nDebug Info:\n");
                Console.WriteLine(osuManager.DebugInfo);

                while (true)
                    Thread.Sleep(1000);
            }

            configManager = new ConfigManager();

            DependencyContainer.Cache(osuManager);
            DependencyContainer.Cache(configManager);

            relax = new Relax();
            timewarp = new Timewarp();

            defaultConsoleTitle = Console.Title;
            if (configManager.UseCustomWindowTitle)
                Console.Title = configManager.CustomWindowTitle;

            DrawMainMenu();
        }

        private static void DrawMainMenu()
        {
            string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            version = version.Remove(version.LastIndexOf(".0"));

            Console.Clear();
            Console.WriteLine($"osu!rx v{version} (MPGH release)");
            Console.WriteLine("\n---Main Menu---");
            Console.WriteLine("\n1. Start");
            Console.WriteLine("2. Settings");

            switch (Console.ReadKey(true).Key)
            {
                case ConsoleKey.D1:
                    DrawPlayer();
                    break;
                case ConsoleKey.D2:
                    DrawSettings();
                    break;
                default:
                    DrawMainMenu();
                    break;
            }
        }

        private static void DrawSettings()
        {
            Console.Clear();
            Console.WriteLine("---Settings---\n");
            Console.WriteLine("1. Relax settings");
            Console.WriteLine("2. Other settings\n");
            Console.WriteLine("3. Experimental settings");

            Console.WriteLine("\nESC. Back to main menu");

            switch (Console.ReadKey(true).Key)
            {
                case ConsoleKey.D1:
                    DrawRelaxSettings();
                    break;
                case ConsoleKey.D2:
                    DrawOtherSettings();
                    break;
                case ConsoleKey.D3:
                    DrawExperimentalSettings();
                    break;
                case ConsoleKey.Escape:
                    DrawMainMenu();
                    break;
                default:
                    DrawSettings();
                    break;
            }
        }

        private static void DrawRelaxSettings()
        {
            Console.Clear();
            Console.WriteLine("---Relax Settings---\n");
            Console.WriteLine($"1. Relax                    | [{(configManager.EnableRelax ? "ENABLED" : "DISABLED")}]");
            Console.WriteLine($"2. Playstyle                | [{configManager.PlayStyle}]");
            Console.WriteLine($"3. Primary key              | [{configManager.PrimaryKey}]");
            Console.WriteLine($"4. Secondary key            | [{configManager.SecondaryKey}]");
            Console.WriteLine($"5. Hit window 100 key       | [{configManager.HitWindow100Key}]");
            Console.WriteLine($"6. Max singletap BPM        | [{configManager.MaxSingletapBPM}]");
            Console.WriteLine($"7. AlternateIfLessThan      | [{configManager.AlternateIfLessThan}]");
            Console.WriteLine($"8. Audio offset             | [{configManager.AudioOffset}]");
            Console.WriteLine($"9. HoldBeforeSpinner time   | [{configManager.HoldBeforeSpinnerTime}]");

            Console.WriteLine($"\n0. Hitscan settings");

            Console.WriteLine("\nESC. Back to settings");

            OsuKeys[] osuKeys = (OsuKeys[])Enum.GetValues(typeof(OsuKeys));
            switch (Console.ReadKey(true).Key)
            {
                case ConsoleKey.D1:
                    configManager.EnableRelax = !configManager.EnableRelax;
                    DrawRelaxSettings();
                    break;
                case ConsoleKey.D2:
                    Console.Clear();
                    Console.WriteLine("Select new playstyle:\n");
                    PlayStyles[] playstyles = (PlayStyles[])Enum.GetValues(typeof(PlayStyles));
                    for (int i = 0; i < playstyles.Length; i++)
                        Console.WriteLine($"{i + 1}. {playstyles[i]}");
                    if (int.TryParse(Console.ReadKey(true).KeyChar.ToString(), out int selected) && selected > 0 && selected < 4)
                        configManager.PlayStyle = (PlayStyles)selected - 1;
                    else
                        goto case ConsoleKey.D2;
                    DrawRelaxSettings();
                    break;
                case ConsoleKey.D3:
                    Console.Clear();
                    Console.WriteLine("Enter new primary key:\n");
                    for (int i = 0; i < osuKeys.Length; i++)
                        Console.WriteLine($"{i + 1}. {osuKeys[i]}");
                    if (int.TryParse(Console.ReadKey(true).KeyChar.ToString(), out int primaryKey) && primaryKey > 0 && primaryKey < 5)
                        configManager.PrimaryKey = (OsuKeys)primaryKey - 1;
                    else
                        goto case ConsoleKey.D3;
                    DrawRelaxSettings();
                    break;
                case ConsoleKey.D4:
                    Console.Clear();
                    Console.WriteLine("Enter new secondary key:\n");
                    for (int i = 0; i < osuKeys.Length; i++)
                        Console.WriteLine($"{i + 1}. {osuKeys[i]}");
                    if (int.TryParse(Console.ReadKey(true).KeyChar.ToString(), out int secondaryKey) && secondaryKey > 0 && secondaryKey < 5)
                        configManager.SecondaryKey = (OsuKeys)secondaryKey - 1;
                    else
                        goto case ConsoleKey.D4;
                    DrawRelaxSettings();
                    break;
                case ConsoleKey.D5:
                    Console.Clear();
                    Console.Write("Enter new hit window 100 key: ");
                    configManager.HitWindow100Key = (VirtualKeyCode)Console.ReadKey(true).Key;
                    DrawRelaxSettings();
                    break;
                case ConsoleKey.D6:
                    Console.Clear();
                    Console.Write("Enter new max singletap BPM: ");
                    if (int.TryParse(Console.ReadLine(), out int bpm))
                    {
                        configManager.MaxSingletapBPM = bpm;
                        configManager.AlternateIfLessThan = 60000 / bpm;
                    }
                    else
                        goto case ConsoleKey.D6;
                    DrawRelaxSettings();
                    break;
                case ConsoleKey.D7:
                    Console.Clear();
                    Console.Write("Enter new AlternateIfLessThan: ");
                    if (int.TryParse(Console.ReadLine(), out int alternateIfLessThan))
                    {
                        configManager.AlternateIfLessThan = alternateIfLessThan;
                        configManager.MaxSingletapBPM = 60000 / alternateIfLessThan;
                    }
                    else
                        goto case ConsoleKey.D7;
                    DrawRelaxSettings();
                    break;
                case ConsoleKey.D8:
                    Console.Clear();
                    Console.Write("Enter new audio offset: ");
                    if (int.TryParse(Console.ReadLine(), out int offset))
                        configManager.AudioOffset = offset;
                    else
                        goto case ConsoleKey.D8;
                    DrawRelaxSettings();
                    break;
                case ConsoleKey.D9:
                    Console.Clear();
                    Console.Write("Enter new HoldBeforeSpinner time: ");
                    if (int.TryParse(Console.ReadLine(), out int holdBeforeSpinnerTime))
                        configManager.HoldBeforeSpinnerTime = holdBeforeSpinnerTime;
                    else
                        goto case ConsoleKey.D9;
                    DrawRelaxSettings();
                    break;
                case ConsoleKey.D0:
                    DrawHitScanSettings();
                    break;
                case ConsoleKey.Escape:
                    DrawSettings();
                    break;
                default:
                    DrawRelaxSettings();
                    break;
            }
        }

        private static void DrawHitScanSettings()
        {
            Console.Clear();
            Console.WriteLine("---HitScan Settings---\n");
            Console.WriteLine($"1. HitScan                | [{(configManager.EnableHitScan ? "ENABLED" : "DISABLED")}]");
            Console.WriteLine($"2. Prediction             | [{(configManager.EnableHitScanPrediction ? "ENABLED" : "DISABLED")}]");
            Console.WriteLine($"3. Radius multiplier      | [{configManager.HitScanRadiusMultiplier}]");
            Console.WriteLine($"4. Radius additional      | [{configManager.HitScanRadiusAdditional}]");
            Console.WriteLine($"5. Max distance           | [{configManager.HitScanMaxDistance}]");
            Console.WriteLine($"6. Miss chance            | [{configManager.HitScanMissChance}%]");
            Console.WriteLine($"7. Miss after HitWindow50 | [{(configManager.HitScanMissAfterHitWindow50 ? "ENABLED" : "DISABLED")}]");

            Console.WriteLine("\nESC. Back to relax settings");

            switch (Console.ReadKey(true).Key)
            {
                case ConsoleKey.D1:
                    configManager.EnableHitScan = !configManager.EnableHitScan;
                    DrawHitScanSettings();
                    break;
                case ConsoleKey.D2:
                    configManager.EnableHitScanPrediction = !configManager.EnableHitScanPrediction;
                    DrawHitScanSettings();
                    break;
                case ConsoleKey.D3:
                    Console.Clear();
                    Console.Write("Enter new radius multiplier: ");
                    if (float.TryParse(Console.ReadLine(), out float multiplier))
                        configManager.HitScanRadiusMultiplier = multiplier;
                    else
                        goto case ConsoleKey.D3;
                    DrawHitScanSettings();
                    break;
                case ConsoleKey.D4:
                    Console.Clear();
                    Console.Write("Enter new radius additional: ");
                    if (int.TryParse(Console.ReadLine(), out int additional))
                        configManager.HitScanRadiusAdditional = additional;
                    else
                        goto case ConsoleKey.D4;
                    DrawHitScanSettings();
                    break;
                case ConsoleKey.D5:
                    Console.Clear();
                    Console.Write("Enter new max distance: ");
                    if (int.TryParse(Console.ReadLine(), out int maxDistance))
                        configManager.HitScanMaxDistance = maxDistance;
                    else
                        goto case ConsoleKey.D5;
                    DrawHitScanSettings();
                    break;
                case ConsoleKey.D6:
                    Console.Clear();
                    Console.Write("Enter new miss chance: ");
                    if (int.TryParse(Console.ReadLine(), out int missChance))
                        configManager.HitScanMissChance = missChance;
                    else
                        goto case ConsoleKey.D6;
                    DrawHitScanSettings();
                    break;
                case ConsoleKey.D7:
                    configManager.HitScanMissAfterHitWindow50 = !configManager.HitScanMissAfterHitWindow50;
                    DrawHitScanSettings();
                    break;
                case ConsoleKey.Escape:
                    DrawRelaxSettings();
                    break;
                default:
                    DrawHitScanSettings();
                    break;
            }
        }

        private static void DrawOtherSettings()
        {
            Console.Clear();
            Console.WriteLine("---Other Settings---\n");
            Console.WriteLine($"1. Custom window title | [{(configManager.UseCustomWindowTitle ? $"ON | {configManager.CustomWindowTitle}" : "OFF")}]");

            Console.WriteLine("\nESC. Back to settings");

            switch (Console.ReadKey(true).Key)
            {
                case ConsoleKey.D1:
                    Console.Clear();
                    Console.WriteLine("Use custom window title?\n");
                    Console.WriteLine("1. Yes");
                    Console.WriteLine("2. No");
                    configManager.UseCustomWindowTitle = Console.ReadKey(true).Key == ConsoleKey.D1;
                    if (configManager.UseCustomWindowTitle)
                    {
                        Console.Clear();
                        Console.Write("Enter new custom window title: ");
                        configManager.CustomWindowTitle = Console.ReadLine();
                        Console.Title = configManager.CustomWindowTitle;
                    }
                    else
                        Console.Title = defaultConsoleTitle;
                    DrawOtherSettings();
                    break;
                case ConsoleKey.Escape:
                    DrawSettings();
                    break;
                default:
                    DrawOtherSettings();
                    break;
            }
        }

        private static void DrawExperimentalSettings()
        {
            Console.Clear();
            Console.WriteLine("---Experimental Settings---\n");
            Console.WriteLine($"1. Timewarp      | [{(configManager.EnableTimewarp ? "ENABLED" : "DISABLED")}]");
            Console.WriteLine($"2. Timewarp rate | [{configManager.TimewarpRate}x]");

            Console.WriteLine("\nESC. Back to settings");

            switch (Console.ReadKey(true).Key)
            {
                case ConsoleKey.D1:
                    configManager.EnableTimewarp = !configManager.EnableTimewarp;
                    DrawExperimentalSettings();
                    break;
                case ConsoleKey.D2:
                    Console.Clear();
                    Console.Write("Enter new timewarp rate: ");
                    if (double.TryParse(Console.ReadLine(), out double rate))
                        configManager.TimewarpRate = rate;
                    else
                        goto case ConsoleKey.D2;
                    DrawExperimentalSettings();
                    break;
                case ConsoleKey.Escape:
                    DrawSettings();
                    break;
                default:
                    DrawExperimentalSettings();
                    break;
            }
        }

        private static void DrawPlayer()
        {
            bool shouldExit = false;
            Task.Run(() =>
            {
                while (Console.ReadKey(true).Key != ConsoleKey.Escape) ;

                shouldExit = true;
                relax.Stop();
                timewarp.Stop();
            });

            while (!shouldExit)
            {
                Console.Clear();
                Console.WriteLine("Idling");
                Console.WriteLine("\nPress ESC to return to the main menu.");

                while (!osuManager.CanLoad && !shouldExit)
                    Thread.Sleep(5);

                if (shouldExit)
                    break;

                var beatmap = osuManager.Player.Beatmap;

                Console.Clear();
                Console.WriteLine($"Playing {beatmap.Artist} - {beatmap.Title} ({beatmap.Creator}) [{beatmap.Version}]");
                Console.WriteLine("\nPress ESC to return to the main menu.");

                var relaxTask = Task.Factory.StartNew(() =>
                {
                    if (configManager.EnableRelax && osuManager.Player.CurrentRuleset == Ruleset.Standard)
                        relax.Start(beatmap);
                });

                var timewarpTask = Task.Factory.StartNew(() =>
                {
                    if (configManager.EnableTimewarp)
                        timewarp.Start();
                });

                Task.WaitAll(relaxTask, timewarpTask);
            }

            DrawMainMenu();
        }
    }
}

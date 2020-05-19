using OsuParsers.Beatmaps;
using OsuParsers.Enums;
using System;
using System.Linq;
using System.Numerics;

namespace osu_rx.osu.Memory.Objects
{
    public class OsuPlayer : OsuObject
    {
        public UIntPtr PointerToBaseAddress { get; private set; }

        public override UIntPtr BaseAddress
        {
            get => (UIntPtr)OsuProcess.ReadInt32(PointerToBaseAddress);
            protected set { }
        }

        public OsuRuleset Ruleset
        {
            get => new OsuRuleset((UIntPtr)OsuProcess.ReadInt32(BaseAddress + 0x60));
        }

        public OsuHitObjectManager HitObjectManager
        {
            get => new OsuHitObjectManager((UIntPtr)OsuProcess.ReadInt32(BaseAddress + 0x40));
        }

        public Beatmap Beatmap
        {
            get
            {
                UIntPtr beatmapBase = (UIntPtr)OsuProcess.ReadInt32(BaseAddress + 0xD4);
                var beatmap = new Beatmap();

                int mode = OsuProcess.ReadInt32(beatmapBase + 0x114);
                beatmap.GeneralSection.Mode = (Ruleset)mode;
                beatmap.GeneralSection.ModeId = mode;
                beatmap.MetadataSection.Artist = OsuProcess.ReadString(beatmapBase + 0x18);
                beatmap.MetadataSection.Title = OsuProcess.ReadString(beatmapBase + 0x24);
                beatmap.MetadataSection.Creator = OsuProcess.ReadString(beatmapBase + 0x78);
                beatmap.MetadataSection.Version = OsuProcess.ReadString(beatmapBase + 0xA8);
                beatmap.DifficultySection.ApproachRate = OsuProcess.ReadFloat(beatmapBase + 0x2C);
                beatmap.DifficultySection.CircleSize = OsuProcess.ReadFloat(beatmapBase + 0x30);
                beatmap.DifficultySection.HPDrainRate = OsuProcess.ReadFloat(beatmapBase + 0x34);
                beatmap.DifficultySection.OverallDifficulty = OsuProcess.ReadFloat(beatmapBase + 0x38);
                beatmap.DifficultySection.SliderMultiplier = OsuProcess.ReadDouble(beatmapBase + 0x8);
                beatmap.DifficultySection.SliderTickRate = OsuProcess.ReadDouble(beatmapBase + 0x10);
                beatmap.HitObjects = HitObjectManager.HitObjects.ToList();

                return beatmap;
            }
        }

        public int AudioCheckTime
        {
            get => OsuProcess.ReadInt32(BaseAddress + 0x154);
            set
            {
                OsuProcess.WriteMemory(BaseAddress + 0x154, BitConverter.GetBytes(value), sizeof(int));
                OsuProcess.WriteMemory(BaseAddress + 0x158, BitConverter.GetBytes(value), sizeof(int));
            }
        }

        public OsuPlayer(UIntPtr pointerToBaseAddress) => PointerToBaseAddress = pointerToBaseAddress;
    }
}

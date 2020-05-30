using OsuParsers.Beatmaps;
using OsuParsers.Enums;
using System;
using System.Linq;

namespace osu_rx.osu.Memory.Objects.Player
{
    public class OsuPlayer : OsuObject
    {
        private bool asyncLoadComplete => OsuProcess.ReadBool(BaseAddress + 0x186);

        public override bool IsLoaded => base.IsLoaded && asyncLoadComplete;

        public OsuRuleset Ruleset { get; private set; }

        public OsuHitObjectManager HitObjectManager { get; private set; }

        public OsuPlayer(UIntPtr pointerToBaseAddress) : base(pointerToBaseAddress)
        {
            Children = new OsuObject[]
            {
                Ruleset = new OsuRuleset
                {
                    Offset = 0x60
                },
                HitObjectManager = new OsuHitObjectManager
                {
                    Offset = 0x40
                }
            };
        }

        public Beatmap Beatmap
        {
            get
            {
                UIntPtr beatmapBase = (UIntPtr)OsuProcess.ReadUInt32(BaseAddress + 0xD4);
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

        public bool ReplayMode => OsuProcess.ReadBool(BaseAddress + 0x17E);

        public int AudioCheckCount
        {
            get => OsuProcess.ReadInt32(BaseAddress + 0x150);
            set => OsuProcess.WriteMemory(BaseAddress + 0x150, BitConverter.GetBytes(value), sizeof(int));
        }
    }
}

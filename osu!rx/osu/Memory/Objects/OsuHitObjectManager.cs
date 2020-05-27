using OsuParsers.Beatmaps.Objects;
using OsuParsers.Enums;
using OsuParsers.Enums.Beatmaps;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace osu_rx.osu.Memory.Objects
{
    public class OsuHitObjectManager : OsuObject
    {
        public override bool IsLoaded => base.IsLoaded && HitObjectsCount > 0;

        public Mods CurrentMods
        {
            get
            {
                UIntPtr modsObjectPointer = (UIntPtr)OsuProcess.ReadUInt32(BaseAddress + 0x34);
                int encryptedValue = OsuProcess.ReadInt32(modsObjectPointer + 0x08);
                int decryptionKey = OsuProcess.ReadInt32(modsObjectPointer + 0x0C);

                return (Mods)(encryptedValue ^ decryptionKey);
            }
        }

        public int HitObjectsCount => OsuProcess.ReadInt32(BaseAddress + 0x90);

        public List<HitObject> HitObjects
        {
            get
            {
                var hitObjects = new List<HitObject>();

                UIntPtr hitObjectsListAddress()
                {
                    UIntPtr hitObjectsListPointer = (UIntPtr)OsuProcess.ReadUInt32(BaseAddress + 0x48);

                    return (UIntPtr)OsuProcess.ReadUInt32(hitObjectsListPointer + 0x4);
                }

                UIntPtr hitObjectAddress(int index) => (UIntPtr)OsuProcess.ReadUInt32(hitObjectsListAddress() + 0x8 + 0x4 * index);

                for (int i = 0; i < HitObjectsCount; i++)
                {
                    HitObject hitObject = null;

                    //TODO: expose this enum in osuparsers
                    HitObjectType type = (HitObjectType)OsuProcess.ReadInt32(hitObjectAddress(i) + 0x18);
                    type &= ~HitObjectType.ComboOffset;
                    type &= ~HitObjectType.NewCombo;

                    int startTime = OsuProcess.ReadInt32(hitObjectAddress(i) + 0x10);
                    int endTime = OsuProcess.ReadInt32(hitObjectAddress(i) + 0x14);
                    HitSoundType hitSoundType = (HitSoundType)OsuProcess.ReadInt32(hitObjectAddress(i) + 0x1C);
                    Vector2 position = new Vector2(OsuProcess.ReadFloat(hitObjectAddress(i) + 0x38), OsuProcess.ReadFloat(hitObjectAddress(i) + 0x3C));

                    switch (type)
                    {
                        case HitObjectType.Circle:
                            hitObject = new HitCircle(position, startTime, endTime, hitSoundType, null, false, 0);
                            break;
                        case HitObjectType.Slider:
                            UIntPtr sliderPointsListAddress()
                            {
                                UIntPtr sliderPointsPointer = (UIntPtr)OsuProcess.ReadUInt32(hitObjectAddress(i) + 0xC0);

                                return (UIntPtr)OsuProcess.ReadUInt32(sliderPointsPointer + 0x4);
                            }

                            int sliderPointsCount()
                            {
                                UIntPtr sliderPointsPointer = (UIntPtr)OsuProcess.ReadUInt32(hitObjectAddress(i) + 0xC0);

                                return OsuProcess.ReadInt32(sliderPointsPointer + 0xC);
                            }

                            int repeats = OsuProcess.ReadInt32(hitObjectAddress(i) + 0x20);
                            double pixelLength = OsuProcess.ReadDouble(hitObjectAddress(i) + 0x8);
                            CurveType curveType = (CurveType)OsuProcess.ReadInt32(hitObjectAddress(i) + 0xE8);
                            List<Vector2> sliderPoints = new List<Vector2>();

                            for (int j = 0; j < sliderPointsCount(); j++)
                            {
                                UIntPtr sliderPoint = sliderPointsListAddress() + 0x8 + 0x8 * j;

                                sliderPoints.Add(new Vector2(OsuProcess.ReadFloat(sliderPoint), OsuProcess.ReadFloat(sliderPoint + 0x4)));
                            }

                            hitObject = new Slider(position, startTime, endTime, hitSoundType, curveType, sliderPoints, repeats, pixelLength, false, 0);
                            break;
                        case HitObjectType.Spinner:
                            hitObject = new Spinner(position, startTime, endTime, hitSoundType, null, false, 0);
                            break;
                    }

                    hitObjects.Add(hitObject);
                }

                return hitObjects;
            }
        }
    }

    enum HitObjectType
    {
        Circle = 1 << 0,
        Slider = 1 << 1,
        NewCombo = 1 << 2,
        Spinner = 1 << 3,
        ComboOffset = 1 << 4 | 1 << 5 | 1 << 6,
        Hold = 1 << 7
    }
}
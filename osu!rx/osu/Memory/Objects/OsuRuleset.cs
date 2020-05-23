using System;
using System.Numerics;

namespace osu_rx.osu.Memory.Objects
{
    public class OsuRuleset : OsuObject
    {
        public Vector2 MousePosition
        {
            get
            {
                bool isCtb = OsuProcess.ReadFloat(BaseAddress + 0x50) == 340;
                float x = OsuProcess.ReadFloat(BaseAddress + (isCtb ? 0x4C : 0x7C));
                float y = OsuProcess.ReadFloat(BaseAddress + (isCtb ? 0x50 : 0x80));

                return new Vector2(x, y);
            }
            set
            {
                bool isCtb = OsuProcess.ReadFloat(BaseAddress + 0x50) == 340;
                if (!isCtb)
                    return;

                UIntPtr wank = (UIntPtr)OsuProcess.ReadInt32(BaseAddress + 0xA4);
                UIntPtr catcherAddress = (UIntPtr)OsuProcess.ReadInt32(BaseAddress + 0x8C);

                OsuProcess.WriteMemory(wank + 0x8, BitConverter.GetBytes(value.X), sizeof(float));
                OsuProcess.WriteMemory(wank + 0xC, BitConverter.GetBytes(1), sizeof(int));
                OsuProcess.WriteMemory(catcherAddress + 0x4C, BitConverter.GetBytes(value.X), sizeof(float));
                OsuProcess.WriteMemory(catcherAddress + 0x50, BitConverter.GetBytes(value.Y), sizeof(float));
            }
        }
    }
}

using System;
using System.Collections.Generic;
using WindowsInput.Native;

namespace osu_rx.osu.Memory.Objects.Bindings
{
    public class BindingManager : OsuObject
    {
        public Dictionary<Bindings, VirtualKeyCode> BindingDictionary
        {
            get
            {
                var bindingDictionary = new Dictionary<Bindings, VirtualKeyCode>();

                UIntPtr items = (UIntPtr)OsuProcess.ReadUInt32(BaseAddress + 0x8);
                int dictionaryLength = OsuProcess.ReadInt32(BaseAddress + 0x1C);
                for (int i = 0; i < dictionaryLength; i++)
                {
                    UIntPtr currentItem = items + 0x8 + 0x8 * i;
                    var key = (Bindings)OsuProcess.ReadInt32(currentItem);
                    var value = (VirtualKeyCode)OsuProcess.ReadInt32(currentItem + 0x4);

                    bindingDictionary[key] = value;
                }

                return bindingDictionary;
            }
        }

        public VirtualKeyCode GetKey(Bindings binding)
        {
            VirtualKeyCode key;
            if (!BindingDictionary.TryGetValue(binding, out key))
                return VirtualKeyCode.ESCAPE;

            return key;
        }

        public BindingManager(UIntPtr pointerToBaseAddress) : base(pointerToBaseAddress) { }
    }
}

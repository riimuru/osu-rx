using osu_rx.Dependencies;
using System;
using System.Collections.Generic;
using System.Linq;

namespace osu_rx.osu.Memory.Objects
{
    public abstract class OsuObject
    {
        protected OsuProcess OsuProcess;

        private UIntPtr? pointerToBaseAddress;
        public UIntPtr BaseAddress
        {
            get
            {
                if (pointerToBaseAddress.HasValue)
                    return (UIntPtr)OsuProcess.ReadInt32(pointerToBaseAddress.Value);

                if (Parent.BaseAddress != UIntPtr.Zero)
                    return (UIntPtr)OsuProcess.ReadInt32(Parent.BaseAddress + Offset);

                return UIntPtr.Zero;
            }
        }

        public int Offset;

        public virtual bool IsLoaded => BaseAddress != UIntPtr.Zero && Children.All(child => child.IsLoaded);

        public OsuObject Parent { get; set; } = null;

        private List<OsuObject> children = new List<OsuObject>();
        public OsuObject[] Children
        {
            get => children.ToArray();
            set
            {
                children = value.ToList();

                foreach (var child in children)
                    child.Parent = this;
            }
        }

        public OsuObject(UIntPtr? pointerToBaseAddress = null)
        {
            this.pointerToBaseAddress = pointerToBaseAddress;
            OsuProcess = DependencyContainer.Get<OsuProcess>();
        }

        public void Add(OsuObject osuObject)
        {
            osuObject.Parent = this;
            children.Add(osuObject);
        }
    }
}

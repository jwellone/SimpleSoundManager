#nullable enable

namespace jwellone
{
    public struct SoundHandle
    {
        public static readonly SoundHandle None = new SoundHandle(0, 0);

        public readonly int groupHashCode;
        public readonly uint id;

        public SoundHandle(int groupHashCode, uint id)
        {
            this.groupHashCode = groupHashCode;
            this.id = id;
        }

        public override int GetHashCode() => base.GetHashCode();

        public override bool Equals(object obj)
        {
            if (obj == null || this.GetType() != obj.GetType())
            {
                return false;
            }

            var target = (SoundHandle)obj;
            return target == this;
        }

        public static bool operator ==(SoundHandle a, SoundHandle b) => a.groupHashCode == b.groupHashCode && a.id == b.id;
        public static bool operator !=(SoundHandle a, SoundHandle b) => a.groupHashCode != b.groupHashCode || a.id != b.id;
    }
}
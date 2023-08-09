using robotManager.Helpful;
using wManager.Wow;
using wManager.Wow.Class;
using wManager.Wow.ObjectManager;
using wManager.Wow.Patchables;

namespace WholesomeDungeonCrawler.Managers.AvoidAOEHelpers
{
    internal class DynamicObject : WoWObject
    {
        public DynamicObject(uint address) : base(address) { }

        public override Vector3 Position =>
            new Vector3(Memory.WowMemory.Memory.ReadFloat(BaseAddress + 0xE8),
                Memory.WowMemory.Memory.ReadFloat(BaseAddress + 0xEC),
                Memory.WowMemory.Memory.ReadFloat(BaseAddress + 0xF0));

        public override string Name => new Spell(SpellID).Name;
        public override float GetDistance => Position.DistanceTo(ObjectManager.Me.PositionWithoutType);
        public ulong Caster =>
            Memory.WowMemory.Memory.ReadUInt64(GetDescriptorAddress((uint)Descriptors.DynamicObjectFields.Caster));
        public int SpellID =>
            Memory.WowMemory.Memory.ReadInt32(GetDescriptorAddress((uint)Descriptors.DynamicObjectFields.SpellID));
        public float Radius =>
            Memory.WowMemory.Memory.ReadFloat(GetDescriptorAddress((uint)Descriptors.DynamicObjectFields.Radius));
        public int CastTime =>
            Memory.WowMemory.Memory.ReadInt32(GetDescriptorAddress((uint)Descriptors.DynamicObjectFields.CastTime));
    }
}

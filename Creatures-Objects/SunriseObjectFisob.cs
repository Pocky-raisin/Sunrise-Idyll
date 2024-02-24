using Fisobs.Core;
using Fisobs.Items;
using Fisobs.Properties;
using Fisobs.Sandbox;
using RWCustom;

namespace Sunrise
{
    namespace Objects
    {
        public class WarmSlimeFisob : Fisob
        {
            public static readonly AbstractPhysicalObject.AbstractObjectType WarmSlime = new("WarmSlime", true);
            public static readonly MultiplayerUnlocks.SandboxUnlockID UnlockWarmSlime = new("UnlockWarmSlime", true);

            public WarmSlimeFisob() : base(WarmSlime)
            {
                Icon = new SimpleIcon("Symbol_SlimeMold", Custom.hexToColor("FF4500"));
                SandboxPerformanceCost = new(linear: 0.1f, exponential: 0f);
                RegisterUnlock(UnlockWarmSlime, parent: MultiplayerUnlocks.SandboxUnlockID.Slugcat);
            }
            public static readonly WarmSlimeProperties properties = new();
            public override ItemProperties Properties(PhysicalObject forObject)
            {
                return properties;
            }

            public override AbstractPhysicalObject Parse(World world, EntitySaveData entitySaveData, SandboxUnlock unlock)
            {
                return new WarmSlimeAbstract(world, entitySaveData.Pos, entitySaveData.ID, Custom.hexToColor("FF4500"));
            }
        }
    }
}
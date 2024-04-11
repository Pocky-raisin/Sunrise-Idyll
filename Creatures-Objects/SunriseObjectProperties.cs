using Fisobs.Properties;
using System.Linq;

namespace SunriseIdyll
{
    namespace Objects
    {
        public class WarmSlimeProperties : ItemProperties
        {
            public override void ScavCollectScore(Scavenger scavenger, ref int score)
                => score = 5;

            public override void ScavWeaponPickupScore(Scavenger scav, ref int score)
                => score = 3;

            public override void ScavWeaponUseScore(Scavenger scav, ref int score)
                => score = 0;
            // Player stuff
            public override void Grabability(Player player, ref Player.ObjectGrabability grabability)
            {
                grabability = Player.ObjectGrabability.OneHand;
            }
        }
    }
}
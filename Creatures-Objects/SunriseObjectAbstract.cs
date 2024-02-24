using Fisobs.Core;
using System;
using UnityEngine;
using Random = UnityEngine.Random;


namespace Sunrise
{
    namespace Objects
    {
        public class WarmSlimeAbstract : AbstractPhysicalObject
        {
            public int MaxLifetime;
            public int Lifetime;
            public Color colour;

            public WarmSlimeAbstract(World world, WorldCoordinate pos, EntityID ID, Color col) : base(world, WarmSlimeFisob.WarmSlime, null, pos, ID)
            {
                Lifetime = 0;
                MaxLifetime = Mathf.RoundToInt(this.world.rainCycle.cycleLength / Random.value > 0.5f? 4 : 5);
                colour = col;
            }
            public override void Realize()
            {
                base.Realize();
                if (realizedObject == null) realizedObject = new WarmSlime(this, Room.realizedRoom.MiddleOfTile(pos.Tile), Vector2.zero);
            }

            public override void Update(int time)
            {
                base.Update(time);
                Lifetime++;
                if (Lifetime > MaxLifetime) Lifetime = MaxLifetime;
            }
        }
    }
}
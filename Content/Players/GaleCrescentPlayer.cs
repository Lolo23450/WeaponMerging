using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace WeaponMerging.Content.Players
{
    public class GaleCrescentPlayer : ModPlayer
    {
        public int galeStacks;
        public int galeDecayTimer;
        private const int MaxStacks = 3;
        private const int DecayDuration = 120; 

        public override void PostUpdate()
        {
            if (galeStacks > 0)
            {
                galeDecayTimer--;
                if (galeDecayTimer <= 0)
                {
                    galeStacks = 0;
                }
            }
        }

        public void AddStack()
        {
            galeStacks = System.Math.Min(MaxStacks, galeStacks + 1);
            galeDecayTimer = DecayDuration;

            
            for (int i = 0; i < 6; i++)
            {
                var v = Main.rand.NextVector2Circular(1.6f, 1.6f);
                var d = Dust.NewDustPerfect(Player.MountedCenter + v * 4f, Terraria.ID.DustID.Clentaminator_Blue, v, 140, new Color(170, 220, 255), 1.05f);
                d.noGravity = true;
            }
        }

        public void ResetStacksVisual()
        {
            
            for (int i = 0; i < 18; i++)
            {
                var v = Main.rand.NextVector2Circular(3.2f, 3.2f);
                var d = Dust.NewDustPerfect(Player.MountedCenter, Terraria.ID.DustID.Clentaminator_Blue, v, 110, new Color(180, 235, 255), Main.rand.NextFloat(1.0f, 1.4f));
                d.noGravity = true;
            }
        }
    }
}

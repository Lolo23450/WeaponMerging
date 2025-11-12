using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.Audio;
using Terraria.DataStructures;

namespace WeaponMerging.Content.Projectiles
{
    public class FusionCraftEffect : ModProjectile
    {
        public override string Texture => "Terraria/Images/Item_0"; 

        internal const int DurationTicks = 300; 
        internal const float DurationSeconds = DurationTicks / 60f;

        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.timeLeft = DurationTicks;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.hide = true;
        }

        public override void OnSpawn(IEntitySource source)
        {
            Vector2 center = Projectile.Center;
            
            SoundEngine.PlaySound(SoundID.Research with { Volume = 0.7f }, center);
            
            for (int k = 0; k < 4; k++)
            {
                Vector2 vel = Vector2.UnitX.RotatedBy(MathHelper.TwoPi * k / 4f) * 0.5f;
                int d = Dust.NewDust(center, 1, 1, DustID.Cloud, vel.X, vel.Y, 30, new Color(120, 120, 120), 0.3f);
                Main.dust[d].noGravity = true;
            }
        }

        public override bool ShouldUpdatePosition() => false;

        public override bool PreDraw(ref Color lightColor) => false;
    }
}


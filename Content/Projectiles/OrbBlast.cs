using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponMerging.Content.Projectiles
{
    public class OrbBlast : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = false;
            Projectile.hostile = true;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 300;
            Projectile.aiStyle = 0;
            Projectile.tileCollide = true;
            Projectile.light = 0.5f;
        }

        public override void AI()
        {
            // Glow and trail effect
            for (int i = 0; i < 2; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Stone);
                dust.velocity *= 0.3f;
                dust.noGravity = true;
            }

            // Homing behavior (slight)
            if (Projectile.ai[0] == 0)
            {
                Projectile.ai[0] = 1;
                Projectile.velocity = Projectile.velocity.RotatedByRandom(MathHelper.ToRadians(10));
            }

            Projectile.rotation += 0.1f;
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            target.AddBuff(BuffID.Confused, 60);
        }

        public override void Kill(int timeLeft)
        {
            for (int i = 0; i < 10; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Stone);
                dust.velocity *= 2f;
            }
        }
    }
}

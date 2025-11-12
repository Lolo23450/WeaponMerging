using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponMerging.Content.Projectiles
{
    public class ChaosBoltProjectile : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 90;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = false;
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            
            if (Main.rand.NextBool(2))
            {
                int dustType = Main.rand.Next(new int[] { 6, 27, 113, 173 });
                Dust d = Dust.NewDustPerfect(Projectile.Center, dustType, Vector2.Zero, 100, default, 1.0f);
                d.noGravity = true;
            }

            Lighting.AddLight(Projectile.Center, new Vector3(0.8f, 0.4f, 0.6f));
        }

        public override void Kill(int timeLeft)
        {
            for (int i = 0; i < 8; i++)
            {
                int dustType = Main.rand.Next(new int[] { 6, 27, 113 });
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, dustType,
                    Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-2f, 2f));
            }
        }
    }
}


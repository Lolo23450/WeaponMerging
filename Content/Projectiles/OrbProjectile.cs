using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponMerging.Content.Projectiles
{
    public class OrbProjectile : ModProjectile
    {
        private const float MaxSpeed = 11f;
        private const float HomingLerp = 0.15f;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 6;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = 18;
            Projectile.height = 18;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void AI()
        {
            Lighting.AddLight(Projectile.Center, 0.1f, 0.2f, 0.35f);

            NPC target = FindTarget(Projectile.Center, 480f);
            if (target != null)
            {
                Vector2 desiredVelocity = Projectile.DirectionTo(target.Center) * MaxSpeed;
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, desiredVelocity, HomingLerp);
            }

            Projectile.rotation = Projectile.velocity.ToRotation()
                + MathHelper.PiOver2;

            if (Main.rand.NextBool(3))
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.BlueCrystalShard, -Projectile.velocity * 0.2f, 120, default, 1.1f);
                dust.noGravity = true;
            }
        }

        private NPC FindTarget(Vector2 position, float range)
        {
            NPC best = null;
            float bestDistance = range;
            foreach (NPC npc in Main.npc)
            {
                if (!npc.active || npc.friendly || npc.dontTakeDamage || npc.life <= 0)
                    continue;

                float distance = Vector2.Distance(position, npc.Center);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = npc;
                }
            }

            return best;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            TriggerExplosion();
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            TriggerExplosion();
            return true;
        }

        private void TriggerExplosion()
        {
            if (Projectile.localAI[0] != 0f)
                return;

            Projectile.localAI[0] = 1f;

            if (Main.myPlayer == Projectile.owner && Main.rand.NextBool(3))
            {
                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    Projectile.Center,
                    Vector2.Zero,
                    ProjectileID.DD2ExplosiveTrapT1Explosion,
                    Projectile.damage,
                    Projectile.knockBack,
                    Projectile.owner);
            }

            SoundEngine.PlaySound(SoundID.Item94 with { Volume = 0.7f, Pitch = -0.2f }, Projectile.Center);

            for (int i = 0; i < 12; i++)
            {
                Vector2 velocity = Main.rand.NextVector2Circular(3f, 3f);
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.BlueFairy, velocity, 150, default, 1.2f);
                dust.noGravity = true;
            }

            Projectile.Kill();
        }
    }
}


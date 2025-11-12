using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using System;

namespace WeaponMerging.Content.Projectiles
{
    public class PainOrbProjectile : ModProjectile
    {
        private const float ORBIT_RADIUS = 48f;
        private const float ORBIT_SPEED = 0.25f;
        private const int MAX_LIFETIME = 600;
        private const int ORB_FIRE_INTERVAL = 32;
        private const float ORB_TARGET_RANGE = 700f;
        private const float FUSION_RADIUS = 32f;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 6;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.penetrate = 1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = MAX_LIFETIME;
            Projectile.DamageType = DamageClass.Magic;
        }

        public override void AI()
        {
            
            if (Projectile.ai[1] == 0f) 
            {
                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    Projectile other = Main.projectile[i];
                    if (other.active && other.type == ModContent.ProjectileType<ShadowOrbProjectile>() && other.owner == Projectile.owner && other.ai[1] == 0f)
                    {
                        if (Vector2.Distance(Projectile.Center, other.Center) < FUSION_RADIUS)
                        {
                            
                            if (Main.myPlayer == Projectile.owner)
                            {
                                Vector2 fusionPos = (Projectile.Center + other.Center) / 2f;
                                Player ownerPlayer = Main.player[Projectile.owner];
                                int heldDmg = ownerPlayer?.HeldItem?.damage ?? 0;
                                int baseDmg = Math.Max(Projectile.damage, other.damage);
                                if (baseDmg <= 0)
                                    baseDmg = (int)(heldDmg * 0.75f);

                                Projectile fusion = Projectile.NewProjectileDirect(
                                    Projectile.GetSource_FromThis(),
                                    fusionPos,
                                    Vector2.Zero,
                                    ModContent.ProjectileType<FusionOrbProjectile>(),
                                    Math.Max(20, baseDmg * 2),
                                    1f,
                                    Projectile.owner
                                );
                                fusion.ai[0] = 0f; 
                                fusion.localAI[0] = 0f; 
                                fusion.netUpdate = true;
                            }

                            
                            Projectile.Kill();
                            other.Kill();
                            return;
                        }
                    }
                }
            }

            
            if (Projectile.ai[1] == 1f)
            {
                NPC releasedTarget = FindNearestTarget(Projectile.Center, 600f);
                if (releasedTarget != null)
                {
                    Vector2 toTarget = releasedTarget.Center - Projectile.Center;
                    toTarget.Normalize();
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * 10f, 0.08f);
                }

                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

                if (Main.rand.NextBool(4))
                {
                    Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(4f, 4f), 113);
                    d.noGravity = true;
                    d.scale = 1.0f + Main.rand.NextFloat(0.3f);
                }

                return;
            }

            
            var modPlayer = Main.player[Projectile.owner].GetModPlayer<Content.Players.PainModPlayer>();
            int orbCount = modPlayer?.orbCount ?? 1;
            if (orbCount < 1) orbCount = 1;

            float index = Projectile.localAI[0];
            float globalAngle = Main.GlobalTimeWrappedHourly * ORBIT_SPEED * 2f;
            float myAngle = globalAngle + (MathHelper.TwoPi * index / orbCount);

            Vector2 orbitTarget = Main.player[Projectile.owner].Center + myAngle.ToRotationVector2() * (ORBIT_RADIUS + orbCount * 4f);
            Vector2 dirToTarget = orbitTarget - Projectile.Center;

            Projectile.velocity = dirToTarget * 0.2f;

            Lighting.AddLight(Projectile.Center, new Vector3(0.25f, 1f, 0.35f));
            Projectile.rotation += 0.24f;

            if (Main.rand.NextBool(6))
            {
                Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(4f, 4f), 113);
                d.noGravity = true;
                d.scale = 1.05f;
            }
        }

        private NPC FindNearestTarget(Vector2 position, float maxRange)
        {
            NPC best = null;
            float bestDist = maxRange;
            foreach (NPC npc in Main.npc)
            {
                if (npc.active && !npc.friendly && npc.life > 0 && !npc.dontTakeDamage)
                {
                    float d = Vector2.Distance(position, npc.Center);
                    if (d < bestDist)
                    {
                        bestDist = d;
                        best = npc;
                    }
                }
            }
            return best;
        }

        public override void Kill(int timeLeft)
        {
            for (int i = 0; i < 12; i++)
            {
                var d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, 113, 
                    Main.rand.NextFloat(-3, 3), Main.rand.NextFloat(-3, 3));
                d.noGravity = true;
                d.scale = 1.3f;
            }

            var modPlayer = Main.player[Projectile.owner].GetModPlayer<Content.Players.PainModPlayer>();
            modPlayer.orbCount = modPlayer.orbCount - 1;
            SoundEngine.PlaySound(SoundID.Item14 with { Volume = 0.5f, Pitch = 0.15f }, Projectile.Center);
        }

        public override bool? CanHitNPC(NPC target) =>
            Projectile.ai[1] == 1f ? true : false;
    }
}

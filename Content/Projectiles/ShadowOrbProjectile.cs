using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System;

namespace WeaponMerging.Content.Projectiles
{
    public class ShadowOrbProjectile : ModProjectile
    {
        private const float ORBIT_RADIUS = 55f;
        private const float ORBIT_SPEED = 0.25f;
        private const float FUSION_RADIUS = 32f;
        private int trailCounter = 0;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.friendly = false;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 99999;
            Projectile.DamageType = DamageClass.Magic;
        }

        public override void AI()
        {
            
            if (Projectile.ai[1] == 0f) 
            {
                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    Projectile other = Main.projectile[i];
                    if (other.active && other.type == ModContent.ProjectileType<PainOrbProjectile>() && other.owner == Projectile.owner && other.ai[1] == 0f)
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

            if (Projectile.owner < 0 || Projectile.owner >= Main.maxPlayers)
            {
                Projectile.Kill();
                return;
            }

            Player owner = Main.player[Projectile.owner];

            var modPlayer = owner.GetModPlayer<Content.Players.ShadowReaperPlayer>();
            int orbCount = modPlayer?.orbCount ?? 1;
            if (orbCount < 1) orbCount = 1;

            var accessoryPlayer = owner.GetModPlayer<Content.Players.AccessoryEffectsPlayer>();
            float speedMult = accessoryPlayer.orbSpeedMultipliers.TryGetValue("Shadow", out float mult) ? mult : 1f;

            float index = Projectile.localAI[0];
            float globalAngle = Main.GlobalTimeWrappedHourly * ORBIT_SPEED * 2f * speedMult;
            float myAngle = globalAngle + (MathHelper.TwoPi * index / orbCount);

            Vector2 orbitTarget = owner.Center + myAngle.ToRotationVector2() * (ORBIT_RADIUS + orbCount * 5f);
            Projectile.velocity = (orbitTarget - Projectile.Center) * 0.25f;

            Lighting.AddLight(Projectile.Center, new Vector3(0.6f, 0.2f, 0.8f));
            Projectile.rotation += 0.2f;

            trailCounter++;

            
            if (trailCounter % 2 == 0)
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, 27,
                    -Projectile.velocity * 0.3f, 100, Color.Purple, Main.rand.NextFloat(1.0f, 1.5f));
                dust.noGravity = true;
                dust.alpha = 150;
            }

            
            if (Main.rand.NextBool(5))
            {
                Dust sparkle = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(10, 10),
                    173, Vector2.Zero, 120, Color.MediumPurple, Main.rand.NextFloat(0.8f, 1.3f));
                sparkle.noGravity = true;
            }
        }
    }
}


using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using System;

namespace WeaponMerging.Content.Projectiles
{
    public class FusionOrbProjectile : ModProjectile
    {
        private const float ORBIT_RADIUS = 60f;
        private const float ORBIT_SPEED = 0.25f;
        private const int MAX_LIFETIME = 200;
        private float pulseTimer = 0f;
        private int teleportCooldown = 0;
        private int teleportTrailTime = 0;
        private Vector2 lastTeleportPos = Vector2.Zero;

        
        private static readonly Color[] GalaxyColors = new Color[]
        {
            new Color(180, 80, 255),   
            new Color(80, 180, 255),   
            new Color(255, 80, 200),   
            new Color(120, 60, 180),   
            new Color(80, 255, 220),   
        };

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 20;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 36;
            Projectile.height = 36;
            Projectile.friendly = true;
            Projectile.penetrate = 3;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = MAX_LIFETIME;
            Projectile.DamageType = DamageClass.Magic;
        }

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            if (!owner.active)
            {
                Projectile.Kill();
                return;
            }

            pulseTimer += 0.13f;

            
            if (Projectile.ai[1] == 0f)
            {
                float globalAngle = Main.GlobalTimeWrappedHourly * ORBIT_SPEED * 2f;
                float wave = (float)Math.Sin(Main.GlobalTimeWrappedHourly * 2f + Projectile.whoAmI) * 0.25f;
                float myAngle = globalAngle + wave;

                float pulse = 1f + (float)Math.Sin(pulseTimer) * 0.18f;
                float orbitRadius = ORBIT_RADIUS + (float)Math.Sin(Main.GlobalTimeWrappedHourly * 3f + Projectile.whoAmI) * 10f;

                Vector2 orbitTarget = owner.Center + myAngle.ToRotationVector2() * orbitRadius;
                Projectile.velocity = (orbitTarget - Projectile.Center) * 0.18f;

                Lighting.AddLight(Projectile.Center, new Vector3(0.5f, 0.3f, 0.8f) * pulse);
                Projectile.rotation += 0.18f;
                Projectile.scale = 1f * pulse;

                
                if (Main.rand.NextBool(2))
                {
                    float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                    float dist = Main.rand.NextFloat(18f, 32f);
                    Color dustCol = GalaxyColors[Main.rand.Next(GalaxyColors.Length)];
                    Dust d = Dust.NewDustPerfect(Projectile.Center + angle.ToRotationVector2() * dist, 267, angle.ToRotationVector2() * 0.5f, 100, dustCol, Main.rand.NextFloat(1.1f, 1.5f));
                    d.noGravity = true;
                }

                
                if (Main.myPlayer == Projectile.owner && Main.rand.NextBool(60))
                {
                    float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                    Vector2 spawnPos = Projectile.Center + angle.ToRotationVector2() * 44f;
                    Vector2 vel = (Projectile.Center - spawnPos).SafeNormalize(Vector2.UnitY).RotatedByRandom(0.4f) * 1.5f;
                    int deco = Projectile.NewProjectile(
                        Projectile.GetSource_FromThis(),
                        spawnPos,
                        vel,
                        617,
                        0, 0f, Projectile.owner
                    );
                    if (deco >= 0 && deco < Main.maxProjectiles)
                    {
                        Main.projectile[deco].timeLeft = 30;
                        Main.projectile[deco].scale = 0.7f;
                        Main.projectile[deco].friendly = false;
                        Main.projectile[deco].hostile = false;
                        Main.projectile[deco].alpha = 180;
                    }
                }
            }
            else 
            {
                NPC target = FindNearestTarget(Projectile.Center, 900f);
                if (target != null)
                {
                    Vector2 toTarget = target.Center - Projectile.Center;
                    toTarget.Normalize();
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * 7f, 0.13f);

                    
                    if (teleportCooldown <= 0 && Vector2.Distance(Projectile.Center, target.Center) > 100f && Vector2.Distance(Projectile.Center, target.Center) < 700f)
                    {
                        Vector2 dir = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitY);
                        lastTeleportPos = Projectile.Center;
                        Projectile.Center += dir * 160f;
                        teleportCooldown = 22;
                        teleportTrailTime = 14;

                        
                        for (int i = 0; i < 14; i++)
                        {
                            float angle = MathHelper.TwoPi * i / 14f;
                            Vector2 dustVel = angle.ToRotationVector2() * 5f;
                            Color dustCol = GalaxyColors[i % GalaxyColors.Length];
                            Dust d1 = Dust.NewDustPerfect(lastTeleportPos, 267, dustVel, 100, dustCol, 1.5f);
                            d1.noGravity = true;
                            Dust d2 = Dust.NewDustPerfect(Projectile.Center, 267, dustVel, 100, dustCol, 1.5f);
                            d2.noGravity = true;
                        }
                        
                        for (int i = 0; i < 10; i++)
                        {
                            Dust d = Dust.NewDustPerfect(Projectile.Center, 267, Main.rand.NextVector2Circular(12f, 12f), 100, Color.White, 2.2f);
                            d.noGravity = true;
                        }
                        SoundEngine.PlaySound(SoundID.Item117 with { Volume = 0.8f, Pitch = 0.5f }, Projectile.Center);
                    }
                    else
                    {
                        teleportCooldown--;
                    }
                }
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

                float pulse = 1f + (float)Math.Sin(pulseTimer) * 0.22f;
                Projectile.scale = 1f * pulse;

                
                if (Main.rand.NextBool(2))
                {
                    Color dustCol = GalaxyColors[Main.rand.Next(GalaxyColors.Length)];
                    Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(8f, 8f), 267, Vector2.Zero, 100, dustCol, 1.3f * pulse);
                    d.noGravity = true;
                }

                
                if (teleportTrailTime > 0)
                {
                    teleportTrailTime--;
                    for (int i = 0; i < 8; i++)
                    {
                        float t = i / 8f;
                        Vector2 pos = Vector2.Lerp(lastTeleportPos, Projectile.Center, t);
                        Color col = GalaxyColors[i % GalaxyColors.Length] * (0.7f - t * 0.5f);
                        Dust d = Dust.NewDustPerfect(pos, 267, Vector2.Zero, 100, col, 1.3f - t * 0.5f);
                        d.noGravity = true;
                    }
                }
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

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            
            target.AddBuff(BuffID.Poisoned, 240);
            target.AddBuff(BuffID.ShadowFlame, 240);

            
            for (int i = 0; i < 20; i++)
            {
                Color dustCol = GalaxyColors[i % GalaxyColors.Length];
                Dust d = Dust.NewDustPerfect(Projectile.Center, 267, Main.rand.NextVector2Circular(7f, 7f), 100, dustCol, 1.8f);
                d.noGravity = true;
            }
            for (int i = 0; i < 10; i++)
            {
                Dust d = Dust.NewDustPerfect(Projectile.Center, 267, Main.rand.NextVector2Circular(10f, 10f), 100, Color.White, 2.2f);
                d.noGravity = true;
            }
            SoundEngine.PlaySound(SoundID.Item74 with { Volume = 0.9f, Pitch = 0.3f }, Projectile.Center);

            
            if (Projectile.penetrate > 1 && Main.myPlayer == Projectile.owner)
            {
                for (int i = 0; i < 2; i++)
                {
                    Vector2 vel = Main.rand.NextVector2Circular(8f, 8f);
                    int p = Projectile.NewProjectile(
                        Projectile.GetSource_FromThis(),
                        Projectile.Center,
                        vel,
                        ModContent.ProjectileType<FusionOrbProjectile>(),
                        (int)(Projectile.damage * 0.8f),
                        Projectile.knockBack * 0.7f,
                        Projectile.owner,
                        0f,
                        1f 
                    );
                    if (p >= 0 && p < Main.maxProjectiles)
                    {
                        Main.projectile[p].scale = 0.7f;
                        Main.projectile[p].timeLeft = 120;
                    }
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = ModContent.Request<Texture2D>("WeaponMerging/Content/Projectiles/FusionOrbProjectile").Value;
            Vector2 origin = tex.Size() / 2f;

            
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                float progress = i / (float)Projectile.oldPos.Length;
                float alpha = (1f - progress) * 0.8f;
                float pulse = 1f + (float)Math.Sin(pulseTimer - i * 0.2f) * 0.22f;
                Color trailColor = GalaxyColors[i % GalaxyColors.Length] * alpha;

                Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;

                Main.EntitySpriteDraw(tex, trailPos, null, trailColor, Projectile.oldRot[i],
                    origin, Projectile.scale * (1f - progress * 0.2f) * pulse, SpriteEffects.None, 0);

                
                if (i % 4 == 0)
                {
                    Main.EntitySpriteDraw(tex, trailPos, null, trailColor * 0.5f, Projectile.oldRot[i],
                        origin, Projectile.scale * (1.1f - progress * 0.15f) * pulse, SpriteEffects.None, 0);
                }
            }

            
            Color mainColor = GalaxyColors[(int)(pulseTimer * 2) % GalaxyColors.Length];
            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null,
                mainColor, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);

            
            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null,
                mainColor * 0.7f, Projectile.rotation, origin, Projectile.scale * 1.45f, SpriteEffects.None, 0);

            
            if (teleportTrailTime > 0)
            {
                float flash = teleportTrailTime / 14f;
                Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null,
                    Color.White * flash, Projectile.rotation, origin, Projectile.scale * (1.3f + flash), SpriteEffects.None, 0);
            }

            return false;
        }
    }
}


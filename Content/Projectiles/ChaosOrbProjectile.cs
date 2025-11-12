using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using System;

namespace WeaponMerging.Content.Projectiles
{
    public class ChaosOrbProjectile : ModProjectile
    {
        private const float ORBIT_RADIUS = 68f;
        private const float ORBIT_SPEED = 0.22f;
        private const int MAX_LIFETIME = 240;
        private float pulseTimer = 0f;
        private int chaosBurstTimer = 0;

        private static readonly Color[] ChaosColors = new Color[]
        {
            new Color(255, 100, 0),    
            new Color(180, 80, 255),   
            new Color(80, 255, 140),   
            Color.Red,
            Color.Cyan
        };

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 16;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.friendly = true;
            Projectile.penetrate = 5;
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

            pulseTimer += 0.16f;
            chaosBurstTimer++;

            
            if (Projectile.ai[1] == 0f)
            {
                float globalAngle = Main.GlobalTimeWrappedHourly * ORBIT_SPEED * 2f;
                float chaos = (float)Math.Sin(Main.GlobalTimeWrappedHourly * 5f + Projectile.whoAmI) * 0.4f;
                float myAngle = globalAngle + chaos;

                float pulse = 1f + (float)Math.Sin(pulseTimer) * 0.25f;
                float orbitRadius = ORBIT_RADIUS + (float)Math.Sin(Main.GlobalTimeWrappedHourly * 4f) * 15f;

                Vector2 orbitTarget = owner.Center + myAngle.ToRotationVector2() * orbitRadius;
                Projectile.velocity = (orbitTarget - Projectile.Center) * 0.22f;

                Lighting.AddLight(Projectile.Center, new Vector3(0.8f, 0.4f, 0.6f) * pulse);
                Projectile.rotation += 0.22f;
                Projectile.scale = 1.1f * pulse;

                
                if (Main.rand.NextBool(2))
                {
                    Color dustCol = ChaosColors[Main.rand.Next(ChaosColors.Length)];
                    Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(20f, 20f),
                        Main.rand.Next(new int[] { 6, 27, 113, 173, 267 }),
                        Main.rand.NextVector2Circular(2f, 2f), 100, dustCol, Main.rand.NextFloat(1.2f, 1.8f));
                    d.noGravity = true;
                }

                
                if (chaosBurstTimer >= 35 && Main.myPlayer == Projectile.owner)
                {
                    chaosBurstTimer = 0;
                    NPC target = FindNearestTarget(Projectile.Center, 600f);
                    if (target != null)
                    {
                        Vector2 vel = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX) * 12f;
                        int bolt = Projectile.NewProjectile(
                            Projectile.GetSource_FromThis(),
                            Projectile.Center,
                            vel,
                            ModContent.ProjectileType<ChaosBoltProjectile>(),
                            (int)(Projectile.damage * 0.6f),
                            Projectile.knockBack * 0.7f,
                            Projectile.owner
                        );
                    }
                }
            }
            else 
            {
                NPC target = FindNearestTarget(Projectile.Center, 800f);
                if (target != null)
                {
                    Vector2 toTarget = target.Center - Projectile.Center;
                    toTarget.Normalize();
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * 10f, 0.15f);
                }

                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

                
                if (Main.rand.NextBool(2))
                {
                    Color dustCol = ChaosColors[Main.rand.Next(ChaosColors.Length)];
                    Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(6f, 6f),
                        Main.rand.Next(new int[] { 6, 27, 113 }), Vector2.Zero, 100, dustCol, 1.5f);
                    d.noGravity = true;
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
            
            target.AddBuff(BuffID.OnFire, 300);
            target.AddBuff(BuffID.ShadowFlame, 240);
            target.AddBuff(BuffID.Poisoned, 240);

            
            for (int i = 0; i < 30; i++)
            {
                Color dustCol = ChaosColors[i % ChaosColors.Length];
                Dust d = Dust.NewDustPerfect(Projectile.Center, Main.rand.Next(new int[] { 6, 27, 113, 173 }),
                    Main.rand.NextVector2Circular(9f, 9f), 100, dustCol, 2.0f);
                d.noGravity = true;
            }

            SoundEngine.PlaySound(SoundID.Item74 with { Volume = 1.0f, Pitch = -0.2f }, Projectile.Center);

            
            if (Main.myPlayer == Projectile.owner)
            {
                for (int i = 0; i < 3; i++)
                {
                    Vector2 vel = Main.rand.NextVector2CircularEdge(10f, 10f);
                    Projectile.NewProjectile(
                        Projectile.GetSource_FromThis(),
                        Projectile.Center,
                        vel,
                        ModContent.ProjectileType<ChaosBoltProjectile>(),
                        (int)(Projectile.damage * 0.4f),
                        Projectile.knockBack * 0.5f,
                        Projectile.owner
                    );
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = ModContent.Request<Texture2D>("WeaponMerging/Content/Projectiles/ChaosOrbProjectile").Value;
            Vector2 origin = tex.Size() / 2f;

            
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                float progress = i / (float)Projectile.oldPos.Length;
                float alpha = (1f - progress) * 0.7f;
                Color trailColor = ChaosColors[i % ChaosColors.Length] * alpha;

                Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                Main.EntitySpriteDraw(tex, trailPos, null, trailColor, Projectile.oldRot[i],
                    origin, Projectile.scale * (1f - progress * 0.3f), SpriteEffects.None, 0);
            }

            
            Color mainColor = ChaosColors[(int)(pulseTimer * 3) % ChaosColors.Length];
            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null,
                mainColor, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);

            
            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null,
                mainColor * 0.6f, -Projectile.rotation, origin, Projectile.scale * 1.5f, SpriteEffects.None, 0);

            return false;
        }
    }
}


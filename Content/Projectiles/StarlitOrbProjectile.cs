using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using System;

namespace WeaponMerging.Content.Projectiles
{
    public class StarlitOrbProjectile : ModProjectile
    {
        private const float ORBIT_RADIUS = 45f;
        private const float ORBIT_SPEED = 0.22f;
        private const int MAX_LIFETIME = 600;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
            Main.projFrames[Projectile.type] = 1;
        }

        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
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
                    if (!other.active || other.owner != Projectile.owner || other.ai[1] != 0f)
                        continue;

                    bool isShadow = other.type == ModContent.ProjectileType<ShadowOrbProjectile>();
                    bool isPain = other.type == ModContent.ProjectileType<PainOrbProjectile>();
                    bool isInferno = other.type == ModContent.ProjectileType<InfernoOrbProjectile>();

                    if (isShadow || isPain || isInferno)
                    {
                        float dist = Vector2.Distance(Projectile.Center, other.Center);
                        if (dist < 32f)
                        {
                            if (Main.myPlayer == Projectile.owner)
                            {
                                Vector2 fusionPos = (Projectile.Center + other.Center) / 2f;
                                int fusionType = isInferno
                                    ? ModContent.ProjectileType<StarlitInfernoFusionOrbProjectile>()
                                    : (isShadow
                                        ? ModContent.ProjectileType<StarlitShadowFusionOrbProjectile>()
                                        : ModContent.ProjectileType<StarlitPainFusionOrbProjectile>());

                                Player ownerPlayer = Main.player[Projectile.owner];
                                int heldDmg = ownerPlayer?.HeldItem?.damage ?? 0;
                                int baseDmg = Math.Max(Projectile.damage, other.damage);
                                if (baseDmg <= 0)
                                    baseDmg = (int)(heldDmg * 0.75f);

                                Projectile fusion = Projectile.NewProjectileDirect(
                                    Projectile.GetSource_FromThis(),
                                    fusionPos,
                                    Vector2.Zero,
                                    fusionType,
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
                NPC target = FindNearestTarget(Projectile.Center, 500f);
                if (target != null)
                {
                    Vector2 toTarget = target.Center - Projectile.Center;
                    toTarget.Normalize();
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * 12f, 0.1f);
                }

                Projectile.rotation = Projectile.velocity.ToRotation();

                if (Main.rand.NextBool(3))
                {
                    Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(6f, 6f), DustID.YellowStarDust);
                    d.noGravity = true;
                    d.scale = 1.2f;
                }

                return;
            }

            
            var modPlayer = Main.player[Projectile.owner].GetModPlayer<Content.Players.StarlitModPlayer>();
            int orbCount = modPlayer?.orbCount ?? 1;
            if (orbCount < 1) orbCount = 1;

            float index = Projectile.localAI[0];
            float globalAngle = Main.GlobalTimeWrappedHourly * ORBIT_SPEED * 2.5f;
            float myAngle = globalAngle + (MathHelper.TwoPi * index / orbCount);

            Vector2 orbitTarget = Main.player[Projectile.owner].Center + myAngle.ToRotationVector2() * (ORBIT_RADIUS + orbCount * 3f);
            Vector2 dirToTarget = orbitTarget - Projectile.Center;

            Projectile.velocity = dirToTarget * 0.25f;

            Lighting.AddLight(Projectile.Center, new Vector3(1f, 0.9f, 0.5f));
            Projectile.rotation += 0.2f;

            if (Main.rand.NextBool(5))
            {
                Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(6f, 6f), DustID.YellowStarDust);
                d.noGravity = true;
                d.scale = 0.9f;
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
            for (int i = 0; i < 15; i++)
            {
                var d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.YellowStarDust, 
                    Main.rand.NextFloat(-4, 4), Main.rand.NextFloat(-4, 4));
                d.noGravity = true;
                d.scale = 1.5f;
            }

            var modPlayer = Main.player[Projectile.owner].GetModPlayer<Content.Players.StarlitModPlayer>();
            modPlayer.orbCount = modPlayer.orbCount - 1;
            SoundEngine.PlaySound(SoundID.Item10 with { Volume = 0.6f, Pitch = 0.2f }, Projectile.Center);
        }

        public override bool? CanHitNPC(NPC target) =>
            Projectile.ai[1] == 1f ? true : false;

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D starTex = Terraria.GameContent.TextureAssets.Projectile[ProjectileID.FallingStar].Value;
            Vector2 origin = starTex.Size() / 2f;
            
            
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                float trailProgress = i / (float)Projectile.oldPos.Length;
                float trailAlpha = (1f - trailProgress) * 0.6f;
                Color trailColor = new Color(255, 240, 150) * trailAlpha;

                Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;

                Main.EntitySpriteDraw(
                    starTex,
                    trailPos,
                    null,
                    trailColor,
                    Projectile.rotation,
                    origin,
                    Projectile.scale * (1f - trailProgress * 0.2f),
                    SpriteEffects.None,
                    0
                );
            }

            
            Main.EntitySpriteDraw(
                starTex,
                Projectile.Center - Main.screenPosition,
                null,
                new Color(255, 255, 200),
                Projectile.rotation,
                origin,
                Projectile.scale,
                SpriteEffects.None,
                0
            );

            
            Color glowColor = new Color(255, 255, 150, 0) * 0.7f;
            Main.EntitySpriteDraw(
                starTex,
                Projectile.Center - Main.screenPosition,
                null,
                glowColor,
                Projectile.rotation,
                origin,
                Projectile.scale * 1.3f,
                SpriteEffects.None,
                0
            );

            return false;
        }
    }
}


using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System;

namespace WeaponMerging.Content.Projectiles
{
    public class InfernoOrbProjectile : ModProjectile
    {
        private const float ORBIT_RADIUS = 52f;
        private const float ORBIT_SPEED = 0.28f;
        private const float FUSION_RADIUS = 35f;
        private int trailCounter = 0;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 12;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 22;
            Projectile.height = 22;
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
                    if (!other.active || other.owner != Projectile.owner || other.ai[1] != 0f)
                        continue;

                    bool isShadow = other.type == ModContent.ProjectileType<ShadowOrbProjectile>();
                    bool isPain = other.type == ModContent.ProjectileType<PainOrbProjectile>();

                    if ((isShadow || isPain) && Vector2.Distance(Projectile.Center, other.Center) < FUSION_RADIUS)
                    {
                        
                        if (Main.myPlayer == Projectile.owner)
                        {
                            Vector2 fusionPos = (Projectile.Center + other.Center) / 2f;
                            int fusionType = isShadow
                                ? ModContent.ProjectileType<ChaosOrbProjectile>()
                                : ModContent.ProjectileType<InfernoPainFusionOrbProjectile>();

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
                                1.5f,
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

            if (Projectile.owner < 0 || Projectile.owner >= Main.maxPlayers)
            {
                Projectile.Kill();
                return;
            }

            Player owner = Main.player[Projectile.owner];
            var modPlayer = owner.GetModPlayer<Content.Players.InfernoPlayer>();
            int orbCount = modPlayer?.orbCount ?? 1;
            if (orbCount < 1) orbCount = 1;

            float index = Projectile.localAI[0];
            float globalAngle = Main.GlobalTimeWrappedHourly * ORBIT_SPEED * 2f;
            float myAngle = globalAngle + (MathHelper.TwoPi * index / orbCount);

            Vector2 orbitTarget = owner.Center + myAngle.ToRotationVector2() * (ORBIT_RADIUS + orbCount * 6f);
            Projectile.velocity = (orbitTarget - Projectile.Center) * 0.28f;

            Lighting.AddLight(Projectile.Center, new Vector3(1f, 0.55f, 0.1f));
            Projectile.rotation += 0.25f;

            trailCounter++;

            
            if (trailCounter % 2 == 0)
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, 6,
                    -Projectile.velocity * 0.4f, 100, new Color(255, 140, 0), Main.rand.NextFloat(1.2f, 1.8f));
                dust.noGravity = true;
                dust.alpha = 120;
            }

            
            if (Main.rand.NextBool(4))
            {
                Dust sparkle = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(12, 12),
                    158, Vector2.Zero, 100, Color.OrangeRed, Main.rand.NextFloat(1.0f, 1.5f));
                sparkle.noGravity = true;
            }

            
            if (Main.rand.NextBool(8))
            {
                Dust flame = Dust.NewDustPerfect(Projectile.Center, 174,
                    Main.rand.NextVector2Circular(2f, 2f), 100, Color.Yellow, Main.rand.NextFloat(0.8f, 1.2f));
                flame.noGravity = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = ModContent.Request<Texture2D>("WeaponMerging/Content/Projectiles/InfernoOrbProjectile").Value;
            Vector2 origin = tex.Size() / 2f;

            
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                float progress = i / (float)Projectile.oldPos.Length;
                float alpha = (1f - progress) * 0.7f;
                
                
                Color trailColor;
                int colorIndex = i % 3;
                if (colorIndex == 0)
                    trailColor = new Color(255, 140, 0) * alpha;
                else if (colorIndex == 1)
                    trailColor = new Color(255, 80, 0) * alpha;
                else
                    trailColor = new Color(255, 200, 0) * alpha;

                Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                Main.EntitySpriteDraw(tex, trailPos, null, trailColor, Projectile.oldRot[i],
                    origin, Projectile.scale * (1f - progress * 0.3f), SpriteEffects.None, 0);
            }

            
            float pulse = 1f + (float)Math.Sin(Main.GlobalTimeWrappedHourly * 8f + Projectile.whoAmI) * 0.15f;
            Color mainColor = new Color(255, 140, 0);
            
            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null,
                mainColor, Projectile.rotation, origin, Projectile.scale * pulse, SpriteEffects.None, 0);

            
            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null,
                new Color(255, 200, 100) * 0.6f, Projectile.rotation, origin, Projectile.scale * pulse * 1.4f, SpriteEffects.None, 0);

            return false;
        }
    }
}


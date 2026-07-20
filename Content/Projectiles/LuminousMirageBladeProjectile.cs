using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using System;

namespace WeaponMerging.Content.Projectiles
{
    public class LuminousMirageBladeProjectile : ModProjectile
    {
        private const int SLASH_DURATION = 36; 
        private float startAngle;
        private float endAngle;
        private int comboStep; 
        private int spriteDirection;
        private float baseAngle;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 24; 
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 150;
            Projectile.height = 150;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = SLASH_DURATION;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.ownerHitCheck = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            comboStep = (int)Projectile.ai[0];
            Projectile.Center = player.Center;

            if (Projectile.localAI[0] == 0f)
            {
                Projectile.localAI[0] = 1f;
                baseAngle = Projectile.velocity.ToRotation();
                spriteDirection = (Main.MouseWorld.X < player.Center.X) ? -1 : 1;
            }

            float progress = 1f - (Projectile.timeLeft / (float)SLASH_DURATION);
            float easedProgress = EaseOutCubic(progress);

            switch (comboStep)
            {
                case 0: 
                    startAngle = -MathHelper.PiOver4 * 2.5f;
                    endAngle = MathHelper.PiOver4 * 2f;
                    Projectile.scale = 1.5f + progress * 0.4f;
                    break;
                case 1: 
                    startAngle = MathHelper.PiOver4 * 2f;
                    endAngle = -MathHelper.PiOver4 * 2.8f;
                    Projectile.scale = 1.7f + progress * 0.5f;
                    break;
                case 2: 
                    startAngle = -MathHelper.Pi * 0.9f;
                    endAngle = MathHelper.Pi * 0.9f; 
                    Projectile.scale = 2.0f + progress * 0.6f;
                    break;
                case 3: 
                    startAngle = -MathHelper.Pi * 1.2f;
                    endAngle = MathHelper.Pi * 0.6f;
                    Projectile.scale = 2.8f + progress * 1.2f; 
                    break;
            }

            Projectile.width = (int)(150 * Projectile.scale);
            Projectile.height = (int)(150 * Projectile.scale);

            float swingAngle = MathHelper.Lerp(startAngle, endAngle, easedProgress);

            if (spriteDirection == -1)
            {
                swingAngle = -swingAngle + MathHelper.Pi;
            }

            Projectile.rotation = baseAngle + swingAngle;

            player.heldProj = Projectile.whoAmI;
            player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.PiOver2);

            SpawnComboEffects(player, progress);

            if (Projectile.timeLeft == SLASH_DURATION)
            {
                SoundStyle swingSound = comboStep switch
                {
                    0 => SoundID.Item15 with { Pitch = 0.3f, Volume = 0.8f },
                    1 => SoundID.Item15 with { Pitch = 0.5f, Volume = 0.8f },
                    2 => SoundID.Item71 with { Pitch = 0.1f, Volume = 0.9f },
                    3 => SoundID.Item117 with { Pitch = -0.2f, Volume = 1.0f }, 
                    _ => SoundID.Item1
                };
                SoundEngine.PlaySound(swingSound, Projectile.position);
            }

            Color lightColor = GetComboColor();
            Lighting.AddLight(Projectile.Center, lightColor.ToVector3() * 1.8f * Projectile.scale); 
        }

        private void SpawnComboEffects(Player player, float progress)
        {
            // 1. Calculate the TRUE angle the blade is visually pointing. 
            // Terraria swords point Top-Right (-45 deg) when facing right, and Top-Left (-135 deg) when facing left.
            float visualBladeAngle = Projectile.rotation + (spriteDirection == 1 ? -MathHelper.PiOver4 : -MathHelper.PiOver4 * 3);

            // 3. Set the dust position 55 pixels backwards along that line
            Vector2 dustPos = Projectile.Center + visualBladeAngle.ToRotationVector2() * (55f * Projectile.scale);

            int dustType = comboStep switch
            {
                0 => DustID.Vortex,
                1 => DustID.PinkCrystalShard,
                2 => DustID.CrystalPulse,
                3 => DustID.GoldFlame,
                _ => DustID.MagicMirror
            };

            for(int i = 0; i < 2; i++)
            {
                // 4. Make the dust fly backwards away from the hilt
                Vector2 dustVelocity = visualBladeAngle.ToRotationVector2().RotatedByRandom(0.8f) * Main.rand.NextFloat(4f, 12f);

                Dust dust = Dust.NewDustPerfect(dustPos, dustType, dustVelocity, 0, default, Main.rand.NextFloat(1.5f, 3.5f));
                dust.noGravity = true;
            }
        }

        private Color GetComboColor()
        {
            return comboStep switch
            {
                0 => new Color(0, 255, 255),    // Cyan
                1 => new Color(255, 105, 180),  // Hot Pink
                2 => new Color(200, 0, 255),    // Magenta
                3 => new Color(255, 255, 150),  // Bright Gold/White
                _ => Color.White
            };
        }

        private float EaseOutCubic(float x) => 1f - (float)Math.Pow(1f - x, 3);

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            int baseDustType = comboStep == 0 ? DustID.Vortex : comboStep == 1 ? DustID.PinkCrystalShard : comboStep == 2 ? DustID.CrystalPulse : DustID.GoldFlame;

            for (int i = 0; i < 15 + (comboStep * 10); i++)
            {
                Dust dust = Dust.NewDustDirect(target.position, target.width, target.height, baseDustType,
                    Main.rand.NextFloat(-8f, 8f), Main.rand.NextFloat(-8f, 8f), 0, default, 2f + (comboStep * 0.5f));
                dust.noGravity = true;
            }

            // --- REWORKED HIT ABILITY: Localized Explosions & Impact Dusts Only ---
            if (Main.myPlayer == Projectile.owner)
            {
                switch (comboStep)
                {
                    case 2:
                        SoundEngine.PlaySound(SoundID.Item101 with { Pitch = 0.2f }, target.position);
                        
                        // Intense localized crystal crunch (No projectiles)
                        for (int i = 0; i < 20; i++)
                        {
                            Vector2 burstVel = Main.rand.NextVector2Circular(8f, 8f);
                            Dust burst = Dust.NewDustPerfect(target.Center, DustID.CrystalPulse, burstVel, 0, default, 2.5f);
                            burst.noGravity = true;
                        }
                        break;

                    case 3:
                        SoundEngine.PlaySound(SoundID.Item162 with { Pitch = -0.3f, Volume = 1.0f }, target.position); 

                        // Massive Luminous Nova particle starburst
                        for (int i = 0; i < 40; i++)
                        {
                            // Fast outer ring
                            Vector2 ringVel = Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(10f, 20f);
                            Dust.NewDustPerfect(target.Center, DustID.GoldFlame, ringVel, 0, default, 3f).noGravity = true;
                            
                            // Dense inner core
                            Vector2 coreVel = Main.rand.NextVector2Circular(6f, 6f);
                            Dust.NewDustPerfect(target.Center, DustID.MagicMirror, coreVel, 0, default, 2f).noGravity = true;
                        }

                        // Stationary AoE Detonation (Hits nearby enemies, does NOT fly away)
                        int shockwave = Projectile.NewProjectile(
                            Projectile.GetSource_FromThis(),
                            target.Center,
                            Vector2.Zero,
                            ProjectileID.SolarWhipSwordExplosion, // Massive stationary fiery blast
                            (int)(Projectile.damage * 0.7f),
                            4f,
                            Projectile.owner
                        );
                        if (shockwave >= 0 && shockwave < Main.maxProjectiles)
                        {
                            Main.projectile[shockwave].timeLeft = 15;
                            Main.projectile[shockwave].scale = 2.5f; // Huge blast radius
                            Main.projectile[shockwave].DamageType = DamageClass.Melee;
                        }
                        break;
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Player player = Main.player[Projectile.owner];
            Texture2D swordTexture = ModContent.Request<Texture2D>("WeaponMerging/Content/Items/Weapons/LuminousMirageBlade").Value;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            Rectangle sourceRect = new Rectangle(0, 0, swordTexture.Width, swordTexture.Height);
            
            Vector2 origin = spriteDirection == -1
                ? new Vector2(swordTexture.Width - 10, swordTexture.Height - 10)
                : new Vector2(10, swordTexture.Height - 10);

            Color comboColor = GetComboColor();
            SpriteEffects effects = spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            float progress = 1f - (Projectile.timeLeft / (float)SLASH_DURATION);

            // --- ADDITIVE BLENDING PASS ---
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // 1. Dynamic Mirage Clones 
            float fanSpread = (float)Math.Sin(progress * MathHelper.Pi) * 0.6f; 
            for (int i = -1; i <= 1; i += 2)
            {
                float cloneRot = Projectile.rotation + (fanSpread * i * spriteDirection);
                Main.EntitySpriteDraw(
                    swordTexture,
                    drawPosition,
                    sourceRect,
                    comboColor * 0.45f,
                    cloneRot,
                    origin,
                    Projectile.scale,
                    effects,
                    0
                );
            }

            // 2. High-Fidelity Energy Trail Ribbon
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldRot[i] == 0) continue;

                float trailProgress = i / (float)Projectile.oldPos.Length;
                float trailAlpha = (1f - trailProgress);
                
                Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                
                // Draw wide colored base
                float baseScale = Projectile.scale * (1f - trailProgress * 0.6f);
                Main.EntitySpriteDraw(swordTexture, trailPos, sourceRect, comboColor * trailAlpha * 0.8f, Projectile.oldRot[i], origin, baseScale, effects, 0);

                // Draw thinner, intensely bright "hot core" inside the trail
                float coreScale = Projectile.scale * (1f - trailProgress * 0.8f) * 0.8f;
                Color coreColor = Color.Lerp(comboColor, Color.White, 0.7f) * trailAlpha * 0.6f;
                Main.EntitySpriteDraw(swordTexture, trailPos, sourceRect, coreColor, Projectile.oldRot[i], origin, coreScale, effects, 0);
            }

            // 3. Superheated Weapon Glare
            Main.EntitySpriteDraw(
                swordTexture,
                drawPosition,
                sourceRect,
                comboColor * 0.9f,
                Projectile.rotation,
                origin,
                Projectile.scale * 1.15f,
                effects,
                0
            );
            
            Main.EntitySpriteDraw(
                swordTexture,
                drawPosition,
                sourceRect,
                Color.White * 0.5f,
                Projectile.rotation,
                origin,
                Projectile.scale * 1.05f,
                effects,
                0
            );

            // Revert back to normal Alpha blending for the solid core
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // 4. Solid Blade Core
            Main.EntitySpriteDraw(
                swordTexture,
                drawPosition,
                sourceRect,
                lightColor,
                Projectile.rotation,
                origin,
                Projectile.scale,
                effects,
                0
            );

            return false;
        }
    }
}
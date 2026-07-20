using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using System;

namespace WeaponMerging.Content.Projectiles
{
    public class VoidEchoAfterimage : ModProjectile
    {
        private const int HOVER_TIME = 25; // Time spent freezing and gathering energy
        private const int FLURRY_TIME = 15; // Time spent doing the rapid multi-slash
        
        public override void SetDefaults()
        {
            Projectile.width = 220; // Massive hit area for the flurry
            Projectile.height = 220;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = HOVER_TIME + FLURRY_TIME;
            Projectile.DamageType = DamageClass.Melee;
            
            // Rapid multi-hits during the flurry
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 5; 
        }

        public override void AI()
        {
            float savedRotation = Projectile.ai[0];
            int spriteDir = (int)Projectile.ai[1];

            if (Projectile.timeLeft > FLURRY_TIME)
            {
                // PHASE 1: Frozen & Vibrating
                float chargeProgress = 1f - ((Projectile.timeLeft - FLURRY_TIME) / (float)HOVER_TIME);
                Projectile.rotation = savedRotation;
                
                // Vibrate violently the closer it gets to erupting
                Vector2 vibration = Main.rand.NextVector2Circular(8f, 8f) * chargeProgress;
                Projectile.Center += vibration * 0.1f; // Slight actual jitter

                // Suck in dark dust
                if (Main.rand.NextBool(2))
                {
                    Vector2 dustSpawn = Projectile.Center + Main.rand.NextVector2CircularEdge(100f, 100f);
                    Vector2 toCenter = (Projectile.Center - dustSpawn).SafeNormalize(Vector2.Zero);
                    Dust suckDust = Dust.NewDustPerfect(dustSpawn, DustID.Shadowflame, toCenter * 6f, 0, default, 1.2f);
                    suckDust.noGravity = true;
                }
            }
            else
            {
                // PHASE 2: Reality Shatter & Rapid Flurry
                if (Projectile.timeLeft == FLURRY_TIME)
                {
                    // Explosive glass shatter / dark magic sound
                    SoundEngine.PlaySound(SoundID.Item103 with { Pitch = 0.5f, Volume = 0.8f }, Projectile.Center);
                    SoundEngine.PlaySound(SoundID.Item71 with { Pitch = -0.2f, Volume = 1f }, Projectile.Center);

                    // Burst of void energy outwards
                    for (int i = 0; i < 20; i++)
                    {
                        Dust.NewDustPerfect(Projectile.Center, DustID.Granite, 
                            Main.rand.NextVector2Circular(12f, 12f), 0, Color.Magenta, Main.rand.NextFloat(1.5f, 3f)).noGravity = true;
                    }
                }

                // Rapidly spin the core rotation for the draw code
                Projectile.rotation += 0.6f * spriteDir;
            }

            Lighting.AddLight(Projectile.Center, 0.4f, 0.05f, 0.5f);
        }

        public override bool? CanDamage()
        {
            // Only deal damage during the Flurry phase
            return Projectile.timeLeft <= FLURRY_TIME;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.ShadowFlame, 120);
            
            // Small tearing impacts on hit
            Dust.NewDustDirect(target.position, target.width, target.height, DustID.Shadowflame, 
                Main.rand.NextFloat(-4f, 4f), Main.rand.NextFloat(-4f, 4f), 0, default, 1.5f).noGravity = true;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D swordTexture = ModContent.Request<Texture2D>("WeaponMerging/Content/Items/Weapons/VoidEchoBlade").Value;
            int spriteDir = (int)Projectile.ai[1];
            Vector2 origin = spriteDir == -1 ? new Vector2(swordTexture.Width - 10, swordTexture.Height - 10) : new Vector2(10, swordTexture.Height - 10);
            SpriteEffects effects = spriteDir == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            
            // Apply the jitter to the drawing if in hover phase
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            if (Projectile.timeLeft > FLURRY_TIME)
            {
                float chargeProgress = 1f - ((Projectile.timeLeft - FLURRY_TIME) / (float)HOVER_TIME);
                drawPos += Main.rand.NextVector2Circular(6f, 6f) * chargeProgress;
            }

            // ADDITIVE BLENDING PASS
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            if (Projectile.timeLeft > FLURRY_TIME)
            {
                // Draw 1: Frozen Phase. Glows increasingly brighter violet.
                float chargeProgress = 1f - ((Projectile.timeLeft - FLURRY_TIME) / (float)HOVER_TIME);
                Color ghostColor = Color.DarkViolet * (0.5f + chargeProgress * 0.5f);
                
                // Multi-layered ghostly effect
                Main.EntitySpriteDraw(swordTexture, drawPos, null, ghostColor, Projectile.rotation, origin, 2.0f, effects, 0);
                Main.EntitySpriteDraw(swordTexture, drawPos, null, Color.Magenta * chargeProgress, Projectile.rotation, origin, 2.0f + (chargeProgress * 0.2f), effects, 0);
            }
            else
            {
                // Draw 2: Flurry Phase. Multiple overlapping spinning blades like a blender.
                float flurryProgress = 1f - (Projectile.timeLeft / (float)FLURRY_TIME);
                float alpha = 1f - (flurryProgress * flurryProgress); // Fades out sharply at the end
                
                Color flurryColor = Color.Magenta * alpha;
                Color coreColor = Color.White * (alpha * 0.8f);

                // Draw 4 overlapping swords to create a circular rift effect
                for (int i = 0; i < 4; i++)
                {
                    float offsetRot = Projectile.rotation + (MathHelper.PiOver2 * i);
                    float scale = 2.5f + (flurryProgress * 0.5f); // Expands outward

                    Main.EntitySpriteDraw(swordTexture, drawPos, null, flurryColor, offsetRot, origin, scale, effects, 0);
                    Main.EntitySpriteDraw(swordTexture, drawPos, null, coreColor, offsetRot, origin, scale * 0.8f, effects, 0);
                }
            }

            // NORMAL ALPHA BLEND PASS (For Pitch Black Core)
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            if (Projectile.timeLeft <= FLURRY_TIME)
            {
                // Draw a collapsing pitch-black hole in the center during the flurry
                float flurryProgress = 1f - (Projectile.timeLeft / (float)FLURRY_TIME);
                float blackScale = 2.0f * (1f - flurryProgress); // Shrinks as it ends
                
                Main.EntitySpriteDraw(swordTexture, drawPos, null, Color.Black * (1f - flurryProgress), Projectile.rotation, origin, blackScale, effects, 0);
            }

            return false;
        }
    }
}
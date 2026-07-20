using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using System;

namespace WeaponMerging.Content.Projectiles
{
    public class CelestialFrostBladeProjectile : ModProjectile
    {
        private float startAngle;
        private float endAngle;
        private int comboStep; 
        private int spriteDirection;
        private float baseAngle;
        private bool hasFiredProjectile = false;

        // Uses the Item's Texture
        public override string Texture => "WeaponMerging/Content/Items/Weapons/CelestialFrostBlade";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 20; 
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 120;
            Projectile.height = 120;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.ownerHitCheck = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1; // Hit once per swing
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            comboStep = (int)Projectile.ai[0];
            int duration = (int)Projectile.ai[1]; 
            
            Projectile.Center = player.Center;

            // Initialization Step
            if (Projectile.localAI[0] == 0f)
            {
                Projectile.localAI[0] = 1f;
                Projectile.timeLeft = duration;
                baseAngle = Projectile.velocity.ToRotation();
                spriteDirection = (Main.MouseWorld.X < player.Center.X) ? -1 : 1;
            }

            float progress = 1f - (Projectile.timeLeft / (float)duration);
            float easedProgress = EaseOutCubic(progress);

            // Dynamic Swing Arc based on Combo Step
            switch (comboStep)
            {
                case 0: // Fast Down
                    startAngle = -MathHelper.PiOver4 * 2.2f;
                    endAngle = MathHelper.PiOver4 * 1.8f;
                    Projectile.scale = 1.2f + progress * 0.3f;
                    break;
                case 1: // Fast Up
                    startAngle = MathHelper.PiOver4 * 1.8f;
                    endAngle = -MathHelper.PiOver4 * 2.5f;
                    Projectile.scale = 1.3f + progress * 0.4f;
                    break;
                case 2: // Wide Charge Sweep
                    startAngle = -MathHelper.Pi * 0.9f;
                    endAngle = MathHelper.Pi * 0.8f; 
                    Projectile.scale = 1.6f + progress * 0.5f;
                    break;
                case 3: // Massive Heavy Slam
                    // Uses an Expo curve for the final slam (hangs in the air, then snaps down violently)
                    easedProgress = progress == 0 ? 0 : (float)Math.Pow(2, 10 * progress - 10);
                    startAngle = -MathHelper.Pi * 1.1f;
                    endAngle = MathHelper.Pi * 0.8f;
                    Projectile.scale = 2.2f + progress * 0.8f; 
                    break;
            }

            Projectile.width = (int)(120 * Projectile.scale);
            Projectile.height = (int)(120 * Projectile.scale);

            float swingAngle = MathHelper.Lerp(startAngle, endAngle, easedProgress);
            if (spriteDirection == -1) swingAngle = -swingAngle + MathHelper.Pi;

            Projectile.rotation = baseAngle + swingAngle;

            // Player Arm Puppeting
            player.heldProj = Projectile.whoAmI;
            player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.PiOver2);

            SpawnTrailDust(player, progress);

            // Fire Ranged Projectiles exactly at the apex of the swing (progress = 0.5)
            float firePoint;

            switch (comboStep)
            {
                case 0:
                    firePoint = 0.35f;
                    break;

                case 1:
                    firePoint = 0.35f;
                    break;

                case 2:
                    firePoint = 0.25f;
                    break;

                case 3:
                    firePoint = 0.82f;
                    break;

                default:
                    firePoint = 0.45f;
                    break;
            }

            if (progress >= firePoint && !hasFiredProjectile)
            {
                hasFiredProjectile = true;
                FireAbilities(player);
            }

            // End-of-swing Sound Effects
            if (Projectile.timeLeft == duration)
            {
                SoundStyle swingSound = comboStep switch
                {
                    0 => SoundID.Item1 with { Pitch = 0.2f },
                    1 => SoundID.Item1 with { Pitch = 0.3f },
                    2 => SoundID.Item71 with { Pitch = 0.1f, Volume = 0.9f }, // Magic charge sound
                    3 => SoundID.Item60 with { Pitch = -0.3f, Volume = 1.0f }, // Heavy woosh
                    _ => SoundID.Item1
                };
                SoundEngine.PlaySound(swingSound, Projectile.position);
            }

            Lighting.AddLight(Projectile.Center, GetComboColor().ToVector3() * 1.5f * Projectile.scale); 
        }

        private void FireAbilities(Player player)
        {
            Vector2 aim = player.DirectionTo(Main.MouseWorld);

            if (comboStep == 0 || comboStep == 1 || comboStep == 2)
            {
                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    player.Center,
                    aim * 14f,
                    ModContent.ProjectileType<CelestialFrostSlashProjectile>(),
                    Projectile.damage,
                    Projectile.knockBack,
                    player.whoAmI,
                    comboStep
                );
            }
            else if (comboStep == 3)
            {
                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    player.Center,
                    aim * 10f,
                    ModContent.ProjectileType<CelestialFrostFinisherProjectile>(),
                    (int)(Projectile.damage * 1.5f),
                    Projectile.knockBack * 3f,
                    player.whoAmI
                );

                SoundEngine.PlaySound(
                    SoundID.Item62 with
                    {
                        Volume = 1.2f,
                        Pitch = -0.3f
                    },
                    player.Center);
            }
        }

        private void SpawnTrailDust(Player player, float progress)
        {
            float visualBladeAngle = Projectile.rotation + (spriteDirection == 1 ? -MathHelper.PiOver4 : -MathHelper.PiOver4 * 3);
            Vector2 dustPos = Projectile.Center + visualBladeAngle.ToRotationVector2() * (45f * Projectile.scale);

            if (Main.rand.NextBool(2))
            {
                int dustType = comboStep == 3 ? DustID.BlueFairy : DustID.IceTorch;
                Vector2 dustVelocity = visualBladeAngle.ToRotationVector2().RotatedByRandom(0.5f) * Main.rand.NextFloat(2f, 6f);
                Dust dust = Dust.NewDustPerfect(dustPos, dustType, dustVelocity, 0, default, Main.rand.NextFloat(1.2f, 2.0f));
                dust.noGravity = true;
            }
        }

        private Color GetComboColor()
        {
            return comboStep switch
            {
                0 => new Color(0, 255, 255),    // Cyan
                1 => new Color(150, 200, 255),  // Light Blue
                2 => new Color(100, 150, 255),  // Deep Frost Blue
                3 => new Color(255, 255, 255),  // Blinding White Finisher
                _ => Color.White
            };
        }

        private float EaseOutCubic(float x) => 1f - (float)Math.Pow(1f - x, 3);

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Frostburn, 120);
            
            for (int i = 0; i < 10 + (comboStep * 5); i++)
            {
                Dust dust = Dust.NewDustDirect(target.position, target.width, target.height, DustID.IceTorch,
                    Main.rand.NextFloat(-6f, 6f), Main.rand.NextFloat(-6f, 6f), 0, default, 1.5f + (comboStep * 0.3f));
                dust.noGravity = true;
            }

            if (comboStep == 3 && Main.myPlayer == Projectile.owner)
            {
                SoundEngine.PlaySound(SoundID.Item27 with { Pitch = -0.2f, Volume = 1.0f }, target.position); // Heavy Ice Shatter
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Player player = Main.player[Projectile.owner];
            Texture2D swordTexture = Terraria.GameContent.TextureAssets.Projectile[Type].Value;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            Rectangle sourceRect = new Rectangle(0, 0, swordTexture.Width, swordTexture.Height);
            
            Vector2 origin = spriteDirection == -1
                ? new Vector2(swordTexture.Width - 10, swordTexture.Height - 10)
                : new Vector2(10, swordTexture.Height - 10);

            Color comboColor = GetComboColor();
            SpriteEffects effects = spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            int duration = (int)Projectile.ai[1];
            float progress = 1f - (Projectile.timeLeft / (float)duration);

            // --- ADDITIVE BLENDING PASS ---
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // 1. Dynamic "Frost Mirage" Clones
            float fanSpread = (float)Math.Sin(progress * MathHelper.Pi) * 0.5f; 
            for (int i = -1; i <= 1; i += 2)
            {
                float cloneRot = Projectile.rotation + (fanSpread * i * spriteDirection);
                Main.EntitySpriteDraw(
                    swordTexture,
                    drawPosition,
                    sourceRect,
                    comboColor * 0.4f,
                    cloneRot,
                    origin,
                    Projectile.scale,
                    effects,
                    0
                );
            }

            // 2. High-Fidelity Energy Ribbon Trail
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldRot[i] == 0) continue;

                float trailProgress = i / (float)Projectile.oldPos.Length;
                float trailAlpha = (1f - trailProgress);
                Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                
                // Outer Cyan Glow
                float baseScale = Projectile.scale * (1f - trailProgress * 0.5f);
                Main.EntitySpriteDraw(swordTexture, trailPos, sourceRect, comboColor * trailAlpha * 0.7f, Projectile.oldRot[i], origin, baseScale, effects, 0);

                // Inner Bright White Core
                float coreScale = Projectile.scale * (1f - trailProgress * 0.7f) * 0.8f;
                Color coreColor = Color.Lerp(comboColor, Color.White, 0.8f) * trailAlpha * 0.6f;
                Main.EntitySpriteDraw(swordTexture, trailPos, sourceRect, coreColor, Projectile.oldRot[i], origin, coreScale, effects, 0);
            }

            // 3. Superheated Frost Glare on the active blade
            Main.EntitySpriteDraw(swordTexture, drawPosition, sourceRect, comboColor * 0.8f, Projectile.rotation, origin, Projectile.scale * 1.15f, effects, 0);
            Main.EntitySpriteDraw(swordTexture, drawPosition, sourceRect, Color.White * 0.6f, Projectile.rotation, origin, Projectile.scale * 1.05f, effects, 0);

            // Revert to Normal Alpha Blending
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // 4. Solid Blade Core
            Main.EntitySpriteDraw(swordTexture, drawPosition, sourceRect, lightColor, Projectile.rotation, origin, Projectile.scale, effects, 0);

            return false;
        }
    }
}
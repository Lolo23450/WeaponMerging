using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using System;

namespace WeaponMerging.Content.Projectiles
{
    public class VoidEchoSpecial : ModProjectile
    {
        private const int SLASH_DURATION = 45;
        private int spriteDirection;
        private float baseAngle;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 24;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 250;
            Projectile.height = 250;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = SLASH_DURATION;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            Projectile.Center = player.Center;

            if (Projectile.localAI[0] == 0f)
            {
                Projectile.localAI[0] = 1f;
                baseAngle = Projectile.velocity.ToRotation();
                spriteDirection = (Main.MouseWorld.X < player.Center.X) ? -1 : 1;
                SoundEngine.PlaySound(SoundID.Item103 with { Pitch = -0.5f, Volume = 1.2f }, Projectile.position); // Heavy dark explosion sound
            }

            float progress = 1f - (Projectile.timeLeft / (float)SLASH_DURATION);
            float easedProgress = EaseOutCubic(progress);

            float startAngle = -MathHelper.Pi * 1.2f;
            float endAngle = MathHelper.Pi * 1.2f;
            float swingAngle = MathHelper.Lerp(startAngle, endAngle, easedProgress);

            if (spriteDirection == -1) swingAngle = -swingAngle + MathHelper.Pi;

            Projectile.rotation = baseAngle + swingAngle;
            Projectile.scale = 2.0f + progress * 1.5f; // Gigantic weapon

            Projectile.width = (int)(150 * Projectile.scale);
            Projectile.height = (int)(150 * Projectile.scale);

            player.heldProj = Projectile.whoAmI;
            player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.PiOver2);

            // Spawn Echoes
            if (Projectile.timeLeft % 6 == 0 && Projectile.timeLeft > 10 && Projectile.timeLeft < SLASH_DURATION - 5)
            {
                SpawnEcho(player);
            }

            // Screen tearing dusts
            if (Main.rand.NextBool(2))
            {
                Vector2 dustPos = Projectile.Center + Projectile.rotation.ToRotationVector2() * (80f * Projectile.scale);
                Dust dust = Dust.NewDustPerfect(dustPos, DustID.Granite,
                    Projectile.rotation.ToRotationVector2().RotatedByRandom(0.8f) * Main.rand.NextFloat(8f, 15f),
                    0, Color.Magenta, Main.rand.NextFloat(2f, 4f));
                dust.noGravity = true;
            }

            Lighting.AddLight(Projectile.Center, 0.8f, 0.2f, 1.0f);
        }

        private void SpawnEcho(Player owner)
        {
            if (Main.myPlayer != Projectile.owner) return;

            Projectile.NewProjectile(
                Projectile.GetSource_FromThis(),
                Projectile.Center,
                Vector2.Zero,
                ModContent.ProjectileType<VoidEchoAfterimage>(),
                (int)(Projectile.damage * 0.4f), // Echoes deal 40% damage
                Projectile.knockBack * 0.5f,
                owner.whoAmI,
                Projectile.rotation, 
                spriteDirection
            );
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.ShadowFlame, 300);
            SoundEngine.PlaySound(SoundID.Item74 with { Pitch = -0.8f }, target.position);
            
            for (int i = 0; i < 20; i++)
            {
                Dust dust = Dust.NewDustDirect(target.position, target.width, target.height, DustID.Shadowflame,
                    Main.rand.NextFloat(-8f, 8f), Main.rand.NextFloat(-8f, 8f), 0, default, 3f);
                dust.noGravity = true;
            }

            if (Main.myPlayer == Projectile.owner)
            {
                int explosion = Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.Center, Vector2.Zero, 
                    ProjectileID.SolarWhipSwordExplosion, (int)(Projectile.damage * 0.5f), 0f, Projectile.owner);
                Main.projectile[explosion].DamageType = DamageClass.Melee;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D swordTexture = ModContent.Request<Texture2D>("WeaponMerging/Content/Items/Weapons/VoidEchoBlade").Value;
            Vector2 origin = spriteDirection == -1 ? new Vector2(swordTexture.Width - 10, swordTexture.Height - 10) : new Vector2(10, swordTexture.Height - 10);
            SpriteEffects effects = spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            // 1. ADDITIVE (Violent Pink/Purple Tearing)
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldRot[i] == 0) continue;
                float trailProgress = i / (float)Projectile.oldPos.Length;
                float trailAlpha = (1f - trailProgress);
                
                Color trailColor = new Color(255, 50, 255) * trailAlpha;
                float trailScale = Projectile.scale * (1f - trailProgress * 0.5f);

                Main.EntitySpriteDraw(swordTexture, Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition, null, trailColor, Projectile.oldRot[i], origin, trailScale, effects, 0);
            }

            Main.EntitySpriteDraw(swordTexture, drawPos, null, Color.HotPink * 0.8f, Projectile.rotation, origin, Projectile.scale * 1.2f, effects, 0);

            // 2. NORMAL BLEND (Pure Pitch Black Core)
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Main.EntitySpriteDraw(swordTexture, drawPos, null, Color.Black * 0.95f, Projectile.rotation, origin, Projectile.scale * 1.1f, effects, 0);
            Main.EntitySpriteDraw(swordTexture, drawPos, null, Color.White * 0.5f, Projectile.rotation, origin, Projectile.scale, effects, 0); // Subtle inner detail

            return false;
        }

        private float EaseOutCubic(float x) => 1f - (float)Math.Pow(1f - x, 3);
    }
}
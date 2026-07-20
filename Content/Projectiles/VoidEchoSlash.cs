using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using System;

namespace WeaponMerging.Content.Projectiles
{
    public class VoidEchoSlash : ModProjectile
    {
        private const int SLASH_DURATION = 26;
        private int spriteDirection;
        private float baseAngle;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 16;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 140;
            Projectile.height = 140;
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
                SoundEngine.PlaySound(SoundID.Item71 with { Pitch = 0.2f, Volume = 0.7f }, Projectile.position);
            }

            float progress = 1f - (Projectile.timeLeft / (float)SLASH_DURATION);
            float easedProgress = EaseOutQuint(progress);

            float startAngle = -MathHelper.Pi * 0.7f;
            float endAngle = MathHelper.Pi * 0.7f;
            float swingAngle = MathHelper.Lerp(startAngle, endAngle, easedProgress);

            if (spriteDirection == -1) swingAngle = -swingAngle + MathHelper.Pi;

            Projectile.rotation = baseAngle + swingAngle;
            Projectile.scale = 1.3f + progress * 0.5f;

            player.heldProj = Projectile.whoAmI;
            player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.PiOver2);

            if (Main.rand.NextBool(2))
            {
                Vector2 dustPos = Projectile.Center + Projectile.rotation.ToRotationVector2() * (60f * Projectile.scale);
                Dust dust = Dust.NewDustPerfect(dustPos, DustID.Shadowflame,
                    Projectile.rotation.ToRotationVector2().RotatedByRandom(0.5f) * Main.rand.NextFloat(3f, 8f),
                    0, default, Main.rand.NextFloat(1.5f, 2.5f));
                dust.noGravity = true;
            }

            Lighting.AddLight(Projectile.Center, 0.4f, 0.1f, 0.6f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.ShadowFlame, 180);
            SoundEngine.PlaySound(SoundID.Item103 with { Pitch = -0.3f }, target.position);

            for (int i = 0; i < 12; i++)
            {
                Dust dust = Dust.NewDustDirect(target.position, target.width, target.height, DustID.Shadowflame,
                    Main.rand.NextFloat(-5f, 5f), Main.rand.NextFloat(-5f, 5f), 0, default, 2f);
                dust.noGravity = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D swordTexture = ModContent.Request<Texture2D>("WeaponMerging/Content/Items/Weapons/VoidEchoBlade").Value;
            Vector2 origin = spriteDirection == -1 ? new Vector2(swordTexture.Width - 10, swordTexture.Height - 10) : new Vector2(10, swordTexture.Height - 10);
            SpriteEffects effects = spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            // 1. ADDITIVE GLOW & TRAILS
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldRot[i] == 0) continue;
                float trailProgress = i / (float)Projectile.oldPos.Length;
                float trailAlpha = (1f - trailProgress);
                
                Color trailColor = Color.Magenta * trailAlpha * 0.8f;
                float trailScale = Projectile.scale * (1f - trailProgress * 0.4f);

                Main.EntitySpriteDraw(swordTexture, Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition, null, trailColor, Projectile.oldRot[i], origin, trailScale, effects, 0);
            }

            // High-glow aura
            Main.EntitySpriteDraw(swordTexture, drawPos, null, Color.Violet * 0.8f, Projectile.rotation, origin, Projectile.scale * 1.15f, effects, 0);

            // 2. NORMAL BLEND (BLACK CORE)
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // Draw pitch black shadow inside the glow to look like a true void
            Main.EntitySpriteDraw(swordTexture, drawPos, null, Color.Black * 0.9f, Projectile.rotation, origin, Projectile.scale * 1.05f, effects, 0);
            
            // Draw actual weapon texture slightly smaller on top
            Main.EntitySpriteDraw(swordTexture, drawPos, null, lightColor, Projectile.rotation, origin, Projectile.scale, effects, 0);

            return false;
        }

        private float EaseOutQuint(float x) => 1f - (float)Math.Pow(1f - x, 5);
    }
}
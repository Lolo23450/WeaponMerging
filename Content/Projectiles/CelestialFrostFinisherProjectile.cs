using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using System;

namespace WeaponMerging.Content.Projectiles
{
    public class CelestialFrostFinisherProjectile : ModProjectile
    {
        private const int Lifetime = 45;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 40;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 80;
            Projectile.height = 80;

            Projectile.friendly = true;
            Projectile.hostile = false;

            Projectile.penetrate = -1;

            Projectile.timeLeft = Lifetime;

            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;

            Projectile.DamageType = DamageClass.Melee;

            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;

            Projectile.extraUpdates = 2;

            Projectile.scale = 1.4f;
        }

        public override void AI()
        {
            float progress = 1f - Projectile.timeLeft / (float)Lifetime;

            // Maintain momentum
            Projectile.velocity *= 0.995f;

            // Massive growth over lifetime
            Projectile.scale = 1.4f + progress * 1.8f;

            Projectile.rotation = Projectile.velocity.ToRotation();

            // Bright frost lighting
            Lighting.AddLight(
                Projectile.Center,
                0.4f * Projectile.scale,
                0.8f * Projectile.scale,
                1.2f * Projectile.scale);

            // Frost aura slowing nearby enemies
            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (!npc.CanBeChasedBy())
                    continue;

                float distance = Vector2.Distance(npc.Center, Projectile.Center);

                if (distance < 220f * Projectile.scale)
                {
                    npc.velocity *= 0.97f;
                }
            }

            // Heavy frost particles
            for (int i = 0; i < 3; i++)
            {
                Vector2 vel =
                    Projectile.velocity.RotatedByRandom(0.4f) *
                    Main.rand.NextFloat(0.2f, 0.7f);

                Dust dust = Dust.NewDustPerfect(
                    Projectile.Center +
                    Main.rand.NextVector2Circular(
                        25f * Projectile.scale,
                        25f * Projectile.scale),
                    DustID.BlueFairy,
                    vel,
                    100,
                    Color.White,
                    Main.rand.NextFloat(1.4f, 2.4f));

                dust.noGravity = true;
            }

            // Ice mist
            if (Main.rand.NextBool())
            {
                Dust mist = Dust.NewDustPerfect(
                    Projectile.Center +
                    Main.rand.NextVector2Circular(
                        40f * Projectile.scale,
                        40f * Projectile.scale),
                    DustID.IceTorch,
                    Vector2.Zero,
                    150,
                    Color.Cyan,
                    Main.rand.NextFloat(1.2f, 1.8f));

                mist.noGravity = true;
                mist.velocity *= 0.2f;
            }
        }

        public override bool? Colliding(
            Rectangle projHitbox,
            Rectangle targetHitbox)
        {
            float radius = 60f * Projectile.scale;

            Vector2 closestPoint = new Vector2(
                MathHelper.Clamp(
                    Projectile.Center.X,
                    targetHitbox.Left,
                    targetHitbox.Right),
                MathHelper.Clamp(
                    Projectile.Center.Y,
                    targetHitbox.Top,
                    targetHitbox.Bottom));

            return Vector2.Distance(
                Projectile.Center,
                closestPoint) <= radius;
        }

        public override void OnHitNPC(
            NPC target,
            NPC.HitInfo hit,
            int damageDone)
        {
            target.AddBuff(BuffID.Frostburn2, 600);

            target.velocity *= 0.25f;

            if (Main.rand.NextBool(3))
            {
                target.AddBuff(BuffID.Frozen, 30);
            }

            for (int i = 0; i < 15; i++)
            {
                Vector2 velocity =
                    Main.rand.NextVector2Circular(7f, 7f);

                Dust dust = Dust.NewDustPerfect(
                    target.Center,
                    DustID.BlueFairy,
                    velocity,
                    100,
                    Color.White,
                    2f);

                dust.noGravity = true;
            }

            SoundEngine.PlaySound(
                SoundID.Item27 with
                {
                    Volume = 1f,
                    Pitch = -0.2f
                },
                target.Center);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture =
                Terraria.GameContent.TextureAssets.Projectile[Type].Value;

            Vector2 origin =
                texture.Size() * 0.5f;

            // Large layered trail
            for (int i = 1; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero)
                    continue;

                float t =
                    (Projectile.oldPos.Length - i) /
                    (float)Projectile.oldPos.Length;

                Vector2 pos =
                    Projectile.oldPos[i] +
                    Projectile.Size * 0.5f -
                    Main.screenPosition;

                float scale =
                    Projectile.scale *
                    (1f + t * 0.4f);

                Main.spriteBatch.Draw(
                    texture,
                    pos,
                    null,
                    new Color(100, 220, 255, 0) *
                    (t * 0.8f),
                    Projectile.oldRot[i],
                    origin,
                    scale,
                    SpriteEffects.None,
                    0f);

                Main.spriteBatch.Draw(
                    texture,
                    pos,
                    null,
                    new Color(220, 255, 255, 0) *
                    (t * 0.5f),
                    Projectile.oldRot[i],
                    origin,
                    scale * 0.8f,
                    SpriteEffects.None,
                    0f);
            }

            // Main projectile glow
            Main.spriteBatch.Draw(
                texture,
                Projectile.Center - Main.screenPosition,
                null,
                Color.Cyan,
                Projectile.rotation,
                origin,
                Projectile.scale * 1.2f,
                SpriteEffects.None,
                0f);

            Main.spriteBatch.Draw(
                texture,
                Projectile.Center - Main.screenPosition,
                null,
                Color.White,
                Projectile.rotation,
                origin,
                Projectile.scale,
                SpriteEffects.None,
                0f);

            return false;
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(
                SoundID.Item62 with
                {
                    Volume = 1.2f,
                    Pitch = -0.3f
                },
                Projectile.Center);

            // Massive ice burst
            for (int i = 0; i < 70; i++)
            {
                Vector2 velocity =
                    Main.rand.NextVector2Circular(12f, 12f);

                Dust dust = Dust.NewDustPerfect(
                    Projectile.Center,
                    DustID.IceTorch,
                    velocity,
                    100,
                    Color.Cyan,
                    Main.rand.NextFloat(1.8f, 3.2f));

                dust.noGravity = true;
            }

            // Final explosion hit
            Projectile.NewProjectile(
                Projectile.GetSource_FromThis(),
                Projectile.Center,
                Vector2.Zero,
                ProjectileID.DD2ExplosiveTrapT3Explosion,
                Projectile.damage,
                Projectile.knockBack,
                Projectile.owner);
        }
    }
}
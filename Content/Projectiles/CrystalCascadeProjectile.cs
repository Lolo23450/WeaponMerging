using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using System;

namespace WeaponMerging.Content.Projectiles
{
    public class CrystalCascadeProjectile : ModProjectile
    {
        private const int SLASH_DURATION = 20;
        private float targetAngle;
        private int comboStep;
        private int spriteDirection;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 14;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 50;
            Projectile.height = 50;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = SLASH_DURATION;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.ownerHitCheck = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 14;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            if (Projectile.localAI[0] == 0f)
            {
                Projectile.localAI[0] = 1f;
                targetAngle = Projectile.velocity.ToRotation();
                spriteDirection = (Main.MouseWorld.X < player.Center.X) ? -1 : 1;
            }

            comboStep = (int)Projectile.ai[0];
            Projectile.Center = player.Center;

            float progress = 1f - (Projectile.timeLeft / (float)SLASH_DURATION);
            float easedProgress = EaseOutCubic(progress);

            float swingRange = comboStep switch
            {
                0 => MathHelper.Pi * 1.0f,
                1 => MathHelper.Pi * 1.2f,
                2 => MathHelper.Pi * 1.4f,
                _ => MathHelper.Pi
            };
            float swingStart = comboStep switch
            {
                0 => -swingRange * 0.5f,
                1 => swingRange * 0.5f,
                2 => -swingRange * 0.5f,
                _ => 0f
            };

            Projectile.rotation = targetAngle + swingStart + (swingRange * easedProgress * spriteDirection);
            Projectile.scale = 1.0f + progress * (0.15f + comboStep * 0.1f);

            player.heldProj = Projectile.whoAmI;
            player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.PiOver2 * spriteDirection);

            SpawnComboEffects(progress);

            if (Projectile.timeLeft == SLASH_DURATION)
            {
                SoundStyle swingSound = comboStep switch
                {
                    0 => SoundID.Item1 with { Pitch = 0.3f, Volume = 0.7f },
                    1 => SoundID.Item1 with { Pitch = 0.2f, Volume = 0.75f },
                    2 => SoundID.Item1 with { Pitch = 0.1f, Volume = 0.8f },
                    _ => SoundID.Item1
                };
                SoundEngine.PlaySound(swingSound, Projectile.Center);
            }

            Lighting.AddLight(Projectile.Center, GetComboColor().ToVector3() * 0.5f);
        }

        private void SpawnComboEffects(float progress)
        {
            int dustCount = comboStep switch { 0 => 1, 1 => 2, 2 => 3, _ => 1 };

            for (int i = 0; i < dustCount; i++)
            {
                float angle = Projectile.rotation + (MathHelper.TwoPi * i / dustCount);
                float distance = 28f + comboStep * 8f;
                Vector2 dustPos = Projectile.Center + angle.ToRotationVector2() * distance;

                if (Main.rand.NextBool(2))
                {
                    Dust d = Dust.NewDustPerfect(dustPos, 68,
                        angle.ToRotationVector2().RotatedByRandom(0.2f) * Main.rand.NextFloat(1.5f, 3f),
                        100, GetComboColor(), Main.rand.NextFloat(1.2f, 1.7f));
                    d.noGravity = true;
                    d.fadeIn = 1.0f;
                }
            }
        }

        private Color GetComboColor() =>
            comboStep switch
            {
                0 => Color.LightCyan,
                1 => Color.Cyan,
                2 => Color.DeepSkyBlue,
                _ => Color.White
            };

        private float EaseOutCubic(float x) => 1f - (float)Math.Pow(1f - x, 3);

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (comboStep >= 1) target.AddBuff(BuffID.Frostburn, 120);
            if (comboStep == 2) target.AddBuff(BuffID.Frostburn, 240);

            
            for (int i = 0; i < 10; i++)
            {
                Dust d = Dust.NewDustDirect(target.position, target.width, target.height, 68,
                    Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-3f, 3f), 100, GetComboColor(), 1.5f);
                d.noGravity = true;
            }

            SoundEngine.PlaySound(SoundID.Item27 with { Volume = 0.5f, Pitch = 0.2f }, target.Center);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = ModContent.Request<Texture2D>("WeaponMerging/Content/Items/Weapons/CrystalCascade").Value;
            Vector2 origin = tex.Size() / 2f;
            Color crystalColor = GetComboColor();

            
            int trailCount = comboStep switch { 0 => 6, 1 => 9, 2 => 12, _ => 6 };
            for (int i = 0; i < trailCount && i < Projectile.oldPos.Length; i++)
            {
                float fade = (1f - i / (float)trailCount) * 0.5f;
                Color col = crystalColor * fade;
                Vector2 pos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;

                Main.EntitySpriteDraw(tex, pos, null, col, Projectile.oldRot[i], origin, 
                    Projectile.scale * (1f - i / (float)trailCount * 0.2f), SpriteEffects.None, 0);
            }

            
            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null, 
                crystalColor, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
            
            
            float glowPulse = 0.3f + (float)System.Math.Sin(Main.GlobalTimeWrappedHourly * 4f) * 0.15f;
            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null, 
                Color.White * glowPulse, Projectile.rotation, origin, Projectile.scale * 1.1f, SpriteEffects.None, 0);

            return false;
        }
    }
}


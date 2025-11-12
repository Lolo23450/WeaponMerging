using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using System;

namespace WeaponMerging.Content.Projectiles
{
    public class ShadowReaperProjectile : ModProjectile
    {
        private const int SLASH_DURATION = 18;
        private float targetAngle;
        private int comboStep;
        private int spriteDirection;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 70;
            Projectile.height = 70;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.timeLeft = SLASH_DURATION;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.ownerHitCheck = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 12;
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
                0 => MathHelper.Pi * 1.2f,
                1 => MathHelper.Pi * 1.4f,
                2 => MathHelper.Pi * 1.6f,
                _ => MathHelper.Pi
            };
            float swingStart = comboStep switch
            {
                0 => -swingRange * 0.5f,
                1 => swingRange * 0.5f,
                2 => -swingRange * 0.55f,
                _ => 0f
            };

            Projectile.rotation = targetAngle + swingStart + (swingRange * easedProgress * spriteDirection);
            Projectile.scale = 1.2f + progress * (0.25f + comboStep * 0.15f);

            player.heldProj = Projectile.whoAmI;
            player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.PiOver2 * spriteDirection);

            SpawnComboEffects(progress);

            if (Projectile.timeLeft == SLASH_DURATION)
            {
                SoundStyle swingSound = comboStep switch
                {
                    0 => SoundID.Item71 with { Pitch = 0.1f, Volume = 0.7f },
                    1 => SoundID.Item71 with { Pitch = -0.1f, Volume = 0.8f },
                    2 => SoundID.Item71 with { Pitch = -0.3f, Volume = 0.9f },
                    _ => SoundID.Item71
                };
                SoundEngine.PlaySound(swingSound, Projectile.Center);
            }

            Lighting.AddLight(Projectile.Center, GetComboColor().ToVector3() * 0.7f);
        }

        private void SpawnComboEffects(float progress)
        {
            int dustCount = comboStep switch { 0 => 2, 1 => 3, 2 => 5, _ => 2 };

            for (int i = 0; i < dustCount; i++)
            {
                float angle = Projectile.rotation + (MathHelper.TwoPi * i / dustCount);
                float distance = 35f + comboStep * 15f;
                Vector2 dustPos = Projectile.Center + angle.ToRotationVector2() * distance;

                if (Main.rand.NextBool(2))
                {
                    int dustType = comboStep switch { 0 => 27, 1 => 173, 2 => 54, _ => 27 };

                    Dust d = Dust.NewDustPerfect(dustPos, dustType,
                        angle.ToRotationVector2().RotatedByRandom(0.3f) * Main.rand.NextFloat(2f, 4f),
                        100, default, Main.rand.NextFloat(1.5f, 2.2f));
                    d.noGravity = true;
                }
            }
        }

        private Color GetComboColor() =>
            comboStep switch
            {
                0 => new Color(120, 80, 160),
                1 => new Color(140, 60, 200),
                2 => new Color(180, 80, 255),
                _ => Color.White
            };

        private float EaseOutCubic(float x) => 1f - (float)Math.Pow(1f - x, 3);

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (comboStep == 1) target.AddBuff(BuffID.ShadowFlame, 180);
            if (comboStep == 2)
            {
                target.AddBuff(BuffID.ShadowFlame, 300);
                target.AddBuff(BuffID.CursedInferno, 180);
            }

            
            for (int i = 0; i < 8; i++)
            {
                Dust d = Dust.NewDustDirect(target.position, target.width, target.height, 27,
                    Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-3f, 3f));
                d.noGravity = true;
                d.scale = 1.5f;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = ModContent.Request<Texture2D>("WeaponMerging/Content/Items/Weapons/ShadowReaper").Value;
            Vector2 origin = tex.Size() / 2f;

            
            int trailCount = comboStep switch { 0 => 5, 1 => 7, 2 => 10, _ => 5 };
            for (int i = 0; i < trailCount && i < Projectile.oldPos.Length; i++)
            {
                float fade = (1f - i / (float)trailCount) * 0.6f;
                Color col = GetComboColor() * fade;
                Vector2 pos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;

                Main.EntitySpriteDraw(tex, pos, null, col, Projectile.oldRot[i], origin, Projectile.scale, SpriteEffects.None, 0);
            }

            
            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null, GetComboColor(), Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
            return false;
        }
    }
}


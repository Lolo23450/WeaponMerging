using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using System;

namespace WeaponMerging.Content.Projectiles
{
    public class PainSpiralProjectile : ModProjectile
    {
        private const int SPIN_DURATION = 30;
        private float targetAngle;
        private int comboStep;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 14;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 120;
            Projectile.height = 120;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = SPIN_DURATION;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.ownerHitCheck = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            if (Projectile.localAI[0] == 0f)
            {
                Projectile.localAI[0] = 1f;
                targetAngle = Projectile.velocity.ToRotation();
            }

            comboStep = (int)Projectile.ai[0];
            Projectile.Center = player.Center;

            float progress = 1f - (Projectile.timeLeft / (float)SPIN_DURATION);

            
            float spinEase = EaseOutCubic(EaseInCubic(progress));

            float spinSpeed = comboStep switch
            {
                0 => MathHelper.TwoPi * 1.3f,
                1 => MathHelper.TwoPi * 2.0f,
                2 => MathHelper.TwoPi * 2.9f,
                _ => MathHelper.TwoPi
            };

            Projectile.rotation = targetAngle + (spinSpeed * spinEase);
            Projectile.scale = 1.0f + (progress * (0.35f + comboStep * 0.25f));

            player.heldProj = Projectile.whoAmI;
            player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation);

            SpawnGreenFlameEffects(progress);

            if (Projectile.timeLeft == SPIN_DURATION)
            {
                SoundStyle spinSound = SoundID.Item1 with { Pitch = -0.35f - comboStep * 0.1f, Volume = 0.9f + comboStep * 0.2f };
                SoundEngine.PlaySound(spinSound, Projectile.Center);
            }

            Lighting.AddLight(Projectile.Center, GetComboColor().ToVector3() * 1.3f);
        }

        private void SpawnGreenFlameEffects(float progress)
        {
            
            float radius = 40f + comboStep * 25f;
            int sparkPoints = 6 + comboStep * 2; 

            for (int i = 0; i < sparkPoints; i++)
            {
                float angle = Projectile.rotation + MathHelper.TwoPi * (i / (float)sparkPoints);
                Vector2 pos = Projectile.Center + angle.ToRotationVector2() * radius;

                
                Dust d = Dust.NewDustPerfect(pos, DustID.GreenTorch);
                d.velocity = angle.ToRotationVector2().RotatedByRandom(0.4f) * Main.rand.NextFloat(0.6f, 1.8f);
                d.scale = 1.2f + comboStep * 0.25f;
                d.fadeIn = 1.2f;
                d.noGravity = true;
            }

            
            if (Projectile.timeLeft < SPIN_DURATION * 0.35f && Main.rand.NextBool(4 - comboStep))
            {
                Vector2 glowPos = Projectile.Center + Main.rand.NextVector2Circular(radius * 0.6f, radius * 0.6f);
                Dust glow = Dust.NewDustPerfect(glowPos, DustID.FlameBurst, Vector2.Zero, 50, GetComboColor(), 2.1f + comboStep * 0.3f);
                glow.noGravity = true;
                glow.fadeIn = 1.5f;
            }

            
            if (comboStep == 2 && Main.rand.NextBool(6)) 
            {
                Vector2 arcStart = Projectile.Center + Main.rand.NextVector2Circular(radius, radius);
                Vector2 arcVel = (Projectile.Center - arcStart).SafeNormalize(Vector2.UnitX) * Main.rand.NextFloat(1.5f, 3.3f);
                Dust arc = Dust.NewDustPerfect(arcStart, DustID.Electric, arcVel, 150, GetComboColor(), 1.6f);
                arc.noGravity = true;
            }
        }

        private Color GetComboColor() =>
            comboStep switch
            {
                0 => new Color(90, 255, 120),
                1 => new Color(120, 255, 140),
                2 => new Color(180, 255, 200),
                _ => Color.White
            };

        private float EaseOutCubic(float x) => 1f - (float)Math.Pow(1f - x, 3);
        private float EaseInCubic(float x) => x * x * x;

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (comboStep >= 1) target.AddBuff(BuffID.Poisoned, 300);
            if (comboStep == 2) target.AddBuff(BuffID.Venom, 240);

            for (int i = 0; i < 14; i++)
            {
                Dust d = Dust.NewDustDirect(target.position, target.width, target.height, DustID.GreenTorch,
                    Main.rand.NextFloat(-5f, 5f), Main.rand.NextFloat(-5f, 5f), 0, default, 1.9f);
                d.noGravity = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = ModContent.Request<Texture2D>("WeaponMerging/Content/Items/Weapons/PainSpiral").Value;
            Vector2 origin = tex.Size() / 2f;

            int trail = 8 + comboStep * 4;
            for (int i = 0; i < trail && i < Projectile.oldPos.Length; i++)
            {
                float fade = (1f - i / (float)trail) * 0.6f;
                Color col = GetComboColor() * fade;
                Vector2 pos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;

                Main.EntitySpriteDraw(tex, pos, null, col, Projectile.oldRot[i], origin, Projectile.scale, SpriteEffects.None, 0);
            }

            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null, GetComboColor(), Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
            return false;
        }
    }
}


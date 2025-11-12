using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using System;

namespace WeaponMerging.Content.Projectiles
{
    public class FossilDancerProjectile : ModProjectile
    {
        private const int SLASH_DURATION = 20;
        private float startAngle;
        private float endAngle;
        private int comboStep; 
        private int spriteDirection;
        private float baseAngle;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 12;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 128;
            Projectile.height = 112;
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
                    endAngle = MathHelper.PiOver4 * 1.5f;
                    Projectile.scale = 1.3f + progress * 0.3f;
                    Projectile.width = 168;
                    Projectile.height = 146;
                    break;

                case 1: 
                    startAngle = MathHelper.PiOver4 * 1.5f;
                    endAngle = -MathHelper.PiOver4 * 2.5f;
                    Projectile.scale = 1.4f + progress * 0.4f;
                    Projectile.width = 180;
                    Projectile.height = 158;
                    break;

                case 2: 
                    startAngle = -MathHelper.Pi;
                    endAngle = MathHelper.Pi;
                    Projectile.scale = 1.7f + progress * 0.5f;
                    Projectile.width = 218;
                    Projectile.height = 190;
                    break;

                case 3: 
                    startAngle = -MathHelper.Pi * 0.8f;
                    endAngle = MathHelper.Pi * 0.3f;
                    Projectile.scale = 2.5f + progress * 0.8f;
                    Projectile.width = 320;
                    Projectile.height = 280;
                    break;
            }

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
                    0 => SoundID.Item1 with { Pitch = 0.2f, Volume = 0.8f },
                    1 => SoundID.Item1 with { Pitch = 0.4f, Volume = 0.8f },
                    2 => SoundID.Item71 with { Pitch = -0.2f, Volume = 0.9f },
                    3 => SoundID.Item71 with { Pitch = -0.5f, Volume = 1.0f },
                    _ => SoundID.Item1
                };
                SoundEngine.PlaySound(swingSound, Projectile.position);
            }

            Color lightColor = GetComboColor();
            Lighting.AddLight(Projectile.Center, lightColor.ToVector3() * 0.8f);
        }

        private void SpawnComboEffects(Player player, float progress)
        {
            Vector2 dustPos = Projectile.Center + Projectile.rotation.ToRotationVector2() * 60f;

            if (Main.rand.NextBool(2))
            {
                int dustType = comboStep switch
                {
                    0 => 32,
                    1 => 36,
                    2 => 159,
                    3 => 31,
                    _ => 32
                };

                Dust dust = Dust.NewDustPerfect(dustPos, dustType,
                    Projectile.rotation.ToRotationVector2().RotatedByRandom(0.5f) * Main.rand.NextFloat(2f, 8f),
                    100, default, Main.rand.NextFloat(1.5f, 2.5f));
                dust.noGravity = true;
            }

            if (comboStep == 3 && progress > 0.7f && Main.rand.NextBool(1))
            {
                Dust fire = Dust.NewDustPerfect(dustPos, 6,
                    Main.rand.NextVector2Circular(4f, 4f),
                    100, default, 2f);
                fire.noGravity = true;
            }
        }

        private Color GetComboColor()
        {
            return comboStep switch
            {
                0 => new Color(200, 200, 180),
                1 => new Color(220, 180, 120),
                2 => new Color(180, 150, 100),
                3 => new Color(255, 180, 100),
                _ => Color.White
            };
        }

        private float EaseOutCubic(float x)
        {
            return 1f - (float)Math.Pow(1f - x, 3);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            switch (comboStep)
            {
                case 0:
                    for (int i = 0; i < 8; i++)
                    {
                        Dust.NewDust(target.position, target.width, target.height, 32,
                            Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-3f, 3f), 100, default, 1.5f);
                    }
                    break;

                case 1:
                    for (int i = 0; i < 12; i++)
                    {
                        Dust.NewDust(target.position, target.width, target.height, 36,
                            Main.rand.NextFloat(-4f, 4f), Main.rand.NextFloat(-4f, 4f), 100, default, 1.8f);
                    }
                    break;

                case 2:
                    SoundEngine.PlaySound(SoundID.NPCHit2, target.position);
                    for (int i = 0; i < 15; i++)
                    {
                        Dust.NewDust(target.position, target.width, target.height, 159,
                            Main.rand.NextFloat(-5f, 5f), Main.rand.NextFloat(-5f, 5f), 100, default, 2.0f);
                    }

                    if (Main.myPlayer == Projectile.owner)
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            Vector2 velocity = Main.rand.NextVector2Circular(6f, 6f);
                            int spawned = Projectile.NewProjectile(
                                Projectile.GetSource_FromThis(),
                                target.Center,
                                velocity,
                                ProjectileID.Bone,
                                (int)(Projectile.damage * 0.4f),
                                1f,
                                Projectile.owner
                            );
                            if (spawned >= 0 && spawned < Main.maxProjectiles)
                                Main.projectile[spawned].timeLeft = 60;
                        }
                    }
                    break;

                case 3:
                    SoundEngine.PlaySound(SoundID.Item14 with { Pitch = -0.5f, Volume = 0.7f }, target.position);

                    for (int i = 0; i < 30; i++)
                    {
                        int dustType = Main.rand.NextBool() ? 6 : 31;
                        Dust.NewDust(target.position, target.width, target.height, dustType,
                            Main.rand.NextFloat(-8f, 8f), Main.rand.NextFloat(-8f, 8f), 100, default, 2.5f);
                    }

                    if (Main.myPlayer == Projectile.owner)
                    {
                        int explosion = Projectile.NewProjectile(
                            Projectile.GetSource_FromThis(),
                            target.Center,
                            Vector2.Zero,
                            ProjectileID.DD2ExplosiveTrapT1Explosion,
                            (int)(Projectile.damage * 0.8f),
                            4f,
                            Projectile.owner
                        );
                        if (explosion >= 0 && explosion < Main.maxProjectiles)
                            Main.projectile[explosion].timeLeft = 30;
                            for (int i = 0; i < 6; i++)
                            {
                                float angle = MathHelper.TwoPi * i / 6f;
                                if (spriteDirection == -1)
                                    angle = MathHelper.Pi - angle; 

                                Vector2 velocity = angle.ToRotationVector2() * 8f;
                                int spawned = Projectile.NewProjectile(
                                    Projectile.GetSource_FromThis(),
                                    target.Center,
                                    velocity,
                                    ProjectileID.Bone,
                                    (int)(Projectile.damage * 0.5f),
                                    2f,
                                    Projectile.owner
                                );
                                if (spawned >= 0 && spawned < Main.maxProjectiles)
                                    Main.projectile[spawned].timeLeft = 60;
                            }
                    }
                    break;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Player player = Main.player[Projectile.owner];
            int spriteDirection = player.direction;
            Texture2D swordTexture = ModContent.Request<Texture2D>("WeaponMerging/Content/Items/Weapons/FossilDancer").Value;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            Rectangle sourceRect = new Rectangle(0, 0, swordTexture.Width, swordTexture.Height);
            Vector2 origin = spriteDirection == -1
                ? new Vector2(swordTexture.Width - 10, swordTexture.Height - 10)
                : new Vector2(10, swordTexture.Height - 10);

            Color comboColor = GetComboColor();

            SpriteEffects effects = spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldRot[i] == 0) continue;

                float trailProgress = i / (float)Projectile.oldPos.Length;
                float trailAlpha = (1f - trailProgress) * 0.5f;
                Color trailColor = comboColor * trailAlpha;

                Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;

                Main.EntitySpriteDraw(
                    swordTexture,
                    trailPos,
                    sourceRect,
                    trailColor,
                    Projectile.oldRot[i],
                    origin,
                    Projectile.scale * (1f - trailProgress * 0.3f),
                    effects,
                    0
                );
            }

            Color mainColor = Color.Lerp(comboColor, lightColor, 0.3f);

            Main.EntitySpriteDraw(
                swordTexture,
                drawPosition,
                sourceRect,
                mainColor,
                Projectile.rotation,
                origin,
                Projectile.scale,
                effects,
                0
            );

            if (comboStep >= 2)
            {
                Color glowColor = comboColor * 0.6f * (1f - (Projectile.timeLeft / (float)SLASH_DURATION));
                Main.EntitySpriteDraw(
                    swordTexture,
                    drawPosition,
                    sourceRect,
                    glowColor,
                    Projectile.rotation,
                    origin,
                    Projectile.scale * 1.1f,
                    effects,
                    0
                );
            }

            return false;
        }
    }
}

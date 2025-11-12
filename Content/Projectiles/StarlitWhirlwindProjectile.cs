using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using System;

namespace WeaponMerging.Content.Projectiles
{
    public class StarlitWhirlwindProjectile : ModProjectile
    {
        private const int SPIN_DURATION = 28;
        private float targetAngle;
        private int comboStep;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 16;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 100;
            Projectile.height = 100;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.timeLeft = SPIN_DURATION;
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
            }

            comboStep = (int)Projectile.ai[0];
            Projectile.Center = player.Center;

            float progress = 1f - (Projectile.timeLeft / (float)SPIN_DURATION);
            float spinEase = EaseOutCubic(EaseInCubic(progress));

            float spinSpeed = comboStep switch
            {
                0 => MathHelper.TwoPi * 1.2f,
                1 => MathHelper.TwoPi * 1.8f,
                2 => MathHelper.TwoPi * 2.5f,
                _ => MathHelper.TwoPi
            };

            Projectile.rotation = targetAngle + (spinSpeed * spinEase);
            Projectile.scale = 0.9f + (progress * (0.3f + comboStep * 0.2f));

            player.heldProj = Projectile.whoAmI;
            player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation);

            SpawnStarEffects(progress);
            
            
            TriggerOrbSynergy(player);

            if (Projectile.timeLeft == SPIN_DURATION)
            {
                SoundStyle spinSound = SoundID.Item9 with { Pitch = 0.2f - comboStep * 0.15f, Volume = 0.7f + comboStep * 0.15f };
                SoundEngine.PlaySound(spinSound, Projectile.Center);
            }

            Lighting.AddLight(Projectile.Center, GetComboColor().ToVector3() * 1.2f);
        }

        private void TriggerOrbSynergy(Player player)
        {
            
            var modPlayer = player.GetModPlayer<Content.Players.StarlitModPlayer>();
            
            if (modPlayer.orbCount > 0 && Projectile.timeLeft % 8 == 0)
            {
                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    Projectile orb = Main.projectile[i];
                    if (orb.active && 
                        orb.owner == player.whoAmI && 
                        orb.type == ModContent.ProjectileType<StarlitOrbProjectile>() &&
                        orb.ai[1] == 0f) 
                    {
                        
                        NPC target = FindNearestTarget(orb.Center, 400f);
                        if (target != null && Main.myPlayer == player.whoAmI)
                        {
                            Vector2 velocity = (target.Center - orb.Center).SafeNormalize(Vector2.Zero) * 10f;
                            
                            Projectile.NewProjectile(
                                Projectile.GetSource_FromThis(),
                                orb.Center,
                                velocity,
                                ProjectileID.StarCannonStar,
                                (int)(Projectile.damage * 0.2f),
                                1f,
                                player.whoAmI
                            );
                            
                            
                            for (int j = 0; j < 3; j++)
                            {
                                Dust d = Dust.NewDustPerfect(orb.Center, DustID.YellowStarDust, velocity * 0.3f);
                                d.noGravity = true;
                                d.scale = 1.2f;
                            }
                        }
                    }
                }
            }
        }

        private NPC FindNearestTarget(Vector2 position, float maxRange)
        {
            NPC best = null;
            float bestDist = maxRange;
            foreach (NPC npc in Main.npc)
            {
                if (npc.active && !npc.friendly && npc.life > 0 && !npc.dontTakeDamage && npc.CanBeChasedBy())
                {
                    float d = Vector2.Distance(position, npc.Center);
                    if (d < bestDist)
                    {
                        bestDist = d;
                        best = npc;
                    }
                }
            }
            return best;
        }

        private void SpawnStarEffects(float progress)
        {
            float radius = 35f + comboStep * 20f;
            int starPoints = 5 + comboStep * 2;

            for (int i = 0; i < starPoints; i++)
            {
                float angle = Projectile.rotation + MathHelper.TwoPi * (i / (float)starPoints);
                Vector2 pos = Projectile.Center + angle.ToRotationVector2() * radius;

                if (Main.rand.NextBool(2))
                {
                    Dust d = Dust.NewDustPerfect(pos, DustID.YellowStarDust);
                    d.velocity = angle.ToRotationVector2().RotatedByRandom(0.5f) * Main.rand.NextFloat(0.5f, 1.5f);
                    d.scale = 1.1f + comboStep * 0.2f;
                    d.fadeIn = 1.1f;
                    d.noGravity = true;
                }
            }

            
            if (Projectile.timeLeft < SPIN_DURATION * 0.4f && Main.rand.NextBool(3 - comboStep))
            {
                Vector2 glowPos = Projectile.Center + Main.rand.NextVector2Circular(radius * 0.7f, radius * 0.7f);
                Dust glow = Dust.NewDustPerfect(glowPos, DustID.SilverFlame, Vector2.Zero, 50, GetComboColor(), 1.8f + comboStep * 0.25f);
                glow.noGravity = true;
                glow.fadeIn = 1.4f;
            }
        }

        private Color GetComboColor() =>
            comboStep switch
            {
                0 => new Color(255, 255, 150),
                1 => new Color(200, 220, 255),
                2 => new Color(255, 200, 255),
                _ => Color.White
            };

        private float EaseOutCubic(float x) => 1f - (float)Math.Pow(1f - x, 3);
        private float EaseInCubic(float x) => x * x * x;

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            
            for (int i = 0; i < 10 + comboStep * 4; i++)
            {
                Dust d = Dust.NewDustDirect(target.position, target.width, target.height, DustID.YellowStarDust,
                    Main.rand.NextFloat(-4f, 4f), Main.rand.NextFloat(-4f, 4f), 0, default, 1.5f);
                d.noGravity = true;
            }

            SoundEngine.PlaySound(SoundID.Item10 with { Volume = 0.5f, Pitch = 0.4f }, target.position);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = ModContent.Request<Texture2D>("WeaponMerging/Content/Items/Weapons/StarlitWhirlwind").Value;
            Vector2 origin = tex.Size() / 2f;

            int trail = 10 + comboStep * 3;
            for (int i = 0; i < trail && i < Projectile.oldPos.Length; i++)
            {
                float fade = (1f - i / (float)trail) * 0.65f;
                Color col = GetComboColor() * fade;
                Vector2 pos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;

                Main.EntitySpriteDraw(tex, pos, null, col, Projectile.oldRot[i], origin, Projectile.scale * 0.95f, SpriteEffects.None, 0);
            }

            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null, GetComboColor(), Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
            
            Color glowColor = GetComboColor() * 0.5f;
            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null, glowColor, Projectile.rotation, origin, Projectile.scale * 1.1f, SpriteEffects.None, 0);

            return false;
        }
    }
}

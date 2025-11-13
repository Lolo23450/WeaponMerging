using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;

namespace WeaponMerging.Content.Projectiles
{
    public class CrystalOrbProjectile : ModProjectile
    {
        private const float ORBIT_RADIUS = 100f;
        private const float ORBIT_SPEED = 0.4f;
        private int trailCounter = 0;
        private float growthScale = 0.9f;
        public int absorbedCount = 0;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 12;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = false;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 99999;
        }

        public override void AI()
        {
            if (Projectile.owner < 0 || Projectile.owner >= Main.maxPlayers)
            {
                Projectile.Kill();
                return;
            }

            Player owner = Main.player[Projectile.owner];
            if (!owner.active || owner.HeldItem.ModItem is not Items.Weapons.CrystalCascade)
            {
                Projectile.Kill();
                return;
            }

            var modPlayer = owner.GetModPlayer<Players.CrystalCascadePlayer>();
            int crystalCount = modPlayer?.crystalCount ?? 1;
            if (crystalCount < 1) crystalCount = 1;

            var accessoryPlayer = owner.GetModPlayer<Players.AccessoryEffectsPlayer>();
            float speedMult = accessoryPlayer.orbSpeedMultipliers.TryGetValue("Crystal", out float mult) ? mult : 1f;
            
            float index = Projectile.localAI[0];
            if (index < 0) index = 0;
            if (index > crystalCount - 1) index = crystalCount - 1;

            
            float speedVar = (Projectile.whoAmI % 7) * 0.01f; 
            float globalAngle = Main.GlobalTimeWrappedHourly * ORBIT_SPEED * (1f + speedVar) * speedMult;
            float phaseJitter = (float)System.Math.Sin(Main.GlobalTimeWrappedHourly * (0.6f + Projectile.whoAmI * 0.03f)) * 0.25f;
            float myAngle = globalAngle + (MathHelper.TwoPi * index / crystalCount) + phaseJitter;

            
            float pulseRadius = ORBIT_RADIUS + (float)System.Math.Sin(globalAngle * 3f + index * 0.2f) * 8f;
            Vector2 orbitTarget = owner.Center + myAngle.ToRotationVector2() * (pulseRadius + crystalCount * 3f);

            
            Vector2 baseVel = (orbitTarget - Projectile.Center) * 0.22f;

            
            
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                if (i == Projectile.whoAmI) continue;

                Projectile other = Main.projectile[i];
                if (other.active && other.type == Projectile.type && other.owner == Projectile.owner)
                {
                    float distance = Vector2.Distance(Projectile.Center, other.Center);
                    if (distance < 60f && distance > 0f)
                    {
                        
                        Vector2 toOther = (other.Center - Projectile.Center);
                        Vector2 dir = toOther.SafeNormalize(Vector2.UnitX);

                        
                        if (Projectile.whoAmI < other.whoAmI)
                        {
                            
                            baseVel += dir * 0.7f;
                            
                            other.velocity -= dir * 0.35f;
                        }
                        else if (Projectile.whoAmI > other.whoAmI)
                        {
                            
                            baseVel += dir * 0.35f;
                        }

                        
                        if (distance < 18f && Projectile.whoAmI < other.whoAmI)
                        {
                            AbsorbCrystal(other);
                            
                            other.Kill();
                            
                            
                            if (modPlayer != null)
                                modPlayer.ReassignCrystalIndices();
                        }
                    }
                }
            }

            
            Projectile.velocity = Vector2.Lerp(Projectile.velocity, baseVel, 0.18f);

            
            Projectile.scale = growthScale;

            
            Projectile.rotation += 0.08f + absorbedCount * 0.02f;

            
            float lightIntensity = 0.3f + absorbedCount * 0.15f;
            Lighting.AddLight(Projectile.Center, new Vector3(0.3f, 0.8f, 1.0f) * lightIntensity);

            trailCounter++;

            
            if (trailCounter % 3 == 0)
            {
                Color trailColor = GetCrystalColor();
                Dust dust = Dust.NewDustPerfect(Projectile.Center, 68,
                    -Projectile.velocity * 0.2f, 100, trailColor, Main.rand.NextFloat(0.9f, 1.4f) * growthScale);
                dust.noGravity = true;
                dust.alpha = 120;
            }

            
            if (absorbedCount > 0 && Main.rand.NextBool(8))
            {
                Vector2 offset = Main.rand.NextVector2Circular(20 * growthScale, 20 * growthScale);
                Dust sparkle = Dust.NewDustPerfect(Projectile.Center + offset, 68,
                    Vector2.Zero, 140, Color.White, Main.rand.NextFloat(0.7f, 1.2f));
                sparkle.noGravity = true;
            }
        }

        private void AbsorbCrystal(Projectile other)
        {
            absorbedCount++;
            growthScale += 0.4f; 

            
            for (int i = 0; i < 20; i++)
            {
                Vector2 direction = (Projectile.Center - other.Center).SafeNormalize(Vector2.UnitX);
                Vector2 perpendicular = direction.RotatedBy(MathHelper.PiOver2);

                Vector2 dustVel = Vector2.Lerp(
                    other.Center + Main.rand.NextVector2Circular(10, 10),
                    Projectile.Center,
                    Main.rand.NextFloat(0.3f, 0.7f)
                ) - other.Center;

                Dust dust = Dust.NewDustPerfect(other.Center, 68, dustVel * 0.3f, 100,
                    GetCrystalColor(), Main.rand.NextFloat(1.2f, 2.0f));
                dust.noGravity = true;
                dust.fadeIn = 1.4f;
            }

            
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.ToRadians(i * 45);
                Vector2 vel = new Vector2(3f, 0).RotatedBy(angle);
                Dust sparkle = Dust.NewDustPerfect(Projectile.Center, 68, vel, 120, Color.White, 1.5f);
                sparkle.noGravity = true;
            }

            SoundEngine.PlaySound(SoundID.Item29 with { Volume = 0.6f, Pitch = 0.2f + absorbedCount * 0.1f }, Projectile.Center);
        }

        private Color GetCrystalColor()
        {
            
            return absorbedCount switch
            {
                0 => Color.LightCyan,
                1 => Color.Cyan,
                2 => Color.DeepSkyBlue,
                _ => Color.RoyalBlue
            };
        }

        public override void Kill(int timeLeft)
        {
            
            for (int i = 0; i < 15 + absorbedCount * 5; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, 68,
                    Main.rand.NextFloat(-4f, 4f), Main.rand.NextFloat(-4f, 4f), 100, GetCrystalColor(),
                    Main.rand.NextFloat(1.2f, 2.0f) * growthScale);
                dust.noGravity = true;
            }

            SoundEngine.PlaySound(SoundID.Item27 with { Volume = 0.5f, Pitch = -0.2f }, Projectile.Center);

            
            if (Projectile.owner >= 0 && Projectile.owner < Main.maxPlayers)
            {
                var modPlayer = Main.player[Projectile.owner].GetModPlayer<Players.CrystalCascadePlayer>();
                modPlayer?.ReassignCrystalIndices();
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 origin = tex.Size() / 2f;
            Color crystalColor = GetCrystalColor();

            
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                float progress = i / (float)Projectile.oldPos.Length;
                float alpha = (1f - progress) * 0.4f;
                Color trailColor = crystalColor * alpha;

                Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;

                Main.EntitySpriteDraw(tex, trailPos, null, trailColor, Projectile.oldRot[i],
                    origin, Projectile.scale * (1f - progress * 0.3f), SpriteEffects.None, 0);
            }

            
            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null,
                crystalColor, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);

            
            if (absorbedCount > 0)
            {
                float glowIntensity = 0.3f + (float)System.Math.Sin(Main.GlobalTimeWrappedHourly * 3f) * 0.2f;
                Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null,
                    Color.White * glowIntensity, Projectile.rotation, origin, Projectile.scale * 1.15f, SpriteEffects.None, 0);
            }

            return false;
        }
    }
}


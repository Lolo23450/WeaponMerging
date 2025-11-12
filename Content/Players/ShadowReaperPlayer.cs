using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace WeaponMerging.Content.Players
{
    public class ShadowReaperPlayer : ModPlayer
    {
        public int orbCount = 0;
        private List<int> orbProjectiles = new List<int>();

        public override void ResetEffects()
        {
            
            orbProjectiles.RemoveAll(id => id < 0 || id >= Main.maxProjectiles || !Main.projectile[id].active);
            orbCount = orbProjectiles.Count;
        }

        private void Attack4AbyssalRequiem(Player player, EntitySource_ItemUse_WithAmmo source, int damage, float knockback)
        {
            
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 velocity = angle.ToRotationVector2() * 24f;
                int beam = Projectile.NewProjectile(source, player.Center, velocity,
                    ProjectileID.ShadowBeamHostile, (int)(damage * 1.2f), knockback * 1.3f, player.whoAmI);
                if (beam >= 0 && beam < Main.maxProjectiles)
                {
                    Main.projectile[beam].friendly = true;
                    Main.projectile[beam].hostile = false;
                    Main.projectile[beam].DamageType = DamageClass.Magic;
                    Main.projectile[beam].timeLeft = 210;
                }
            }

            
            for (int i = 0; i < 10; i++)
            {
                float angle = MathHelper.ToRadians(i * 36);
                Vector2 vel = new Vector2(16f + i * 0.4f, 0).RotatedBy(angle);
                int scythe = Projectile.NewProjectile(source, player.Center, vel,
                    ProjectileID.DeathSickle, (int)(damage * 0.9f), knockback, player.whoAmI);
                if (scythe >= 0 && scythe < Main.maxProjectiles)
                {
                    Main.projectile[scythe].DamageType = DamageClass.Magic;
                }
            }

            
            for (int i = 0; i < 60; i++)
            {
                Vector2 v = Main.rand.NextVector2CircularEdge(12f, 12f);
                Dust d = Dust.NewDustPerfect(player.Center, 173, v, 100, Color.MediumPurple, Main.rand.NextFloat(2.2f, 3.6f));
                d.noGravity = true;
            }
            SoundEngine.PlaySound(SoundID.Item71 with { Pitch = -0.2f }, player.Center);
        }

        public void GainOrb(Player player)
        {
            int maxOrbs = 3 + player.GetModPlayer<Content.Players.AccessoryEffectsPlayer>().BonusMaxOrbs;
            if (orbCount >= maxOrbs) return;

            
            int orb = Projectile.NewProjectile(
                player.GetSource_FromThis(),
                player.Center,
                Vector2.Zero,
                ModContent.ProjectileType<Content.Projectiles.ShadowOrbProjectile>(),
                0, 0f, player.whoAmI
            );

            if (orb >= 0 && orb < Main.maxProjectiles)
            {
                Main.projectile[orb].ai[0] = orbCount; 
                Main.projectile[orb].localAI[0] = orbCount;
                orbProjectiles.Add(orb);
                orbCount++;

                
                for (int i = 0; i < 20; i++)
                {
                    Dust dust = Dust.NewDustPerfect(player.Center, 27,
                        Main.rand.NextVector2Circular(4f, 4f), 100, Color.Purple, Main.rand.NextFloat(1.5f, 2.2f));
                    dust.noGravity = true;
                }

                SoundEngine.PlaySound(SoundID.Item8 with { Pitch = 0.3f }, player.Center);
            }
        }

        public void UseOrbAttack(Player player, EntitySource_ItemUse_WithAmmo source, int damage, float knockback)
        {
            if (orbCount == 0) return;

            
            switch (orbCount)
            {
                case 1:
                    
                    Attack1ShadowBeam(player, source, damage, knockback);
                    break;
                case 2:
                    
                    Attack2ReaperWave(player, source, damage, knockback);
                    break;
                case 3:
                    
                    Attack3DeathStorm(player, source, damage, knockback);
                    break;
                default:
                    
                    Attack4AbyssalRequiem(player, source, damage, knockback);
                    break;
            }

            
            var kept = new List<int>();
            var acc = player.GetModPlayer<Content.Players.AccessoryEffectsPlayer>();
            foreach (int orbID in orbProjectiles)
            {
                if (orbID >= 0 && orbID < Main.maxProjectiles && Main.projectile[orbID].active)
                {
                    if (acc.RollPersist())
                    {
                        kept.Add(orbID); 
                    }
                    else
                    {
                        Main.projectile[orbID].Kill();
                    }
                }
            }
            orbProjectiles = kept;
            orbCount = orbProjectiles.Count;
        }

        private void Attack1ShadowBeam(Player player, EntitySource_ItemUse_WithAmmo source, int damage, float knockback)
        {
            
            Vector2 velocity = (Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitX) * 20f;

            
            for (int i = 0; i < 30; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(50, 50);
                Dust dust = Dust.NewDustPerfect(player.Center + offset, 27,
                    -offset * 0.1f, 100, Color.Purple, Main.rand.NextFloat(1.5f, 2.5f));
                dust.noGravity = true;
            }

            int proj = Projectile.NewProjectile(source, player.Center, velocity,
                ProjectileID.ShadowBeamHostile, (int)(damage * 1.2f), knockback * 1.5f, player.whoAmI);

            if (proj >= 0 && proj < Main.maxProjectiles)
            {
                Main.projectile[proj].friendly = true;
                Main.projectile[proj].hostile = false;
                Main.projectile[proj].DamageType = DamageClass.Magic;
                Main.projectile[proj].timeLeft = 180;
            }

            SoundEngine.PlaySound(SoundID.Item8, player.Center);
        }

        private void Attack2ReaperWave(Player player, EntitySource_ItemUse_WithAmmo source, int damage, float knockback)
        {
            
            Vector2 baseVelocity = (Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitX) * 16f;

            for (int i = 0; i < 5; i++)
            {
                float spread = MathHelper.ToRadians((i - 2) * 15);
                Vector2 velocity = baseVelocity.RotatedBy(spread);

                int proj = Projectile.NewProjectile(source, player.Center, velocity,
                    ProjectileID.DeathSickle, damage, knockback, player.whoAmI);

                if (proj >= 0 && proj < Main.maxProjectiles)
                {
                    Main.projectile[proj].DamageType = DamageClass.Magic;
                }
            }

            
            for (int i = 0; i < 40; i++)
            {
                float angle = MathHelper.ToRadians(i * 9);
                Vector2 offset = new Vector2(60, 0).RotatedBy(angle + baseVelocity.ToRotation());
                Dust dust = Dust.NewDustPerfect(player.Center + offset, 173,
                    offset.SafeNormalize(Vector2.UnitX) * 3f, 100, Color.MediumPurple, 2.0f);
                dust.noGravity = true;
            }

            SoundEngine.PlaySound(SoundID.Item71, player.Center);
        }

        private void Attack3DeathStorm(Player player, EntitySource_ItemUse_WithAmmo source, int damage, float knockback)
        {
            
            
            
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.ToRadians(i * 45);
                Vector2 velocity = new Vector2(14f, 0).RotatedBy(angle);

                int scythe = Projectile.NewProjectile(source, player.Center, velocity,
                    ProjectileID.DeathSickle, (int)(damage * 0.8f), knockback * 0.8f, player.whoAmI);

                if (scythe >= 0 && scythe < Main.maxProjectiles)
                {
                    Main.projectile[scythe].DamageType = DamageClass.Magic;
                }
            }

            
            for (int i = 0; i < 4; i++)
            {
                float angle = MathHelper.ToRadians(i * 90);
                Vector2 velocity = new Vector2(22f, 0).RotatedBy(angle);

                int beam = Projectile.NewProjectile(source, player.Center, velocity,
                    ProjectileID.ShadowBeamHostile, (int)(damage * 1.1f), knockback * 1.2f, player.whoAmI);

                if (beam >= 0 && beam < Main.maxProjectiles)
                {
                    Main.projectile[beam].friendly = true;
                    Main.projectile[beam].hostile = false;
                    Main.projectile[beam].DamageType = DamageClass.Magic;
                    Main.projectile[beam].timeLeft = 180;
                }
            }

            
            for (int i = 0; i < 60; i++)
            {
                Vector2 velocity = Main.rand.NextVector2CircularEdge(10f, 10f);
                Dust dust = Dust.NewDustPerfect(player.Center, 27, velocity, 100, Color.Purple, Main.rand.NextFloat(2.0f, 3.5f));
                dust.noGravity = true;
            }

            for (int i = 0; i < 40; i++)
            {
                Dust dust = Dust.NewDustPerfect(player.Center, 54, Main.rand.NextVector2Circular(8f, 8f), 100, Color.DarkViolet, Main.rand.NextFloat(2.5f, 4.0f));
                dust.noGravity = true;
            }

            SoundEngine.PlaySound(SoundID.Item62 with { Volume = 1.0f, Pitch = -0.4f }, player.Center);
        }
    }
}


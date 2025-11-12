using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace WeaponMerging.Content.Players
{
    public class InfernoPlayer : ModPlayer
    {
        public int orbCount = 0;
        private List<int> orbProjectiles = new List<int>();

        public override void ResetEffects()
        {
            orbProjectiles.RemoveAll(id => id < 0 || id >= Main.maxProjectiles || !Main.projectile[id].active);
            orbCount = orbProjectiles.Count;
        }

        private void Attack5Cataclysm(Player player, EntitySource_ItemUse_WithAmmo source, int damage, float knockback)
        {
            
            for (int i = 0; i < 24; i++)
            {
                float angle = MathHelper.TwoPi * i / 24f;
                Vector2 velocity = angle.ToRotationVector2() * 22f;
                int meteor = Projectile.NewProjectile(source, player.Center, velocity,
                    ProjectileID.Meteor1, (int)(damage * 1.3f), knockback * 1.3f, player.whoAmI);
                if (meteor >= 0 && meteor < Main.maxProjectiles)
                {
                    Main.projectile[meteor].DamageType = DamageClass.Magic;
                }
            }

            
            for (int i = 0; i < 16; i++)
            {
                float angle = MathHelper.ToRadians(i * 22.5f);
                Vector2 velocity = new Vector2(18f + i * 0.4f, 0).RotatedBy(angle);
                int proj = Projectile.NewProjectile(source, player.Center, velocity,
                    ProjectileID.BallofFire, (int)(damage * 1.1f), knockback, player.whoAmI);
                if (proj >= 0 && proj < Main.maxProjectiles)
                {
                    Main.projectile[proj].DamageType = DamageClass.Magic;
                }
            }

            
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                Vector2 velocity = angle.ToRotationVector2() * 20f;
                int ray = Projectile.NewProjectile(source, player.Center, velocity,
                    ProjectileID.SolarFlareRay, (int)(damage * 1.2f), knockback * 1.2f, player.whoAmI);
                if (ray >= 0 && ray < Main.maxProjectiles)
                {
                    Main.projectile[ray].DamageType = DamageClass.Magic;
                    Main.projectile[ray].timeLeft = 210;
                }
            }

            
            for (int i = 0; i < 80; i++)
            {
                Vector2 velocity = Main.rand.NextVector2CircularEdge(14f, 14f);
                Dust dust = Dust.NewDustPerfect(player.Center, 174, velocity, 100, Color.OrangeRed, Main.rand.NextFloat(3.0f, 5.0f));
                dust.noGravity = true;
            }
            player.AddBuff(BuffID.Inferno, 900);
            SoundEngine.PlaySound(SoundID.Item74 with { Volume = 1.0f, Pitch = 0.4f }, player.Center);
        }

        public void GainOrb(Player player)
        {
            int maxOrbs = 4 + player.GetModPlayer<Content.Players.AccessoryEffectsPlayer>().BonusMaxOrbs;
            if (orbCount >= maxOrbs) return;

            int orb = Projectile.NewProjectile(
                player.GetSource_FromThis(),
                player.Center,
                Vector2.Zero,
                ModContent.ProjectileType<Content.Projectiles.InfernoOrbProjectile>(),
                0, 0f, player.whoAmI
            );

            if (orb >= 0 && orb < Main.maxProjectiles)
            {
                Main.projectile[orb].ai[0] = orbCount;
                Main.projectile[orb].localAI[0] = orbCount;
                orbProjectiles.Add(orb);
                orbCount++;

                
                for (int i = 0; i < 25; i++)
                {
                    Dust dust = Dust.NewDustPerfect(player.Center, 6,
                        Main.rand.NextVector2Circular(5f, 5f), 100, new Color(255, 140, 0), Main.rand.NextFloat(1.8f, 2.5f));
                    dust.noGravity = true;
                }

                SoundEngine.PlaySound(SoundID.Item73 with { Pitch = 0.4f }, player.Center);
            }
        }

        public void UseOrbAttack(Player player, EntitySource_ItemUse_WithAmmo source, int damage, float knockback)
        {
            if (orbCount == 0) return;

            switch (orbCount)
            {
                case 1:
                    Attack1Meteor(player, source, damage, knockback);
                    break;
                case 2:
                    Attack2FireStorm(player, source, damage, knockback);
                    break;
                case 3:
                    Attack3SolarFlare(player, source, damage, knockback);
                    break;
                case 4:
                    Attack4Supernova(player, source, damage, knockback);
                    break;
                default:
                    
                    Attack5Cataclysm(player, source, damage, knockback);
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

        public class InfernoHitTracker : GlobalNPC
        {
            public ulong lastInfernoHit = 0;
            public override bool InstancePerEntity => true;
        }

        public override void PostUpdate()
        {
            if (Player.HasBuff(BuffID.Inferno))
            {
                int damage = 12;
                float radius = 160f;
                int cooldownTicks = 30; 

                foreach (NPC npc in Main.npc)
                {
                    if (npc.active && !npc.friendly && !npc.dontTakeDamage && npc.Distance(Player.Center) < radius)
                    {
                        var tracker = npc.GetGlobalNPC<InfernoHitTracker>();
                        if (Main.GameUpdateCount - tracker.lastInfernoHit >= (ulong)cooldownTicks)
                        {
                            tracker.lastInfernoHit = Main.GameUpdateCount;

                            NPC.HitInfo hitInfo = new NPC.HitInfo()
                            {
                                Damage = damage,
                                Knockback = 0f,
                                HitDirection = 0,
                                Crit = false
                            };
                            npc.StrikeNPC(hitInfo);

                            npc.AddBuff(BuffID.OnFire, 60);
                        }
                    }
                }
            }
        }

        private void Attack1Meteor(Player player, EntitySource_ItemUse_WithAmmo source, int damage, float knockback)
        {
            Vector2 velocity = (Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitX) * 18f;

            for (int i = 0; i < 30; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(60, 60);
                Dust dust = Dust.NewDustPerfect(player.Center + offset, 6,
                    -offset * 0.15f, 100, Color.Orange, Main.rand.NextFloat(2.0f, 3.0f));
                dust.noGravity = true;
            }

            int proj = Projectile.NewProjectile(source, player.Center, velocity,
                ProjectileID.Meteor1, (int)(damage * 1.3f), knockback * 1.5f, player.whoAmI);

            if (proj >= 0 && proj < Main.maxProjectiles)
            {
                Main.projectile[proj].DamageType = DamageClass.Magic;
            }

            SoundEngine.PlaySound(SoundID.Item45, player.Center);
        }

        private void Attack2FireStorm(Player player, EntitySource_ItemUse_WithAmmo source, int damage, float knockback)
        {
            Vector2 baseVelocity = (Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitX) * 14f;

            
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 velocity = baseVelocity.RotatedBy(angle);

                int proj = Projectile.NewProjectile(source, player.Center, velocity,
                    ProjectileID.BallofFire, damage, knockback, player.whoAmI);

                if (proj >= 0 && proj < Main.maxProjectiles)
                {
                    Main.projectile[proj].DamageType = DamageClass.Magic;
                }
            }

            
            for (int i = 0; i < 50; i++)
            {
                float angle = MathHelper.TwoPi * i / 50f;
                Vector2 offset = angle.ToRotationVector2() * 70f;
                Dust dust = Dust.NewDustPerfect(player.Center + offset, 158,
                    offset.SafeNormalize(Vector2.UnitX) * 4f, 100, Color.OrangeRed, 2.5f);
                dust.noGravity = true;
            }

            SoundEngine.PlaySound(SoundID.Item73, player.Center);
        }

        private void Attack3SolarFlare(Player player, EntitySource_ItemUse_WithAmmo source, int damage, float knockback)
        {
            
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.ToRadians(i * 30);
                Vector2 velocity = new Vector2(16f, 0).RotatedBy(angle);

                int proj = Projectile.NewProjectile(source, player.Center, velocity,
                    ProjectileID.SolarFlareRay, (int)(damage * 0.9f), knockback * 0.9f, player.whoAmI);

                if (proj >= 0 && proj < Main.maxProjectiles)
                {
                    Main.projectile[proj].DamageType = DamageClass.Magic;
                    Main.projectile[proj].timeLeft = 180;
                }
            }

            
            for (int i = 0; i < 60; i++)
            {
                Vector2 velocity = Main.rand.NextVector2CircularEdge(12f, 12f);
                Dust dust = Dust.NewDustPerfect(player.Center, 6, velocity, 100, Color.Orange, Main.rand.NextFloat(2.5f, 4.0f));
                dust.noGravity = true;
            }

            for (int i = 0; i < 40; i++)
            {
                Dust dust = Dust.NewDustPerfect(player.Center, 174, Main.rand.NextVector2Circular(10f, 10f), 100, Color.Yellow, Main.rand.NextFloat(3.0f, 5.0f));
                dust.noGravity = true;
            }

            player.AddBuff(BuffID.Inferno, 600);
            SoundEngine.PlaySound(SoundID.Item74 with { Volume = 1.0f, Pitch = 0.2f }, player.Center);
        }

        private void Attack4Supernova(Player player, EntitySource_ItemUse_WithAmmo source, int damage, float knockback)
        {
            

            
            for (int i = 0; i < 16; i++)
            {
                float angle = MathHelper.TwoPi * i / 16f;
                Vector2 velocity = angle.ToRotationVector2() * 20f;

                int meteor = Projectile.NewProjectile(source, player.Center, velocity,
                    ProjectileID.Meteor1, (int)(damage * 1.1f), knockback * 1.2f, player.whoAmI);

                if (meteor >= 0 && meteor < Main.maxProjectiles)
                {
                    Main.projectile[meteor].DamageType = DamageClass.Magic;
                }
            }

            
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.ToRadians(i * 30 + 15);
                Vector2 velocity = angle.ToRotationVector2() * 18f;

                int ray = Projectile.NewProjectile(source, player.Center, velocity,
                    ProjectileID.SolarFlareRay, (int)(damage * 0.95f), knockback, player.whoAmI);

                if (ray >= 0 && ray < Main.maxProjectiles)
                {
                    Main.projectile[ray].DamageType = DamageClass.Magic;
                    Main.projectile[ray].timeLeft = 180;
                }
            }

            
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 velocity = angle.ToRotationVector2() * 15f;

                int fire = Projectile.NewProjectile(source, player.Center, velocity,
                    ProjectileID.BallofFire, damage, knockback, player.whoAmI);

                if (fire >= 0 && fire < Main.maxProjectiles)
                {
                    Main.projectile[fire].DamageType = DamageClass.Magic;
                }
            }

            
            for (int i = 0; i < 100; i++)
            {
                Vector2 velocity = Main.rand.NextVector2CircularEdge(15f, 15f);
                Dust dust = Dust.NewDustPerfect(player.Center, 6, velocity, 100, Color.Orange, Main.rand.NextFloat(3.0f, 5.5f));
                dust.noGravity = true;
            }

            for (int i = 0; i < 80; i++)
            {
                Dust dust = Dust.NewDustPerfect(player.Center, 174, Main.rand.NextVector2Circular(12f, 12f), 100, Color.Yellow, Main.rand.NextFloat(3.5f, 6.0f));
                dust.noGravity = true;
            }

            for (int i = 0; i < 60; i++)
            {
                Dust dust = Dust.NewDustPerfect(player.Center, 158, Main.rand.NextVector2Circular(14f, 14f), 100, Color.Red, Main.rand.NextFloat(4.0f, 7.0f));
                dust.noGravity = true;
            }

            
            CombatText.NewText(player.Hitbox, new Color(255, 100, 0), "SUPERNOVA!", true, true);

            SoundEngine.PlaySound(SoundID.Item74 with { Volume = 1.2f, Pitch = -0.4f }, player.Center);
            SoundEngine.PlaySound(SoundID.Item62 with { Volume = 1.0f, Pitch = -0.3f }, player.Center);

            player.AddBuff(BuffID.Inferno, 600);
        }
    }
}

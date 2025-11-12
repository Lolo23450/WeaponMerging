using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponMerging.Content.Players
{
    public class CrystalCascadePlayer : ModPlayer
    {
        public int crystalCount = 0;
        private const int MAX_CRYSTALS = 6;
        private List<int> crystalProjectiles = new List<int>();

        public override void ResetEffects()
        {
            
            crystalProjectiles.RemoveAll(id => id < 0 || id >= Main.maxProjectiles || !Main.projectile[id].active);
            crystalCount = crystalProjectiles.Count;
        }

        public void GainCrystal(Player player)
        {
            if (crystalCount >= MAX_CRYSTALS) return;

            
            int crystal = Projectile.NewProjectile(
                player.GetSource_FromThis(),
                player.Center,
                Vector2.Zero,
                ModContent.ProjectileType<Projectiles.CrystalOrbProjectile>(),
                0, 0f, player.whoAmI
            );

            if (crystal >= 0 && crystal < Main.maxProjectiles)
            {
                crystalProjectiles.Add(crystal);
                crystalCount++;

                
                ReassignCrystalIndices();

                
                for (int i = 0; i < 20; i++)
                {
                    Dust dust = Dust.NewDustPerfect(player.Center, 68,
                        Main.rand.NextVector2Circular(3f, 3f), 100, Color.Cyan, Main.rand.NextFloat(1.3f, 2.0f));
                    dust.noGravity = true;
                }

                SoundEngine.PlaySound(SoundID.Item29 with { Pitch = 0.5f }, player.Center);
            }
        }

        
        public void ReassignCrystalIndices()
        {
            for (int i = 0; i < crystalProjectiles.Count; i++)
            {
                int id = crystalProjectiles[i];
                if (id >= 0 && id < Main.maxProjectiles && Main.projectile[id].active)
                {
                    Main.projectile[id].localAI[0] = i;
                    Main.projectile[id].localAI[1] = 0f; 
                }
                else
                {
                    
                    crystalProjectiles.RemoveAt(i);
                    i--;
                }
            }

            crystalCount = crystalProjectiles.Count;
        }

        public void UseCrystalAttack(Player player, EntitySource_ItemUse_WithAmmo source, int damage, float knockback)
        {
            if (crystalCount == 0) return;

            Vector2 targetVelocity = (Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitX) * 16f;

            
            int largestCrystalSize = GetLargestCrystalSize();

            if (largestCrystalSize >= 2)
            {
                
                AttackMegaCrystalLance(player, source, damage, knockback, targetVelocity, largestCrystalSize);
            }
            else
            {
                
                switch (crystalCount)
                {
                    case 1:
                        Attack1FrostBolt(player, source, damage, knockback, targetVelocity);
                        break;
                    case 2:
                        Attack2PrecisionBeam(player, source, damage, knockback, targetVelocity);
                        break;
                    case 3:
                        Attack3IceSpear(player, source, damage, knockback, targetVelocity);
                        break;
                    case 4:
                        Attack4CrystalBarrage(player, source, damage, knockback, targetVelocity);
                        break;
                    case 5:
                        Attack5GlacialCannon(player, source, damage, knockback, targetVelocity);
                        break;
                    case 6:
                        Attack6DiamondStorm(player, source, damage, knockback, targetVelocity);
                        break;
                }
            }

            
            foreach (int crystalID in crystalProjectiles)
            {
                if (crystalID >= 0 && crystalID < Main.maxProjectiles && Main.projectile[crystalID].active)
                {
                    Main.projectile[crystalID].Kill();
                }
            }
            crystalProjectiles.Clear();
            crystalCount = 0;
        }

        private int GetLargestCrystalSize()
        {
            int largest = 0;
            foreach (int crystalID in crystalProjectiles)
            {
                if (crystalID >= 0 && crystalID < Main.maxProjectiles && Main.projectile[crystalID].active)
                {
                    var crystalProj = Main.projectile[crystalID].ModProjectile as Projectiles.CrystalOrbProjectile;
                    if (crystalProj != null && crystalProj.absorbedCount > largest)
                    {
                        largest = crystalProj.absorbedCount;
                    }
                }
            }
            return largest;
        }

        private void Attack1FrostBolt(Player player, EntitySource_ItemUse_WithAmmo source, int damage, float knockback, Vector2 velocity)
        {
            
            
            
            for (int i = 0; i < 16; i++)
            {
                float angle = MathHelper.ToRadians(i * 22.5f);
                Vector2 offset = new Vector2(35, 0).RotatedBy(angle);
                Dust dust = Dust.NewDustPerfect(player.Center + offset, 68,
                    -offset * 0.12f, 100, Color.LightCyan, 2.0f);
                dust.noGravity = true;
                dust.fadeIn = 1.3f;
            }

            
            int proj = Projectile.NewProjectile(source, player.Center, velocity,
                ProjectileID.FrostBoltStaff, (int)(damage * 1.5f), knockback * 1.3f, player.whoAmI);

            if (proj >= 0 && proj < Main.maxProjectiles)
            {
                Main.projectile[proj].scale = 1.4f;
                Main.projectile[proj].timeLeft = 200;
                Main.projectile[proj].penetrate = 2;
            }

            
            for (int i = 0; i < 15; i++)
            {
                Dust ice = Dust.NewDustPerfect(player.Center, 76,
                    velocity * Main.rand.NextFloat(0.3f, 0.7f), 100, Color.Cyan, Main.rand.NextFloat(1.5f, 2.2f));
                ice.noGravity = true;
            }

            SoundEngine.PlaySound(SoundID.Item28 with { Pitch = -0.2f, Volume = 0.9f }, player.Center);
        }

        private void Attack2PrecisionBeam(Player player, EntitySource_ItemUse_WithAmmo source, int damage, float knockback, Vector2 velocity)
        {
            
            
            Vector2 targetPos = Main.MouseWorld;
            Vector2 shootDir = velocity.SafeNormalize(Vector2.UnitX);

            
            float laserLength = Vector2.Distance(player.Center, targetPos);
            int sightSegments = (int)(laserLength / 10f);
            
            for (int i = 0; i < sightSegments; i++)
            {
                float progress = i / (float)sightSegments;
                Vector2 sightPos = player.Center + shootDir * (progress * laserLength);
                
                Dust sight = Dust.NewDustPerfect(sightPos, 68,
                    Vector2.Zero, 100, Color.Lerp(Color.Red, Color.Cyan, progress), 1.2f);
                sight.noGravity = true;
                sight.fadeIn = 0.8f;
            }

            
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.ToRadians(i * 45f);
                Vector2 reticleOffset = new Vector2(20f, 0).RotatedBy(angle);
                
                Dust reticle = Dust.NewDustPerfect(targetPos + reticleOffset, 68,
                    reticleOffset * 0.1f, 100, Color.Red, 1.8f);
                reticle.noGravity = true;
            }

            
            for (int i = 0; i < 20; i++)
            {
                float angle = MathHelper.ToRadians(i * 18f);
                Vector2 chargeOffset = new Vector2(30f, 0).RotatedBy(angle);
                
                Dust charge = Dust.NewDustPerfect(player.Center + chargeOffset, 68,
                    -chargeOffset * 0.15f, 100, Color.Cyan, 2.0f);
                charge.noGravity = true;
                charge.fadeIn = 1.2f;
            }

            
            Vector2 fastVel = velocity * 1.8f; 
            
            int proj = Projectile.NewProjectile(source, player.Center, fastVel,
                ProjectileID.IceSickle, (int)(damage * 1.8f), knockback * 1.5f, player.whoAmI);

            if (proj >= 0 && proj < Main.maxProjectiles)
            {
                Main.projectile[proj].scale = 1.1f;
                Main.projectile[proj].timeLeft = 150;
                Main.projectile[proj].penetrate = 3;
            }

            
            for (int i = 0; i < 30; i++)
            {
                float progress = i / 30f;
                Vector2 beamPos = player.Center + shootDir * (progress * 150f);
                
                Dust beam = Dust.NewDustPerfect(beamPos, 68,
                    shootDir * 10f, 100, Color.White, 1.3f);
                beam.noGravity = true;
                beam.fadeIn = 1.0f;
            }

            
            for (int i = 0; i < 15; i++)
            {
                Vector2 flashVel = shootDir.RotatedByRandom(0.2f) * Main.rand.NextFloat(4f, 9f);
                Dust flash = Dust.NewDustPerfect(player.Center + shootDir * 20f, 15,
                    flashVel, 140, Color.White, Main.rand.NextFloat(1.8f, 2.5f));
                flash.noGravity = true;
            }

            SoundEngine.PlaySound(SoundID.Item12 with { Pitch = 0.9f, Volume = 0.7f }, player.Center);
            SoundEngine.PlaySound(SoundID.Item91 with { Pitch = 0.7f, Volume = 1.0f }, player.Center);
        }

        private void Attack3IceSpear(Player player, EntitySource_ItemUse_WithAmmo source, int damage, float knockback, Vector2 velocity)
        {
            
            Vector2 slowVel = velocity * 0.15f; 

            
            int proj = Projectile.NewProjectile(source, player.Center, slowVel,
                ProjectileID.FrostBlastFriendly, (int)(damage * 2.5f), knockback * 2.5f, player.whoAmI);

            if (proj >= 0 && proj < Main.maxProjectiles)
            {
                Main.projectile[proj].scale = 6f; 
                Main.projectile[proj].timeLeft = 280;
                Main.projectile[proj].penetrate = 4;
            }

            
            for (int ring = 0; ring < 4; ring++)
            {
                float ringRadius = ring * 15f;
                int dustCount = 8 + ring * 3;
                
                for (int i = 0; i < dustCount; i++)
                {
                    float angle = MathHelper.ToRadians(i * (360f / dustCount));
                    Vector2 offset = new Vector2(ringRadius, 0).RotatedBy(angle);
                    
                    Dust shockwave = Dust.NewDustPerfect(player.Center + offset, 68,
                        offset * 0.4f, 100, Color.RoyalBlue, 3.0f - ring * 0.4f);
                    shockwave.noGravity = true;
                    shockwave.fadeIn = 1.5f;
                }
            }

            
            for (int i = 0; i < 25; i++)
            {
                Vector2 chunkVel = velocity.RotatedByRandom(0.3f) * Main.rand.NextFloat(0.4f, 0.8f);
                Dust chunk = Dust.NewDustPerfect(player.Center, 76,
                    chunkVel, 120, Color.Cyan, Main.rand.NextFloat(2.5f, 3.5f));
                chunk.noGravity = true;
            }

            SoundEngine.PlaySound(SoundID.Item28 with { Volume = 1.2f, Pitch = -0.6f }, player.Center);
            SoundEngine.PlaySound(SoundID.DD2_MonkStaffGroundImpact with { Volume = 0.8f, Pitch = 0.0f }, player.Center);
        }

        private void Attack4CrystalBarrage(Player player, EntitySource_ItemUse_WithAmmo source, int damage, float knockback, Vector2 velocity)
        {
            
            
            int shotCount = 10;
            float spreadAngle = MathHelper.ToRadians(7f);

            
            for (int i = 0; i < 25; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 sparkPos = player.Center + new Vector2(Main.rand.NextFloat(20f, 40f), 0).RotatedBy(angle);
                
                Dust spark = Dust.NewDustPerfect(sparkPos, 15,
                    (player.Center - sparkPos) * 0.3f, 130, Color.LightCyan, 1.5f);
                spark.noGravity = true;
            }

            
            for (int i = 0; i < shotCount; i++)
            {
                float angle = velocity.ToRotation() + Main.rand.NextFloat(-spreadAngle, spreadAngle);
                Vector2 shotVel = new Vector2(20f, 0).RotatedBy(angle); 
                
                int proj = Projectile.NewProjectile(source, player.Center, shotVel,
                    ProjectileID.FrostDaggerfish, (int)(damage * 0.7f), knockback * 0.5f, player.whoAmI);

                if (proj >= 0 && proj < Main.maxProjectiles)
                {
                    Main.projectile[proj].scale = 0.9f; 
                    Main.projectile[proj].timeLeft = 120;
                }
            }

            
            for (int i = 0; i < 40; i++)
            {
                Vector2 shootVel = velocity.RotatedByRandom(0.4f) * Main.rand.NextFloat(0.5f, 1.2f);
                Dust muzzle = Dust.NewDustPerfect(player.Center, 68,
                    shootVel * 8f, 110, Color.Cyan, Main.rand.NextFloat(1.2f, 1.8f));
                muzzle.noGravity = true;
            }

            
            for (int i = 0; i < 30; i++)
            {
                Dust fragment = Dust.NewDustPerfect(player.Center, 76,
                    velocity.RotatedByRandom(0.3f) * Main.rand.NextFloat(3f, 8f), 120, 
                    Color.LightCyan, Main.rand.NextFloat(1.5f, 2.2f));
                fragment.noGravity = true;
            }

            SoundEngine.PlaySound(SoundID.Item30 with { Volume = 1.0f, Pitch = 0.5f }, player.Center);
            SoundEngine.PlaySound(SoundID.Item11 with { Volume = 0.7f, Pitch = 0.8f }, player.Center);
        }

        private void Attack5GlacialCannon(Player player, EntitySource_ItemUse_WithAmmo source, int damage, float knockback, Vector2 velocity)
        {
            
            
            Vector2 cannonVel = velocity * 0.25f; 

            
            for (int i = 0; i < 50; i++)
            {
                float angle = MathHelper.ToRadians(i * 7.2f);
                float radius = 50f + (float)System.Math.Sin(angle * 3) * 20f;
                Vector2 offset = new Vector2(radius, 0).RotatedBy(angle);
                
                Dust charge = Dust.NewDustPerfect(player.Center + offset, 68,
                    -offset * 0.12f, 100, Color.DeepSkyBlue, 3.0f);
                charge.noGravity = true;
                charge.fadeIn = 1.6f;
            }

            
            for (int i = 0; i < 30; i++)
            {
                Vector2 wellPos = player.Center + Main.rand.NextVector2Circular(80f, 80f);
                Dust well = Dust.NewDustPerfect(wellPos, 76,
                    (player.Center - wellPos) * 0.1f, 120, Color.RoyalBlue, Main.rand.NextFloat(2.5f, 3.5f));
                well.noGravity = true;
            }

            
            int proj = Projectile.NewProjectile(source, player.Center, cannonVel,
                ProjectileID.FrostBlastFriendly, (int)(damage * 3.5f), knockback * 3.5f, player.whoAmI);

            if (proj >= 0 && proj < Main.maxProjectiles)
            {
                Main.projectile[proj].scale = 6.0f;
                Main.projectile[proj].timeLeft = 320;
                Main.projectile[proj].penetrate = 6;
            }

            
            for (int ring = 0; ring < 6; ring++)
            {
                float ringRadius = ring * 20f;
                int dustCount = 10 + ring * 4;
                
                for (int i = 0; i < dustCount; i++)
                {
                    float angle = MathHelper.ToRadians(i * (360f / dustCount));
                    Vector2 offset = new Vector2(ringRadius, 0).RotatedBy(angle);
                    
                    Dust explosion = Dust.NewDustPerfect(player.Center + offset, 68,
                        offset.SafeNormalize(Vector2.UnitX) * (7f - ring * 0.6f), 100,
                        Color.Lerp(Color.DeepSkyBlue, Color.White, ring / 6f), 3.5f - ring * 0.3f);
                    explosion.noGravity = true;
                    explosion.fadeIn = 1.7f;
                }
            }

            
            for (int i = 0; i < 40; i++)
            {
                Vector2 boulderVel = velocity.RotatedByRandom(0.5f) * Main.rand.NextFloat(0.3f, 0.7f);
                Dust boulder = Dust.NewDustPerfect(player.Center, 76,
                    boulderVel, 130, Color.Cyan, Main.rand.NextFloat(3.0f, 4.5f));
                boulder.noGravity = true;
            }

            
            for (int i = 0; i < 50; i++)
            {
                Dust shock = Dust.NewDustPerfect(player.Center, 15,
                    Main.rand.NextVector2CircularEdge(10f, 10f), 150, Color.White, Main.rand.NextFloat(3.0f, 4.5f));
                shock.noGravity = true;
            }

            SoundEngine.PlaySound(SoundID.Item28 with { Volume = 1.4f, Pitch = -0.8f }, player.Center);
            SoundEngine.PlaySound(SoundID.DD2_ExplosiveTrapExplode with { Volume = 1.0f, Pitch = -0.2f }, player.Center);
            SoundEngine.PlaySound(SoundID.DD2_BetsyWindAttack with { Volume = 0.9f }, player.Center);
        }

        private void Attack6DiamondStorm(Player player, EntitySource_ItemUse_WithAmmo source, int damage, float knockback, Vector2 velocity)
        {
            
            
            Vector2 aimDirection = velocity.SafeNormalize(Vector2.UnitX);
            float coneAngle = MathHelper.ToRadians(10f);

            
            for (int i = 0; i < 4; i++)
            {
                float spreadProgress = (i / 3f) - 0.5f;
                float angle = aimDirection.ToRotation() + coneAngle * spreadProgress * 0.8f;
                Vector2 heavyVel = new Vector2(10f, 0).RotatedBy(angle);
                
                int proj = Projectile.NewProjectile(source, player.Center, heavyVel,
                    ProjectileID.FrostBlastFriendly, (int)(damage * 1.3f), knockback * 1.5f, player.whoAmI);

                if (proj >= 0 && proj < Main.maxProjectiles)
                {
                    Main.projectile[proj].scale = 2.0f;
                    Main.projectile[proj].timeLeft = 200;
                    Main.projectile[proj].penetrate = 3;
                }
            }

            
            for (int i = 0; i < 8; i++)
            {
                float spreadProgress = (i / 7f) - 0.5f;
                float angle = aimDirection.ToRotation() + coneAngle * spreadProgress;
                Vector2 fastVel = new Vector2(22f, 0).RotatedBy(angle);
                
                int proj = Projectile.NewProjectile(source, player.Center, fastVel,
                    ProjectileID.FrostDaggerfish, (int)(damage * 0.8f), knockback * 0.8f, player.whoAmI);

                if (proj >= 0 && proj < Main.maxProjectiles)
                {
                    Main.projectile[proj].scale = 1.1f;
                    Main.projectile[proj].timeLeft = 150;
                }
            }

            
            for (int i = 0; i < 60; i++)
            {
                float angle = MathHelper.ToRadians(i * 6f);
                float radius = 50f + (float)System.Math.Sin(angle * 4) * 15f;
                Vector2 offset = new Vector2(radius, 0).RotatedBy(angle);
                
                Vector2 targetDir = aimDirection * radius * 0.3f;
                
                Dust charge = Dust.NewDustPerfect(player.Center + offset, 68,
                    (targetDir - offset) * 0.2f, 100, Color.DeepSkyBlue, 2.5f);
                charge.noGravity = true;
                charge.fadeIn = 1.5f;
            }

            
            for (int wave = 0; wave < 6; wave++)
            {
                float waveDistance = wave * 30f;
                int particlesInWave = 8 + wave * 2;
                
                for (int i = 0; i < particlesInWave; i++)
                {
                    float spreadProgress = (i / (float)particlesInWave) - 0.5f;
                    float angle = aimDirection.ToRotation() + coneAngle * spreadProgress;
                    Vector2 wavePos = player.Center + new Vector2(waveDistance, 0).RotatedBy(angle);
                    
                    Dust blast = Dust.NewDustPerfect(wavePos, 68,
                        new Vector2(9f - wave * 0.7f, 0).RotatedBy(angle), 100,
                        Color.Lerp(Color.Cyan, Color.White, wave / 6f), 3.0f - wave * 0.3f);
                    blast.noGravity = true;
                    blast.fadeIn = 1.6f;
                }
            }

            
            Vector2 diamondCenter = player.Center + aimDirection * 80f;
            
            Vector2[] diamondPoints = new Vector2[4]
            {
                diamondCenter + aimDirection * 60f,
                diamondCenter + aimDirection.RotatedBy(MathHelper.PiOver2) * 40f,
                diamondCenter - aimDirection * 40f,
                diamondCenter - aimDirection.RotatedBy(MathHelper.PiOver2) * 40f
            };

            for (int point = 0; point < 4; point++)
            {
                Vector2 start = diamondPoints[point];
                Vector2 end = diamondPoints[(point + 1) % 4];
                
                float lineLength = Vector2.Distance(start, end);
                int segments = (int)(lineLength / 5f);
                
                for (int i = 0; i < segments; i++)
                {
                    float progress = i / (float)segments;
                    Vector2 linePos = Vector2.Lerp(start, end, progress);
                    
                    Dust diamond = Dust.NewDustPerfect(linePos, 15,
                        aimDirection * 5f, 140, Color.White, 2.8f);
                    diamond.noGravity = true;
                    diamond.fadeIn = 1.5f;
                }
            }

            
            for (int i = 0; i < 70; i++)
            {
                float spreadProgress = Main.rand.NextFloat(-0.5f, 0.5f);
                float angle = aimDirection.ToRotation() + coneAngle * spreadProgress;
                float speed = Main.rand.NextFloat(8f, 18f);
                Vector2 vel = new Vector2(speed, 0).RotatedBy(angle);
                
                Dust shard = Dust.NewDustPerfect(player.Center, 76,
                    vel, 130, Color.LightCyan, Main.rand.NextFloat(2.2f, 3.5f));
                shard.noGravity = true;
            }

            
            for (int i = 0; i < 45; i++)
            {
                Vector2 flashVel = aimDirection.RotatedByRandom(0.5f) * Main.rand.NextFloat(5f, 12f);
                Dust flash = Dust.NewDustPerfect(player.Center + aimDirection * 30f, 15,
                    flashVel, 150, Color.White, Main.rand.NextFloat(2.5f, 4.0f));
                flash.noGravity = true;
            }

            
            for (int i = 0; i < 30; i++)
            {
                Vector2 recoilVel = (-aimDirection).RotatedByRandom(0.4f) * Main.rand.NextFloat(4f, 10f);
                Dust recoil = Dust.NewDustPerfect(player.Center - aimDirection * 20f, 68,
                    recoilVel, 110, Color.Cyan, Main.rand.NextFloat(2.0f, 3.0f));
                recoil.noGravity = true;
            }

            SoundEngine.PlaySound(SoundID.Item28 with { Volume = 1.4f, Pitch = -0.3f }, player.Center);
            SoundEngine.PlaySound(SoundID.Item62 with { Volume = 1.2f, Pitch = 0.5f }, player.Center);
            SoundEngine.PlaySound(SoundID.DD2_BetsyWindAttack with { Volume = 1.0f }, diamondCenter);
            SoundEngine.PlaySound(SoundID.Item11 with { Volume = 0.8f, Pitch = 0.6f }, player.Center);
        }

        private void AttackMegaCrystalLance(Player player, EntitySource_ItemUse_WithAmmo source, int damage, float knockback, Vector2 velocity, int absorptions)
        {
            
            
            float damageMultiplier = 1.5f + absorptions * 0.4f;
            float scaleMultiplier = 1.8f + absorptions * 0.3f;

            
            for (int i = 0; i < 50; i++)
            {
                float angle = MathHelper.ToRadians(i * 7.2f);
                float radius = 60f + (float)System.Math.Sin(angle * 2) * 20f;
                Vector2 offset = new Vector2(radius, 0).RotatedBy(angle);
                
                Dust dust = Dust.NewDustPerfect(player.Center + offset, 68,
                    -offset * 0.15f, 100, Color.DeepSkyBlue, 2.5f);
                dust.noGravity = true;
                dust.fadeIn = 1.5f;
            }

            
            int proj = Projectile.NewProjectile(source, player.Center, velocity * 1.2f,
                ProjectileID.FrostBoltStaff, (int)(damage * damageMultiplier), knockback * 1.8f, player.whoAmI);

            if (proj >= 0 && proj < Main.maxProjectiles)
            {
                Main.projectile[proj].scale = scaleMultiplier;
                Main.projectile[proj].timeLeft = 250;
                Main.projectile[proj].penetrate = 5 + absorptions * 2;
            }

            
            for (int ring = 0; ring < 5; ring++)
            {
                float ringRadius = ring * 15f;
                int dustCount = 8 + ring * 4;
                
                for (int i = 0; i < dustCount; i++)
                {
                    float angle = MathHelper.ToRadians(i * (360f / dustCount));
                    Vector2 offset = new Vector2(ringRadius, 0).RotatedBy(angle);
                    
                    Dust explosion = Dust.NewDustPerfect(player.Center + offset, 68,
                        offset.SafeNormalize(Vector2.UnitX) * (5f - ring * 0.5f), 100, 
                        Color.RoyalBlue, 3.0f - ring * 0.3f);
                    explosion.noGravity = true;
                    explosion.fadeIn = 1.6f;
                }
            }

            
            for (int i = 0; i < 30; i++)
            {
                Dust crystal = Dust.NewDustPerfect(player.Center, 76,
                    velocity * Main.rand.NextFloat(0.4f, 0.9f), 120, Color.Cyan, Main.rand.NextFloat(2.0f, 3.0f));
                crystal.noGravity = true;
            }

            
            for (int i = 0; i < 60; i++)
            {
                Dust shockwave = Dust.NewDustPerfect(player.Center, 15,
                    Main.rand.NextVector2CircularEdge(8f, 8f), 150, Color.White, Main.rand.NextFloat(2.5f, 3.5f));
                shockwave.noGravity = true;
            }

            SoundEngine.PlaySound(SoundID.Item28 with { Volume = 1.2f, Pitch = -0.5f }, player.Center);
            SoundEngine.PlaySound(SoundID.DD2_BetsyWindAttack with { Volume = 0.7f }, player.Center);
        }
    }
}

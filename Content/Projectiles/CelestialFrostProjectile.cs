using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponMerging.Content.Projectiles
{
    public class CelestialFrostProjectile : GlobalProjectile
    {
        public override bool InstancePerEntity => true;
        
        private bool hasSpawnedImpactShards = false;
        private int trailCounter = 0;
        private bool isCelestialFrostProjectile = false;

        public override void AI(Projectile projectile)
        {
            
            if (projectile.owner >= 0 && projectile.owner < Main.maxPlayers)
            {
                Player player = Main.player[projectile.owner];
                if (player.HeldItem.type == ModContent.ItemType<Items.Weapons.CelestialFrostBlade>())
                {
                    isCelestialFrostProjectile = true;
                }
            }

            
            if (projectile.type == ProjectileID.StarCannonStar && isCelestialFrostProjectile)
            {
                
                projectile.velocity.Y += 0.15f; 
                if (projectile.velocity.Y > 16f) projectile.velocity.Y = 16f; 
                
                trailCounter++;
                
                
                if (trailCounter % 2 == 0)
                {
                    Dust glow = Dust.NewDustPerfect(projectile.Center, DustID.IceTorch, 
                        Vector2.Zero, 100, Color.LightCyan, Main.rand.NextFloat(1.5f, 2.0f));
                    glow.noGravity = true;
                    glow.fadeIn = 1.3f;
                    glow.alpha = 50;
                }
                
                
                if (trailCounter % 3 == 0)
                {
                    Dust trail = Dust.NewDustPerfect(projectile.Center, DustID.IceRod, 
                        -projectile.velocity * 0.2f, 100, Color.Cyan, 1.2f);
                    trail.noGravity = true;
                    trail.fadeIn = 0.9f;
                }
                
                
                if (Main.rand.NextBool(5))
                {
                    Dust sparkle = Dust.NewDustPerfect(projectile.Center + Main.rand.NextVector2Circular(10, 10), 
                        DustID.BlueFairy, Vector2.Zero, 150, Color.White, Main.rand.NextFloat(1.0f, 1.4f));
                    sparkle.noGravity = true;
                    sparkle.fadeIn = 1.1f;
                }
                
                
                if (trailCounter % 4 == 0)
                {
                    float angle = Main.rand.NextFloat(0, MathHelper.TwoPi);
                    Vector2 offset = new Vector2(15, 0).RotatedBy(angle);
                    Dust ring = Dust.NewDustPerfect(projectile.Center + offset, DustID.IceTorch, 
                        Vector2.Zero, 120, Color.LightBlue, 1.0f);
                    ring.noGravity = true;
                    ring.alpha = 100;
                    ring.velocity = -offset * 0.1f;
                }
            }
            
            
            if (projectile.type == ProjectileID.IceBolt && isCelestialFrostProjectile)
            {
                
                projectile.velocity.Y += 0.08f;
                if (projectile.velocity.Y > 10f) projectile.velocity.Y = 10f;
                
                
                if (projectile.timeLeft < 120) 
                {
                    float falloffFactor = projectile.timeLeft / 120f;
                    if (falloffFactor < 0.75f)
                    {
                        projectile.damage = (int)(projectile.damage * MathHelper.Max(0.75f, falloffFactor));
                    }
                }
                
                
                if (Main.rand.NextBool(2))
                {
                    Dust frostGlow = Dust.NewDustPerfect(projectile.Center, DustID.IceTorch, 
                        -projectile.velocity * 0.15f, 120, Color.Cyan, Main.rand.NextFloat(0.8f, 1.2f));
                    frostGlow.noGravity = true;
                    frostGlow.alpha = 80;
                    frostGlow.fadeIn = 0.8f;
                }
            }
        }

        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (projectile.type == ProjectileID.StarCannonStar && !hasSpawnedImpactShards && isCelestialFrostProjectile)
            {
                hasSpawnedImpactShards = true;
                
                
                for (int i = 0; i < 8; i++)
                {
                    float rotation = MathHelper.ToRadians(i * 45);
                    Vector2 velocity = new Vector2(7f, 0).RotatedBy(rotation);
                    
                    velocity.Y -= 2f;
                    
                    int proj = Projectile.NewProjectile(projectile.GetSource_FromThis(), projectile.Center, velocity,
                        ProjectileID.IceBolt, (int)(damageDone * 0.5f), 2f, projectile.owner);
                    
                    if (proj >= 0 && proj < Main.maxProjectiles && Main.projectile[proj].active)
                    {
                        Main.projectile[proj].CritChance = projectile.CritChance;
                        Main.projectile[proj].scale = 1.1f;
                    }
                }
                
                
                for (int i = 0; i < 20; i++)
                {
                    float angle = MathHelper.ToRadians(i * 18);
                    Vector2 speed = new Vector2(5f, 0).RotatedBy(angle);
                    Dust dust = Dust.NewDustPerfect(projectile.Center, DustID.IceTorch, speed, 100, 
                        Color.LightCyan, Main.rand.NextFloat(1.5f, 2.2f));
                    dust.noGravity = true;
                    dust.fadeIn = 1.2f;
                    dust.alpha = 60;
                }
                
                
                for (int i = 0; i < 12; i++)
                {
                    Vector2 speed = Main.rand.NextVector2CircularEdge(4f, 4f);
                    Dust ice = Dust.NewDustPerfect(projectile.Center, DustID.IceRod, speed, 
                        100, Color.Cyan, Main.rand.NextFloat(1.2f, 1.8f));
                    ice.noGravity = true;
                }
                
                
                for (int i = 0; i < 6; i++)
                {
                    Dust star = Dust.NewDustPerfect(projectile.Center, DustID.BlueFairy, 
                        Main.rand.NextVector2Circular(2f, 2f), 200, Color.White, 1.5f);
                    star.noGravity = true;
                    star.fadeIn = 1.3f;
                }
                
                target.AddBuff(BuffID.Frostburn, 240);
            }
        }
    }
}

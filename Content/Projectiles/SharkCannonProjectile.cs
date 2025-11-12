using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponMerging.Content.Projectiles
{
    public class SharkCannonProjectile : GlobalProjectile
    {
        public override bool InstancePerEntity => true;
        
        private int trailCounter = 0;
        
        private bool IsSharkCannonBullet(Projectile projectile)
        {
            
            if (projectile.owner >= 0 && projectile.owner < Main.maxPlayers)
            {
                Player player = Main.player[projectile.owner];
                return player.HeldItem.type == ModContent.ItemType<Items.Weapons.SharkCannon>();
            }
            return false;
        }

        public override void AI(Projectile projectile)
        {
            if (IsSharkCannonBullet(projectile) && projectile.aiStyle == 1)
            {
                trailCounter++;
                
                
                if (trailCounter % 2 == 0)
                {
                    Dust trail = Dust.NewDustPerfect(projectile.Center, DustID.IceTorch, 
                        -projectile.velocity * 0.15f, 100, Color.Cyan, Main.rand.NextFloat(0.8f, 1.3f));
                    trail.noGravity = true;
                    trail.fadeIn = 1.0f;
                }
                
                
                if (Main.rand.NextBool(3))
                {
                    Dust smoke = Dust.NewDustPerfect(projectile.Center + Main.rand.NextVector2Circular(4, 4), 
                        DustID.Smoke, -projectile.velocity * 0.1f, 120, Color.LightGray, Main.rand.NextFloat(0.6f, 1.0f));
                    smoke.noGravity = false;
                }
                
                
                if (Main.rand.NextBool(5))
                {
                    Dust sparkle = Dust.NewDustPerfect(projectile.Center, DustID.Torch, 
                        Main.rand.NextVector2Circular(1, 1), 0, Color.Yellow, Main.rand.NextFloat(0.5f, 0.8f));
                    sparkle.noGravity = true;
                }
                
                
                if (Main.rand.NextBool(4))
                {
                    Dust water = Dust.NewDustPerfect(projectile.Center, DustID.WaterCandle, 
                        -projectile.velocity * 0.2f, 150, Color.LightBlue, Main.rand.NextFloat(0.7f, 1.1f));
                    water.noGravity = true;
                    water.fadeIn = 0.9f;
                }
            }
        }

        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (IsSharkCannonBullet(projectile) && projectile.aiStyle == 1)
            {
                
                for (int i = 0; i < 20; i++)
                {
                    Vector2 speed = Main.rand.NextVector2CircularEdge(6f, 6f);
                    Dust explosion = Dust.NewDustPerfect(projectile.Center, DustID.Smoke, speed, 100, 
                        Color.DarkGray, Main.rand.NextFloat(1.2f, 2.0f));
                    explosion.noGravity = false;
                }
                
                
                for (int i = 0; i < 15; i++)
                {
                    float angle = MathHelper.ToRadians(i * 24);
                    Vector2 energyVel = new Vector2(Main.rand.NextFloat(4f, 7f), 0).RotatedBy(angle);
                    Dust energy = Dust.NewDustPerfect(projectile.Center, DustID.IceTorch, energyVel, 100, 
                        Color.Cyan, Main.rand.NextFloat(1.3f, 2.0f));
                    energy.noGravity = true;
                    energy.fadeIn = 1.3f;
                }
                
                
                for (int i = 0; i < 12; i++)
                {
                    Vector2 sparkVel = Main.rand.NextVector2CircularEdge(5f, 5f);
                    Dust spark = Dust.NewDustPerfect(projectile.Center, DustID.Torch, sparkVel, 0, 
                        Color.Orange, Main.rand.NextFloat(1.0f, 1.6f));
                    spark.noGravity = true;
                }
                
                
                for (int i = 0; i < 10; i++)
                {
                    Dust water = Dust.NewDustPerfect(projectile.Center + Main.rand.NextVector2Circular(15, 15), 
                        DustID.WaterCandle, Main.rand.NextVector2Circular(4, 4), 150, 
                        Color.LightBlue, Main.rand.NextFloat(1.2f, 1.8f));
                    water.noGravity = true;
                }
                
                
                for (int i = 0; i < 8; i++)
                {
                    float angle = MathHelper.ToRadians(i * 45);
                    Vector2 ringVel = new Vector2(5f, 0).RotatedBy(angle);
                    Dust ring = Dust.NewDustPerfect(projectile.Center, DustID.Electric, ringVel, 120, 
                        Color.White, 1.4f);
                    ring.noGravity = true;
                }
            }
        }
    }
}


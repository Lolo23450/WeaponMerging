using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Terraria.Audio;

namespace WeaponMerging.Content.Projectiles
{
    public class VenomBarrageProjectile : GlobalProjectile
    {
        public override bool InstancePerEntity => true;
        
        private int trailCounter = 0;

        public override void AI(Projectile projectile)
        {
            if ((projectile.type == ProjectileID.ThrowingKnife || projectile.type == ProjectileID.PoisonedKnife) && projectile.owner == Main.myPlayer)
            {
                trailCounter++;
                
                if (trailCounter % 2 == 0)
                {
                    Dust trail = Dust.NewDustPerfect(projectile.Center, DustID.JungleSpore, 
                        -projectile.velocity * 0.2f, 100, Color.LimeGreen, Main.rand.NextFloat(0.8f, 1.4f));
                    trail.noGravity = true;
                    trail.fadeIn = 1.1f;
                }
                
                if (Main.rand.NextBool(3))
                {
                    Dust poison = Dust.NewDustPerfect(projectile.Center + Main.rand.NextVector2Circular(8, 8), 
                        DustID.Poisoned, Vector2.Zero, 150, Color.Green, Main.rand.NextFloat(0.6f, 1.0f));
                    poison.noGravity = true;
                    poison.velocity = -projectile.velocity * 0.1f;
                }
            }
        }

        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (projectile.type == ProjectileID.ThrowingKnife || projectile.type == ProjectileID.PoisonedKnife)
            {
                target.AddBuff(BuffID.Poisoned, 300);
                
                for (int i = 0; i < 25; i++)
                {
                    Vector2 speed = Main.rand.NextVector2CircularEdge(5f, 5f);
                    Dust explosion = Dust.NewDustPerfect(projectile.Center, DustID.JungleSpore, speed, 100, 
                        Color.LimeGreen, Main.rand.NextFloat(1.5f, 2.5f));
                    explosion.noGravity = true;
                    explosion.fadeIn = 1.4f;
                }
                
                for (int i = 0; i < 12; i++)
                {
                    float angle = MathHelper.ToRadians(i * 30);
                    Vector2 venomVel = new Vector2(Main.rand.NextFloat(3f, 6f), 0).RotatedBy(angle);
                    Dust venom = Dust.NewDustPerfect(projectile.Center, DustID.Poisoned, venomVel, 180, 
                        Color.Green, Main.rand.NextFloat(1.8f, 2.4f));
                    venom.noGravity = true;
                }
                
                for (int i = 0; i < 8; i++)
                {
                    Dust bubble = Dust.NewDustPerfect(projectile.Center + Main.rand.NextVector2Circular(20, 20), 
                        DustID.ToxicBubble, Main.rand.NextVector2Circular(3, 3), 200, Color.YellowGreen, 1.6f);
                    bubble.noGravity = true;
                }
                
                if (projectile.type == ProjectileID.ThrowingKnife)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        float rotation = MathHelper.ToRadians(i * 90 + Main.rand.Next(-15, 16));
                        Vector2 velocity = new Vector2(6f, 0).RotatedBy(rotation);
                        
                        int seed = Projectile.NewProjectile(projectile.GetSource_FromThis(), projectile.Center, velocity,
                            ProjectileID.Seed, (int)(damageDone * 0.4f), 1f, projectile.owner);
                        
                        if (seed >= 0 && seed < Main.maxProjectiles && Main.projectile[seed].active)
                        {
                            Main.projectile[seed].CritChance = projectile.CritChance;
                        }
                    }
                }
            }
        }
    }
}

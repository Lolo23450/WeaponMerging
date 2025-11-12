using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;

namespace WeaponMerging.Content.Projectiles
{
    public class SolarConductorProjectile : GlobalProjectile
    {
        public override bool InstancePerEntity => true;
        
        private int trailCounter = 0;
        private int lifeTimer = 0;
        private bool isSolarConductor = false;
        private bool hasSplit = false;
        private bool hasSpawnedOrbiters = false;
        private int[] orbiterIDs = new int[4] { -1, -1, -1, -1 };
        private float orbitAngle = 0f;

        public override void AI(Projectile projectile)
        {
            
            isSolarConductor = false;
            if (projectile.owner >= 0 && projectile.owner < Main.maxPlayers)
            {
                Player player = Main.player[projectile.owner];
                if (player != null && player.HeldItem != null && player.HeldItem.type == ModContent.ItemType<Items.Weapons.SolarConductor>())
                {
                    isSolarConductor = true;
                }
            }

            
            if (projectile.type == ProjectileID.BallofFire && isSolarConductor)
            {
                trailCounter++;
                lifeTimer++;
                orbitAngle += 0.18f; 
                
                
                Lighting.AddLight(projectile.Center, 1.05f, 0.65f, 0.18f);

                if (!hasSpawnedOrbiters && lifeTimer == 2)
                {
                    hasSpawnedOrbiters = true;
                    for (int i = 0; i < 4; i++)
                    {
                        float angle = MathHelper.ToRadians(i * 90);
                        Vector2 offset = new Vector2(30, 0).RotatedBy(angle);

                        
                        int orb = Projectile.NewProjectile(projectile.GetSource_FromThis(),
                            projectile.Center + offset, Vector2.Zero, ProjectileID.Flamelash,
                            0, 0f, projectile.owner);

                        if (orb >= 0 && orb < Main.maxProjectiles)
                        {
                            orbiterIDs[i] = orb;
                            Main.projectile[orb].friendly = false;
                            Main.projectile[orb].hostile = false;
                            Main.projectile[orb].tileCollide = false;
                            Main.projectile[orb].timeLeft = 9999;
                            Main.projectile[orb].scale = 0.95f;
                            Main.projectile[orb].alpha = 90;
                            
                            Lighting.AddLight(Main.projectile[orb].Center, 0.95f, 0.5f, 0.18f);
                            
                            Main.projectile[orb].velocity = Vector2.Zero;
                            Main.projectile[orb].ai[0] = 0f;
                            Main.projectile[orb].ai[1] = 0f;
                        }
                        else
                        {
                            orbiterIDs[i] = -1;
                        }
                    }
                }
            
                
                if (hasSpawnedOrbiters)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        int id = orbiterIDs[i];
                        if (id >= 0 && id < Main.maxProjectiles)
                        {
                            Projectile orb = Main.projectile[id];
                            if (orb != null && orb.active)
                            {
                                float angle = orbitAngle + MathHelper.ToRadians(i * 90);
                                Vector2 offset = new Vector2(36, 0).RotatedBy(angle);
                                orb.Center = projectile.Center + offset;
                                orb.rotation += 0.25f + 0.03f * i;
                                
                                Lighting.AddLight(orb.Center, 0.6f + 0.15f * (float)Main.rand.NextDouble(), 0.3f, 0.06f);
                                
                                if (Main.rand.NextBool(8))
                                {
                                    Dust spark = Dust.NewDustPerfect(orb.Center + Main.rand.NextVector2Circular(6,6), DustID.MagicMirror, 
                                        Main.rand.NextVector2Circular(1.2f,1.2f), 150, Color.Gold, Main.rand.NextFloat(0.7f, 1.2f));
                                    spark.noGravity = true;
                                    spark.fadeIn = 0.8f;
                                }
                            }
                            else
                            {
                                orbiterIDs[i] = -1;
                            }
                        }
                    }
                }
                
                
                if (trailCounter % 2 == 0)
                {
                    Dust core = Dust.NewDustPerfect(projectile.Center, DustID.Torch, 
                        Vector2.Zero, 0, Color.Yellow, Main.rand.NextFloat(1.9f, 2.8f));
                    core.noGravity = true;
                    core.fadeIn = 1.7f;
                    
                    Dust smoke = Dust.NewDustPerfect(projectile.Center - projectile.velocity * 0.6f, DustID.Smoke, 
                        -projectile.velocity * 0.05f, 100, Color.OrangeRed, Main.rand.NextFloat(0.9f, 1.6f));
                    smoke.noGravity = true;
                    smoke.alpha = 120;
                }
                
                
                if (Main.rand.NextBool(2))
                {
                    Dust flame = Dust.NewDustPerfect(projectile.Center - projectile.velocity * 0.3f, DustID.Torch, 
                        -projectile.velocity * 0.35f + Main.rand.NextVector2Circular(0.6f,0.6f), 0, Color.OrangeRed, Main.rand.NextFloat(1.3f, 2.0f));
                    flame.noGravity = true;
                    flame.fadeIn = 0.9f;
                }
                
                
                if (trailCounter % 3 == 0)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        Dust magic = Dust.NewDustPerfect(projectile.Center + Main.rand.NextVector2Circular(4,4), DustID.PurpleTorch, 
                            -projectile.velocity * 0.22f + Main.rand.NextVector2Circular(0.6f,0.6f), 140, Color.MediumPurple, Main.rand.NextFloat(0.9f, 1.6f));
                        magic.noGravity = true;
                    }
                }
                
                
                if (Main.rand.NextBool(3))
                {
                    Vector2 offset = Main.rand.NextVector2Circular(18, 18);
                    Dust glow = Dust.NewDustPerfect(projectile.Center + offset, DustID.MagicMirror, 
                        -offset * 0.06f + Main.rand.NextVector2Circular(0.2f,0.2f), 120, Color.Gold, Main.rand.NextFloat(1.0f, 1.8f));
                    glow.noGravity = true;
                    glow.alpha = 120;
                }
                
                
                if (lifeTimer >= 90 && !hasSplit)
                {
                    hasSplit = true;
                    
                    
                    for (int i = 0; i < 4; i++)
                    {
                        int id = orbiterIDs[i];
                        if (id >= 0 && id < Main.maxProjectiles)
                        {
                            if (Main.projectile[id].active)
                            {
                                
                                for (int j = 0; j < 8; j++)
                                {
                                    Dust mini = Dust.NewDustPerfect(Main.projectile[id].Center + Main.rand.NextVector2Circular(4,4), DustID.PurpleTorch, 
                                        Main.rand.NextVector2CircularEdge(2f,2f), 140, Color.MediumPurple, Main.rand.NextFloat(0.6f, 1.4f));
                                    mini.noGravity = true;
                                }
                                Main.projectile[id].Kill();
                            }
                        }
                        orbiterIDs[i] = -1;
                    }
                    
                    
                    for (int i = 0; i < 50; i++)
                    {
                        Vector2 speed = Main.rand.NextVector2CircularEdge(10f, 10f) * Main.rand.NextFloat(0.6f, 1.3f);
                        Dust explosion = Dust.NewDustPerfect(projectile.Center, DustID.Torch, speed, 0, 
                            Color.Orange, Main.rand.NextFloat(2.0f, 4.0f));
                        explosion.noGravity = true;
                        explosion.fadeIn = 1.8f;
                    }
                    
                    
                    for (int i = 0; i < 30; i++)
                    {
                        Vector2 speed = Main.rand.NextVector2CircularEdge(7f, 7f) * Main.rand.NextFloat(0.6f, 1.1f);
                        Dust magic = Dust.NewDustPerfect(projectile.Center + Main.rand.NextVector2Circular(6,6), DustID.PurpleTorch, speed, 150, 
                            Color.MediumPurple, Main.rand.NextFloat(1.6f, 2.8f));
                        magic.noGravity = true;
                    }
                    
                    
                    for (int i = 0; i < 36; i++)
                    {
                        float angle = MathHelper.ToRadians(i * 10);
                        Vector2 ringVel = new Vector2(9f, 0).RotatedBy(angle);
                        Dust ring = Dust.NewDustPerfect(projectile.Center, DustID.MagicMirror, ringVel, 100, 
                            Color.Gold, Main.rand.NextFloat(1.6f, 2.4f));
                        ring.noGravity = true;
                    }
                    
                    
                    int missileCount = 0;
                    for (int i = 0; i < Main.maxNPCs && missileCount < 6; i++)
                    {
                        NPC npc = Main.npc[i];
                        if (npc.active && !npc.friendly && npc.lifeMax > 5 && !npc.dontTakeDamage)
                        {
                            float distance = Vector2.Distance(projectile.Center, npc.Center);
                            if (distance < 900f)
                            {
                                Vector2 direction = npc.Center - projectile.Center;
                                if (direction == Vector2.Zero) direction = new Vector2(0.1f, 0);
                                Vector2 velocity = Vector2.Normalize(direction) * 13f;
                                
                                velocity = velocity.RotatedByRandom(MathHelper.ToRadians(6f));
                                
                                int missile = Projectile.NewProjectile(projectile.GetSource_FromThis(), 
                                    projectile.Center, velocity, ProjectileID.Flamelash, 
                                    (int)(projectile.damage * 0.65f), 3f, projectile.owner);
                                
                                if (missile >= 0 && missile < Main.maxProjectiles && Main.projectile[missile].active)
                                {
                                    Main.projectile[missile].CritChance = projectile.CritChance;
                                    Main.projectile[missile].DamageType = DamageClass.Magic;
                                    Main.projectile[missile].tileCollide = false;
                                    Main.projectile[missile].timeLeft = 220;
                                    
                                    for (int d = 0; d < 3; d++)
                                    {
                                        Dust trail = Dust.NewDustPerfect(Main.projectile[missile].Center - velocity * 0.2f * d, DustID.MagicMirror, 
                                            -velocity * 0.04f + Main.rand.NextVector2Circular(0.3f,0.3f), 120, Color.Gold, 0.9f);
                                        trail.noGravity = true;
                                    }
                                }
                                
                                missileCount++;
                            }
                        }
                    }
                    
                    
                    if (missileCount == 0)
                    {
                        for (int i = 0; i < 8; i++)
                        {
                            float angle = MathHelper.ToRadians(i * (360f / 8f));
                            Vector2 velocity = new Vector2(14f, 0).RotatedBy(angle) * Main.rand.NextFloat(0.9f, 1.1f);
                            
                            int missile = Projectile.NewProjectile(projectile.GetSource_FromThis(), 
                                projectile.Center, velocity, ProjectileID.Flamelash, 
                                (int)(projectile.damage * 0.65f), 3f, projectile.owner);
                            
                            if (missile >= 0 && missile < Main.maxProjectiles && Main.projectile[missile].active)
                            {
                                Main.projectile[missile].CritChance = projectile.CritChance;
                                Main.projectile[missile].DamageType = DamageClass.Magic;
                                Main.projectile[missile].timeLeft = 220;
                            }
                        }
                    }
                    
                    SoundEngine.PlaySound(SoundID.Item14, projectile.Center); 
                    
                    Lighting.AddLight(projectile.Center, 1.6f, 0.9f, 0.2f);
                    projectile.Kill();
                }
            }
            
            
            if (projectile.type == ProjectileID.MagicMissile && isSolarConductor)
            {
                
                Lighting.AddLight(projectile.Center, 0.9f, 0.45f, 0.08f);

                
                if (Main.rand.NextBool(2))
                {
                    Dust solar = Dust.NewDustPerfect(projectile.Center + Main.rand.NextVector2Circular(2,2), DustID.Torch, 
                        -projectile.velocity * 0.22f + Main.rand.NextVector2Circular(0.2f,0.2f), 0, Color.Orange, Main.rand.NextFloat(1.2f, 1.9f));
                    solar.noGravity = true;
                    solar.fadeIn = 1.2f;
                }
                
                
                if (Main.rand.NextBool(3))
                {
                    Dust magic = Dust.NewDustPerfect(projectile.Center + Main.rand.NextVector2Circular(1.5f,1.5f), DustID.PurpleTorch, 
                        Main.rand.NextVector2Circular(0.2f,0.2f), 140, Color.MediumPurple, Main.rand.NextFloat(1.0f, 1.6f));
                    magic.noGravity = true;
                    magic.alpha = 90;
                }
                
                
                if (Main.rand.NextBool(4))
                {
                    Dust sparkle = Dust.NewDustPerfect(projectile.Center + Main.rand.NextVector2Circular(2,2), DustID.MagicMirror, 
                        -projectile.velocity * 0.05f + Main.rand.NextVector2Circular(0.3f,0.3f), 120, Color.Gold, 1.2f);
                    sparkle.noGravity = true;
                }

                
                if (trailCounter % 4 == 0)
                {
                    Dust ghost = Dust.NewDustPerfect(projectile.Center - projectile.velocity * 0.5f, DustID.Torch, 
                        Vector2.Zero, 0, Color.Orange, 0.9f);
                    ghost.noGravity = true;
                    ghost.fadeIn = 0.9f;
                    ghost.alpha = 150;
                }
            }
        }

        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {
            
            if (projectile.type == ProjectileID.BallofFire && isSolarConductor)
            {
                
                for (int i = 0; i < 32; i++)
                {
                    Vector2 speed = Main.rand.NextVector2CircularEdge(7f, 7f) * Main.rand.NextFloat(0.6f, 1.4f);
                    Dust explosion = Dust.NewDustPerfect(projectile.Center, DustID.Torch, speed, 0, 
                        Color.OrangeRed, Main.rand.NextFloat(1.9f, 3.0f));
                    explosion.noGravity = true;
                }
                
                
                for (int i = 0; i < 18; i++)
                {
                    Dust magic = Dust.NewDustPerfect(projectile.Center + Main.rand.NextVector2Circular(4,4), DustID.PurpleTorch, 
                        Main.rand.NextVector2Circular(5f, 5f), 150, Color.Purple, Main.rand.NextFloat(1.4f, 2.2f));
                    magic.noGravity = true;
                }
                
                
                for (int i = 0; i < 6; i++)
                {
                    Dust gold = Dust.NewDustPerfect(projectile.Center + Main.rand.NextVector2CircularEdge(10,10), DustID.MagicMirror,
                        Main.rand.NextVector2Circular(2f,2f), 120, Color.Gold, Main.rand.NextFloat(1.0f,1.6f));
                    gold.noGravity = true;
                }
                
                target.AddBuff(BuffID.OnFire, 240);
                SoundEngine.PlaySound(SoundID.Item20, projectile.Center); 
            }
            
            
            if (projectile.type == ProjectileID.MagicMissile && isSolarConductor)
            {
                
                for (int i = 0; i < 20; i++)
                {
                    float angle = MathHelper.ToRadians(i * 18);
                    Vector2 speed = new Vector2(5f, 0).RotatedBy(angle) * Main.rand.NextFloat(0.7f, 1.3f);
                    Dust solar = Dust.NewDustPerfect(projectile.Center, DustID.Torch, speed, 0, 
                        Color.Orange, Main.rand.NextFloat(1.5f, 2.2f));
                    solar.noGravity = true;
                }
                
                
                for (int i = 0; i < 10; i++)
                {
                    Dust sparkle = Dust.NewDustPerfect(projectile.Center + Main.rand.NextVector2Circular(3f, 3f), DustID.PurpleTorch, 
                        Main.rand.NextVector2Circular(1.2f, 1.2f), 180, Color.MediumPurple, 1.4f);
                    sparkle.noGravity = true;
                }
                
                
                for (int i = 0; i < 4; i++)
                {
                    Dust fleck = Dust.NewDustPerfect(projectile.Center + Main.rand.NextVector2Circular(6,6), DustID.MagicMirror, 
                        Main.rand.NextVector2Circular(1.6f,1.6f), 120, Color.Gold, 1.1f);
                    fleck.noGravity = true;
                }

                target.AddBuff(BuffID.OnFire, 180);
                SoundEngine.PlaySound(SoundID.Item27, projectile.Center); 
            }
        }
    }
}

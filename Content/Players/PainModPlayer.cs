using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using System;

namespace WeaponMerging.Content.Players
{
    public class PainModPlayer : ModPlayer
    {
        public int orbCount = 0;
        public bool laserModeActive = false;
        private const int MAX_ORBS = 5;

        private int laserTick = 0;
        private const int LASER_INTERVAL = 6; 

        private int GetMaxOrbs(Player player) => MAX_ORBS + player.GetModPlayer<Content.Players.AccessoryEffectsPlayer>().BonusMaxOrbs;
        private int GetChargeThreshold(Player player) => GetMaxOrbs(player);

        
        
        
        public void GainOrb(Player player)
        {
            int maxOrbs = GetMaxOrbs(player);
            orbCount = Math.Min(maxOrbs, orbCount + 1);

            if (orbCount >= GetChargeThreshold(player))
                laserModeActive = true;

            if (Main.myPlayer == player.whoAmI)
            {
                int index = orbCount - 1;

                Projectile proj = Projectile.NewProjectileDirect(
                    new EntitySource_Misc("PainOrbSpawn"),
                    player.Center + new Vector2(0, -24),
                    Vector2.Zero,
                    ModContent.ProjectileType<Content.Projectiles.PainOrbProjectile>(),
                    (int)(player.HeldItem.damage * 0.5f),
                    0f,
                    player.whoAmI
                );

                proj.ai[1] = 0f;            
                proj.localAI[0] = index;   
                proj.localAI[1] = 0f;      
                proj.netUpdate = true;
            }
        }

        
        
        
        public void ReleaseAllOrbs(Player player, EntitySource_ItemUse_WithAmmo source)
        {
            
            if (orbCount >= GetChargeThreshold(player))
            {
                CombatText.NewText(player.getRect(), new Color(80, 255, 120), "Charged!");
                SoundEngine.PlaySound(SoundID.Item4 with { Volume = 0.7f }, player.Center);
                return;
            }

            
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile pr = Main.projectile[i];
                if (pr.active && pr.owner == player.whoAmI && pr.type == ModContent.ProjectileType<Content.Projectiles.PainOrbProjectile>())
                {
                    pr.ai[1] = 1f; 

                    NPC target = FindNearestTarget(pr.Center, 600f);
                    Vector2 dir = (target != null ? target.Center : Main.MouseWorld) - pr.Center;
                    if (dir == Vector2.Zero) dir = Main.rand.NextVector2Unit();
                    dir.Normalize();
                    pr.velocity = dir * 20f;
                    pr.timeLeft = 300;
                    pr.netUpdate = true;
                }
            }

            
            laserModeActive = false;
            orbCount = 0;

            SoundEngine.PlaySound(SoundID.Item14 with { Volume = 0.7f }, player.Center);
        }

        private NPC FindNearestTarget(Vector2 position, float maxRange)
        {
            NPC result = null;
            float nearest = maxRange;

            foreach (NPC npc in Main.npc)
            {
                if (npc.active && !npc.friendly && npc.life > 0)
                {
                    float dist = Vector2.Distance(position, npc.Center);
                    if (dist < nearest)
                    {
                        nearest = dist;
                        result = npc;
                    }
                }
            }
            return result;
        }

        public override void PostUpdateEquips()
        {
            Player player = Player;
            Item held = player.HeldItem;

            
            if (held?.type == ModContent.ItemType<Content.Items.Weapons.PainSpiral>() && laserModeActive)
            {
                laserTick++;

                var acc = player.GetModPlayer<Content.Players.AccessoryEffectsPlayer>();
                int interval = Math.Max(2, LASER_INTERVAL - acc.IntervalReduction);
                if (laserTick >= interval)
                {
                    laserTick = 0;

                    int shotsPerOrb = ((orbCount >= 6) ? 12 : 8) + acc.BonusShotsPerOrb;
                    int laserDamage = Math.Max(1, (int)(held.damage / 4f));

                    if (Main.myPlayer == player.whoAmI)
                    {
                        for (int i = 0; i < Main.maxProjectiles; i++)
                        {
                            Projectile pr = Main.projectile[i];
                            if (!pr.active || pr.owner != player.whoAmI || pr.type != ModContent.ProjectileType<Content.Projectiles.PainOrbProjectile>())
                                continue;

                            if (pr.ai[1] != 0f)
                                continue;

                            if (pr.localAI[1] <= 0f)
                                pr.localAI[1] = shotsPerOrb;

                            if (pr.localAI[1] > 0f)
                            {
                                NPC target = FindNearestTarget(pr.Center, 700f);
                                Vector2 vel;

                                if (target != null)
                                {
                                    vel = (target.Center - pr.Center).SafeNormalize(Vector2.Zero) * 14f;
                                }
                                else
                                {
                                    vel = Main.rand.NextVector2Unit() * 20f;
                                }

                                Projectile.NewProjectile(
                                    new EntitySource_Misc("PainOrbLaserFromOrb"),
                                    pr.Center,
                                    vel,
                                    ModContent.ProjectileType<Content.Projectiles.PainOrbLaser>(),
                                    laserDamage,
                                    0f,
                                    player.whoAmI
                                );

                                pr.localAI[1]--;
                                pr.netUpdate = true;

                                if (pr.localAI[1] <= 0f)
                                {
                                    var accEffects = player.GetModPlayer<Content.Players.AccessoryEffectsPlayer>();
                                    
                                    if (accEffects.RollPersist())
                                    {
                                        pr.localAI[1] = 1f; 
                                        pr.netUpdate = true;
                                    }
                                    else
                                    {
                                        pr.Kill();
                                        orbCount = Math.Max(0, orbCount - 1);
                                        if (orbCount <= 0)
                                            laserModeActive = false;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                laserTick = 0;
            }
        }
    }
}


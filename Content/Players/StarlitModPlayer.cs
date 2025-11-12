using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using System;

namespace WeaponMerging.Content.Players
{
    public class StarlitModPlayer : ModPlayer
    {
        public int orbCount = 0;
        public bool laserModeActive = false;
        private const int MAX_ORBS = 4;

        
        public bool hasShield = false;
        public int shieldRechargeTimer = 0;
        private const int SHIELD_RECHARGE_TIME = 300; 
        private const int SHIELD_THRESHOLD = 3; 

        private int starburstTick = 0;
        private const int STARBURST_INTERVAL = 8;

        private int GetMaxOrbs(Player player) => MAX_ORBS + player.GetModPlayer<Content.Players.AccessoryEffectsPlayer>().BonusMaxOrbs;
        private int GetChargeThreshold(Player player) => GetMaxOrbs(player);

        public void GainOrb(Player player)
        {
            int maxOrbs = GetMaxOrbs(player);
            orbCount = Math.Min(maxOrbs, orbCount + 1);

            if (orbCount >= GetChargeThreshold(player))
                laserModeActive = true;
            
            
            if (orbCount >= SHIELD_THRESHOLD && !hasShield && shieldRechargeTimer <= 0)
            {
                hasShield = true;
                SoundEngine.PlaySound(SoundID.Item4 with { Volume = 0.6f, Pitch = 0.5f }, player.Center);
                
                
                for (int i = 0; i < 30; i++)
                {
                    Dust d = Dust.NewDustDirect(player.Center - Vector2.One * 20, 40, 40, DustID.YellowStarDust,
                        Main.rand.NextFloat(-5f, 5f), Main.rand.NextFloat(-5f, 5f));
                    d.noGravity = true;
                    d.scale = 1.5f;
                }
                
                CombatText.NewText(player.getRect(), new Color(255, 240, 100), "Shield Active!", false, false);
            }

            if (Main.myPlayer == player.whoAmI)
            {
                int index = orbCount - 1;

                Projectile proj = Projectile.NewProjectileDirect(
                    new EntitySource_Misc("StarlitOrbSpawn"),
                    player.Center + new Vector2(0, -20),
                    Vector2.Zero,
                    ModContent.ProjectileType<Content.Projectiles.StarlitOrbProjectile>(),
                    (int)(player.HeldItem.damage * 0.6f),
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
                CombatText.NewText(player.getRect(), new Color(255, 240, 100), "Starlit Burst!", true);
                SoundEngine.PlaySound(SoundID.Item9 with { Volume = 0.8f, Pitch = 0.3f }, player.Center);
                return;
            }

            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile pr = Main.projectile[i];
                if (pr.active && pr.owner == player.whoAmI && pr.type == ModContent.ProjectileType<Content.Projectiles.StarlitOrbProjectile>())
                {
                    pr.ai[1] = 1f;

                    NPC target = FindNearestTarget(pr.Center, 500f);
                    Vector2 dir = (target != null ? target.Center : Main.MouseWorld) - pr.Center;
                    if (dir == Vector2.Zero) dir = Main.rand.NextVector2Unit();
                    dir.Normalize();
                    pr.velocity = dir * 18f;
                    pr.timeLeft = 240;
                    pr.netUpdate = true;
                }
            }

            laserModeActive = false;
            orbCount = 0;
            hasShield = false; 
            shieldRechargeTimer = SHIELD_RECHARGE_TIME;

            SoundEngine.PlaySound(SoundID.Item9 with { Volume = 0.7f, Pitch = 0.1f }, player.Center);
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

            
            if (shieldRechargeTimer > 0)
            {
                shieldRechargeTimer--;
                if (shieldRechargeTimer == 0 && orbCount >= SHIELD_THRESHOLD)
                {
                    hasShield = true;
                    SoundEngine.PlaySound(SoundID.Item4 with { Volume = 0.5f, Pitch = 0.4f }, player.Center);
                    CombatText.NewText(player.getRect(), new Color(100, 200, 255), "Shield Recharged!", false, false);
                }
            }

            
            if (orbCount < SHIELD_THRESHOLD && hasShield)
            {
                hasShield = false;
            }

            if (held?.type == ModContent.ItemType<Content.Items.Weapons.StarlitWhirlwind>() && laserModeActive)
            {
                starburstTick++;

                var acc = player.GetModPlayer<Content.Players.AccessoryEffectsPlayer>();
                int interval = Math.Max(2, STARBURST_INTERVAL - acc.IntervalReduction);
                if (starburstTick >= interval)
                {
                    starburstTick = 0;

                    int shotsPerOrb = 6 + acc.BonusShotsPerOrb;
                    int starDamage = Math.Max(1, (int)(held.damage / 3f));

                    if (Main.myPlayer == player.whoAmI)
                    {
                        for (int i = 0; i < Main.maxProjectiles; i++)
                        {
                            Projectile pr = Main.projectile[i];
                            if (!pr.active || pr.owner != player.whoAmI || pr.type != ModContent.ProjectileType<Content.Projectiles.StarlitOrbProjectile>())
                                continue;

                            if (pr.ai[1] != 0f)
                                continue;

                            if (pr.localAI[1] <= 0f)
                                pr.localAI[1] = shotsPerOrb;

                            if (pr.localAI[1] > 0f)
                            {
                                NPC target = FindNearestTarget(pr.Center, 600f);
                                Vector2 vel;

                                if (target != null)
                                {
                                    vel = (target.Center - pr.Center).SafeNormalize(Vector2.Zero) * 11f;
                                }
                                else
                                {
                                    vel = Main.rand.NextVector2Unit() * 16f;
                                }

                                Projectile.NewProjectile(
                                    new EntitySource_Misc("StarlitStarburstFromOrb"),
                                    pr.Center,
                                    vel,
                                    ProjectileID.FallingStar,
                                    starDamage,
                                    1f,
                                    player.whoAmI
                                );

                                pr.localAI[1]--;
                                pr.netUpdate = true;

                                if (pr.localAI[1] <= 0f)
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
            else
            {
                starburstTick = 0;
            }
        }

        public override bool FreeDodge(Player.HurtInfo info)
        {
            if (hasShield)
            {
                
                hasShield = false;
                shieldRechargeTimer = SHIELD_RECHARGE_TIME;
                
                
                SoundEngine.PlaySound(SoundID.Item14 with { Volume = 0.8f, Pitch = 0.3f }, Player.Center);
                
                for (int i = 0; i < 40; i++)
                {
                    Dust d = Dust.NewDustDirect(Player.Center - Vector2.One * 30, 60, 60, DustID.YellowStarDust,
                        Main.rand.NextFloat(-10f, 10f), Main.rand.NextFloat(-10f, 10f));
                    d.noGravity = true;
                    d.scale = 2f;
                }
                
                
                if (Main.myPlayer == Player.whoAmI)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        float angle = MathHelper.TwoPi * i / 8f;
                        Vector2 velocity = angle.ToRotationVector2() * 12f;
                        
                        Projectile.NewProjectile(
                            Player.GetSource_FromThis(),
                            Player.Center,
                            velocity,
                            ProjectileID.FallingStar,
                            (int)(Player.HeldItem.damage * 0.5f),
                            2f,
                            Player.whoAmI
                        );
                    }
                }
                
                CombatText.NewText(Player.getRect(), new Color(255, 200, 100), "Shield Broken!", true, false);
                
                return true; 
            }
            
            return false;
        }

        public override void DrawEffects(PlayerDrawSet drawInfo, ref float r, ref float g, ref float b, ref float a, ref bool fullBright)
        {
            
            if (hasShield && drawInfo.drawPlayer.whoAmI == Main.myPlayer)
            {
                
                if (Main.rand.NextBool(10))
                {
                    Dust d = Dust.NewDustDirect(drawInfo.drawPlayer.position, drawInfo.drawPlayer.width, drawInfo.drawPlayer.height, 
                        DustID.YellowStarDust, 0, 0, 100, default, 1.2f);
                    d.noGravity = true;
                    d.velocity *= 0.3f;
                }
            }
        }
    }
}

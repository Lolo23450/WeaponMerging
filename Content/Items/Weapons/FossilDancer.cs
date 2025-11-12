using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Terraria.Audio;

namespace WeaponMerging.Content.Items.Weapons
{
    public class FossilDancer : ModItem
    {
        private int comboCounter = 0;
        private int comboResetTimer = 0;
        private const int COMBO_RESET_TIME = 45; 
        
        public override void SetDefaults()
        {
            Item.width = 68;
            Item.height = 60;
            Item.scale = 1.1f;
            Item.rare = ItemRarityID.Orange;
            Item.value = Item.sellPrice(gold: 1, silver: 50);

            
            Item.useTime = 26;
            Item.useAnimation = 26;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.autoReuse = true;
            
            
            Item.damage = 8;
            Item.knockBack = 6f;
            Item.DamageType = DamageClass.Melee;
            
            
            Item.shoot = ModContent.ProjectileType<Content.Projectiles.FossilDancerProjectile>();
            Item.shootSpeed = 1f;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            
            Item.UseSound = null; 
        }

        public override void UpdateInventory(Player player)
        {
            
            if (comboResetTimer > 0)
            {
                comboResetTimer--;
                if (comboResetTimer <= 0)
                {
                    comboCounter = 0;
                }
            }
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            
            damage = comboCounter switch
            {
                0 => damage,
                1 => (int)(damage * 1.35f),
                2 => (int)(damage * 1.45f),
                3 => (int)(damage * 1.8f), 
                _ => damage
            };
            
            
            knockback = comboCounter switch
            {
                3 => knockback * 3f, 
                2 => knockback * 1.5f,
                _ => knockback
            };
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            
            Vector2 toMouse = Main.MouseWorld - player.Center;
            toMouse.Normalize();
            
            
            Projectile proj = Projectile.NewProjectileDirect(
                source,
                player.Center,
                toMouse, 
                type,
                damage,
                knockback,
                player.whoAmI
            );
            
            
            proj.ai[0] = comboCounter;
            
            
            SpawnComboStartEffects(player, comboCounter);
            
            
            comboCounter++;
            if (comboCounter > 3)
            {
                comboCounter = 0;
                
                
                SoundEngine.PlaySound(SoundID.Item4 with { Volume = 0.5f, Pitch = -0.3f }, player.position);
                for (int i = 0; i < 20; i++)
                {
                    Dust.NewDust(player.position, player.width, player.height, 31, 
                        Main.rand.NextFloat(-5f, 5f), Main.rand.NextFloat(-5f, 5f), 100, default, 2f);
                }
            }
            
            
            comboResetTimer = COMBO_RESET_TIME;
            
            return false;
        }
                
        private void SpawnComboStartEffects(Player player, int combo)
        {
            int dustCount = combo switch
            {
                0 => 5,
                1 => 8,
                2 => 12,
                3 => 20,
                _ => 5
            };
            
            int dustType = combo switch
            {
                0 => 32,
                1 => 36,
                2 => 159,
                3 => 6,
                _ => 32
            };
            
            for (int i = 0; i < dustCount; i++)
            {
                Vector2 velocity = Main.rand.NextVector2Circular(3f, 3f);
                Dust dust = Dust.NewDustDirect(player.position, player.width, player.height, dustType, velocity.X, velocity.Y, 100, default, 1.5f);
                dust.noGravity = true;
            }
            
            
            if (combo == 3)
            {
                CombatText.NewText(player.Hitbox, new Color(255, 200, 100), "FINISHER!", true, false);
            }
        }
    }
}


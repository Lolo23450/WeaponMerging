using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Terraria.Audio;

namespace WeaponMerging.Content.Items.Weapons
{
    public class CelestialFrostBlade : ModItem
    {
        private int comboCounter = 0;
        private int comboResetTimer = 0;
        private const int COMBO_RESET_TIME = 60; 
        
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 60;
            Item.height = 60;
            Item.scale = 1.2f;
            Item.rare = ItemRarityID.Pink;
            Item.value = Item.sellPrice(gold: 5);

            Item.useStyle = ItemUseStyleID.Shoot;
            Item.autoReuse = true;
            Item.channel = true;
            
            Item.damage = 72; 
            Item.knockBack = 6f;
            Item.DamageType = DamageClass.Melee;
            
            Item.shoot = ModContent.ProjectileType<Content.Projectiles.CelestialFrostBladeProjectile>();
            Item.shootSpeed = 1f;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            
            Item.UseSound = null; // Handled by projectile
        }

        public override void UpdateInventory(Player player)
        {
            if (comboResetTimer > 0)
            {
                comboResetTimer--;
                if (comboResetTimer <= 0) comboCounter = 0;
            }
        }

        public override bool CanUseItem(Player player)
        {
            // Dynamic combo pacing: Fast -> Fast -> Slow Sweep -> Heavy Slam
            switch (comboCounter)
            {
                case 0:
                case 1:
                    Item.useTime = 22; 
                    Item.useAnimation = 22;
                    break;
                case 2:
                    Item.useTime = 36; // Slower charge/windup sweep
                    Item.useAnimation = 36;
                    break;
                case 3:
                    Item.useTime = 48; // Huge heavy slam
                    Item.useAnimation = 48;
                    break;
            }
            return player.ownedProjectileCounts[Item.shoot] < 1;
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            damage = comboCounter switch
            {
                2 => (int)(damage * 1.25f),
                3 => (int)(damage * 2.50f), // Finisher massive damage
                _ => damage
            };
            
            knockback = comboCounter switch
            {
                3 => knockback * 2.5f, 
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
            
            // Pass the combo state and the duration of this specific swing
            proj.ai[0] = comboCounter;
            proj.ai[1] = Item.useAnimation; 
            
            SpawnComboStartEffects(player, comboCounter);
            
            comboCounter++;
            if (comboCounter > 3)
            {
                comboCounter = 0;
                SoundEngine.PlaySound(SoundID.Item28 with { Volume = 0.8f, Pitch = -0.2f }, player.position); // Shatter sound
            }
            
            comboResetTimer = COMBO_RESET_TIME;
            return false;
        }
                
        private void SpawnComboStartEffects(Player player, int combo)
        {
            int dustType = combo switch
            {
                3 => DustID.BlueFairy,
                _ => DustID.IceTorch
            };
            
            for (int i = 0; i < 10 + (combo * 5); i++)
            {
                Vector2 velocity = Main.rand.NextVector2Circular(5f, 5f);
                Dust dust = Dust.NewDustDirect(player.position, player.width, player.height, dustType, velocity.X, velocity.Y, 0, default, 1.5f);
                dust.noGravity = true;
                dust.velocity *= (combo + 1) * 0.6f; 
            }
            
            if (combo == 3) {
                CombatText.NewText(player.Hitbox, new Color(150, 255, 255), "FROSTFALL!", true, true);
            }
        }
    }
}
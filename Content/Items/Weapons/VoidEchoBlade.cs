using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;

namespace WeaponMerging.Content.Items.Weapons
{
    public class VoidEchoBlade : ModItem
    {
        private int specialCooldown;
        private const int MAX_COOLDOWN = 150;

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.damage = 68;
            Item.DamageType = DamageClass.Melee;
            Item.width = 64;
            Item.height = 64;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 6f;
            Item.value = Item.sellPrice(gold: 5, silver: 80);
            Item.rare = ItemRarityID.Purple;
            Item.autoReuse = true;
            Item.useTurn = false;
            Item.crit = 12;

            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.shoot = ModContent.ProjectileType<Projectiles.VoidEchoSlash>();
            Item.shootSpeed = 1f;
        }

        public override void UpdateInventory(Player player)
        {
            if (specialCooldown > 0)
            {
                specialCooldown--;
                if (specialCooldown == 0 && player.whoAmI == Main.myPlayer)
                {
                    // Visual/Audio cue when right-click is ready
                    SoundEngine.PlaySound(SoundID.Item117 with { Pitch = 0.5f, Volume = 0.6f }, player.position);
                    for (int i = 0; i < 15; i++)
                    {
                        Dust dust = Dust.NewDustDirect(player.position, player.width, player.height, DustID.Shadowflame, 
                            Main.rand.NextFloat(-4f, 4f), Main.rand.NextFloat(-4f, 4f), 0, default, 1.5f);
                        dust.noGravity = true;
                    }
                }
            }
        }

        public override bool AltFunctionUse(Player player) => specialCooldown <= 0;

        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                Item.useTime = 45; // Heavy, slow right-click
                Item.useAnimation = 45;
                Item.shoot = ModContent.ProjectileType<Projectiles.VoidEchoSpecial>();
            }
            else
            {
                Item.useTime = 26; // Faster left-click
                Item.useAnimation = 26;
                Item.shoot = ModContent.ProjectileType<Projectiles.VoidEchoSlash>();
            }
            return base.CanUseItem(player);
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            if (player.altFunctionUse == 2)
            {
                damage = (int)(damage * 2.2f);
                knockback *= 2.0f;
            }
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Vector2 toMouse = (Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitX);
            
            if (player.altFunctionUse == 2)
            {
                specialCooldown = MAX_COOLDOWN;
            }

            Projectile.NewProjectileDirect(source, player.Center, toMouse, type, damage, knockback, player.whoAmI);
            return false;
        }

    }
}
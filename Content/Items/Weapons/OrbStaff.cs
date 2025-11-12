using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using WeaponMerging.Content.Projectiles;

namespace WeaponMerging.Content.Items.Weapons
{
    public class OrbStaff : ModItem
    {
        public override void SetDefaults()
        {
            Item.damage = 12;
            Item.DamageType = DamageClass.Magic;
            Item.width = 40;
            Item.height = 40;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 4f;
            Item.value = Item.sellPrice(gold: 1);
            Item.rare = ItemRarityID.LightRed;
            Item.UseSound = SoundID.Item20;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<OrbProjectile>();
            Item.shootSpeed = 14f;
            Item.mana = 10;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.WoodenBow)
                .AddIngredient(ModContent.ItemType<OrbFragment>(), 12)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}


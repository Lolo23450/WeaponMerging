using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using WeaponMerging.Content.Projectiles;

namespace WeaponMerging.Content.Items.Weapons
{
    public class AuroraBow : ModItem
    {
        public override void SetDefaults()
        {
            Item.damage = 25;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 40;
            Item.height = 40;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 2f;
            Item.value = Item.sellPrice(silver: 50);
            Item.rare = ItemRarityID.LightRed;
            Item.UseSound = SoundID.Item5;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<AuroraArrow>();
            Item.shootSpeed = 10f;
            Item.useAmmo = AmmoID.Arrow;
        }

        public override bool Shoot(Player player, Terraria.DataStructures.EntitySource_ItemUse_WithAmmo source,
            Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            
            float spread = 0.1f;
            for (int i = -1; i <= 1; i++)
            {
                Vector2 vel = velocity.RotatedBy(spread * i);
                Projectile.NewProjectile(
                    source,
                    position,
                    vel,
                    ModContent.ProjectileType<AuroraArrow>(),
                    damage,
                    knockback,
                    player.whoAmI
                );
            }
            return false; 
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.DaedalusStormbow)
                .AddIngredient(ItemID.RainbowRod)
                .AddIngredient(ItemID.FallenStar, 10)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}


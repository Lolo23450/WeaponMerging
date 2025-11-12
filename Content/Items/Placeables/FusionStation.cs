using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponMerging.Content.Items.Placeables
{
    public class FusionStation : ModItem
    {
        public override string Texture => "Terraria/Images/Item_36"; 

        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 20;
            Item.maxStack = 99;
            Item.useTurn = true;
            Item.autoReuse = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.consumable = true;
            Item.value = Item.buyPrice(silver: 50);
            Item.rare = ItemRarityID.Blue;
            Item.createTile = ModContent.TileType<Content.Tiles.FusionStationTile>();
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.WorkBench, 1)
                .AddIngredient(ItemID.FallenStar, 5)
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }
}


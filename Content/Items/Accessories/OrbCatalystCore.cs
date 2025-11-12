using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponMerging.Content.Items.Accessories
{
    public class OrbCatalystCore : ModItem
    {
        public override string Texture => "Terraria/Images/Item_520"; 
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.accessory = true;
            Item.value = Item.sellPrice(gold: 3);
            Item.rare = ItemRarityID.Orange;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var acc = player.GetModPlayer<Content.Players.AccessoryEffectsPlayer>();
            acc.catalystEquipped = true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.SoulofLight, 8)
                .AddIngredient(ItemID.SoulofNight, 8)
                .AddIngredient(ItemID.FallenStar, 5)
                .AddTile(ModContent.TileType<Content.Tiles.FusionStationTile>())
                .Register();
        }
    }
}


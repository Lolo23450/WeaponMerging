using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponMerging.Content.Items.Accessories
{
    public class OrbAmplifierRing : ModItem
    {
        public override string Texture => "Terraria/Images/Item_3337"; // Example texture, replace with custom if available

        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 28;
            Item.value = Item.sellPrice(gold: 2);
            Item.rare = ItemRarityID.LightRed;
            Item.accessory = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<Content.Players.AccessoryEffectsPlayer>().amplifierEquipped = true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.GoldBar, 10)
                .AddIngredient(ModContent.ItemType<OrbFragment>(), 5)
                .AddTile(TileID.TinkerersWorkbench)
                .Register();
        }
    }
}

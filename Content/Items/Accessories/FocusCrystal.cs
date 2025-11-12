using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponMerging.Content.Items.Accessories
{
    public class FocusCrystal : ModItem
    {
        public override string Texture => "Terraria/Images/Item_521"; 

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
            player.GetModPlayer<Content.Players.AccessoryEffectsPlayer>().focusEquipped = true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.CrystalShard, 20)
                .AddIngredient(ItemID.SoulofLight, 6)
                .AddTile(TileID.TinkerersWorkbench)
                .Register();
        }
    }
}

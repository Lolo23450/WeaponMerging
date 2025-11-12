using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponMerging.Content.Items.Accessories
{
    public class AccelerantCharm : ModItem
    {
        public override string Texture => "Terraria/Images/Item_535"; 

        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 28;
            Item.value = Item.sellPrice(gold: 2);
            Item.rare = ItemRarityID.Orange;
            Item.accessory = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<Content.Players.AccessoryEffectsPlayer>().ComboReduction += 1; 
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.Bone, 30)
                .AddIngredient(ItemID.SoulofNight, 6)
                .AddTile(TileID.TinkerersWorkbench)
                .Register();
        }
    }
}


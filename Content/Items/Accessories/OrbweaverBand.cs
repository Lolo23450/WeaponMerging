using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponMerging.Content.Items.Accessories
{
    public class OrbweaverBand : ModItem
    {
        public override string Texture => "Terraria/Images/Item_861"; 

        public override void SetStaticDefaults()
        {
        }

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
            player.GetModPlayer<Content.Players.AccessoryEffectsPlayer>().orbBandEquipped = true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.SoulofLight, 8)
                .AddIngredient(ItemID.SoulofNight, 8)
                .AddTile(TileID.TinkerersWorkbench)
                .Register();
        }
    }
}


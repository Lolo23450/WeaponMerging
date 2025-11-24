using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponMerging.Content.Items.Accessories
{
    public class OrbInfusionCore : ModItem
    {
        public override string Texture => "Terraria/Images/Item_935";

        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 28;
            Item.value = Item.sellPrice(gold: 3);
            Item.rare = ItemRarityID.LightPurple;
            Item.accessory = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<Players.AccessoryEffectsPlayer>().infusionCoreEquipped = true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.CrystalShard, 15)
                .AddIngredient(ItemID.SoulofLight, 8)
                .AddIngredient(ItemID.SoulofNight, 4)
                .AddIngredient(ModContent.ItemType<OrbFragment>(), 12)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}

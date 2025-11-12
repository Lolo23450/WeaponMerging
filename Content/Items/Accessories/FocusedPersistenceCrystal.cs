using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponMerging.Content.Items.Accessories
{
    public class FocusedPersistenceCrystal : ModItem
    {
        public override string Texture => "Terraria/Images/Item_521"; 

        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 28;
            Item.value = Item.sellPrice(gold: 5);
            Item.rare = ItemRarityID.Red;
            Item.accessory = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var acc = player.GetModPlayer<Content.Players.AccessoryEffectsPlayer>();
            acc.focusedPersistenceEquipped = true; 
        }
    }
}

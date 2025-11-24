using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using WeaponMerging.Content.Players;

namespace WeaponMerging.Content.Items.Weapons
{
    public class OrbBlade : ModItem
    {
        public override void SetDefaults()
        {
            Item.damage = 12;
            Item.DamageType = DamageClass.Melee;
            Item.width = 40;
            Item.height = 40;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 4f;
            Item.value = Item.sellPrice(silver: 75);
            Item.rare = ItemRarityID.LightRed;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
        }

        public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {
            var orbPlayer = player.GetModPlayer<OrbManaPlayer>();
            if (orbPlayer.TrySpendOrbMana(5, player))
            {
                target.AddBuff(BuffID.OnFire, 120);
            }
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.WoodenSword)
                .AddIngredient(ModContent.ItemType<OrbFragment>(), 10)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}

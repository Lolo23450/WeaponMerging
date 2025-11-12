using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Terraria.Enums;
using WeaponMerging.Systems;

namespace WeaponMerging.Content.Tiles
{
    public class FusionStationTile : ModTile
    {
        public override string Texture => "Terraria/Images/Tiles_18"; 

        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileSolidTop[Type] = false;
            Main.tileNoAttach[Type] = true;
            Main.tileTable[Type] = true;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style2x1);
            TileObjectData.newTile.CoordinateHeights = new[] { 16 };
            TileObjectData.newTile.Origin = new Point16(0, 0);
            TileObjectData.newTile.Direction = TileObjectDirection.PlaceLeft;
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.SolidSide, 2, 0);
            TileObjectData.addTile(Type);
            
            TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
            TileObjectData.newAlternate.Origin = new Point16(1, 0);
            TileObjectData.newAlternate.Direction = TileObjectDirection.PlaceRight;
            TileObjectData.addAlternate(Type);

            AddMapEntry(new Color(120, 200, 255), CreateMapEntryName());
            DustType = DustID.BlueCrystalShard;
            RegisterItemDrop(ModContent.ItemType<Content.Items.Placeables.FusionStation>());

            
            AdjTiles = new int[] { TileID.WorkBenches };
        }

        public override void MouseOver(int i, int j)
        {
            Player player = Main.LocalPlayer;
            player.noThrow = 2;
            player.cursorItemIconEnabled = true;
            player.cursorItemIconID = ModContent.ItemType<Content.Items.Placeables.FusionStation>();
        }

        public override bool RightClick(int i, int j)
        {
            if (Main.netMode == NetmodeID.Server)
                return false;

            
            Vector2 worldPos = new Vector2(i * 16 + 16, j * 16 + 8);
            FusionUISystem.SetStationWorldPosition(worldPos);
            FusionUISystem.ShowFusionUI();
            return true;
        }
    }
}


using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Terraria.DataStructures;
using Terraria.Enums;

namespace WeaponMerging.Content.Tiles
{
    public class FusionRuinsBrick : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileMergeDirt[Type] = true;
            Main.tileBlockLight[Type] = true;
            Main.tileLighted[Type] = true;

            DustType = DustID.Stone;
            HitSound = SoundID.Tink;

            AddMapEntry(new Color(100, 120, 140), CreateMapEntryName());
        }

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            // Subtle glowing effect
            r = 0.3f;
            g = 0.3f;
            b = 0.2f;
        }
    }

    public class FusionAltar : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileNoAttach[Type] = true;
            Main.tileTable[Type] = true;
            Main.tileLighted[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style3x3);
            TileObjectData.newTile.Origin = new Point16(1, 1);
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop, 3, 0);
            TileObjectData.addTile(Type);

            AddMapEntry(new Color(150, 100, 200), CreateMapEntryName());
            DustType = DustID.Stone;
        }

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            // Magical glow
            r = 1.6f;
            g = 1.6f;
            b = 1.4f;
        }

        public override bool RightClick(int i, int j)
        {
            // Could open a fusion recipe menu or spawn items
            return true;
        }
    }

    public class FusionRuinsLight : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileMergeDirt[Type] = true;
            Main.tileBlockLight[Type] = true;
            Main.tileLighted[Type] = true;

            DustType = DustID.Stone;
            HitSound = SoundID.Tink;

            AddMapEntry(new Color(100, 120, 140), CreateMapEntryName());
        }

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            // Subtle glowing effect
            r = 3.0f;
            g = 3.0f;
            b = 2.0f;
        }
    }
}

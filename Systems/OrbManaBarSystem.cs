using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.UI;
using WeaponMerging.Content.Players;

namespace WeaponMerging.Systems
{
    public class OrbManaBarSystem : ModSystem
    {
        private static UserInterface _orbManaInterface;
        private static OrbManaBarUI _orbManaBarUI;

        public override void Load()
        {
            if (!Main.dedServ)
            {
                _orbManaBarUI = new OrbManaBarUI();
                _orbManaBarUI.Activate();
                _orbManaInterface = new UserInterface();
                _orbManaInterface.SetState(_orbManaBarUI);
            }
        }

        public override void ModifyInterfaceLayers(System.Collections.Generic.List<GameInterfaceLayer> layers)
        {
            int manaBarIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Resource Bars"));
            if (manaBarIndex != -1)
            {
                layers.Insert(manaBarIndex + 1, new LegacyGameInterfaceLayer(
                    "WeaponMerging: Orb Mana Bar",
                    delegate
                    {
                        _orbManaInterface?.Draw(Main.spriteBatch, new GameTime());
                        return true;
                    }, InterfaceScaleType.UI));
            }
        }
    }

    public class OrbManaBarUI : UIState
    {
        public override void Draw(SpriteBatch spriteBatch)
        {
            if (Main.playerInventory || Main.LocalPlayer.ghost || Main.LocalPlayer.dead)
                return;

            var player = Main.LocalPlayer.GetModPlayer<OrbManaPlayer>();
            if (player == null)
                return;

            // Position below the mana bar
            Vector2 position = new Vector2(28, 28 + 28); // Adjust based on mana bar position
            Texture2D manaTexture = TextureAssets.Mana.Value;
            Texture2D orbTexture = TextureAssets.Mana.Value;

            // Draw background
            for (int i = 0; i < player.OrbManaMax; i++)
            {
                Vector2 pos = position + new Vector2(i * 28, 0);
                spriteBatch.Draw(manaTexture, pos, null, Color.Gray * 0.8f, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
            }

            // Draw filled orbs
            for (int i = 0; i < player.OrbManaCurrent; i++)
            {
                Vector2 pos = position + new Vector2(i * 28, 0);
                spriteBatch.Draw(orbTexture, pos, null, Color.Cyan, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
            }

            // Draw cooldown overlay if on cooldown
            if (player.IsOnCooldown)
            {
                for (int i = 0; i < player.OrbManaCurrent; i++)
                {
                    Vector2 pos = position + new Vector2(i * 28, 0);
                    float alpha = 0.5f; // Dim when on cooldown
                    spriteBatch.Draw(orbTexture, pos, null, Color.Cyan * alpha, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                }
            }
        }
    }
}

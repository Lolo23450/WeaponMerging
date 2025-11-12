using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.GameContent;
using Terraria.ID;
using System;
using WeaponMerging.Content.Projectiles;

namespace WeaponMerging.Systems
{
    public class FusionUISystem : ModSystem
    {
        private static UserInterface _fusionInterface;
        internal static UI.FusionStationUI FusionUIState;
        private GameTime _lastUpdateUiGameTime;
        internal static Vector2? LastStationWorldPos;
        private static int _animationLockTicks;
        private static Texture2D _proceduralGlow;

        internal static bool IsCraftAnimationActive => _animationLockTicks > 0;

        public override void Load()
        {
            if (!Main.dedServ)
            {
                FusionUIState = new UI.FusionStationUI();
                FusionUIState.Activate();
                _fusionInterface = new UserInterface();
                _fusionInterface.SetState(null);
            }
        }

        public override void UpdateUI(GameTime gameTime)
        {
            _lastUpdateUiGameTime = gameTime;
            if (_fusionInterface?.CurrentState != null)
            {
                _fusionInterface.Update(gameTime);
            }

            if (_animationLockTicks > 0)
            {
                _animationLockTicks--;
            }
        }

        public override void ModifyInterfaceLayers(System.Collections.Generic.List<GameInterfaceLayer> layers)
        {
            int mouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
            if (mouseTextIndex != -1)
            {
                layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer(
                    "WeaponMerging:FusionStationUI",
                    delegate
                    {
                        if (_lastUpdateUiGameTime != null && _fusionInterface?.CurrentState != null)
                        {
                            _fusionInterface.Draw(Main.spriteBatch, _lastUpdateUiGameTime);
                        }
                        return true;
                    }, InterfaceScaleType.UI));

                
                layers.Insert(mouseTextIndex + 1, new LegacyGameInterfaceLayer(
                    "WeaponMerging:FusionCraftOverlay",
                    delegate
                    {
                        DrawFusionCraftOverlay(Main.spriteBatch);
                        return true;
                    }, InterfaceScaleType.None));
            }
        }

        public static void ShowFusionUI()
        {
            if (_fusionInterface == null) return;
            if (IsCraftAnimationActive)
                return;
            _fusionInterface.SetState(FusionUIState);
            Main.playerInventory = false;
            Main.npcChatText = string.Empty;
        }

        public static void HideFusionUI()
        {
            _fusionInterface?.SetState(null);
        }

        internal static void BeginCraftAnimationLock()
        {
            _animationLockTicks = Math.Max(_animationLockTicks, FusionCraftEffect.DurationTicks);
        }

        public static void SetStationWorldPosition(Vector2 worldPosition)
        {
            LastStationWorldPos = worldPosition;
        }

        private static void DrawFusionCraftOverlay(SpriteBatch spriteBatch)
        {
            int effectType = ModContent.ProjectileType<Content.Projectiles.FusionCraftEffect>();
            
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                var pr = Main.projectile[i];
                if (!pr.active || pr.type != effectType)
                    continue;

                
                if (_fusionInterface?.CurrentState != null)
                    HideFusionUI();

                _animationLockTicks = Math.Max(_animationLockTicks, pr.timeLeft);

                
                float durationTicks = FusionCraftEffect.DurationTicks;
                float t = 1f - (pr.timeLeft / durationTicks);
                t = MathHelper.Clamp(t, 0f, 1f);

                
                Texture2D pixel = TextureAssets.MagicPixel.Value;
                var screenRect = new Rectangle(0, 0, Main.screenWidth, Main.screenHeight);
                Color overlayColor;
                if (t < 0.65f)
                {
                    overlayColor = new Color(0, 0, 0, 190);
                }
                else if (t < 0.95f)
                {
                    float transT = MathHelper.Clamp((t - 0.65f) / 0.3f, 0f, 1f);
                    float eased = EaseOutCubic(transT);
                    overlayColor = Color.Lerp(new Color(0, 0, 0, 190), Color.White, eased);
                }
                else
                {
                    overlayColor = Color.White;
                }
                spriteBatch.Draw(pixel, screenRect, overlayColor);

                
                Vector2 worldCenter = pr.Center;
                float rise = MathHelper.SmoothStep(0f, -120f, t);
                Vector2 centerScreen = worldCenter - Main.screenPosition + new Vector2(0f, rise);

                if (_proceduralGlow == null || _proceduralGlow.IsDisposed)
                {
                    _proceduralGlow?.Dispose();
                    _proceduralGlow = GenerateGlowTexture(Main.instance.GraphicsDevice, 384);
                }
                Texture2D glowTex = _proceduralGlow ?? pixel;

                
                float glow = 0.5f + 0.5f * (float)Math.Sin(t * 4.0f);
                float coverage = EaseInOutQuad(MathHelper.Clamp(t / 0.8f, 0f, 1f));
                float glowRadius = MathHelper.Lerp(52f, 138f, coverage);
                float baseScale = glowRadius / (glowTex.Width * 0.5f);
                float fade = 1f - EaseOutCubic(MathHelper.Clamp((t - 0.6f) / 0.05f, 0f, 1f));


                Color innerGlow = new Color(1f, 1f, 1f, 0.95f * fade);
                Color closeGlow = new Color(0.8f, 0.9f, 1f, 0.6f * fade);
                Color midGlow = new Color(0.6f, 0.75f, 1f, 0.4f * fade);
                Color outerGlow = new Color(0.3f, 0.5f, 1f, 0.18f * fade);
                spriteBatch.Draw(glowTex, centerScreen, null, innerGlow, 0f, glowTex.Size() * 0.5f, baseScale, SpriteEffects.None, 0f);
                spriteBatch.Draw(glowTex, centerScreen, null, closeGlow, 0f, glowTex.Size() * 0.5f, baseScale * 1.12f, SpriteEffects.None, 0f);
                spriteBatch.Draw(glowTex, centerScreen, null, midGlow, 0f, glowTex.Size() * 0.5f, baseScale * 1.28f, SpriteEffects.None, 0f);
                spriteBatch.Draw(glowTex, centerScreen, null, outerGlow, 0f, glowTex.Size() * 0.5f, baseScale * 1.55f, SpriteEffects.None, 0f);

                
                spriteBatch.Draw(glowTex, centerScreen, null, new Color(1f, 0.9f, 0.6f, 0.14f * fade), 0f, glowTex.Size() * 0.5f, baseScale * 2.05f, SpriteEffects.None, 0f);

                
                int crafted = (int)pr.ai[0];
                int baseA = (int)pr.ai[1];
                int baseB = (int)pr.localAI[0];

                float timeSec = (FusionCraftEffect.DurationTicks - pr.timeLeft) / 60f;
                float w0 = 0.9f;
                float accel = 2.0f;
                float angle = w0 * timeSec + 0.3f * accel * timeSec * timeSec;
                float radius = MathHelper.Lerp(24f, 64f, MathHelper.Clamp(t / 0.85f, 0f, 1f));

                
                float preludeT = MathHelper.Clamp(t / 0.25f, 0f, 1f);
                float effectiveRadius = MathHelper.Lerp(0f, radius, preludeT);
                float effectiveAngle = MathHelper.Lerp(MathHelper.PiOver4, angle, preludeT);

                
                float fusionT = MathHelper.Clamp((t - 0.6f) / 0.2f, 0f, 1f);
                effectiveRadius = MathHelper.Lerp(effectiveRadius, 0f, fusionT);

                Vector2 offA = new Vector2((float)Math.Cos(effectiveAngle), (float)Math.Sin(effectiveAngle)) * effectiveRadius;
                Vector2 offB = -offA;

                var texA = TextureAssets.Item[baseA].Value;
                var texB = TextureAssets.Item[baseB].Value;
                Vector2 originA = new Vector2(texA.Width, texA.Height) * 0.5f;
                Vector2 originB = new Vector2(texB.Width, texB.Height) * 0.5f;
                if (effectiveRadius >= 2f) 
                {
                    spriteBatch.Draw(texA, centerScreen + offA, null, Color.White, effectiveAngle, originA, 0.7f, SpriteEffects.None, 0f);
                    spriteBatch.Draw(texB, centerScreen + offB, null, Color.White, effectiveAngle + MathHelper.Pi, originB, 0.7f, SpriteEffects.None, 0f);
                }

                
                if (effectiveRadius < 2f)
                {
                    float appearT = MathHelper.Clamp((2f - effectiveRadius) / 2f, 0f, 1f); 
                    float resultScale = MathHelper.Lerp(0.1f, 1.0f, appearT);
                    float resultAlpha = appearT;
                    var craftedTex = TextureAssets.Item[crafted].Value;
                    Vector2 craftedOrigin = new Vector2(craftedTex.Width, craftedTex.Height) * 0.5f;
                    spriteBatch.Draw(craftedTex, centerScreen, null, Color.White * resultAlpha, 0f, craftedOrigin, resultScale, SpriteEffects.None, 0f);
                }

                

                
                fade *= preludeT;
            }
        }

        private static Texture2D GenerateGlowTexture(GraphicsDevice device, int size)
        {
            if (device == null)
                return null;
            var texture = new Texture2D(device, size, size, false, SurfaceFormat.Color);
            Color[] data = new Color[size * size];
            Vector2 center = new Vector2(size - 1, size - 1) * 0.5f;
            float maxRadius = size * 0.5f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int index = y * size + x;
                    float dist = Vector2.Distance(new Vector2(x, y), center);
                    float t = MathHelper.Clamp(1f - (dist / maxRadius), 0f, 1f);
                    float falloff = MathHelper.SmoothStep(0f, 1f, t);
                    float brightness = MathF.Pow(falloff, 1.6f);
                    float alpha = MathF.Pow(falloff, 2.3f);
                    data[index] = new Color(brightness, brightness, brightness, alpha);
                }
            }

            texture.SetData(data);
            return texture;
        }

        private static float EaseInOutQuad(float x)
        {
            return x < 0.5f ? 2f * x * x : 1f - MathF.Pow(-2f * x + 2f, 2f) / 2f;
        }

        private static float EaseOutCubic(float x)
        {
            return 1f - MathF.Pow(1f - x, 3f);
        }

        private static void DrawLine(SpriteBatch spriteBatch, Texture2D tex, Vector2 start, Vector2 end, Color color, float thickness)
        {
            Vector2 edge = end - start;
            float angle = (float)System.Math.Atan2(edge.Y, edge.X);
            float length = edge.Length();
            spriteBatch.Draw(tex, new Rectangle((int)start.X, (int)start.Y, (int)length, (int)thickness), null, color, angle, Vector2.Zero, SpriteEffects.None, 0f);
        }
    }
}


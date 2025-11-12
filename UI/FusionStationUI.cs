using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.GameContent.UI.Elements;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Localization;
using System;
using WeaponMerging.Configs;
using WeaponMerging.Content.Items;
using WeaponMerging.Content.Items.Weapons;
using WeaponMerging.Content.Items.Accessories;

namespace WeaponMerging.UI
{
    public class FusionStationUI : UIState
    {
        private UIPanel panel;
        private UIText titleText;
        private UIElement treeArea;
        private UIText closeText;
        private UIText _infoText;
        private string _currentDescTitle = string.Empty;
        private string _currentDescBody = string.Empty;
        private CostDisplay _costDisplay;
        private UIPanel infoPanel;
        private readonly List<(UIPanel panel, string label, List<(int type,int count)> cost, System.Func<bool> isUnlocked)> _entries
            = new List<(UIPanel, string, List<(int,int)>, System.Func<bool>)>();
        private readonly List<(UIPanel from, UIPanel to)> _links = new();
        private UIPanel _journeyUnlockAllButton;
        private readonly Dictionary<string, UIPanel> _journeySkipButtons = new();

        private Dictionary<string, Vector2> _basePositions = new();
        private Dictionary<string, float> _baseWidths = new();
        private Dictionary<string, float> _baseHeights = new();
        private float _zoom = 1f;

        private Vector2 _panOffset = Vector2.Zero;
        private bool _isDragging = false;
        private Vector2 _lastMousePos;

        private bool JourneyCheatsEnabled => Main.LocalPlayer?.difficulty == PlayerDifficultyID.Creative;

        public override void OnInitialize()
        {
            panel = new UIPanel();
            panel.Width.Set(1200f, 0f);
            panel.Height.Set(800f, 0f);
            panel.HAlign = 0.5f;
            panel.VAlign = 0.5f;
            panel.BackgroundColor = Color.Transparent;
            panel.BorderColor = new Color(75, 85, 120);
            panel.OverflowHidden = true;
            Append(panel);

            panel.OnScrollWheel += OnPanelScrollWheel;

            titleText = new UIText("Fusion Tech Tree");
            titleText.Top.Set(8f, 0f);
            titleText.HAlign = 0.5f;
            panel.Append(titleText);

            treeArea = new UIElement();
            treeArea.Width.Set(-348f, 1f);
            treeArea.Height.Set(-108f, 1f);
            treeArea.Left.Set(24f, 0f);
            treeArea.Top.Set(44f, 0f);
            panel.Append(treeArea);

            infoPanel = new UIPanel();
            infoPanel.Width.Set(300f, 0f);
            infoPanel.Height.Set(-108f, 1f);
            infoPanel.Left.Set(-300f, 1f);
            infoPanel.Top.Set(44f, 0f);
            infoPanel.BackgroundColor = Color.Transparent;
            infoPanel.BorderColor = new Color(75, 85, 120);
            panel.Append(infoPanel);

            var infoText = new UIText("Hover over a node to see requirements");
            infoText.Top.Set(8f, 0f);
            infoText.HAlign = 0.5f;
            infoPanel.Append(infoText);
            _infoText = infoText;

            _costDisplay = new CostDisplay(new List<(int, int)>());
            _costDisplay.Top.Set(320f, 0f);
            _costDisplay.Width.Set(0f, 1f);
            _costDisplay.Height.Set(150f, 0f);
            infoPanel.Append(_costDisplay);

            var nodes = new Dictionary<string, UIPanel>();

            nodes["Starlit Whirlwind"] = CreateNode("Starlit Whirlwind", AttemptUnlock_StarlitWhirlwind,
                new List<(int,int)>{ (ItemID.WandofSparking,1), (ItemID.FallenStar,5), (ItemID.CopperBar,10), (ModContent.ItemType<OrbFragment>(),5) },
                () => FP != null && FP.Unlocked_StarlitWhirlwind,
                new Vector2(300, 50));

            nodes["Crystal Cascade"] = CreateNode("Crystal Cascade", AttemptUnlock_CrystalCascade,
                new List<(int,int)>{ (ItemID.IceBlade,1), (2745,10), (ItemID.Sapphire,10), (ItemID.Snowball,25), (ModContent.ItemType<OrbFragment>(),5) },
                () => FP != null && FP.Unlocked_CrystalCascade,
                new Vector2(200, 150));

            nodes["Pain Spiral"] = CreateNode("Pain Spiral", AttemptUnlock_PainSpiral,
                new List<(int,int)>{ (ItemID.BallOHurt,1), (ItemID.ThornChakram,1), (ItemID.Stinger,4), (ItemID.JungleSpores,8), (ModContent.ItemType<OrbFragment>(),5) },
                () => FP != null && FP.Unlocked_PainSpiral,
                new Vector2(400, 150));

            nodes["Shurikenwood Bow"] = CreateNode("Shurikenwood Bow", AttemptUnlock_ShurikenwoodBow,
                new List<(int,int)>{ (ItemID.WoodenBow,1), (ItemID.Shuriken,50), (ItemID.Wood,20) },
                () => FP != null && FP.Unlocked_ShurikenwoodBow,
                new Vector2(400, -50));

            nodes["Frost Shurikenwood Bow"] = CreateNode("Frost Shurikenwood Bow", AttemptUnlock_FrostShurikenwoodBow,
                new List<(int,int)>{ (ModContent.ItemType<ShurikenwoodBow>(),1), (ItemID.IceBlock,10), (ItemID.Shiverthorn,5) },
                () => FP != null && FP.Unlocked_FrostShurikenwoodBow,
                new Vector2(550, -50));

            nodes["Aurora Bow"] = CreateNode("Aurora Bow", AttemptUnlock_AuroraBow,
                new List<(int,int)>{ (ModContent.ItemType<FrostShurikenwoodBow>(),1), (ItemID.IceBlade,1), (ItemID.Diamond,10) },
                () => FP != null && FP.Unlocked_AuroraBow,
                new Vector2(700, -50));

            nodes["Venom Barrage"] = CreateNode("Venom Barrage", AttemptUnlock_VenomBarrage,
                new List<(int,int)>{ (ItemID.ThrowingKnife,100), (ItemID.Blowpipe,1), (ItemID.Stinger,5), (ItemID.JungleSpores,7) },
                () => FP != null && FP.Unlocked_VenomBarrage,
                new Vector2(550, 150));

            nodes["Inferno Orb"] = CreateNode("Inferno Orb", AttemptUnlock_InfernoOrb,
                new List<(int,int)>{ (ItemID.FlowerofFire,1), (121,1), (ItemID.HellstoneBar,15), (2348,2), (ModContent.ItemType<OrbFragment>(),5) },
                () => FP != null && FP.Unlocked_InfernoOrb,
                new Vector2(300, 250));

            nodes["Fossil Dancer"] = CreateNode("Fossil Dancer", AttemptUnlock_FossilDancer,
                new List<(int,int)>{ (ItemID.BoneSword,1), (ItemID.AntlionClaw,1), (ItemID.FossilOre,8), (ItemID.AntlionMandible,4) },
                () => FP != null && FP.Unlocked_FossilDancer,
                new Vector2(400, 350));

            nodes["Celestial Frost Blade"] = CreateNode("Celestial Frost Blade", AttemptUnlock_CelestialFrost,
                new List<(int,int)>{ (ItemID.Starfury,1), (ItemID.IceBlade,1), (ItemID.MeteoriteBar,10), (ItemID.Ruby,7) },
                () => FP != null && FP.Unlocked_CelestialFrostBlade,
                new Vector2(200, 450));

            nodes["Nightfall Harbinger"] = CreateNode("Nightfall Harbinger", AttemptUnlock_NightfallHarbinger,
                new List<(int,int)>{ (ItemID.DemonBow,1), (ItemID.DemonScythe,1), (ItemID.SoulofNight,10), (47,5) },
                () => FP != null && FP.Unlocked_NightfallHarbinger,
                new Vector2(100, 50));

            nodes["Verdant Conduit Tome"] = CreateNode("Verdant Conduit Tome", AttemptUnlock_VerdantConduitTome,
                new List<(int,int)>{ (ItemID.SpellTome,1), (3,25), (ItemID.JungleSpores,7), (ItemID.Vine,7), (ItemID.SoulofLight,5) },
                () => FP != null && FP.Unlocked_VerdantConduitTome,
                new Vector2(100, 350));

            nodes["Shark Cannon"] = CreateNode("Shark Cannon", AttemptUnlock_SharkCannon,
                new List<(int,int)>{ (ItemID.Boomstick,1), (ItemID.Minishark,1), (ItemID.SharkFin,5), (ItemID.IllegalGunParts,1) },
                () => FP != null && FP.Unlocked_SharkCannon,
                new Vector2(500, 550));

            nodes["Gale Crescent"] = CreateNode("Gale Crescent", AttemptUnlock_GaleCrescent,
                new List<(int,int)>{ (ItemID.DemonScythe,1), (ItemID.Cloud,25), (ItemID.Feather,12), (ItemID.SoulofNight,5) },
                () => FP != null && FP.Unlocked_GaleCrescent,
                new Vector2(300, 550));

            nodes["Shadow Reaper"] = CreateNode("Shadow Reaper", AttemptUnlock_ShadowReaper,
                new List<(int,int)>{ (ItemID.ShadowbeamStaff,1), (ItemID.DeathSickle,1), (ItemID.Ectoplasm,10), (ItemID.SoulofNight,15), (ModContent.ItemType<OrbFragment>(),5) },
                () => FP != null && FP.Unlocked_ShadowReaper,
                new Vector2(400, 650));

            nodes["Solar Conductor"] = CreateNode("Solar Conductor", AttemptUnlock_SolarConductor,
                new List<(int,int)>{ (ItemID.Flamelash,1), (ItemID.FlowerofFire,1), (ItemID.SoulofLight,5), (ItemID.SoulofNight,5) },
                () => FP != null && FP.Unlocked_SolarConductor,
                new Vector2(100, 550));

            nodes["Abyssal Shark Cannon"] = CreateNode("Abyssal Shark Cannon", AttemptUnlock_AbyssalSharkCannon,
                new List<(int,int)>{ (ModContent.ItemType<SharkCannon>(),1), (ItemID.LunarBar,10), (ItemID.FragmentVortex,15), (ItemID.SharkFin,5) },
                () => FP != null && FP.Unlocked_AbyssalSharkCannon,
                new Vector2(600, 550));

            nodes["Orb Master Band"] = CreateNode("Orb Master Band", AttemptUnlock_OrbMasterBand,
                new List<(int,int)>{ (ModContent.ItemType<Content.Items.Accessories.OrbweaverBand>(),1), (ModContent.ItemType<Content.Items.Accessories.OrbCatalystCore>(),1), (ItemID.SoulofLight,10), (ItemID.SoulofNight,10) },
                () => FP != null && FP.Unlocked_OrbMasterBand,
                new Vector2(550, 350));

            nodes["Focused Persistence Crystal"] = CreateNode("Focused Persistence Crystal", AttemptUnlock_FocusedPersistenceCrystal,
                new List<(int,int)>{ (ModContent.ItemType<Content.Items.Accessories.FocusCrystal>(),1), (ModContent.ItemType<Content.Items.Accessories.OrbPersistenceCharm>(),1), (ItemID.CrystalShard,20), (ItemID.FallenStar,5) },
                () => FP != null && FP.Unlocked_FocusedPersistenceCrystal,
                new Vector2(100, 150));

            nodes["Combo Catalyst Charm"] = CreateNode("Combo Catalyst Charm", AttemptUnlock_ComboCatalystCharm,
                new List<(int,int)>{ (ModContent.ItemType<Content.Items.Accessories.AccelerantCharm>(),1), (ModContent.ItemType<Content.Items.Accessories.OrbCatalystCore>(),1), (ItemID.Bone, 20), (ItemID.SoulofLight,5) },
                () => FP != null && FP.Unlocked_ComboCatalystCharm,
                new Vector2(650, 250));

            Link(nodes["Starlit Whirlwind"], nodes["Crystal Cascade"]);
            Link(nodes["Starlit Whirlwind"], nodes["Pain Spiral"]);
            Link(nodes["Starlit Whirlwind"], nodes["Shurikenwood Bow"]);
            Link(nodes["Shurikenwood Bow"], nodes["Frost Shurikenwood Bow"]);
            Link(nodes["Frost Shurikenwood Bow"], nodes["Aurora Bow"]);
            Link(nodes["Pain Spiral"], nodes["Venom Barrage"]);
            Link(nodes["Crystal Cascade"], nodes["Inferno Orb"]);
            Link(nodes["Pain Spiral"], nodes["Inferno Orb"]);
            Link(nodes["Inferno Orb"], nodes["Fossil Dancer"]);
            Link(nodes["Inferno Orb"], nodes["Celestial Frost Blade"]);
            Link(nodes["Crystal Cascade"], nodes["Nightfall Harbinger"]);
            Link(nodes["Celestial Frost Blade"], nodes["Verdant Conduit Tome"]);
            Link(nodes["Fossil Dancer"], nodes["Gale Crescent"]);
            Link(nodes["Celestial Frost Blade"], nodes["Gale Crescent"]);
            Link(nodes["Fossil Dancer"], nodes["Shark Cannon"]);
            Link(nodes["Gale Crescent"], nodes["Shadow Reaper"]);
            Link(nodes["Celestial Frost Blade"], nodes["Solar Conductor"]);
            Link(nodes["Shark Cannon"], nodes["Abyssal Shark Cannon"]);
            Link(nodes["Shark Cannon"], nodes["Shadow Reaper"]);

            Link(nodes["Combo Catalyst Charm"], nodes["Orb Master Band"]);
            Link(nodes["Crystal Cascade"], nodes["Focused Persistence Crystal"]);
            Link(nodes["Venom Barrage"], nodes["Combo Catalyst Charm"]);

            var closeBtn = new UIPanel();
            closeBtn.Width.Set(60f, 0f);
            closeBtn.Height.Set(24f, 0f);
            closeBtn.Top.Set(8f, 0f);
            closeBtn.Left.Set(1000f, 0f);
            closeBtn.OnLeftClick += (_, __) => Systems.FusionUISystem.HideFusionUI();
            panel.Append(closeBtn);
            closeText = new UIText("Close");
            closeText.HAlign = 0.5f;
            closeText.VAlign = 0.5f;
            closeText.IgnoresMouseInteraction = true;
            closeBtn.Append(closeText);

            _journeyUnlockAllButton = new UIPanel();
            _journeyUnlockAllButton.Width.Set(80f, 0f);
            _journeyUnlockAllButton.Height.Set(24f, 0f);
            _journeyUnlockAllButton.Top.Set(8f, 0f);
            _journeyUnlockAllButton.Left.Set(1000f - 10f - 80f, 0f);
            _journeyUnlockAllButton.OnLeftClick += UnlockAll;
            var unlockAllText = new UIText("Unlock All");
            unlockAllText.HAlign = 0.5f;
            unlockAllText.VAlign = 0.5f;
            unlockAllText.IgnoresMouseInteraction = true;
            _journeyUnlockAllButton.Append(unlockAllText);

            EnsureJourneyControls();
        }

        public override void OnActivate()
        {
            base.OnActivate();
            EnsureJourneyControls();
        }

        private void OnPanelScrollWheel(UIScrollWheelEvent evt, UIElement listeningElement)
        {
            float oldZoom = _zoom;
            _zoom = MathHelper.Clamp(_zoom + evt.ScrollWheelValue * 0.001f, 0.5f, 2f);
            float newZoom = _zoom;

            var panelDims = panel.GetDimensions();
            Vector2 center = new Vector2(panelDims.X + panelDims.Width / 2, panelDims.Y + panelDims.Height / 2);
            Vector2 mouseRel = Main.MouseScreen - center;
            float zoomFactor = newZoom / oldZoom;
            _panOffset += mouseRel * (1 - zoomFactor);

            UpdateZoomPositions();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (Main.mouseRight && panel.ContainsPoint(Main.MouseScreen))
            {
                if (!_isDragging)
                {
                    _isDragging = true;
                    _lastMousePos = Main.MouseScreen;
                }

                Vector2 delta = Main.MouseScreen - _lastMousePos;
                _panOffset += delta;
                _lastMousePos = Main.MouseScreen;
                UpdateZoomPositions();
            }
            else
            {
                _isDragging = false;
            }
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), new Color(18, 20, 31) * 0.95f);

            UpdateZoomPositions();

            base.DrawSelf(spriteBatch);
            EnsureJourneyControls();

            if (_entries == null || _entries.Count == 0)
                return;

            foreach (var idx in _entries)
            {
                if (idx.panel == null || idx.isUnlocked == null)
                    continue;

                bool unlocked = false;
                try { unlocked = idx.isUnlocked?.Invoke() ?? false; } catch { unlocked = false; }
                bool prereqsMet = PrereqsMet(idx.panel);
                bool affordable = idx.cost != null && HasItems(idx.cost);

                if (unlocked)
                    idx.panel.BackgroundColor = new Color(40,140,60) * 0.7f;
                else if (!prereqsMet)
                    idx.panel.BackgroundColor = new Color(80, 80, 80) * 0.7f;
                else if (affordable)
                    idx.panel.BackgroundColor = new Color(63, 82, 151) * 0.7f;
                else
                    idx.panel.BackgroundColor = new Color(140, 60, 40) * 0.7f;
            }

            var panelDims = panel.GetDimensions();
            if (_links != null && _links.Count > 0)
            {
                var pixel = TextureAssets.MagicPixel.Value;
                foreach (var (from, to) in _links)
                {
                    if (from == null || to == null)
                        continue;

                    Vector2 a = GetCenter(from);
                    Vector2 b = GetCenter(to);

                    if (a.X < panelDims.X || a.X > panelDims.X + panelDims.Width || a.Y < panelDims.Y || a.Y > panelDims.Y + panelDims.Height ||
                        b.X < panelDims.X || b.X > panelDims.X + panelDims.Width || b.Y < panelDims.Y || b.Y > panelDims.Y + panelDims.Height)
                        continue;

                    var (toUnlocked, toPrereqs, toAffordable) = GetEntryState(to);
                    Color lineCol = new Color(180, 180, 180);
                    bool hasParticles = false;
                    if (toUnlocked)
                    {
                        lineCol = new Color(150, 255, 180);
                        hasParticles = !ModContent.GetInstance<FusionStationConfig>().DisableAnimation;
                    }
                    else if (toPrereqs && toAffordable)
                    {
                        lineCol = new Color(180, 220, 255);
                    }
                    else if (toPrereqs)
                    {
                        lineCol = new Color(255, 200, 150);
                    }

                    DrawLine(spriteBatch, pixel, a, b, lineCol * 0.3f, 10f);
                    DrawLine(spriteBatch, pixel, a, b, lineCol * 0.8f, 6f);

                    if (hasParticles)
                    {
                        Vector2 dir = b - a;
                        float length = dir.Length();
                        if (length > 0f)
                        {
                            dir.Normalize();
                            float time = (float)Main.GameUpdateCount * 0.03f;
                            for (int p = 0; p < 4; p++)
                            {
                                float tParticle = (time + p * 0.25f) % 1f;
                                Vector2 particlePos = a + dir * (length * tParticle);
                                if (particlePos.X < panelDims.X || particlePos.X > panelDims.X + panelDims.Width ||
                                    particlePos.Y < panelDims.Y || particlePos.Y > panelDims.Y + panelDims.Height)
                                    continue;
                                spriteBatch.Draw(pixel, new Rectangle((int)particlePos.X - 3, (int)particlePos.Y - 3, 6, 6), Color.White * 0.6f);
                            }
                        }
                    }
                }
            }

            foreach (var idx in _entries)
            {
                if (idx.panel == null)
                    continue;

                bool unlocked = false;
                try { unlocked = idx.isUnlocked?.Invoke() ?? false; } catch { unlocked = false; }

                string display = unlocked ? $"{idx.label} [Unlocked]" : idx.label;
                var dims = idx.panel.GetDimensions();
                Vector2 textPos = new Vector2(dims.X + dims.Width / 2, dims.Y + dims.Height + 10);
                
                if (textPos.X < panelDims.X || textPos.X > panelDims.X + panelDims.Width ||
                    textPos.Y < panelDims.Y || textPos.Y > panelDims.Y + panelDims.Height)
                    continue;
                Utils.DrawBorderString(spriteBatch, display, textPos, Color.White, _zoom * 0.6f, 0.5f, 0.5f);
            }

            if (!string.IsNullOrEmpty(_currentDescTitle))
            {
                var infoDims = infoPanel.GetDimensions();
                Vector2 titlePos = new Vector2(infoDims.X + infoDims.Width / 2, infoDims.Y + 60f);
                Utils.DrawBorderString(spriteBatch, _currentDescTitle, titlePos, Color.White, 0.8f, 0.5f, 0.5f);
                Vector2 bodyPos = new Vector2(infoDims.X + 8f, infoDims.Y + 84f);
                string[] lines = Utils.WordwrapString(_currentDescBody, FontAssets.MouseText.Value, 400, 10, out int lineCount);
                for (int i = 0; i < lineCount; i++)
                {
                    string line = lines[i];
                    bool isLast = i == lineCount - 1;
                    if (isLast || lineCount == 1)
                    {
                        Utils.DrawBorderString(spriteBatch, line, new Vector2(bodyPos.X, bodyPos.Y + i * 22f), Color.White, 0.7f, 0f, 0f);
                    }
                    else
                    {
                        string[] words = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                        if (words.Length <= 1)
                        {
                            Utils.DrawBorderString(spriteBatch, line, new Vector2(bodyPos.X, bodyPos.Y + i * 22f), Color.White, 0.7f, 0f, 0f);
                            continue;
                        }
                        float totalWordWidth = 0;
                        foreach (string word in words)
                        {
                            totalWordWidth += FontAssets.MouseText.Value.MeasureString(word).X * 0.7f;
                        }
                        float spaceWidth = FontAssets.MouseText.Value.MeasureString(" ").X * 0.7f;
                        float availableWidth = infoDims.Width - 16f;
                        float extraSpace = availableWidth - totalWordWidth - spaceWidth * (words.Length - 1);
                        float additionalSpace = extraSpace / (words.Length - 1);
                        additionalSpace = Math.Min(additionalSpace, spaceWidth * 0.6f);
                        float currentX = bodyPos.X;
                        for (int j = 0; j < words.Length; j++)
                        {
                            Utils.DrawBorderString(spriteBatch, words[j], new Vector2(currentX, bodyPos.Y + i * 22f), Color.White, 0.7f, 0f, 0f);
                            currentX += FontAssets.MouseText.Value.MeasureString(words[j]).X * 0.7f;
                            if (j < words.Length - 1)
                            {
                                currentX += spaceWidth + additionalSpace;
                            }
                        }
                    }
                }
            }

            
            {
                var pixel = TextureAssets.MagicPixel.Value;
                var pd = panel.GetDimensions();
                float y = pd.Y + 36f;
                DrawLine(spriteBatch, pixel, new Vector2(pd.X + 16f, y), new Vector2(pd.X + pd.Width - 16f, y), new Color(255, 255, 255) * 0.15f, 2f);
            }

            

        }

        private UIPanel CreateNode(string label, UIElement.MouseEvent onClick, List<(int type, int count)> cost, System.Func<bool> isUnlocked, Vector2 pos)
        {
            _basePositions[label] = pos;
            _baseWidths[label] = 60f;
            _baseHeights[label] = 60f;
            var button = new UIPanel();
            button.Width.Set(_baseWidths[label] * _zoom, 0f);
            button.Height.Set(_baseHeights[label] * _zoom, 0f);
            button.Left.Set(pos.X * _zoom, 0f);
            button.Top.Set(pos.Y * _zoom, 0f);
            button.OnLeftClick += onClick;
            button.BorderColor = new Color(90, 100, 140);
            treeArea.Append(button);

            
            int craftedType = GetItemTypeFromLabel(label);
            var icon = new IconElement(craftedType);
            button.Append(icon);

            

            

            
            var skipBtn = new UIPanel();
            skipBtn.Width.Set(35f, 0f);
            skipBtn.Height.Set(12f, 0f);
            skipBtn.Top.Set(0f, 0f);
            skipBtn.Left.Set(80f - 35f, 0f);
            skipBtn.OnLeftClick += (evt, el) => SkipUnlock(label);
            var skipText = new UIText("Skip");
            skipText.HAlign = 0.5f;
            skipText.VAlign = 0.5f;
            skipText.IgnoresMouseInteraction = true;
            skipBtn.Append(skipText);
            skipBtn.IgnoresMouseInteraction = true;
            _journeySkipButtons[label] = skipBtn;

            
            button.OnMouseOver += (_, __) =>
            {
                button.BorderColor = new Color(255, 230, 120);
                UpdateInfoPanel(label, cost);
            };
            button.OnMouseOut += (_, __) =>
            {
                button.BorderColor = new Color(90, 100, 140);
                _currentDescTitle = string.Empty;
                _currentDescBody = string.Empty;
                _costDisplay.SetCost(new List<(int, int)>());
            };

            _entries.Add((button, label, cost, isUnlocked));

            return button;
        }

        private void UpdateInfoPanel(string label, List<(int type, int count)> cost)
        {
            _costDisplay.SetCost(cost);

            int type = GetItemTypeFromLabel(label);
            if (type > 0)
            {
                string name = Lang.GetItemNameValue(type);
                string desc = ItemLoader.GetItem(type)?.Tooltip?.Value ?? string.Empty;
                _currentDescTitle = name;
                _currentDescBody = desc;
            }
            else
            {
                _currentDescTitle = string.Empty;
                _currentDescBody = string.Empty;
            }
        }

        private void UpdateZoomPositions()
        {
            foreach (var entry in _entries)
            {
                if (_basePositions.TryGetValue(entry.label, out var basePos))
                {
                    entry.panel.Left.Set(basePos.X * _zoom + _panOffset.X, 0f);
                    entry.panel.Top.Set(basePos.Y * _zoom + _panOffset.Y, 0f);
                }
                if (_baseWidths.TryGetValue(entry.label, out var baseWidth))
                {
                    entry.panel.Width.Set(baseWidth * _zoom, 0f);
                }
                if (_baseHeights.TryGetValue(entry.label, out var baseHeight))
                {
                    entry.panel.Height.Set(baseHeight * _zoom, 0f);
                }
            }
            
            foreach (var kvp in _journeySkipButtons)
            {
                var skipBtn = kvp.Value;
                skipBtn.Width.Set(35f * _zoom, 0f);
                skipBtn.Height.Set(12f * _zoom, 0f);
                skipBtn.Top.Set(0f * _zoom, 0f);
                skipBtn.Left.Set((80f - 35f) * _zoom, 0f);
            }
        }

        private Player P => Main.LocalPlayer;
        private Systems.FusionUnlockPlayer FP => P.GetModPlayer<Systems.FusionUnlockPlayer>();

        private void Link(UIPanel from, UIPanel to)
        {
            _links.Add((from, to));
        }

        private static Vector2 GetCenter(UIElement e)
        {
            var dims = e.GetDimensions();
            return new Vector2(dims.Center().X, dims.Center().Y);
        }

        private static void DrawLine(SpriteBatch spriteBatch, Texture2D tex, Vector2 start, Vector2 end, Color color, float thickness)
        {
            Vector2 edge = end - start;
            float angle = (float)System.Math.Atan2(edge.Y, edge.X);
            float length = edge.Length();
            spriteBatch.Draw(tex, new Rectangle((int)start.X, (int)start.Y, (int)length, (int)thickness), null, color, angle, Vector2.Zero, SpriteEffects.None, 0f);
        }

        private (bool unlocked, bool prereqsMet, bool affordable) GetEntryState(UIPanel panel)
        {
            foreach (var e in _entries)
            {
                if (e.panel == panel)
                {
                    bool unlocked = false;
                    try { unlocked = e.isUnlocked?.Invoke() ?? false; } catch { unlocked = false; }
                    bool prereqs = PrereqsMet(panel);
                    bool affordable = HasItems(e.cost);
                    return (unlocked, prereqs, affordable);
                }
            }
            return (false, false, false);
        }

        private class IconElement : UIElement
        {
            private int _craftedType;
            private Texture2D _texture;
            public IconElement(int craftedType)
            {
                _craftedType = craftedType;
                Width.Set(48f, 0f);
                Height.Set(48f, 0f);
                HAlign = 0.5f;
                VAlign = 0.5f;
            }
            protected override void DrawSelf(SpriteBatch spriteBatch)
            {
                var dims = GetDimensions();
                if (_texture == null)
                {
                    
                    _texture = (_craftedType > 0 && _craftedType < TextureAssets.Item.Length && TextureAssets.Item[_craftedType] != null) ? TextureAssets.Item[_craftedType].Value : TextureAssets.MagicPixel.Value;
                }
                if (_texture == null || _texture == TextureAssets.MagicPixel.Value)
                {
                    
                    spriteBatch.Draw(TextureAssets.MagicPixel.Value, dims.ToRectangle(), Color.Red);
                }
                else
                {
                    int frameCount = Main.itemAnimations[_craftedType]?.FrameCount ?? 1;
                    Rectangle sourceRect = new Rectangle(0, 0, _texture.Width, _texture.Height / frameCount);
                    float scale = 48f / Math.Max(_texture.Width, _texture.Height / frameCount);
                    spriteBatch.Draw(_texture, dims.Center(), sourceRect, Color.White, 0f, sourceRect.Size() / 2, scale, SpriteEffects.None, 0f);
                }
            }
        }

        private class CostDisplay : UIElement
        {
            private List<(int type, int count)> _cost;
            public CostDisplay(List<(int type, int count)> cost)
            {
                _cost = cost;
            }

            public void SetCost(List<(int type, int count)> newCost)
            {
                _cost = newCost;
            }
            protected override void DrawSelf(SpriteBatch spriteBatch)
            {
                var dims = GetDimensions();
                float startY = dims.Y + 8f;
                float lineHeight = 24f;
                float iconSize = 24f;
                float textOffset = iconSize + 8f;

                
                Utils.DrawBorderString(spriteBatch, "Requirements:", new Vector2(dims.X + 8f, startY), Color.White, 0.8f);
                startY += lineHeight + 4f;

                foreach (var (type, count) in _cost)
                {
                    var tex = TextureAssets.Item[type].Value;
                    int frameCount = Main.itemAnimations[type]?.FrameCount ?? 1;
                    Rectangle sourceRect = new Rectangle(0, 0, tex.Width, tex.Height / frameCount);
                    int owned = CountOwned(type);
                    bool ok = owned >= count;
                    Color tint = ok ? Color.White : Color.White * 0.35f;

                    
                    float scale = iconSize / Math.Max(tex.Width, tex.Height / frameCount);
                    spriteBatch.Draw(tex, new Vector2(dims.X + 8f, startY), sourceRect, tint, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);

                    
                    string itemName = Lang.GetItemNameValue(type);
                    string countText = $"{itemName} {owned}/{count}";
                    Color textCol = ok ? new Color(140, 240, 140) : new Color(240, 130, 120);
                    Utils.DrawBorderString(spriteBatch, countText, new Vector2(dims.X + 8f + textOffset, startY + 2f), textCol, 0.7f);

                    startY += lineHeight;
                }
            }
            private static int CountOwned(int type)
            {
                var player = Main.LocalPlayer;
                int owned = 0;
                for (int i = 0; i < 58; i++)
                {
                    Item it = player.inventory[i];
                    if (it != null && it.type == type)
                        owned += it.stack;
                }
                return owned;
            }
        }

        private bool PrereqsMet(UIPanel node)
        {
            
            foreach (var (from, to) in _links)
            {
                if (to == node)
                {
                    
                    foreach (var e in _entries)
                    {
                        if (e.panel == from)
                        {
                            bool ok = false;
                            try { ok = e.isUnlocked?.Invoke() ?? false; } catch { ok = false; }
                            if (!ok) return false;
                            break;
                        }
                    }
                }
            }
            return true;
        }

        private bool HasItems(List<(int type, int count)> cost)
        {
            foreach (var (type, count) in cost)
            {
                int owned = 0;
                for (int i = 0; i < 58; i++)
                {
                    Item it = P.inventory[i];
                    if (it != null && it.type == type)
                        owned += it.stack;
                }
                if (owned < count) return false;
            }
            return true;
        }

        private UIPanel GetPanelByLabel(string label)
        {
            foreach (var e in _entries)
            {
                if (e.label == label)
                    return e.panel;
            }
            return null;
        }

        private void ConsumeItems(List<(int type, int count)> cost)
        {
            foreach (var (type, countReq) in cost)
            {
                int remaining = countReq;
                for (int i = 0; i < 58 && remaining > 0; i++)
                {
                    Item it = P.inventory[i];
                    if (it != null && it.type == type && it.stack > 0)
                    {
                        int take = it.stack >= remaining ? remaining : it.stack;
                        it.stack -= take;
                        if (it.stack <= 0)
                            it.TurnToAir();
                        remaining -= take;
                    }
                }
            }
        }

        private void GrantItem(int type)
        {
            if (Main.myPlayer == P.whoAmI)
            {
                P.QuickSpawnItem(P.GetSource_GiftOrReward(), type);
            }
        }

        private void ShowRejected(string reason)
        {
            CombatText.NewText(P.getRect(), Color.OrangeRed, reason);
            SoundEngine.PlaySound(SoundID.MenuClose);
            if (reason == "Missing materials")
            {
                Systems.FusionUISystem.HideFusionUI();
            }
        }

        private void ShowAccepted(string text)
        {
            CombatText.NewText(P.getRect(), Color.LightGreen, text);
            SoundEngine.PlaySound(SoundID.Research);
        }

        private void AttemptUnlock_InfernoOrb(UIMouseEvent evt, UIElement listeningElement)
        {
            if (FP.Unlocked_InfernoOrb) { ShowRejected("Already unlocked"); return; }
            var node = GetPanelByLabel("Inferno Orb");
            if (node != null && !PrereqsMet(node)) { ShowRejected("Missing prerequisites"); return; }
            var cost = new List<(int,int)> { (ItemID.FlowerofFire,1), (121,1), (ItemID.HellstoneBar,15), (2348,2), (ModContent.ItemType<OrbFragment>(),5) };
            if (!HasItems(cost)) { ShowRejected("Missing materials"); return; }
            ConsumeItems(cost);
            FP.Unlocked_InfernoOrb = true;
            ShowAccepted("Inferno Orb unlocked");
            GrantItem(ModContent.ItemType<InfernoOrb>());
            if (!ModContent.GetInstance<FusionStationConfig>().DisableAnimation)
                StartCraftAnimation(ModContent.ItemType<InfernoOrb>(), ItemID.FlowerofFire, ItemID.MeteorStaff);
        }

        private void AttemptUnlock_ShadowReaper(UIMouseEvent evt, UIElement listeningElement)
        {
            if (FP.Unlocked_ShadowReaper) { ShowRejected("Already unlocked"); return; }
            var node = GetPanelByLabel("Shadow Reaper");
            if (node != null && !PrereqsMet(node)) { ShowRejected("Missing prerequisites"); return; }
            var cost = new List<(int,int)> { (ItemID.ShadowbeamStaff,1), (ItemID.DeathSickle,1), (ItemID.Ectoplasm,10), (ItemID.SoulofNight,15), (ModContent.ItemType<OrbFragment>(),5) };
            if (!HasItems(cost)) { ShowRejected("Missing materials"); return; }
            ConsumeItems(cost);
            FP.Unlocked_ShadowReaper = true;
            ShowAccepted("Shadow Reaper unlocked");
            GrantItem(ModContent.ItemType<ShadowReaper>());
            if (!ModContent.GetInstance<FusionStationConfig>().DisableAnimation)
                StartCraftAnimation(ModContent.ItemType<ShadowReaper>(), ItemID.ShadowbeamStaff, ItemID.DeathSickle);
        }

        private void AttemptUnlock_CelestialFrost(UIMouseEvent evt, UIElement listeningElement)
        {
            if (FP.Unlocked_CelestialFrostBlade) { ShowRejected("Already unlocked"); return; }
            var node = GetPanelByLabel("Celestial Frost Blade");
            if (node != null && !PrereqsMet(node)) { ShowRejected("Missing prerequisites"); return; }
            var cost = new List<(int,int)> { (ItemID.Starfury,1), (ItemID.IceBlade,1), (ItemID.MeteoriteBar,10), (ItemID.Ruby,7) };
            if (!HasItems(cost)) { ShowRejected("Missing materials"); return; }
            ConsumeItems(cost);
            FP.Unlocked_CelestialFrostBlade = true;
            ShowAccepted("Celestial Frost Blade unlocked");
            GrantItem(ModContent.ItemType<CelestialFrostBlade>());
            if (!ModContent.GetInstance<FusionStationConfig>().DisableAnimation)
                StartCraftAnimation(ModContent.ItemType<CelestialFrostBlade>(), ItemID.Starfury, ItemID.IceBlade);
        }

        private void AttemptUnlock_PainSpiral(UIMouseEvent evt, UIElement listeningElement)
        {
            if (FP.Unlocked_PainSpiral) { ShowRejected("Already unlocked"); return; }
            var node = GetPanelByLabel("Pain Spiral");
            if (node != null && !PrereqsMet(node)) { ShowRejected("Missing prerequisites"); return; }
            var cost = new List<(int,int)> { (ItemID.BallOHurt,1), (ItemID.ThornChakram,1), (ItemID.Stinger,4), (ItemID.JungleSpores,8), (ModContent.ItemType<OrbFragment>(),5) };
            if (!HasItems(cost)) { ShowRejected("Missing materials"); return; }
            ConsumeItems(cost);
            FP.Unlocked_PainSpiral = true;
            ShowAccepted("Pain Spiral unlocked");
            GrantItem(ModContent.ItemType<PainSpiral>());
            if (!ModContent.GetInstance<FusionStationConfig>().DisableAnimation)
                StartCraftAnimation(ModContent.ItemType<PainSpiral>(), ItemID.BallOHurt, ItemID.ThornChakram);
        }

        private void AttemptUnlock_SolarConductor(UIMouseEvent evt, UIElement listeningElement)
        {
            if (FP.Unlocked_SolarConductor) { ShowRejected("Already unlocked"); return; }
            var node = GetPanelByLabel("Solar Conductor");
            if (node != null && !PrereqsMet(node)) { ShowRejected("Missing prerequisites"); return; }
            var cost = new List<(int,int)> { (ItemID.Flamelash,1), (ItemID.FlowerofFire,1), (ItemID.SoulofLight,5), (ItemID.SoulofNight,5) };
            if (!HasItems(cost)) { ShowRejected("Missing materials"); return; }
            ConsumeItems(cost);
            FP.Unlocked_SolarConductor = true;
            ShowAccepted("Solar Conductor unlocked");
            GrantItem(ModContent.ItemType<SolarConductor>());
            if (!ModContent.GetInstance<FusionStationConfig>().DisableAnimation)
                StartCraftAnimation(ModContent.ItemType<SolarConductor>(), ItemID.Flamelash, ItemID.FlowerofFire);
        }

        private void AttemptUnlock_CrystalCascade(UIMouseEvent evt, UIElement listeningElement)
        {
            if (FP.Unlocked_CrystalCascade) { ShowRejected("Already unlocked"); return; }
            var node = GetPanelByLabel("Crystal Cascade");
            if (node != null && !PrereqsMet(node)) { ShowRejected("Missing prerequisites"); return; }
            var cost = new List<(int,int)> { (ItemID.IceBlade,1), (2745,10), (ItemID.Sapphire,10), (ItemID.Snowball,25), (ModContent.ItemType<OrbFragment>(),5) };
            if (!HasItems(cost)) { ShowRejected("Missing materials"); return; }
            ConsumeItems(cost);
            FP.Unlocked_CrystalCascade = true;
            ShowAccepted("Crystal Cascade unlocked");
            GrantItem(ModContent.ItemType<CrystalCascade>());
            if (!ModContent.GetInstance<FusionStationConfig>().DisableAnimation)
                StartCraftAnimation(ModContent.ItemType<CrystalCascade>(), ItemID.IceBlade, 1306);
        }

        private void AttemptUnlock_FossilDancer(UIMouseEvent evt, UIElement listeningElement)
        {
            if (FP.Unlocked_FossilDancer) { ShowRejected("Already unlocked"); return; }
            var node = GetPanelByLabel("Fossil Dancer");
            if (node != null && !PrereqsMet(node)) { ShowRejected("Missing prerequisites"); return; }
            var cost = new List<(int,int)> { (ItemID.BoneSword,1), (ItemID.AntlionClaw,1), (ItemID.FossilOre,8), (ItemID.AntlionMandible,4) };
            if (!HasItems(cost)) { ShowRejected("Missing materials"); return; }
            ConsumeItems(cost);
            FP.Unlocked_FossilDancer = true;
            ShowAccepted("Fossil Dancer unlocked");
            GrantItem(ModContent.ItemType<Content.Items.Weapons.FossilDancer>());
            if (!ModContent.GetInstance<FusionStationConfig>().DisableAnimation)
                StartCraftAnimation(ModContent.ItemType<Content.Items.Weapons.FossilDancer>(), ItemID.BoneSword, ItemID.AntlionClaw);
        }

        private void AttemptUnlock_GaleCrescent(UIMouseEvent evt, UIElement listeningElement)
        {
            if (FP.Unlocked_GaleCrescent) { ShowRejected("Already unlocked"); return; }
            var node = GetPanelByLabel("Gale Crescent");
            if (node != null && !PrereqsMet(node)) { ShowRejected("Missing prerequisites"); return; }
            var cost = new List<(int,int)> { (ItemID.DemonScythe,1), (ItemID.Cloud,25), (ItemID.Feather,12), (ItemID.SoulofNight,5) };
            if (!HasItems(cost)) { ShowRejected("Missing materials"); return; }
            ConsumeItems(cost);
            FP.Unlocked_GaleCrescent = true;
            ShowAccepted("Gale Crescent unlocked");
            GrantItem(ModContent.ItemType<Content.Items.Weapons.GaleCrescent>());
            if (!ModContent.GetInstance<FusionStationConfig>().DisableAnimation)
                StartCraftAnimation(ModContent.ItemType<Content.Items.Weapons.GaleCrescent>(), ItemID.BloodButcherer, ItemID.Cloud);
        }

        private void AttemptUnlock_StarlitWhirlwind(UIMouseEvent evt, UIElement listeningElement)
        {
            if (FP.Unlocked_StarlitWhirlwind) { ShowRejected("Already unlocked"); return; }
            var node = GetPanelByLabel("Starlit Whirlwind");
            if (node != null && !PrereqsMet(node)) { ShowRejected("Missing prerequisites"); return; }
            var cost = new List<(int,int)> { (ItemID.WandofSparking,1), (ItemID.FallenStar,5), (ItemID.CopperBar,10), (ModContent.ItemType<OrbFragment>(),5) };
            if (!HasItems(cost)) { ShowRejected("Missing materials"); return; }
            ConsumeItems(cost);
            FP.Unlocked_StarlitWhirlwind = true;
            ShowAccepted("Starlit Whirlwind unlocked");
            GrantItem(ModContent.ItemType<Content.Items.Weapons.StarlitWhirlwind>());
            if (!ModContent.GetInstance<FusionStationConfig>().DisableAnimation)
                StartCraftAnimation(ModContent.ItemType<Content.Items.Weapons.StarlitWhirlwind>(), ItemID.WandofSparking, ItemID.FallenStar);
        }

        private void AttemptUnlock_OrbMasterBand(UIMouseEvent evt, UIElement listeningElement)
        {
            if (FP.Unlocked_OrbMasterBand) { ShowRejected("Already unlocked"); return; }
            var node = GetPanelByLabel("Orb Master Band");
            if (node != null && !PrereqsMet(node)) { ShowRejected("Missing prerequisites"); return; }
            var cost = new List<(int,int)> { (ModContent.ItemType<Content.Items.Accessories.OrbweaverBand>(),1), (ModContent.ItemType<Content.Items.Accessories.OrbCatalystCore>(),1), (ItemID.SoulofLight,10), (ItemID.SoulofNight,10) };
            if (!HasItems(cost)) { ShowRejected("Missing materials"); return; }
            ConsumeItems(cost);
            FP.Unlocked_OrbMasterBand = true;
            ShowAccepted("Orb Master Band unlocked");
            GrantItem(ModContent.ItemType<Content.Items.Accessories.OrbMasterBand>());
            
        }

        private void AttemptUnlock_FocusedPersistenceCrystal(UIMouseEvent evt, UIElement listeningElement)
        {
            if (FP.Unlocked_FocusedPersistenceCrystal) { ShowRejected("Already unlocked"); return; }
            var node = GetPanelByLabel("Focused Persistence Crystal");
            if (node != null && !PrereqsMet(node)) { ShowRejected("Missing prerequisites"); return; }
            var cost = new List<(int,int)> { (ModContent.ItemType<Content.Items.Accessories.FocusCrystal>(),1), (ModContent.ItemType<Content.Items.Accessories.OrbPersistenceCharm>(),1), (ItemID.CrystalShard,20), (ItemID.FallenStar,5) };
            if (!HasItems(cost)) { ShowRejected("Missing materials"); return; }
            ConsumeItems(cost);
            FP.Unlocked_FocusedPersistenceCrystal = true;
            ShowAccepted("Focused Persistence Crystal unlocked");
            GrantItem(ModContent.ItemType<Content.Items.Accessories.FocusedPersistenceCrystal>());
        }

        private void AttemptUnlock_ComboCatalystCharm(UIMouseEvent evt, UIElement listeningElement)
        {
            if (FP.Unlocked_ComboCatalystCharm) { ShowRejected("Already unlocked"); return; }
            var node = GetPanelByLabel("Combo Catalyst Charm");
            if (node != null && !PrereqsMet(node)) { ShowRejected("Missing prerequisites"); return; }
            var cost = new List<(int,int)> { (ModContent.ItemType<Content.Items.Accessories.AccelerantCharm>(),1), (ModContent.ItemType<Content.Items.Accessories.OrbCatalystCore>(),1), (ItemID.Bone, 20), (ItemID.SoulofLight,5) };
            if (!HasItems(cost)) { ShowRejected("Missing materials"); return; }
            ConsumeItems(cost);
            FP.Unlocked_ComboCatalystCharm = true;
            ShowAccepted("Combo Catalyst Charm unlocked");
            GrantItem(ModContent.ItemType<Content.Items.Accessories.ComboCatalystCharm>());
        }

        private void AttemptUnlock_ShurikenwoodBow(UIMouseEvent evt, UIElement listeningElement)
        {
            if (FP.Unlocked_ShurikenwoodBow) { ShowRejected("Already unlocked"); return; }
            var node = GetPanelByLabel("Shurikenwood Bow");
            if (node != null && !PrereqsMet(node)) { ShowRejected("Missing prerequisites"); return; }
            var cost = new List<(int,int)> { (ItemID.WoodenBow,1), (ItemID.Shuriken,50), (ItemID.Wood,20) };
            if (!HasItems(cost)) { ShowRejected("Missing materials"); return; }
            ConsumeItems(cost);
            FP.Unlocked_ShurikenwoodBow = true;
            ShowAccepted("Shurikenwood Bow unlocked");
            GrantItem(ModContent.ItemType<Content.Items.Weapons.ShurikenwoodBow>());
            if (!ModContent.GetInstance<FusionStationConfig>().DisableAnimation)
                StartCraftAnimation(ModContent.ItemType<Content.Items.Weapons.ShurikenwoodBow>(), ItemID.WoodenBow, ItemID.Shuriken);
        }

        private void AttemptUnlock_FrostShurikenwoodBow(UIMouseEvent evt, UIElement listeningElement)
        {
            if (FP.Unlocked_FrostShurikenwoodBow) { ShowRejected("Already unlocked"); return; }
            var node = GetPanelByLabel("Frost Shurikenwood Bow");
            if (node != null && !PrereqsMet(node)) { ShowRejected("Missing prerequisites"); return; }
            var cost = new List<(int,int)> { (ModContent.ItemType<Content.Items.Weapons.ShurikenwoodBow>(),1), (ItemID.IceBlock,10), (ItemID.Shiverthorn,5) };
            if (!HasItems(cost)) { ShowRejected("Missing materials"); return; }
            ConsumeItems(cost);
            FP.Unlocked_FrostShurikenwoodBow = true;
            ShowAccepted("Frost Shurikenwood Bow unlocked");
            GrantItem(ModContent.ItemType<Content.Items.Weapons.FrostShurikenwoodBow>());
            if (!ModContent.GetInstance<FusionStationConfig>().DisableAnimation)
                StartCraftAnimation(ModContent.ItemType<Content.Items.Weapons.FrostShurikenwoodBow>(), ModContent.ItemType<Content.Items.Weapons.ShurikenwoodBow>(), ItemID.IceBlock);
        }

        private void AttemptUnlock_VenomBarrage(UIMouseEvent evt, UIElement listeningElement)
        {
            if (FP.Unlocked_VenomBarrage) { ShowRejected("Already unlocked"); return; }
            var node = GetPanelByLabel("Venom Barrage");
            if (node != null && !PrereqsMet(node)) { ShowRejected("Missing prerequisites"); return; }
            var cost = new List<(int,int)> { (ItemID.ThrowingKnife,100), (ItemID.Blowpipe,1), (ItemID.Stinger,15), (ItemID.JungleSpores,12) };
            if (!HasItems(cost)) { ShowRejected("Missing materials"); return; }
            ConsumeItems(cost);
            FP.Unlocked_VenomBarrage = true;
            ShowAccepted("Venom Barrage unlocked");
            GrantItem(ModContent.ItemType<Content.Items.Weapons.VenomBarrage>());
            if (!ModContent.GetInstance<FusionStationConfig>().DisableAnimation)
                StartCraftAnimation(ModContent.ItemType<Content.Items.Weapons.VenomBarrage>(), ItemID.ThrowingKnife, ItemID.Blowpipe);
        }

        private void AttemptUnlock_NightfallHarbinger(UIMouseEvent evt, UIElement listeningElement)
        {
            if (FP.Unlocked_NightfallHarbinger) { ShowRejected("Already unlocked"); return; }
            var node = GetPanelByLabel("Nightfall Harbinger");
            if (node != null && !PrereqsMet(node)) { ShowRejected("Missing prerequisites"); return; }
            var cost = new List<(int,int)> { (ItemID.DemonBow,1), (ItemID.DemonScythe,1), (ItemID.SoulofNight,10), (47,5) };
            if (!HasItems(cost)) { ShowRejected("Missing materials"); return; }
            ConsumeItems(cost);
            FP.Unlocked_NightfallHarbinger = true;
            ShowAccepted("Nightfall Harbinger unlocked");
            GrantItem(ModContent.ItemType<Content.Items.Weapons.NightfallHarbinger>());
            if (!ModContent.GetInstance<FusionStationConfig>().DisableAnimation)
                StartCraftAnimation(ModContent.ItemType<Content.Items.Weapons.NightfallHarbinger>(), ItemID.DemonBow, ItemID.DemonScythe);
        }

        private void AttemptUnlock_VerdantConduitTome(UIMouseEvent evt, UIElement listeningElement)
        {
            if (FP.Unlocked_VerdantConduitTome) { ShowRejected("Already unlocked"); return; }
            var node = GetPanelByLabel("Verdant Conduit Tome");
            if (node != null && !PrereqsMet(node)) { ShowRejected("Missing prerequisites"); return; }
            var cost = new List<(int,int)> { (ItemID.SpellTome,1), (3,25), (ItemID.JungleSpores,7), (ItemID.Vine,7), (ItemID.SoulofLight,5) };
            if (!HasItems(cost)) { ShowRejected("Missing materials"); return; }
            ConsumeItems(cost);
            FP.Unlocked_VerdantConduitTome = true;
            ShowAccepted("Verdant Conduit Tome unlocked");
            GrantItem(ModContent.ItemType<Content.Items.Weapons.VerdantConduitTome>());
            if (!ModContent.GetInstance<FusionStationConfig>().DisableAnimation)
                StartCraftAnimation(ModContent.ItemType<Content.Items.Weapons.VerdantConduitTome>(), ItemID.SpellTome, ItemID.CrystalShard);
        }

        private void AttemptUnlock_SharkCannon(UIMouseEvent evt, UIElement listeningElement)
        {
            if (FP.Unlocked_SharkCannon) { ShowRejected("Already unlocked"); return; }
            var node = GetPanelByLabel("Shark Cannon");
            if (node != null && !PrereqsMet(node)) { ShowRejected("Missing prerequisites"); return; }
            var cost = new List<(int,int)> { (ItemID.Boomstick,1), (ItemID.Minishark,1), (ItemID.SharkFin,5), (ItemID.IllegalGunParts,1) };
            if (!HasItems(cost)) { ShowRejected("Missing materials"); return; }
            ConsumeItems(cost);
            FP.Unlocked_SharkCannon = true;
            ShowAccepted("Shark Cannon unlocked");
            GrantItem(ModContent.ItemType<Content.Items.Weapons.SharkCannon>());
            if (!ModContent.GetInstance<FusionStationConfig>().DisableAnimation)
                StartCraftAnimation(ModContent.ItemType<Content.Items.Weapons.SharkCannon>(), ItemID.Boomstick, ItemID.Minishark);
        }

        private void AttemptUnlock_AbyssalSharkCannon(UIMouseEvent evt, UIElement listeningElement)
        {
            if (FP.Unlocked_AbyssalSharkCannon) { ShowRejected("Already unlocked"); return; }
            var node = GetPanelByLabel("Abyssal Shark Cannon");
            if (node != null && !PrereqsMet(node)) { ShowRejected("Missing prerequisites"); return; }
            var cost = new List<(int,int)> { (ModContent.ItemType<SharkCannon>(),1), (ItemID.LunarBar,10), (ItemID.FragmentVortex,15), (ItemID.SharkFin,5) };
            if (!HasItems(cost)) { ShowRejected("Missing materials"); return; }
            ConsumeItems(cost);
            FP.Unlocked_AbyssalSharkCannon = true;
            ShowAccepted("Abyssal Shark Cannon unlocked");
            GrantItem(ModContent.ItemType<Content.Items.Weapons.AbyssalSharkCannon>());
            if (!ModContent.GetInstance<FusionStationConfig>().DisableAnimation)
                StartCraftAnimation(ModContent.ItemType<Content.Items.Weapons.AbyssalSharkCannon>(), ModContent.ItemType<Content.Items.Weapons.SharkCannon>(), ItemID.LunarBar);
        }

        private void AttemptUnlock_AuroraBow(UIMouseEvent evt, UIElement listeningElement)
        {
            if (FP.Unlocked_AuroraBow) { ShowRejected("Already unlocked"); return; }
            var node = GetPanelByLabel("Aurora Bow");
            if (node != null && !PrereqsMet(node)) { ShowRejected("Missing prerequisites"); return; }
            var cost = new List<(int,int)> { (ModContent.ItemType<FrostShurikenwoodBow>(),1), (ItemID.IceBlade,1), (ItemID.Diamond,10) };
            if (!HasItems(cost)) { ShowRejected("Missing materials"); return; }
            ConsumeItems(cost);
            FP.Unlocked_AuroraBow = true;
            ShowAccepted("Aurora Bow unlocked");
            GrantItem(ModContent.ItemType<Content.Items.Weapons.AuroraBow>());
            if (!ModContent.GetInstance<FusionStationConfig>().DisableAnimation)
                StartCraftAnimation(ModContent.ItemType<Content.Items.Weapons.AuroraBow>(), ItemID.DaedalusStormbow, ItemID.RainbowRod);
        }

        private void StartCraftAnimation(int crafted, int baseA, int baseB)
        {
            Systems.FusionUISystem.BeginCraftAnimationLock();
            Systems.FusionUISystem.HideFusionUI();
            Vector2 pos = Systems.FusionUISystem.LastStationWorldPos ?? P.Center;
            if (Main.myPlayer == P.whoAmI)
            {
                var src = P.GetSource_Misc("FusionCraft");
                int type = ModContent.ProjectileType<Content.Projectiles.FusionCraftEffect>();
                int idx = Projectile.NewProjectile(src, pos, Vector2.Zero, type, 0, 0f, P.whoAmI, crafted, baseA);
                if (idx >= 0 && idx < Main.maxProjectiles)
                {
                    Main.projectile[idx].localAI[0] = baseB;
                }
            }
        }

        private void UnlockAll(UIMouseEvent evt, UIElement listeningElement)
        {
            FP.Unlocked_InfernoOrb = true;
            FP.Unlocked_ShadowReaper = true;
            FP.Unlocked_CelestialFrostBlade = true;
            FP.Unlocked_PainSpiral = true;
            FP.Unlocked_SolarConductor = true;
            FP.Unlocked_CrystalCascade = true;
            FP.Unlocked_FossilDancer = true;
            FP.Unlocked_GaleCrescent = true;
            FP.Unlocked_StarlitWhirlwind = true;
            FP.Unlocked_OrbMasterBand = true;
            FP.Unlocked_FocusedPersistenceCrystal = true;
            FP.Unlocked_ComboCatalystCharm = true;
            FP.Unlocked_ShurikenwoodBow = true;
            FP.Unlocked_FrostShurikenwoodBow = true;
            FP.Unlocked_VenomBarrage = true;
            FP.Unlocked_NightfallHarbinger = true;
            FP.Unlocked_VerdantConduitTome = true;
            FP.Unlocked_SharkCannon = true;
            FP.Unlocked_AbyssalSharkCannon = true;
            FP.Unlocked_AuroraBow = true;
            GrantItem(ModContent.ItemType<InfernoOrb>());
            GrantItem(ModContent.ItemType<ShadowReaper>());
            GrantItem(ModContent.ItemType<CelestialFrostBlade>());
            GrantItem(ModContent.ItemType<PainSpiral>());
            GrantItem(ModContent.ItemType<SolarConductor>());
            GrantItem(ModContent.ItemType<CrystalCascade>());
            GrantItem(ModContent.ItemType<FossilDancer>());
            GrantItem(ModContent.ItemType<GaleCrescent>());
            GrantItem(ModContent.ItemType<StarlitWhirlwind>());
            GrantItem(ModContent.ItemType<OrbMasterBand>());
            GrantItem(ModContent.ItemType<FocusedPersistenceCrystal>());
            GrantItem(ModContent.ItemType<ComboCatalystCharm>());
            GrantItem(ModContent.ItemType<ShurikenwoodBow>());
            GrantItem(ModContent.ItemType<FrostShurikenwoodBow>());
            GrantItem(ModContent.ItemType<VenomBarrage>());
            GrantItem(ModContent.ItemType<NightfallHarbinger>());
            GrantItem(ModContent.ItemType<VerdantConduitTome>());
            GrantItem(ModContent.ItemType<SharkCannon>());
            GrantItem(ModContent.ItemType<AbyssalSharkCannon>());
            GrantItem(ModContent.ItemType<AuroraBow>());
            ShowAccepted("All fusions unlocked");
        }

        private void SkipUnlock(string label)
        {
            if (!JourneyCheatsEnabled)
                return;

            bool unlocked = label switch
            {
                "Inferno Orb" => FP.Unlocked_InfernoOrb,
                "Shadow Reaper" => FP.Unlocked_ShadowReaper,
                "Celestial Frost Blade" => FP.Unlocked_CelestialFrostBlade,
                "Pain Spiral" => FP.Unlocked_PainSpiral,
                "Solar Conductor" => FP.Unlocked_SolarConductor,
                "Crystal Cascade" => FP.Unlocked_CrystalCascade,
                "Fossil Dancer" => FP.Unlocked_FossilDancer,
                "Gale Crescent" => FP.Unlocked_GaleCrescent,
                "Starlit Whirlwind" => FP.Unlocked_StarlitWhirlwind,
                "Orb Master Band" => FP.Unlocked_OrbMasterBand,
                "Focused Persistence Crystal" => FP.Unlocked_FocusedPersistenceCrystal,
                "Combo Catalyst Charm" => FP.Unlocked_ComboCatalystCharm,
                "Shurikenwood Bow" => FP.Unlocked_ShurikenwoodBow,
                "Frost Shurikenwood Bow" => FP.Unlocked_FrostShurikenwoodBow,
                "Venom Barrage" => FP.Unlocked_VenomBarrage,
                "Nightfall Harbinger" => FP.Unlocked_NightfallHarbinger,
                "Verdant Conduit Tome" => FP.Unlocked_VerdantConduitTome,
                "Shark Cannon" => FP.Unlocked_SharkCannon,
                "Abyssal Shark Cannon" => FP.Unlocked_AbyssalSharkCannon,
                "Aurora Bow" => FP.Unlocked_AuroraBow,
                _ => true
            };

            if (unlocked)
                return;

            switch (label)
            {
                case "Inferno Orb": FP.Unlocked_InfernoOrb = true; GrantItem(ModContent.ItemType<InfernoOrb>()); break;
                case "Shadow Reaper": FP.Unlocked_ShadowReaper = true; GrantItem(ModContent.ItemType<ShadowReaper>()); break;
                case "Celestial Frost Blade": FP.Unlocked_CelestialFrostBlade = true; GrantItem(ModContent.ItemType<CelestialFrostBlade>()); break;
                case "Pain Spiral": FP.Unlocked_PainSpiral = true; GrantItem(ModContent.ItemType<PainSpiral>()); break;
                case "Solar Conductor": FP.Unlocked_SolarConductor = true; GrantItem(ModContent.ItemType<SolarConductor>()); break;
                case "Crystal Cascade": FP.Unlocked_CrystalCascade = true; GrantItem(ModContent.ItemType<CrystalCascade>()); break;
                case "Fossil Dancer": FP.Unlocked_FossilDancer = true; GrantItem(ModContent.ItemType<FossilDancer>()); break;
                case "Gale Crescent": FP.Unlocked_GaleCrescent = true; GrantItem(ModContent.ItemType<GaleCrescent>()); break;
                case "Starlit Whirlwind": FP.Unlocked_StarlitWhirlwind = true; GrantItem(ModContent.ItemType<StarlitWhirlwind>()); break;
                case "Orb Master Band": FP.Unlocked_OrbMasterBand = true; GrantItem(ModContent.ItemType<OrbMasterBand>()); break;
                case "Focused Persistence Crystal": FP.Unlocked_FocusedPersistenceCrystal = true; GrantItem(ModContent.ItemType<FocusedPersistenceCrystal>()); break;
                case "Combo Catalyst Charm": FP.Unlocked_ComboCatalystCharm = true; GrantItem(ModContent.ItemType<ComboCatalystCharm>()); break;
                case "Shurikenwood Bow": FP.Unlocked_ShurikenwoodBow = true; GrantItem(ModContent.ItemType<ShurikenwoodBow>()); break;
                case "Frost Shurikenwood Bow": FP.Unlocked_FrostShurikenwoodBow = true; GrantItem(ModContent.ItemType<FrostShurikenwoodBow>()); break;
                case "Venom Barrage": FP.Unlocked_VenomBarrage = true; GrantItem(ModContent.ItemType<VenomBarrage>()); break;
                case "Nightfall Harbinger": FP.Unlocked_NightfallHarbinger = true; GrantItem(ModContent.ItemType<NightfallHarbinger>()); break;
                case "Verdant Conduit Tome": FP.Unlocked_VerdantConduitTome = true; GrantItem(ModContent.ItemType<VerdantConduitTome>()); break;
                case "Shark Cannon": FP.Unlocked_SharkCannon = true; GrantItem(ModContent.ItemType<SharkCannon>()); break;
                case "Abyssal Shark Cannon": FP.Unlocked_AbyssalSharkCannon = true; GrantItem(ModContent.ItemType<AbyssalSharkCannon>()); break;
                case "Aurora Bow": FP.Unlocked_AuroraBow = true; GrantItem(ModContent.ItemType<AuroraBow>()); break;
                default: return;
            }

            ShowAccepted(label + " unlocked");
            RefreshEntryStates();
            PlayCraftAnimationForLabel(label);
        }

        private void RefreshEntryStates()
        {
            foreach (var idx in _entries)
            {
                if (idx.panel == null)
                    continue;

                bool unlocked = false;
                try { unlocked = idx.isUnlocked?.Invoke() ?? false; } catch { unlocked = false; }
                bool prereqsMet = PrereqsMet(idx.panel);
                bool affordable = HasItems(idx.cost);

                if (unlocked)
                    idx.panel.BackgroundColor = new Color(40,140,60) * 0.7f;
                else if (!prereqsMet)
                    idx.panel.BackgroundColor = new Color(80, 80, 80) * 0.7f;
                else if (affordable)
                    idx.panel.BackgroundColor = new Color(63, 82, 151) * 0.7f;
                else
                    idx.panel.BackgroundColor = new Color(140, 60, 40) * 0.7f;
            }
        }

        private void PlayCraftAnimationForLabel(string label)
        {
            if (ModContent.GetInstance<FusionStationConfig>().DisableAnimation)
                return;
            var recipe = GetRecipeData(label);
            if (recipe.HasValue)
            {
                var (crafted, baseA, baseB) = recipe.Value;
                StartCraftAnimation(crafted, baseA, baseB);
            }
        }

        private static (int crafted, int baseA, int baseB)? GetRecipeData(string label) => label switch
        {
            "Inferno Orb" => (ModContent.ItemType<Content.Items.Weapons.InfernoOrb>(), ItemID.FlowerofFire, ItemID.MeteorStaff),
            "Shadow Reaper" => (ModContent.ItemType<Content.Items.Weapons.ShadowReaper>(), ItemID.ShadowbeamStaff, ItemID.DeathSickle),
            "Celestial Frost Blade" => (ModContent.ItemType<Content.Items.Weapons.CelestialFrostBlade>(), ItemID.Starfury, ItemID.IceBlade),
            "Pain Spiral" => (ModContent.ItemType<Content.Items.Weapons.PainSpiral>(), ItemID.BallOHurt, ItemID.ThornChakram),
            "Solar Conductor" => (ModContent.ItemType<Content.Items.Weapons.SolarConductor>(), ItemID.Flamelash, ItemID.FlowerofFire),
            "Crystal Cascade" => (ModContent.ItemType<Content.Items.Weapons.CrystalCascade>(), ItemID.IceBlade, 1306),
            "Fossil Dancer" => (ModContent.ItemType<Content.Items.Weapons.FossilDancer>(), ItemID.BoneSword, ItemID.AntlionClaw),
            "Gale Crescent" => (ModContent.ItemType<Content.Items.Weapons.GaleCrescent>(), ItemID.BloodButcherer, ItemID.Cloud),
            "Starlit Whirlwind" => (ModContent.ItemType<Content.Items.Weapons.StarlitWhirlwind>(), ItemID.WandofSparking, ItemID.FallenStar),
            _ => null
        };

        private static int GetItemTypeFromLabel(string label) => label switch
        {
            "Inferno Orb" => ModContent.ItemType<InfernoOrb>(),
            "Shadow Reaper" => ModContent.ItemType<ShadowReaper>(),
            "Celestial Frost Blade" => ModContent.ItemType<CelestialFrostBlade>(),
            "Pain Spiral" => ModContent.ItemType<PainSpiral>(),
            "Solar Conductor" => ModContent.ItemType<SolarConductor>(),
            "Crystal Cascade" => ModContent.ItemType<CrystalCascade>(),
            "Fossil Dancer" => ModContent.ItemType<FossilDancer>(),
            "Gale Crescent" => ModContent.ItemType<GaleCrescent>(),
            "Starlit Whirlwind" => ModContent.ItemType<StarlitWhirlwind>(),
            "Orb Master Band" => ModContent.ItemType<OrbMasterBand>(),
            "Focused Persistence Crystal" => ModContent.ItemType<FocusedPersistenceCrystal>(),
            "Combo Catalyst Charm" => ModContent.ItemType<ComboCatalystCharm>(),
            "Shurikenwood Bow" => ModContent.ItemType<ShurikenwoodBow>(),
            "Frost Shurikenwood Bow" => ModContent.ItemType<FrostShurikenwoodBow>(),
            "Venom Barrage" => ModContent.ItemType<VenomBarrage>(),
            "Nightfall Harbinger" => ModContent.ItemType<NightfallHarbinger>(),
            "Verdant Conduit Tome" => ModContent.ItemType<VerdantConduitTome>(),
            "Shark Cannon" => ModContent.ItemType<SharkCannon>(),
            "Abyssal Shark Cannon" => ModContent.ItemType<AbyssalSharkCannon>(),
            "Aurora Bow" => ModContent.ItemType<AuroraBow>(),
            _ => ItemID.None
        };

        private static string GetTexturePathFromLabel(string label) => label switch
        {
            "Inferno Orb" => "Content/Items/Weapons/InfernoOrb",
            "Shadow Reaper" => "Content/Items/Weapons/ShadowReaper",
            "Celestial Frost Blade" => "Content/Items/Weapons/CelestialFrostBlade",
            "Pain Spiral" => "Content/Items/Weapons/PainSpiral",
            "Solar Conductor" => "Content/Items/Weapons/SolarConductor",
            "Crystal Cascade" => "Content/Items/Weapons/CrystalCascade",
            "Fossil Dancer" => "Content/Items/Weapons/FossilDancer",
            "Gale Crescent" => "Content/Items/Weapons/GaleCrescent",
            "Starlit Whirlwind" => "Content/Items/Weapons/StarlitWhirlwind",
            _ => ""
        };

        private void EnsureJourneyControls()
        {
            bool isJourney = JourneyCheatsEnabled;

            if (_journeyUnlockAllButton != null)
            {
                if (isJourney)
                {
                    if (_journeyUnlockAllButton.Parent != panel)
                    {
                        _journeyUnlockAllButton.Remove();
                        panel.Append(_journeyUnlockAllButton);
                    }
                    _journeyUnlockAllButton.IgnoresMouseInteraction = false;
                }
                else if (_journeyUnlockAllButton.Parent != null)
                {
                    _journeyUnlockAllButton.Remove();
                }
            }

            foreach (var kvp in _journeySkipButtons)
            {
                string label = kvp.Key;
                var skipPanel = kvp.Value;
                var nodePanel = GetPanelByLabel(label);
                if (skipPanel == null || nodePanel == null)
                    continue;

                if (isJourney)
                {
                    if (skipPanel.Parent != nodePanel)
                    {
                        skipPanel.Remove();
                        nodePanel.Append(skipPanel);
                        skipPanel.BackgroundColor = new Color(60, 80, 120) * 0.9f;
                        skipPanel.BorderColor = new Color(200, 220, 255);
                    }
                    skipPanel.IgnoresMouseInteraction = false;
                }
                else if (skipPanel.Parent != null)
                {
                    skipPanel.Remove();
                }
            }
        }

    }
}


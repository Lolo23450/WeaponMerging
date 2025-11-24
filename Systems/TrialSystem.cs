using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.UI;
using Terraria.GameContent;
using Terraria.GameInput;
using System;
using System.Collections.Generic;
using System.Linq;
using SubworldLibrary;

namespace WeaponMerging.Systems
{
    // Trial Difficulty Levels
    public enum TrialDifficulty
    {
        Normal,
        Hard,
        Expert,
        Legendary
    }

    // Trial Modifiers that players can choose
    public enum TrialModifier
    {
        None,
        SlipperyFloor,     // Increased friction, harder movement control
        GravityShift,      // Random gravity changes
        ToxicArena,        // Poison clouds spawn periodically
        SpeedBoost,        // Player moves faster but takes more damage
        DefenseReduction,  // Player defense reduced but damage increased
        NoHealing,         // No potions/healing items allowed
        DoubleMinions,     // Boss spawns twice as many minions
        RapidAttacks,      // Boss attacks much more frequently
        InvisibleBoss,     // Boss becomes partially invisible
        RandomTeleports,   // Player randomly teleports during fight
        TimePressure,      // Timer counts down faster
        LowVisibility,     // Arena becomes dark with fog
        KnockbackMaster,   // Boss has increased knockback
        ProjectileStorm,   // Constant projectile barrage
        BossRegen,         // Boss regenerates health over time
        PlayerWeakness     // Player takes increased damage
    }

    // Player's trial statistics
    public class TrialStats
    {
        public int TotalTrialsCompleted { get; set; }
        public int CurrentStreak { get; set; }
        public int HighestDifficulty { get; set; }
        public Dictionary<string, int> BossBestTimes { get; set; } // Boss name -> best completion time
        public Dictionary<TrialModifier, int> ModifierCompletions { get; set; }
        public List<string> UnlockedTitles { get; set; }
        public int TrialTokens { get; set; }

        public TrialStats()
        {
            BossBestTimes = new Dictionary<string, int>();
            ModifierCompletions = new Dictionary<TrialModifier, int>();
            UnlockedTitles = new List<string>();
        }
    }

    public class TrialSystem : ModSystem
    {
        public static bool IsTrialActive { get; private set; } = false;
        public static string CurrentTrial { get; private set; }
        public static TrialDifficulty CurrentDifficulty { get; private set; } = TrialDifficulty.Normal;
        public static List<TrialModifier> ActiveModifiers { get; private set; } = new List<TrialModifier>();
        public static Vector2 OriginalPlayerPosition { get; private set; }
        public static List<Item> OriginalPlayerInventory { get; private set; }
        public static int TrialNPC { get; private set; } = -1;
        public static long TrialStartTime { get; private set; }
        public static int TrialTimeLimit { get; private set; }

        // Save main world time state
        private static double _originalTime;
        private static bool _originalDayTime;

        private static int _trialStartDelay = 0; // Prevent instant completion
        private static int _bossDefeatDelay = 0; // Buffer delay for defeat detection

        // Trial UI state
        private static bool _showModifierSelection = false;
        private static TrialModifier[] _availableModifiers = new TrialModifier[3];
        public static bool IsModifierSelectionActive => _showModifierSelection;

        public override void UpdateUI(GameTime gameTime)
        {
            if (IsTrialActive)
            {
                if (_trialStartDelay > 0)
                {
                    _trialStartDelay--;
                    // Spawn boss when delay reaches 30 (0.5 seconds after entering)
                    if (_trialStartDelay == 30 && TrialNPC == -1)
                    {
                        SpawnTrialBoss(CurrentTrial);
                    }
                }
                else
                {
                    // Update trial timer
                    if (TrialStartTime > 0)
                    {
                        long elapsedFrames = (long)Main.GameUpdateCount - TrialStartTime;
                        int elapsedSeconds = (int)(elapsedFrames / 60L);

                        // Check time limit
                        if (elapsedSeconds >= TrialTimeLimit)
                        {
                            Main.NewText("Time's up! Trial failed.", Color.Red);
                            EndTrial(Main.LocalPlayer, false);
                            return;
                        }

                        // Apply modifier effects that need updating
                        UpdateModifierEffects(Main.LocalPlayer);
                    }

                    CheckTrialCompletion();
                }

                CheckPlayerEscape();
                KeepPlayerInArena();
            }
        }

        private static void DrawTrialUI()
        {
            if (TrialStartTime <= 0) return;

            long elapsedFrames = (long)Main.GameUpdateCount - TrialStartTime;
            if (elapsedFrames < 0)
            {
                elapsedFrames = 0;
            }

            int elapsedSeconds = (int)(elapsedFrames / 60L);
            int remainingSeconds = TrialTimeLimit - elapsedSeconds;

            // Format time display
            int minutes = remainingSeconds / 60;
            int seconds = remainingSeconds % 60;
            string timeText = $"{minutes}:{seconds:D2}";

            // Draw time remaining
            Vector2 timePos = new Vector2(20, 80);
            Utils.DrawBorderString(Main.spriteBatch, $"Time: {timeText}", timePos, Color.White);

            // Draw difficulty and modifiers
            Vector2 difficultyPos = new Vector2(20, 100);
            Utils.DrawBorderString(Main.spriteBatch, $"Difficulty: {CurrentDifficulty}", difficultyPos, GetDifficultyColor(CurrentDifficulty));

            if (ActiveModifiers.Count > 0)
            {
                Vector2 modifierPos = new Vector2(20, 120);
                Utils.DrawBorderString(Main.spriteBatch, $"Modifiers: {ActiveModifiers.Count}", modifierPos, Color.Yellow);
            }

            // Draw boss health if available
            if (TrialNPC >= 0 && TrialNPC < Main.maxNPCs)
            {
                NPC boss = Main.npc[TrialNPC];
                if (boss.active)
                {
                    Vector2 healthPos = new Vector2(20, 140);
                    float healthPercent = (float)boss.life / boss.lifeMax;
                    Color healthColor = healthPercent > 0.5f ? Color.Green : healthPercent > 0.25f ? Color.Yellow : Color.Red;
                    Utils.DrawBorderString(Main.spriteBatch, $"Boss HP: {boss.life}/{boss.lifeMax}", healthPos, healthColor);
                }
            }
        }

        private static Color GetDifficultyColor(TrialDifficulty difficulty)
        {
            switch (difficulty)
            {
                case TrialDifficulty.Normal: return Color.Green;
                case TrialDifficulty.Hard: return Color.Yellow;
                case TrialDifficulty.Expert: return Color.Orange;
                case TrialDifficulty.Legendary: return Color.Red;
                default: return Color.White;
            }
        }

        private static void UpdateModifierEffects(Player player)
        {
            foreach (var modifier in ActiveModifiers)
            {
                switch (modifier)
                {
                    case TrialModifier.GravityShift:
                        // Random gravity changes every 5 seconds
                        if (Main.GameUpdateCount % 300 == 0)
                        {
                            player.gravDir = Main.rand.NextBool() ? 1f : -1f;
                            Main.NewText("Gravity shifts!", Color.Purple);
                        }
                        break;
                    case TrialModifier.ToxicArena:
                        // Poison clouds spawn periodically
                        if (Main.rand.NextBool(600)) // Every 10 seconds
                        {
                            Vector2 cloudPos = player.Center + new Vector2(Main.rand.Next(-300, 301), Main.rand.Next(-200, 201));
                            Projectile.NewProjectile(null, cloudPos, Vector2.Zero, ProjectileID.ToxicCloud, 20, 0f, Main.myPlayer);
                        }
                        break;
                    case TrialModifier.RandomTeleports:
                        // Random teleportation every 15 seconds
                        if (Main.GameUpdateCount % 900 == 0)
                        {
                            Vector2 teleportPos = new Vector2(
                                Main.rand.Next(200, Main.maxTilesX * 16 - 200),
                                Main.rand.Next(Main.maxTilesY * 16 - 300, Main.maxTilesY * 16 - 100)
                            );
                            player.Teleport(teleportPos, 1);
                            Main.NewText("Reality warps around you!", Color.Magenta);
                        }
                        break;
                    case TrialModifier.LowVisibility:
                        // Fog effect (would need lighting system integration)
                        break;
                }
            }
        }

        public static void StartTrialWithModifiers(string trialName, Player player, TrialDifficulty difficulty)
        {
            if (IsTrialActive) return;

            // Check if player has unlocked this difficulty
            var trialPlayer = player.GetModPlayer<TrialPlayer>();
            if ((int)difficulty > trialPlayer.TrialStats.HighestDifficulty + 1)
            {
                Main.NewText($"You must complete {GetPreviousDifficulty(difficulty)} trials first!", Color.Red);
                return;
            }

            CurrentTrial = trialName;
            CurrentDifficulty = difficulty;

            // Generate random modifier options
            GenerateModifierOptions();

            // Show modifier selection UI
            _showModifierSelection = true;
            Main.NewText($"Choose modifiers for {trialName} [{difficulty}]", Color.Cyan);
            Main.NewText("Press number keys 1-3 to select modifiers, or 0 for none", Color.LightGray);
        }

        private static void GenerateModifierOptions()
        {
            // Reset available modifiers
            _availableModifiers = new TrialModifier[3];

            // Get all possible modifiers
            var allModifiers = Enum.GetValues(typeof(TrialModifier)).Cast<TrialModifier>()
                .Where(m => m != TrialModifier.None).ToList();

            // Randomly select 3 modifiers
            for (int i = 0; i < 3; i++)
            {
                if (allModifiers.Count > 0)
                {
                    int randomIndex = Main.rand.Next(allModifiers.Count);
                    _availableModifiers[i] = allModifiers[randomIndex];
                    allModifiers.RemoveAt(randomIndex);
                }
            }
        }

        private static TrialDifficulty GetPreviousDifficulty(TrialDifficulty difficulty)
        {
            switch (difficulty)
            {
                case TrialDifficulty.Hard: return TrialDifficulty.Normal;
                case TrialDifficulty.Expert: return TrialDifficulty.Hard;
                case TrialDifficulty.Legendary: return TrialDifficulty.Expert;
                default: return TrialDifficulty.Normal;
            }
        }

        public static void HandleModifierSelection(int selection)
        {
            if (!_showModifierSelection) return;

            ActiveModifiers.Clear();

            if (selection >= 1 && selection <= 3)
            {
                // Add selected modifier
                int modifierIndex = selection - 1;
                if (modifierIndex < _availableModifiers.Length)
                {
                    ActiveModifiers.Add(_availableModifiers[modifierIndex]);
                }
            }

            // Hide selection UI and start trial
            _showModifierSelection = false;
            StartTrial(CurrentTrial, Main.LocalPlayer, CurrentDifficulty, ActiveModifiers);
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            if (_showModifierSelection)
            {
                // Add modifier selection UI layer
                layers.Add(new LegacyGameInterfaceLayer(
                    "WeaponMerging: Trial Modifier Selection",
                    delegate
                    {
                        DrawModifierSelection();
                        return true;
                    },
                    InterfaceScaleType.UI)
                );
            }

            if (IsTrialActive)
            {
                int resourceIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Resource Bars"));
                LegacyGameInterfaceLayer trialHudLayer = new LegacyGameInterfaceLayer(
                    "WeaponMerging: Trial HUD",
                    delegate
                    {
                        DrawTrialUI();
                        return true;
                    },
                    InterfaceScaleType.UI);

                if (resourceIndex != -1)
                {
                    layers.Insert(resourceIndex + 1, trialHudLayer);
                }
                else
                {
                    layers.Add(trialHudLayer);
                }
            }
        }

        private static void DrawModifierSelection()
        {
            // Draw semi-transparent background
            Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.Black * 0.5f);

            // Draw modifier selection panel
            Vector2 panelCenter = new Vector2(Main.screenWidth / 2, Main.screenHeight / 2);
            Vector2 panelSize = new Vector2(400, 300);
            Rectangle panelRect = new Rectangle((int)(panelCenter.X - panelSize.X / 2), (int)(panelCenter.Y - panelSize.Y / 2), (int)panelSize.X, (int)panelSize.Y);

            // Draw panel background
            Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, panelRect, Color.DarkBlue * 0.8f);
            Utils.DrawBorderString(Main.spriteBatch, "Choose a Trial Modifier", panelCenter - new Vector2(0, 120), Color.White, 1.2f, 0.5f, 0.5f);

            // Display trial tokens
            var trialPlayer = Main.LocalPlayer.GetModPlayer<TrialPlayer>();
            Utils.DrawBorderString(Main.spriteBatch, $"Trial Tokens: {trialPlayer.TrialStats.TrialTokens}", panelCenter - new Vector2(0, 100), Color.Gold, 0.9f, 0.5f, 0.5f);

            // Display base reward for the selected difficulty
            int baseTokens = GetEstimatedTokenReward(CurrentTrial, CurrentDifficulty);
            Utils.DrawBorderString(Main.spriteBatch, $"Base Reward ({CurrentDifficulty}): {baseTokens} tokens", panelCenter - new Vector2(0, 82), Color.LightGreen, 0.8f, 0.5f, 0.5f);

            // Draw modifier options
            for (int i = 0; i < _availableModifiers.Length; i++)
            {
                Vector2 optionPos = panelCenter + new Vector2(-150, -60 + i * 40);
                string optionText = $"{i + 1}. {GetModifierDisplayName(_availableModifiers[i])}";
                Utils.DrawBorderString(Main.spriteBatch, optionText, optionPos, Color.LightGray);

                // Draw description
                Vector2 descPos = panelCenter + new Vector2(-150, -45 + i * 40);
                string description = GetModifierDescription(_availableModifiers[i]);
                Utils.DrawBorderString(Main.spriteBatch, description, descPos, Color.Gray, 0.8f);

                int bonus = GetModifierTokenBonus(_availableModifiers[i]);
                Vector2 bonusPos = panelCenter + new Vector2(140, -60 + i * 40);
                string bonusText = bonus > 0 ? $"+{bonus} tokens" : "No bonus";
                Utils.DrawBorderString(Main.spriteBatch, bonusText, bonusPos, Color.Goldenrod, 0.8f, 0.5f, 0.5f);
            }

            // Draw "No modifier" option
            Vector2 noModifierPos = panelCenter + new Vector2(-150, 60);
            Utils.DrawBorderString(Main.spriteBatch, "0. No modifier (Easier)", noModifierPos, Color.LightGreen);
        }

        private static string GetModifierDisplayName(TrialModifier modifier)
        {
            switch (modifier)
            {
                case TrialModifier.SlipperyFloor: return "Slippery Floor";
                case TrialModifier.GravityShift: return "Gravity Shift";
                case TrialModifier.ToxicArena: return "Toxic Arena";
                case TrialModifier.SpeedBoost: return "Speed Boost";
                case TrialModifier.DefenseReduction: return "Defense Down";
                case TrialModifier.NoHealing: return "No Healing";
                case TrialModifier.DoubleMinions: return "Double Minions";
                case TrialModifier.RapidAttacks: return "Rapid Attacks";
                case TrialModifier.InvisibleBoss: return "Invisible Boss";
                case TrialModifier.RandomTeleports: return "Random Teleports";
                case TrialModifier.TimePressure: return "Time Pressure";
                case TrialModifier.LowVisibility: return "Low Visibility";
                case TrialModifier.KnockbackMaster: return "Knockback Master";
                case TrialModifier.ProjectileStorm: return "Projectile Storm";
                case TrialModifier.BossRegen: return "Boss Regeneration";
                case TrialModifier.PlayerWeakness: return "Player Weakness";
                default: return modifier.ToString();
            }
        }

        private static string GetModifierDescription(TrialModifier modifier)
        {
            switch (modifier)
            {
                case TrialModifier.SlipperyFloor: return "Harder movement control, but +5 tokens";
                case TrialModifier.GravityShift: return "Gravity randomly changes direction";
                case TrialModifier.ToxicArena: return "Poison clouds spawn periodically";
                case TrialModifier.SpeedBoost: return "You move faster but take more damage";
                case TrialModifier.DefenseReduction: return "Your defense is reduced but you deal more damage";
                case TrialModifier.NoHealing: return "Cannot use healing potions";
                case TrialModifier.DoubleMinions: return "Boss spawns twice as many minions";
                case TrialModifier.RapidAttacks: return "Boss attacks much more frequently";
                case TrialModifier.InvisibleBoss: return "Boss becomes partially invisible";
                case TrialModifier.RandomTeleports: return "You randomly teleport during fight";
                case TrialModifier.TimePressure: return "Timer counts down faster";
                case TrialModifier.LowVisibility: return "Arena becomes dark with reduced visibility";
                case TrialModifier.KnockbackMaster: return "Boss has increased knockback";
                case TrialModifier.ProjectileStorm: return "Boss unleashes constant projectile barrages";
                case TrialModifier.BossRegen: return "Boss regenerates health over time";
                case TrialModifier.PlayerWeakness: return "You take increased damage from all sources";
                default: return "";
            }
        }

        public static void StartTrial(string trialName, Player player, TrialDifficulty difficulty = TrialDifficulty.Normal, List<TrialModifier> modifiers = null)
        {
            if (IsTrialActive) return;

            CurrentTrial = trialName;
            CurrentDifficulty = difficulty;
            ActiveModifiers = modifiers ?? new List<TrialModifier>();
            IsTrialActive = true;
            _trialStartDelay = 60; // 1 second delay before checking completion

            // Calculate time limit based on difficulty (in seconds)
            TrialTimeLimit = GetTrialTimeLimit(difficulty);
            if (modifiers.Contains(TrialModifier.TimePressure))
            {
                TrialTimeLimit /= 2;
            }
            TrialStartTime = 0; // Will be set when boss spawns

            // Save player state before entering subworld
            OriginalPlayerPosition = player.position;
            OriginalPlayerInventory = new List<Item>();
            for (int i = 0; i < 58; i++)
            {
                OriginalPlayerInventory.Add(player.inventory[i].Clone());
            }

            // Save main world time state
            _originalTime = Main.time;
            _originalDayTime = Main.dayTime;

            // Enter the trial subworld (creates a completely new world)
            SubworldSystem.Enter<TrialArenaSubworld>();

            // Set night time for Eye of Cthulhu trial
            if (trialName == "Eye of Cthulhu Trial")
            {
                Main.time = 0; // Night time
                Main.dayTime = false;
            }

            // Position player at the bottom of the arena (on the ground platform)
            player.position = new Vector2(Main.maxTilesX * 16 / 2 - player.width / 2, Main.maxTilesY * 16 - 120);

            // Apply modifier effects to player
            ApplyModifierEffects(player);

            Main.NewText($"Entering {trialName} [{difficulty}]...", Color.Cyan);
            // Boss spawning will happen in UpdateUI after subworld loads
        }

        private static int GetTrialTimeLimit(TrialDifficulty difficulty)
        {
            switch (difficulty)
            {
                case TrialDifficulty.Normal: return 300; // 5 minutes
                case TrialDifficulty.Hard: return 360; // 6 minutes
                case TrialDifficulty.Expert: return 420; // 7 minutes
                case TrialDifficulty.Legendary: return 480; // 8 minutes
                default: return 300;
            }
        }

        private static void ApplyModifierEffects(Player player)
        {
            foreach (var modifier in ActiveModifiers)
            {
                switch (modifier)
                {
                    case TrialModifier.SpeedBoost:
                        player.moveSpeed *= 1.25f;
                        player.maxRunSpeed *= 1.25f;
                        // Increased damage vulnerability is handled in GlobalNPC
                        break;
                    case TrialModifier.DefenseReduction:
                        player.endurance = Math.Max(0f, player.endurance - 0.15f);
                        player.GetDamage(DamageClass.Generic) += 0.12f;
                        break;
                    case TrialModifier.PlayerWeakness:
                        // Handled in GlobalNPC for increased damage taken
                        break;
                    case TrialModifier.TimePressure:
                        // Increases time depletion rate (handled in UpdateUI)
                        break;
                    case TrialModifier.NoHealing:
                        // Prevented in item usage hooks
                        break;
                    case TrialModifier.RandomTeleports:
                        // Handled in UpdateUI with random teleportation
                        break;
                    case TrialModifier.SlipperyFloor:
                        player.runAcceleration *= 0.3f;
                        player.runSlowdown *= 0.3f;
                        break;
                    case TrialModifier.GravityShift:
                        player.gravDir *= -1;
                        break;
                    case TrialModifier.ToxicArena:
                        // Handled in UpdateUI for spawning poison clouds
                        break;
                    case TrialModifier.LowVisibility:
                        // Reduce lighting brightness
                        Lighting.GlobalBrightness *= 0.5f;
                        break;
                }
            }
        }

        private static void SpawnTrialBoss(string trialName)
        {
            // Clear any existing trial NPCs first
            if (TrialNPC >= 0 && TrialNPC < Main.maxNPCs && Main.npc[TrialNPC].active)
            {
                Main.npc[TrialNPC].active = false;
            }

            // Spawn in the center of the subworld arena (adjusted for 500x300 arena)
            Vector2 spawnPos = new Vector2(Main.maxTilesX * 16 / 2, Main.maxTilesY * 16 - 400); // Center X, above platform

            switch (trialName)
            {
                case "Eye of Cthulhu Trial":
                    TrialNPC = NPC.NewNPC(null, (int)spawnPos.X, (int)spawnPos.Y, NPCID.EyeofCthulhu, 0, 0f, 0f, 0f, 0f, Main.myPlayer);
                    break;
                case "Skeletron Trial":
                    Vector2 skeletronSpawnPos = new Vector2(spawnPos.X, Main.maxTilesY * 16 - 320);
                    TrialNPC = NPC.NewNPC(null, (int)skeletronSpawnPos.X, (int)skeletronSpawnPos.Y, NPCID.SkeletronHead, 0, 0f, 0f, 0f, 0f, Main.myPlayer);
                    if (TrialNPC >= 0)
                    {
                        NPC head = Main.npc[TrialNPC];
                        int hand = NPC.NewNPC(null, (int)head.Center.X - 120, (int)head.Center.Y, NPCID.SkeletronHand);
                        if (hand >= 0)
                        {
                            Main.npc[hand].GetGlobalNPC<TrialBossNPC>().IsTrialBoss = true;
                        }
                    }
                    break;
                case "Queen Bee Trial":
                    TrialNPC = NPC.NewNPC(null, (int)spawnPos.X, (int)spawnPos.Y, NPCID.QueenBee, 0, 0f, 0f, 0f, 0f, Main.myPlayer);
                    break;
            }

            if (TrialNPC >= 0 && TrialNPC < Main.maxNPCs && Main.npc[TrialNPC].active)
            {
                NPC npc = Main.npc[TrialNPC];
                npc.GetGlobalNPC<TrialBossNPC>().IsTrialBoss = true;
                npc.timeLeft = NPC.activeTime;

                // Apply difficulty scaling
                ApplyDifficultyScaling(npc);

                // Apply modifier effects to boss
                ApplyModifierEffectsToBoss(npc);

                // Start the trial timer
                TrialStartTime = (long)Main.GameUpdateCount;

                Main.NewText($"{npc.FullName} has appeared in the trial arena!", Color.Orange);
            }
            else
            {
                Main.NewText("Failed to spawn trial boss! Exiting trial...", Color.Red);
                EndTrial(Main.LocalPlayer, false);
            }
        }

        private static void ApplyDifficultyScaling(NPC npc)
        {
            float healthMultiplier = 1f;
            float damageMultiplier = 1f;
            float defenseMultiplier = 1f;

            switch (CurrentDifficulty)
            {
                case TrialDifficulty.Normal:
                    healthMultiplier = 1.5f;
                    damageMultiplier = 1.2f;
                    defenseMultiplier = 1.1f;
                    break;
                case TrialDifficulty.Hard:
                    healthMultiplier = 2.0f;
                    damageMultiplier = 1.5f;
                    defenseMultiplier = 1.3f;
                    break;
                case TrialDifficulty.Expert:
                    healthMultiplier = 2.5f;
                    damageMultiplier = 1.8f;
                    defenseMultiplier = 1.5f;
                    break;
                case TrialDifficulty.Legendary:
                    healthMultiplier = 3.0f;
                    damageMultiplier = 2.0f;
                    defenseMultiplier = 1.7f;
                    break;
            }

            npc.lifeMax = (int)(npc.lifeMax * healthMultiplier);
            npc.life = npc.lifeMax;
            npc.damage = (int)(npc.damage * damageMultiplier);
            npc.defense = (int)(npc.defense * defenseMultiplier);
        }

        private static void ApplyModifierEffectsToBoss(NPC npc)
        {
            foreach (var modifier in ActiveModifiers)
            {
                switch (modifier)
                {
                    case TrialModifier.DoubleMinions:
                        // Boss spawns twice as many minions (handled in AI methods)
                        break;
                    case TrialModifier.RapidAttacks:
                        // Boss attacks much more frequently (handled in AI methods)
                        break;
                    case TrialModifier.InvisibleBoss:
                        npc.alpha = 100; // Semi-transparent
                        break;
                    case TrialModifier.KnockbackMaster:
                        // Increased knockback (handled in GlobalNPC)
                        break;
                    case TrialModifier.ProjectileStorm:
                        // Constant projectile barrage (handled in AI methods)
                        break;
                    case TrialModifier.BossRegen:
                        // Boss regenerates health (handled in GlobalNPC)
                        break;
                }
            }
        }

        public static void EndTrial(Player player, bool success = false)
        {
            if (!IsTrialActive) return;

            // Calculate trial time
            int completionTime = 0;
            if (TrialStartTime > 0)
            {
                long elapsedFrames = (long)Main.GameUpdateCount - TrialStartTime;
                if (elapsedFrames < 0)
                {
                    elapsedFrames = 0;
                }

                completionTime = (int)(elapsedFrames / 60L); // Convert to seconds
            }

            // Update persistent player data
            var modifiersUsed = new List<TrialModifier>(ActiveModifiers);

            UpdatePlayerTrialStats(player, success, completionTime);

            // Give rewards if successful (before exiting subworld)
            if (success)
            {
                GrantTrialRewards(CurrentTrial, player, completionTime, modifiersUsed);
            }

            // Exit the trial subworld (back to main world)
            SubworldSystem.Exit();

            // Restore player position in main world
            player.position = OriginalPlayerPosition;

            // Restore player inventory
            for (int i = 0; i < 58 && i < OriginalPlayerInventory.Count; i++)
            {
                player.inventory[i] = OriginalPlayerInventory[i].Clone();
            }

            // Restore main world time state
            Main.time = _originalTime;
            Main.dayTime = _originalDayTime;

            // Clean up
            IsTrialActive = false;
            CurrentTrial = null;
            TrialNPC = -1;
            TrialStartTime = 0;
            TrialTimeLimit = 0;
            ActiveModifiers.Clear();
            _trialStartDelay = 0;
            _bossDefeatDelay = 0;

            Main.NewText(success ? $"Trial completed in {completionTime} seconds!" : "Trial failed.", success ? Color.Green : Color.Red);
        }

        private static void UpdatePlayerTrialStats(Player player, bool success, int completionTime)
        {
            var trialPlayer = player.GetModPlayer<TrialPlayer>();

            if (success)
            {
                trialPlayer.TrialStats.TotalTrialsCompleted++;

                // Update current streak
                if (trialPlayer.TrialStats.CurrentStreak < 0) trialPlayer.TrialStats.CurrentStreak = 1;
                else trialPlayer.TrialStats.CurrentStreak++;

                // Update best times
                if (!trialPlayer.TrialStats.BossBestTimes.ContainsKey(CurrentTrial) ||
                    completionTime < trialPlayer.TrialStats.BossBestTimes[CurrentTrial])
                {
                    trialPlayer.TrialStats.BossBestTimes[CurrentTrial] = completionTime;
                }

                // Update modifier completions
                foreach (var modifier in ActiveModifiers)
                {
                    if (!trialPlayer.TrialStats.ModifierCompletions.ContainsKey(modifier))
                        trialPlayer.TrialStats.ModifierCompletions[modifier] = 0;
                    trialPlayer.TrialStats.ModifierCompletions[modifier]++;
                }

                // Update highest difficulty
                if ((int)CurrentDifficulty > trialPlayer.TrialStats.HighestDifficulty)
                {
                    trialPlayer.TrialStats.HighestDifficulty = (int)CurrentDifficulty;
                }

                // Unlock titles based on achievements
                CheckTitleUnlocks(trialPlayer);
            }
            else
            {
                // Reset streak on failure
                trialPlayer.TrialStats.CurrentStreak = -1;
            }
        }

        private static void CheckTitleUnlocks(TrialPlayer trialPlayer)
        {
            var stats = trialPlayer.TrialStats;

            if (stats.TotalTrialsCompleted >= 10 && !stats.UnlockedTitles.Contains("Trial Initiate"))
                stats.UnlockedTitles.Add("Trial Initiate");

            if (stats.TotalTrialsCompleted >= 50 && !stats.UnlockedTitles.Contains("Trial Veteran"))
                stats.UnlockedTitles.Add("Trial Veteran");

            if (stats.CurrentStreak >= 5 && !stats.UnlockedTitles.Contains("Streak Master"))
                stats.UnlockedTitles.Add("Streak Master");

            if (stats.HighestDifficulty >= (int)TrialDifficulty.Legendary && !stats.UnlockedTitles.Contains("Legendary Challenger"))
                stats.UnlockedTitles.Add("Legendary Challenger");

            if (stats.ModifierCompletions.Count >= 5 && !stats.UnlockedTitles.Contains("Modifier Master"))
                stats.UnlockedTitles.Add("Modifier Master");
        }

        private static void CheckTrialCompletion()
        {
            // Add a small delay before checking completion to prevent instant completion
            if (TrialNPC >= 0 && TrialNPC < Main.maxNPCs && _trialStartDelay <= 0)
            {
                NPC npc = Main.npc[TrialNPC];

                // Multiple defeat condition checks for robustness
                bool bossDefeated = false;

                // Check 1: NPC is inactive
                if (!npc.active)
                {
                    bossDefeated = true;
                }
                // Check 2: Life is zero or below with buffer delay
                else if (npc.life <= 0)
                {
                    _bossDefeatDelay++;
                    if (_bossDefeatDelay >= 10) // Wait 10 frames after life reaches 0
                    {
                        bossDefeated = true;
                    }
                }
                // Check 3: Special defeat conditions for specific bosses
                else if (IsBossSpecificallyDefeated(npc))
                {
                    bossDefeated = true;
                    _bossDefeatDelay = 0; // Reset delay for special conditions
                }
                else
                {
                    _bossDefeatDelay = 0; // Reset if not defeated
                }

                if (bossDefeated)
                {
                    // Debug message to confirm defeat detection
                    Main.NewText($"Boss {npc.FullName} defeated! Life: {npc.life}, Active: {npc.active}", Color.Green);

                    // Ensure NPC is properly cleaned up
                    npc.active = false;
                    npc.life = 0;

                    // Clear any associated NPCs (minions, servants, etc.)
                    ClearBossMinions();

                    // Reset defeat delay
                    _bossDefeatDelay = 0;

                    EndTrial(Main.LocalPlayer, true);
                }
            }
        }

        private static bool IsBossSpecificallyDefeated(NPC npc)
        {
            // Special defeat conditions for specific bosses
            switch (npc.type)
            {
                case NPCID.EyeofCthulhu:
                    // Eye might have special defeat states
                    return npc.ai[0] == 3f || (npc.life <= 1 && npc.ai[1] <= 0);
                case NPCID.SkeletronHead:
                    // Skeletron defeat when head reaches 1 HP
                    return npc.life <= 1;
                case NPCID.QueenBee:
                    // Queen Bee might have special phases
                    return npc.life <= 1;
                default:
                    return false;
            }
        }

        private static void ClearBossMinions()
        {
            // Clean up any remaining minions or associated NPCs
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && npc.GetGlobalNPC<TrialBossNPC>().IsTrialBoss &&
                    npc.type != NPCID.EyeofCthulhu && npc.type != NPCID.KingSlime && npc.type != NPCID.QueenBee)
                {
                    // Kill minions/servants/bees that are still active
                    npc.active = false;
                    npc.life = 0;
                }
            }
        }

        private static void CheckPlayerEscape()
        {
            if (Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Escape) && !Main.oldKeyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Escape))
            {
                // Check if boss is defeated before escaping
                bool bossDefeated = false;
                if (TrialNPC >= 0 && TrialNPC < Main.maxNPCs)
                {
                    NPC npc = Main.npc[TrialNPC];

                    // Use same defeat checking logic as completion check
                    if (!npc.active || npc.life <= 0 || IsBossSpecificallyDefeated(npc))
                    {
                        bossDefeated = true;
                        // Clean up the defeated boss and minions
                        npc.active = false;
                        npc.life = 0;
                        ClearBossMinions();
                    }
                }

                EndTrial(Main.LocalPlayer, bossDefeated);
            }
        }

        private static void KeepPlayerInArena()
        {
            Player player = Main.LocalPlayer;
            
            // Keep player within subworld bounds (200x120 tiles)
            float worldWidth = Main.maxTilesX * 16;
            float worldHeight = Main.maxTilesY * 16;
            
            // Keep within horizontal bounds with some padding
            if (player.position.X < 32)
                player.position.X = 32;
            if (player.position.X + player.width > worldWidth - 32)
                player.position.X = worldWidth - player.width - 32;
                
            // Keep within vertical bounds
            if (player.position.Y < 32)
                player.position.Y = 32;
            if (player.position.Y + player.height > worldHeight - 32)
                player.position.Y = worldHeight - player.height - 32;
        }

        private static void GrantTrialRewards(string trialName, Player player, int completionTime, List<TrialModifier> modifiersUsed)
        {
            var trialPlayer = player.GetModPlayer<TrialPlayer>();
            var stats = trialPlayer.TrialStats;

            // Base trial tokens
            int baseTokens = GetBaseTrialTokens(trialName, CurrentDifficulty);
            int modifierBonus = CalculateModifierBonus(modifiersUsed);
            int timeBonus = CalculateTimeBonus(completionTime);
            int totalTokens = baseTokens + modifierBonus + timeBonus;

            stats.TrialTokens += totalTokens;
            Main.NewText($"Earned {totalTokens} trial tokens! (Base: {baseTokens}, Modifiers: {modifierBonus}, Time: {timeBonus})", Color.Gold);

            // Random rewards
            GrantRandomRewards(OriginalPlayerInventory);

            // Achievement rewards
            GrantAchievementRewards(OriginalPlayerInventory, trialPlayer);
        }

        private static int GetBaseTrialTokens(string trialName, TrialDifficulty difficulty)
        {
            int baseTokens = 10; // Base reward

            // Difficulty multiplier
            float difficultyMultiplier = 1f;
            switch (difficulty)
            {
                case TrialDifficulty.Normal: difficultyMultiplier = 1.0f; break;
                case TrialDifficulty.Hard: difficultyMultiplier = 1.5f; break;
                case TrialDifficulty.Expert: difficultyMultiplier = 2.0f; break;
                case TrialDifficulty.Legendary: difficultyMultiplier = 3.0f; break;
            }

            // Boss-specific multipliers
            float bossMultiplier = 1f;
            switch (trialName)
            {
                case "Eye of Cthulhu Trial": bossMultiplier = 1.2f; break;
                case "Skeletron Trial": bossMultiplier = 1.15f; break;
                case "Queen Bee Trial": bossMultiplier = 1.1f; break;
            }

            return (int)(baseTokens * difficultyMultiplier * bossMultiplier);
        }

        public static int GetEstimatedTokenReward(string trialName, TrialDifficulty difficulty = TrialDifficulty.Normal)
        {
            return GetBaseTrialTokens(trialName, difficulty);
        }

        private static int CalculateModifierBonus(List<TrialModifier> modifiers = null)
        {
            int bonus = 0;
            var source = modifiers ?? ActiveModifiers;
            foreach (var modifier in source)
            {
                bonus += GetModifierTokenBonus(modifier);
            }
            return bonus;
        }

        private static int GetModifierTokenBonus(TrialModifier modifier)
        {
            switch (modifier)
            {
                case TrialModifier.SlipperyFloor:
                case TrialModifier.GravityShift:
                case TrialModifier.ToxicArena:
                    return 5;

                case TrialModifier.SpeedBoost:
                case TrialModifier.DefenseReduction:
                case TrialModifier.PlayerWeakness:
                    return 8;

                case TrialModifier.NoHealing:
                case TrialModifier.DoubleMinions:
                case TrialModifier.RapidAttacks:
                    return 12;

                case TrialModifier.InvisibleBoss:
                case TrialModifier.RandomTeleports:
                case TrialModifier.TimePressure:
                    return 15;

                case TrialModifier.LowVisibility:
                case TrialModifier.KnockbackMaster:
                case TrialModifier.ProjectileStorm:
                case TrialModifier.BossRegen:
                    return 20;

                default:
                    return 0;
            }
        }

        private static int CalculateTimeBonus(int completionTime)
        {
            int timeLimit = TrialTimeLimit;
            float timeRatio = (float)completionTime / timeLimit;

            if (timeRatio <= 0.5f) return 15;      // Under 50% of time limit
            if (timeRatio <= 0.75f) return 10;     // Under 75% of time limit
            if (timeRatio <= 1.0f) return 5;       // Under time limit

            return 0; // Over time limit
        }

        private static void GrantRandomRewards(List<Item> inventory)
        {
            Random rand = new Random();
            int rewardRolls = 1 + (int)CurrentDifficulty + ActiveModifiers.Count;

            for (int i = 0; i < rewardRolls; i++)
            {
                float roll = (float)rand.NextDouble();

                if (roll < 0.4f) // 40% chance for materials
                {
                    GrantMaterialReward(inventory);
                }
                else if (roll < 0.7f) // 30% chance for potions
                {
                    GrantPotionReward(inventory);
                }
                else if (roll < 0.9f) // 20% chance for weapons/tools
                {
                    GrantWeaponReward(inventory);
                }
                else // 10% chance for rare items
                {
                    GrantRareReward(inventory);
                }
            }
        }

        private static void GrantMaterialReward(List<Item> inventory)
        {
            int[] materials =
            {
                ItemID.CobaltBar,
                ItemID.PalladiumBar,
                ItemID.MythrilBar,
                ItemID.OrichalcumBar,
                ItemID.AdamantiteBar,
                ItemID.TitaniumBar,
                ItemID.HallowedBar
            };

            int material = materials[Main.rand.Next(materials.Length)];
            int amount = 8 + (int)CurrentDifficulty * 4;

            AddItemToInventory(inventory, material, amount);
            Main.NewText($"Found {amount} hardmode materials!", Color.LightBlue);
        }

        private static void GrantPotionReward(List<Item> inventory)
        {
            int[] potions = { ItemID.GreaterHealingPotion, ItemID.GreaterManaPotion, ItemID.RegenerationPotion, ItemID.IronskinPotion, ItemID.SwiftnessPotion, ItemID.WrathPotion, ItemID.RagePotion };
            int potion = potions[Main.rand.Next(potions.Length)];
            int amount = 2 + (int)CurrentDifficulty;

            AddItemToInventory(inventory, potion, amount);
            Main.NewText($"Found {amount} potions!", Color.LightGreen);
        }

        private static void GrantWeaponReward(List<Item> inventory)
        {
            if (CurrentDifficulty >= TrialDifficulty.Hard)
            {
                int[] weapons = { ItemID.TerraBlade, ItemID.StarWrath, ItemID.SkyFracture, ItemID.InfluxWaver, ItemID.PhoenixBlaster, ItemID.Xenopopper, ItemID.LastPrism };
                int weapon = weapons[Main.rand.Next(weapons.Length)];
                AddItemToInventory(inventory, weapon, 1);
                Main.NewText("Found a legendary weapon!", Color.Yellow);
            }
        }

        private static void GrantRareReward(List<Item> inventory)
        {
            if (CurrentDifficulty >= TrialDifficulty.Expert)
            {
                int[] rares =
                {
                    ItemID.GoldenKey,
                    ItemID.NightKey,
                    ItemID.LightKey,
                    ItemID.RodofDiscord,
                    ItemID.LifeCrystal,
                    ItemID.ManaCrystal,
                    ItemID.SoulofLight,
                    ItemID.SoulofNight
                };

                int rare = rares[Main.rand.Next(rares.Length)];
                int amount = rare == ItemID.LifeCrystal || rare == ItemID.ManaCrystal ? 2 : 20;
                if (rare == ItemID.RodofDiscord || rare == ItemID.GoldenKey || rare == ItemID.NightKey || rare == ItemID.LightKey)
                {
                    amount = 1;
                }

                AddItemToInventory(inventory, rare, amount);
                Main.NewText("Found a legendary item!", Color.Purple);
            }
        }

        private static void AddItemToInventory(List<Item> inventory, int itemType, int amount)
        {
            for (int i = 0; i < inventory.Count && amount > 0; i++)
            {
                if (inventory[i].IsAir || (inventory[i].type == itemType && inventory[i].stack < inventory[i].maxStack))
                {
                    int canAdd = Math.Min(amount, inventory[i].IsAir ? inventory[i].maxStack : inventory[i].maxStack - inventory[i].stack);
                    if (inventory[i].IsAir)
                    {
                        inventory[i] = new Item(itemType, canAdd);
                    }
                    else
                    {
                        inventory[i].stack += canAdd;
                    }
                    amount -= canAdd;
                }
            }
        }

        private static void GrantAchievementRewards(List<Item> inventory, TrialPlayer trialPlayer)
        {
            var stats = trialPlayer.TrialStats;

            // First completion of each difficulty
            if (stats.HighestDifficulty == (int)CurrentDifficulty && stats.TotalTrialsCompleted == GetDifficultyUnlockRequirement(CurrentDifficulty))
            {
                AddItemToInventory(inventory, ItemID.GoldChest, 1);
                Main.NewText($"First {CurrentDifficulty} trial completed! Bonus chest awarded!", Color.Gold);
            }

            // Streak rewards
            if (stats.CurrentStreak > 0 && stats.CurrentStreak % 10 == 0)
            {
                AddItemToInventory(inventory, ItemID.PiggyBank, 1);
                Main.NewText($"Streak milestone! {stats.CurrentStreak} wins in a row!", Color.Gold);
            }

            // Modifier mastery
            if (ActiveModifiers.Count >= 3 && Main.rand.NextBool(10)) // 10% chance with 3+ modifiers
            {
                AddItemToInventory(inventory, ItemID.DiscountCard, 1);
                Main.NewText("Modifier mastery! Discount card earned!", Color.Cyan);
            }
        }

        private static int GetDifficultyUnlockRequirement(TrialDifficulty difficulty)
        {
            switch (difficulty)
            {
                case TrialDifficulty.Normal: return 0;
                case TrialDifficulty.Hard: return 5;
                case TrialDifficulty.Expert: return 15;
                case TrialDifficulty.Legendary: return 30;
                default: return 0;
            }
        }

        private static void TrialEyeAI(NPC npc)
        {
            Player player = Main.player[(int)npc.target];
            float arenaWidth = Main.maxTilesX * 16;
            float arenaHeight = Main.maxTilesY * 16;

            // Check for modifier effects
            bool doubleMinions = TrialSystem.ActiveModifiers.Contains(TrialModifier.DoubleMinions);
            bool rapidAttacks = TrialSystem.ActiveModifiers.Contains(TrialModifier.RapidAttacks);
            bool projectileStorm = TrialSystem.ActiveModifiers.Contains(TrialModifier.ProjectileStorm);

            // Arena-specific: Eye creates illusions during dashes
            if (npc.ai[1] > 0) // Currently dashing
            {
                // Create illusion clones that deal damage
                int illusionFrequency = doubleMinions ? 2 : 3; // More frequent illusions with double minions
                if (Main.rand.NextBool(illusionFrequency)) // Every few frames during dash
                {
                    Vector2 illusionPos = npc.position + new Vector2(Main.rand.Next(-50, 51), Main.rand.Next(-50, 51));
                    int illusion = NPC.NewNPC(null, (int)illusionPos.X, (int)illusionPos.Y, NPCID.EyeofCthulhu);
                    if (illusion >= 0 && illusion < Main.maxNPCs)
                    {
                        Main.npc[illusion].GetGlobalNPC<TrialBossNPC>().IsTrialBoss = true;
                        Main.npc[illusion].life = 1; // Fragile illusions
                        Main.npc[illusion].damage = 15; // Moderate damage
                        Main.npc[illusion].timeLeft = 180; // Short lifetime
                        Main.npc[illusion].alpha = 100; // Semi-transparent
                    }
                }
            }

            // Handle rotation - Eye should face the player at all times
            Vector2 toPlayer = player.Center - npc.Center;
            npc.rotation = toPlayer.ToRotation() - MathHelper.PiOver2; // Face the player

            // Additional rotation during dashes (Eye should spin)
            if (npc.ai[1] > 0) // Currently dashing
            {
                // Add spinning effect during dashes
                npc.rotation += 0.5f; // Faster spinning during dashes
            }

            // Smooth rotation transitions
            if (npc.ai[1] <= 0) // Not dashing
            {
                // Gradually approach target rotation
                float targetRotation = toPlayer.ToRotation() - MathHelper.PiOver2;
                float rotationDiff = targetRotation - npc.rotation;
                while (rotationDiff > MathHelper.Pi) rotationDiff -= MathHelper.TwoPi;
                while (rotationDiff < -MathHelper.Pi) rotationDiff += MathHelper.TwoPi;
                npc.rotation += rotationDiff * 0.1f; // Smooth rotation toward player
            }

            // Enhanced movement for enraged phase (ai[0] == 2) - now aggressive close combat
            if (npc.ai[0] == 2f) // Enraged phase
            {
                // Aggressive close-range combat - charge and swarm the player
                float distanceToPlayer = Vector2.Distance(npc.Center, player.Center);

                if (distanceToPlayer > 150) // Too far - charge in aggressively
                {
                    Vector2 chargeDir = player.Center - npc.Center;
                    chargeDir.Normalize();
                    npc.velocity += chargeDir * 0.2f; // Controlled charge toward player
                }
                else // Close range - circle and harass
                {
                    // Quick circling motion around player
                    Vector2 circleDir = new Vector2(player.Center.Y - npc.Center.Y, npc.Center.X - player.Center.X);
                    circleDir.Normalize();
                    npc.velocity += circleDir * 0.12f;
                }

                // Random bursts of speed for unpredictability
                if (Main.rand.NextBool(60)) // Frequent random boosts
                {
                    Vector2 randomBoost = new Vector2(Main.rand.NextFloat(-1.2f, 1.2f), Main.rand.NextFloat(-1.2f, 1.2f));
                    npc.velocity += randomBoost;
                }

                // Keep within arena bounds with aggressive bouncing
                if (npc.position.X < 50 || npc.position.X + npc.width > arenaWidth - 50)
                {
                    npc.velocity.X *= -1.4f; // Even harder bounce when enraged
                }
                if (npc.position.Y < 50 || npc.position.Y + npc.height > arenaHeight - 100)
                {
                    npc.velocity.Y *= -1.3f; // Bounce vertically
                }
            }
            else
            {
                // Normal phase - still aggressive but with some positioning
                float distanceToPlayer = Vector2.Distance(npc.Center, player.Center);

                if (distanceToPlayer > 200) // Moderate distance - close in
                {
                    Vector2 moveDir = player.Center - npc.Center;
                    moveDir.Normalize();
                    npc.velocity += moveDir * 0.12f; // Move toward player
                }
                else if (distanceToPlayer < 100) // Too close - back off slightly
                {
                    Vector2 backOffDir = npc.Center - player.Center;
                    backOffDir.Normalize();
                    npc.velocity += backOffDir * 0.08f; // Slight back off
                }
                else // Good distance - occasional positioning adjustments
                {
                    if (Main.rand.NextBool(180)) // Less frequent orbiting
                    {
                        Vector2 orbitDir = new Vector2(player.Center.Y - npc.Center.Y, npc.Center.X - player.Center.X);
                        orbitDir.Normalize();
                        npc.velocity += orbitDir * 0.05f;
                    }
                }

                // Keep reasonable height above player for mixed combat
                float idealY = player.Center.Y - 80; // Much closer to player height
                float distanceToIdealY = idealY - npc.Center.Y;

                if (Math.Abs(distanceToIdealY) > 60) // Smaller height tolerance
                {
                    npc.velocity.Y += Math.Sign(distanceToIdealY) * 0.12f;
                }
            }

            // Additional projectile attacks when enraged
            if (npc.ai[0] == 2f) // Enraged phase
            {
                // More frequent cursed flames
                if (Main.rand.NextBool(120)) // More frequent flames
                {
                    Vector2 flameDirection = player.Center - npc.Center;
                    flameDirection.Normalize();
                    flameDirection *= 8f; // Faster flames

                    Projectile.NewProjectile(null, npc.Center, flameDirection, ProjectileID.CursedFlameHostile, 25, 0f, Main.myPlayer);
                }

                // Corrupt projectiles - spread pattern
                if (Main.rand.NextBool(200))
                {
                    for (int i = 0; i < 3; i++)
                    {
                        Vector2 corruptDirection = (player.Center - npc.Center).RotatedBy(Main.rand.NextFloat(-0.5f, 0.5f));
                        corruptDirection.Normalize();
                        corruptDirection *= 6f;

                        Projectile.NewProjectile(null, npc.Center, corruptDirection, ProjectileID.EyeLaser, 20, 0f, Main.myPlayer);
                    }
                }

                // Projectile storm - constant barrage
                if (projectileStorm && Main.rand.NextBool(60)) // Very frequent projectiles
                {
                    // Fire multiple projectiles in all directions
                    for (int i = 0; i < 8; i++)
                    {
                        float angle = (MathHelper.TwoPi / 8) * i;
                        Vector2 stormDirection = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 5f;
                        Projectile.NewProjectile(null, npc.Center, stormDirection, ProjectileID.EyeLaser, 15, 0f, Main.myPlayer);
                    }
                }

                // Summon servant eyes more frequently
                int summonFrequency = rapidAttacks ? 300 : 450; // More frequent summons with rapid attacks
                if (Main.rand.NextBool(summonFrequency)) // More frequent summons
                {
                    int servantCount = doubleMinions ? 10 : 5; // Double minions with modifier
                    for (int i = 0; i < servantCount; i++) // More servants
                    {
                        Vector2 servantPos = npc.Center + new Vector2(Main.rand.Next(-150, 151), Main.rand.Next(-150, 151));
                        int servant = NPC.NewNPC(null, (int)servantPos.X, (int)servantPos.Y, NPCID.ServantofCthulhu);
                        if (servant >= 0 && servant < Main.maxNPCs)
                        {
                            Main.npc[servant].GetGlobalNPC<TrialBossNPC>().IsTrialBoss = true;
                            Main.npc[servant].damage = (int)(Main.npc[servant].damage * 1.5f); // Stronger servants
                        }
                    }
                    Main.NewText("The Eye unleashes its full power!", Color.Purple);
                }
            }
            else // Normal phase attacks
            {
                // Occasional eye lasers even when not enraged
                if (Main.rand.NextBool(300))
                {
                    Vector2 laserDirection = player.Center - npc.Center;
                    laserDirection.Normalize();
                    laserDirection *= 7f;

                    Projectile.NewProjectile(null, npc.Center, laserDirection, ProjectileID.EyeLaser, 18, 0f, Main.myPlayer);
                }

                // Blood projectiles for extra damage
                if (Main.rand.NextBool(250))
                {
                    Vector2 bloodDirection = player.Center - npc.Center;
                    bloodDirection.Normalize();
                    bloodDirection *= 5f;

                    // Shoot multiple blood projectiles
                    for (int i = 0; i < 2; i++)
                    {
                        Vector2 spreadBlood = bloodDirection.RotatedBy(Main.rand.NextFloat(-0.3f, 0.3f));
                        Projectile.NewProjectile(null, npc.Center, spreadBlood, ProjectileID.BloodNautilusShot, 15, 0f, Main.myPlayer);
                    }
                }
            }

            // Apply gentle dampening to keep speeds in check
            npc.velocity *= npc.ai[0] == 2f ? 0.982f : 0.988f;

            // Enhanced speed control for arena close combat
            float maxXSpeed = (npc.ai[0] == 2f) ? 12f : 9f; // Controlled close combat speeds
            float maxYSpeed = (npc.ai[0] == 2f) ? 9f : 6.5f;

            if (Math.Abs(npc.velocity.X) > maxXSpeed)
                npc.velocity.X = Math.Sign(npc.velocity.X) * maxXSpeed;
            if (Math.Abs(npc.velocity.Y) > maxYSpeed)
                npc.velocity.Y = Math.Sign(npc.velocity.Y) * maxYSpeed;

            // Enrage at half health (existing logic)
            if (npc.life < npc.lifeMax * 0.5f && npc.ai[0] != 2f && npc.ai[0] != 3f)
            {
                npc.ai[0] = 2f; // Enter enraged state
                npc.damage = (int)(npc.damage * 1.4f); // More damage boost
                npc.defense = (int)(npc.defense * 1.6f); // More defense boost
                Main.NewText("The Eye of Cthulhu charges in for close combat!", Color.Red);
            }
        }

        private static void TrialSkeletronAI(NPC npc)
        {
            npc.TargetClosest(true);
            Player player = Main.player[(int)npc.target];
            if (!player.active || player.dead)
            {
                npc.velocity = Vector2.Lerp(npc.velocity, new Vector2(0f, 10f), 0.05f);
                return;
            }

            bool doubleMinions = ActiveModifiers.Contains(TrialModifier.DoubleMinions);
            bool rapidAttacks = ActiveModifiers.Contains(TrialModifier.RapidAttacks);
            bool projectileStorm = ActiveModifiers.Contains(TrialModifier.ProjectileStorm);

            Vector2 toPlayer = player.Center - npc.Center;
            float distance = toPlayer.Length();
            Vector2 direction = toPlayer.SafeNormalize(Vector2.UnitY);

            float orbitalRadius = 240f;
            Vector2 orbitTarget = player.Center + direction.RotatedBy(MathHelper.PiOver2) * orbitalRadius;
            Vector2 desiredVelocity = (orbitTarget - npc.Center) * 0.05f;
            npc.velocity = Vector2.Lerp(npc.velocity, desiredVelocity, 0.08f);

            float maxSpeed = rapidAttacks ? 13f : 10f;
            npc.velocity = npc.velocity.Length() > maxSpeed ? npc.velocity.SafeNormalize(Vector2.UnitX) * maxSpeed : npc.velocity;

            int spinInterval = rapidAttacks ? 45 : 90;
            if (Main.GameUpdateCount % spinInterval == 0)
            {
                for (int i = -2; i <= 2; i++)
                {
                    Vector2 skullDir = direction.RotatedBy(i * 0.25f) * 12f;
                    Projectile.NewProjectile(null, npc.Center, skullDir, ProjectileID.Skull, 30, 0f, Main.myPlayer);
                }
                Main.NewText("Spectral skull barrage!", Color.Gray);
                    npc.velocity.Y *= 0.9f;
            }

            // Prevent despawning
            npc.timeLeft = NPC.activeTime;
        }

        private static void TrialQueenBeeAI(NPC npc)
        {
            // Enhanced Queen Bee AI with unique arena mechanics
            Player player = Main.player[(int)npc.target];

            // Arena-specific: Create bee hives that spawn bees over time
            if (Main.rand.NextBool(500)) // Rare hive creation
            {
                Vector2 hivePos = new Vector2(Main.rand.Next(300, Main.maxTilesX * 16 - 300), Main.rand.Next(200, 250));
                // Create a "hive" effect with multiple projectiles
                for (int i = 0; i < 8; i++)
                {
                    Vector2 hiveOffset = new Vector2(Main.rand.Next(-20, 21), Main.rand.Next(-20, 21));
                    Projectile.NewProjectile(null, hivePos + hiveOffset, Vector2.Zero, ProjectileID.Bee, 12, 0f, Main.myPlayer);
                }
                // Spawn wasps from the "hive" over time
                for (int j = 0; j < 3; j++)
                {
                    int waspBee = NPC.NewNPC(null, (int)hivePos.X, (int)hivePos.Y, NPCID.Hornet);
                    if (waspBee >= 0 && waspBee < Main.maxNPCs)
                    {
                        Main.npc[waspBee].GetGlobalNPC<TrialBossNPC>().IsTrialBoss = true;
                        Main.npc[waspBee].damage = (int)(Main.npc[waspBee].damage * 1.5f); // Stronger wasps
                        Main.npc[waspBee].timeLeft = NPC.activeTime * 2; // Longer lifetime
                    }
                }
                Main.NewText("A wasp hive appears!", Color.Yellow);
            }

            // More frequent stings with seeking behavior
            if (Main.rand.NextBool(100)) // Very frequent
            {
                Vector2 stingDirection = player.Center - npc.Center;
                stingDirection.Normalize();
                stingDirection *= 9f; // Faster stings

                // Create homing stings
                for (int i = 0; i < 2; i++)
                {
                    Vector2 spreadDirection = stingDirection.RotatedBy(Main.rand.NextFloat(-0.1f, 0.1f));
                    Projectile.NewProjectile(null, npc.Center, spreadDirection, ProjectileID.Stinger, 22, 0f, Main.myPlayer);
                }
            }

            // Enhanced bee swarm attack with hostile wasps
            if (Main.rand.NextBool(300)) // Summon enhanced wasp swarm
            {
                int numWasps = Main.rand.Next(4, 7); // More wasps
                for (int i = 0; i < numWasps; i++)
                {
                    Vector2 waspPos = npc.Center + new Vector2(Main.rand.Next(-180, 181), Main.rand.Next(-180, 181));
                    int wasp = NPC.NewNPC(null, (int)waspPos.X, (int)waspPos.Y, NPCID.Hornet);
                    if (wasp >= 0 && wasp < Main.maxNPCs)
                    {
                        Main.npc[wasp].GetGlobalNPC<TrialBossNPC>().IsTrialBoss = true;
                        Main.npc[wasp].damage = (int)(Main.npc[wasp].damage * 1.4f); // Stronger wasps
                        Main.npc[wasp].lifeMax = 20; // Consistent health
                        Main.npc[wasp].life = Main.npc[wasp].lifeMax;
                        Main.npc[wasp].timeLeft = NPC.activeTime; // Prevent immediate despawn
                    }
                }
                Main.NewText("A deadly wasp swarm emerges!", Color.Yellow);
            }

            // Honey projectile attack with area denial
            if (Main.rand.NextBool(200)) // Create honey traps
            {
                // Create honey projectiles that leave slowing zones
                for (int i = 0; i < 3; i++)
                {
                    Vector2 honeyPos = player.Center + new Vector2(Main.rand.Next(-150, 151), Main.rand.Next(-150, 151));
                    Vector2 honeyDirection = honeyPos - npc.Center;
                    honeyDirection.Normalize();
                    honeyDirection *= 4f; // Slow but numerous

                    Projectile.NewProjectile(null, npc.Center, honeyDirection, ProjectileID.Bee, 18, 0f, Main.myPlayer);
                }
            }

            // Enhanced poison cloud defense - creates toxic zones
            if (npc.life < npc.lifeMax * 0.6f && Main.rand.NextBool(150)) // More frequent when damaged
            {
                // Create larger poison cloud field
                for (int i = 0; i < 10; i++) // More clouds
                {
                    Vector2 cloudPos = npc.Center + new Vector2(Main.rand.Next(-120, 121), Main.rand.Next(-120, 121));
                    Projectile.NewProjectile(null, cloudPos, Vector2.Zero, ProjectileID.ToxicCloud, 18, 0f, Main.myPlayer);
                }
                // Add some wasps to the toxic area
                for (int i = 0; i < 2; i++)
                {
                    Vector2 waspPos = npc.Center + new Vector2(Main.rand.Next(-80, 81), Main.rand.Next(-80, 81));
                    int toxicWasp = NPC.NewNPC(null, (int)waspPos.X, (int)waspPos.Y, NPCID.Hornet);
                    if (toxicWasp >= 0 && toxicWasp < Main.maxNPCs)
                    {
                        Main.npc[toxicWasp].GetGlobalNPC<TrialBossNPC>().IsTrialBoss = true;
                        Main.npc[toxicWasp].damage = (int)(Main.npc[toxicWasp].damage * 1.8f); // Very strong wasps in toxic zones
                    }
                }
                Main.NewText("Toxic wasp zone created!", Color.Purple);
            }

            // Arena dance - Queen Bee performs a special dance that summons bees in patterns
            if (Main.rand.NextBool(600)) // Rare special attack
            {
                // Create bee summoning pattern
                for (int ring = 0; ring < 3; ring++)
                {
                    int beesInRing = 6 + ring * 2;
                    for (int i = 0; i < beesInRing; i++)
                    {
                        float angle = (MathHelper.TwoPi / beesInRing) * i;
                        Vector2 ringPos = npc.Center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * (80 + ring * 40);
                        int danceWasp = NPC.NewNPC(null, (int)ringPos.X, (int)ringPos.Y, NPCID.Hornet);
                        if (danceWasp >= 0 && danceWasp < Main.maxNPCs)
                        {
                            Main.npc[danceWasp].GetGlobalNPC<TrialBossNPC>().IsTrialBoss = true;
                            Main.npc[danceWasp].damage = (int)(Main.npc[danceWasp].damage * 1.6f);
                        }
                    }
                }
                Main.NewText("Queen Bee performs the Dance of Wasps!", Color.Gold);
            }

            // Enhanced movement patterns - more aggressive and unpredictable
            if (Main.rand.NextBool(150)) // More frequent charges
            {
                Vector2 chargeDirection = player.Center - npc.Center;
                chargeDirection.Normalize();
                npc.velocity = chargeDirection * 8f; // Faster charges
                // Leave stinger trail during charge
                Projectile.NewProjectile(null, npc.position, Vector2.Zero, ProjectileID.Stinger, 12, 0f, Main.myPlayer);
            }
            else
            {
                // More dynamic lateral movement
                float arenaWidth = Main.maxTilesX * 16;
                if (npc.position.X < 120)
                {
                    npc.velocity.X = Math.Abs(npc.velocity.X) + 3f;
                }
                else if (npc.position.X + npc.width > arenaWidth - 120)
                {
                    npc.velocity.X = -Math.Abs(npc.velocity.X) - 3f;
                }

                // Add some vertical movement for unpredictability
                if (Main.rand.NextBool(200))
                {
                    npc.velocity.Y += Main.rand.NextFloat(-2f, 2f);
                }

                // Dampen excessive speeds more aggressively
                if (Math.Abs(npc.velocity.X) > 7f)
                    npc.velocity.X *= 0.9f;
                if (Math.Abs(npc.velocity.Y) > 6f)
                    npc.velocity.Y *= 0.9f;
            }

            // Prevent despawning
            npc.timeLeft = NPC.activeTime;
        }

        public class TrialBossNPC : GlobalNPC
        {
            public override bool InstancePerEntity => true;

            public bool IsTrialBoss;

            public override void SetDefaults(NPC npc)
            {
                if (IsTrialBoss)
                {
                    // Make trial bosses stronger
                    npc.damage = (int)(npc.damage * 1.5f);
                    npc.defense = (int)(npc.defense * 1.2f);
                    npc.lifeMax = (int)(npc.lifeMax * 2f);
                    npc.life = npc.lifeMax;
                    npc.scale = 1.2f;
                }
            }

            public override void AI(NPC npc)
            {
                if (IsTrialBoss)
                {
                    // Prevent ALL trial bosses from despawning due to time limits
                    npc.timeLeft = NPC.activeTime;

                    // Add world border collision for ALL trial bosses
                    float worldWidth = Main.maxTilesX * 16;
                    float worldHeight = Main.maxTilesY * 16;

                    // Keep within horizontal bounds
                    if (npc.position.X < 32)
                        npc.position.X = 32;
                    if (npc.position.X + npc.width > worldWidth - 32)
                        npc.position.X = worldWidth - npc.width - 32;

                    // Keep within vertical bounds
                    if (npc.position.Y < 32)
                        npc.position.Y = 32;
                    if (npc.position.Y + npc.height > worldHeight - 32)
                        npc.position.Y = worldHeight - npc.height - 32;

                    // Apply modifier effects to trial bosses
                    ApplyModifierEffectsToBoss(npc);

                    // Custom AI behaviors for trial bosses
                    switch (npc.type)
                    {
                        case NPCID.EyeofCthulhu:
                            // Enhanced Eye AI for arena combat
                            TrialEyeAI(npc);
                            break;
                        case NPCID.SkeletronHead:
                            TrialSkeletronAI(npc);
                            break;
                        case NPCID.QueenBee:
                            // Enhanced Queen Bee AI for arena combat
                            TrialQueenBeeAI(npc);
                            break;
                    }
                }
            }

            private static void ApplyModifierEffectsToBoss(NPC npc)
            {
                foreach (var modifier in TrialSystem.ActiveModifiers)
                {
                    switch (modifier)
                    {
                        case TrialModifier.DoubleMinions:
                            // Double the frequency of minion spawns (handled in individual AI methods)
                            break;
                        case TrialModifier.RapidAttacks:
                            // Increase attack frequency (handled in individual AI methods)
                            break;
                        case TrialModifier.BossRegen:
                            // Boss regenerates health over time
                            if (Main.GameUpdateCount % 60 == 0) // Every second
                            {
                                int regenAmount = (int)(npc.lifeMax * 0.01f); // 1% of max health per second
                                npc.life = Math.Min(npc.life + regenAmount, npc.lifeMax);
                            }
                            break;
                        case TrialModifier.KnockbackMaster:
                            // Increased knockback (handled in ModifyHitPlayer)
                            break;
                    }
                }
            }

            public override void ModifyHitPlayer(NPC npc, Player target, ref Player.HurtModifiers modifiers)
            {
                if (IsTrialBoss)
                {
                    // Apply modifier effects that affect player damage
                    foreach (var modifier in TrialSystem.ActiveModifiers)
                    {
                        switch (modifier)
                        {
                            case TrialModifier.KnockbackMaster:
                                // Increase knockback on hit
                                modifiers.Knockback.Flat += 6f;
                                break;
                            case TrialModifier.PlayerWeakness:
                                // Increase damage taken by player
                                modifiers.FinalDamage *= 1.5f;
                                break;
                            case TrialModifier.SpeedBoost:
                                // Increase damage taken due to speed boost
                                modifiers.FinalDamage *= 1.2f;
                                break;
                        }
                    }
                }
            }

            public override void OnHitPlayer(NPC npc, Player target, Player.HurtInfo hurtInfo)
            {
                if (!IsTrialBoss)
                    return;

                if (TrialSystem.ActiveModifiers.Contains(TrialModifier.KnockbackMaster))
                {
                    Vector2 direction = (target.Center - npc.Center).SafeNormalize(Vector2.UnitY);
                    target.velocity += direction * 15f;
                }
            }
        }
    }
}

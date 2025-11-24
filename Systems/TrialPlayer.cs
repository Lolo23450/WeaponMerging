using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameInput;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace WeaponMerging.Systems
{
    public class TrialPlayer : ModPlayer
    {
        public TrialStats TrialStats { get; private set; }

        public override void Initialize()
        {
            TrialStats = new TrialStats();
        }

        public override void SaveData(TagCompound tag)
        {
            if (TrialStats == null)
            {
                TrialStats = new TrialStats();
            }

            tag["TotalTrialsCompleted"] = TrialStats.TotalTrialsCompleted;
            tag["CurrentStreak"] = TrialStats.CurrentStreak;
            tag["HighestDifficulty"] = TrialStats.HighestDifficulty;
            tag["UnlockedTitles"] = TrialStats.UnlockedTitles ?? new List<string>();
            tag["TrialTokens"] = TrialStats.TrialTokens;

            var bossTimesTag = new TagCompound();
            if (TrialStats.BossBestTimes != null)
            {
                foreach (var kvp in TrialStats.BossBestTimes)
                {
                    bossTimesTag[kvp.Key] = kvp.Value;
                }
            }
            tag["BossBestTimes"] = bossTimesTag;

            var modifierTag = new TagCompound();
            if (TrialStats.ModifierCompletions != null)
            {
                foreach (var kvp in TrialStats.ModifierCompletions)
                {
                    modifierTag[kvp.Key.ToString()] = kvp.Value;
                }
            }
            tag["ModifierCompletions"] = modifierTag;
        }

        public override void LoadData(TagCompound tag)
        {
            if (TrialStats == null)
            {
                TrialStats = new TrialStats();
            }

            TrialStats.TotalTrialsCompleted = tag.GetInt("TotalTrialsCompleted");
            TrialStats.CurrentStreak = tag.GetInt("CurrentStreak");
            TrialStats.HighestDifficulty = tag.GetInt("HighestDifficulty");

            if (tag.ContainsKey("BossBestTimes"))
            {
                var bossTimesTag = tag.GetCompound("BossBestTimes");
                TrialStats.BossBestTimes = new Dictionary<string, int>();
                foreach (var kvp in bossTimesTag)
                {
                    TrialStats.BossBestTimes[kvp.Key] = Convert.ToInt32(kvp.Value);
                }
            }
            else
            {
                TrialStats.BossBestTimes = new Dictionary<string, int>();
            }

            if (tag.ContainsKey("ModifierCompletions"))
            {
                var modifierTag = tag.GetCompound("ModifierCompletions");
                TrialStats.ModifierCompletions = new Dictionary<TrialModifier, int>();
                foreach (var kvp in modifierTag)
                {
                    if (Enum.TryParse(kvp.Key, out TrialModifier modifier))
                    {
                        TrialStats.ModifierCompletions[modifier] = Convert.ToInt32(kvp.Value);
                    }
                }
            }
            else
            {
                TrialStats.ModifierCompletions = new Dictionary<TrialModifier, int>();
            }

            if (tag.ContainsKey("UnlockedTitles"))
            {
                TrialStats.UnlockedTitles = tag.Get<List<string>>("UnlockedTitles");
            }
            else
            {
                TrialStats.UnlockedTitles = new List<string>();
            }

            TrialStats.TrialTokens = tag.GetInt("TrialTokens");
        }

        public override void ProcessTriggers(TriggersSet triggersSet)
        {
            if (!TrialSystem.IsModifierSelectionActive)
            {
                return;
            }

            if (WasKeyJustPressed(Keys.D0) || WasKeyJustPressed(Keys.NumPad0))
            {
                TrialSystem.HandleModifierSelection(0);
            }
            else if (WasKeyJustPressed(Keys.D1) || WasKeyJustPressed(Keys.NumPad1))
            {
                TrialSystem.HandleModifierSelection(1);
            }
            else if (WasKeyJustPressed(Keys.D2) || WasKeyJustPressed(Keys.NumPad2))
            {
                TrialSystem.HandleModifierSelection(2);
            }
            else if (WasKeyJustPressed(Keys.D3) || WasKeyJustPressed(Keys.NumPad3))
            {
                TrialSystem.HandleModifierSelection(3);
            }
        }

        private static bool WasKeyJustPressed(Keys key)
        {
            return Main.keyState.IsKeyDown(key) && !Main.oldKeyState.IsKeyDown(key);
        }

        public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource)
        {
            if (TrialSystem.IsTrialActive)
            {
                // End trial on death
                TrialSystem.EndTrial(Player, false);
            }
        }

        public override bool CanUseItem(Item item)
        {
            if (TrialSystem.IsTrialActive && TrialSystem.ActiveModifiers.Contains(TrialModifier.NoHealing))
            {
                if (item.healLife > 0 || item.healMana > 0 || item.potion)
                {
                    Main.NewText("Healing is disabled during this trial!", Color.Red);
                    return false;
                }
            }

            return base.CanUseItem(item);
        }
    }
}

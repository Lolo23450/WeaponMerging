using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace WeaponMerging.Content.Players
{
    public class OrbManaPlayer : ModPlayer
    {
        private const int BaseMaxUnits = 6;
        private const int RegenIntervalTicks = 55;
        private const int RegenCooldownTicks = 90;

        private int regenTimer;
        private int regenCooldown;

        public int OrbManaCurrent { get; private set; }
        public int OrbManaMax { get; private set; }
        public int BonusMaxUnits { get; private set; }

        public bool IsOnCooldown => regenCooldown > 0;
        public float RegenProgress => OrbManaCurrent >= OrbManaMax ? 1f : regenCooldown > 0 ? 0f : regenTimer / (float)RegenIntervalTicks;

        public override void Initialize()
        {
            OrbManaMax = BaseMaxUnits;
            OrbManaCurrent = OrbManaMax;
        }

        public override void SaveData(TagCompound tag)
        {
            tag["orbMana"] = OrbManaCurrent;
            tag["orbManaMax"] = OrbManaMax;
        }

        public override void LoadData(TagCompound tag)
        {
            OrbManaMax = tag.GetInt("orbManaMax");
            if (OrbManaMax <= 0)
                OrbManaMax = BaseMaxUnits;
            OrbManaCurrent = Utils.Clamp(tag.GetInt("orbMana"), 0, OrbManaMax);
        }

        public override void ResetEffects()
        {
            BonusMaxUnits = 0;
            OrbManaMax = BaseMaxUnits;
        }

        public override void PostUpdate()
        {
            OrbManaMax = BaseMaxUnits + BonusMaxUnits;
            if (OrbManaCurrent > OrbManaMax)
            {
                OrbManaCurrent = OrbManaMax;
            }

            if (regenCooldown > 0)
            {
                regenCooldown--;
                regenTimer = 0;
                return;
            }

            if (OrbManaCurrent >= OrbManaMax)
            {
                regenTimer = 0;
                return;
            }

            regenTimer++;
            if (regenTimer >= RegenIntervalTicks)
            {
                regenTimer = 0;
                OrbManaCurrent++;
            }
        }

        public override void OnRespawn()
        {
            OrbManaCurrent = OrbManaMax;
            regenCooldown = 0;
            regenTimer = 0;
        }

        public void AddBonusMaxUnits(int amount)
        {
            BonusMaxUnits += amount;
        }

        public void RestoreOrbMana(int amount)
        {
            if (amount <= 0)
                return;
            OrbManaCurrent = Utils.Clamp(OrbManaCurrent + amount, 0, OrbManaMax);
            if (OrbManaCurrent == OrbManaMax)
            {
                regenCooldown = 0;
                regenTimer = 0;
            }
        }

        public bool TrySpendOrbMana(int amount, Player player)
        {
            if (amount <= 0)
                return true;

            if (OrbManaCurrent < amount)
            {
                if (player.whoAmI == Main.myPlayer)
                {
                    CombatText.NewText(player.getRect(), Color.CornflowerBlue, "Not enough orb energy!");
                    SoundEngine.PlaySound(SoundID.MenuClose, player.Center);
                }
                return false;
            }

            Spend(amount);
            return true;
        }

        private void Spend(int amount)
        {
            OrbManaCurrent = Utils.Clamp(OrbManaCurrent - amount, 0, OrbManaMax);
            regenCooldown = RegenCooldownTicks;
            regenTimer = 0;
        }
    }
}

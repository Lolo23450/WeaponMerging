using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponMerging.Content
{
	public class WeaponMergingMenu : ModMenu
	{
		public override int Music => MusicLoader.GetMusicSlot(Mod, "Assets/Music/MenuMusic");

		public override string DisplayName => "Weapon Merging Menu";
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Terraria.Graphics.Effects;

namespace WeaponMerging
{
	
	public class WeaponMerging : Mod
	{
		private const string FusionRuinsSkyKey = "WeaponMerging:FusionRuinsSky";
		private static bool _fusionSkyRegistered;

		public override void Load()
		{
			if (Main.dedServ || Main.instance == null)
			{
				return;
			}

			var skyManager = SkyManager.Instance;
			if (skyManager != null)
			{
				skyManager[FusionRuinsSkyKey] = new Content.Biomes.FusionRuinsSky();
				_fusionSkyRegistered = true;
			}
		}

		public override void Unload()
		{
			if (Main.dedServ)
			{
				return;
			}

			if (!_fusionSkyRegistered)
			{
				return;
			}

			var skyManager = SkyManager.Instance;
			try
			{
				if (skyManager != null)
				{
					if (skyManager[FusionRuinsSkyKey] != null)
					{
						skyManager[FusionRuinsSkyKey] = null;
					}
				}
			}
			catch
			{
				}
			finally
			{
				_fusionSkyRegistered = false;
			}
		}
	}
}


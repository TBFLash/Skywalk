using HarmonyLib;

namespace TBFlash.Skywalk
{
	[HarmonyPatch(typeof(TrashBag))]
	[HarmonyPatch("Changed")]
	public static class TrashBag_Changed
	{
		private static readonly AccessTools.FieldRef<TrashBag, POSprite> spriteRef = AccessTools.FieldRefAccess<POSprite>(typeof(TrashBag), "sprite");

		private static bool Prefix(TrashBag __instance, out TrashBag __state, IPrefab us)
		{
			__state = __instance;
			return true;
		}

		private static void Postfix(TrashBag __state, IPrefab us)
		{
			POSprite sprite = spriteRef(__state);
			if (sprite.layerMask == 1179648)
			{
				sprite.sortOrder = -DepthSort.Z(sprite.worldPos.y, -1);
			}
		}
	}
}

using HarmonyLib;
using TerrainTools;

namespace TBFlash.Skywalk
{
	[HarmonyPatch(typeof(ConveyorTool))]
	[HarmonyPatch("ValidateCell")]
	public static class ConveyorTool_ValidateCell
	{
		private static bool Prefix(ref bool __result, Cell cell, PlacementValidator validator, bool allowWall = false)
		{
			__result = true;
			if (cell == null || cell.immutable || cell.isFence || cell.isWindow)
			{
				validator.Error(i18n.Get("UI.strings.functionality.InvalidPlacement", ""));
				__result = false;
			}
			if (cell?.isWall == true && !allowWall)
			{
				validator.Error(i18n.Get("UI.strings.functionality.InvalidPlacement", ""));
				__result = false;
			}
			// changed from cell.below != null && !cell.below.indoors to test if the cell is indoors as the cell below can be null.
			if (UILevelSelector.CURRENT_FLOOR > 0 && cell?.indoors == false && cell?.FutureMaterial()!= Foundation.Concrete)
			{
				validator.Error(i18n.Get("UI.strings.functionality.ReqFoundationBelow", ""));
				__result = false;
			}
			if (cell?.placeableObj != null)
			{
				validator.Error(i18n.Get("UI.strings.functionality.InvalidPlacement", ""));
				__result = false;
			}
			if (cell?.roadNode != null)
			{
				validator.Error(i18n.Get("UI.strings.functionality.InvalidPlacement", ""));
				__result = false;
			}
			if (UILevelSelector.CURRENT_FLOOR == 0 && cell != null && Game.current.Map().extrudedRoadNodes[cell.x, cell.y])
			{
				validator.Error(string.Format("{0} ({1})", i18n.Get("UI.strings.functionality.ProximityObject", ""), i18n.Get("UI.tools.Taxiway.name", "")));
				__result = false;
			}
			return false;
		}
	}
}

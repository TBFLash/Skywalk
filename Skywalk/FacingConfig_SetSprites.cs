using UnityEngine;
using HarmonyLib;
using SimAirport.Logging;

namespace TBFlash.Skywalk
{
	[HarmonyPatch(typeof(FacingConfig))]
	[HarmonyPatch("SetSprites")]
	public static class FacingConfig_SetSprites
	{
		private static bool Prefix(FacingConfig __instance, PlaceableObject po, bool isFacingChanged)
		{
			int layerMaskCheck = 0;
			Color white = Color.white;
			if (!Application.isPlaying)
				layerMaskCheck = 1 << UILevelSelector.LevelToLayer(0);
			else if (!po.isFootprintSet)
				Debug.LogError("Footprint should always be set prior to SetSprites to determine if object is indoors or not.");
			else
			{
				layerMaskCheck = 1 << UILevelSelector.GetGOLayer(po.footprintV3I_0, false);
				Cell cell;
				if (!po.isPlaced || ((cell = Cell.At(po.footprintV3I_0, false)) != null && cell.indoors))
					white.a = 0.5f;
			}
			float rtFloorPoint = po.rt_floorPoint();
			float worldRotation = po.rt_Rotation();
			float sortOrderVar = 1f;
			List<SpriteConfig> sprites = __instance.sprites;
			for (int i = 0; i < sprites.Count; i++)
			{
				SpriteConfig spriteConfig = sprites[i];
				if (spriteConfig.sprite != null)
				{
					POSprite posprite;
					if (i < po.sprites.Count)
					{
						posprite = po.sprites[i];
						if (isFacingChanged)
							PORenderer.instance.Remove(posprite);
					}
					else
					{
						posprite = new POSprite();
						po.sprites.Add(posprite);
					}
					int layerMask = (spriteConfig.relativeLevel == 0) ? layerMaskCheck : UILevelSelector.GetVisibilityMask(po.footprintV3I_0.z + spriteConfig.relativeLevel);
					posprite.sd = SpriteManager.GetSpriteDefinition(spriteConfig.sprite.name, true);
					posprite.layerMask = layerMask;
					posprite.worldPos = po.iprefab.position;
					posprite.localPosition = spriteConfig.localPosition;
					if (i > 0 && po.na?.depot != null && po.facing == PlaceableObject.Orientation.Right)
						posprite.localPosition += new Vector2(0f, 1f);
					posprite.worldRotation = worldRotation;
					posprite.localRotation = spriteConfig.localRotation;
					if (spriteConfig.sortingOrder == -1000f)
                    {
                        posprite.sortOrder = sortOrderVar;
                        if (po.MyZeroAllocName.Contains("Remote Bus Pickup") || po.MyZeroAllocName.Contains("ParkingLot") || po.MyZeroAllocName.Contains("Label"))
                            posprite.sortOrder = DepthSort.Order(rtFloorPoint - sortOrderVar, -1);
                        sortOrderVar += 0.01f;
                    }
                    /*else if (po.MyZeroAllocName == "Baggage Depot")
                    {
						if (posprite.sd.sprite.name.Contains("LugaggeSysConnector"))
							posprite.sortOrder = DepthSort.Order(rtFloorPoint - spriteConfig.sortingOrder, -1);
						else
							posprite.sortOrder = DepthSort.Order(rtFloorPoint - spriteConfig.sortingOrder, 0);
						TBFlash_Skywalk_Helpers.TBFlashLogger(Log.FromPool(string.Format("Baggage Depot 1: Sprite: {0}; posprite.sortOrder: {1}", posprite.sd.sprite.name, posprite.sortOrder)));
					}*/
                    else if (po.gameObject.layer == 17 && po.gameObject.activeInHierarchy && (!Cell.At(po.footprintV3I_0)?.indoors ?? true) && po.footprintV3I_0.z >= 0 && spriteConfig.relativeLevel == 0 && (!(po.MyZeroAllocName.Contains("Aircraft Gate") || po.MyZeroAllocName == "ATC Tower" || po.MyZeroAllocName.Contains("Hangar") || po.MyZeroAllocName.Contains("Fuel Depot") || po.MyZeroAllocName.Contains("Fuel Tank") || po.MyZeroAllocName.Contains("Fueling Station") || po.MyZeroAllocName.Contains("Road Ramp") || po.MyZeroAllocName.Contains("Fuel Port") || po.MyZeroAllocName.Contains("Small Gate Stairs"))))
                        posprite.sortOrder = DepthSort.Order(rtFloorPoint - spriteConfig.sortingOrder, -1);
                    else if (po.MyZeroAllocName.Contains("Road Ramp") && ((posprite.sd.sprite.name == "Tunnel_Entrance" && po.roadRamp.EntranceV3I.z == 0) || (posprite.sd.sprite.name == "Tunnel_Exit" && po.roadRamp.ExitV3I.z == 0)))
                        posprite.sortOrder = DepthSort.Order(rtFloorPoint - spriteConfig.sortingOrder, -1);
                    else
                        posprite.sortOrder = DepthSort.Order(rtFloorPoint - spriteConfig.sortingOrder, 0);
                    posprite.color = white;
					posprite.flipX = (po.rt_Scale() == -1f);
					if (posprite.index == -1)
						PORenderer.instance.Add(posprite);
				}
			}
			int j = sprites.Count;
			while (j < po.sprites.Count)
			{
				POSprite posprite2 = po.sprites[j];
				if (posprite2.index == -1)
					break;
				PORenderer.instance.Remove(posprite2);
				int index = po.sprites.Count - 1;
				po.sprites[j] = po.sprites[index];
				po.sprites[index] = posprite2;
				po.sprites.RemoveAt(index);
			}
			return false;
		}
	}
}

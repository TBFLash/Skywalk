using UnityEngine;
using HarmonyLib;
using System.Reflection;

namespace TBFlash.Skywalk
{
    [HarmonyPatch(typeof(PlacementValidator))]
    [HarmonyPatch("Validate_Cell")]
    public static class PlacementValidator_Validate_Cell
    {
        private static readonly AccessTools.FieldRef<PlacementValidator, PlaceableObject> placeableObjectRef = AccessTools.FieldRefAccess<PlaceableObject>(typeof(PlacementValidator), "placeableObject");

        private static bool Prefix(PlacementValidator __instance, out PlacementValidator __state, Cell cell, bool skipConstructionValidate = false, bool skipWallCheck = false)
        {
            __state = __instance;
            return true;
        }
        private static void Postfix(PlacementValidator __state, Cell cell, bool skipConstructionValidate = false, bool skipWallCheck = false)
        {
            //Type theType = __state.GetType();
            //var fieldInfo = theType.GetField("placeableObject", BindingFlags.Instance | BindingFlags.NonPublic);
            //PlaceableObject po = (PlaceableObject)fieldInfo.GetValue(__state);
            PlaceableObject po = placeableObjectRef(__state);

            if (po != null && (po.aircraftGate != null || po.MyZeroAllocName == "ATC Tower" || po.MyZeroAllocName.Contains("Hangar") || po.MyZeroAllocName.Contains("Fuel Depot") ||(po.MyZeroAllocName.Contains("Fuel Tank") && !po.MyZeroAllocName.Contains("Underground")) || po.MyZeroAllocName.Contains("Fueling Station") || po.MyZeroAllocName.Contains("Platform")))
            {
                for (int z=1; z<=2; z++)
                {
                    Cell testingCell = Cell.At(new Vector3Int(cell.x, cell.y, z));
                    if(testingCell.constructionMaterial != null || testingCell.isPendingConstruction)
                    {
                        __state.Error(i18n.Get("TBFlash.Skywalk.floorAboveProximity", ""));
                        return;
                    }
                }
            }
        }
    }
}

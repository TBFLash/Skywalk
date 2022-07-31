using SimAirport.Logging;
using Modding;
using TerrainTools;
using System.Reflection;

namespace TBFlash.Skywalk
{
    internal static class TBFlash_SkywalkLoader
    {
        private static void SetPrivateFieldValue(object obj!!, string propName, String val)
        {
            Type theType = obj.GetType();
            FieldInfo? fieldInfo = null;
            while (fieldInfo == null && theType != null)
            {
                fieldInfo = theType.GetField(propName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                theType = theType.BaseType;
            }
            if (fieldInfo == null) throw new ArgumentOutOfRangeException(nameof(propName), string.Format("Field {0} was not found in Type {1}", propName, obj.GetType().FullName));
            fieldInfo.SetValue(obj, val);
        }
        public static bool Loader()
        {
            bool loaded = false;
            TBFlash_Skywalk.TBFlashLogger(Log.FromPool("!loaded && Game.isLoaded").WithCodepoint());
            TBFlash_Skywalk _Skywalk = UnityEngine.ScriptableObject.CreateInstance<TBFlash_Skywalk>();
            if (_Skywalk != null)
            {
                TBFlash_Skywalk.TBFlashLogger(Log.FromPool("_Skywalk != null").WithCodepoint());
                TBFlash_Skywalk.TBFlashLogger(Log.FromPool("All_sprites has " + SpriteManager.all_sprites.Count + " Items.").WithCodepoint());
                _Skywalk.userVisible = true;
                _Skywalk.displayOrder = 4;

                try
                {
                    if (!SpriteManager.TryGet("SkywalkIcon", out _Skywalk.icon))
                    {
                        TBFlash_Skywalk.TBFlashLogger(Log.FromPool("Did not load icon").WithCodepoint());
                        SpriteManager.TryGet("FoundationIcons", out _Skywalk.icon);
                    }
                }
                catch (Exception ex)
                {
                    TBFlash_Skywalk.TBFlashLogger(Log.FromPool("Error loading Icons" + ex.ToString()));
                }
                _Skywalk.requiresIndoors = false;
                _Skywalk.excludeWWF = false;
                _Skywalk.requiresOutdoors = true;
                _Skywalk.OnlyStraightLines = false;
                _Skywalk.enableVisualEndpoints = false;
                _Skywalk.Level2u = false;
                _Skywalk.Level1u = false;
                _Skywalk.Level0 = false;
                _Skywalk.Level1 = true;
                _Skywalk.Level2 = true;
                _Skywalk.i18nNameKey = "i18n Skywalk";
                _Skywalk.i18nDescKey = "i18n Desc";
                _Skywalk.name = "Skywalk";
                SetPrivateFieldValue(_Skywalk, "_i18nNameKey", "TBFlash.Skywalk.name");
                SetPrivateFieldValue(_Skywalk, "_i18nDescKey", "TBFlash.Skywalk.description");
                TBFlash_Skywalk.Reset();

                if (TerrainToolHandler.Instance.ModdedTerrainTools.Any((TerrainTool t) => t.name == _Skywalk.name))
                {
                    TBFlash_Skywalk.TBFlashLogger(Log.FromPool("Could not load Skywalk, already loaded a tool with the name \"" + _Skywalk.name + "\"!"));
                }
                else
                {
                    TBFlash_Skywalk.TBFlashLogger(Log.FromPool("Loading _Skywalk into ModdedTerrainTools").WithCodepoint());
                    TerrainToolHandler.Instance.ModdedTerrainTools.Add(_Skywalk);
                    if (TBFlash_Skywalk.isTBFlashDebug && TerrainToolHandler.Instance.ModdedTerrainTools.Any((TerrainTool t) => t.name == _Skywalk.name))
                        TBFlash_Skywalk.TBFlashLogger(Log.FromPool("Loaded TerrainTool \"" + _Skywalk.name + "\" into ModdedTerrainTools").WithCodepoint());
                }
                try
                {
                    typeof(TerrainTool).GetMethod("LoadTool", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, new object[] { _Skywalk });
                    TBFlash_Skywalk.TBFlashLogger(Log.FromPool("Added " + _Skywalk.name + " to TerrainTool.Tools").WithCodepoint());
                    loaded = true;
                }
                catch (Exception ex)
                {
                    TBFlash_Skywalk.TBFlashLogger(Log.FromPool("did not add " + _Skywalk.name + " to TerrainTool.Tools. Exception = " + ex.ToString()).WithCodepoint());
                }
            }
            return loaded;
        }
    }
}

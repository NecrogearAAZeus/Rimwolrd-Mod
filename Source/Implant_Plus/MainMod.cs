using Verse;
using HarmonyLib;

namespace Implant_Plus
{
    public class ImplantPlusMod : Mod
    {
        public ImplantPlusMod(ModContentPack content) : base(content)
        {
            var harmony = new Harmony("ImplantPlus.Mod");
            harmony.PatchAll();
        }
    }
}

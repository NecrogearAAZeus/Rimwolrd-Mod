// using RimWorld;
// using HarmonyLib;
// using Verse;
// using System.Linq;

// namespace Implant_Plus.StatOffset
// {
//    [HarmonyPatch(typeof(BodyPartDef), "GetMaxHealth")]
//     public static class Patch_GetMaxHealth
//     {
//         static void Postfix(BodyPartDef __instance, Pawn pawn, ref float __result)
//         {
//             if (pawn == null || __instance == null) return;

//             // 이 Pawn의 살아있는 BodyPartRecord 중 현재 BodyPartDef와 일치하는 실제 부위를 찾음
//             var realPart = pawn.health.hediffSet.GetNotMissingParts()
//                 .FirstOrDefault(p => p.def == __instance);

//             if (realPart == null) return;

//             // 해당 부위에 Phantom Reaper가 장착되어 있는지 확인
//             var matchPhantom = pawn.health.hediffSet.hediffs
//                 .OfType<Hediff_AddedPart>()
//                 .FirstOrDefault(h =>
//                     h.def.defName == "IP_ORD02_PHANTOM_REAPER" &&
//                     h.Part == realPart);

//             if (matchPhantom != null)
//             {
//                 __result = 15f; // 해당 부위의 최대 체력을 15으로 고정
//             }
//         }
//     }
// }
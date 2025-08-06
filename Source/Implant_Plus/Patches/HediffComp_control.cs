using Verse;
using RimWorld;
using HarmonyLib;
using System.Linq;

namespace Implant_Plus.Patches
{

    [HarmonyPatch(typeof(Pawn_EquipmentTracker), "TryEquip")]
    public static class Pawn_EquipmentTracker_TryEquip_Patch
    {
        public static bool Prefix(Pawn_EquipmentTracker __instance, ThingWithComps newEq)
        {
            var pawn = __instance.pawn;

            // 무기 장착 금지 임플란트 목록
            var weaponDisablingImplants = new string[]
            {
                "IP_ORD_02_Phantom_Reaper",  // 팬텀 리퍼
                // 다른 임플란트들도 여기에 추가 가능
            };

            // 해당 임플란트가 있는지 확인
            bool hasDisablingImplant = pawn.health.hediffSet.hediffs
                .Any(h => weaponDisablingImplants.Contains(h.def.defName));

            if (hasDisablingImplant)
            {
                // 무기 장착 차단 메시지
                Messages.Message(
                    $"{pawn.LabelShort}의 {pawn.health.hediffSet.hediffs.FirstOrDefault(h => weaponDisablingImplants.Contains(h.def.defName))?.def.label}로 인해 무기를 장착할 수 없습니다.",
                    pawn,
                    MessageTypeDefOf.RejectInput
                );

                return false; // 장착 차단
            }

            return true; // 정상 진행
        }
    }
    // HediffComp: 임플란트 설치 시 기존 무기 제거
    public class HediffCompProperties_WeaponDrop : HediffCompProperties
    {
        public string dropMessage = "특수 임플란트로 인해 무기를 장착할 수 없습니다.";

        public HediffCompProperties_WeaponDrop()
        {
            compClass = typeof(HediffComp_WeaponDrop);
        }
    }

    public class HediffComp_WeaponDrop : HediffComp
    {
        public HediffCompProperties_WeaponDrop Props => 
            (HediffCompProperties_WeaponDrop)props;

        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);
            
            // 임플란트 설치 시 기존 무기 제거
            if (Pawn.equipment?.Primary != null)
            {
                var droppedWeapon = Pawn.equipment.Primary;
                
                if (Pawn.equipment.TryDropEquipment(
                    Pawn.equipment.Primary, 
                    out ThingWithComps resultingWeapon, 
                    Pawn.Position))
                {
                    // 메시지 표시
                    Messages.Message(
                        $"{Pawn.LabelShort}이(가) {droppedWeapon.Label}을(를) 떨어뜨렸습니다. {Props.dropMessage}",
                        Pawn, 
                        MessageTypeDefOf.NeutralEvent
                    );
                }
            }
        }
    }
}


    // [HarmonyPatch(typeof(HediffSet), "GetPartHealth")]
    // public static class Patch_HediffSet_GetPartHealth_MaxLimit
    // {
    //     static void Postfix(HediffSet __instance, BodyPartRecord part, ref float __result)
    //     {
    //         if (__instance.pawn?.health == null || part == null)
    //             return;

    //         // 특정 임플란트가 있는지 확인
    //         foreach (var hediff in __instance.hediffs)
    //         {
    //             if (hediff.Part == part && 
    //                 hediff.def.defName == "IP_ORD_02_Phantom_Reaper" && 
    //                 __result > 15f)
    //             {
    //                 __result = 15f; // 체력을 15로 제한
    //                 return;
    //             }
    //         }
    //     }
    // }
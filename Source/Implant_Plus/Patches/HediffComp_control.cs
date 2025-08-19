using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using RimWorld;
using HarmonyLib;

namespace Implant_Plus.Control
{
    // 임플란트 설치 시 무기 드롭
    [HarmonyPatch(typeof(Recipe_InstallImplant), "ApplyOnPawn")]
    public static class DropWeaponOnImplant_Patch
    {
        static void Postfix(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
            //  수술 대상이 Phantom Reaper가 아닐 경우 종료
            if (!pawn.health.hediffSet.HasHediff(HediffDef.Named("IP_ORD02_PHANTOM_REAPER")))
            {
                return;
            }
            var phantomReaper = pawn.health.hediffSet.hediffs
                .FirstOrDefault(h => 
                    h.def.defName == "IP_ORD02_PHANTOM_REAPER" &&
                    h.Part == part);
            if (phantomReaper == null)
            {
                return;
            }
            if (pawn.equipment?.Primary != null)
            {
                if (pawn.MapHeld == null || !pawn.Spawned)
                {
                    return;
                }
                ThingWithComps weapon = pawn.equipment.Primary;
                // 장비에서 강제로 제거
                pawn.equipment.Remove(weapon);
                // 현재 위치에 떨군다
                bool placed = GenPlace.TryPlaceThing(
                    weapon,
                    pawn.Position,
                    pawn.Map,
                    ThingPlaceMode.Near);
                if (placed)
                {
                    weapon.SetForbidden(true, warnOnFail: false);
                }
            }
        }
    }

    // FloatMenu 수정, 특정 임플란트 장착시 무기 장비 방지.
    [HarmonyPatch(typeof(FloatMenuMakerMap), "GetOptions")]
    public static class PreventEquipFloatMenu_Patch
    {
        static void Postfix(List<Pawn> selectedPawns, Vector3 clickPos, ref List<FloatMenuOption> __result, FloatMenuContext context)
        {
            if (selectedPawns == null || !selectedPawns.Any()) return;
            
            Pawn pawn = selectedPawns.FirstOrDefault();
            if (pawn?.health?.hediffSet?.HasHediff(DefDatabase<HediffDef>.GetNamed("IP_ORD02_PHANTOM_REAPER")) != true)
                return;

            // 클릭한 위치의 셀 확인
            IntVec3 cell = IntVec3.FromVector3(clickPos);
            if (!cell.InBounds(pawn.Map)) return;

            // 해당 셀의 모든 아이템 확인
            List<Thing> thingsAtCell = cell.GetThingList(pawn.Map);
            
            foreach (Thing thing in thingsAtCell)
            {
                // 무기인지 확인
                if (thing.def.IsWeapon)
                {
                    // 기존 장비 관련 옵션들을 찾아서 수정
                    for (int i = __result.Count - 1; i >= 0; i--)
                    {
                        FloatMenuOption option = __result[i];
                        if (option.Label != null && 
                            (option.Label.StartsWith("Equip ") || 
                             option.Label.Contains("장비") ||
                             option.Label.Contains(thing.Label)))
                        {
                            // 기존 옵션 제거
                            __result.RemoveAt(i);
                            
                            // 새로운 비활성화된 옵션 추가
                            __result.Insert(i, new FloatMenuOption(
                                $"Equip {thing.Label} (Phantom Reaper 임플란트로 인해 불가)",
                                null,
                                MenuOptionPriority.DisabledOption
                            ));
                        }
                    }
                    
                    break; // 한 번만 처리
                }
            }
        }
    }

    // 백업 안전장치 혹시 모를 무기 장비 방지와 장비시 무기를 떨어트림
    [HarmonyPatch(typeof(Pawn_EquipmentTracker), "AddEquipment")]
    public class BlockWeaponAdd_Patch
    {
        static bool Prefix(Pawn_EquipmentTracker __instance, ThingWithComps newEq)
        {
            Pawn pawn = __instance.pawn;
            
            if (pawn.health?.hediffSet?.HasHediff(DefDatabase<HediffDef>.GetNamed("IP_ORD02_PHANTOM_REAPER")) == true)
            {
                // 무기를 땅에 떨어뜨림
                if (!GenPlace.TryPlaceThing(newEq, pawn.Position, pawn.Map, ThingPlaceMode.Near))
                {
                    Log.Error("Phantom Reaper: 무기를 떨어뜨릴 수 없습니다.");
                }
                
                Messages.Message(
                    "Phantom Reaper 임플란트로 인해 무기를 장비할 수 없습니다.", 
                    pawn, 
                    MessageTypeDefOf.RejectInput
                );
                
                return false;
            }
            
            return true;
        }
    }

   [HarmonyPatch(typeof(Pawn_GeneTracker), "get_AffectedByDarkness")]
    public class DarkVisionPatch
    {
        static bool Postfix(bool result, Pawn_GeneTracker __instance)
        {
            if (__instance.pawn.health?.hediffSet?.HasHediff(DefDatabase<HediffDef>.GetNamed("IP_ICD_23_NIGHTOWL")) == true)
            {
                return false;
            }
            return result;
        }
    }
}
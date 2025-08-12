using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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


    // 어둠에서 작업, 이동시 패널티 제거 
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

    // 어둠 무드 감소 제거
    [HarmonyPatch(typeof(ThoughtWorker_Dark), "CurrentStateInternal")]
    public class DarknessMoodPatch
    {
        static ThoughtState Postfix(ThoughtState result, Pawn p)
        {
            if (p.health?.hediffSet?.HasHediff(DefDatabase<HediffDef>.GetNamed("IP_ICD_23_NIGHTOWL")) == true)
            {
                return ThoughtState.Inactive;
            }
            return result;
        }
    }

    // Pawn의 장비 총무게 제어
    [HarmonyPatch(typeof(MassUtility), "Capacity")]
    public class ImplantCapacityBonus_Patch
    {
        static void Postfix(ref float __result, Pawn p, StringBuilder explanation = null)
        {
            if (!MassUtility.CanEverCarryAnything(p))
                return;

            // 다리 임플란트 목록 + 10KG
            var legImplantList = new HashSet<string>
            {
                "IP_IDC_2A_LOADER_GOLIATH",
                // 추후 추가할 다리 임플란트들
            };

            // 내장형 프레임 목록 + 15KG
            var innerFrameList = new HashSet<string>
            {
                "IP_CombatInnerFrame",
                "IP_WorkerInnerFrame",
                "IP_StandardInnerFrame",
                "IP_IDC_23_SURVIVOR",
                "IP_DAON_31_VAITALBOOST",
                "IP_ORD_25_EVOLUTION",
                "IP_IMSL_17_BASTION_SHIELD",
                // 추후 추가할 내장형 프레임들
            };

            int legImplantCount_5KG = 0;
            int innerFrameCount_15KG = 0;

            // 임플란트 개수 확인
            foreach (var hediff in p.health.hediffSet.hediffs)
            {
                if (legImplantList.Contains(hediff.def.defName))
                {
                    legImplantCount_5KG++;
                }
                else if (innerFrameList.Contains(hediff.def.defName))
                {
                    innerFrameCount_15KG++;
                }
            }

            // 보정치 계산
            float bonusWeight = 0f;
            bonusWeight += legImplantCount_5KG * 10f;      // 다리당 10kg
            bonusWeight += innerFrameCount_15KG * 15f;      // 프레임당 15kg

            // 새로운 공식: (기본무게 + 보정치) * 신체크기
            float baseWeight = 35f + bonusWeight;
            __result = baseWeight * p.BodySize;

            // 설명 추가
            if (explanation != null)
            {
                if (explanation.Length > 0)
                {
                    explanation.AppendLine();
                }

                explanation.Append($"  - {p.LabelShortCap}: {baseWeight.ToStringMassOffset()} x {p.BodySize:F1} = {__result.ToStringMassOffset()}");

                if (bonusWeight > 0f)
                {
                    explanation.AppendLine();
                    if (legImplantCount_5KG > 0)
                    {
                        explanation.Append($"    • Leg Implants: +{(legImplantCount_5KG * 10f).ToStringMassOffset()}");
                        explanation.AppendLine();
                    }
                    if (innerFrameCount_15KG > 0)
                    {
                        explanation.Append($"    • Inner Frames: +{(innerFrameCount_15KG * 15f).ToStringMassOffset()}");
                    }
                }
            }
        }
    }
}
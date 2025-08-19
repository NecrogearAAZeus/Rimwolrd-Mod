using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Verse.AI;
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
            float darkSightValue = __instance.pawn.GetStatValue(DefDatabase<StatDef>.GetNamed("IP_DarkSight"));
            if (darkSightValue >= 1f)
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
            float darkSightValue = p.GetStatValue(DefDatabase<StatDef>.GetNamed("IP_DarkSight"));
            if (darkSightValue >= 1f)
            {
                return ThoughtState.Inactive;
            }
            return result;
        }
    }

    [HarmonyPatch(typeof(StatWorker), "GetValueUnfinalized")]
    public class FoodPoisonChanceStatPatch
    {
        static void Postfix(StatRequest req, ref float __result, StatWorker __instance)
        {
            // FoodPoisonChance 스탯이고 폰이 있을 때만 적용
            // StatWorker.stat 대신 StatDef 직접 비교
            if (__instance.GetType() == typeof(StatWorker) && 
                req.HasThing && req.Thing is Pawn pawn)
            {
                // Reflection으로 stat 필드 접근
                var statField = typeof(StatWorker).GetField("stat", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                StatDef currentStat = (StatDef)statField.GetValue(__instance);
                
                if (currentStat == StatDefOf.FoodPoisonChance)
                {
                    float IP_CookingPosionChanceFactor = pawn.GetStatValue(DefDatabase<StatDef>.GetNamed("IP_CookingPoisonChanceFactor"));
                    __result *= IP_CookingPosionChanceFactor;
                }
            }
        }
    }

    // 아드레날린 부스트 트리거 시스템
    [HarmonyPatch(typeof(Thing), "TakeDamage")]
    public class AdrenalineBoostTriggerPatch
    {
        // 쿨다운 관리용 딕셔너리 (폰 ID -> 마지막 트리거 시간)
        public static Dictionary<int, int> lastTriggerTicks = new Dictionary<int, int>(); // private에서 public으로 변경
        private static readonly int stalker_COOLDOWN_TICKS = 120; // 2초 (60틱 = 1초)

        static void Postfix(DamageInfo dinfo, Thing __instance)
        {
            // 데미지를 받은 대상이 메카노이드가 아니고, 실제로 데미지를 입었는지 확인
            if (__instance is Pawn victim && 
                !victim.RaceProps.IsMechanoid && 
                dinfo.Amount > 0)
            {
                // 공격자가 있고, 폰인지 확인
                if (dinfo.Instigator is Pawn attacker)
                {
                    // IP_IMSL_SENTINEL_STALKER 임플란트가 있는지 확인
                    if (attacker.health?.hediffSet?.HasHediff(DefDatabase<HediffDef>.GetNamed("IP_IMSL_SENTINEL_STALKER")) == true)
                    {
                        // 쿨다운 체크
                        int stalker_CurrentTick = Find.TickManager.TicksGame;
                        int attackerId = attacker.thingIDNumber;
                        
                        if (!lastTriggerTicks.ContainsKey(attackerId) || 
                            stalker_CurrentTick - lastTriggerTicks[attackerId] >= stalker_COOLDOWN_TICKS)
                        {
                            // 아드레날린 부스트 적용
                            ApplyAdrenalineBoost(attacker);
                            
                            // 쿨다운 갱신
                            lastTriggerTicks[attackerId] = stalker_CurrentTick;
                        }
                    }
                }
            }
        }
        
        private static void ApplyAdrenalineBoost(Pawn pawn)
        {
            // 기존 아드레날린 부스트가 있는지 확인
            Hediff stalker_existingBoost = pawn.health.hediffSet.GetFirstHediffOfDef(DefDatabase<HediffDef>.GetNamed("IP_Adrenaline_Boost"));
            
            if (stalker_existingBoost != null)
            {
                // 기존 헤디프의 지속시간만 초기화
                HediffComp_Disappears disappearComp = stalker_existingBoost.TryGetComp<HediffComp_Disappears>();
                if (disappearComp != null)
                {
                    // 지속시간을 처음 상태로 리셋 - IntRange 직접 처리
                    HediffCompProperties_Disappears props = (HediffCompProperties_Disappears)disappearComp.props;
                    int ticksToSet;
                    
                    // disappearsAfterTicks가 IntRange인지 확인하고 처리
                    var disappearTicks = props.disappearsAfterTicks;
                    if (disappearTicks != null)
                    {
                        // IntRange에서 평균값 사용 (float를 int로 변환)
                        ticksToSet = (int)disappearTicks.Average;
                    }
                    else
                    {
                        // 기본값 설정 (예: 10초)
                        ticksToSet = 600;
                    }
                    
                    disappearComp.ticksToDisappear = ticksToSet;
                }
            }
            else
            {
                // 기존 헤디프가 없으면 새로 생성
                Hediff stalker_ResetBoost = HediffMaker.MakeHediff(DefDatabase<HediffDef>.GetNamed("IP_Adrenaline_Boost"), pawn);
                pawn.health.AddHediff(stalker_ResetBoost);
            }
            
        }
    }

    // 게임 종료 시 쿨다운 딕셔너리 정리
    [HarmonyPatch(typeof(Game), "DeinitAndRemoveMap")]
    public class AdrenalineBoostCleanupPatch
    {
        static void Postfix()
        {
            // 딕셔너리 정리하여 메모리 누수 방지
            AdrenalineBoostTriggerPatch.lastTriggerTicks?.Clear();
        }
    }
}
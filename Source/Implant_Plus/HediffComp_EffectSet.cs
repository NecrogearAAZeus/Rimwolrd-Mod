using Verse;
using RimWorld;
using System.Linq;

namespace Implant_Plus
{   
    // EMP 반응 처리
    public class HediffCompProperties_EMPReaction : HediffCompProperties
    {
        public HediffCompProperties_EMPReaction()
        {
            compClass = typeof(HediffComp_EMPReaction);
        }
    }

    public class HediffComp_EMPReaction : HediffComp
    {
        public HediffCompProperties_EMPReaction Props => (HediffCompProperties_EMPReaction)props;

        public override void Notify_PawnPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            base.Notify_PawnPostApplyDamage(dinfo, totalDamageDealt);

            // EMP 데미지인지 확인
            if (dinfo.Def != DamageDefOf.EMP) 
                return;

            // 1. 엑소슈트 Tier1 체크 및 처리
            if (HasInnerFrameTier1())
            {
                var exoEmpShockDef = DefDatabase<HediffDef>.GetNamedSilentFail("IP_InnerFrameEmpShockedMinor");
                if (exoEmpShockDef != null && !Pawn.health.hediffSet.HasHediff(exoEmpShockDef))
                {
                    Pawn.health.AddHediff(exoEmpShockDef);
                }
            }

            // 2. 뇌 이식물 체크 및 처리
            if (HasBrainImplantTier1())
            {
                // 바닐라 BrainShock이 없을 때만 약한 버전 적용
                var brainShockDef = DefDatabase<HediffDef>.GetNamedSilentFail("BrainShock");
                if (brainShockDef == null || !Pawn.health.hediffSet.HasHediff(brainShockDef))
                {
                    var brainShockMinorDef = DefDatabase<HediffDef>.GetNamedSilentFail("IP_BrainShockMinor");
                    if (brainShockMinorDef != null && !Pawn.health.hediffSet.HasHediff(brainShockMinorDef))
                    {
                        BodyPartRecord brain = Pawn.RaceProps.body.AllParts
                            .FirstOrDefault(part => part.def.defName == "Brain");

                        if (brain != null)
                        {
                            Pawn.health.AddHediff(brainShockMinorDef, brain);
                        }
                    }
                }
            }
        }

        private bool HasInnerFrameTier1()
        {
            var InnerFrameDefNames = new string[]
            {
                "IP_CombatInnerFrame",
                "IP_StandardInnerFrame",
                "IP_WorkerInnerFrame"
                // 추후 1티어 다른 엑소 스켈레톤 추가
            };
            
            return Pawn.health.hediffSet.hediffs
                .Any(h => InnerFrameDefNames.Contains(h.def.defName));
        }

        private bool HasBrainImplantTier1()
        {
            var brainImplantDefNames = new string[]
            {
                "IP_CombatAssistAI",
                "IP_FieldworkerAssistAI",
                "IP_CraftmasterAssistAI",
                "IP_CognitorAssistAI",
                // 다른 뇌 임플란트들 추가
            };
            
            return Pawn.health.hediffSet.hediffs
                .Any(h => brainImplantDefNames.Contains(h.def.defName));
        }
    }

    // 커스텀 HARMONY 전용 Hediff 적용

    public class AddDisplayHediff_IP_DAON_43_HARMONY : HediffCompProperties
    {
        public AddDisplayHediff_IP_DAON_43_HARMONY()
        {
            compClass = typeof(HediffComp_AddDisplayHARMONY_Hediff);
        }
    }

    public class HediffComp_AddDisplayHARMONY_Hediff : HediffComp
    {
        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);
            
            // 표시용 hediff 추가
            var displayHediff = HediffMaker.MakeHediff(
                DefDatabase<HediffDef>.GetNamed("IP_DAON_43_HARMONY_DisplayHediff"), 
                parent.pawn);
            
            parent.pawn.health.AddHediff(displayHediff);
        }

        public override void CompPostPostRemoved()
        {
            base.CompPostPostRemoved();
            
            // 표시용 hediff 제거
            var displayHediff = parent.pawn.health.hediffSet.GetFirstHediffOfDef(
                DefDatabase<HediffDef>.GetNamed("IP_DAON_43_HARMONY_DisplayHediff"));
            
            if (displayHediff != null)
            {
                parent.pawn.health.RemoveHediff(displayHediff);
            }
        }
    }

}
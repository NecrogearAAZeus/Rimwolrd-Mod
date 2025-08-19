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
                "IP_WorkerInnerFrame",
                "IP_IDC_23_SURVIVOR",
                "IP_DAON_31_VAITALBOOST",
                "IP_ORD_25_EVOLUTION",
                "IP_IMSL_BASTION_SHIELD",


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
                "IP_ORD_41_OVERSEER",
                "IP_DAON_43_HARMONY",
                "IP_DAON_44_APPRENTICE",
                "IP_IMSL_SENTINEL_OFFICER",

                // 다른 뇌 임플란트들 추가
            };
            
            return Pawn.health.hediffSet.hediffs
                .Any(h => brainImplantDefNames.Contains(h.def.defName));
        }
    }

}

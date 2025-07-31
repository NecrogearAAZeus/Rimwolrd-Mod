using Verse;
using RimWorld;

namespace Implant_Plus
{

    //뇌충격 있을 시 BrainShork(weak) 적용 방지

     public class HediffCompProperties_EMPReaction : HediffCompProperties
    {
        public string createHediff = "BrainShork_Weak";

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

            if (dinfo.Def.defName == "EMP")
            {
                LongEventHandler.QueueLongEvent(() =>
                {
                    // 1틱 뒤에 실행
                    if (!Pawn.health.hediffSet.HasHediff(DefDatabase<HediffDef>.GetNamed("BrainShock")))
                    {
                        BodyPartRecord brain = Pawn.RaceProps.body.AllParts
                            .Find(part => part.def.defName == "Brain");

                        if (brain != null)
                        {
                            var targetHediff = DefDatabase<HediffDef>.GetNamed(Props.createHediff);
                            Pawn.health.AddHediff(targetHediff, brain);
                            Log.Message($"[Implant_Plus] {Props.createHediff} applied to {Pawn.LabelShortCap} after EMP.");
                        }
                    }
                }, "EMPWeakCheck", false, null);
            }
        }
    }
}
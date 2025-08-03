// using Verse;
// using RimWorld;

// namespace Implant_Plus
// {
//     // 셀프 이식 임플란트 코드
    
//     public class CompProperties_UseEffectGiveHediff : CompProperties
//     {
//         public HediffDef hediffDef;
//         public BodyPartDef applyToPart;

//         public CompProperties_UseEffectGiveHediff()
//         {
//             this.compClass = typeof(CompUseEffect_GiveHediff);
//         }
//     }

//     public class CompUseEffect_GiveHediff : CompUseEffect
//     {
//         public new CompProperties_UseEffectGiveHediff Props => (CompProperties_UseEffectGiveHediff)this.props;

//         public override void DoEffect(Pawn user)
//         {
//             base.DoEffect(user);

//             // 비인간 제한
//             if (!user.RaceProps.Humanlike)
//             {
//                 Messages.Message("Only humanlike pawns can use this implant.", user, MessageTypeDefOf.RejectInput);
//                 return;
//             }

//             if (user.health.hediffSet.HasHediff(Props.hediffDef))
//             {
//                 Messages.Message($"{user.LabelShortCap} already has {Props.hediffDef.label}.", user, MessageTypeDefOf.RejectInput);
//                 return;
//             }

//             BodyPartRecord part = null;
//             if (Props.applyToPart != null)
//             {
//                 part = user.RaceProps.body.AllParts.Find(p => p.def == Props.applyToPart);
//             }
//             else
//             {
//                 part = user.RaceProps.body.AllParts.Find(p => p.def.defName == "Brain");
//             }

//             // 여기가 빠져있었어!
//             if (part != null)
//             {
//                 // 임플란트 추가
//                 Hediff hediff = HediffMaker.MakeHediff(Props.hediffDef, user, part);
//                 user.health.AddHediff(hediff, part);
                
//                 Messages.Message($"{user.LabelShortCap} successfully installed {Props.hediffDef.label}.", 
//                     user, MessageTypeDefOf.PositiveEvent);
//             }
//             else
//             {
//                 Messages.Message($"Could not find appropriate body part for {Props.hediffDef.label}.", 
//                     user, MessageTypeDefOf.RejectInput);
//             }
//         }
//     }
// }
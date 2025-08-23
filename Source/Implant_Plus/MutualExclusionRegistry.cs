using Verse;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;

namespace Implant_Plus
{
    // InnerFrames
    public static class InnerFrameManager
    {
        private static readonly List<string> AllInnerFrames = new List<string>
        {
            //Tier 1
            "IP_CombatInnerFrame",
            "IP_WorkerInnerFrame",
            "IP_StandardInnerFrame",
            "IP_IDC_23_SURVIVOR",
            "IP_DAON_31_VAITALBOOST",
            "IP_ORD_25_EVOLUTION",
            "IP_IMSL_BASTION_SHIELD",

            //Tier 2

            //Tier 3
        };

        public static bool IsInnerFrame(string defName)
        {
            return AllInnerFrames.Contains(defName);
        }
        
        public static List<Hediff> GetConflictingFrames(Pawn pawn, string newFrameDefName)
        {
            if (!IsInnerFrame(newFrameDefName)) 
                return new List<Hediff>();
            
            // 방법 1: hediffs 프로퍼티 직접 사용
            return pawn.health.hediffSet.hediffs
                .Where(h => IsInnerFrame(h.def.defName) && h.def.defName != newFrameDefName)
                .ToList();
        }
    }

    // Tier1 brain implant
    public static class BrainImplantManager
    {
        private static readonly List<string> Tier1BrainImplants = new List<string>
        {
            //Tier 1 brain implant list
            "IP_CombatAssistAI",
            "IP_FieldworkerAssistAI",
            "IP_CraftmasterAssistAI",
            "IP_CognitorAssistAI",
            "IP_ORD_41_OVERSEER",
            "IP_DAON_43_HARMONY",
            "IP_DAON_44_APPRENTICE",
            "IP_IMSL_SENTINEL_OFFICER"
        };

        public static bool IsTier1BrainImplant(string defName)
        {
            return Tier1BrainImplants.Contains(defName);
        }
        
        public static List<Hediff> GetConflictingBrainImplants(Pawn pawn, string tier1BrainImplantList)
        {
            if (!IsTier1BrainImplant(tier1BrainImplantList)) 
                return new List<Hediff>();
            
            // 방법 1: hediffs 프로퍼티 직접 사용
            return pawn.health.hediffSet.hediffs
                .Where(h => IsTier1BrainImplant(h.def.defName) && h.def.defName != tier1BrainImplantList)
                .ToList();
        }
    }
}
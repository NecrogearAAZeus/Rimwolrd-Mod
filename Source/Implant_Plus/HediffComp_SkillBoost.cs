using Verse;
using RimWorld;
using System;

namespace Implant_Plus
{
    public class HediffCompProperties_SkillBoost : HediffCompProperties
    {
        public int shootingBonus = 0;
        public int meleeBonus = 0;
        public int socialBonus = 0;
        public int cookingBonus = 0;
        public int constructionBonus = 0;
        public int plantsBonus = 0;
        public int miningBonus = 0;
        public int artisticBonus = 0;
        public int craftingBonus = 0;
        public int medicineBonus = 0;
        public int intellectualBonus = 0;

        public HediffCompProperties_SkillBoost()
        {
            this.compClass = typeof(HediffComp_SkillBoost);
        }
    }

    public class HediffComp_SkillBoost : HediffComp
    {
        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            var props = (HediffCompProperties_SkillBoost)this.props;

            if (Pawn?.skills == null) return;

            Pawn.skills.GetSkill(SkillDefOf.Shooting).Level += props.shootingBonus;
            Pawn.skills.GetSkill(SkillDefOf.Melee).Level += props.meleeBonus;
            Pawn.skills.GetSkill(SkillDefOf.Social).Level += props.socialBonus;
            Pawn.skills.GetSkill(SkillDefOf.Cooking).Level += props.cookingBonus;
            Pawn.skills.GetSkill(SkillDefOf.Construction).Level += props.constructionBonus;
            Pawn.skills.GetSkill(SkillDefOf.Plants).Level += props.plantsBonus;
            Pawn.skills.GetSkill(SkillDefOf.Mining).Level += props.miningBonus;
            Pawn.skills.GetSkill(SkillDefOf.Artistic).Level += props.artisticBonus;
            Pawn.skills.GetSkill(SkillDefOf.Crafting).Level += props.craftingBonus;
            Pawn.skills.GetSkill(SkillDefOf.Medicine).Level += props.medicineBonus;
            Pawn.skills.GetSkill(SkillDefOf.Intellectual).Level += props.intellectualBonus;
        }

        public override void CompPostPostRemoved()
        {
            var props = (HediffCompProperties_SkillBoost)this.props;

            if (Pawn?.skills == null) return;

            ApplySkillReduction(SkillDefOf.Shooting, props.shootingBonus);
            ApplySkillReduction(SkillDefOf.Melee, props.meleeBonus);
            ApplySkillReduction(SkillDefOf.Social, props.socialBonus);
            ApplySkillReduction(SkillDefOf.Cooking, props.cookingBonus);
            ApplySkillReduction(SkillDefOf.Construction, props.constructionBonus);
            ApplySkillReduction(SkillDefOf.Plants, props.plantsBonus);
            ApplySkillReduction(SkillDefOf.Mining, props.miningBonus);
            ApplySkillReduction(SkillDefOf.Artistic, props.artisticBonus);
            ApplySkillReduction(SkillDefOf.Crafting, props.craftingBonus);
            ApplySkillReduction(SkillDefOf.Medicine, props.medicineBonus);
            ApplySkillReduction(SkillDefOf.Intellectual, props.intellectualBonus);
        }

        private void ApplySkillReduction(SkillDef skillDef, int amount)
        {
            if (amount == 0) return;

            var skill = Pawn.skills.GetSkill(skillDef);
            skill.Level = Math.Max(0, skill.Level - amount);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Text;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.Core;

namespace PlayableKids.Models
{
    public sealed class WrappedAgeModel : AgeModel
    {
        public AgeModel BaseModel { get; }

        public override int BecomeInfantAge => BaseModel.BecomeInfantAge;

        public override int BecomeChildAge => BaseModel.BecomeChildAge;

        public override int BecomeTeenagerAge => BaseModel.BecomeTeenagerAge;

        public override int HeroComesOfAge => BaseModel.HeroComesOfAge;

        public override int BecomeOldAge => BaseModel.BecomeOldAge;

        public override int MaxAge => BaseModel.MaxAge;

        public WrappedAgeModel(AgeModel baseModel)
        {
            BaseModel = baseModel;
        }

        public override void GetAgeLimitForLocation(CharacterObject character, out int minimumAge, out int maximumAge, string additionalTags = "")
        {
            BaseModel.GetAgeLimitForLocation(character, out minimumAge, out maximumAge, additionalTags);
        }

        public override float GetSkillScalingModifierForAge(Hero hero, SkillObject skill, bool isByNaturalGrowth)
        {
            return BaseModel.GetSkillScalingModifierForAge(hero, skill, isByNaturalGrowth);
        }
    }
}

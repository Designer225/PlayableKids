using System.Collections.Generic;
using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace PlayableKids.Models
{
    public class WrappedHeroCreationModel : HeroCreationModel
    {
        public WrappedHeroCreationModel(HeroCreationModel baseModel) => Initialize(baseModel);
        
        public override (CampaignTime, CampaignTime) GetBirthAndDeathDay(CharacterObject character, bool createAlive,
            int age)
        {
            if (!createAlive || age == -1 || age == 0 || character.Occupation != Occupation.Wanderer)
                return BaseModel.GetBirthAndDeathDay(character, createAlive, age);
            age = Campaign.Current.Models.AgeModel.HeroComesOfAge + Settings.Instance.WandererMinAgeIncrease +
                  MBRandom.RandomInt(Settings.Instance.WandererAgeRandomization);
            return (HeroHelper.GetRandomBirthDayForAge(age), CampaignTime.Never);
        }

        public override Settlement GetBornSettlement(Hero character) => BaseModel.GetBornSettlement(character);

        public override StaticBodyProperties GetStaticBodyProperties(Hero character, bool isOffspring,
            float variationAmount = 0.35f) =>
            BaseModel.GetStaticBodyProperties(character, isOffspring, variationAmount);

        public override FormationClass GetPreferredUpgradeFormation(Hero character) =>
            BaseModel.GetPreferredUpgradeFormation(character);

        public override Clan GetClan(Hero character) => BaseModel.GetClan(character);

        public override CultureObject GetCulture(Hero hero, Settlement bornSettlement, Clan clan) =>
            BaseModel.GetCulture(hero, bornSettlement, clan);

        public override CharacterObject GetRandomTemplateByOccupation(Occupation occupation,
            Settlement? settlement = null) => BaseModel.GetRandomTemplateByOccupation(occupation, settlement);

        public override List<(TraitObject trait, int level)> GetTraitsForHero(Hero hero) =>
            BaseModel.GetTraitsForHero(hero);

        public override Equipment GetCivilianEquipment(Hero hero) => BaseModel.GetCivilianEquipment(hero);

        public override Equipment GetBattleEquipment(Hero hero) => BaseModel.GetBattleEquipment(hero);

        public override CharacterObject GetCharacterTemplateForOffspring(Hero mother, Hero father,
            bool isOffspringFemale) => BaseModel.GetCharacterTemplateForOffspring(mother, father, isOffspringFemale);

        public override (TextObject firstName, TextObject name) GenerateFirstAndFullName(Hero hero) =>
            BaseModel.GenerateFirstAndFullName(hero);

        public override List<(SkillObject, int)> GetDefaultSkillsForHero(Hero hero)
        {
            var defaultSkillsForHero = BaseModel.GetDefaultSkillsForHero(hero);
            if (defaultSkillsForHero.Count > 0 || hero.Age < Settings.Instance.MinimumPlayerAge ||
                hero.Age >= Campaign.Current.Models.AgeModel.HeroComesOfAge) return defaultSkillsForHero;
            var defaultCharacterSkills = hero.CharacterObject.GetDefaultCharacterSkills();
            foreach (var attribute in Skills.All)
            {
                var skillValue = defaultCharacterSkills.Skills.GetPropertyValue(attribute);
                if (skillValue > 0)
                    skillValue = AddNoiseToSkillValue(skillValue);
                defaultSkillsForHero.Add((attribute, skillValue));
            }
            return defaultSkillsForHero;
        }

        private static int AddNoiseToSkillValue(int skillValue)
        {
            skillValue += MBRandom.RandomInt(5, 10);
            return MathF.Max(skillValue, 1);
        }

        public override List<(SkillObject, int)> GetInheritedSkillsForHero(Hero hero) =>
            BaseModel.GetInheritedSkillsForHero(hero);

        public override bool IsHeroCombatant(Hero hero) => BaseModel.IsHeroCombatant(hero);
    }
}
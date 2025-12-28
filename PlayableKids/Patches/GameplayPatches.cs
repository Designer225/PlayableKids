using HarmonyLib;
using Helpers;
using SandBox.CampaignBehaviors;
using StoryMode;
using StoryMode.StoryModeObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using StoryMode.GameComponents.CampaignBehaviors;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.Encyclopedia;
using TaleWorlds.CampaignSystem.Encyclopedia.Pages;
using TaleWorlds.CampaignSystem.ViewModelCollection.Encyclopedia.Pages;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.Tableaus;

namespace PlayableKids.Patches
{
    [HarmonyPatch]
    [HarmonyPatchCategory(Category)]
    internal static class GameplayPatches
    {
        internal const string Category = "PlayableKids.Misc";

        private static readonly MethodInfo DefaultEncyclopediaHeroPage_CanPlayerSeeValuesOf
            = AccessTools.Method(typeof(DefaultEncyclopediaHeroPage), "CanPlayerSeeValuesOf");

        #region Patches
        #region StoryMode
        [HarmonyPatch(typeof(StoryModeCharacterCreationCampaignBehavior), "FinalizeParentsAndLittleSiblings")]
        [HarmonyPrefix]
        static void StoryModeCharacterCreationCampaignBehavior_FinalizeParentsAndLittleSiblings()
        {
            StoryModeHeroes.LittleBrother.SetBirthDay(HeroHelper.GetRandomBirthDayForAge(Settings.Instance.LittleBrotherStartingAge));
            StoryModeHeroes.LittleSister.SetBirthDay(HeroHelper.GetRandomBirthDayForAge(Settings.Instance.LittleSisterStartingAge));
            StoryModeHeroes.ElderBrother.SetBirthDay(HeroHelper.GetRandomBirthDayForAge(Settings.Instance.ElderBrotherStartingAge));
        }
        #endregion

        #region TaleWorlds.Core
        [HarmonyPatch(typeof(CommonVillagersCampaignBehavior), "conversation_children_rhymes_on_condition")]
        [HarmonyPostfix]
        static void CommonVillagersCampaignBehavior_conversation_children_rhymes_on_conditionPostfix(ref bool __result)
        {
            var agent = Campaign.Current.ConversationManager.OneToOneConversationCharacter;
            if (agent != null)
            {
                var culture = agent.Culture;
                __result = agent == culture.TownsmanInfant || agent == culture.TownswomanInfant
                    || agent == culture.TownsmanChild || agent == culture.TownswomanChild
                    || agent == culture.VillagerMaleChild || agent == culture.VillagerFemaleChild;
            }
        }
        #endregion

        #region TaleWorlds.CampaignSystem
        [HarmonyPatch(typeof(CompanionsCampaignBehavior), "CreateCompanionAndAddToSettlement")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> CompanionsCampaignBehavior_CreateCompanionAndAddToSettlementTranspiler(
            IEnumerable<CodeInstruction> instructions, MethodBase original)
        {
            Debug.Print($"[PlayableKids] Patching: {original}");

            foreach (var instruction in instructions)
            {
                if (instruction.opcode == OpCodes.Ldc_I4_5)
                {
                    //yield return new CodeInstruction(OpCodes.Ldc_I4_0);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(Settings), nameof(Settings.Instance)));
                    yield return new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(Settings), nameof(Settings.WandererMinAgeIncrease)));
                }
                else if (instruction.Is(OpCodes.Ldc_I4_S, 27))
                {
                    //yield return new CodeInstruction(OpCodes.Ldc_I4_S, 32);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(Settings), nameof(Settings.Instance)));
                    yield return new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(Settings), nameof(Settings.WandererAgeRandomization)));
                }
                else
                    yield return instruction;
            }
        }

        [HarmonyPatch(typeof(DefaultEncyclopediaHeroPage), "InitializeListItems")]
        [HarmonyPostfix]
        static IEnumerable<EncyclopediaListItem> DefaultEncyclopediaHeroPage_InitializeListItemsPassThroughPostfix
            (IEnumerable<EncyclopediaListItem> values, DefaultEncyclopediaHeroPage __instance)
        {
            // included are fine; the rest needs processing
            foreach (var value in values)
                yield return value;

            // the processing
            var toIncludedHeroes = values.Select(x => x.Object).Cast<Hero>();
            var visibleAge = Settings.Instance.MinimumVisibleAge;
            var func = AccessTools.MethodDelegate<Func<Hero, bool>>(DefaultEncyclopediaHeroPage_CanPlayerSeeValuesOf, __instance);
            TextObject heroName = new TextObject("{=TauRjAud}{NAME} of the {FACTION}");

            foreach (var hero in Hero.AllAliveHeroes.Except(toIncludedHeroes)
                .Where(x => __instance.IsValidEncyclopediaItem(x) && !x.IsNotable && x.Age >= visibleAge))
            {
                var clan = hero.Clan;
                string name;
                if (clan != null)
                {
                    heroName.SetTextVariable("NAME", hero.FirstName ?? hero.Name);
                    heroName.SetTextVariable("FACTION", hero.Clan?.Name ?? TextObject.GetEmpty());
                    name = heroName.ToString();
                }
                else
                    name = hero.Name.ToString();
                yield return new EncyclopediaListItem(hero, name, "", hero.StringId, __instance.GetIdentifier(typeof(Hero)),
                    func(hero), () => InformationManager.ShowTooltip(typeof(Hero), hero, false));
            }

            foreach (var hero in Hero.DeadOrDisabledHeroes.Except(toIncludedHeroes)
                .Where(x => __instance.IsValidEncyclopediaItem(x) && !x.IsNotable && x.Age >= visibleAge))
            {
                Clan clan = hero.Clan;
                if (clan != null)
                {
                    heroName.SetTextVariable("NAME", hero.FirstName ?? hero.Name);
                    heroName.SetTextVariable("FACTION", hero.Clan?.Name ?? TextObject.GetEmpty());
                    yield return new EncyclopediaListItem(hero, heroName.ToString(), "", hero.StringId,
                        __instance.GetIdentifier(typeof(Hero)), func(hero), () => InformationManager.ShowTooltip(typeof(Hero), hero, false));
                }
                else
                    yield return new EncyclopediaListItem(hero, hero.Name.ToString(), "", hero.StringId,
                        __instance.GetIdentifier(typeof(Hero)), func(hero), () => InformationManager.ShowTooltip(typeof(Hero), hero, false));

            }
        }

        [HarmonyPatch(typeof(HeroCreator), "CreateNewHero")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> HeroCreator_CreateNewHeroTranspiler(IEnumerable<CodeInstruction> instructions, MethodBase original)
        {
            Debug.Print($"[PlayableKids] Patching: {original}");

            var list = instructions.ToList();
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].opcode == OpCodes.Ldarg_0 && i + 3 < list.Count && list[i + 3].opcode == OpCodes.Starg_S)
                {
                    list.RemoveRange(i, 3);
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return list[i];
                }
                else if (list[i].opcode == OpCodes.Stloc_S && list[i].operand is LocalBuilder lb && lb.LocalIndex == 6)
                {
                    yield return list[i];
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(list[i]);
                }
                else
                    yield return list[i];
            }
        }

        [HarmonyPatch(typeof(TeleportationCampaignBehavior), "OnHeroComesOfAge")]
        [HarmonyPrefix]
        static bool TeleportationCampaignBehavior_OnHeroComesOfAgePrefix(Hero hero) => hero.Clan != default;
        #endregion

        #region TaleWorlds.CampaignSystem.CampaignBehaviors
        [HarmonyPatch(typeof(AgingCampaignBehavior), "DailyTickHero")]
        [HarmonyPrefix]
        static bool AgingCampaignBehavior_DailyTickHeroPrefix(Hero hero)
        {
            if (StoryModeManager.Current?.MainStoryLine?.FamilyRescued ?? true) return true;
            if (hero.Age >= Campaign.Current.Models.AgeModel.HeroComesOfAge) return true;
            if (hero == StoryModeHeroes.LittleBrother) return false;
            if (hero == StoryModeHeroes.LittleSister) return false;
            if (hero == StoryModeHeroes.ElderBrother) return false;
            return true;
        }
        #endregion

        #region TaleWorlds.MountAndBlade
        [HarmonyPatch(typeof(Mission), nameof(Mission.SpawnAgent))]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> Mission_SpawnAgentTranspiler(IEnumerable<CodeInstruction> instructions, MethodBase original)
        {
            Debug.Print($"[PlayableKids] Patching: {original}");

            var list = instructions.ToList();

            for (int i = 0; i < list.Count; i++)
            {
                yield return list[i];
                if (list[i].Is(OpCodes.Call, AccessTools.Method(typeof(MBBodyProperties), nameof(MBBodyProperties.GetMaturityType))))
                    list[i + 1] = new CodeInstruction(OpCodes.Ldc_I4_0);
            }
        }

        [HarmonyPatch(typeof(Agent), nameof(Agent.EquipItemsFromSpawnEquipment))]
        [HarmonyPostfix]
        static void Agent_EquipItemsFromSpawnEquipmentPostfix(Agent __instance)
        {
            if (!__instance.IsHuman || __instance.Age >= 18f) return;

            if (__instance.IsHuman && __instance.Age < 18f)
            {
                float age = __instance.Age;
                float scale = __instance.AgentScale;
                __instance.Age = 18f;

                SkinGenerationParams skinParams =
                    new SkinGenerationParams((int)SkinMask.NoneVisible, __instance.SpawnEquipment.GetUnderwearType(__instance.IsFemale && __instance.Age >= 14f),
                    (int)__instance.SpawnEquipment.BodyMeshType, (int)__instance.SpawnEquipment.HairCoverType, (int)__instance.SpawnEquipment.BeardCoverType,
                    (int)__instance.SpawnEquipment.BodyDeformType, __instance == Agent.Main, __instance.Character.FaceDirtAmount, __instance.IsFemale ? 1 : 0,
                    __instance.Character.Race, false, false, 0);
                __instance.AgentVisuals.AddSkinMeshes(skinParams, __instance.BodyPropertiesValue, true, __instance.Character != null && __instance.Character.FaceMeshCache);
                AccessTools.Method(typeof(Agent), "SetInitialAgentScale").Invoke(__instance, new object[] { scale });
                __instance.Age = age;
            }
        }
        #endregion

        #region TaleWorlds.MountAndBlade.View
        [HarmonyPatch(typeof(BasicCharacterTableau), "RefreshCharacterTableau")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> BasicCharacterTableau_RefreshCharacterTableauTranspiler(IEnumerable<CodeInstruction> instructions, MethodBase original)
        {
            Debug.Print($"[PlayableKids] Patching: {original}");

            var list = instructions.ToList();
            for (int i = 0; i < list.Count; i++)
            {
                yield return list[i];
                if (list[i].Is(OpCodes.Ldfld, AccessTools.Field(typeof(BasicCharacterTableau), "_faceDirtAmount"))
                    && list[i + 1].opcode == OpCodes.Ldloc_S && list[i + 1].operand is LocalBuilder lb && lb.LocalIndex == 4)
                {
                    list.RemoveAt(i + 1);
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(BasicCharacterTableau), "_isFemale"));
                }
            }
        }
        #endregion
        #endregion
    }
}

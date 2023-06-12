using HarmonyLib;
using Helpers;
using SandBox.CampaignBehaviors;
using StoryMode.CharacterCreationContent;
using StoryMode.StoryModeObjects;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.Tableaus;

namespace PlayableKids.Patches
{
    internal static class GameplayPatches
    {
        internal const string Category = "PlayableKids.Misc";

        internal static bool Matches(this CodeInstruction instruction, OpCode opcode) => instruction.opcode == opcode;

        internal static bool Matches<T>(this CodeInstruction instruction, OpCode opcode, T operand)
            => instruction.Matches(opcode) && instruction.operand is T t && t.Equals(operand);

        internal static void Patch()
        {
            var instance = new Harmony("Designer225.PlayableKids");
            instance.PatchCategory(AgeModel_HeroComesOfAgeTargetPatches.Category);
            instance.PatchCategory(FaceGen_GetMaturityTypeWithAgePatches.Category);
            instance.PatchCategory(HardcodedPatches.Category);
            instance.PatchAllUncategorized();
        }

        #region Patches
        #region StoryMode
        [HarmonyPatch(typeof(StoryModeCharacterCreationContent), "EscapeOnInit")]
        [HarmonyPrefix]
        static void StoryModeCharacterCreationContent_EscapeOnInitPrefix()
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
        static IEnumerable<CodeInstruction> CompanionsCampaignBehavior_CreateCompanionAndAddToSettlementTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (var instruction in instructions)
            {
                if (instruction.Matches(OpCodes.Ldc_I4_5))
                    yield return new CodeInstruction(OpCodes.Ldc_I4_0);
                else if (instruction.Matches(OpCodes.Ldc_I4_S, 27))
                    yield return new CodeInstruction(OpCodes.Ldc_I4_S, 32);
                else
                    yield return instruction;
            }
        }

        [HarmonyPatch(typeof(HeroCreator), "CreateNewHero")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> HeroCreator_CreateNewHeroTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            var list = instructions.ToList();
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Matches(OpCodes.Ldarg_0) && i + 3 < list.Count && list[i + 3].Matches(OpCodes.Starg_S))
                {
                    list.RemoveRange(i, 3);
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return list[i];
                }
                else if (list[i].Matches(OpCodes.Stloc_S) && list[i].operand is LocalBuilder lb && lb.LocalIndex == 5)
                {
                    yield return list[i];
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(list[i]);
                }
                else
                    yield return list[i];
            }
        }
        #endregion

        #region TaleWorlds.MountAndBlade
        [HarmonyPatch(typeof(Mission), nameof(Mission.SpawnAgent))]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> Mission_SpawnAgentTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            var list = instructions.ToList();

            for (int i = 0; i < list.Count; i++)
            {
                yield return list[i];
                if (list[i].Matches(OpCodes.Call, AccessTools.Method(typeof(MBBodyProperties), nameof(MBBodyProperties.GetMaturityType))))
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
                    __instance.Character.Race, false, false);
                __instance.AgentVisuals.AddSkinMeshes(skinParams, __instance.BodyPropertiesValue, true, __instance.Character != null && __instance.Character.FaceMeshCache);
                AccessTools.Method(typeof(Agent), "SetInitialAgentScale").Invoke(__instance, new object[] { scale });
                __instance.Age = age;
            }
        }
        #endregion

        #region TaleWorlds.MountAndBlade.View
        [HarmonyPatch(typeof(BasicCharacterTableau), "RefreshCharacterTableau")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> BasicCharacterTableau_RefreshCharacterTableauTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            var list = instructions.ToList();
            for (int i = 0; i < list.Count; i++)
            {
                yield return list[i];
                if (list[i].Matches(OpCodes.Ldfld, AccessTools.Field(typeof(BasicCharacterTableau), "_faceDirtAmount"))
                    && list[i + 1].Matches(OpCodes.Ldloc_S) && list[i + 1].operand is LocalBuilder lb && lb.LocalIndex == 4)
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

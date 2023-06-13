using HarmonyLib;
using Helpers;
using SandBox.CampaignBehaviors;
using StoryMode.GameComponents.CampaignBehaviors;
using StoryMode.Quests.PlayerClanQuests;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Encyclopedia.Pages;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Issues;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements.Locations;
using TaleWorlds.CampaignSystem.TournamentGames;
using TaleWorlds.CampaignSystem.ViewModelCollection.Encyclopedia.Pages;
using TaleWorlds.CampaignSystem.ViewModelCollection.GameMenu.Overlay;

namespace PlayableKids.Patches
{
    internal static class AgeModel_HeroComesOfAgeTargetPatches
    {
        internal const string Category = "TaleWorlds.CampaignSystem.ComponentInterfaces.AgeModel.get_HeroComesOfAge";

        [HarmonyPatch]
        [HarmonyPatchCategory(Category)]
        static class PlayerAgePatches
        {
            static IEnumerable<MethodBase> TargetMethods()
            {
                // Sandbox
                yield return AccessTools.Method(typeof(ClanMemberRolesCampaignBehavior), "UpdateAccompanyingCharacters");
                yield return AccessTools.Method(typeof(CompanionRolesCampaignBehavior), "CreateNewHeroForNewCompanionClan");
                // StoryMode
                yield return AccessTools.Method(typeof(MainStorylineCampaignBehavior), "OnGameLoadFinished");
                yield return AccessTools.Method(typeof(RescueFamilyQuestBehavior.RescueFamilyQuest), "OnCompleteWithSuccess");
                // TaleWorlds.CampaignSystem
                yield return AccessTools.Method(typeof(BannerCampaignBehavior), "CanBannerBeGivenToHero");
                yield return AccessTools.Method(typeof(CampaignCheats), nameof(CampaignCheats.SetMainHeroAge));
                yield return AccessTools.Method(typeof(CompanionsCampaignBehavior), "CreateCompanionAndAddToSettlement");
                yield return AccessTools.Method(typeof(Clan), nameof(Clan.GetHeirApparents));
                if (Settings.Instance != default && !Settings.Instance.OverrideWithModels)
                    yield return AccessTools.Method(typeof(DefaultEmissaryModel), nameof(DefaultEmissaryModel.IsEmissary));
                yield return AccessTools.Method(typeof(FactionHelper), "IsMainClanMemberAvailableForRelocate");
                yield return AccessTools.Method(typeof(FactionManager), nameof(FactionManager.GetRelationBetweenClans));
                yield return AccessTools.Method(typeof(FightTournamentGame), "CanNpcJoinTournament");
                yield return AccessTools.PropertyGetter(typeof(Hero), nameof(Hero.IsChild));
                yield return AccessTools.Method(typeof(HeroAgentSpawnCampaignBehavior), "AddCompanionsAndClanMembersToSettlement");
                yield return AccessTools.Method(typeof(HeroAgentSpawnCampaignBehavior), "AddLordsHallCharacters");
                yield return AccessTools.Method(typeof(HeroCreator), "CreateNewHero");
                yield return AccessTools.Method(typeof(HeroDeveloper), "AfterLoad");
                yield return AccessTools.Method(typeof(HeroSpawnCampaignBehavior), "CanHeroMoveToAnotherSettlement");
                yield return AccessTools.Method(typeof(HeroSpawnCampaignBehavior), "GetBestAvailableCommander");
                yield return AccessTools.Method(typeof(HeroSpawnCampaignBehavior), "OnNewGameCreatedPartialFollowUp");
                yield return AccessTools.Method(typeof(LocationComplex), nameof(LocationComplex.CanIfGrownUpMaleOrHero));
                yield return AccessTools.Method(typeof(TownHelpers), nameof(TownHelpers.RequestAMeetingHeroWithoutPartyCondition));
                // TaleWorlds.CampaignSystem.ViewModelCollection
                yield return AccessTools.Method(typeof(SettlementMenuOverlayVM), "ExecuteOnSetAsActiveContextMenuItem");

                // originally for PositionAge, which was removed
                // TaleWorlds.CampaignSystem
                yield return AccessTools.Method(typeof(ClanVariablesCampaignBehavior), "UpdateGovernorsOfClan");
                yield return AccessTools.Method(typeof(IssuesCampaignBehavior), "CreateAnIssueForClanNobles");
                yield return AccessTools.Method(typeof(LandLordCompanyOfTroubleIssueBehavior.LandLordCompanyOfTroubleIssueQuest), "PersuasionDialogForLordGeneralCondition");
            }

            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var list = instructions.ToList();

                for (int i = 0; i < list.Count; i++)
                {
                    if (i + 3 < list.Count && list[i + 3].Is(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(AgeModel), nameof(AgeModel.HeroComesOfAge))))
                    {
                        var labels = list[i].ExtractLabels();
                        list.RemoveRange(i, 4);
                        i--;
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(Settings), nameof(Settings.Instance))).WithLabels(labels);
                        yield return new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(Settings), nameof(Settings.MinimumPlayerAge)));
                    }
                    else
                        yield return list[i];
                }
            }
        }

        [HarmonyPatch]
        [HarmonyPatchCategory(Category)]
        static class VisibleAgePatches
        {
            static IEnumerable<MethodBase> TargetMethods()
            {
                // TaleWorlds.CampaignSystem
                //yield return AccessTools.EnumeratorMoveNext(AccessTools.Method(typeof(DefaultEncyclopediaHeroPage), "InitializeListItems")); // doesn't seem to work
                // TaleWorlds.CampaignSystem.ViewModelCollection
                yield return AccessTools.Method(typeof(EncyclopediaHeroPageVM), "AddHeroToRelatedVMList");
                yield return AccessTools.Method(typeof(EncyclopediaHeroPageVM), nameof(EncyclopediaHeroPageVM.Refresh));
            }

            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var list = instructions.ToList();

                for (int i = 0; i < list.Count; i++)
                {
                    if (i + 3 < list.Count && list[i + 3].Is(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(AgeModel), nameof(AgeModel.HeroComesOfAge))))
                    {
                        var labels = list[i].ExtractLabels();
                        list.RemoveRange(i, 4);
                        i--;
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(Settings), nameof(Settings.Instance))).WithLabels(labels);
                        yield return new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(Settings), nameof(Settings.MinimumVisibleAge)));
                    }
                    else
                        yield return list[i];
                }
            }
        }

        //[HarmonyPatch]
        //[HarmonyPatchCategory(Category)]
        //static class PositionAgePatches
        //{
        //    static IEnumerable<MethodBase> TargetMethods()
        //    {
        //        // TaleWorlds.CampaignSystem
        //        yield return AccessTools.Method(typeof(ClanVariablesCampaignBehavior), "UpdateGovernorsOfClan");
        //        yield return AccessTools.Method(typeof(IssuesCampaignBehavior), "CreateAnIssueForClanNobles");
        //        yield return AccessTools.Method(typeof(LandLordCompanyOfTroubleIssueBehavior.LandLordCompanyOfTroubleIssueQuest), "PersuasionDialogForLordGeneralCondition");
        //    }

        //    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        //    {
        //        var list = instructions.ToList();

        //        for (int i = 0; i < list.Count; i++)
        //        {
        //            if (i + 3 < list.Count && list[i + 3].Matches(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(AgeModel), nameof(AgeModel.HeroComesOfAge))))
        //            {
        //                var labels = list[i].ExtractLabels();
        //                list.RemoveRange(i, 4);
        //                i--;
        //                yield return new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(Settings), nameof(Settings.Instance))).WithLabels(labels);
        //                yield return new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(Settings), nameof(Settings.MinimumPositionAge)));
        //            }
        //            else
        //                yield return list[i];
        //        }
        //    }
        //}

        [HarmonyPatch]
        [HarmonyPatchCategory(Category)]
        static class ExecutionAgePatches
        {
            static IEnumerable<MethodBase> TargetMethods()
            {
                // TaleWorlds.CampaignSystem
                yield return AccessTools.Method(typeof(PartyScreenLogic), nameof(PartyScreenLogic.IsExecutable));
            }

            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var list = instructions.ToList();

                for (int i = 0; i < list.Count; i++)
                {
                    if (i + 3 < list.Count && list[i + 3].Is(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(AgeModel), nameof(AgeModel.HeroComesOfAge))))
                    {
                        var labels = list[i].ExtractLabels();
                        list.RemoveRange(i, 4);
                        i--;
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(Settings), nameof(Settings.Instance))).WithLabels(labels);
                        yield return new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(Settings), nameof(Settings.MinimumExecutionAge)));
                    }
                    else
                        yield return list[i];
                }
            }
        }
    }
}

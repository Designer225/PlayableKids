using HarmonyLib;
using Helpers;
using SandBox.CampaignBehaviors;
using StoryMode.GameComponents.CampaignBehaviors;
using StoryMode.Quests.PlayerClanQuests;
using System.Collections.Generic;
using System.Diagnostics;
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
    internal static class AgeModelPatches
    {
        internal static class HeroComesOfAgeTargetPatches
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
                    // yield return AccessTools.Method(typeof(CampaignCheats), nameof(CampaignCheats.SetMainHeroAge)); // this is apparently now an age adder and no longer uses that variable
                    yield return AccessTools.Method(typeof(CompanionsCampaignBehavior), "CreateCompanionAndAddToSettlement");
                    yield return AccessTools.Method(typeof(Clan), nameof(Clan.GetHeirApparents));
                    yield return AccessTools.Method(typeof(DefaultEmissaryModel), nameof(DefaultEmissaryModel.IsEmissary)); // too much to copy and change
                    yield return AccessTools.Method(typeof(DefaultHeroAgentLocationModel), // the original version is too much code to simply copy and change
                        nameof(DefaultHeroAgentLocationModel.GetLocationForHero));
                    yield return AccessTools.Method(typeof(FactionHelper), "IsMainClanMemberAvailableForRelocate");
                    yield return AccessTools.Method(typeof(FactionManager), nameof(FactionManager.GetRelationBetweenClans));
                    yield return AccessTools.Method(typeof(FightTournamentGame), "CanNpcJoinTournament");
                    yield return AccessTools.PropertyGetter(typeof(Hero), nameof(Hero.IsChild));
                    // yield return AccessTools.Method(typeof(HeroAgentSpawnCampaignBehavior), "AddCompanionsAndClanMembersToSettlement"); // it's apparently a model now
                    // yield return AccessTools.Method(typeof(HeroAgentSpawnCampaignBehavior), "AddLordsHallCharacters"); // see DefaultHeroAgentLocationModel, above
                    // yield return AccessTools.Method(typeof(HeroCreator), "CreateNewHero"); // replaced by WrappedHeroCreationModel.GetBirthAndDeathDay in part
                    // yield return AccessTools.Method(typeof(HeroDeveloper), "AfterLoad"); // suspected to be replaced by wrappedHeroCreationModel.GetDefaultSkillsForHero
                    yield return AccessTools.Method(typeof(HeroSpawnCampaignBehavior), "CanHeroMoveToAnotherSettlement");
                    yield return AccessTools.Method(typeof(HeroSpawnCampaignBehavior), "GetBestAvailableCommander");
                    yield return AccessTools.Method(typeof(HeroSpawnCampaignBehavior), "OnNewGameCreatedPartialFollowUp");
                    yield return AccessTools.Method(typeof(LocationComplex), nameof(LocationComplex.CanIfGrownUpMaleOrHero));
                    yield return AccessTools.Method(typeof(TownHelpers), nameof(TownHelpers.RequestAMeetingHeroWithoutPartyCondition));
                    // TaleWorlds.CampaignSystem.ViewModelCollection
                    yield return AccessTools.Method(typeof(SettlementMenuOverlayVM), "ExecuteOnSetAsActiveContextMenuItem");

                    // originally for PositionAge, which was removed
                    // TaleWorlds.CampaignSystem
                    // below line is replaced by DefaultClanPoliticsModel.CanHeroBeGovernor, which uses Hero.IsChild instead of AgeModel.HeroComesOfAge
                    // Hero.IsChild is already patched earlier, so this one is skipped.
                    //yield return AccessTools.Method(typeof(ClanVariablesCampaignBehavior), "UpdateGovernorsOfClan");
                    yield return AccessTools.Method(typeof(IssuesCampaignBehavior), "CreateAnIssueForClanNobles");
                    yield return AccessTools.Method(typeof(LandLordCompanyOfTroubleIssueBehavior.LandLordCompanyOfTroubleIssueQuest), "PersuasionDialogForLordGeneralCondition");
                }

                static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase original)
                {
                    Debug.Print($"[PlayableKids] Patching: {original}");
                    var codeMatcher = new CodeMatcher(instructions);
                    MatchAndModify(codeMatcher);
                    if (original == AccessTools.Method(typeof(RescueFamilyQuestBehavior.RescueFamilyQuest),
                            "OnCompleteWithSuccess") ||
                        original == AccessTools.Method(typeof(FactionManager),
                            nameof(FactionManager.GetRelationBetweenClans)) ||
                        original == AccessTools.Method(typeof(HeroSpawnCampaignBehavior), "GetBestAvailableCommander"))
                        MatchAndModify(codeMatcher); // there are two matches for this method, so we need to match it twice

                    return codeMatcher.InstructionEnumeration();
                }

                private static void MatchAndModify(CodeMatcher codeMatcher)
                {
                    codeMatcher.MatchStartForward(CodeMatch.Calls(AccessTools.PropertyGetter(typeof(Campaign), nameof(Campaign.Current))),
                        CodeMatch.Calls(AccessTools.PropertyGetter(typeof(Campaign), nameof(Campaign.Models))),
                        CodeMatch.Calls(AccessTools.PropertyGetter(typeof(GameModels), nameof(GameModels.AgeModel))),
                        CodeMatch.Calls(AccessTools.PropertyGetter(typeof(AgeModel), nameof(AgeModel.HeroComesOfAge))));
                    if (codeMatcher.IsValid)
                    {
                        var labels = codeMatcher.Labels;
                        codeMatcher.RemoveInstructions(4).InsertAndAdvance(
                            new CodeInstruction(OpCodes.Call,
                                    AccessTools.PropertyGetter(typeof(Settings), nameof(Settings.Instance)))
                                .WithLabels(labels),
                            new CodeInstruction(OpCodes.Callvirt,
                                AccessTools.PropertyGetter(typeof(Settings), nameof(Settings.MinimumPlayerAge))));
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
                    //// trying to get this to work...
                    //MethodInfo moveNext = AccessTools.EnumeratorMoveNext(AccessTools.Method(typeof(DefaultEncyclopediaHeroPage), "InitializeListItems"));
                    //if (moveNext != null) yield return moveNext;
                    //else yield return AccessTools.Method(typeof(DefaultEncyclopediaHeroPage), "InitializeListItems");

                    // TaleWorlds.CampaignSystem.ViewModelCollection
                    yield return AccessTools.Method(typeof(EncyclopediaHeroPageVM), "AddHeroToRelatedVMList");
                    yield return AccessTools.Method(typeof(EncyclopediaHeroPageVM), nameof(EncyclopediaHeroPageVM.Refresh));
                }

                static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase original)
                {
                    Debug.Print($"[PlayableKids] Patching: {original}");
                    
                    var codeMatcher = new CodeMatcher(instructions);
                    codeMatcher.MatchStartForward(CodeMatch.Calls(AccessTools.PropertyGetter(typeof(Campaign), nameof(Campaign.Current))),
                        CodeMatch.Calls(AccessTools.PropertyGetter(typeof(Campaign), nameof(Campaign.Models))),
                        CodeMatch.Calls(AccessTools.PropertyGetter(typeof(GameModels), nameof(GameModels.AgeModel))),
                        CodeMatch.Calls(AccessTools.PropertyGetter(typeof(AgeModel), nameof(AgeModel.HeroComesOfAge))));
                    if (codeMatcher.IsValid)
                    {
                        var labels = codeMatcher.Labels;
                        codeMatcher.RemoveInstructions(4).InsertAndAdvance(
                            new CodeInstruction(OpCodes.Call,
                                    AccessTools.PropertyGetter(typeof(Settings), nameof(Settings.Instance)))
                                .WithLabels(labels),
                            new CodeInstruction(OpCodes.Callvirt,
                                AccessTools.PropertyGetter(typeof(Settings), nameof(Settings.MinimumVisibleAge))));
                    }
                    return codeMatcher.InstructionEnumeration();
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

                static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase original)
                {
                    Debug.Print($"[PlayableKids] Patching: {original}");
                    
                    var codeMatcher = new CodeMatcher(instructions);
                    codeMatcher.MatchStartForward(CodeMatch.Calls(AccessTools.PropertyGetter(typeof(Campaign), nameof(Campaign.Current))),
                        CodeMatch.Calls(AccessTools.PropertyGetter(typeof(Campaign), nameof(Campaign.Models))),
                        CodeMatch.Calls(AccessTools.PropertyGetter(typeof(GameModels), nameof(GameModels.AgeModel))),
                        CodeMatch.Calls(AccessTools.PropertyGetter(typeof(AgeModel), nameof(AgeModel.HeroComesOfAge))));
                    if (codeMatcher.IsValid)
                    {
                        var labels = codeMatcher.Labels;
                        codeMatcher.RemoveInstructions(4).InsertAndAdvance(
                            new CodeInstruction(OpCodes.Call,
                                    AccessTools.PropertyGetter(typeof(Settings), nameof(Settings.Instance)))
                                .WithLabels(labels),
                            new CodeInstruction(OpCodes.Callvirt,
                                AccessTools.PropertyGetter(typeof(Settings), nameof(Settings.MinimumExecutionAge))));
                    }
                    return codeMatcher.InstructionEnumeration();
                    // var list = instructions.ToList();
                    //
                    // for (int i = 0; i < list.Count; i++)
                    // {
                    //     if (i + 3 < list.Count && list[i + 3].Is(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(AgeModel), nameof(AgeModel.HeroComesOfAge))))
                    //     {
                    //         var labels = list[i].ExtractLabels();
                    //         list.RemoveRange(i, 4);
                    //         i--;
                    //         yield return new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(Settings), nameof(Settings.Instance))).WithLabels(labels);
                    //         yield return new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(Settings), nameof(Settings.MinimumExecutionAge)));
                    //     }
                    //     else
                    //         yield return list[i];
                    // }
                }
            }
        }
    }
}

using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.CampaignSystem.ViewModelCollection.KingdomManagement.Decisions.ItemTypes;

namespace PlayableKids.Patches
{
    [HarmonyPatch]
    [HarmonyPatchCategory(Category)]
    internal static class HardcodedPatches
    {
        internal const string Category = "PlayableKids.AntiHardcoded";

        static IEnumerable<MethodBase> TargetMethods()
        {
            // TaleWorlds.CampaignSystem
            yield return AccessTools.Method(typeof(DefaultTournamentModel), "SuitableForTournament");
            // TaleWorlds.CampaignSystem.ViewModelCollection
            yield return AccessTools.Method(typeof(CampaignUIHelper), nameof(CampaignUIHelper.GetClanProsperityTooltip));
            yield return AccessTools.Method(typeof(ExpelClanDecisionItemVM), "InitValues");
        }

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (var instruction in instructions)
            {
                if (instruction.Matches(OpCodes.Ldc_R4, 18f))
                {
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(Settings), nameof(Settings.Instance)));
                    yield return new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(Settings), nameof(Settings.MinimumPlayerAge)));
                    yield return new CodeInstruction(OpCodes.Conv_R4);
                }
                else
                    yield return instruction;
            }
        }
    }
}

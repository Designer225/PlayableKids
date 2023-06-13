using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.ViewModelCollection.ClanManagement;
using TaleWorlds.CampaignSystem.ViewModelCollection.Party;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.MountAndBlade.GauntletUI.TextureProviders;
using System.Reflection.Emit;
using TaleWorlds.CampaignSystem.CampaignBehaviors;

namespace PlayableKids.Patches
{
    [HarmonyPatch]
    internal static class Hero_GetIsChildPatches
    {
        internal const string Category = "TaleWorlds.CampaignSystem.Hero.get_IsChild";

        static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(InitialChildGenerationCampaignBehavior), "OnNewGameCreatedPartialFollowUp");
        }

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (var instruction in instructions)
            {
                if (instruction.Is(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(Hero), nameof(Hero.IsChild))))
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Hero_GetIsChildPatches), nameof(SpoofedMethod)));
                else
                    yield return instruction;
            }
        }

        static bool SpoofedMethod(this Hero hero) => hero.Age < Campaign.Current.Models.AgeModel.HeroComesOfAge;
    }
}

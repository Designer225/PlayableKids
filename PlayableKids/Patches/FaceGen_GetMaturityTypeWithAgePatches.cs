﻿using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.CampaignSystem.ViewModelCollection.ClanManagement;
using TaleWorlds.CampaignSystem.ViewModelCollection.KingdomManagement.Decisions.ItemTypes;
using TaleWorlds.CampaignSystem.ViewModelCollection.Party;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.MountAndBlade.GauntletUI.TextureProviders;

namespace PlayableKids.Patches
{
    [HarmonyPatch]
    [HarmonyPatchCategory(Category)]
    static class FaceGen_GetMaturityTypeWithAgePatches
    {
        internal const string Category = "TaleWorlds.Core.FaceGen.GetMaturityTypeWithAge";

        static IEnumerable<MethodBase> TargetMethods()
        {
            // TaleWorlds.Core.ViewModelCollection
            yield return AccessTools.Method(typeof(CharacterViewModel), nameof(CharacterViewModel.FillFrom));
            // TaleWorlds.CampaignSystem
            yield return AccessTools.Method(typeof(PartyScreenLogic), nameof(PartyScreenLogic.IsExecutable));
            // TaleWorlds.CampaignSystem.ViewModelCollection
            yield return AccessTools.Method(typeof(ClanLordItemVM), nameof(ClanLordItemVM.UpdateProperties));
            yield return AccessTools.Constructor(typeof(HeroVM), new Type[] { typeof(Hero), typeof(bool) });
            yield return AccessTools.Method(typeof(HeroViewModel), nameof(HeroViewModel.FillFrom), new Type[] { typeof(Hero), typeof(int), typeof(bool), typeof(bool) });
            yield return AccessTools.Method(typeof(PartyCharacterVM), nameof(PartyCharacterVM.ExecuteExecuteTroop));
            // TaleWorlds.MountAndBlade.GauntletUI
            yield return AccessTools.Method(typeof(ImageIdentifierTextureProvider), nameof(ImageIdentifierTextureProvider.CreateImageWithId));
            yield return AccessTools.Method(typeof(ImageIdentifierTextureProvider), nameof(ImageIdentifierTextureProvider.ReleaseCache));
        }

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var list = instructions.ToList();

            for (int i = 0; i < list.Count; i++)
            {
                yield return list[i];
                if (list[i].Matches(OpCodes.Call, AccessTools.Method(typeof(FaceGen), nameof(FaceGen.GetMaturityTypeWithAge))))
                    list[i + 1] = new CodeInstruction(OpCodes.Ldc_I4_0);
            }
        }
    }
}

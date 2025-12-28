using HarmonyLib;
using PlayableKids.Models;
using PlayableKids.Patches;
using System;
using System.Linq;
using System.Runtime.CompilerServices;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.LinQuick;
using TaleWorlds.MountAndBlade;

namespace PlayableKids
{
    public class SubModule : MBSubModuleBase
    {
        private bool _initialized = false;

        [MethodImpl(MethodImplOptions.NoInlining)]
        protected override void OnBeforeInitialModuleScreenSetAsRoot()
        {
            base.OnBeforeInitialModuleScreenSetAsRoot();
            if (_initialized) return;
            _initialized = true;

            var instance = new Harmony("Designer225.PlayableKids");
            Debug.Print($"[PlayableKids] Patching category: {AgeModelPatches.HeroComesOfAgeTargetPatches.Category}");
            instance.PatchCategory(AgeModelPatches.HeroComesOfAgeTargetPatches.Category);
            Debug.Print($"[PlayableKids] Patching category: {FaceGen_GetMaturityTypeWithAgePatches.Category}");
            instance.PatchCategory(FaceGen_GetMaturityTypeWithAgePatches.Category);
            Debug.Print($"[PlayableKids] Patching category: {GameplayPatches.Category}");
            instance.PatchCategory(GameplayPatches.Category);
            Debug.Print($"[PlayableKids] Patching category: {HardcodedPatches.Category}");
            instance.PatchCategory(HardcodedPatches.Category);
            Debug.Print($"[PlayableKids] Patching category: {Hero_GetIsChildPatches.Category}");
            instance.PatchCategory(Hero_GetIsChildPatches.Category);
        }

        protected override void OnGameStart(Game game, IGameStarter gameStarterObject)
        {
            base.OnGameStart(game, gameStarterObject);

            if (!(game.GameType is Campaign) || !(gameStarterObject is CampaignGameStarter gameStarter))
                return;

            // add game models
            gameStarter.AddModel(new WrappedAgeModel(gameStarter.Models.WhereQ(x => x is AgeModel).Cast<AgeModel>().Last()));
            // gameStarter.AddModel(new WrappedEmissaryModel(gameStarter.Models.WhereQ(x => x is EmissaryModel).Cast<EmissaryModel>().Last()));
        }
    }
}

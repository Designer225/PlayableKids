using PlayableKids.Models;
using PlayableKids.Patches;
using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace PlayableKids
{
    public class SubModule : MBSubModuleBase
    {
        private bool _initialized = false;

        protected override void OnBeforeInitialModuleScreenSetAsRoot()
        {
            base.OnBeforeInitialModuleScreenSetAsRoot();
            if (_initialized) return;
            _initialized = true;

            GameplayPatches.Patch();
        }

        protected override void OnGameStart(Game game, IGameStarter gameStarterObject)
        {
            base.OnGameStart(game, gameStarterObject);

            if (!(game.GameType is Campaign campaign) || !(gameStarterObject is CampaignGameStarter gameStarter))
                return;

            // add game models
            if (Settings.Instance.OverrideWithModels)
            {
                gameStarter.AddModel(new WrappedAgeModel(campaign.Models.AgeModel));
                gameStarter.AddModel(new WrappedEmissaryModel(campaign.Models.EmissaryModel));
            }
        }
    }
}

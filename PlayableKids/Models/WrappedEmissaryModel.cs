using System;
using System.Collections.Generic;
using System.Text;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;

namespace PlayableKids.Models
{
    public sealed class WrappedEmissaryModel : EmissaryModel
    {
        public EmissaryModel BaseModel { get; }

        public override int EmissaryRelationBonusForMainClan => BaseModel.EmissaryRelationBonusForMainClan;

        public WrappedEmissaryModel(EmissaryModel baseModel)
        {
            BaseModel = baseModel;
        }

        public override bool IsEmissary(Hero hero) => BaseModel.IsEmissary(hero)
            || ((hero.CompanionOf == Clan.PlayerClan || hero.Clan == Clan.PlayerClan)
            && hero.PartyBelongedTo == null && hero.CurrentSettlement != null && hero.CurrentSettlement.IsFortification
            && !hero.IsPrisoner && hero.Age >= Settings.Instance.MinimumPlayerAge);
    }
}

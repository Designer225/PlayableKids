using MCM.Abstractions.Attributes;
using MCM.Abstractions.Attributes.v2;
using MCM.Abstractions.Base.Global;

namespace PlayableKids
{
    public sealed class Settings : AttributeGlobalSettings<Settings>
    {
        private const string
            MinimumAgesGroup = "{=PlayableKids.MinimumAges}Age Minimum Settings",
            CustomAges = "{=PlayableKids.CustomAges}Custom Age Settings";

        public override string Id => "PlayableKids.Settings";

        public override string DisplayName => "{=PlayableKids.DisplayName}Playable Kids".Localized();

        public override string FormatType => "json2";

        public override string FolderName => "PlayableKids";

        [SettingPropertyBool("{=PlayableKids.OverrideWithModels}Override With Models", RequireRestart = true,
            HintText = "{PlayableKids.OverrideWithModels.Hint}Select this to override functions within some models with model wrappers. Models are less prone to script breakage by updates, and model wrappers allow compatibility with existing models. Default is enabled. If disabled, patching will be used instead.")]
        public bool OverrideWithModels { get; set; } = true;

        [SettingPropertyInteger("{=PlayableKids.MinimumPlayerAge}Minimum Playable Age", 6, 18, RequireRestart = false,
            HintText = "{=PlayableKids.MinimumPlayerAge.Hint}The minimum age for a hero to be playable.")]
        [SettingPropertyGroup(MinimumAgesGroup)]
        public int MinimumPlayerAge { get; set; } = 18;

        [SettingPropertyInteger("{=PlayableKids.MinimumVisibleAge}Minimum Visible Age", 3, 18, RequireRestart = false,
            HintText = "{=PlayableKids.MinimumVisibleAge.Hint}The minimum age for a hero to appear in the encyclopedia. (Note that children do have entries, but those are normally hidden from search.)")]
        [SettingPropertyGroup(MinimumAgesGroup)]
        public int MinimumVisibleAge { get; set; } = 6;

        //[SettingPropertyInteger("{=PlayableKids.MinimumPositionAge}Minimum Position Age", 6, 18, RequireRestart = false,
        //    HintText = "{=PlayableKids.MinimumPositionAge.Hint}The minimum age for a hero to perform in a position.")]
        //[SettingPropertyGroup(MinimumAgesGroup)]
        //public int MinimumPositionAge { get; set; } = 18;

        [SettingPropertyInteger("{=PlayableKids.MinimumExecutionAge}Minimum Execution Age", 3, 18, RequireRestart = false,
            HintText = "{=PlayableKids.MinimumExecutionAge.Hint}Minimum age for a character to be eligible for execution.")]
        [SettingPropertyGroup(MinimumAgesGroup)]
        public int MinimumExecutionAge { get; set; } = 18;

        [SettingPropertyInteger("{=PlayableKids.ElderBrotherStartingAge}Elder Brother Age", 3, 18, RequireRestart = false,
            HintText = "{=PlayableKids.ElderBrotherStartingAge.Hint}Starting age of the elder brother.")]
        [SettingPropertyGroup(CustomAges)]
        public int ElderBrotherStartingAge { get; set; } = 25;

        [SettingPropertyInteger("{=PlayableKids.LittleSisterStartingAge}Little Sister Age", 3, 18, RequireRestart = false,
            HintText = "{=PlayableKids.LittleSisterStartingAge.Hint}Starting age of the little sister.")]
        [SettingPropertyGroup(CustomAges)]
        public int LittleSisterStartingAge { get; set; } = 14;

        [SettingPropertyInteger("{=PlayableKids.LittleBrotherStartingAge}Little Brother Age", 3, 18, RequireRestart = false,
            HintText = "{=PlayableKids.LittleBrotherStartingAge.Hint}Starting age of the little brother.")]
        [SettingPropertyGroup(CustomAges)]
        public int LittleBrotherStartingAge { get; set; } = 11;
    }
}

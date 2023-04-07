using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace MechHumanlikes
{
    public class Dialog_RestrictToMHC_PawnType : Window
    {
        public static Vector2 scrollPosition = Vector2.zero;

        private List<CompMHC_PawnTypeRestrictable> compRestricts;

        private MHC_PawnType pawnTypes;

        private static readonly Vector2 ButSize = new Vector2(200f, 40f);

        private static readonly List<Texture2D> exemplarImages = new List<Texture2D> {MHC_Textures.MechDroneExemplar, MHC_Textures.MechSapientExemplar, MHC_Textures.BasicHumanExemplar, MHC_Textures.MechDroneMHC_PawnTypeRestricted, MHC_Textures.MechSapientMHC_PawnTypeRestricted, MHC_Textures.OrganicMHC_PawnTypeRestricted};

        public Dialog_RestrictToMHC_PawnType()
        {
            forcePause = true;
            compRestricts = new List<CompMHC_PawnTypeRestrictable>();
            foreach (object selectedObject in Find.Selector.SelectedObjectsListForReading)
            {
                if (selectedObject is ThingWithComps selectedThing)
                {
                    CompMHC_PawnTypeRestrictable compMHC_PawnTypeRestrictable = selectedThing.GetComp<CompMHC_PawnTypeRestrictable>();
                    if (compMHC_PawnTypeRestrictable != null)
                    {
                        compRestricts.Add(compMHC_PawnTypeRestrictable);
                    }
                }
            }
        }

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Medium;
            Rect TitleRect = new Rect(inRect);
            TitleRect.height = Text.LineHeight * 2f;
            Widgets.Label(TitleRect, "MHC_RestrictedMHC_PawnTypes".Translate());
            Text.Font = GameFont.Small;
            inRect.yMin = TitleRect.yMax + 4f;
            Rect exemplarRect = inRect;
            exemplarRect.width *= 0.3f;
            exemplarRect.yMax -= ButSize.y + 4f;
            pawnTypes = MHC_PawnType.None;
            for (int i = compRestricts.Count - 1; i >= 0; i--)
            {
                pawnTypes |= compRestricts[i].assignedToType;
            }
            DrawExemplars(exemplarRect);
            Rect optionsRect = inRect;
            optionsRect.xMin = exemplarRect.xMax + 10f;
            optionsRect.yMax -= ButSize.y + 4f;
            DrawOptions(optionsRect);
            DrawBottomButtons(inRect);
        }

        private void DrawExemplars(Rect rect)
        {
            Rect rect2 = rect;
            rect2.yMin = rect.yMax - Text.LineHeight * 2f;
            rect.yMax = rect2.yMin - 4f;
            Widgets.BeginGroup(rect);
            Rect position = new Rect(0f, 0, rect.width, rect.height / 3f).ContractedBy(4f);
            GUI.DrawTexture(position, exemplarImages[(pawnTypes & MHC_PawnType.Drone) == MHC_PawnType.Drone ? 0 : 3]);
            position = new Rect(0f, rect.height / 3f, rect.width, rect.height / 3f).ContractedBy(4f);
            GUI.DrawTexture(position, exemplarImages[(pawnTypes & MHC_PawnType.Sapient) == MHC_PawnType.Sapient ? 1 : 4]);
            position = new Rect(0f, rect.height * 2f / 3f, rect.width, rect.height / 3f).ContractedBy(4f);
            GUI.DrawTexture(position, exemplarImages[(pawnTypes & MHC_PawnType.Organic) == MHC_PawnType.Organic ? 2 : 5]);
            Widgets.EndGroup();
        }

        private void DrawOptions(Rect rect)
        {

            Listing_Standard listingStandard = new Listing_Standard
            {
                maxOneColumn = true
            };
            listingStandard.Begin(rect);
            if (listingStandard.RadioButton("MHC_MHC_PawnTypeNone".Translate(), (MHC_PawnType.None | pawnTypes) == MHC_PawnType.None, tooltip: "MHC_MHC_PawnTypeNoneTooltip".Translate(), tooltipDelay: 0.25f))
            {
                for (int j = compRestricts.Count - 1; j >= 0; j--)
                {
                    compRestricts[j].SwitchToType(MHC_PawnType.None);
                }
            }
            for (int i = 1; i < 8; i++)
            {
                if (listingStandard.RadioButton($"MHC_MHC_PawnType{(MHC_PawnType)i}".Translate(), ((MHC_PawnType)i & pawnTypes) == (MHC_PawnType)i, tooltip: $"MHC_MHC_PawnType{(MHC_PawnType)i}Tooltip".Translate(), tooltipDelay: 0.25f))
                {
                    for (int j = compRestricts.Count - 1; j >= 0; j--)
                    {
                        compRestricts[j].SwitchToType((MHC_PawnType)i);
                    }
                }
            }
            listingStandard.End();
        }

        private void DrawBottomButtons(Rect inRect)
        {
            if (Widgets.ButtonText(new Rect(inRect.x, inRect.yMax - ButSize.y, ButSize.x, ButSize.y), "Reset".Translate()))
            {
                Reset();
                SoundDefOf.Tick_Low.PlayOneShotOnCamera();
            }
            if (Widgets.ButtonText(new Rect(inRect.xMax - ButSize.x, inRect.yMax - ButSize.y, ButSize.x, ButSize.y), "Accept".Translate()))
            {
                Close();
            }
        }

        private void Reset()
        {
            foreach (var compRestrict in compRestricts)
            {
                compRestrict.ResetToDefault();
            }
        }
    }
}
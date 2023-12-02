using UnityEngine;
using Verse;

namespace MechHumanlikes
{
    // Replicate HediffGiver_Bleeding but allow for different rise/fall rates.
    public class HediffGiver_MechBleeding : HediffGiver_Bleeding
    {
        public float riseRatePerDay = 1f;

        public float fallRatePerDay = 1f;

        [NoTranslate]
        public string iconPath;

        [Unsaved(false)]
        private Texture2D cachedIcon;

        public override void OnIntervalPassed(Pawn pawn, Hediff cause)
        {
            HediffSet hediffSet = pawn.health.hediffSet;
            if (hediffSet.BleedRateTotal >= 0.1f)
            {
                HealthUtility.AdjustSeverity(pawn, hediff, hediffSet.BleedRateTotal * riseRatePerDay * 0.001f);
            }
            else
            {
                HealthUtility.AdjustSeverity(pawn, hediff, -fallRatePerDay * 0.001f);
            }
        }

        public Texture2D Icon
        {
            get
            {
                if (cachedIcon == null)
                {
                    if (iconPath.NullOrEmpty())
                    {
                        cachedIcon = ContentFinder<Texture2D>.Get("UI/Icons/Medical/Bleeding");
                    }
                    else
                    {
                        cachedIcon = ContentFinder<Texture2D>.Get(iconPath) ?? ContentFinder<Texture2D>.Get("UI/Icons/Medical/Bleeding");
                    }
                }
                return cachedIcon;
            }
        }
    }
}

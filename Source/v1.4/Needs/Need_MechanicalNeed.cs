using RimWorld;
using Verse;

namespace MechHumanlikes
{
    public class Need_MechanicalNeed : Need
    {
        private int ticksAtZero = 0;

        public int TicksAtZero => ticksAtZero;

        public float CoolantDesired => MaxLevel - CurLevel;

        public float PercentageFallRatePerTick
        {
            get
            {
                return def.fallPerDay / 60000;
            }
        }

        public Need_MechanicalNeed(Pawn pawn) 
            : base(pawn)
        {
        }

        public override float MaxLevel
        {
            get
            {
                return pawn.GetStatValue(MHC_StatDefOf.MHC_MechanicalNeedCapacity, applyPostProcess: true, 60000);
            }
        }

        public override void NeedInterval()
        {
            if (IsFrozen)
            {
                return;
            }

            CurLevelPercentage -= 150 * PercentageFallRatePerTick;

            MHC_MechanicalNeedExtension needExtension = def.GetModExtension<MHC_MechanicalNeedExtension>();

            if (CurLevel <= 0.001)
            {
                ticksAtZero += 150;
                if (needExtension.hediffToApplyOnEmpty != null)
                {
                    HealthUtility.AdjustSeverity(pawn, needExtension.hediffToApplyOnEmpty, 150 * (needExtension.hediffRisePerDay / 60000));
                }
            }
            else
            {
                ticksAtZero = 0;
                if (needExtension.hediffToApplyOnEmpty != null)
                {
                    HealthUtility.AdjustSeverity(pawn, needExtension.hediffToApplyOnEmpty, -150 * (needExtension.hediffFallPerDay / 60000));
                }
            }
        }

        public override int GUIChangeArrow
        {
            get
            {
                if (IsFrozen)
                {
                    return 0;
                }
                return -1;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref ticksAtZero, "MHC_ticksAtZero", 0);
        }

        public override string GetTipString()
        {
            return (LabelCap + ": " + CurLevelPercentage.ToStringPercent()).Colorize(ColoredText.TipSectionTitleColor) + " (" + CurLevel.ToString("0.##") + " / " + MaxLevel.ToString("0.##") + ")\n" + def.description;
        }
    }
}

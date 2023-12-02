using RimWorld;
using Verse;

namespace MechHumanlikes
{
    public class Need_MechanicalNeed : Need
    {
        private int ticksAtZero = 0;

        private MHC_MechanicalNeedExtension needExtension;

        private MHC_MechanicalPawnExtension pawnExtension;

        public int TicksAtZero => ticksAtZero;

        public float CoolantDesired => MaxLevel - CurLevel;

        protected MHC_MechanicalNeedExtension NeedExtension
        {
            get
            {
                if (needExtension == null)
                {
                    needExtension = def.GetModExtension<MHC_MechanicalNeedExtension>();
                }
                return needExtension;
            }
        }

        protected MHC_MechanicalPawnExtension PawnExtension
        {
            get
            {
                if (pawnExtension == null)
                {
                    pawnExtension = pawn.def.GetModExtension<MHC_MechanicalPawnExtension>();
                }
                return pawnExtension;
            }
        }

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

        public override void SetInitialLevel()
        {
            CurLevelPercentage = 1.0f;
        }

        public override float MaxLevel
        {
            get
            {
                if (def == null || PawnExtension.mechanicalNeeds.NullOrEmpty() || !PawnExtension.mechanicalNeeds.ContainsKey(def))
                {
                    return 1f;
                }
                return PawnExtension.mechanicalNeeds[def];
            }
        }

        public override void NeedInterval()
        {
            if (IsFrozen)
            {
                return;
            }

            CurLevelPercentage -= 150 * PercentageFallRatePerTick;

            if (CurLevel <= 0.001)
            {
                ticksAtZero += 150;
                if (NeedExtension.hediffToApplyOnEmpty != null)
                {
                    HealthUtility.AdjustSeverity(pawn, NeedExtension.hediffToApplyOnEmpty, 150 * (NeedExtension.hediffRisePerDay / 60000));
                }
            }
            else
            {
                ticksAtZero = 0;
                if (NeedExtension.hediffToApplyOnEmpty != null)
                {
                    HealthUtility.AdjustSeverity(pawn, NeedExtension.hediffToApplyOnEmpty, -150 * (NeedExtension.hediffFallPerDay / 60000));
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

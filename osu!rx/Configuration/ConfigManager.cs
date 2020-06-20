using SimpleIniConfig;
using WindowsInput.Native;

namespace osu_rx.Configuration
{
    public class ConfigManager //TODO: config migration on v1.2.0
    {
        private Config config;

        public bool EnableRelax
        {
            get => config.GetValue("EnableRelax", true);
            set => config.SetValue("EnableRelax", value);
        }

        public PlayStyles PlayStyle
        {
            get => config.GetValue("RelaxPlayStyle", PlayStyles.Singletap);
            set => config.SetValue("RelaxPlayStyle", value);
        }

        public OsuKeys PrimaryKey
        {
            get => config.GetValue("RelaxPrimaryKey", OsuKeys.K1);
            set => config.SetValue("RelaxPrimaryKey", value);
        }

        public OsuKeys SecondaryKey
        {
            get => config.GetValue("RelaxSecondaryKey", OsuKeys.K2);
            set => config.SetValue("RelaxSecondaryKey", value);
        }

        public VirtualKeyCode DoubleDelayKey
        {
            get => config.GetValue("RelaxDoubleDelayKey", VirtualKeyCode.SPACE);
            set => config.SetValue("RelaxDoubleDelayKey", value);
        }

        public int MaxSingletapBPM
        {
            get => config.GetValue("RelaxMaxSingletapBPM", 500);
            set => config.SetValue("RelaxMaxSingletapBPM", value);
        }

        public int AlternateIfLessThan
        {
            get => config.GetValue("RelaxAlternateIfLessThan", 60000 / MaxSingletapBPM);
            set => config.SetValue("RelaxAlternateIfLessThan", value);
        }

        public int AudioOffset
        {
            get => config.GetValue("RelaxAudioOffset", 0);
            set => config.SetValue("RelaxAudioOffset", value);
        }

        public int HoldBeforeSpinnerTime
        {
            get => config.GetValue("RelaxHoldBeforeSpinnerTime", 500);
            set => config.SetValue("RelaxHoldBeforeSpinnerTime", value);
        }

        public bool EnableHitScan
        {
            get => config.GetValue("EnableHitScan", true);
            set => config.SetValue("EnableHitScan", value);
        }

        public bool EnableHitScanPrediction
        {
            get => config.GetValue("HitscanEnablePrediction", true);
            set => config.SetValue("HitscanEnablePrediction", value);
        }

        public float HitScanRadiusMultiplier
        {
            get => config.GetValue<float>("HitscanRadiusMultiplier", 0.9f);
            set => config.SetValue<float>("HitscanRadiusMultiplier", value);
        }

        public int HitScanRadiusAdditional
        {
            get => config.GetValue("HitscanRadiusAdditional", 50);
            set => config.SetValue("HitscanRadiusAdditional", value);
        }

        public int HitScanMaxDistance
        {
            get => config.GetValue("HitscanMaxDistance", 30);
            set => config.SetValue("HitscanMaxDistance", value);
        }

        public int HitScanMissChance
        {
            get => config.GetValue("HitScanMissChance", 20);
            set => config.SetValue("HitScanMissChance", value);
        }

        public bool HitScanMissAfterHitWindow50
        {
            get => config.GetValue("HitScanMissAfterHitWindow50", true);
            set => config.SetValue("HitScanMissAfterHitWindow50", value);
        }

        public int HitTimingsMinOffset
        {
            get => config.GetValue("HitTimingsMinOffset", -40);
            set => config.SetValue("HitTimingsMinOffset", value);
        }

        public int HitTimingsMaxOffset
        {
            get => config.GetValue("HitTimingsMaxOffset", 40);
            set => config.SetValue("HitTimingsMaxOffset", value);
        }

        public int HitTimingsAlternateMinOffset
        {
            get => config.GetValue("HitTimingsAlternateMinOffset", -80);
            set => config.SetValue("HitTimingsAlternateMinOffset", value);
        }

        public int HitTimingsAlternateMaxOffset
        {
            get => config.GetValue("HitTimingsAlternateMaxOffset", 80);
            set => config.SetValue("HitTimingsAlternateMaxOffset", value);
        }

        public int HitTimingsMinHoldTime
        {
            get => config.GetValue("HitTimingsMinHoldTime", 25);
            set => config.SetValue("HitTimingsMinHoldTime", value);
        }

        public int HitTimingsMaxHoldTime
        {
            get => config.GetValue("HitTimingsMaxHoldTime", 50);
            set => config.SetValue("HitTimingsMaxHoldTime", value);
        }

        public int HitTimingsMinSliderHoldTime
        {
            get => config.GetValue("HitTimingsMinSliderHoldTime", -36);
            set => config.SetValue("HitTimingsMinSliderHoldTime", value);
        }

        public int HitTimingsMaxSliderHoldTime
        {
            get => config.GetValue("HitTimingsMaxSliderHoldTime", 50);
            set => config.SetValue("HitTimingsMaxSliderHoldTime", value);
        }

        public float HitTimingsDoubleDelayFactor
        {
            get => config.GetValue("HitTimingsDoubleDelayFactor", 2);
            set => config.SetValue("HitTimingsDoubleDelayFactor", value);
        }

        public bool EnableTimewarp
        {
            get => config.GetValue("EnableTimewarp", false);
            set => config.SetValue("EnableTimewarp", value);
        }

        public double TimewarpRate
        {
            get => config.GetValue("TimewarpRate", 1);
            set => config.SetValue("TimewarpRate", value);
        }

        public bool UseCustomWindowTitle
        {
            get => config.GetValue("UseCustomWindowTitle", false);
            set => config.SetValue("UseCustomWindowTitle", value);
        }

        public string CustomWindowTitle
        {
            get => config.GetValue("CustomWindowTitle", string.Empty);
            set => config.SetValue("CustomWindowTitle", value);
        }

        public ConfigManager() => config = new Config();
    }
}

using SimpleIniConfig;
using WindowsInput.Native;

namespace osu_rx.Configuration
{
    public class ConfigManager
    {
        private Config config;

        public bool EnableRelax
        {
            get => config.GetValue<bool>("EnableRelax", true);
            set => config.SetValue<bool>("EnableRelax", value);
        }

        public PlayStyles PlayStyle
        {
            get => config.GetValue<PlayStyles>("RelaxPlayStyle", PlayStyles.Singletap);
            set => config.SetValue<PlayStyles>("RelaxPlayStyle", value);
        }

        public OsuKeys PrimaryKey
        {
            get => config.GetValue<OsuKeys>("RelaxPrimaryKey", OsuKeys.K1M1);
            set => config.SetValue<OsuKeys>("RelaxPrimaryKey", value);
        }

        public OsuKeys SecondaryKey
        {
            get => config.GetValue<OsuKeys>("RelaxSecondaryKey", OsuKeys.K2M2);
            set => config.SetValue<OsuKeys>("RelaxSecondaryKey", value);
        }

        public VirtualKeyCode HitWindow100Key
        {
            get => config.GetValue<VirtualKeyCode>("RelaxHitWindow100Key", VirtualKeyCode.SPACE);
            set => config.SetValue<VirtualKeyCode>("RelaxHitWindow100Key", value);
        }

        public int MaxSingletapBPM
        {
            get => config.GetValue<int>("RelaxMaxSingletapBPM", 250);
            set => config.SetValue<int>("RelaxMaxSingletapBPM", value);
        }

        public int AudioOffset
        {
            get => config.GetValue<int>("RelaxAudioOffset", 0);
            set => config.SetValue<int>("RelaxAudioOffset", value);
        }

        public int HoldBeforeSpinnerTime
        {
            get => config.GetValue<int>("RelaxHoldBeforeSpinnerTime", 500);
            set => config.SetValue<int>("RelaxHoldBeforeSpinnerTime", value);
        }

        public bool UseCustomWindowTitle
        {
            get => config.GetValue<bool>("UseCustomWindowTitle", false);
            set => config.SetValue<bool>("UseCustomWindowTitle", value);
        }

        public string CustomWindowTitle
        {
            get => config.GetValue<string>("CustomWindowTitle", string.Empty);
            set => config.SetValue<string>("CustomWindowTitle", value);
        }

        public bool EnableHitScan
        {
            get => config.GetValue<bool>("EnableHitScan", true);
            set => config.SetValue<bool>("EnableHitScan", value);
        }

        public bool EnableHitScanPrediction
        {
            get => config.GetValue<bool>("HitscanEnablePrediction", true);
            set => config.SetValue<bool>("HitscanEnablePrediction", value);
        }

        public float HitScanRadiusMultiplier
        {
            get => config.GetValue<float>("HitscanRadiusMultiplier", 0.9f);
            set => config.SetValue<float>("HitscanRadiusMultiplier", value);
        }

        public int HitScanRadiusAdditional
        {
            get => config.GetValue<int>("HitscanRadiusAdditional", 50);
            set => config.SetValue<int>("HitscanRadiusAdditional", value);
        }

        public int HitScanMaxDistance
        {
            get => config.GetValue<int>("HitscanMaxDistance", 30);
            set => config.SetValue<int>("HitscanMaxDistance", value);
        }

        public int HitScanMissChance
        {
            get => config.GetValue<int>("HitScanMissChance", 20);
            set => config.SetValue<int>("HitScanMissChance", value);
        }

        public bool HitScanMissAfterHitWindow50
        {
            get => config.GetValue<bool>("HitScanMissAfterHitWindow50", true);
            set => config.SetValue<bool>("HitScanMissAfterHitWindow50", value);
        }

        public bool EnableTimewarp
        {
            get => config.GetValue<bool>("EnableTimewarp", false);
            set => config.SetValue<bool>("EnableTimewarp", value);
        }

        public double TimewarpRate
        {
            get => config.GetValue<double>("TimewarpRate", 1);
            set => config.SetValue<double>("TimewarpRate", value);
        }

        public ConfigManager() => config = new Config();
    }
}

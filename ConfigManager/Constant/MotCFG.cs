namespace ConfigManager.Constant
{
    public class MotCFG
    {
        // (For Adlink - it is active low to enable motor)
        public const bool ENA_MTR = true;
        public const bool DIS_MTR = false;
        public const bool ON = true;
        public const bool OFF = false;
        // Speed Profile
        public const int Fast = 0;
        public const int Medium = 1;
        public const int Slow = 2;
        public const int Home = 3;

        // Motor array index 
        public const int XAxis = 0;
        public const int YAxis = 1;
        public const int ZAxis = 2;
    }

}

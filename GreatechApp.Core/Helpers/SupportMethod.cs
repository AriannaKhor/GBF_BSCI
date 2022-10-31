namespace GreatechApp.Core.Helpers
{
    public static class SupportMethod
    {
        public static bool IsInRange(double value, double min, double max, bool isInclusive)
        {
            if (isInclusive)
            {
                if (value >= min && value <= max)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                if (value > min && value < max)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
    }
}

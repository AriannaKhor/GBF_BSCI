using ConfigManager;
using ConfigManager.Constant;

namespace InterlockManager.Motion
{
    public interface IMtrInterlock
    {
        bool CheckMtrInterlock(BaseCfg cfg, bool isSkipBusy = false);

        int Provider { get; }
    }
}

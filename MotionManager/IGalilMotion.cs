
namespace MotionManager
{
    public interface IGalilMotion : IBaseMotion
    {
        bool FindIndex(int cardNo, int axis, double speed);
    }
}

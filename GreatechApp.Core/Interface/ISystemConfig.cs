namespace GreatechApp.Core.Interface
{
    public interface ISystemConfig<out T>
    {
        T Cfg { get; }
    }
}

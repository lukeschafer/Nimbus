namespace Nimbus.Hooks
{
    public interface IHookProvider
    {
        IFilterHooks Filters { get; }
    }

    public class HookProvider : IHookProvider
    {
        public IFilterHooks Filters { get; protected set; }
        public HookProvider()
        {
            Filters = new FilterHooks();
        }
    }
}

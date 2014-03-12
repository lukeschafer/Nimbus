namespace Nimbus.Hooks
{
    /// <summary>
    /// Marker interface for IFilterXXXXX
    /// </summary>
    public interface IFilter
    {
        int? Order { get; }
    }
}

using Spark.Models;

namespace Spark.DataAccess.Repository.IRepository
{
    public interface IBillboardRepository : IRepository<Billboard>
    {
        // هنا ممكن تضيف دوال خاصة بالـ Billboard لو محتاج
        Task<IEnumerable<Billboard>> GetAvailableBillboardsAsync();
        Task<Billboard?> GetByCodeAsync(string code);

        Task<Billboard> GetBillboardWithAddressAsync(int id);
        Task<IEnumerable<Billboard>> GetAllBillboardsWithAddressAsync();
        Task UpdateAsync(Billboard entity);
    }
}

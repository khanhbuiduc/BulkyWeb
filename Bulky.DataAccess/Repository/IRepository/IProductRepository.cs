using Bulky.Models;

namespace Bulky.DataAccess.Repository.IRepository
{
    public interface IProductRepository : IRepository<Product>
    {
        void Save();
        IEnumerable<Product> SearchProducts(string searchTerm);
    }
}

using RiotStore.Infrastructure.Data;

namespace RiotStore.Infrastructure.Repositories.Interfaces
{
    public interface ICategoryRepository
    {
        Task<List<Category>> GetAllCategoriesAsync();
    }
}
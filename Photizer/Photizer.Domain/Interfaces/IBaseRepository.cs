using System.Collections.Generic;
using System.Threading.Tasks;

namespace Photizer.Domain.Interfaces
{
    public interface IBaseRepository<T> where T : class
    {
        Task<T> Add(T entity);

        Task<IEnumerable<T>> GetAll();

        Task<bool> Delete(T entity);

        Task ReloadAll();
    }
}
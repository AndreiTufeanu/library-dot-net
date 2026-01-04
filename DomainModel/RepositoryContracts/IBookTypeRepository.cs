using DomainModel.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel.RepositoryContracts
{
    public interface IBookTypeRepository
    {
        Task<BookType> GetByIdAsync(Guid id);
        Task<IEnumerable<BookType>> GetAllAsync();
        Task<bool> ExistsAsync(Guid id);
        Task<BookType> FindByNameAsync(string name);
        Task<BookType> AddAsync(BookType entity);
        Task<BookType> UpdateAsync(BookType entity);
        Task<bool> DeleteAsync(Guid id); 
        Task<bool> HasEditionsAsync(Guid id);
    }
}

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
        BookType GetById(Guid id);
        IEnumerable<BookType> GetAll();
        void Add(BookType entity);
        void Update(BookType entity);
        void Delete(Guid id);
        bool Exists(Guid id);
        void SaveChanges();
    }
}

using DomainModel.Entities;
using Infrastructure.ApplicationContext;
using DomainModel.RepositoryContracts;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class BookTypeRepository : IBookTypeRepository
    {
        private readonly LibraryContext _context;

        public BookTypeRepository(LibraryContext context)
        {
            _context = context;
        }

        public BookType GetById(Guid id)
        {
            return _context.BookTypes.Find(id);
        }

        public IEnumerable<BookType> GetAll()
        {
            return _context.BookTypes.ToList();
        }

        public void Add(BookType entity)
        {
            _context.BookTypes.Add(entity);
        }

        public void Update(BookType entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
        }

        public void Delete(Guid id)
        {
            var bookType = _context.BookTypes.Find(id);
            if (bookType != null)
            {
                _context.BookTypes.Remove(bookType);
            }
        }

        public bool Exists(Guid id)
        {
            return _context.BookTypes.Any(bt => bt.Id == id);
        }

        public void SaveChanges()
        {
            _context.SaveChanges();
        }
    }
}

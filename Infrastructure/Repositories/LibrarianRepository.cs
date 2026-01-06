using DomainModel.Entities;
using DomainModel.RepositoryContracts;
using Infrastructure.ApplicationContext;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class LibrarianRepository : ILibrarianRepository
    {
        private readonly LibraryContext _context;

        public LibrarianRepository(LibraryContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<Librarian> GetByIdAsync(Guid id)
        {
            return await _context.Librarians
                .Include(l => l.ReaderDetails)
                .Include(l => l.ProcessedLoans)
                .FirstOrDefaultAsync(l => l.Id == id);
        }

        public async Task<IEnumerable<Librarian>> GetAllAsync()
        {
            return await _context.Librarians
                .Include(l => l.ReaderDetails)
                .ToListAsync();
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.Librarians.AnyAsync(l => l.Id == id);
        }

        public async Task<Librarian> AddAsync(Librarian entity)
        {
            return await Task.FromResult(_context.Librarians.Add(entity));
        }

        public async Task<Librarian> UpdateAsync(Librarian entity)
        {
            var existingEntity = await _context.Librarians.FindAsync(entity.Id);
            if (existingEntity == null)
                return null;

            _context.Entry(existingEntity).CurrentValues.SetValues(entity);
            return existingEntity;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var librarian = await _context.Librarians.FindAsync(id);
            if (librarian == null)
                return false;

            _context.Librarians.Remove(librarian);
            return true;
        }

        public async Task<Librarian> GetByReaderIdAsync(Guid readerId)
        {
            return await _context.Librarians
                .Include(l => l.ReaderDetails)
                .FirstOrDefaultAsync(l => l.ReaderDetails != null && l.ReaderDetails.Id == readerId);
        }

        public async Task<bool> IsReaderAlsoLibrarianAsync(Guid readerId)
        {
            return await _context.Librarians
                .AnyAsync(l => l.ReaderDetails != null && l.ReaderDetails.Id == readerId);
        }
    }
}

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
    public class ReaderRepository : IReaderRepository
    {
        private readonly LibraryContext _context;

        public ReaderRepository(LibraryContext context)
        {
            _context = context;
        }

        public async Task<Reader> GetByIdAsync(Guid id)
        {
            return await _context.Readers.FindAsync(id);
        }

        public async Task<IEnumerable<Reader>> GetAllAsync()
        {
            return await _context.Readers.ToListAsync();
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.Readers.AnyAsync(r => r.Id == id);
        }

        public async Task<Reader> AddAsync(Reader entity)
        {
            var addedEntity = _context.Readers.Add(entity);
            await _context.SaveChangesAsync();
            return addedEntity;
        }

        public async Task<Reader> UpdateAsync(Reader entity)
        {
            var existingEntity = await _context.Readers.FindAsync(entity.Id);
            if (existingEntity == null)
                return null;

            _context.Entry(existingEntity).CurrentValues.SetValues(entity);
            await _context.SaveChangesAsync();
            return existingEntity;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var reader = await _context.Readers.FindAsync(id);
            if (reader == null)
                return false;

            _context.Readers.Remove(reader);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Reader>> FindByLastNameAsync(string lastName)
        {
            return await _context.Readers
                .Where(r => r.LastName.ToLower() == lastName.ToLower())
                .ToListAsync();
        }

        public async Task<Reader> FindByEmailAsync(string email)
        {
            return await _context.Readers
                .FirstOrDefaultAsync(r => r.Email.ToLower() == email.ToLower());
        }

        public async Task<Reader> FindByPhoneAsync(string phoneNumber)
        {
            return await _context.Readers
                .FirstOrDefaultAsync(r => r.PhoneNumber == phoneNumber);
        }

        public async Task<bool> HasActiveBorrowingsAsync(Guid id)
        {
            return await _context.Borrowings
                .AnyAsync(b => b.Reader.Id == id && b.ReturnDate == null);
        }
    }
}

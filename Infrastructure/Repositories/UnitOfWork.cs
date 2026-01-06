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
    public class UnitOfWork : IUnitOfWork
    {
        private readonly LibraryContext _context;
        private readonly IBookTypeRepository _bookTypeRepository;
        private readonly IBookRepository _bookRepository;
        private readonly IAuthorRepository _authorRepository;
        private readonly IDomainRepository _domainRepository;
        private readonly IBookCopyRepository _bookCopyRepository;
        private readonly IEditionRepository _editionRepository;
        private readonly IReaderRepository _readerRepository;
        private readonly ILibrarianRepository _librarianRepository;
        private readonly IBorrowingRepository _borrowingRepository;
        private DbContextTransaction _transaction;
        private bool _disposed = false;

        public UnitOfWork(
            LibraryContext context,
            IBookTypeRepository bookTypeRepository,
            IBookRepository bookRepository,
            IAuthorRepository authorRepository,
            IDomainRepository domainRepository,
            IBookCopyRepository bookCopyRepository,
            IEditionRepository editionRepository,
            IReaderRepository readerRepository,
            ILibrarianRepository librarianRepository,
            IBorrowingRepository borrowingRepository)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _bookTypeRepository = bookTypeRepository ?? throw new ArgumentNullException(nameof(bookTypeRepository));
            _bookRepository = bookRepository ?? throw new ArgumentNullException(nameof(bookRepository));
            _authorRepository = authorRepository ?? throw new ArgumentNullException(nameof(authorRepository));
            _domainRepository = domainRepository ?? throw new ArgumentNullException(nameof(domainRepository));
            _bookCopyRepository = bookCopyRepository ?? throw new ArgumentNullException(nameof(bookCopyRepository));
            _editionRepository = editionRepository ?? throw new ArgumentNullException(nameof(editionRepository));
            _readerRepository = readerRepository ?? throw new ArgumentNullException(nameof(readerRepository));
            _librarianRepository = librarianRepository ?? throw new ArgumentNullException(nameof(librarianRepository));
            _borrowingRepository = borrowingRepository ?? throw new ArgumentNullException(nameof(borrowingRepository));
        }

        public IBookTypeRepository BookTypes => _bookTypeRepository;
        public IBookRepository Books => _bookRepository;
        public IAuthorRepository Authors => _authorRepository;
        public IDomainRepository Domains => _domainRepository;
        public IBookCopyRepository BookCopies => _bookCopyRepository;
        public IEditionRepository Editions => _editionRepository;
        public IReaderRepository Readers => _readerRepository;
        public ILibrarianRepository Librarians => _librarianRepository;
        public IBorrowingRepository Borrowings => _borrowingRepository;

        public async Task BeginTransactionAsync()
        {
            if (_transaction != null)
            {
                throw new InvalidOperationException("Transaction already in progress");
            }
            _transaction = _context.Database.BeginTransaction();
            await Task.CompletedTask;
        }

        public async Task CommitAsync()
        {
            try
            {
                await SaveChangesAsync();
                _transaction?.Commit();
            }
            catch
            {
                await RollbackAsync();
                throw;
            }
            finally
            {
                if (_transaction != null)
                {
                    _transaction.Dispose();
                    _transaction = null;
                }
            }
        }

        public async Task RollbackAsync()
        {
            try
            {
                _transaction?.Rollback();
            }
            finally
            {
                if (_transaction != null)
                {
                    _transaction.Dispose();
                    _transaction = null;
                }
            }
            await Task.CompletedTask;
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _transaction?.Dispose();
                    _context?.Dispose();
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}

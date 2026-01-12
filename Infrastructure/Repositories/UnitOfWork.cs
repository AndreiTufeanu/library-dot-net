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
    /// <summary>Coordinates database operations across multiple repositories within a single transaction.</summary>
    /// <remarks>
    /// Implements <see cref="IUnitOfWork"/> to provide transaction management and repository coordination.
    /// Ensures all repository operations share the same database context for consistency.
    /// </remarks>
    public class UnitOfWork : IUnitOfWork
    {
        /// <summary>The Entity Framework database context shared by all repositories.</summary>
        private readonly LibraryContext _context;

        /// <summary>The repository for accessing book type data.</summary>
        private readonly IBookTypeRepository _bookTypeRepository;

        /// <summary>The repository for accessing book data.</summary>
        private readonly IBookRepository _bookRepository;

        /// <summary>The repository for accessing author data.</summary>
        private readonly IAuthorRepository _authorRepository;

        /// <summary>The repository for accessing domain data.</summary>
        private readonly IDomainRepository _domainRepository;

        /// <summary>The repository for accessing book copy data.</summary>
        private readonly IBookCopyRepository _bookCopyRepository;

        /// <summary>The repository for accessing edition data.</summary>
        private readonly IEditionRepository _editionRepository;

        /// <summary>The repository for accessing reader data.</summary>
        private readonly IReaderRepository _readerRepository;

        /// <summary>The repository for accessing librarian data.</summary>
        private readonly ILibrarianRepository _librarianRepository;

        /// <summary>The repository for accessing borrowing data.</summary>
        private readonly IBorrowingRepository _borrowingRepository;

        /// <summary>The current database transaction, if one is active.</summary>
        private DbContextTransaction _transaction;

        /// <summary>Flag indicating whether this unit of work has been disposed.</summary>
        private bool _disposed = false;

        /// <summary>Initializes a new instance of the <see cref="UnitOfWork"/> class.</summary>
        /// <param name="context">The database context shared by all repositories.</param>
        /// <param name="bookTypeRepository">The repository for accessing book type data.</param>
        /// <param name="bookRepository">The repository for accessing book data.</param>
        /// <param name="authorRepository">The repository for accessing author data.</param>
        /// <param name="domainRepository">The repository for accessing domain data.</param>
        /// <param name="bookCopyRepository">The repository for accessing book copy data.</param>
        /// <param name="editionRepository">The repository for accessing edition data.</param>
        /// <param name="readerRepository">The repository for accessing reader data.</param>
        /// <param name="librarianRepository">The repository for accessing librarian data.</param>
        /// <param name="borrowingRepository">The repository for accessing borrowing data.</param>
        /// <exception cref="ArgumentNullException">Thrown when any required parameter is null.</exception>
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

        /// <inheritdoc/>
        public IBookTypeRepository BookTypes => _bookTypeRepository;

        /// <inheritdoc/>
        public IBookRepository Books => _bookRepository;

        /// <inheritdoc/>
        public IAuthorRepository Authors => _authorRepository;

        /// <inheritdoc/>
        public IDomainRepository Domains => _domainRepository;

        /// <inheritdoc/>
        public IBookCopyRepository BookCopies => _bookCopyRepository;

        /// <inheritdoc/>
        public IEditionRepository Editions => _editionRepository;

        /// <inheritdoc/>
        public IReaderRepository Readers => _readerRepository;

        /// <inheritdoc/>
        public ILibrarianRepository Librarians => _librarianRepository;

        /// <inheritdoc/>
        public IBorrowingRepository Borrowings => _borrowingRepository;


        /// <inheritdoc/>
        public async Task BeginTransactionAsync()
        {
            if (_transaction != null)
            {
                throw new InvalidOperationException("Transaction already in progress");
            }
            _transaction = _context.Database.BeginTransaction();
            await Task.CompletedTask;
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        /// <param name="disposing">
        /// <c>true</c> to release both managed and unmanaged resources; 
        /// <c>false</c> to release only unmanaged resources.
        /// </param>
        /// <remarks>
        /// <para>
        /// This method is called by the public <see cref="Dispose()"/> method and the finalizer.
        /// When the <paramref name="disposing"/> parameter is <c>true</c>, the method releases all
        /// managed resources (database transaction and context) held by this instance.
        /// </para>
        /// <para>
        /// The method ensures that resources are only disposed once by checking the
        /// <c>_disposed</c> flag. After disposal, the flag is set to <c>true</c> to prevent
        /// multiple disposal attempts.
        /// </para>
        /// </remarks>
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

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel.RepositoryContracts
{
    /// <summary>
    /// Defines a unit of work interface for coordinating database transactions and managing repositories.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The unit of work pattern is used to maintain a list of objects affected by a business transaction
    /// and coordinate the writing out of changes and the resolution of concurrency problems.
    /// </para>
    /// <para>
    /// This interface follows the Repository Pattern and provides access to all entity-specific repositories,
    /// transaction management, and atomic commit operations.
    /// </para>
    /// <para>
    /// Implementations of this interface should ensure that all repository operations within a single
    /// unit of work share the same <see cref="DbContext"/> instance to maintain consistency.
    /// </para>
    /// <example>
    /// The following example demonstrates a typical usage pattern:
    /// <code>
    /// using (var unitOfWork = serviceProvider.GetService&lt;IUnitOfWork&gt;())
    /// {
    ///     await unitOfWork.BeginTransactionAsync();
    ///     try
    ///     {
    ///         var book = new Book(10) { Title = "New Book" };
    ///         await unitOfWork.Books.AddAsync(book);
    ///         
    ///         var author = await unitOfWork.Authors.GetByIdAsync(authorId);
    ///         book.Authors.Add(author);
    ///         
    ///         await unitOfWork.SaveChangesAsync();
    ///         await unitOfWork.CommitAsync();
    ///     }
    ///     catch
    ///     {
    ///         await unitOfWork.RollbackAsync();
    ///         throw;
    ///     }
    /// }
    /// </code>
    /// </example>
    /// </remarks>
    public interface IUnitOfWork : IDisposable
    {
        #region Repository properties

        /// <summary>
        /// Gets the repository for managing <see cref="Entities.Author"/> entities.
        /// </summary>
        /// <value>The author repository.</value>
        IAuthorRepository Authors { get; }

        /// <summary>
        /// Gets the repository for managing <see cref="Entities.Book"/> entities.
        /// </summary>
        /// <value>The book repository.</value>
        IBookRepository Books { get; }

        /// <summary>
        /// Gets the repository for managing <see cref="Entities.BookCopy"/> entities.
        /// </summary>
        /// <value>The book copy repository.</value>
        IBookCopyRepository BookCopies { get; }

        /// <summary>
        /// Gets the repository for managing <see cref="Entities.BookType"/> entities.
        /// </summary>
        /// <value>The book type repository.</value>
        IBookTypeRepository BookTypes { get; }

        /// <summary>
        /// Gets the repository for managing <see cref="Entities.Domain"/> entities.
        /// </summary>
        /// <value>The domain repository.</value>
        IDomainRepository Domains { get; }

        /// <summary>
        /// Gets the repository for managing <see cref="Entities.Edition"/> entities.
        /// </summary>
        /// <value>The edition repository.</value>
        IEditionRepository Editions { get; }

        /// <summary>
        /// Gets the repository for managing <see cref="Entities.Reader"/> entities.
        /// </summary>
        /// <value>The reader repository.</value>
        IReaderRepository Readers { get; }

        /// <summary>
        /// Gets the repository for managing <see cref="Entities.Librarian"/> entities.
        /// </summary>
        /// <value>The librarian repository.</value>
        ILibrarianRepository Librarians { get; }

        /// <summary>
        /// Gets the repository for managing <see cref="Entities.Borrowing"/> entities.
        /// </summary>
        /// <value>The borrowing repository.</value>
        IBorrowingRepository Borrowings { get; }

        #endregion

        #region Transaction management

        /// <summary>
        /// Begins a new database transaction asynchronously.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when attempting to begin a transaction while another transaction is already in progress.
        /// </exception>
        /// <remarks>
        /// <para>
        /// This method starts a new database transaction. All subsequent operations on repositories
        /// within this unit of work will be part of this transaction until it is committed or rolled back.
        /// </para>
        /// <para>
        /// Transactions are necessary to ensure data consistency when performing multiple related
        /// database operations that must either all succeed or all fail.
        /// </para>
        /// <para>
        /// This method must be called before <see cref="CommitAsync"/> or <see cref="RollbackAsync"/>.
        /// </para>
        /// </remarks>
        Task BeginTransactionAsync();

        /// <summary>
        /// Commits the current transaction asynchronously, saving all changes to the database.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when there is no active transaction to commit.
        /// </exception>
        /// <remarks>
        /// <para>
        /// This method saves all pending changes to the database and commits the transaction.
        /// After successful completion, the transaction is disposed and cannot be reused.
        /// </para>
        /// <para>
        /// If an exception occurs during commit, the transaction is automatically rolled back
        /// and the exception is re-thrown.
        /// </para>
        /// <para>
        /// This method calls <see cref="SaveChangesAsync"/> internally before committing.
        /// </para>
        /// </remarks>
        Task CommitAsync();

        /// <summary>
        /// Rolls back the current transaction asynchronously, discarding all pending changes.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <remarks>
        /// <para>
        /// This method discards all changes made within the current transaction and restores
        /// the database to its state before the transaction began.
        /// </para>
        /// <para>
        /// After rollback, the transaction is disposed and cannot be reused.
        /// </para>
        /// <para>
        /// This method should be called in exception handlers to ensure data consistency
        /// when an error occurs during transaction processing.
        /// </para>
        /// </remarks>
        Task RollbackAsync();

        #endregion

        #region Save Changes

        /// <summary>
        /// Saves all changes made in this unit of work to the database asynchronously.
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous save operation. The task result contains
        /// the number of state entries written to the database.
        /// </returns>
        /// <remarks>
        /// <para>
        /// This method persists all pending changes (inserts, updates, deletes) made to entities
        /// through the repositories in this unit of work.
        /// </para>
        /// <para>
        /// When called within a transaction (after <see cref="BeginTransactionAsync"/>), changes
        /// are saved to the database but not committed until <see cref="CommitAsync"/> is called.
        /// </para>
        /// <para>
        /// This method should be called after making all necessary changes to ensure they are
        /// persisted to the database.
        /// </para>
        /// </remarks>
        Task<int> SaveChangesAsync();

        #endregion
    }
}

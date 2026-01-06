using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel.RepositoryContracts
{
    public interface IUnitOfWork : IDisposable
    {
        // Repository properties
        IAuthorRepository Authors { get; }
        IBookRepository Books { get; }
        IBookCopyRepository BookCopies { get; }
        IBookTypeRepository BookTypes { get; }
        IDomainRepository Domains { get; }
        IEditionRepository Editions { get; }
        IReaderRepository Readers { get; }
        ILibrarianRepository Librarians { get; }
        IBorrowingRepository Borrowings { get; }

        // Transaction management
        Task BeginTransactionAsync();
        Task CommitAsync();
        Task RollbackAsync();

        // Save changes
        Task<int> SaveChangesAsync();
    }
}

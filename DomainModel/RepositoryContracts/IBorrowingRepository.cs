using DomainModel.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel.RepositoryContracts
{
    public interface IBorrowingRepository : IRepository<Borrowing>
    {
        Task<IEnumerable<Borrowing>> GetActiveByReaderAsync(Guid readerId);
        Task<IEnumerable<Borrowing>> GetByReaderAsync(Guid readerId, DateTime? startDate = null, DateTime? endDate = null);
        Task<IEnumerable<Borrowing>> GetByLibrarianAsync(Guid librarianId, DateTime? startDate = null, DateTime? endDate = null);
        Task<IEnumerable<Borrowing>> GetOverdueAsync();
        Task<IEnumerable<Borrowing>> GetByBookCopyAsync(Guid bookCopyId);
        Task<IEnumerable<Borrowing>> GetByBookAsync(Guid bookId);
        Task<bool> IsBookCopyCurrentlyBorrowedAsync(Guid bookCopyId);
        Task<int> GetCountByReaderInPeriodAsync(Guid readerId, DateTime startDate, DateTime endDate);
        Task<int> GetCountByReaderAndDomainInPeriodAsync(Guid readerId, Guid domainId, DateTime startDate);
        Task<int> GetTotalExtensionDaysByReaderInPeriodAsync(Guid readerId, DateTime startDate);
        Task<DateTime?> GetLastBorrowDateForBookByReaderAsync(Guid readerId, Guid bookId);
        Task<int> GetCountByReaderOnDateAsync(Guid readerId, DateTime date);
        Task<int> GetCountByLibrarianOnDateAsync(Guid librarianId, DateTime date);
        Task<int> GetCountByReaderAndDomainsInPeriodAsync(Guid readerId, List<Guid> domainIds, DateTime startDate);
    }
}

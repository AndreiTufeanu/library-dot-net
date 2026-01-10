using DomainModel.Entities;
using DomainModel.RepositoryContracts;
using FluentValidation;
using Microsoft.Extensions.Logging;
using ServiceLayer.Exceptions;
using ServiceLayer.Helpers;
using ServiceLayer.ServiceContracts;
using ServiceLayer.ServiceContracts.HelperServiceContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLayer.Services
{
    public class BookService : BaseService, IBookService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IValidator<Book> _validator;
        private readonly IBookHelperService _bookHelperService;

        public BookService(
            IUnitOfWork unitOfWork,
            IValidator<Book> validator,
            IBookHelperService bookHelperService,
            ILogger<BookService> logger)
            : base(logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
            _bookHelperService = bookHelperService ?? throw new ArgumentNullException(nameof(bookHelperService));
        }

        public async Task<ServiceResult<Book>> CreateAsync(Book book)
        {
            return await ExecuteServiceOperationAsync(async () =>
            {
                ValidationHelper.Validate(book, _validator);
                await _bookHelperService.ValidateMaxDomainsPerBookAsync(book.Domains);

                return await _unitOfWork.Books.AddAsync(book);

            }, nameof(CreateAsync));
        }

        public async Task<ServiceResult<Book>> GetByIdAsync(Guid id)
        {
            return await ExecuteServiceOperationAsync(async () =>
            {
                var book = await _unitOfWork.Books.GetByIdAsync(id);
                if (book == null)
                {
                    throw new NotFoundException(nameof(Book), id);
                }
                return book;

            }, nameof(GetByIdAsync));
        }

        public async Task<ServiceResult<IEnumerable<Book>>> GetAllAsync()
        {
            return await ExecuteServiceOperationAsync(async () =>
            {
                return await _unitOfWork.Books.GetAllAsync();

            }, nameof(GetAllAsync));
        }

        public async Task<ServiceResult<bool>> UpdateAsync(Book book)
        {
            return await ExecuteServiceOperationAsync(async () =>
            {
                if (!await _unitOfWork.Books.ExistsAsync(book.Id))
                {
                    throw new NotFoundException(nameof(Book), book.Id);
                }

                ValidationHelper.Validate(book, _validator);
                await _bookHelperService.ValidateMaxDomainsPerBookAsync(book.Domains);

                await _unitOfWork.Books.UpdateAsync(book);
                await _unitOfWork.SaveChangesAsync();
                return true;

            }, nameof(UpdateAsync));
        }

        public async Task<ServiceResult<bool>> DeleteAsync(Guid id)
        {
            return await ExecuteServiceOperationAsync(async () =>
            {
                var book = await _unitOfWork.Books.GetByIdAsync(id);
                if (book == null)
                {
                    throw new NotFoundException(nameof(Book), id);
                }

                if (await _unitOfWork.Books.HasPhysicalCopiesAsync(id))
                {
                    throw new BusinessRuleException("Cannot delete a book that has physical copies.");
                }

                var success = await _unitOfWork.Books.DeleteAsync(id);
                if (!success)
                {
                    throw new InvalidOperationException("Failed to delete book");
                }

                await _unitOfWork.SaveChangesAsync();
                return true;

            }, nameof(DeleteAsync));
        }

        public async Task<ServiceResult<bool>> ExistsAsync(Guid id)
        {
            return await ExecuteServiceOperationAsync(async () =>
            {
                return await _unitOfWork.Books.ExistsAsync(id);

            }, nameof(ExistsAsync));
        }

        public async Task<ServiceResult<IEnumerable<Book>>> FindByTitleAsync(string title)
        {
            return await ExecuteServiceOperationAsync(async () =>
            {
                if (string.IsNullOrWhiteSpace(title))
                {
                    throw new AggregateValidationException("Title cannot be empty.");
                }

                return await _unitOfWork.Books.FindByTitleAsync(title);

            }, nameof(FindByTitleAsync));
        }

        public async Task<ServiceResult<IEnumerable<Book>>> FindByDomainAsync(Guid domainId)
        {
            return await ExecuteServiceOperationAsync(async () =>
            {
                var domain = await _unitOfWork.Domains.GetByIdAsync(domainId);
                if (domain == null)
                {
                    throw new NotFoundException(nameof(Domain), domainId);
                }

                return await _unitOfWork.Books.FindByDomainAsync(domainId);

            }, nameof(FindByDomainAsync));
        }

        public async Task<ServiceResult<IEnumerable<Book>>> FindByAuthorAsync(Guid authorId)
        {
            return await ExecuteServiceOperationAsync(async () =>
            {
                var author = await _unitOfWork.Authors.GetByIdAsync(authorId);
                if (author == null)
                {
                    throw new NotFoundException(nameof(Author), authorId);
                }

                return await _unitOfWork.Books.FindByAuthorAsync(authorId);

            }, nameof(FindByAuthorAsync));
        }

        public async Task<ServiceResult<IEnumerable<Book>>> GetAvailableForBorrowingAsync()
        {
            return await ExecuteServiceOperationAsync(async () =>
            {
                return await _unitOfWork.Books.GetAvailableForBorrowingAsync();

            }, nameof(GetAvailableForBorrowingAsync));
        }
    }
}

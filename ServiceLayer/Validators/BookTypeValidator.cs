using DomainModel.Entities;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLayer.Validators
{
    public class BookTypeValidator : AbstractValidator<BookType>
    {
        public BookTypeValidator()
        {
            
        }
    }
}

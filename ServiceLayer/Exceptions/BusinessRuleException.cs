using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLayer.Exceptions
{
    /// <summary>
    /// Represents an exception that occurs when a business rule validation fails.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This exception is thrown when an operation violates one or more business rules
    /// defined for the library management system. Business rules are domain-specific
    /// constraints that ensure data integrity and compliance with library policies.
    /// </para>
    /// <para>
    /// Unlike <see cref="ValidationException"/>, which is used for input validation,
    /// <see cref="BusinessRuleException"/> is used for violations of complex business
    /// logic that may involve multiple entities or database state.
    /// </para>
    /// <para>
    /// Examples of business rule violations include:
    /// <list type="bullet">
    /// <item><description>Attempting to delete an author with associated books</description></item>
    /// <item><description>Borrowing a book when available copies are below the threshold</description></item>
    /// <item><description>Violating borrowing limits (NMC, C, D, LIM, etc.)</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public class BusinessRuleException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BusinessRuleException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the business rule violation.</param>
        public BusinessRuleException(string message) : base(message)
        {
        }
    }
}

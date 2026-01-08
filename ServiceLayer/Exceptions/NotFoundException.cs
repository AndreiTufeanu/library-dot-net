using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLayer.Exceptions
{
    /// <summary>
    /// Represents an exception that occurs when a requested resource is not found.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This exception is thrown when an operation attempts to access, update, or delete
    /// an entity that does not exist in the database. It is typically thrown by service
    /// layer methods when repository operations return null or false for existence checks.
    /// </para>
    /// <para>
    /// This exception provides constructors for both generic entity-not-found scenarios
    /// and specific entity-type scenarios with entity identifiers.
    /// </para>
    /// </remarks>
    public class NotFoundException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NotFoundException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the resource not found error.</param>
        public NotFoundException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotFoundException"/> class for a specific entity and identifier.
        /// </summary>
        /// <param name="entityName">The name of the entity type that was not found (e.g., "Book", "Author").</param>
        /// <param name="id">The identifier of the entity that was not found.</param>
        public NotFoundException(string entityName, object id)
            : base($"{entityName} with ID '{id}' not found.")
        {
        }
    }
}

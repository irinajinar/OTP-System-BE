using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Exceptions
{
    public class MultivalidationException : Exception
    {
        public List<string> ValidationErrors { get; }

        public MultivalidationException(int v, string validationError)
            : base(validationError)
        {
            ValidationErrors = new List<string> { validationError };
        }

        public MultivalidationException(List<string> validationErrors)
            : base(string.Join(Environment.NewLine, validationErrors))
        {
            ValidationErrors = validationErrors ?? throw new ArgumentNullException(nameof(validationErrors));
        }

        public MultivalidationException(string? message) : base(message)
        {
        }
    }
}


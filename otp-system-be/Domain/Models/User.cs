using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class User
    {
        public User()
        {
            Id = Guid.NewGuid();
        }

        public User(string email, string personalIdentificationNumber, string pin)
            : this()
        {           
            Email = email;
            PersonalIdentificationNumber = personalIdentificationNumber;
            Pin = pin;
        }

        [Key]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Personal identification number is required")]
        [StringLength(50, ErrorMessage = "Personal identification number must be between 1 and 13 characters", MinimumLength = 1)]
        public string PersonalIdentificationNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "PIN is required")]
        [StringLength(50, ErrorMessage = "PIN must be between 1 and 4 characters", MinimumLength = 1)]
        public string Pin { get; set; } = string.Empty;

        

        
    }
}

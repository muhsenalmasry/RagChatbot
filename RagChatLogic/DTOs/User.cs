namespace RagChatLogic.DTOs
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Holds user related data.
    /// </summary>
    public class User
    {
        /// <summary>
        /// User identifier.
        /// </summary>
        public string? Id { get; set; }

        /// <summary>
        /// First name.
        /// </summary>
        [Required]
        public string? FirstName { get; set; }

        /// <summary>
        /// Last name.
        /// </summary>
        [Required]
        public string? LastName { get; set; }

        /// <summary>
        /// Email address.
        /// </summary>
        [Required]
        [EmailAddress]
        [DataType(DataType.EmailAddress)]
        public string? Email { get; set; }

        /// <summary>
        /// Passwoord.
        /// </summary>
        [Required, DataType(DataType.Password)]
        public string? Password { get; set; }

        /// <summary>
        /// Confirm password.
        /// </summary>
        [Required]
        [DataType(DataType.Password)]
        [Compare(nameof(Password))]
        public string? ConfirmPassword { get; set; }
    }
}

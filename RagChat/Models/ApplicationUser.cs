namespace RagChat.Models
{
    using Microsoft.AspNetCore.Identity;

    /// <summary>
    /// Application user.
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        /// <summary>
        /// Application user's
        /// full name.
        /// </summary>
        public string FullName { get; set; }
    }
}

namespace RagChat.Models
{
    using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore;
    using RagChatLogic.DTOs;

    /// <summary>
    /// Rag chatbot database context.
    /// </summary>
    public class RagChatbotDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<UserIndex> UserIndexes { get; set; }
        public DbSet<Message> Messages { get; set; }

        /// <summary>
        /// Rag chatbot database context.
        /// </summary>
        public RagChatbotDbContext(DbContextOptions<RagChatbotDbContext> options)
        : base(options)
        {
        }

        /// <summary>
        /// On model creating.
        /// </summary>
        /// <param name="builder"></param>
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }
    }
}

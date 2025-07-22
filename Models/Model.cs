using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Author
{
    [Key]
    public int Id { get; set; }
    public string Name { get; set; }

}


public class Category
{
    [Key]
    public int Id { get; set; }
    public string Name { get; set; }
    public virtual ICollection<Book> Books { get; set; }
}

public class Book
{
    [Key]
    public int Id { get; set; }
    public string Title { get; set; }
    [ForeignKey("Author")]
    public int AuthorId { get; set; }
    public string ImageBase64 { get; set; }
    public DateTime PublishedDate { get; set; }
    public virtual List<BookCopy> BookCopies { get; set; }
    public virtual Author Author { get; set; }
    public virtual ICollection<Category> Categories { get; set; }
}

public class BookCopy
{
    [Key]
    public int Id { get; set; }
    [ForeignKey("Book")]
    public int BookId { get; set; }
    // dang muon, unavailable, available
    public string Status { get; set; } // "available"
    public string Condition { get; set; } // "new", "old", "mid"
    // rental
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public virtual Book Book { get; set; }
}

public class Rental
{
    [Key]
    public int Id { get; set; }
    [ForeignKey("User")]
    public int UserId { get; set; }
    [ForeignKey("BookCopy")]
    public int BookCopyId { get; set; }
    public string Status { get; set; }
    public int RenewCount { get; set; } // count number renew book
    public DateTime RentalDate { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public virtual User User { get; set; }
    public virtual BookCopy Book { get; set; }
}

public class Return
{
    [Key]
    public int Id { get; set; }
    [ForeignKey("Rental")]
    public int RentalId { get; set; }
    public string Condition { get; set; }
    public DateTime ReturnDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public virtual Rental Rental { get; set; }
}

public class User
{
    [Key]
    public int Id { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public string Email { get; set; }
    public string? PasswordResetToken { get; set; } // for password reset
    public long? PasswordResetTokenExpiryUnixTS { get; set; }
    public bool Active { get; set; }
    public string Role { get; set; }
    // edit website/admin
}
public class Rule
{
    [Key]
    public int Id { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class PrnContext : DbContext
{
    public PrnContext()
    {

    }

    public PrnContext(DbContextOptions<PrnContext> options) : base(options) { }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();
            var connectionString = configuration.GetConnectionString("Default");
            optionsBuilder.UseSqlServer(connectionString);
        }
    }
    public DbSet<Author> Authors { get; set; }
    public DbSet<Book> Books { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<BookCopy> BookCopies { get; set; }
    public DbSet<Rental> Rentals { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Return> Returns { get; set; }
    public DbSet<Rule> Rules { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Author>().HasData(
            new Author { Id = 1, Name = "George Orwell" },
            new Author { Id = 2, Name = "Harper Lee" },
            new Author { Id = 3, Name = "F. Scott Fitzgerald" }
        );

        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Classic" },
            new Category { Id = 2, Name = "Dystopian" },
            new Category { Id = 3, Name = "Drama" }
        );

        modelBuilder.Entity<Book>(entity =>
        {
            entity.HasData(
                        new Book { Id = 1, Title = "1984", AuthorId = 1, PublishedDate = new DateTime(1949, 6, 8), ImageBase64 = "https://bizweb.dktcdn.net/100/326/228/products/1984-by-george-orwell-bookworm-hanoi-038adcb4-d1cc-49ee-85eb-50f188229ecf.jpg" },
                        new Book { Id = 2, Title = "To Kill a Mockingbird", AuthorId = 2, PublishedDate = new DateTime(1960, 7, 11), ImageBase64 = "https://upload.wikimedia.org/wikipedia/commons/thumb/4/4f/To_Kill_a_Mockingbird_%28first_edition_cover%29.jpg/1200px-To_Kill_a_Mockingbird_%28first_edition_cover%29.jpg" },
                        new Book { Id = 3, Title = "The Great Gatsby", AuthorId = 3, PublishedDate = new DateTime(1925, 4, 10), ImageBase64 = "https://upload.wikimedia.org/wikipedia/commons/7/7a/The_Great_Gatsby_Cover_1925_Retouched.jpg" }
                    );


            entity.HasMany(b => b.Categories)
            .WithMany(c => c.Books)
            .UsingEntity(j => j.HasData(
                new { BooksId = 1, CategoriesId = 1 },
                new { BooksId = 1, CategoriesId = 2 },
                new { BooksId = 1, CategoriesId = 3 },
                new { BooksId = 2, CategoriesId = 1 },
                new { BooksId = 2, CategoriesId = 2 },
                new { BooksId = 2, CategoriesId = 3 },
                new { BooksId = 3, CategoriesId = 1 },
                new { BooksId = 3, CategoriesId = 2 },
                new { BooksId = 3, CategoriesId = 3 }
            ));
        });

        modelBuilder.Entity<BookCopy>().HasData(
            new BookCopy { Id = 1, BookId = 1, Status = "available", Condition = "new", CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
            new BookCopy { Id = 2, BookId = 1, Status = "unavailable", Condition = "old", CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
            new BookCopy { Id = 3, BookId = 2, Status = "available", Condition = "mid", CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
            new BookCopy { Id = 4, BookId = 3, Status = "available", Condition = "new", CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now }
        );

        modelBuilder.Entity<User>().HasData(
            new User { Id = 1, Username = "admin", Password = "a", Active = true, Role = "admin", Email = "admin@example.com" },
            new User { Id = 2, Username = "john_doe", Password = "a", Active = true, Role = "user", Email = "john_doe@example.com" },
            new User { Id = 3, Username = "librarian", Password = "a", Active = true, Role = "staff", Email = "staff1@example.com" },
            new User { Id = 4, Username = "abc", Password = "a", Active = true, Role = "user", Email = "abc@example.com" },
            new User { Id = 5, Username = "a", Password = "a", Active = true, Role = "user", Email = "abcd@example.com" }
        );

        modelBuilder.Entity<Rule>().HasData(
           new Rule { Id = 1, Title = "rule 1", Content = "none", CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now }
        );


        modelBuilder.Entity<Return>().HasData(
           new Return { Id = 1, RentalId = 1, Condition = "100%", ReturnDate = DateTime.Now, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now }
        );

        modelBuilder.Entity<Rental>().HasData(
            new Rental
            {
                Id = 1,
                UserId = 2, // john_doe
                BookCopyId = 1, // BookId = 1, "1984"
                Status = RentalStatus.Approved,
                RentalDate = new DateTime(2025, 7, 1),
                DueDate = new DateTime(2025, 7, 15),
                CreatedAt = new DateTime(2025, 7, 1),
                UpdatedAt = new DateTime(2025, 7, 1)
            },
            new Rental
            {
                Id = 2,
                UserId = 2,
                BookCopyId = 2,
                Status = RentalStatus.Returned,
                RentalDate = new DateTime(2025, 6, 10),
                DueDate = new DateTime(2025, 6, 24),
                CreatedAt = new DateTime(2025, 6, 10),
                UpdatedAt = new DateTime(2025, 6, 24)
            }
        );
    }

    public static class RentalStatus
    {
        public const string Pending = "Pending";
        public const string Approved = "Approved";
        public const string Returned = "Returned";
        public const string Cancelled = "Cancelled";
    }

    public static class BookCopyStatus{
        public const string Available = "Available";
        public const string Unavailable = "Unavailable";
    }
}

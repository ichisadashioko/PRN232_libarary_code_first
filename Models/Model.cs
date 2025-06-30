using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using System.IO;



using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection.Emit;
using System.Security.Cryptography.X509Certificates;

public class Author
{
    [Key]
    public int Id { get; set; }
    public string Name { get; set; }

}
//ád

public class Category
{
    [Key]
    public int Id { get; set; }
    public string Name { get; set; }
    public virtual List<Book> Books { get; set; } = [];

}

public class BookCategory
{
    [ForeignKey("Book")]
    public int BookId { get; set; }
    [ForeignKey("Category")]
    public int CategoryId { get; set; }
    public virtual Book Book { get; set; } = null;
    public virtual Category Category { get; set; } = null;
}

public class Book
{
    [Key]
    public int Id { get; set; }
    public string Title { get; set; }
    [ForeignKey("Author")]
    public int AuthorId { get; set; }
    public string ImageBase64 { get; set; } // in database
    //public string ImageUrl { get; set; } // base64 png
    // image 2-4mb bytes => string/base64 png
    public DateTime PublishedDate { get; set; }
    public virtual List<BookCopy> BookCopies { get; set; }
    public virtual Author Author { get; set; }
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
    public bool Active { get; set; }
    public string Role { get; set; }
    // edit website/admin
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
    public DbSet<BookCategory> BookCategory { get; set; }
    public DbSet<BookCopy> BookCopies { get; set; }
    public DbSet<Rental> Rentals { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Return> Returns { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<BookCategory>().HasKey(bc => new { bc.BookId, bc.CategoryId });
    }
}

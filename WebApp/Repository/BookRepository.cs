using Libria.Data;
using Libria.Models;
using Microsoft.EntityFrameworkCore;

namespace Libria.Repository
{
    public class BookRepository : IBookRepository
    {
        private readonly LibriaDbContext _context;

        public BookRepository(LibriaDbContext context) => _context = context;

        public async Task<Book?> GetBookByIdAsync(int bookId)
		{
            return await (from book in _context.Books
                          where book.BookId == bookId
                          select book).Include(b => b.Authors).FirstOrDefaultAsync();
			//         return new Book { BookId = 1, Title = "BoOok 1", ImageUrl = "img/book_cover/1.jpg", SalePrice = 1200, Price = 1000, Authors = new List<Author> { new Author { Name = "Author 1" } },
			//         Description = "Lorem ipsum dolor sit amet.Lorem ipsum dolor sit amet.Lorem ipsum dolor sit amet.Lorem ipsum dolor sit amet.Lorem ipsum dolor sit amet.Lorem ipsum dolor sit amet." +
			//"Lorem ipsum dolor sit amet.Lorem ipsum dolor sit amet.Lorem ipsum dolor sit amet.Lorem ipsum dolor sit amet.Lorem ipsum dolor sit amet.Lorem ipsum dolor sit amet" +
			//"Lorem ipsum dolor sit amet.Lorem ipsum dolor sit amet.Lorem ipsum dolor sit amet.Lorem ipsum dolor sit amet.Lorem ipsum dolor sit amet.Lorem ipsum dolor sit amet"
			//};
		}

		public async Task<List<Book>> GetNewBooksAsync(int booksNum)
        {
            return await (from book in _context.Books
                          orderby book.BookId descending
                          select book).Take(booksNum).Include(b => b.Authors).ToListAsync();
            //return new List<Book>()
            //{
            //    new Book { BookId = 1, Title = "BoOok 1", Price = 1000, ImageUrl="img/book_cover/1.jpg", Authors= new List<Author> { new Author { Name = "Author 1" } } },
            //    new Book { BookId = 2, Title = "BoOok #2", Available=false, ImageUrl="img/book_cover/2.png", Price = 500, Authors= new List<Author> { new Author { Name = "Author 1" } } }
            //};
        }


    }
}

using Libria.Models;

namespace Libria.Repository
{
    public interface IBookRepository
    {
        Task<List<Book>> GetNewBooksAsync(int booksNum);

        Task<Book?> GetBookByIdAsync(int bookId);
    }
}

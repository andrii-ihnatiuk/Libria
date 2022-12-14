using Libria.Repository;
using Microsoft.AspNetCore.Mvc;

namespace Libria.Components
{
    public class NewBooksViewComponent : ViewComponent
    {
        private readonly IBookRepository _bookRepository;

        public NewBooksViewComponent(IBookRepository bookRepository)
        {
            _bookRepository = bookRepository;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var books = await _bookRepository.GetNewBooksAsync(10);
            return View(books);
        }
    }
}

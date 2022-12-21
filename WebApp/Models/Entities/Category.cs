﻿using System.ComponentModel.DataAnnotations;

namespace Libria.Models.Entities
{
    public class Category
    {
        public Category()
        {
            Books = new List<Book>();
        }

        public int CategoryId { get; set; }

        [Required]
        public string Name { get; set; } = null!;

        public string? Description { get; set; }

        /* RELATIONS */

        public ICollection<Book> Books { get; set; }
    }
}

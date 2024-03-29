﻿namespace Libria.Models.Entities
{
	public class Review
	{
		public int Id { get; set; }
		public string Username { get; set; } = null!;
		public string Text { get; set; } = null!;
		public int StarsQuantity { get; set; }
		public DateTime ReviewDate { get; set; }

		public int BookId { get; set; }
		public Book Book { get; set; } = null!;
	}
}

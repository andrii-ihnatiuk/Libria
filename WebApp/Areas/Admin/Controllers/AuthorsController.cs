﻿using Libria.Areas.Admin.Models;
using Libria.Areas.Admin.ViewModels.Authors;
using Libria.Data;
using Libria.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Libria.Areas.Admin.Controllers
{
    [Area("Admin")]
	[Authorize(Roles = "admin")]
	public class AuthorsController : Controller
	{
		private readonly LibriaDbContext _context;

		public AuthorsController(LibriaDbContext context)
		{
			_context = context;
		}

		public async Task<IActionResult> Index(string? q)
		{
			var query = _context.Authors.AsNoTracking();
			if (q != null)
				query = query.Where(a => EF.Functions.ILike(a.Name, $"%{q}%"));

			var authorCards = await query
				.OrderByDescending(a => a.AuthorId)
				.Select(a => new AuthorCard { Author = a, ItemsCount = a.Books.Count })
				.ToListAsync();
			var viewModel = new AllAuthorsViewModel(authorCards)
			{
				CurrentSearchString = q
			};

			return View(viewModel);
		}

		[HttpPost]
		public async Task<IActionResult> Remove(int? authorId = null)
		{
			if (authorId == null)
				return BadRequest();

			var author = await _context.Authors.FirstOrDefaultAsync(a => a.AuthorId == authorId);
			if (author == null)
				return NotFound();

			_context.Authors.Remove(author);
			await _context.SaveChangesAsync();

			return RedirectToAction("Index");
		}

		[HttpGet]
		public async Task<IActionResult> Edit(int? id = null)
		{
			if (id == null)
				return BadRequest();

			var author = await _context.Authors
				.AsNoTracking()
				.FirstOrDefaultAsync(a => a.AuthorId == id);
			if (author == null)
				return NotFound();

			var viewModel = new EditAuthorViewModel
			{
				Id = author.AuthorId,
				Name = author.Name,
				Description = author.Description
			};

			return View(viewModel);
		}

		[HttpPost]
		public async Task<IActionResult> Edit(EditAuthorViewModel model)
		{
			if (ModelState.IsValid)
			{
				if (model.Id == null)
					return BadRequest();
				var author = _context.Authors.FirstOrDefault(a => a.AuthorId == model.Id);
				if (author == null)
					return NotFound();

				author.Name = model.Name.Trim();
				author.Description = model.Description?.Trim();

				await _context.SaveChangesAsync();
				return RedirectToAction("Index");
			}
			return View(model);
		}

		[HttpGet]
		public IActionResult Create()
		{
			return View("Edit", new EditAuthorViewModel { ActionName = "Create", PageTitle = "Новий автор" });
		}

		[HttpPost]
		public async Task<IActionResult> Create(EditAuthorViewModel model)
		{
			if (ModelState.IsValid)
			{
				if (await _context.Authors.AnyAsync(a => EF.Functions.ILike(a.Name, $"{model.Name}")))
				{
					ModelState.AddModelError(nameof(model.Name), "Дубльоване ім'я");
					model.ActionName = nameof(Create);
					model.PageTitle = "Новий автор";
					return View("Edit", model);
				}
				var author = new Author { Name = model.Name.Trim(), Description = model.Description?.Trim() };

				_context.Authors.Add(author);

				await _context.SaveChangesAsync();
				return RedirectToAction("Index");
			}
			return View("Edit", model);
		}
	}
}

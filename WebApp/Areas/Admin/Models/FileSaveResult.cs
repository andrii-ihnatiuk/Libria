using Microsoft.AspNetCore.Mvc;

namespace Libria.Areas.Admin.Models
{
	public class FileSaveResult
	{
		public FileSaveStatus Status { get; set; }

		public string ErrorMessage { get; set; } = string.Empty;

		public string? FileUrl { get; set; }
	}

	public enum FileSaveStatus
	{
		Saved,
		Error,
		LargeFile,
		EmptyFile
	}
}

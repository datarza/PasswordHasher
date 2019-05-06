using Microsoft.AspNetCore.Mvc.RazorPages;

using myClippit.ApplicationCore.Shared.Hashers;

namespace WebApplication.Pages
{
	public class IndexModel : PageModel
	{
		private readonly IPasswordHasher hasher;

		public IndexModel(IPasswordHasher hasher)
		{
			this.hasher = hasher;
		}

		public string Password1 { get; private set; }

		public string Password1Hash { get; private set; }

		public string Password2 { get; private set; }

		public string Password2Hash { get; private set; }

		public string Password3 { get; private set; }

		public string Password3Hash { get; private set; }

		public void OnGet()
		{
			Password1 = "my password";
			Password1Hash = hasher.HashPassword(Password1);

			Password2 = "my password";
			Password2Hash = hasher.HashPassword(Password2);

			Password3 = "my password";
			Password3Hash = hasher.HashPassword(Password3);

		}
	}
}

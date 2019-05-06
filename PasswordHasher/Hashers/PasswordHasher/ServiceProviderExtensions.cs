using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using myClippit.ApplicationCore.Shared.Comparers;

namespace myClippit.ApplicationCore.Shared.Hashers
{
	public static class ServiceProviderExtensions
	{
		public static void AddPasswordHasher(this IServiceCollection services)
		{
			//services.AddScoped<IEqualityComparer<byte[]>, BytesEqualityComparer>();
			services.AddScoped<IPasswordHasher, PasswordHasher>(srv =>
			{
				var options = srv.GetRequiredService<IOptions<PasswordHasherOptions>>();
				return new PasswordHasher(options, new BytesEqualityComparer());
			});
		}
	}
}

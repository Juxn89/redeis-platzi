using StackExchange.Redis;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

internal class Program
{
	private static async Task Main(string[] args)
	{
		Console.WriteLine("Hello, World!");

		var config = new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
				.Build();

		string redisConnectionString = config["CacheConnection"] ?? throw new InvalidOperationException("Connection string 'CacheConnection' not found.");
		using (var redisCache = ConnectionMultiplexer.Connect(redisConnectionString))
		{
			IDatabase db = redisCache.GetDatabase();

			bool setValue = await db.StringSetAsync("test:key", "some value");
			Console.WriteLine($"Set value: {setValue}");

			string? getValue = await db.StringGetAsync("test:key");
			Console.WriteLine($"Get value: {getValue}");
		}
	}
}
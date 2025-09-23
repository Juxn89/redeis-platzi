using System.Text.Json;
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

			#region Lists on Redis
			// 0. Clean the current key
			await db.KeyDeleteAsync("test:list");

			// 1. Create a list
			var listKey = "test:list";
			await db.ListLeftPushAsync(listKey, "Alan Turing");
			Console.WriteLine($"Pushed element to list (left): Alan Turing \n");

			// 2. Get first element of the list
			string? firstElement = await db.ListGetByIndexAsync(listKey, 0);
			Console.WriteLine($"First element: {firstElement}");

			// 3. Add new elements to the list in the first position
			await db.ListLeftPushAsync(listKey, "Linus Torvalds");
			Console.WriteLine($"Pushed element to list (left): Linus Torvalds \n");

			// 4. Get all elements of the list
			var allElements = await db.ListRangeAsync(listKey);
			Console.WriteLine("All elements:");
			foreach (var element in allElements)
			{
				Console.WriteLine($" - {element}");
			}
			Console.WriteLine("\n");

			// 5. Add new elements to the list with range of values
			RedisValue[] listOfNewElements = { "Grace Hopper", "Joan Clarke", "Ada Lovelace", "Nicola Tesla" };
			await db.ListRightPushAsync(listKey, listOfNewElements);
			Console.WriteLine($"Pushed elements to list: {string.Join(", ", listOfNewElements)} \n");

			// 6. Get all elements from the list
			allElements = await db.ListRangeAsync(listKey);
			Console.WriteLine("All elements:");
			foreach (var element in allElements)
			{
				Console.WriteLine($" - {element}");
			}
			Console.WriteLine("\n");

			// 7. Get range of elements from the list
			var rangeElements = await db.ListRangeAsync(listKey, 1, 3);

			Console.WriteLine("Range elements from position 1 to 3:");
			foreach (var element in rangeElements)
			{
				Console.WriteLine($" - {element}");
			}

			// 8. Delete element from the list - POP
			var poppedElement = await db.ListLeftPopAsync(listKey);
			Console.WriteLine($"Popped element from list (left): {poppedElement} \n");

			// 9. Add element to the list in JSON format
			var jsonElement = JsonSerializer.Serialize(new { Name = "Newman", Age = 30, Occupation = "Developer" });
			await db.ListRightPushAsync(listKey, jsonElement);
			#endregion

			#region Hash
			Console.WriteLine("\n\t***Hash operations ***\n");

			// Add elemento to hash
			await db.HashSetAsync("student:48", new HashEntry[] {
				new HashEntry("name", "Newman"),
				new HashEntry("age", 30),
				new HashEntry("occupation", "Developer")
			});

			// Retrieve name from hash
			var getNameStudentHash = await db.HashGetAsync("student:48", "name");
			Console.WriteLine($"Student #48, name: {getNameStudentHash}");

			// Retrieve age from hash
			var getAgeStudentHash = await db.HashGetAsync("student:48", "age");
			Console.WriteLine($"Student #48, age: {getAgeStudentHash}");

			// Retrieve occupation from hash
			var getOccupationStudentHash = await db.HashGetAsync("student:48", "occupation");
			Console.WriteLine($"Student #48, occupation: {getOccupationStudentHash}");

			// Retrieve all fields from hash
			var getStudentHash = await db.HashGetAllAsync("student:48");
			foreach (var entry in getStudentHash)
			{
				Console.WriteLine($"Student #48, {entry.Name}: {entry.Value}");
			}

			// Add a new element to hash
			await db.HashSetAsync("student:2048", new HashEntry[] {
				new HashEntry("name", "Ada Lovelace"),
				new HashEntry("age", 36),
				new HashEntry("occupation", "Mathematician"),
				new HashEntry("nationality", "British")
			});

			// Retrieve all fields from hash
			var keysInStudent2048 = await db.HashKeysAsync("student:2048");
			foreach (var key in keysInStudent2048)
			{
				var value = await db.HashGetAsync("student:2048", key);
				Console.WriteLine($"Student #2048, {key}: {value}");
			}
			#endregion
		}
	}
}
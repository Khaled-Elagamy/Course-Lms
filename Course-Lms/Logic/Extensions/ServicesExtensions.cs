using System.Reflection;

namespace Course_Lms.Logic.Extensions
{
	public static class ServicesExtensions
	{
		public static IServiceCollection RegisterServiceReflection(this IServiceCollection serviceCollection, Type serviceType)
		{
			try
			{
				var assembly = Assembly.GetAssembly(serviceType);

				var types = assembly.GetTypes()
					.Where(t => !t.IsInterface && !t.IsAbstract)
					.Where(t => t.Name.ToLower().EndsWith("service"))
					.ToList();

				foreach (var type in types)
				{
					var interfaceType = type.GetInterfaces().FirstOrDefault(i => i.Name == $"I{type.Name}");
					if (interfaceType != null)
					{
						serviceCollection.AddScoped(interfaceType, type);
						Console.WriteLine($"{interfaceType} service is registered.");
					}
					else
					{
						Console.WriteLine($"No corresponding interface found for {type.Name}.");
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error during service registration: {ex.Message}");
			}

			return serviceCollection;
		}
	}

}

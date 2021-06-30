using Microsoft.Extensions.DependencyInjection;

namespace Recurop
{
    public static class Initializers
    {
        /// <summary>
        /// Registers the RecurringOperationsManager as a singleton instance in the Dependency Injection Container's
        /// IServiceCollection.
        /// </summary>
        /// <param name="services"></param>
        public static void AddRecurop(this IServiceCollection services)
        {
            services.AddSingleton(typeof(RecurringOperationsManager), RecurringOperationsManager.Instance);
        }
    }
}

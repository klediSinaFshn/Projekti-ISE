using AirLineSystem.DependencyContainer;
namespace AirLineSystem.Api.Configurtations
{
    public static class DependencyInjectionConfig
    {
        public static void AddDependencyInjectionConfiguration(this IServiceCollection services) { 
            if(services == null) throw new ArgumentNullException(nameof(services));
                
            DependencyContainer.DependencyContainer.RegisterServices(services);
;
        }    
    }
}

using MeetingIntelli.Configurations;
using MeetingIntelli.Data;
using MeetingIntelli.Services.Implementations;
using MeetingIntelli.Services.Interface;



namespace MeetingIntelli.Extension;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAppServices(this IServiceCollection services, IConfiguration configurition)
    {
        services.AddDbContext<AppDbContext>(options =>
           options.UseSqlServer(
               configurition.GetConnectionString("DefaultConnection"),
               npgsqlOptions => npgsqlOptions.EnableRetryOnFailure(
                   maxRetryCount: 3,
                   maxRetryDelay: TimeSpan.FromSeconds(5),
                   errorNumbersToAdd:null
                
               )
           ));

       
        services.AddHttpClient<IClaudeService, ClaudeService>();

      
        services.AddScoped<IMeetingAnalysisService, MeetingAnalysisService>();
        services.AddScoped<IPdfService, PdfService>();

        services.Configure<AnthropicSettings>(configurition.GetSection(AnthropicSettings.SectionName)); ;
        services.Configure<FrontEndSettings>(configurition.GetSection(FrontEndSettings.SectionName));

        return services;

    }

    public static IServiceCollection AddCorsPolicy(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend", policy =>
            {
                policy.WithOrigins("http://localhost:3000")
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials();
            });
        });

        return services;
    }
}

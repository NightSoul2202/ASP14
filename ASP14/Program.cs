using OpenTelemetry.Metrics;
using OpenTelemetry;
using OpenTelemetry.Trace;
using OpenTelemetry.Resources;
using System.Diagnostics;

namespace ASP14
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            builder.Services.AddOpenTelemetry()
                .WithTracing(tracingBuilder =>
                {
                    tracingBuilder
                    .SetSampler(new AlwaysOnSampler())
                    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("YourAppName"))
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri("http://localhost:4317"); // ¬казуЇмо адресу Collector
                    })
                    .AddProcessor(new SimpleActivityExportProcessor(new CustomExporter()));

                });

            var app = builder.Build();

            app.Use(async (context, next) =>
            {
                // Get the current Activity (trace) if one exists
                var activity = Activity.Current;

                if (activity != null)
                {
                    // Adding custom context to the trace
                    activity.SetTag("custom.tag", "value");
                    activity.SetTag("user.id", "12345");

                    // ¬иводимо ц≥ теги в консоль
                    Console.WriteLine($"Trace Activity: {activity.DisplayName}");
                    Console.WriteLine($"Tag custom.tag: {activity.GetTagItem("custom.tag")}");
                    Console.WriteLine($"Tag user.id: {activity.GetTagItem("user.id")}");
                }

                await next.Invoke();
            });

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}

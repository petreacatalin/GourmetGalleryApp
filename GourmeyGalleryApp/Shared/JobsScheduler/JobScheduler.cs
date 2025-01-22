using GourmeyGalleryApp.Services.NewsletterService;
using Hangfire;
using Microsoft.Extensions.DependencyInjection;

public class JobScheduler
{
    private readonly IRecurringJobManager _recurringJobManager;
    private readonly IServiceProvider _serviceProvider;

    public JobScheduler(IRecurringJobManager recurringJobManager, IServiceProvider serviceProvider)
    {
        _recurringJobManager = recurringJobManager;
        _serviceProvider = serviceProvider;
    }

    public void ConfigureRecurringJobs()
    {
        //_recurringJobManager.AddOrUpdate(
        //    "SendWeeklyNewsletter",
        //    () => SendNewsletterAsync(),
        //    "*/2 * * * *", // Cron expression
        //    TimeZoneInfo.Local);

        var timeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Bucharest");

        // Define the cron schedule
        string cronExpression = "0 12,20 * * *"; // 12:00 PM and 8:00 PM

        // Add or update the job
        _recurringJobManager.AddOrUpdate(
            "SendWeeklyNewsletter", // Unique job identifier
            () => SendNewsletterAsync(), // The method to execute
            cronExpression,
            timeZone);
    }

    // Use a scoped instance of NewsletterService for the job
    public async Task SendNewsletterAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var newsletterService = scope.ServiceProvider.GetRequiredService<NewsletterService>();
        await newsletterService.SendNewsletterAsync();
    }
}

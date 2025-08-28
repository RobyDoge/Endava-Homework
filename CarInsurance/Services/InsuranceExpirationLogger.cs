using Microsoft.Extensions.Hosting;

namespace CarInsurance.Api.Services;
public class InsuranceExpirationLogger(IServiceProvider serviceProvider) : BackgroundService
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var expiredPoliciesId = new HashSet<long>();
        const string path = "expiredPolicies.log";
        expiredPoliciesId.UnionWith(await ReadLoggedPolicies(path));

        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var policyService = scope.ServiceProvider.GetRequiredService<PolicyService>();

                var expiredPolicies = await policyService.GetExpiredPolicies();
                foreach (var policy in expiredPolicies)
                {
                    expiredPoliciesId.Add(policy.Id);
                }
                await File.WriteAllLinesAsync(path, expiredPoliciesId.Select(id => id.ToString()), stoppingToken);
            }

            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
        
    }

    private static async Task<List<long>> ReadLoggedPolicies(string path)
    {
        if (!File.Exists(path))
            return new List<long>();
        var lines = await File.ReadAllLinesAsync(path);
        var numbers = new List<long>();
        foreach (var line in lines)
        {
            if (long.TryParse(line, out var number))
            {
                numbers.Add(number);
            }
        }
        return numbers;
    }

}
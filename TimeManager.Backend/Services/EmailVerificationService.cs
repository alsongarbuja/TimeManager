using DnsClient;
using System.Text.RegularExpressions;

namespace TimeManager.Backend.Services
{
    public interface IEmailVerificationService
    {
        Task<EmailVerificationResult> VerifyAsync(string email);
    }

    public class EmailVerificationService(
        ILookupClient lookupClient,
        ILogger<EmailVerificationService> logger,
        IWebHostEnvironment env) : IEmailVerificationService
    {
        private static readonly Regex EmailRegex = new(
            @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private readonly ILookupClient _lookupClient = lookupClient;
        private readonly ILogger<EmailVerificationService> _logger = logger;
        private readonly Lazy<Task<HashSet<string>>> _disposableDomains = new Lazy<Task<HashSet<string>>>(() => LoadDisposableDomainsAsync(env));

        public async Task<EmailVerificationResult> VerifyAsync(string email)
        {
            var result = new EmailVerificationResult();

            if (string.IsNullOrWhiteSpace(email) || !EmailRegex.IsMatch(email))
            {
                result.IsValidFormat = false;
                result.FailureReason = "Invalid email format";
                return result;
            }

            result.IsValidFormat = true;

            var domain = email.Split('@')[1];

            var disposableDomains = await _disposableDomains.Value;
            if (disposableDomains.Contains(domain.ToLowerInvariant()))
            {
                result.IsDisposableDomain = true;
                result.FailureReason = "Cannot use a disposable email domain";
                return result;
            }

            try
            {
                var dnsResult = await _lookupClient.QueryAsync(domain, QueryType.MX);
                result.HasMxRecord = dnsResult.Answers.MxRecords().Any();

                if (!result.HasMxRecord)
                {
                    result.FailureReason = "Domain has no mail server";
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "MX lookup failed for domain {Domain}", domain);
                result.HasMxRecord = false;
                result.FailureReason = "Could not verify domain mail server";
            }

            return result;
        }

        private static async Task<HashSet<string>> LoadDisposableDomainsAsync(IWebHostEnvironment env)
        {
            var path = Path.Combine(env.ContentRootPath, "Data", "disposable-domains.txt");

            if (!File.Exists(path))
            {
                return new HashSet<string>();
            }

            var lines = await File.ReadAllLinesAsync(path);
            return lines
                .Where(l => !string.IsNullOrWhiteSpace(l))
                .Select(l => l.Trim().ToLowerInvariant())
                .ToHashSet();
        }
    }

    public class EmailVerificationResult
    {
        public bool IsValidFormat { get; set; }
        public bool HasMxRecord { get; set; }
        public bool IsDisposableDomain { get; set; }
        public string? FailureReason { get; set; }

        public bool IsValid => IsValidFormat && HasMxRecord && !IsDisposableDomain;
    }
}

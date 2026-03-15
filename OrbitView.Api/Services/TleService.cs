using OrbitView.Api.DTOs;
using OrbitView.Api.Models;
using OrbitView.Api.Repositories;

namespace OrbitView.Api.Services;

public class TleService : ITleService
{
    private readonly ITleRepository _repo;
    private readonly HttpClient _httpClient;
    private readonly ILogger<TleService> _logger;

    private const string CelesTrakUrl =
        "https://celestrak.org/SOCRATES/query.php?CODE=all&FORMAT=TLE";

    private const string CelesTrakStationsUrl =
        "https://celestrak.org/pub/TLE/stations.txt";

    public TleService(ITleRepository repo, IHttpClientFactory httpClientFactory,
        ILogger<TleService> logger)
    {
        _repo = repo;
        _httpClient = httpClientFactory.CreateClient();
        _logger = logger;
    }

    public async Task<TleFetchResultDto> FetchAndStoreAsync(int? triggeredByUserId = null)
    {
        var fetchedAt = DateTime.UtcNow;
        int satellitesUpdated = 0;

        try
        {
            _logger.LogInformation("Starting TLE fetch from CelesTrak...");

            // Fetch TLE data from CelesTrak stations feed
            var response = await _httpClient.GetStringAsync(CelesTrakStationsUrl);
            var tleDict = ParseTleResponse(response);

            _logger.LogInformation("Parsed {Count} TLE records from CelesTrak", tleDict.Count);

            // Get all our active satellites from DB
            var satellites = await _repo.GetAllActiveSatellitesAsync();

            foreach (var satellite in satellites)
            {
                // Try to find matching TLE by NORAD ID
                if (!tleDict.TryGetValue(satellite.NoradId, out var tlePair))
                {
                    _logger.LogWarning("No TLE found for {Name} (NORAD {Id})",
                        satellite.Name, satellite.NoradId);
                    continue;
                }

                var (line1, line2) = tlePair;

                // Parse fields from TLE lines
                var parsed = ParseTleFields(line1, line2);
                if (parsed == null) continue;

                // Mark old records as not current
                await _repo.SetAllNotCurrentAsync(satellite.Id);

                // Insert new current record
                var record = new TleRecord
                {
                    SatelliteId = satellite.Id,
                    Line1 = line1,
                    Line2 = line2,
                    Epoch = parsed.Epoch,
                    Inclination = parsed.Inclination,
                    Eccentricity = parsed.Eccentricity,
                    MeanMotion = parsed.MeanMotion,
                    IsCurrent = true,
                    FetchedAt = fetchedAt
                };

                await _repo.AddTleRecordAsync(record);
                satellitesUpdated++;
            }

            await _repo.SaveChangesAsync();

            // Log the successful fetch
            await _repo.AddFetchLogAsync(new TleFetchLog
            {
                FetchedAt = fetchedAt,
                Source = triggeredByUserId.HasValue ? "manual" : "scheduled",
                SatellitesUpdated = satellitesUpdated,
                Success = true,
                TriggeredByUserId = triggeredByUserId
            });

            await _repo.SaveChangesAsync();

            _logger.LogInformation("TLE fetch complete. {Count} satellites updated.", satellitesUpdated);

            return new TleFetchResultDto
            {
                Success = true,
                SatellitesUpdated = satellitesUpdated,
                FetchedAt = fetchedAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "TLE fetch failed");

            await _repo.AddFetchLogAsync(new TleFetchLog
            {
                FetchedAt = fetchedAt,
                Source = triggeredByUserId.HasValue ? "manual" : "scheduled",
                SatellitesUpdated = 0,
                Success = false,
                ErrorMessage = ex.Message,
                TriggeredByUserId = triggeredByUserId
            });

            try { await _repo.SaveChangesAsync(); } catch { }

            return new TleFetchResultDto
            {
                Success = false,
                SatellitesUpdated = 0,
                FetchedAt = fetchedAt,
                ErrorMessage = ex.Message
            };
        }
    }

    private Dictionary<int, (string line1, string line2)> ParseTleResponse(string raw)
    {
        var result = new Dictionary<int, (string, string)>();
        var lines = raw.Split('\n', StringSplitOptions.RemoveEmptyEntries)
                       .Select(l => l.Trim())
                       .ToArray();

        // TLE format: name line, line1 (starts with "1 "), line2 (starts with "2 ")
        for (int i = 0; i < lines.Length - 2; i++)
        {
            var line1 = lines[i + 1];
            var line2 = lines[i + 2];

            if (!line1.StartsWith("1 ") || !line2.StartsWith("2 "))
                continue;

            // NORAD ID is in characters 2-7 of line 1
            if (int.TryParse(line1.Substring(2, 5).Trim(), out int noradId))
            {
                result[noradId] = (line1, line2);
                i += 2; // skip the two TLE lines
            }
        }

        return result;
    }

    private TleParsed? ParseTleFields(string line1, string line2)
    {
        try
        {
            // Epoch from line 1 characters 18-32
            var epochStr = line1.Substring(18, 14).Trim();
            var year = int.Parse(epochStr.Substring(0, 2));
            var fullYear = year < 57 ? 2000 + year : 1900 + year;
            var dayOfYear = double.Parse(epochStr.Substring(2));
            var epoch = new DateTime(fullYear, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                .AddDays(dayOfYear - 1);

            // Inclination from line 2 characters 8-16
            var inclination = decimal.Parse(line2.Substring(8, 8).Trim(),
                System.Globalization.CultureInfo.InvariantCulture);

            // Eccentricity from line 2 characters 26-33 (implied decimal point)
            var eccStr = "0." + line2.Substring(26, 7).Trim();
            var eccentricity = decimal.Parse(eccStr,
                System.Globalization.CultureInfo.InvariantCulture);

            // Mean motion from line 2 characters 52-63
            var meanMotion = decimal.Parse(line2.Substring(52, 11).Trim(),
                System.Globalization.CultureInfo.InvariantCulture);

            return new TleParsed
            {
                Epoch = epoch,
                Inclination = inclination,
                Eccentricity = eccentricity,
                MeanMotion = meanMotion
            };
        }
        catch (Exception)
        {
            return null;
        }
    }

    private class TleParsed
    {
        public DateTime Epoch { get; set; }
        public decimal Inclination { get; set; }
        public decimal Eccentricity { get; set; }
        public decimal MeanMotion { get; set; }
    }
}
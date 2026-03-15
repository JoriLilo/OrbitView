using OrbitView.Api.DTOs;
using OrbitView.Api.Models;
using OrbitView.Api.Repositories;
using System.Text.Json;

namespace OrbitView.Api.Services;

public class TleService : ITleService
{
    private readonly ITleRepository _repo;
    private readonly HttpClient _httpClient;
    private readonly ILogger<TleService> _logger;

    public TleService(ITleRepository repo, IHttpClientFactory httpClientFactory,
        ILogger<TleService> logger)
    {
        _repo = repo;
        _httpClient = httpClientFactory.CreateClient();
        _httpClient.DefaultRequestHeaders.Add("User-Agent",
            "OrbitView/1.0 (educational project)");
        _httpClient.Timeout = TimeSpan.FromSeconds(30);
        _logger = logger;
    }

    private string BuildUrl(int noradId) =>
        $"https://celestrak.org/NORAD/elements/gp.php?CATNR={noradId}&FORMAT=JSON";

    public async Task<TleFetchResultDto> FetchAndStoreAsync(int? triggeredByUserId = null)
    {
        var fetchedAt = DateTime.UtcNow;
        int satellitesUpdated = 0;

        try
        {
            _logger.LogInformation("Starting TLE fetch from CelesTrak...");

            var satellites = await _repo.GetAllActiveSatellitesAsync();
            _logger.LogInformation("Fetching TLE for {Count} satellites...", satellites.Count);

            foreach (var satellite in satellites)
            {
                try
                {
                    var url = BuildUrl(satellite.NoradId);
                    var json = await _httpClient.GetStringAsync(url);
                    var entry = ParseGpJson(json);

                    if (entry == null)
                    {
                        _logger.LogWarning("No data returned for {Name} (NORAD {Id})",
                            satellite.Name, satellite.NoradId);
                        continue;
                    }

                    await _repo.SetAllNotCurrentAsync(satellite.Id);

                    await _repo.AddTleRecordAsync(new TleRecord
                    {
                        SatelliteId = satellite.Id,
                        Line1 = entry.Line1,
                        Line2 = entry.Line2,
                        Epoch = entry.Epoch,
                        Inclination = entry.Inclination,
                        Eccentricity = entry.Eccentricity,
                        MeanMotion = entry.MeanMotion,
                        IsCurrent = true,
                        FetchedAt = fetchedAt
                    });

                    satellitesUpdated++;
                    _logger.LogInformation("Updated TLE for {Name}", satellite.Name);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("Failed TLE fetch for {Name}: {Error}",
                        satellite.Name, ex.Message);
                }
            }

            await _repo.SaveChangesAsync();

            await _repo.AddFetchLogAsync(new TleFetchLog
            {
                FetchedAt = fetchedAt,
                Source = triggeredByUserId.HasValue ? "manual" : "scheduled",
                SatellitesUpdated = satellitesUpdated,
                Success = true,
                TriggeredByUserId = triggeredByUserId
            });

            await _repo.SaveChangesAsync();

            _logger.LogInformation("TLE fetch complete. {Count} satellites updated.",
                satellitesUpdated);

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

    private GpEntry? ParseGpJson(string json)
    {
        try
        {
            var docs = JsonSerializer.Deserialize<List<JsonElement>>(json);
            if (docs == null || docs.Count == 0) return null;

            var doc = docs[0];

            // Parse epoch
            var epochStr = doc.GetProperty("EPOCH").GetString() ?? string.Empty;
            var epoch = DateTime.Parse(epochStr, null,
                System.Globalization.DateTimeStyles.RoundtripKind);

            // Read orbital elements directly from JSON
            var inclination = (decimal)doc.GetProperty("INCLINATION").GetDouble();
            var eccentricity = (decimal)doc.GetProperty("ECCENTRICITY").GetDouble();
            var meanMotion = (decimal)doc.GetProperty("MEAN_MOTION").GetDouble();
            var noradId = doc.GetProperty("NORAD_CAT_ID").GetInt32();
            var objectName = doc.GetProperty("OBJECT_NAME").GetString() ?? string.Empty;

            // Build TLE lines from GP data so we still have them for satellite.js
            var line1 = BuildTleLine1(doc);
            var line2 = BuildTleLine2(doc);

            return new GpEntry
            {
                NoradId = noradId,
                Epoch = epoch,
                Inclination = inclination,
                Eccentricity = eccentricity,
                MeanMotion = meanMotion,
                Line1 = line1,
                Line2 = line2
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning("GP JSON parse error: {Error}", ex.Message);
            return null;
        }
    }

    private string BuildTleLine1(JsonElement doc)
    {
        // Reconstruct TLE Line 1 from GP fields
        // Format: 1 NNNNNC NNNNNAAA NNNNN.NNNNNNNN +.NNNNNNNN +NNNNN-N +NNNNN-N N NNNNN
        try
        {
            var noradId = doc.GetProperty("NORAD_CAT_ID").GetInt32();
            var classification = doc.GetProperty("CLASSIFICATION_TYPE").GetString() ?? "U";
            var intlDesig = doc.GetProperty("OBJECT_ID").GetString() ?? "00000A";
            var epochStr = doc.GetProperty("EPOCH").GetString() ?? string.Empty;
            var epoch = DateTime.Parse(epochStr);
            var bstar = doc.GetProperty("BSTAR").GetDouble();
            var elementSetNo = doc.GetProperty("ELEMENT_SET_NO").GetInt32();
            var mmDot = doc.GetProperty("MEAN_MOTION_DOT").GetDouble();

            // Epoch in TLE format: YYDDD.DDDDDDDD
            var yy = epoch.Year % 100;
            var dayOfYear = epoch.DayOfYear +
                (epoch.Hour * 3600 + epoch.Minute * 60 + epoch.Second) / 86400.0;

            var intlDesigFormatted = intlDesig.Replace("-", "").PadRight(8).Substring(0, 8);
            var bstarStr = FormatScientific(bstar);
            var mmDotStr = $"{mmDot:+.00000000;-.00000000}".Replace(".", "");
            if (mmDot >= 0) mmDotStr = $" {Math.Abs(mmDot):00000000}";
            else mmDotStr = $"-{Math.Abs(mmDot):00000000}";

            return $"1 {noradId:D5}{classification} {intlDesigFormatted} " +
                   $"{yy:D2}{dayOfYear:000.00000000} {mmDot:+.00000000} " +
                   $" 00000-0 {bstarStr} 0 {elementSetNo:D4}0";
        }
        catch
        {
            return string.Empty;
        }
    }

    private string BuildTleLine2(JsonElement doc)
    {
        try
        {
            var noradId = doc.GetProperty("NORAD_CAT_ID").GetInt32();
            var inclination = doc.GetProperty("INCLINATION").GetDouble();
            var raan = doc.GetProperty("RA_OF_ASC_NODE").GetDouble();
            var ecc = doc.GetProperty("ECCENTRICITY").GetDouble();
            var argPerigee = doc.GetProperty("ARG_OF_PERICENTER").GetDouble();
            var meanAnomaly = doc.GetProperty("MEAN_ANOMALY").GetDouble();
            var meanMotion = doc.GetProperty("MEAN_MOTION").GetDouble();
            var revAtEpoch = doc.GetProperty("REV_AT_EPOCH").GetInt32();

            var eccStr = ecc.ToString("0.0000000").Replace("0.", "");

            return $"2 {noradId:D5} {inclination:000.0000} {raan:000.0000} " +
                   $"{eccStr} {argPerigee:000.0000} {meanAnomaly:000.0000} " +
                   $"{meanMotion:00.00000000}{revAtEpoch:D5}0";
        }
        catch
        {
            return string.Empty;
        }
    }

    private string FormatScientific(double value)
    {
        if (value == 0) return " 00000-0";
        var exp = (int)Math.Floor(Math.Log10(Math.Abs(value)));
        var mantissa = value / Math.Pow(10, exp);
        var sign = value >= 0 ? " " : "-";
        return $"{sign}{Math.Abs(mantissa * 100000):00000}{(exp >= 0 ? "+" : "-")}{Math.Abs(exp)}";
    }

    private class GpEntry
    {
        public int NoradId { get; set; }
        public DateTime Epoch { get; set; }
        public decimal Inclination { get; set; }
        public decimal Eccentricity { get; set; }
        public decimal MeanMotion { get; set; }
        public string Line1 { get; set; } = string.Empty;
        public string Line2 { get; set; } = string.Empty;
    }
}
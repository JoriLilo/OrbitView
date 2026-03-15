using OrbitView.Api.DTOs;
using OrbitView.Api.Repositories;

namespace OrbitView.Api.Services;

public class SatelliteService : ISatelliteService
{
    private readonly ISatelliteRepository _repo;

    public SatelliteService(ISatelliteRepository repo)
    {
        _repo = repo;
    }

    public async Task<SatelliteListDto> GetAllAsync(
        string? category, string? search, bool? isActive, int page, int pageSize)
    {
        var (satellites, total) = await _repo.GetAllAsync(
            category, search, isActive, page, pageSize);

        return new SatelliteListDto
        {
            Total = total,
            Page = page,
            PageSize = pageSize,
            Data = satellites.Select(s => new SatelliteDto
            {
                Id = s.Id,
                NoradId = s.NoradId,
                Name = s.Name,
                IsActive = s.IsActive,
                Category = s.Category == null ? null : new SatelliteCategoryDto
                {
                    Slug = s.Category.Slug,
                    DisplayName = s.Category.DisplayName,
                    ColourHex = s.Category.ColourHex
                },
                CurrentTle = s.TleRecords.FirstOrDefault() == null ? null : new TleRecordDto
                {
                    Line1 = s.TleRecords.First().Line1,
                    Line2 = s.TleRecords.First().Line2,
                    Epoch = s.TleRecords.First().Epoch,
                    Inclination = s.TleRecords.First().Inclination,
                    FetchedAt = s.TleRecords.First().FetchedAt
                }
            }).ToList()
        };
    }

    public async Task<SatelliteDetailDto?> GetByIdAsync(int id)
    {
        var s = await _repo.GetByIdAsync(id);
        if (s == null) return null;

        return new SatelliteDetailDto
        {
            Id = s.Id,
            NoradId = s.NoradId,
            Name = s.Name,
            IsActive = s.IsActive,
            CountryOfOrigin = s.CountryOfOrigin,
            LaunchDate = s.LaunchDate,
            MissionDescription = s.MissionDescription,
            Category = s.Category == null ? null : new SatelliteCategoryDto
            {
                Slug = s.Category.Slug,
                DisplayName = s.Category.DisplayName,
                ColourHex = s.Category.ColourHex
            },
            CurrentTle = s.TleRecords.FirstOrDefault() == null ? null : new TleRecordDto
            {
                Line1 = s.TleRecords.First().Line1,
                Line2 = s.TleRecords.First().Line2,
                Epoch = s.TleRecords.First().Epoch,
                Inclination = s.TleRecords.First().Inclination,
                FetchedAt = s.TleRecords.First().FetchedAt
            }
        };
    }
}
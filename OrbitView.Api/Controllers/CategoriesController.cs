using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrbitView.Api.Data;
using OrbitView.Api.DTOs;

namespace OrbitView.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly AppDbContext _context;

    public CategoriesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var categories = await _context.SatelliteCategories
            .Select(c => new SatelliteCategoryDto
            {
                Slug = c.Slug,
                DisplayName = c.DisplayName,
                ColourHex = c.ColourHex
            })
            .ToListAsync();

        return Ok(categories);
    }
}
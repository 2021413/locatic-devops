using System.Diagnostics;
using Locatic.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Locatic.Web.Models;

namespace Locatic.Web.Controllers;

public class HomeController : Controller
{
    private readonly ITableauDeBordService _tableauDeBord;

    public HomeController(ITableauDeBordService tableauDeBord)
    {
        _tableauDeBord = tableauDeBord;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var aujourdhui = DateOnly.FromDateTime(DateTime.Today);
        var stats = await _tableauDeBord.ObtenirStatsAsync(aujourdhui, cancellationToken);
        return View(stats);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

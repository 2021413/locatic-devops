using Locatic.Application.Services;
using Locatic.Domain.Entities;
using Locatic.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Locatic.Web.Controllers;

public class ModelesController : Controller
{
    private readonly IModeleService _modeles;
    private readonly IMarqueService _marques;

    public ModelesController(IModeleService modeles, IMarqueService marques)
    {
        _modeles = modeles;
        _marques = marques;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var modeles = await _modeles.ListerAsync(cancellationToken);
        return View(modeles);
    }

    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        var form = new ModeleFormViewModel
        {
            MarquesDisponibles = await ChargerMarquesAsync(cancellationToken)
        };
        return View(form);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ModeleFormViewModel form, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            form.MarquesDisponibles = await ChargerMarquesAsync(cancellationToken);
            return View(form);
        }

        var modele = new Modele { Nom = form.Nom, MarqueId = form.MarqueId };
        var resultat = await _modeles.CreerAsync(modele, cancellationToken);
        if (!resultat.Succes)
        {
            ModelState.AddModelError(string.Empty, resultat.Erreur!);
            form.MarquesDisponibles = await ChargerMarquesAsync(cancellationToken);
            return View(form);
        }

        TempData["Succes"] = $"Le modèle « {modele.Nom} » a été ajouté.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<IEnumerable<SelectListItem>> ChargerMarquesAsync(CancellationToken cancellationToken)
    {
        var marques = await _marques.ListerAsync(cancellationToken);
        return marques
            .OrderBy(m => m.Nom)
            .Select(m => new SelectListItem(m.Nom, m.Id.ToString()));
    }
}

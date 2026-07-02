using Locatic.Application.Services;
using Locatic.Domain.Entities;
using Locatic.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Locatic.Web.Controllers;

public class MarquesController : Controller
{
    private readonly IMarqueService _marques;

    public MarquesController(IMarqueService marques)
    {
        _marques = marques;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var marques = await _marques.ListerAvecModelesAsync(cancellationToken);
        return View(marques);
    }

    [HttpGet]
    public IActionResult Create() => View(new MarqueFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MarqueFormViewModel form, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(form);

        var marque = new Marque { Nom = form.Nom, PaysOrigine = form.PaysOrigine };
        var resultat = await _marques.CreerAsync(marque, cancellationToken);
        if (!resultat.Succes)
        {
            ModelState.AddModelError(string.Empty, resultat.Erreur!);
            return View(form);
        }

        TempData["Succes"] = $"La marque « {marque.Nom} » a été ajoutée.";
        return RedirectToAction(nameof(Index));
    }
}

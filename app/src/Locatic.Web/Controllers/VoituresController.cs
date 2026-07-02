using Locatic.Application.Services;
using Locatic.Domain.Entities;
using Locatic.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Locatic.Web.Controllers;

public class VoituresController : Controller
{
    private readonly IVoitureService _voitures;
    private readonly IModeleService _modeles;

    public VoituresController(IVoitureService voitures, IModeleService modeles)
    {
        _voitures = voitures;
        _modeles = modeles;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var voitures = await _voitures.ListerAsync(cancellationToken);
        return View(voitures);
    }

    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var voiture = await _voitures.ObtenirDetailAsync(id, cancellationToken);
        if (voiture is null)
            return NotFound();

        return View(voiture);
    }

    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        var form = new VoitureFormViewModel
        {
            ModelesDisponibles = await ChargerModelesAsync(cancellationToken)
        };
        return View(form);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(VoitureFormViewModel form, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            form.ModelesDisponibles = await ChargerModelesAsync(cancellationToken);
            return View(form);
        }

        var resultat = await _voitures.CreerAsync(VersEntite(form), cancellationToken);
        if (!resultat.Succes)
        {
            ModelState.AddModelError(string.Empty, resultat.Erreur!);
            form.ModelesDisponibles = await ChargerModelesAsync(cancellationToken);
            return View(form);
        }

        TempData["Succes"] = "La voiture a été ajoutée.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var voiture = await _voitures.ObtenirAsync(id, cancellationToken);
        if (voiture is null)
            return NotFound();

        var form = VersFormulaire(voiture);
        form.ModelesDisponibles = await ChargerModelesAsync(cancellationToken);
        return View(form);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, VoitureFormViewModel form, CancellationToken cancellationToken)
    {
        if (id != form.Id)
            return BadRequest();

        if (!ModelState.IsValid)
        {
            form.ModelesDisponibles = await ChargerModelesAsync(cancellationToken);
            return View(form);
        }

        var resultat = await _voitures.ModifierAsync(VersEntite(form), cancellationToken);
        if (!resultat.Succes)
        {
            ModelState.AddModelError(string.Empty, resultat.Erreur!);
            form.ModelesDisponibles = await ChargerModelesAsync(cancellationToken);
            return View(form);
        }

        TempData["Succes"] = "La voiture a été modifiée.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var voiture = await _voitures.ObtenirDetailAsync(id, cancellationToken);
        if (voiture is null)
            return NotFound();

        return View(voiture);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken cancellationToken)
    {
        var resultat = await _voitures.SupprimerAsync(id, cancellationToken);
        if (!resultat.Succes)
        {
            TempData["Erreur"] = resultat.Erreur;
            return RedirectToAction(nameof(Delete), new { id });
        }

        TempData["Succes"] = "La voiture a été supprimée.";
        return RedirectToAction(nameof(Index));
    }

    private static Voiture VersEntite(VoitureFormViewModel form) => new()
    {
        Id = form.Id,
        Immatriculation = form.Immatriculation,
        Annee = form.Annee,
        TarifJournalier = form.TarifJournalier,
        NombrePlaces = form.NombrePlaces,
        Carburant = form.Carburant,
        ModeleId = form.ModeleId
    };

    private static VoitureFormViewModel VersFormulaire(Voiture voiture) => new()
    {
        Id = voiture.Id,
        Immatriculation = voiture.Immatriculation,
        Annee = voiture.Annee,
        TarifJournalier = voiture.TarifJournalier,
        NombrePlaces = voiture.NombrePlaces,
        Carburant = voiture.Carburant,
        ModeleId = voiture.ModeleId
    };

    private async Task<IEnumerable<SelectListItem>> ChargerModelesAsync(CancellationToken cancellationToken)
    {
        var modeles = await _modeles.ListerAsync(cancellationToken);
        return modeles.Select(m => new SelectListItem(
            $"{m.Marque?.Nom} — {m.Nom}",
            m.Id.ToString()));
    }
}

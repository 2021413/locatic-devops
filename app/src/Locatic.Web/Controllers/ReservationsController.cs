using Locatic.Application.Services;
using Locatic.Domain.Entities;
using Locatic.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Locatic.Web.Controllers;

public class ReservationsController : Controller
{
    private readonly IReservationService _reservations;
    private readonly IClientService _clients;
    private readonly IVoitureService _voitures;

    public ReservationsController(
        IReservationService reservations,
        IClientService clients,
        IVoitureService voitures)
    {
        _reservations = reservations;
        _clients = clients;
        _voitures = voitures;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var reservations = await _reservations.ListerAsync(cancellationToken);
        return View(reservations);
    }

    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        var aujourdhui = DateOnly.FromDateTime(DateTime.Today);
        var form = new ReservationFormViewModel
        {
            DateDebut = aujourdhui,
            DateFin = aujourdhui.AddDays(1)
        };
        await ChargerListesAsync(form, cancellationToken);
        return View(form);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ReservationFormViewModel form, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await ChargerListesAsync(form, cancellationToken);
            return View(form);
        }

        var reservation = new Reservation
        {
            ClientId = form.ClientId,
            VoitureId = form.VoitureId,
            DateDebut = form.DateDebut,
            DateFin = form.DateFin
        };

        var resultat = await _reservations.CreerAsync(reservation, cancellationToken);
        if (!resultat.Succes)
        {
            ModelState.AddModelError(string.Empty, resultat.Erreur!);
            await ChargerListesAsync(form, cancellationToken);
            return View(form);
        }

        TempData["Succes"] = $"Réservation enregistrée — montant : {resultat.Valeur!.Montant:0.00} €.";
        return RedirectToAction(nameof(Index));
    }

    private async Task ChargerListesAsync(ReservationFormViewModel form, CancellationToken cancellationToken)
    {
        var clients = await _clients.ListerAsync(cancellationToken);
        form.ClientsDisponibles = clients.Select(c => new SelectListItem(c.NomComplet, c.Id.ToString()));

        var voitures = await _voitures.ListerAsync(cancellationToken);
        form.VoituresDisponibles = voitures.Select(v => new SelectListItem(
            $"{v.Modele?.Marque?.Nom} {v.Modele?.Nom} ({v.Immatriculation}) — {v.TarifJournalier:0.00} €/j",
            v.Id.ToString()));
    }
}

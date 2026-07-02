using Locatic.Application.Services;
using Locatic.Domain.Entities;
using Locatic.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Locatic.Web.Controllers;

public class ClientsController : Controller
{
    private readonly IClientService _clients;

    public ClientsController(IClientService clients)
    {
        _clients = clients;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var clients = await _clients.ListerAsync(cancellationToken);
        return View(clients);
    }

    [HttpGet]
    public IActionResult Create() => View(new ClientFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ClientFormViewModel form, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(form);

        var client = new Client
        {
            Nom = form.Nom,
            Prenom = form.Prenom,
            Email = form.Email,
            Telephone = form.Telephone
        };

        var resultat = await _clients.CreerAsync(client, cancellationToken);
        if (!resultat.Succes)
        {
            ModelState.AddModelError(string.Empty, resultat.Erreur!);
            return View(form);
        }

        TempData["Succes"] = $"Le client « {client.NomComplet} » a été ajouté.";
        return RedirectToAction(nameof(Index));
    }
}

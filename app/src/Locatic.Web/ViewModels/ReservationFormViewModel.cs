using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Locatic.Web.ViewModels;

public class ReservationFormViewModel
{
    [Required(ErrorMessage = "Veuillez choisir un client.")]
    [Range(1, int.MaxValue, ErrorMessage = "Veuillez choisir un client.")]
    [Display(Name = "Client")]
    public int ClientId { get; set; }

    [Required(ErrorMessage = "Veuillez choisir une voiture.")]
    [Range(1, int.MaxValue, ErrorMessage = "Veuillez choisir une voiture.")]
    [Display(Name = "Voiture")]
    public int VoitureId { get; set; }

    [Required(ErrorMessage = "La date de début est obligatoire.")]
    [DataType(DataType.Date)]
    [Display(Name = "Date de début")]
    public DateOnly DateDebut { get; set; }

    [Required(ErrorMessage = "La date de fin est obligatoire.")]
    [DataType(DataType.Date)]
    [Display(Name = "Date de fin")]
    public DateOnly DateFin { get; set; }

    public IEnumerable<SelectListItem> ClientsDisponibles { get; set; } = new List<SelectListItem>();
    public IEnumerable<SelectListItem> VoituresDisponibles { get; set; } = new List<SelectListItem>();
}

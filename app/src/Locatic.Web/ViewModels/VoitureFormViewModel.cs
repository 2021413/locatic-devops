using System.ComponentModel.DataAnnotations;
using Locatic.Domain.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Locatic.Web.ViewModels;

public class VoitureFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "L'immatriculation est obligatoire.")]
    [StringLength(15, ErrorMessage = "L'immatriculation ne peut dépasser 15 caractères.")]
    [Display(Name = "Immatriculation")]
    public string Immatriculation { get; set; } = string.Empty;

    [Range(1950, 2100, ErrorMessage = "Veuillez saisir une année valide.")]
    [Display(Name = "Année")]
    public int Annee { get; set; } = 2024;

    [Range(0.01, 100000, ErrorMessage = "Le tarif journalier doit être strictement positif.")]
    [Display(Name = "Tarif journalier (€)")]
    public decimal TarifJournalier { get; set; }

    [Range(1, 9, ErrorMessage = "Le nombre de places doit être compris entre 1 et 9.")]
    [Display(Name = "Nombre de places")]
    public int NombrePlaces { get; set; } = 5;

    [Display(Name = "Carburant")]
    public TypeCarburant Carburant { get; set; }

    [Required(ErrorMessage = "Veuillez choisir un modèle.")]
    [Range(1, int.MaxValue, ErrorMessage = "Veuillez choisir un modèle.")]
    [Display(Name = "Modèle")]
    public int ModeleId { get; set; }

    /// <summary>Liste déroulante des modèles (« Marque — Modèle »), remplie par le controller.</summary>
    public IEnumerable<SelectListItem> ModelesDisponibles { get; set; } = new List<SelectListItem>();
}

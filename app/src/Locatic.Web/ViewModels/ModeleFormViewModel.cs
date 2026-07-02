using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Locatic.Web.ViewModels;

public class ModeleFormViewModel
{
    [Required(ErrorMessage = "Le nom est obligatoire.")]
    [StringLength(100, ErrorMessage = "Le nom ne peut dépasser 100 caractères.")]
    [Display(Name = "Nom du modèle")]
    public string Nom { get; set; } = string.Empty;

    [Required(ErrorMessage = "Veuillez choisir une marque.")]
    [Range(1, int.MaxValue, ErrorMessage = "Veuillez choisir une marque.")]
    [Display(Name = "Marque")]
    public int MarqueId { get; set; }

    /// <summary>Liste déroulante des marques existantes (remplie par le controller).</summary>
    public IEnumerable<SelectListItem> MarquesDisponibles { get; set; } = new List<SelectListItem>();
}

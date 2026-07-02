using System.ComponentModel.DataAnnotations;

namespace Locatic.Web.ViewModels;

public class MarqueFormViewModel
{
    [Required(ErrorMessage = "Le nom est obligatoire.")]
    [StringLength(100, ErrorMessage = "Le nom ne peut dépasser 100 caractères.")]
    [Display(Name = "Nom de la marque")]
    public string Nom { get; set; } = string.Empty;

    [StringLength(80)]
    [Display(Name = "Pays d'origine")]
    public string? PaysOrigine { get; set; }
}

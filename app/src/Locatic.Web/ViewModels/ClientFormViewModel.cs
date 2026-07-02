using System.ComponentModel.DataAnnotations;

namespace Locatic.Web.ViewModels;

public class ClientFormViewModel
{
    [Required(ErrorMessage = "Le nom est obligatoire.")]
    [StringLength(80)]
    [Display(Name = "Nom")]
    public string Nom { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le prénom est obligatoire.")]
    [StringLength(80)]
    [Display(Name = "Prénom")]
    public string Prenom { get; set; } = string.Empty;

    [Required(ErrorMessage = "L'email est obligatoire.")]
    [EmailAddress(ErrorMessage = "Veuillez saisir un email valide.")]
    [StringLength(150)]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Veuillez saisir un numéro de téléphone valide.")]
    [StringLength(20)]
    [Display(Name = "Téléphone")]
    public string? Telephone { get; set; }
}

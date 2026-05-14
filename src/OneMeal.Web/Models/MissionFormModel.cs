using System.ComponentModel.DataAnnotations;

namespace OneMeal.Web.Models;

public sealed class MissionFormModel
{
    [Required]
    public string Ingredients { get; set; } = string.Empty;

    [Range(5, 180)]
    public int AvailableMinutes { get; set; } = 20;

    public string Goal { get; set; } = "alta proteina";

    public string DietaryPreference { get; set; } = "Carnívoro";

    public string AgendaContext { get; set; } = "janela curta entre duas reunioes";
}

using System.ComponentModel.DataAnnotations;

namespace ProjectDataService.Entities;

public class Category
{
    [MaxLength(36)]
    public string Id { get; init; } = Guid.NewGuid().ToString();

    [MaxLength(64)]
    public string Name { get; set; } = string.Empty;
}
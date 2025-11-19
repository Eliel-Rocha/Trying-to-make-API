using System.ComponentModel.DataAnnotations;

// Este modelo representa os dados que o formulário público para geração de chave API 
namespace API_ovni.Models;
public class ApiKeyRequest
{
    [Required]
    public string FirstName { get; set; }

    [Required]
    public string LastName { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; }

    public string? UsageDescription { get; set; }
}
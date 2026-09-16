namespace Syner.Api.Domain.Entities;

public sealed class Rol : EntityBase
{
public string Nombre { get; set; } = string.Empty;

public string Descripcion { get; set; } = string.Empty;

public ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();

}

namespace digitalArsv1.DTOs
{
    public class UsuarioUpdateDTO
{
    public int Id { get; set; }
    public string Nombre { get; set; } = null!;
    public string Apellido { get; set; } = null!;
    public string Direccion { get; set; } = null!;
    public string Mail { get; set; } = null!;
    public string Telefono { get; set; } = null!;
}}
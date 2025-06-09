using System.ComponentModel.DataAnnotations.Schema;
using digitalArsv1.DTOs;

using digitalArsv1.Models;
using digitalArsv1.Repositories;

namespace digitalArsv1.DTOs
{

    public class UsuarioProfileDTO
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Direccion { get; set; }
        public string Mail { get; set; }
        public string Telefono { get; set; }
    }
}
using digitalArsv1.Models;
using digitalArsv1.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace digitalArsv1.Models
{
    public class LoginRequest
    {
        public int nro_usuario { get; set; }
        public string acceso { get; set; }
    }
}
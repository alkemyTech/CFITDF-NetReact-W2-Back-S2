﻿using Microsoft.AspNetCore.Authorization;
using digitalArsv1.DTOs;
using digitalArsv1.Models;
using digitalArsv1.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Linq;                 // ← NECESARIO para .Select(...) en UsuarioDTO
using Microsoft.Extensions.Configuration; // ✅ NUEVO: para poder usar IConfiguration

[ApiController]
[Route("api/[controller]")]
public class UsuariosController : ControllerBase
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly ICuentaRepository _cuentaRepository;
    private readonly IConfiguration _config;

    public UsuariosController(
        IUsuarioRepository usuarioRepository,
        ICuentaRepository cuentaRepository,   // ✅ NUEVO: inyectar también ICuentaRepository
        IConfiguration config)
    {
        _usuarioRepository = usuarioRepository;
        _cuentaRepository = cuentaRepository; // ✅ NUEVO: asignar la dependencia inyectada
        _config = config;
    }
    // ================================
    [HttpGet] // muestra usuarios que puede ver el admin
    public async Task<ActionResult<IEnumerable<UsuarioDTO>>> GetUsuarios()
    {
        var usuarios = await _usuarioRepository.GetAllAsync();
        var usuariosDto = usuarios.Select(u => new UsuarioDTO
        {
            Id = u.nro_cliente,
            mail = u.mail,
            Nombre = u.nombre + " " + u.apellido,  // Para concatenar nombre + apellido,
            tipo_cliente = u.tipo_cliente
        });

        return Ok(usuariosDto);
    }
    // ================================
    [HttpGet("{id}")] // levanta datos del usuario id
    public async Task<ActionResult<UsuarioDTO>> GetUsuario(int id)
    {
        var usuario = await _usuarioRepository.GetByIdAsync(id);
        if (usuario == null)
            return NotFound();

        var usuarioDto = new UsuarioDTO
        {
            Id = usuario.nro_cliente,
            mail = usuario.mail,
            Nombre = usuario.nombre + " " + usuario.apellido,
            tipo_cliente = usuario.tipo_cliente
        };

        return Ok(usuarioDto);
    }
    // ================================
    [HttpPost] // METODO POST - crea usuario de app
    public async Task<ActionResult<UsuarioDTO>> PostUsuario([FromBody] RegisterRequest dto)
    {
        // 1️⃣ Validar datos obligatorios
        if (string.IsNullOrWhiteSpace(dto.Mail))
            return BadRequest("El correo (Mail) es obligatorio.");
        if (string.IsNullOrWhiteSpace(dto.Password))
            return BadRequest("La contraseña (Password) es obligatoria.");
        if (string.IsNullOrWhiteSpace(dto.Nombre)
           || string.IsNullOrWhiteSpace(dto.Apellido))
            return BadRequest("Nombre y Apellido son obligatorios.");
        if (string.IsNullOrWhiteSpace(dto.Tipo_cliente))
            return BadRequest("El tipo de cliente es obligatorio.");

        // 2️⃣ Verificar si ya existe un usuario con ese correo
        var existente = await _usuarioRepository.ObtenerPorEmailAsync(dto.Mail);
        if (existente != null)
            return Conflict("Ya existe un usuario registrado con ese correo.");

        // 3️⃣ Mapear DTO -> Entidad Usuario y generar hash BCrypt
        var usuario = new Usuario
        {
            mail = dto.Mail,
            nombre = dto.Nombre,
            apellido = dto.Apellido,
            direccion = dto.Direccion,
            telefono = dto.Telefono,
            tipo_cliente = dto.Tipo_cliente,
            // Aquí generamos el HASH (por ejemplo: "$2a$10$abcd...") en NVARCHAR(MAX)
            password_hash = BCrypt.Net.BCrypt.HashPassword(dto.Password) // ✅ NUEVO
        };

        // 4️⃣ Guardar en la base de datos
        await _usuarioRepository.CrearAsync(usuario);
        await _usuarioRepository.SaveAsync();

        // 5️⃣ Preparar DTO de respuesta (no expongo el hash)
        var usuarioDto = new UsuarioDTO
        {
            Id = usuario.nro_cliente,
            mail = usuario.mail,
            Nombre = usuario.nombre + " " + usuario.apellido,
            tipo_cliente = usuario.tipo_cliente
        };

        return CreatedAtAction(nameof(GetUsuario),
            new { id = usuario.nro_cliente },
            usuarioDto);
    }
    // ================================
    [HttpPost("login")]
    public async Task<ActionResult> Login([FromBody] LoginDTO dto)
    {
        // 1️⃣ Buscar el usuario por email
        var usuario = await _usuarioRepository.ObtenerPorEmailAsync(dto.Mail);
        if (usuario == null)
            return Unauthorized("Credenciales inválidas.");

        // 2️⃣ Validar formato BCrypt del password_hash
        if (string.IsNullOrWhiteSpace(usuario.password_hash) ||
              !(usuario.password_hash.StartsWith("$2a$") ||
                usuario.password_hash.StartsWith("$2b$") ||
                usuario.password_hash.StartsWith("$2y$")))
        {
            return Unauthorized("Credenciales inválidas.");
        }

        // 3️⃣ Verificar la contraseña con try/catch
        try
        {
            bool esValida = BCrypt.Net.BCrypt.Verify(dto.Password, usuario.password_hash);
            if (!esValida)
                return Unauthorized("Credenciales inválidas.");
        }
        catch (BCrypt.Net.SaltParseException)
        {
            return Unauthorized("Credenciales inválidas.");
        }

        // 4️⃣ Crear claims y token JWT
        var claims = new[]
        {
        new Claim(ClaimTypes.NameIdentifier, usuario.nro_cliente.ToString()),
        new Claim(ClaimTypes.Email, usuario.mail),
        new Claim(ClaimTypes.Role, "Billetera") // o el rol real que corresponda
    };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: creds
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        // 5️⃣ Devolver token y datos básicos del usuario en un objeto JSON
        return Ok(new
        {
            token = tokenString,
            usuario = new
            {
                nombre = usuario.nombre,
                apellido = usuario.apellido,
                mail = usuario.mail,
                tipo_cliente = usuario.tipo_cliente
            }
        });
    }
    /*/////////////////////////////*/
    [Authorize]
    [HttpGet("perfil")]
    public async Task<IActionResult> GetPerfil()
    {
        var email = User.FindFirstValue(ClaimTypes.Email);

        var usuario = await _usuarioRepository.GetByMailWithCuentasAsync(email);

        if (usuario == null)
            return NotFound("Usuario no encontrado");

        // Tomamos la primera cuenta activa (o la que corresponda) para mostrar datos
        var cuenta = usuario.Cuentas.FirstOrDefault(c => c.estado);

        return Ok(new
        {
            nombre = usuario.nombre,
            nro_cuenta = cuenta?.nro_cuenta ?? 0,
            saldo = cuenta?.saldo ?? 0m,
            cbu = cuenta?.CBU ?? "Sin CBU"
        });
    }
    // ================================
    // ==== MÉTODO PUT PARA ACTUALIZAR DATOS DEL USUARIO (sin ID en la ruta) ====
    [Authorize(Roles = "BILLETERA")]
    [HttpPut]
    public async Task<IActionResult> UpdateUsuario([FromBody] UpdateUsuarioDTO dto)
    {
        // 1️⃣ Validar que el DTO no sea null
        if (dto == null)
            return BadRequest("El cuerpo de la petición (UpdateUsuarioDTO) no puede estar vacío.");

        // 2️⃣ Extraer nro_cliente desde el JWT
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? User.FindFirstValue("sub");
        if (!int.TryParse(userId, out var nroClienteToken))
            return Forbid("Token inválido.");

        // 3️⃣ Obtener el usuario existente usando el ID del token
        var usuarioExistente = await _usuarioRepository.GetByIdAsync(nroClienteToken);
        if (usuarioExistente == null)
            return NotFound($"No se encontró el usuario con ID = {nroClienteToken}.");

        // 4️⃣ Actualizar únicamente los campos diferentes y no vacíos
        var cambioRealizado = false;

        if (!string.IsNullOrWhiteSpace(dto.Nombre) && dto.Nombre != usuarioExistente.nombre)
        {
            usuarioExistente.nombre = dto.Nombre;
            cambioRealizado = true;
        }
        if (!string.IsNullOrWhiteSpace(dto.Apellido) && dto.Apellido != usuarioExistente.apellido)
        {
            usuarioExistente.apellido = dto.Apellido;
            cambioRealizado = true;
        }
        if (!string.IsNullOrWhiteSpace(dto.Direccion) && dto.Direccion != usuarioExistente.direccion)
        {
            usuarioExistente.direccion = dto.Direccion;
            cambioRealizado = true;
        }
        if (!string.IsNullOrWhiteSpace(dto.Mail) && dto.Mail != usuarioExistente.mail)
        {
            usuarioExistente.mail = dto.Mail;
            cambioRealizado = true;
        }
        if (!string.IsNullOrWhiteSpace(dto.Telefono) && dto.Telefono != usuarioExistente.telefono)
        {
            usuarioExistente.telefono = dto.Telefono;
            cambioRealizado = true;
        }

        // 5️⃣ Si no se actualizó nada válido, informar al cliente
        if (!cambioRealizado)
            return BadRequest("No se detectó ningún cambio en los datos.");

        // 6️⃣ Persistir cambios en la base de datos
        _usuarioRepository.Update(usuarioExistente);
        await _usuarioRepository.SaveAsync();

        // 7️⃣ Devolver mensaje genérico
        return Ok("Datos actualizados exitosamente");
    }
    [Authorize]
    [HttpPut("basic-profile")]
    public async Task<IActionResult> UpdateBasicProfile([FromBody] UsuarioUpdateDTO updateDto)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out int userId))
            return Unauthorized("Token inválido o sin ID");

        if (userId != updateDto.Id)
            return Forbid("No puede modificar el perfil de otro usuario.");

        bool actualizado = await _usuarioRepository.UpdateProfileAsync(updateDto);

        if (!actualizado)
            return BadRequest("No se pudo actualizar el perfil.");

        return NoContent();
    }
    [Authorize]
    [HttpGet("basic-profile")]
    public async Task<ActionResult<UsuarioUpdateDTO>> GetBasicProfile()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out int userId))
            return Unauthorized("Token inválido o sin ID");

        var usuario = await _usuarioRepository.GetUsuarioByIdAsync(userId);
        if (usuario == null)
            return NotFound("Usuario no encontrado.");

        var dto = new UsuarioUpdateDTO
        {
            Id = usuario.nro_cliente,
            Nombre = usuario.nombre,
            Apellido = usuario.apellido,
            Mail = usuario.mail,
            Telefono = usuario.telefono,
            Direccion = usuario.direccion
        };

        return Ok(dto);
    }
    // ================================
}




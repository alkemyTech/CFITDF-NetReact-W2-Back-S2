using digitalArsv1.Models;
using digitalArsv1.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class TransaccionesController : ControllerBase
{
    private readonly ITransaccionRepository _transaccionRepository;

    public TransaccionesController(ITransaccionRepository transaccionRepository)
    {
        _transaccionRepository = transaccionRepository;
    }

    /*
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Transaccion>>> GetTransacciones()
        {
            var transacciones = await _transaccionRepository.GetAllAsync();
            return Ok(transacciones);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Transaccion>> GetTransaccion(int id)
        {
            var transaccion = await _transaccionRepository.GetByIdAsync(id);
            if (transaccion == null)
                return NotFound();

            return Ok(transaccion);
        }

        [HttpPost]
        public async Task<ActionResult<Transaccion>> PostTransaccion(Transaccion transaccion)
        {
            await _transaccionRepository.AddAsync(transaccion);
            return CreatedAtAction(nameof(GetTransaccion), new { id = transaccion.codigo_transaccion }, transaccion);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutTransaccion(int id, Transaccion transaccion)
        {
            if (id != transaccion.codigo_transaccion)
                return BadRequest();

            await _transaccionRepository.UpdateAsync(transaccion);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTransaccion(int id)
        {
            await _transaccionRepository.DeleteAsync(id);
            return NoContent();
        }
    */
}
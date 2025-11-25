using DRAPI.DTO;
using DRCore.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DRAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CFDIController : ControllerBase
    {
        private readonly ICFDI _cfdi;

        public CFDIController(ICFDI cfdi)
        {
            _cfdi = cfdi;
        }

        [HttpPost]
        public async Task<IActionResult> Timbrar([FromBody] TimbrarDTO request)
        {
            try
            {
                var result = await _cfdi.Timbrar(request.ObjType, request.DocEntry);
                if (result.IsSuccess)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result.Message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return BadRequest("Error al timbrar el documento");
            }
        }
    }
}

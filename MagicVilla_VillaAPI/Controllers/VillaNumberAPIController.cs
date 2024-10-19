using AutoMapper;
using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Models.DTO;
using MagicVilla_VillaAPI.Repository.IRepository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace MagicVilla_VillaAPI.Controllers
{
    [Route("api/VillaNumberAPI")]
    [ApiController]
    public class VillaNumberAPIController : ControllerBase
    {
        private readonly IVillaNumberRepository db;
        private readonly IVillaRepository dbVilla;
        private readonly IMapper mapper;
        public VillaNumberAPIController(IVillaNumberRepository db1, IMapper mapper,IVillaRepository db2)
        {
            this.db = db1;
            this.mapper = mapper;
            dbVilla = db2;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<VillaNumberDTO>>> GetAllVillaNumbers()
        {
            try
            {
                var villaNumbers = await db.GetAllAsync();
                var villlaNumbersList = mapper.Map<IEnumerable<VillaNumberDTO>>(villaNumbers);
                return Ok(villlaNumbersList);
            }
            catch (Exception ex) {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpGet("{villaNo:int}",Name = "GetVillaNumber")]
        public async Task<ActionResult<VillaNumberDTO>> GetVillaNumber(int villaNo)
        {
            try
            {
                if (villaNo == 0)
                {
                    return BadRequest();
                }
                var villaNumber = await db.GetAsync(villaNumber => villaNumber.VillaNo == villaNo);
                if (villaNumber == null)
                {
                    return NotFound();
                }
                var villaNumberDTO = mapper.Map<VillaNumberDTO>(villaNumber);
                return Ok(villaNumberDTO);
            }
            catch (Exception ex) {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPost]
        public async Task<ActionResult<VillaNumberDTO>> Create([FromBody] VillaNumberCreateDTO villaNumberCreateDTO)
        {
            try
            {
                if (villaNumberCreateDTO == null)
                {
                    return BadRequest();
                }
                var villaNumber = await db.GetAsync(villaNumber => villaNumber.VillaNo == villaNumberCreateDTO.VillaNo);
                if (villaNumber != null)
                {
                    ModelState.AddModelError("", "Already exists");
                    return BadRequest(ModelState);
                }
                if(await dbVilla.GetAsync(u => u.Id == villaNumberCreateDTO.VillaID) == null)
                {
                    ModelState.AddModelError("", "Villa doesnot exist");
                    return BadRequest(ModelState);
                }
                villaNumber = mapper.Map<VillaNumber>(villaNumberCreateDTO);
                villaNumber.CreatedTime = DateTime.Now;
                await db.CreateAsync(villaNumber);
                return CreatedAtRoute("GetVillaNumber", new { villaNo = villaNumber.VillaNo }, villaNumberCreateDTO);
            }catch(Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpDelete("{villaNo:int}")]
        public async Task<IActionResult> Delete(int villaNo)
        {
            try
            {
                if (villaNo == 0)
                {
                    return BadRequest();
                }
                var villaNumber = await db.GetAsync(u => u.VillaNo == villaNo);
                if (villaNumber == null)
                {
                    return NotFound();
                }
                await db.RemoveAsync(villaNumber);
                return NoContent();
            }
            catch (Exception ex) {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPut("{villaNo:int}")]
        public async Task<IActionResult> Update(int villaNo,VillaNumberUpdateDTO villaNumberUpdateDTO)
        {
            try
            {
                if (villaNo != villaNumberUpdateDTO.VillaNo || villaNumberUpdateDTO == null)
                {
                    return BadRequest();
                }
                if (await db.GetAsync(u => u.VillaNo == villaNo, false) == null)
                {
                    return NotFound();
                }
                if (await dbVilla.GetAsync(u => u.Id == villaNumberUpdateDTO.VillaID) == null)
                {
                    ModelState.AddModelError("", "Villa doesnot exist");
                    return BadRequest(ModelState);
                }
                var villaNumber = mapper.Map<VillaNumber>(villaNumberUpdateDTO);
                await db.UpdateAsync(villaNumber);
                return NoContent();
            }
            catch (Exception ex) {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}

using MagicVilla_VillaAPI.Data;
using MagicVilla_VillaAPI.Models.DTO;
using MagicVilla_VillaAPI.Models;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using MagicVilla_VillaAPI.Repository.IRepository;

namespace MagicVilla_VillaAPI.Controllers
{
    [Route("api/VillaAPI")]
    [ApiController]
    public class VillaAPIController : ControllerBase
    {
        //private readonly ILogging logger; using custom logger
        protected APIResponse response;
        private readonly IVillaRepository db;
        private readonly IMapper mapper;
        //private readonly ILogger<VillaAPIController> logger;
        //public VillaAPIController(ILogger<VillaAPIController> logger) { 
        //    this.logger = logger;
        //}
        public VillaAPIController(IVillaRepository db,IMapper mapper)
        {
            this.mapper = mapper;
            this.db = db;
            this.response = new();
        }

        [HttpGet]
        public async Task<ActionResult<APIResponse>> GetVillas()
        {
            IEnumerable<Villa> villaList = await db.GetAllAsync();
            response.Result = mapper.Map<IEnumerable<VillaDTO>>(villaList);
            response.StatusCode = System.Net.HttpStatusCode.OK;
            return Ok(response);
        }


        [HttpGet("{id:int}", Name = "GetVilla")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        //[ProducesResponseType((200) , Type = typeof(VillaDTO))]
        //[ProducesResponseType(400)]
        //[ProducesResponseType(404)]
        public async Task<ActionResult<APIResponse>> GetVillas(int id)
        {
            if (id == 0)
            {
                return BadRequest();
            }
            var villa = await db.GetAsync(villa => villa.Id == id);
            if (villa == null)
            {
                return NotFound();
            }
            response.Result = mapper.Map<VillaDTO>(villa);
            response.StatusCode = System.Net.HttpStatusCode.OK;
            return Ok(response);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<APIResponse>> CreateVilla([FromBody]VillaCreateDTO villaDTO) {

            if (await db.GetAsync(u => u.Name.ToLower() == villaDTO.Name.ToLower()) != null) {
                ModelState.AddModelError("", "Villa already Exists!");
                return BadRequest(ModelState);
            }
            if (villaDTO == null)
            {
                return BadRequest(villaDTO);
            }

            var villa = mapper.Map<Villa>(villaDTO);
            villa.CreatedDate = DateTime.Now;
            await db.CreateAsync(villa);
            response.Result = mapper.Map<VillaDTO>(villa);
            response.StatusCode = System.Net.HttpStatusCode.Created;
            return CreatedAtRoute("GetVilla", new { id = villa.Id },response);
        }

        [HttpDelete("{id:int}", Name = "DeleteVilla")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<APIResponse>> DeleteVilla(int id) {
            if (id == 0) {
                return BadRequest();
            }
            try
            {
                var villa = await db.GetAsync(u => u.Id == id);
                if (villa == null)
                {
                    return NotFound();
                }
                await db.RemoveAsync(villa);
            }catch(Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            response.StatusCode = System.Net.HttpStatusCode.NoContent;
            response.IsSuccess = true;
            return Ok(response);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<APIResponse>> UpdateVilla(int id, [FromBody] VillaUpdateDTO villaDTO)
        {
            if (villaDTO == null || id != villaDTO.Id) {
                return BadRequest();
            }
            try
            {
                var villa = mapper.Map<Villa>(villaDTO);
                //VillaStore.villaList.Add(villaDTO);
                await db.UpdateAsync(villa);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            response.StatusCode = System.Net.HttpStatusCode.NoContent;
            response.IsSuccess = true;
            return Ok(response);
        }

        [HttpPatch("{id:int}")]
        public async Task<IActionResult> updatePartialVilla(int id,JsonPatchDocument<VillaUpdateDTO> patchDTO) {
            if (id == 0||patchDTO==null)
            {
                return BadRequest();
            }
            var villa = await db.GetAsync(u => u.Id == id,tracked:false);
            VillaUpdateDTO villaDTO = mapper.Map<VillaUpdateDTO>(villa);
            if (villa == null)
            {
                return NotFound();
            }
            patchDTO.ApplyTo(villaDTO, ModelState);
            if (!ModelState.IsValid) {
                return BadRequest(ModelState);
            }
            try
            {
                villa = mapper.Map<Villa>(villaDTO);
                //VillaStore.villaList.Add(villaDTO);
                await db.UpdateAsync(villa);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            return NoContent();
        }
    }
}

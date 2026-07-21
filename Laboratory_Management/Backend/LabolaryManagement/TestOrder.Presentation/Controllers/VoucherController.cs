using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TestOrder.Application.DTOs.Vouchers;
using TestOrder.Application.Services;

namespace TestOrder.Presentation.Controllers
{
    [Route("api/vouchers")]
    [ApiController]
    [Tags("Vouchers")]
    public class VoucherController : ControllerBase
    {
        private readonly VoucherService _voucherService;

        public VoucherController(VoucherService voucherService)
        {
            _voucherService = voucherService;
        }

        /// <summary>
        /// Get a list of vouchers with pagination (REST GET)
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _voucherService.GetAllVouchersPagedAsync(pageNumber, pageSize);
            return Ok(result);
        }

        /// <summary>
        /// Get details of a single voucher by ID (REST GET)
        /// </summary>
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            var voucher = await _voucherService.GetVoucherByIdAsync(id);
            if (voucher == null)
            {
                return NotFound(new { message = $"Voucher with ID {id} not found." });
            }
            return Ok(voucher);
        }

        /// <summary>
        /// Create a new voucher (REST POST)
        /// </summary>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] CreateVoucherDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createdVoucher = await _voucherService.CreateVoucherAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = createdVoucher.VoucherId }, createdVoucher);
        }

        /// <summary>
        /// Update a voucher (REST PUT)
        /// </summary>
        [HttpPut("{id:int}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateVoucherDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var updatedVoucher = await _voucherService.UpdateVoucherAsync(id, dto);
            if (updatedVoucher == null)
            {
                return NotFound(new { message = $"Voucher with ID {id} not found." });
            }
            return Ok(updatedVoucher);
        }

        /// <summary>
        /// Delete a voucher (REST DELETE)
        /// </summary>
        [HttpDelete("{id:int}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _voucherService.DeleteVoucherAsync(id);
            if (!result)
            {
                return NotFound(new { message = $"Voucher with ID {id} not found." });
            }
            return NoContent();
        }

        /// <summary>
        /// Validate and calculate discount for a voucher (REST Action POST)
        /// </summary>
        [HttpPost("validate")]
        [AllowAnonymous]
        public async Task<IActionResult> ValidateVoucher([FromBody] ApplyVoucherDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _voucherService.ValidateAndCalculateDiscountAsync(dto.Code, dto.OrderValue);
            if (!response.IsValid)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }
    }
}

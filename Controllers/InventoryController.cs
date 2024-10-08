using Microsoft.AspNetCore.Mvc;
using ECommerceAPI.Models;
using ECommerceAPI.Services;

namespace ECommerceAPI.Controllers
{
    [Route("api/inventory")]
    [ApiController]
    public class InventoryController : ControllerBase
    {
        private readonly InventoryService _inventoryService;

        public InventoryController(InventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        // GET: api/inventory
        [HttpGet]
        public async Task<List<Inventory>> Get() =>
            await _inventoryService.GetAsync();

        // GET api/inventory/64a51019c925955cfda51194
        [HttpGet("{id:length(24)}")]
        public async Task<ActionResult<Inventory>> Get(string id)
        {
            var inventory = await _inventoryService.GetAsync(id);
            if (inventory == null)
            {
                return NotFound("Inventory record not found.");
            }

            return inventory;
        }

        // POST api/inventory
        [HttpPost]
        public async Task<ActionResult<Inventory>> Post(Inventory newInventory)
        {
            await _inventoryService.CreateAsync(newInventory);
            return CreatedAtAction(nameof(Get), new { id = newInventory.Id }, newInventory);
        }

        // PUT api/inventory/64a51019c925955cfda51194
        [HttpPut("{id:length(24)}")]
        public async Task<IActionResult> Put(string id, Inventory updatedInventory)
        {
            var inventory = await _inventoryService.GetAsync(id);
            if (inventory == null)
            {
                return NotFound("Inventory record not found.");
            }

            updatedInventory.Id = inventory.Id;
            await _inventoryService.UpdateAsync(id, updatedInventory);

            return Ok("Inventory updated successfully.");
        }

        // DELETE api/inventory/64a51019c925955cfda51194
        [HttpDelete("{id:length(24)}")]
        public async Task<IActionResult> Delete(string id)
        {
            var inventory = await _inventoryService.GetAsync(id);
            if (inventory == null)
            {
                return NotFound("Inventory record not found.");
            }

            await _inventoryService.RemoveAsync(id);
            return Ok("Inventory deleted successfully.");
        }

        // PUT api/inventory/64a51019c925955cfda51194/stock
        [HttpPut("{id:length(24)}/stock")]
        public async Task<IActionResult> UpdateStock(string id, [FromBody] int stockQuantity)
        {
            var inventory = await _inventoryService.GetAsync(id);
            if (inventory == null)
            {
                return NotFound("Inventory record not found.");
            }

            await _inventoryService.UpdateStockAsync(id, stockQuantity);
            return Ok("Stock updated successfully.");
        }

        // GET api/inventory/lowstock
        [HttpGet("lowstock")]
        public async Task<List<Inventory>> GetLowStock() =>
            await _inventoryService.GetLowStockItemsAsync();

        // PUT api/inventory/64a51019c925955cfda51194/alert
        [HttpPut("{id:length(24)}/alert")]
        public async Task<IActionResult> UpdateLowStockAlert(string id, [FromBody] bool enableAlert)
        {
            var inventory = await _inventoryService.GetAsync(id);
            if (inventory == null)
            {
                return NotFound("Inventory record not found.");
            }

            await _inventoryService.UpdateLowStockAlertAsync(id, enableAlert);
            return Ok(enableAlert ? "Low stock alert enabled." : "Low stock alert disabled.");
        }
    }
}

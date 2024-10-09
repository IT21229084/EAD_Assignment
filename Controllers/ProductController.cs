using Microsoft.AspNetCore.Mvc;
using ECommerceAPI.Models;
using ECommerceAPI.Services;

namespace ECommerceAPI.Controllers
{
    [Route("api/product")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly ProductService _productService;

        public ProductController(ProductService productService)
        {
            _productService = productService;
        }

        // GET: api/product
        [HttpGet]
        public async Task<List<Product>> Get() =>
            await _productService.GetAsync();

        // GET api/product/64a51019c925955cfda51194
        [HttpGet("{id:length(24)}")]
        public async Task<ActionResult<Product>> Get(string id)
        {
            var product = await _productService.GetAsync(id);
            if (product == null)
            {
                return NotFound("Product not found.");
            }

            return product;
        }
        [HttpGet("vendor/{vendorId}")]
        public async Task<ActionResult<List<Product>>> GetProductsByVendorId(string vendorId)
        {
            if (string.IsNullOrEmpty(vendorId))
            {
                return BadRequest("Vendor ID is required.");
            }

            var products = await _productService.GetProductsByVendorIdAsync(vendorId);

            if (products == null || products.Count == 0)
            {
                return NotFound("No products found for the given vendor ID.");
            }

            return Ok(products);
        }

        // POST api/product
        [HttpPost]
        public async Task<ActionResult<Product>> Post(Product newProduct)
        {
            await _productService.CreateAsync(newProduct);
            return CreatedAtAction(nameof(Get), new { id = newProduct.Id }, newProduct);
        }

        // PUT api/product/64a51019c925955cfda51194
        [HttpPut("{id:length(24)}")]
        public async Task<IActionResult> Put(string id, Product updatedProduct)
        {
            var product = await _productService.GetAsync(id);
            if (product == null)
            {
                return NotFound("Product not found.");
            }

            // Preserve existing fields if they are not being updated
            updatedProduct.Name = !string.IsNullOrEmpty(updatedProduct.Name) ? updatedProduct.Name : product.Name;
            updatedProduct.Description = !string.IsNullOrEmpty(updatedProduct.Description) ? updatedProduct.Description : product.Description;
            updatedProduct.Category = !string.IsNullOrEmpty(updatedProduct.Category) ? updatedProduct.Category : product.Category;
            updatedProduct.Price = updatedProduct.Price > 0 ? updatedProduct.Price : product.Price;
            updatedProduct.StockQuantity = updatedProduct.StockQuantity >= 0 ? updatedProduct.StockQuantity : product.StockQuantity;
            updatedProduct.IsActive = updatedProduct.IsActive;

            // Keep the original Id
            updatedProduct.Id = product.Id;

            await _productService.UpdateAsync(id, updatedProduct);
            return Ok("Product updated successfully.");
        }


        // DELETE api/product/64a51019c925955cfda51194
        [HttpDelete("{id:length(24)}")]
        public async Task<IActionResult> Delete(string id)
        {
            var product = await _productService.GetAsync(id);
            if (product == null)
            {
                return NotFound("Product not found.");
            }

            await _productService.RemoveAsync(id);
            return Ok("Product deleted successfully.");
        }

        // PUT api/product/64a51019c925955cfda51194/status
        [HttpPut("{id:length(24)}/status")]
        public async Task<ActionResult> UpdateProductStatus(string id)
        {
            try
            {
                await _productService.UpdateProductStatusAsync(id);
                return Ok("Product status updated successfully.");
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // PUT api/product/64a51019c925955cfda51194/stock
        [HttpPut("{id:length(24)}/stock")]
        public async Task<IActionResult> UpdateStock(string id, [FromBody] int stockQuantity)
        {
            var product = await _productService.GetAsync(id);
            if (product == null)
            {
                return NotFound("Product not found.");
            }

            await _productService.UpdateStockAsync(id, stockQuantity);
            return Ok("Stock updated successfully.");
        }

        // GET api/product/category/electronics
        [HttpGet("category/{category}")]
        public async Task<List<Product>> GetByCategory(string category) =>
            await _productService.GetByCategoryAsync(category);

        // GET api/product/lowstock/10
        [HttpGet("lowstock/{threshold:int}")]
        public async Task<List<Product>> GetLowStock(int threshold) =>
            await _productService.GetLowStockProductsAsync(threshold);
    }
}

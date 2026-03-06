using Microsoft.AspNetCore.Mvc;
using Core.Entities;
using Core.Interfaces;
using Core.Specifications;
using Core.Helpers;
using API.Services;

namespace API.Controllers
{
    public class ProductsController : BaseApiController
    {
        private readonly IGenericRepository<Product> _repository;
        private readonly ProductCacheService _productCache;

        public ProductsController(IGenericRepository<Product> repository, ProductCacheService productCache)
        {
            _repository = repository;
            _productCache = productCache;
        }

        /// <summary>
        /// Get all products with optional filtering, sorting, and pagination. Cached in Redis (30-day expiry).
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<Pagination<Product>>> GetProducts([FromQuery] ProductSpecParams specParams)
        {
            var cached = await _productCache.GetProductsAsync(specParams);
            if (cached != null)
                return Ok(cached);

            var spec = new ProductSpecification(specParams);
            var countSpec = new ProductWithFiltersForCountSpecification(specParams);
            var items = await _repository.GetAllAsync(spec);
            var totalItems = await _repository.CountAsync(countSpec);
            var pagination = new Pagination<Product>(specParams.PageIndex, specParams.PageSize, totalItems, items);

            await _productCache.SetProductsAsync(specParams, pagination);
            return Ok(pagination);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            var cached = await _productCache.GetProductAsync(id);
            if (cached != null)
                return Ok(cached);

            var product = await _repository.GetByIdAsync(id);
            if (product == null)
                return NotFound(new { message = $"Product with ID {id} not found" });

            await _productCache.SetProductAsync(product);
            return Ok(product);
        }

        [HttpGet("brands")]
        public async Task<ActionResult<IEnumerable<string>>> GetBrands()
        {
            var cached = await _productCache.GetBrandsAsync();
            if (cached != null)
                return Ok(cached);

            var spec = new ProductBrandSpecification();
            var brands = (await _repository.GetAllAsync<string>(spec)).ToList();
            await _productCache.SetBrandsAsync(brands);
            return Ok(brands);
        }

        [HttpGet("types")]
        public async Task<ActionResult<IEnumerable<string>>> GetTypes()
        {
            var cached = await _productCache.GetTypesAsync();
            if (cached != null)
                return Ok(cached);

            var spec = new ProductTypeSpecification();
            var types = (await _repository.GetAllAsync<string>(spec)).ToList();
            await _productCache.SetTypesAsync(types);
            return Ok(types);
        }

        [HttpPost]
        public async Task<ActionResult<Product>> CreateProduct([FromBody] Product product)
        {
            var createdProduct = await _repository.AddAsync(product);
            return CreatedAtAction(nameof(GetProduct), new { id = createdProduct.Id }, createdProduct);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] Product product)
        {
            if (id != product.Id)
            {
                return BadRequest(new { message = "ID in URL does not match ID in request body" });
            }

            if (!await _repository.ExistsAsync(id))
            {
                return NotFound(new { message = $"Product with ID {id} not found" });
            }

            await _repository.UpdateAsync(product);
            await _productCache.RemoveProductAsync(id);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            if (!await _repository.ExistsAsync(id))
            {
                return NotFound(new { message = $"Product with ID {id} not found" });
            }

            await _repository.DeleteAsync(id);
            await _productCache.RemoveProductAsync(id);
            return NoContent();
        }
    }
}
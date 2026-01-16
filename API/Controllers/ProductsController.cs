using Microsoft.AspNetCore.Mvc;
using Core.Entities;
using Core.Interfaces;
using Core.Specifications;
using Core.Helpers;
using Swashbuckle.AspNetCore.Annotations;

namespace API.Controllers
{
    public class ProductsController : BaseApiController
    {
        private readonly IGenericRepository<Product> _repository;

        public ProductsController(IGenericRepository<Product> repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// Get all products with optional filtering, sorting, and pagination
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<Pagination<Product>>> GetProducts([FromQuery] ProductSpecParams specParams)
        {
            var spec = new ProductSpecification(specParams);
            var countSpec = new ProductWithFiltersForCountSpecification(specParams);

            return await CreatePageResult(
                _repository,
                spec,
                countSpec,
                specParams.PageIndex,
                specParams.PageSize
            );
        }

        /// <summary>
        /// Get a specific product by ID
        /// </summary>
        [HttpGet("{id}")]
        [SwaggerOperation(Summary = "Get product by ID")]
        [SwaggerResponse(200, "Successfully retrieved product", typeof(Product))]
        [SwaggerResponse(404, "Product not found")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            var product = await _repository.GetByIdAsync(id);

            if (product == null)
            {
                return NotFound(new { message = $"Product with ID {id} not found" });
            }

            return Ok(product);
        }

        /// <summary>
        /// Get all distinct product brands
        /// </summary>
        [HttpGet("brands")]
        [SwaggerOperation(Summary = "Get all brands")]
        [SwaggerResponse(200, "Successfully retrieved brands", typeof(IEnumerable<string>))]
        public async Task<ActionResult<IEnumerable<string>>> GetBrands()
        {
            var spec = new ProductBrandSpecification();
            var brands = await _repository.GetAllAsync<string>(spec);
            return Ok(brands);
        }

        /// <summary>
        /// Get all distinct product types
        /// </summary>
        [HttpGet("types")]
        [SwaggerOperation(Summary = "Get all types")]
        [SwaggerResponse(200, "Successfully retrieved types", typeof(IEnumerable<string>))]
        public async Task<ActionResult<IEnumerable<string>>> GetTypes()
        {
            var spec = new ProductTypeSpecification();
            var types = await _repository.GetAllAsync<string>(spec);
            return Ok(types);
        }

        /// <summary>
        /// Create a new product
        /// </summary>
        [HttpPost]
        [SwaggerOperation(Summary = "Create a new product")]
        [SwaggerResponse(201, "Product successfully created", typeof(Product))]
        [SwaggerResponse(400, "Invalid product data")]
        public async Task<ActionResult<Product>> CreateProduct([FromBody] Product product)
        {
            var createdProduct = await _repository.AddAsync(product);
            return CreatedAtAction(nameof(GetProduct), new { id = createdProduct.Id }, createdProduct);
        }

        /// <summary>
        /// Update an existing product
        /// </summary>
        [HttpPut("{id}")]
        [SwaggerOperation(Summary = "Update a product")]
        [SwaggerResponse(204, "Product successfully updated")]
        [SwaggerResponse(400, "Invalid product data")]
        [SwaggerResponse(404, "Product not found")]
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
            return NoContent();
        }

        /// <summary>
        /// Delete a product
        /// </summary>
        [HttpDelete("{id}")]
        [SwaggerOperation(Summary = "Delete a product")]
        [SwaggerResponse(204, "Product successfully deleted")]
        [SwaggerResponse(404, "Product not found")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            if (!await _repository.ExistsAsync(id))
            {
                return NotFound(new { message = $"Product with ID {id} not found" });
            }

            await _repository.DeleteAsync(id);
            return NoContent();
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using Core.Entities;
using Core.Interfaces;
using Swashbuckle.AspNetCore.Annotations;

namespace API.Controllers
{
    public class CartController : BaseApiController
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        /// <summary>
        /// Get shopping cart by ID
        /// </summary>
        [HttpGet()]
        [SwaggerOperation(Summary = "Get cart by ID")]
        [SwaggerResponse(200, "Successfully retrieved cart", typeof(ShoppingCart))]
        public async Task<ActionResult<ShoppingCart>> GetCart(string id)
        {
            var cart = await _cartService.GetCartAsync(id);
            
            return Ok(cart ?? new ShoppingCart { Id = id });
        }

        /// <summary>
        /// Create or update shopping cart
        /// </summary>
        [HttpPost]
        [SwaggerOperation(Summary = "Create or update cart")]
        [SwaggerResponse(200, "Cart successfully saved", typeof(ShoppingCart))]
        [SwaggerResponse(400, "Invalid cart data")]
        public async Task<ActionResult<ShoppingCart>> UpdateCart([FromBody] ShoppingCart cart)
        {
            var updatedCart = await _cartService.SetCartAsync(cart);

            if (updatedCart == null)
            {
                return BadRequest(new { message = "Failed to save cart" });
            }

            return Ok(updatedCart);
        }

        /// <summary>
        /// Delete shopping cart
        /// </summary>
        [HttpDelete()]
        [SwaggerOperation(Summary = "Delete cart by ID")]
        [SwaggerResponse(200, "Cart successfully deleted")]
        [SwaggerResponse(404, "Cart not found")]
        public async Task<ActionResult> DeleteCart(string id)
        {
            var result = await _cartService.DeleteCartAsync(id);

            if (!result)
            {
                return NotFound(new { message = $"Cart with ID {id} not found" });
            }

            return Ok(new { message = "Cart deleted successfully" });
        }
    }
}
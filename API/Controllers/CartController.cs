using Microsoft.AspNetCore.Mvc;
using Core.Entities;
using Core.Interfaces;
using API.DTOs;

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
        /// Demo checkout: accepts cart id and optional user/shipping info, clears the cart. No real payment.
        /// </summary>
        [HttpPost("checkout")]
        public async Task<ActionResult> Checkout([FromBody] CheckoutRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.CartId))
                return BadRequest(new { message = "CartId is required" });

            var deleted = await _cartService.DeleteCartAsync(request.CartId);
            if (!deleted)
                return NotFound(new { message = "Cart not found or already cleared" });

            return Ok(new { message = "Order placed (demo). Cart cleared.", orderId = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant() });
        }

        [HttpGet()]
        public async Task<ActionResult<ShoppingCart>> GetCart(string id)
        {
            var cart = await _cartService.GetCartAsync(id);
            
            return Ok(cart ?? new ShoppingCart { Id = id });
        }

        [HttpPost]
        public async Task<ActionResult<ShoppingCart>> UpdateCart([FromBody] ShoppingCart cart)
        {
            var updatedCart = await _cartService.SetCartAsync(cart);

            if (updatedCart == null)
            {
                return BadRequest(new { message = "Failed to save cart" });
            }

            return Ok(updatedCart);
        }

        [HttpDelete()]
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
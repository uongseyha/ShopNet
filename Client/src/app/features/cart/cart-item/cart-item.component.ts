import { Component, inject, input } from '@angular/core';
import { Router } from '@angular/router';
import { CartItem } from '../../../shared/models/cart';
import { CurrencyPipe } from '@angular/common';
import { MatIconButton } from '@angular/material/button';
import { MatIcon } from '@angular/material/icon';
import { CartService } from '../../../core/services/cart.service';

@Component({
  selector: 'app-cart-item',
  imports: [CurrencyPipe, MatIconButton, MatIcon],
  templateUrl: './cart-item.component.html',
  styleUrl: './cart-item.component.css',
})
export class CartItemComponent {
  item = input.required<CartItem>();
  cartService = inject(CartService);
  router = inject(Router);

  incrementQuantity() {
    this.cartService.addItemToCart(this.item(), 1);
  }

  decrementQuantity() {
    this.cartService.removeItemFromCart(this.item().productId, 1);
  }

  removeItem() {
    this.cartService.removeItemFromCart(this.item().productId, this.item().quantity);
  }

  goToProduct() {
    this.router.navigate(['/shop', this.item().productId]);
  }
}

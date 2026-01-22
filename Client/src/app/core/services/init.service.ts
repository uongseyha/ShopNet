import { inject, Injectable } from '@angular/core';
import { CartService } from './cart.service';
import { firstValueFrom, of } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class InitService {
  private cartService = inject(CartService);

  init() {
    const cartId = localStorage.getItem('cart_id');
    
    if (cartId) {
      return firstValueFrom(
        this.cartService.getCart(cartId)
      ).catch((error) => {
        console.error('Failed to load cart:', error);
        // Clear invalid cart ID from localStorage
        localStorage.removeItem('cart_id');
        return null;
      });
    }
    
    return Promise.resolve(null);
  }
}

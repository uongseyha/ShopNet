import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { CartService } from '../../core/services/cart.service';
import { CartItemComponent } from './cart-item/cart-item.component';
import { OrderSummaryComponent } from '../../shared/components/order-summary/order-summary.component';
import { CurrencyPipe } from '@angular/common';
import { MatButton } from '@angular/material/button';
import { MatIcon } from '@angular/material/icon';

@Component({
  selector: 'app-cart',
  imports: [CartItemComponent, OrderSummaryComponent, CurrencyPipe, MatButton, MatIcon],
  templateUrl: './cart.component.html',
  styleUrl: './cart.component.css',
})
export class CartComponent {
  cartService = inject(CartService);
  router = inject(Router);

  getAppliedVoucher(): string | null {
    const cart = this.cartService.cart();
    return cart?.coupon?.promotionCode || null;
  }

  goToShop() {
    this.router.navigate(['/shop']);
  }

  onCheckout() {
    this.router.navigate(['/checkout']);
  }

  async onApplyVoucher(code: string) {
    try {
      const coupon = await this.cartService.applyDiscount(code).toPromise();
      if (coupon) {
        const cart = this.cartService.cart();
        if (cart) {
          cart.coupon = coupon;
          await this.cartService.setCart(cart).toPromise();
          console.log('Voucher applied successfully');
        }
      }
    } catch (error) {
      console.error('Failed to apply voucher:', error);
    }
  }

  async onRemoveVoucher() {
    const cart = this.cartService.cart();
    if (cart) {
      cart.coupon = undefined;
      await this.cartService.setCart(cart).toPromise();
      console.log('Voucher removed');
    }
  }
}

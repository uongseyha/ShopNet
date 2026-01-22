import { RouterLink, ActivatedRoute } from '@angular/router';
import { Product } from '../../shared/models/product';
import { ShopService } from './../../core/services/shop.service';
import { CartService } from '../../core/services/cart.service';
import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIcon } from "@angular/material/icon";
import { MatFormField, MatInput, MatLabel } from "@angular/material/input";
import { MatDivider } from "@angular/material/divider";
import { MatCardModule } from '@angular/material/card';
import { MatButton, MatIconButton } from '@angular/material/button';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-product-details',
  imports: [
    MatCardModule,
    CommonModule,
    MatIcon,
    MatDivider,
    MatButton,
    MatIconButton,
    MatInput,
    FormsModule
],
  templateUrl: './product-details.component.html',
  styleUrl: './product-details.component.css',
})
export class ProductDetailsComponent implements OnInit {
  private shopService = inject(ShopService);
  private cartService = inject(CartService);
  private activatedRoute = inject(ActivatedRoute);
  product = signal<Product | null>(null);
  quantity = signal<number>(1);
  addingToCart = signal<boolean>(false);

  ngOnInit(): void {
    this.loadProduct();
  }

  loadProduct() {
    const id = this.activatedRoute.snapshot.paramMap.get('id');
    if (!id) return;
    this.shopService.getProduct(+id).subscribe({
        next: (product) => {
          this.product.set(product);
          // Set initial quantity from cart if product already exists in cart
          //this.setInitialQuantityFromCart(product.id);
        },
        error: (error) => {
          console.error('Error fetching product details:', error);
        },
      });
  }

  setInitialQuantityFromCart(productId: number) {
    const cart = this.cartService.cart();
    if (cart) {
      const cartItem = cart.items.find(item => item.productId === productId);
      if (cartItem) {
        this.quantity.set(cartItem.quantity);
      } else {
        this.quantity.set(1);
      }
    } else {
      this.quantity.set(1);
    }
  }

  incrementQuantity() {
    const currentQty = this.quantity();
    const maxStock = this.product()?.quantityInStock || 0;
    if (currentQty < maxStock) {
      this.quantity.set(currentQty + 1);
    }
  }

  decrementQuantity() {
    const currentQty = this.quantity();
    if (currentQty > 1) {
      this.quantity.set(currentQty - 1);
    }
  }

  updateQuantity(value: string) {
    const qty = parseInt(value, 10);
    const maxStock = this.product()?.quantityInStock || 0;
    if (!isNaN(qty) && qty >= 1) {
      this.quantity.set(Math.min(qty, maxStock));
    } else {
      this.quantity.set(1);
    }
  }

  async addToCart() {
    const product = this.product();
    if (!product) return;
    
    this.addingToCart.set(true);
    try {
      await this.cartService.addItemToCart(product, this.quantity());
      console.log('Added to cart successfully');
      // Keep the quantity as is after adding (don't reset to 1)
    } catch (error) {
      console.error('Failed to add to cart:', error);
    } finally {
      this.addingToCart.set(false);
    }
  }
}

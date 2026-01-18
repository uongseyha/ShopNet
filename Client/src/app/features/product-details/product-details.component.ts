import { RouterLink, ActivatedRoute } from '@angular/router';
import { Product } from '../../shared/models/product';
import { ShopService } from './../../core/services/shop.service';
import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIcon } from "@angular/material/icon";
import { MatFormField, MatInput, MatLabel } from "@angular/material/input";
import { MatDivider } from "@angular/material/divider";
import { MatCardModule } from '@angular/material/card';
import { MatButton } from '@angular/material/button';

@Component({
  selector: 'app-product-details',
  imports: [
    MatCardModule,
    CommonModule,
    MatIcon,
    MatFormField,
    MatLabel,
    MatDivider,
    MatButton,
    MatInput
],
  templateUrl: './product-details.component.html',
  styleUrl: './product-details.component.css',
})
export class ProductDetailsComponent implements OnInit {
  private shopService = inject(ShopService);
  private activatedRoute = inject(ActivatedRoute);
  product = signal<Product | null>(null);

  ngOnInit(): void {
    this.loadProduct();
  }

  loadProduct() {
    const id = this.activatedRoute.snapshot.paramMap.get('id');
    if (!id) return;
    this.shopService.getProduct(+id).subscribe({
        next: (product) => {
          this.product.set(product);
        },
        error: (error) => {
          console.error('Error fetching product details:', error);
        },
      });
  }
}

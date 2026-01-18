import { Component, Input } from '@angular/core';
import { MatCard, MatCardModule } from "@angular/material/card";
import { Product } from '../../../shared/models/product';
import { MatButtonModule } from '@angular/material/button';
import { MatIcon } from "@angular/material/icon";
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-product-items',
  imports: [MatCardModule, MatButtonModule, MatIcon, CommonModule],
  templateUrl: './product-items.component.html',
  styleUrl: './product-items.component.css',
})
export class ProductItemsComponent {
  @Input() product!: Product;
}

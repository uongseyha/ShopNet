import { Component, inject, OnInit, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { HeaderComponent } from "./layout/header/header.component";
import { Product } from './shared/models/product';
import { ShopComponent } from "./features/shop/shop.component";

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, HeaderComponent, ShopComponent],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App implements OnInit {
  products = signal<Product[]>([]);

  ngOnInit(): void {
  }
}

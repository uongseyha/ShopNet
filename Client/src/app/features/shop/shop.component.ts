import { Component, inject, OnInit, signal, HostListener, effect } from '@angular/core';
import { Product } from '../../shared/models/product';
import { ShopParams } from '../../shared/models/shopParams';
import { ShopService } from '../../core/services/shop.service';
import { MatCard, MatCardModule } from '@angular/material/card';
import { ProductItemsComponent } from "./product-items/product-items.component";
import { MatIcon } from "@angular/material/icon";
import { MatDialog } from '@angular/material/dialog';
import { FiltersDialogComponent } from './filters-dialog/filters-dialog.component';
import { MatButton } from '@angular/material/button';
import { MatMenu, MatMenuItem, MatMenuTrigger } from '@angular/material/menu';
import { Pagination } from '../../shared/models/pagination';
import { FormsModule } from '@angular/forms';
import { MatFormField, MatInput, MatLabel } from '@angular/material/input';
import { MatProgressSpinner } from '@angular/material/progress-spinner';
import { Subject } from 'rxjs';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';

@Component({
  selector: 'app-shop',
  imports: [
    ProductItemsComponent,
    MatIcon,
    MatButton,
    MatCardModule,
    MatMenu,
    MatMenuItem,
    MatMenuTrigger,
    FormsModule,
    MatFormField,
    MatInput,
    MatLabel,
    MatProgressSpinner
],
  templateUrl: './shop.component.html',
  styleUrl: './shop.component.css',
})
export class ShopComponent implements OnInit {
  private shopService = inject(ShopService);
  private dialogService = inject(MatDialog)
  products = signal<Pagination<Product> | null>(null)
  allProducts = signal<Product[]>([]);
  loading = signal<boolean>(false);
  searchTerm = '';
  private searchSubject = new Subject<string>();
  shopParams: ShopParams = {
    brands: [],
    types: [],
    sort: 'name',
    pageNumber: 1,
    pageSize: 10
  };
  sortOptions = [
    { name: 'Alphabetical', value: 'name' },
    { name: 'Price: Low-High', value: 'priceAsc' },
    { name: 'Price: High-Low', value: 'priceDesc' }
  ];
  
  constructor() {
    this.searchSubject.pipe(
      debounceTime(500),
      distinctUntilChanged()
    ).subscribe(searchTerm => {
      this.shopParams.search = searchTerm;
      this.shopParams.pageNumber = 1;
      this.getProducts();
    });
  }
  
  ngOnInit(): void {
    this.initialzeShop();
  }

  initialzeShop() {
    this.shopService.getTypes();
    this.shopService.getBrands();
    this.getProducts();
  }

  getProducts(append: boolean = false) {
    if (this.loading()) return;
    this.loading.set(true);
    this.shopService.getProducts(this.shopParams).subscribe({
      next: (response) => {
        this.products.set(response);
        if (append) {
          this.allProducts.set([...this.allProducts(), ...response.data]);
        } else {
          this.allProducts.set(response.data);
        }
        this.loading.set(false);
      },
      error: (error) => {
        console.error(error);
        this.loading.set(false);
      }
    });
  }
    
  openFiltersDialog() {
    const dialogRef = this.dialogService.open(FiltersDialogComponent, {
      minWidth: '500px',
      data: {
        selectedBrands: this.shopParams.brands,
        selectedTypes: this.shopParams.types
      }
    });
    dialogRef.afterClosed().subscribe({
      next: result => {
        if (result) {
          this.shopParams.types = result.selectedTypes;
          this.shopParams.brands = result.selectedBrands;
          this.shopParams.pageNumber = 1;
          this.getProducts(); 
        }
      },
    });
  }

  onSortChange(sortValue: string) {
    this.shopParams.sort = sortValue;
    this.shopParams.pageNumber = 1;
    this.getProducts();
  }

  loadMore() {
    this.shopParams.pageNumber++;
    this.getProducts(true);
  }

  hasMoreProducts(): boolean {
    return this.products() ? this.allProducts().length < this.products()!.count : false;
  }

  onSearch() {
    this.searchSubject.next(this.searchTerm);
  }

  @HostListener('window:scroll', [])
  onWindowScroll() {
    const scrollPosition = window.pageYOffset + window.innerHeight;
    const documentHeight = document.documentElement.scrollHeight;
    
    if (scrollPosition >= documentHeight - 100 && this.hasMoreProducts() && !this.loading()) {
      this.loadMore();
    }
  }
}

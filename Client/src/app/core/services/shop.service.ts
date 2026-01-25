import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';
import { Product } from '../../shared/models/product';
import { Pagination } from '../../shared/models/pagination';
import { ShopParams } from '../../shared/models/shopParams';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root',
})
export class ShopService {
  private http = inject(HttpClient);
  private baseUrl = environment.baseUrl + 'products';
  brands: string[] = [];
  types: string[] = [];

  /**
   * Get all products with optional filtering and pagination
   */
  getProducts(params?: ShopParams): Observable<Pagination<Product>> {
    let httpParams = new HttpParams();

    if (params) {
      if (params.brands && params.brands.length > 0) {
        httpParams = httpParams.append('brands', params.brands.join(','));
      }
      if (params.types && params.types.length > 0) {
        httpParams = httpParams.append('types', params.types.join(','));
      }
      if (params.sort) {
        httpParams = httpParams.append('sort', params.sort);
      }
      if (params.pageNumber) {
        httpParams = httpParams.append('pageIndex', params.pageNumber.toString());
      }
      if (params.pageSize) {
        httpParams = httpParams.append('pageSize', params.pageSize.toString());
      }
      if (params.search) {
        httpParams = httpParams.append('search', params.search);
      }
    }

    return this.http.get<Pagination<Product>>(this.baseUrl, { params: httpParams });
  }

  /**
   * Get a single product by ID
   */
  getProduct(id: number): Observable<Product> {
    return this.http.get<Product>(`${this.baseUrl}/${id}`);
  }

  /**
   * Get all available brands
   */
  getBrands() {
    if (this.brands.length > 0) return;

    return this.http.get<string[]>(`${this.baseUrl}/brands`).subscribe({
      next: (response) => (this.brands = response),
      error: (error) => console.error(error),
    });
  }

  /**
   * Get all available types
   */
  getTypes() {
    if (this.types.length > 0) return;

    return this.http.get<string[]>(`${this.baseUrl}/types`).subscribe({
      next: (response) => this.types=response,
      error: (error) => console.error(error)
    });
  }
}

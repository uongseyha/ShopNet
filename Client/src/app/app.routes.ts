import { Routes } from '@angular/router';

export const routes: Routes = [
  { 
    path: '', 
    loadComponent: () => import('./features/home/home.component').then(m => m.HomeComponent) 
  },
  { 
    path: 'home', 
    loadComponent: () => import('./features/home/home.component').then(m => m.HomeComponent) 
  },
  { 
    path: 'shop', 
    loadComponent: () => import('./features/shop/shop.component').then(m => m.ShopComponent) 
  },
  { 
    path: 'shop/:id', 
    loadComponent: () => import('./features/product-details/product-details.component').then(m => m.ProductDetailsComponent) 
  },
  { 
    path: 'cart', 
    loadComponent: () => import('./features/cart/cart.component').then(m => m.CartComponent) 
  },
  { 
    path: 'checkout', 
    loadComponent: () => import('./features/checkout/checkout.component').then(m => m.CheckoutComponent) 
  },
  { 
    path: 'test-error', 
    loadComponent: () => import('./features/test-error/test-error.component').then(m => m.TestErrorComponent) 
  },
  { 
    path: 'not-found', 
    loadComponent: () => import('./shared/components/not-found/not-found.component').then(m => m.NotFoundComponent) 
  },
  { 
    path: 'server-error', 
    loadComponent: () => import('./shared/components/server-error/server-error.component').then(m => m.ServerErrorComponent) 
  },
  { path: '**', redirectTo: 'not-found', pathMatch: 'full' }
];

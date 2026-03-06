import { Component, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { CartService } from '../../core/services/cart.service';
import { AccountService } from '../../core/services/account.service';
import { OrderSummaryComponent } from '../../shared/components/order-summary/order-summary.component';
import { CurrencyPipe } from '@angular/common';
import { MatButton } from '@angular/material/button';
import { MatFormField, MatLabel, MatError } from '@angular/material/form-field';
import { MatInput } from '@angular/material/input';
import { MatIcon } from '@angular/material/icon';
import { MatProgressSpinner } from '@angular/material/progress-spinner';

@Component({
  selector: 'app-checkout',
  imports: [
    ReactiveFormsModule,
    OrderSummaryComponent,
    CurrencyPipe,
    MatButton,
    MatFormField,
    MatLabel,
    MatInput,
    MatIcon,
    MatProgressSpinner,
    MatError
],
  templateUrl: './checkout.component.html',
  styleUrl: './checkout.component.css',
})
export class CheckoutComponent implements OnInit {
  private fb = inject(FormBuilder);
  private cartService = inject(CartService);
  private accountService = inject(AccountService);
  private router = inject(Router);

  checkoutForm!: FormGroup;
  submitting = signal(false);
  errorMessage = signal<string | null>(null);

  ngOnInit(): void {
    this.cartService.ensureCartLoaded();
    this.buildForm();
    this.prefillFromUser();
  }

  private buildForm(): void {
    this.checkoutForm = this.fb.group({
      firstName: ['', [Validators.required, Validators.minLength(2)]],
      lastName: ['', [Validators.required, Validators.minLength(2)]],
      email: ['', [Validators.required, Validators.email]],
      line1: ['', Validators.required],
      line2: [''],
      city: ['', Validators.required],
      state: ['', Validators.required],
      postalCode: ['', Validators.required],
      country: ['', Validators.required],
    });
  }

  private prefillFromUser(): void {
    const user = this.accountService.currentUser();
    if (user) {
      this.checkoutForm.patchValue({
        firstName: user.firstName ?? '',
        lastName: user.lastName ?? '',
        email: user.email ?? '',
      });
      const addr = (user as { address?: { line1?: string; line2?: string; city?: string; state?: string; country?: string; postalCode?: string } }).address;
      if (addr) {
        this.checkoutForm.patchValue({
          line1: addr.line1 ?? '',
          line2: addr.line2 ?? '',
          city: addr.city ?? '',
          state: addr.state ?? '',
          postalCode: addr.postalCode ?? '',
          country: addr.country ?? '',
        });
      }
    }
  }

  get cart() {
    return this.cartService.cart();
  }

  get totals() {
    return this.cartService.totals;
  }

  get itemCount() {
    return this.cartService.itemCount;
  }

  onSubmit(): void {
    this.errorMessage.set(null);
    if (this.checkoutForm.invalid || !this.cart?.id) {
      this.checkoutForm.markAllAsTouched();
      return;
    }
    this.submitting.set(true);
    this.cartService
      .checkout({
        cartId: this.cart.id,
        userInfo: {
          firstName: this.checkoutForm.get('firstName')?.value,
          lastName: this.checkoutForm.get('lastName')?.value,
          email: this.checkoutForm.get('email')?.value,
        },
        shippingAddress: {
          line1: this.checkoutForm.get('line1')?.value,
          line2: this.checkoutForm.get('line2')?.value || undefined,
          city: this.checkoutForm.get('city')?.value,
          state: this.checkoutForm.get('state')?.value,
          country: this.checkoutForm.get('country')?.value,
          postalCode: this.checkoutForm.get('postalCode')?.value,
        },
      })
      .subscribe({
        next: () => {
          this.submitting.set(false);
          this.router.navigate(['/shop'], { queryParams: { orderComplete: 'true' } });
        },
        error: (err) => {
          this.submitting.set(false);
          this.errorMessage.set(err.error?.message || 'Checkout failed. Please try again.');
        },
      });
  }

  goToCart(): void {
    this.router.navigate(['/cart']);
  }
}

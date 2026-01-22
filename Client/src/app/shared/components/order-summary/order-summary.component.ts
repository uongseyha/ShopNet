import { Component, inject, input, output, signal } from '@angular/core';
import { CurrencyPipe } from '@angular/common';
import { MatButton, MatIconButton } from '@angular/material/button';
import { MatIcon } from '@angular/material/icon';
import { MatFormField, MatLabel, MatSuffix } from '@angular/material/form-field';
import { MatInput } from '@angular/material/input';
import { FormsModule } from '@angular/forms';
import { CartService } from '../../../core/services/cart.service';

@Component({
  selector: 'app-order-summary',
  imports: [CurrencyPipe, MatButton, MatIconButton, MatIcon, MatFormField, MatInput, FormsModule],
  templateUrl: './order-summary.component.html',
  styleUrl: './order-summary.component.css',
})
export class OrderSummaryComponent {
  private cartService = inject(CartService);
  itemCount = input.required<number>();
  subtotal = input.required<number>();
  discount = input<number>(0);
  shipping = input<number>(0);
  total = input.required<number>();
  appliedVoucher = input<string | null>(null);

  voucherCode = signal<string>('');

  checkout = output<void>();
  continueShopping = output<void>();
  applyVoucher = output<string>();
  removeVoucher = output<void>();

  onApplyVoucher() {
    if (this.voucherCode().trim()) {
      this.applyVoucher.emit(this.voucherCode());
    }
  }

  onRemoveVoucher() {
    this.voucherCode.set('');
    this.removeVoucher.emit();
  }
}

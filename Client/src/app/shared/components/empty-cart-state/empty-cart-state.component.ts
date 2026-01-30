import { Component, input, output } from '@angular/core';
import { MatButton } from '@angular/material/button';
import { MatIcon } from '@angular/material/icon';

@Component({
  selector: 'app-empty-cart-state',
  imports: [MatButton, MatIcon],
  templateUrl: './empty-cart-state.component.html',
  styleUrl: './empty-cart-state.component.css',
})
export class EmptyCartStateComponent {
  title = input<string>('Your cart is empty');
  message = input<string>('Discover amazing products and start shopping!');
  buttonText = input<string>('Start Shopping');
  iconName = input<string>('shopping_cart');
  
  actionClick = output<void>();

  onActionClick() {
    this.actionClick.emit();
  }
}

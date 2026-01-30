import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButton } from '@angular/material/button';
import { MatCard } from '@angular/material/card';
import { MatError, MatFormField, MatLabel } from '@angular/material/form-field';
import { MatInput } from '@angular/material/input';
import { AccountService } from '../../../core/services/account.service';
import { Router, RouterLink } from '@angular/router';
import { SnackbarService } from '../../../core/services/snackbar.service';
import { TextInputComponent } from "../../../shared/components/text-input/text-input.component";

@Component({
  selector: 'app-register',
  imports: [
    ReactiveFormsModule,
    MatCard,
    MatButton,
    RouterLink,
    TextInputComponent
],
  templateUrl: './register.component.html',
  styleUrl: './register.component.css',
})
export class RegisterComponent {
  private fb = inject(FormBuilder);
  private accountService = inject(AccountService);
  private router = inject(Router);
  private snackbar = inject(SnackbarService);
  validationErrors: string[] = [];

  registerForm = this.fb.group({
    firstName: ['', Validators.required],
    lastName: ['', Validators.required],
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]]
  });

  onSubmit() {
    if (this.registerForm.valid) {
      this.validationErrors = []; // Clear previous errors
      this.accountService.register(this.registerForm.value).subscribe({
        next: (user) => {
          this.snackbar.success('Registration successful! Welcome to ShopNet.');
          //this.accountService.currentUser.set(user);
          this.router.navigateByUrl('/login');
        },
        error: (err) => {
          this.validationErrors = err || ['Registration failed'];
          this.snackbar.error('Registration failed. Please check the errors.');
        }
      });
    }
  }
}

import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { SnackbarService } from '../services/snackbar.service';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);
  const snackbar = inject(SnackbarService);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error) {
        switch (error.status) {
          case 400:
            // Bad Request - validation errors
            if (error.error?.errors) {
              const modelStateErrors: string[] = [];
              for (const key in error.error.errors) {
                if (error.error.errors[key]) {
                  modelStateErrors.push(error.error.errors[key]);
                }
              }
              throw modelStateErrors.flat();
            } else {
              snackbar.error(error.error?.message || error.message);
            }
            break;

          case 401:
            // Unauthorized
            snackbar.error(error.error?.message || 'You are not authorized');
            break;

          case 403:
            // Forbidden
            snackbar.error(error.error?.message || 'Access forbidden');
            break;

          case 404:
            // Not Found
            router.navigateByUrl('/not-found');
            break;

          case 500:
            // Internal Server Error
            const navigationExtras = {
              state: {
                error: error.error
              }
            };
            router.navigateByUrl('/server-error', navigationExtras);
            break;

          default:
            // Other errors
            snackbar.error('An unexpected error occurred');
            break;
        }
      }

      return throwError(() => error);
    })
  );
};

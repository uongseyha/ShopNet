import { Component, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { MatButton } from '@angular/material/button';

@Component({
  selector: 'app-test-error',
  imports: [MatButton],
  templateUrl: './test-error.component.html',
  styleUrl: './test-error.component.css'
})
export class TestErrorComponent {
  private http = inject(HttpClient);
  private baseUrl = '/api/buggy';
  validationErrors: string[] = [];

  get404Error() {
    this.http.get(this.baseUrl + '/notfound').subscribe({
      next: response => console.log(response),
      error: error => console.log(error)
    });
  }

  get400Error() {
    this.http.get(this.baseUrl + '/badrequest').subscribe({
      next: response => console.log(response),
      error: error => console.log(error)
    });
  }

  get401Error() {
    this.http.get(this.baseUrl + '/unauthorized').subscribe({
      next: response => console.log(response),
      error: error => console.log(error)
    });
  }

  get500Error() {
    this.http.get(this.baseUrl + '/servererror').subscribe({
      next: response => console.log(response),
      error: error => console.log(error)
    });
  }

  get400ValidationError() {
    this.http.post(this.baseUrl + '/validationerror', {}).subscribe({
      next: response => console.log(response),
      error: error => {
        console.log(error);
        this.validationErrors = error;
      }
    });
  }
}

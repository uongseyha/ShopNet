import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import { User } from '../../shared/models/user';
import { Address } from '../../shared/models/address';

@Injectable({
  providedIn: 'root',
})
export class AccountService {
  private http = inject(HttpClient);
  private baseUrl = environment.baseUrl;
  currentUser= signal<User | null>(null);

  register(values: any): Observable<User> {
    return this.http.post<User>(`${this.baseUrl}account/register`, values);
  }

  login(values: { email: string; password: string }) {
    return this.http.post<User>(`${this.baseUrl}account/login`, values, { withCredentials: true });
  }

  logout(){
    return this.http.post(`${this.baseUrl}account/logout`, {}, { withCredentials: true });
  }

  getUserInfo() {
    return this.http.get<User>(this.baseUrl + 'account/user-info').pipe(
      map(user => {
        this.currentUser.set(user);
        return user;
      })
    );
  }

  updateAddress(address: Address): Observable<Address> {
    return this.http.post<Address>(`${this.baseUrl}account/address`, address);
  }

  getAuthState() {
    return this.http.get<{ isAuthenticated: boolean }>(this.baseUrl + 'account/auth-status');
  }
  
}

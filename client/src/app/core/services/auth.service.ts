import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, catchError, mapTo, of, tap } from 'rxjs';
import { AuthState, AuthTokenResponse, LoginRequest, RegisterRequest } from '../models/auth.model';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly apiBaseUrl = environment.apiBaseUrl;
  private readonly accessTokenStorageKey = 'skyshop.accessToken';
  private readonly refreshTokenStorageKey = 'skyshop.refreshToken';
  private readonly emailStorageKey = 'skyshop.email';
  private readonly authStateSubject = new BehaviorSubject<AuthState>({
    email: this.getStoredEmail(),
    isAuthenticated: !!this.getStoredToken()
  });

  readonly authState$ = this.authStateSubject.asObservable();

  constructor(private http: HttpClient) {}

  login(request: LoginRequest): Observable<AuthTokenResponse> {
    return this.http.post<AuthTokenResponse>(`${this.apiBaseUrl}login`, request).pipe(
      tap((response) => this.storeAuth(response, request.email))
    );
  }

  register(request: RegisterRequest): Observable<void> {
    return this.http.post<void>(`${this.apiBaseUrl}register`, request);
  }

  logout(): Observable<void> {
    return this.http.post(`${this.apiBaseUrl}auth/logout`, {}).pipe(
      mapTo(void 0),
      catchError(() => of(void 0)),
      tap(() => this.clearAuth())
    );
  }

  clearAuth(): void {
    localStorage.removeItem(this.accessTokenStorageKey);
    localStorage.removeItem(this.refreshTokenStorageKey);
    localStorage.removeItem(this.emailStorageKey);
    this.authStateSubject.next({ email: null, isAuthenticated: false });
  }

  isLoggedIn(): boolean {
    return this.authStateSubject.value.isAuthenticated;
  }

  getAccessToken(): string | null {
    return this.getStoredToken();
  }

  getCurrentUserEmail(): string | null {
    return this.authStateSubject.value.email;
  }

  private storeAuth(response: AuthTokenResponse, email: string): void {
    localStorage.setItem(this.accessTokenStorageKey, response.accessToken);
    localStorage.setItem(this.refreshTokenStorageKey, response.refreshToken);
    localStorage.setItem(this.emailStorageKey, email);
    this.authStateSubject.next({ email, isAuthenticated: true });
  }

  private getStoredToken(): string | null {
    return localStorage.getItem(this.accessTokenStorageKey);
  }

  private getStoredEmail(): string | null {
    return localStorage.getItem(this.emailStorageKey);
  }
}
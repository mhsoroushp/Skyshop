import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, EMPTY, Observable, catchError, finalize, map, mapTo, of, shareReplay, tap, throwError } from 'rxjs';
import { AuthState, AuthTokenResponse, LoginRequest, RegisterRequest } from '../models/auth.model';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly apiBaseUrl = environment.apiBaseUrl;
  private accessToken: string | null = null;
  private refreshRequest$: Observable<AuthTokenResponse> | null = null;

  authEmail = signal<string | null>(null);
  
  private readonly authStateSubject = new BehaviorSubject<AuthState>({
    email: this.authEmail(),
    isAuthenticated: false
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

  refresh(): Observable<AuthTokenResponse> {
    if (!this.refreshRequest$) {
      this.refreshRequest$ = this.http.post<AuthTokenResponse>(`${this.apiBaseUrl}refresh`,{}, ).pipe(
        tap({
          next: (response) => this.storeAuth(response),
        }),
        shareReplay({ bufferSize: 1, refCount: true }),
        finalize(() => {
          this.refreshRequest$ = null;
        })
      );
    }

    return this.refreshRequest$;
  }

  restoreSession(): Observable<void> {
    return this.refresh().pipe(
      map(() => void 0)
    );
  }

  logout(): Observable<void> {
    return this.http.post(`${this.apiBaseUrl}logout`, {}).pipe(
      map(() => void 0),
      catchError(() => of(void 0)),
      tap(() => this.clearAuth())
    );
  }

  clearAuth(): void {
    this.authEmail.set(null);
    this.accessToken = null;
    this.authStateSubject.next({ email: null, isAuthenticated: false });
  }

  isLoggedIn(): boolean {
    return this.authStateSubject.value.isAuthenticated;
  }

  getAccessToken(): string | null {
    return this.accessToken;
  }

  private storeAuth(response: AuthTokenResponse, email?: string): void {
    this.accessToken = response.accessToken;
    const resolvedEmail = email ?? response.email ?? this.authEmail();

    if (resolvedEmail) {
      this.authEmail.set(resolvedEmail);
    }

    this.authStateSubject.next({
      email: resolvedEmail,
      isAuthenticated: true
    });
  }
}
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
  private authSignal = signal<AuthState | null>(null);

  public authState = this.authSignal.asReadonly();

  constructor(private http: HttpClient) {}

  login(request: LoginRequest): Observable<AuthTokenResponse> {
    return this.http.post<AuthTokenResponse>(`${this.apiBaseUrl}login`, request).pipe(
      tap((response) => this.storeAuth(response))
    );
  }

  register(request: RegisterRequest): Observable<void> {
    return this.http.post<void>(`${this.apiBaseUrl}register`, request);
  }

  refresh(): Observable<AuthTokenResponse> {
    if (!this.refreshRequest$) {
      this.refreshRequest$ = this.http.post<AuthTokenResponse>(`${this.apiBaseUrl}refresh`,{}).pipe(
        tap((response => {
          if(response) {
            this.storeAuth(response);
          }
        }))
        ,
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
    // correct place
    this.authSignal.set({
      email: null,
      isAuthenticated: false,
      roles: [],
      accessToken: null
    })
  }

  isLoggedIn(): boolean {
    return this.authState()?.isAuthenticated ?? false;
  }

  getAccessToken(): string | null {
    return this.authState()?.accessToken ?? null;
  }

  private storeAuth(response: AuthTokenResponse): void {
    if(!response) return;

    // correct place
    this.authSignal.set({
      email: response.email,
      isAuthenticated: true, 
      roles: response.roles,
      accessToken: response.accessToken
    })

  }
}
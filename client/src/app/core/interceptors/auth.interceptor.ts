import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { AuthService } from '../services/auth.service';
import { environment } from '../../../environments/environment';
import { catchError, switchMap, throwError } from 'rxjs';

const AUTH_PATHS = ['/login', '/register', '/refresh', '/auth/logout'];

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const token = authService.getAccessToken();

  const apiBaseUrl = environment.apiBaseUrl;
  const requestUrl = new URL(req.url, window.location.origin);
  const apiUrl = new URL(apiBaseUrl, window.location.origin);
  const isApiRequest = requestUrl.origin === apiUrl.origin && requestUrl.pathname.startsWith(apiUrl.pathname);
  const isAuthRequest = AUTH_PATHS.some((path) => requestUrl.pathname === `${apiUrl.pathname.replace(/\/$/, '')}${path}`);

  if (!isApiRequest || isAuthRequest || !token) {
    return next(req);
  }

  const authorizedRequest = req.clone({
    setHeaders: {
      Authorization: `Bearer ${token}`
    }
  });

  return next(authorizedRequest).pipe(
    catchError((error: unknown) => {
      if (!(error instanceof HttpErrorResponse) || error.status !== 401) {
        return throwError(() => error);
      }

      return authService.refresh().pipe(
        switchMap(() => {
          const refreshedToken = authService.getAccessToken();

          if (!refreshedToken) {
            return throwError(() => error);
          }

          return next(req.clone({
            setHeaders: {
              Authorization: `Bearer ${refreshedToken}`
            }
          }));
        }),
        catchError((refreshError: unknown) => {
          authService.clearAuth();
          return throwError(() => refreshError);
        })
      );
    })
  );
};
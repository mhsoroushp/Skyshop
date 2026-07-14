import { HttpInterceptorFn, HttpRequest } from '@angular/common/http';
import { inject } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { AuthService } from '../services/auth.service';
import { environment } from '../../../environments/environment';
import { catchError, switchMap, throwError } from 'rxjs';

// Don't attach a token to these endpoints
const AUTH_PATHS = ['/login', '/register', '/refresh', '/logout'];

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  

  const apiBaseUrl = environment.apiBaseUrl;
  const requestUrl = new URL(req.url, window.location.origin);
  const apiUrl = new URL(apiBaseUrl, window.location.origin);
  const isApiRequest = requestUrl.origin === apiUrl.origin && requestUrl.pathname.startsWith(apiUrl.pathname);
  const isAuthRequest = AUTH_PATHS.some((path) => requestUrl.pathname === `${apiUrl.pathname.replace(/\/$/, '')}${path}`);

  if (!isApiRequest || isAuthRequest) {
    return next(req);
  }

  const accessToken = authService.getAccessToken();


  // we already have a token
  if (accessToken) {
    return next(addBearerToken(req, accessToken));
  }

  // we don't have a token, try to refresh it

  return authService.refresh().pipe(
    switchMap((response) => {
      const newAccessToken = response.accessToken;
      return next(addBearerToken(req, newAccessToken)
      );
    })
  );
};

function addBearerToken(req: HttpRequest<unknown>, token: string) {
  return req.clone({
    setHeaders: {
      Authorization: `Bearer ${token}`
    },
    withCredentials: true
  });
}
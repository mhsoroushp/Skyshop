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
  const credentialedRequest = isApiRequest ? req.clone({ withCredentials: true }) : req;

  if (!isApiRequest || isAuthRequest) {
    return next(credentialedRequest);
  }

  const accessToken = authService.getAccessToken();


  // we already have a token
  if (accessToken) {
    return next(addBearerToken(credentialedRequest, accessToken));
  }

  // we don't have a token, try to refresh it, if it is already logged in

  return authService.refresh().pipe(
    switchMap((response) => {
      if(response){
        const newAccessToken = response.accessToken;
        return next(addBearerToken(credentialedRequest, newAccessToken));
      }
      return next(credentialedRequest);
    })
  );
};

function addBearerToken(req: HttpRequest<unknown>, token: string) {
  return req.clone({
    setHeaders: {
      Authorization: `Bearer ${token}`
    }
  });
}
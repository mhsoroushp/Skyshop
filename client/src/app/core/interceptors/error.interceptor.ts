import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';
import { SnackbarService } from '../services/snackbar.service';

const AUTH_ENDPOINTS = ['/login', '/register', '/refresh', '/logout'];

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
	const authService = inject(AuthService);
	const router = inject(Router);
	const snackBar = inject(SnackbarService);

	return next(req).pipe(
		catchError((err: HttpErrorResponse) => {
			
			if(err.status === 401) {
				snackBar.error(err.error?.message || 'Unauthorized request. Logging out...');
			}

			if(err.status === 403) {
				snackBar.error('Forbidden request. Logging out...');
			}

			if(err.status === 404) {
				router.navigate(['/not-found']);
			}

			if(err.status === 500) {
				snackBar.error('Internal server error. Please try again later.');
			}

			if(err.status === 0) {
				snackBar.error('Network error. Please check your internet connection.');
			}	

			return throwError(() => "nice, error happened");
		})
	);
};


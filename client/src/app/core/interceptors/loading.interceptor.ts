import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { LoadingService } from '../services/loading.service';
import { finalize } from 'rxjs/operators';

export const loadingInterceptor: HttpInterceptorFn = (req, next) => {
  var loadingService = inject(LoadingService);

  loadingService.showLoading();

  return next(req).pipe( 
    finalize(() => {
      loadingService.hideLoading();
    })
  );
};
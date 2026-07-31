import { Injectable, signal } from "@angular/core";

@Injectable({
  providedIn: 'root'
})
export class LoadingService {
private activeRequests = 0;
  loadingSignal = signal<boolean>(false);

  isLoading = this.loadingSignal.asReadonly();

  showLoading() {
    this.activeRequests++;
    this.loadingSignal.set(true);
  }

  hideLoading() {
    this.activeRequests--;
    if (this.activeRequests <= 0) {
      this.loadingSignal.set(false);
      this.activeRequests = 0;
    }
  }
}
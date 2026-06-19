// skyshop/src/app/basket/basket.service.ts
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, Subject, timeout } from 'rxjs';

export interface BasketItem {
  productId: number;
  quantity: number;
  price?: number;
  productName?: string;
}

export interface AddToBasketRequest {
  productId: number;
  quantity: number;
}

export interface UpdateQuantityRequest {
  quantity: number;
}

@Injectable({
  providedIn: 'root'
})
export class BasketService {
  private apiUrl = 'http://localhost:5057/api/basket';
  private basketUpdated$ = new Subject<void>();

  constructor(private http: HttpClient) { }

  getBasketUpdated(): Observable<void> {
    return this.basketUpdated$.asObservable();
  }

  notifyBasketUpdated(): void {
    console.log('🔔 Notifying basket updated...');
    this.basketUpdated$.next();
  }

  getBasket(): Observable<BasketItem[]> {
    console.log('📡 Calling getBasket API...');
    return this.http.get<BasketItem[]>(this.apiUrl, {
      withCredentials: true
    }).pipe(
      timeout(5000)
    );
  }

  addToBasket(productId: number, quantity: number): Observable<any> {
    console.log('📤 Calling addToBasket API for product:', productId, 'quantity:', quantity);
    const request: AddToBasketRequest = { productId, quantity };
    
    return this.http.post<any>(`${this.apiUrl}/add`, request, {
      withCredentials: true
    }).pipe(
      timeout(5000)
    );
  }

  removeFromBasket(productId: number): Observable<any> {
    return this.http.delete<any>(`${this.apiUrl}/product/${productId}`, {
      withCredentials: true
    }).pipe(
      timeout(5000)
    );
  }

  updateQuantity(productId: number, quantity: number): Observable<any> {
    const request: UpdateQuantityRequest = { quantity };

    return this.http.put<any>(`${this.apiUrl}/product/${productId}/quantity`, request, {
      withCredentials: true
    }).pipe(
      timeout(5000)
    );
  }

  clearBasket(): Observable<any> {
    return this.http.delete<any>(`${this.apiUrl}/clear`, {
      withCredentials: true
    }).pipe(
      timeout(5000)
    );
  }

  getBasketCount(): Observable<{ count: number }> {
    return this.http.get<{ count: number }>(`${this.apiUrl}/count`, {
      withCredentials: true
    }).pipe(
      timeout(5000)
    );
  }

  getBasketTotal(): Observable<{ total: number }> {
    return this.http.get<{ total: number }>(`${this.apiUrl}/total`, {
      withCredentials: true
    }).pipe(
      timeout(5000)
    );
  }
}
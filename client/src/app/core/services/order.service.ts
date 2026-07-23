import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, Subject, tap } from 'rxjs';
import { Order, CreateOrderRequest } from '../models/order.model';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class OrderService {
  private readonly apiBaseUrl = environment.apiBaseUrl;
  private readonly apiUrl =  `${this.apiBaseUrl}orders`;
  private _orderIdWithSuccessfulPaymentSubject = new Subject<string>();

  private _currentOrder= signal<Order | null>(null);

  currentOrder$ = this._currentOrder.asReadonly();
  orderIdWithSuccessfulPayment$ = this._orderIdWithSuccessfulPaymentSubject.asObservable();

  constructor(private http: HttpClient) {}

  createOrder(request: CreateOrderRequest): Observable<Order> {
    return this.http.post<Order>(`${this.apiUrl}`, request).
    pipe(
      tap((order) => {
        if (order) {
          console.log('Order created in service and set in signal:', order);
          this._currentOrder.set(order);
        }
      })
    );
  }

  clearCurrentOrderAfterPaymentSuccess(): void {
    this._currentOrder.set(null);
  }

  setOrderIdWithSuccessfulPayment(orderId: string): void {
    this._orderIdWithSuccessfulPaymentSubject.next(orderId);
  }

  getOrderById(orderId: string): Observable<Order> {
    return this.http.get<Order>(`${this.apiUrl}/${orderId}`).
    pipe(
      tap((order) => {
        if (order) {
          // this._currentOrder.set(order);
        }
      })
    );
  }

  getOrderBySessionId(sessionId: string): Observable<Order> {
    return this.http.get<Order>(`${this.apiUrl}/session/${sessionId}`).
    pipe(
      tap((order) => {
        if (order) {
          // this._currentOrder.set(order);
        }
      })
    );
  }
}

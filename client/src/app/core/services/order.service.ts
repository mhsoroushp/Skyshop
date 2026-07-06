import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Order, CreateOrderRequest } from '../models/order.model';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class OrderService {

  private readonly apiBaseUrl = environment.apiBaseUrl;
  private readonly apiUrl =  `${this.apiBaseUrl}orders`;

  constructor(private http: HttpClient) {}

  createOrder(request: CreateOrderRequest): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}`, request, {
      withCredentials: true 
    });
  }

  getOrder(orderId: string): Observable<Order> {
    return this.http.get<Order>(`${this.apiUrl}/${orderId}`, {
      withCredentials: true
    });
  }

  getOrderBySessionId(sessionId: string): Observable<Order> {
    return this.http.get<Order>(`${this.apiUrl}/session/${sessionId}`, {
      withCredentials: true 
    });
  }
}

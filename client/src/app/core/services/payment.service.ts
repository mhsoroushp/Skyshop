import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { 
  Payment, 
  CreatePaymentIntentRequest, 
  CreatePaymentIntentResponse, 
  ProcessPaymentRequest 
} from '../models/payment.model';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class PaymentService {
  private readonly apiUrl = `${environment.apiBaseUrl}payments`;

  constructor(private http: HttpClient) {}

  createPaymentIntent(request: CreatePaymentIntentRequest): Observable<CreatePaymentIntentResponse> {
    return this.http.post<CreatePaymentIntentResponse>(`${this.apiUrl}/create-intent`, request);
  }

  confirmPayment(request: ProcessPaymentRequest): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/confirm`, request);
  }

  getPayment(paymentId: string): Observable<Payment> {
    return this.http.get<Payment>(`${this.apiUrl}/${paymentId}`);
  }
}

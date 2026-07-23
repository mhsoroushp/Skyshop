import { ChangeDetectorRef, Component, OnDestroy, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatListModule } from '@angular/material/list';
import { Router, RouterModule } from '@angular/router';
import { Order } from '../../../core/models/order.model';
import { OrderService } from '../../../core/services/order.service';
import { Subscription } from 'rxjs/internal/Subscription';

@Component({
  selector: 'app-payment-confirmation',
  standalone: true,
  imports: [
    CommonModule,
    MatButtonModule,
    MatCardModule,
    MatIconModule,
    MatListModule,
    RouterModule
  ],
  templateUrl: './payment-confirmation.component.html',
  styleUrls: ['./payment-confirmation.component.scss']
})
export class PaymentConfirmationComponent implements OnInit, OnDestroy {

  private destroyOrderIdWithSuccessfulPayment?: Subscription;
  orderService = inject(OrderService);


  orderId = '';
  order?: Order;
  loading = false;
  error = '';
  realtimeStatusMessage = 'Waiting for payment confirmation...';
  realtimePaymentStatus = 'processing';
  realtimeOrderStatus = 'pending';
  realtimeUpdatedAt = '';
  hasRealtimeUpdate = false;
  paymentCompleted = false;

  constructor(
    private router: Router,
    private cdr: ChangeDetectorRef
  ) {}


  ngOnInit(): void {
    this.destroyOrderIdWithSuccessfulPayment = this.orderService.orderIdWithSuccessfulPayment$.subscribe((orderId) => {
      // TODO: log the orderId for debugging
      // console.log('[PaymentConfirmation] Received orderId with successful payment:', orderId);
      this.orderId = orderId;

      this.orderService.getOrderById(orderId).subscribe({
        next: (order) => {
          this.order = order;
          this.realtimeStatusMessage = 'Payment confirmed successfully!';
          this.realtimePaymentStatus = 'succeeded';
          this.realtimeOrderStatus = order.status;
          this.realtimeUpdatedAt = new Date().toLocaleString();
          this.hasRealtimeUpdate = true;
          this.paymentCompleted = true;
          this.cdr.detectChanges();
        },
        error: (err) => {
          // TODO: log the error for debugging
          console.error('[PaymentConfirmation] Error fetching order details:', err);
          this.error = 'Failed to fetch order details.';
        }
      }); 

    });



  }
  
  ngOnDestroy(): void {
    this.destroyOrderIdWithSuccessfulPayment?.unsubscribe();

  }

  continueShopping(): void {
    this.router.navigate(['/dashboard']);
  }

  viewOrders(): void {
    this.router.navigate(['/orders']);
  }
}
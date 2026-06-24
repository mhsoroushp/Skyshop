import { ChangeDetectorRef, Component, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatListModule } from '@angular/material/list';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { OrderService } from '../../core/services/order.service';
import { Order } from '../../core/models/order.model';
import { catchError, finalize, of, Subject, takeUntil, timeout } from 'rxjs';
import { PaymentStatusRealtimeService } from '../../core/services/payment-status-realtime.service';

@Component({
  selector: 'app-order-confirmation',
  standalone: true,
  imports: [
    CommonModule,
    MatButtonModule,
    MatCardModule,
    MatIconModule,
    MatListModule,
    RouterModule
  ],
  templateUrl: './order-confirmation.component.html',
  styleUrls: ['./order-confirmation.component.scss']
})
export class OrderConfirmationComponent implements OnInit, OnDestroy {
  order?: Order;
  loading = true;
  error = '';
  realtimeStatusMessage = '';
  private currentOrderId = '';
  private readonly destroy$ = new Subject<void>();

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private orderService: OrderService,
    private paymentStatusRealtimeService: PaymentStatusRealtimeService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    const orderId = this.route.snapshot.paramMap.get('orderId');

    if (!orderId) {
      this.error = 'Order ID is missing';
      this.loading = false;
      this.cdr.detectChanges();
      return;
    }

    this.currentOrderId = orderId;
    this.initializeRealtimeUpdates(orderId);
    this.loadOrder(orderId);
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();

    if (this.currentOrderId) {
      void this.paymentStatusRealtimeService.leaveOrderGroup(this.currentOrderId);
    }
  }

  private loadOrder(orderId: string): void {
    this.loading = true;
    this.error = '';
    this.order = undefined;
    this.cdr.detectChanges();

    this.orderService.getOrder(orderId)
      .pipe(
        timeout(10000),
        catchError((err) => {
          this.error = 'Failed to load order: ' + (err.error?.message || err.message || 'Request timed out');
          return of(null);
        }),
        finalize(() => {
          this.loading = false;
          this.cdr.detectChanges();
        })
      )
      .subscribe((order) => {
        if (order) {
          this.order = order;
        }

        this.cdr.detectChanges();
      });
  }

  continueShopping(): void {
    this.router.navigate(['/shop']);
  }

  viewOrders(): void {
    this.router.navigate(['/orders']);
  }

  private initializeRealtimeUpdates(orderId: string): void {
    this.paymentStatusRealtimeService.statusUpdates$
      .pipe(takeUntil(this.destroy$))
      .subscribe((update) => {
        // orderId from backend may be a Guid object or string — compare case-insensitively.
        const incomingId = typeof update.orderId === 'string'
          ? update.orderId
          : String(update.orderId);

        if (incomingId.toLowerCase() !== orderId.toLowerCase()) {
          return;
        }

        this.realtimeStatusMessage = `Live update: payment ${update.paymentStatus.toLowerCase()}`;
        this.loadOrder(orderId);
      });

    void this.paymentStatusRealtimeService.joinOrderGroup(orderId)
      .catch((err) => {
        this.realtimeStatusMessage = `Realtime updates unavailable: ${err?.message ?? 'Connection failed'}`;
        this.cdr.detectChanges();
      });
  }
}

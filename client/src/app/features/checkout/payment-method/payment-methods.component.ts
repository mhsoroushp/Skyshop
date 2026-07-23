import { AfterViewInit, Component, OnInit, OnDestroy, inject, effect, EventEmitter, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subject } from 'rxjs';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { Router } from '@angular/router';
import {
  Stripe,
  StripeCardCvcElement,
  StripeCardExpiryElement,
  StripeCardNumberElement,
  StripeElements,
  loadStripe
} from '@stripe/stripe-js';
import { PaymentService } from '../../../core/services/payment.service';
import { environment } from '../../../../environments/environment';
import { ShowBasketService } from '../../../core/services/show-basket.service';
import { PaymentStatusRealtimeService } from '../../../core/services/payment-status-realtime.service';
import { takeUntil } from 'rxjs';
import { OrderService } from '../../../core/services/order.service';

@Component({
  selector: 'app-payment-methods',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatCardModule,
    MatIconModule,
    MatProgressSpinnerModule,
  ],
  templateUrl: './payment-methods.component.html',
  styleUrls: ['./payment-methods.component.css']
})
export class PaymentMethodsComponent implements OnInit, AfterViewInit, OnDestroy {
  @Output() IsPaymentCompleted = new EventEmitter<boolean>(false);
  showBasketService = inject(ShowBasketService);
  orderService = inject(OrderService);
  private readonly watchOrderForRealtimeJoin = effect(() => {
    const currentOrder = this.orderService.currentOrder$();
    const orderId = currentOrder?.id;

    if (orderId && orderId !== this.joinedOrderId) {
      void this.joinCurrentOrderGroup();
    }
  });

  paymentForm!: FormGroup;
  private joinedOrderId = '';
  private readonly destroy$ = new Subject<void>();
  loading = false;
  error = '';
  success = false;
  stripe: Stripe | null = null;
  elements: StripeElements | null = null;
  cardNumberElement: StripeCardNumberElement | null = null;
  cardExpiryElement: StripeCardExpiryElement | null = null;
  cardCvcElement: StripeCardCvcElement | null = null;
  private cardNumberMounted = false;
  private cardExpiryMounted = false;
  private cardCvcMounted = false;
  private stripeElementsDisabled = false;
  private mountRetryTimer: ReturnType<typeof setInterval> | null = null;
  private mountRetryCount = 0;

  get canSubmit(): boolean {
    return !this.loading && !this.success && this.paymentForm.valid;
  }

  constructor(
    private router: Router,
    private formBuilder: FormBuilder,
    private paymentService: PaymentService,
    private paymentStatusRealtimeService: PaymentStatusRealtimeService
  ) {
    this.initializeForm();
  }

  ngOnInit(): void {
    void this.initializeStripe();
  
    this.paymentStatusRealtimeService.statusUpdates$
      .pipe(takeUntil(this.destroy$))
      .subscribe(async (order) => {
        if(order.paymentStatus === 'Succeeded') {
          this.orderService.setOrderIdWithSuccessfulPayment(order.orderId);
          this.IsPaymentCompleted.emit(true);
          if(!this.success) await this.afterSuccessfulPayment();
        }
      });
  }

  ngAfterViewInit(): void {
    this.startMountRetry();
  }

  ngOnDestroy(): void {
    if (this.mountRetryTimer) {
      clearInterval(this.mountRetryTimer);
      this.mountRetryTimer = null;
    }

    this.cardNumberElement?.destroy();
    this.cardExpiryElement?.destroy();
    this.cardCvcElement?.destroy();

    this.destroy$.next();
    this.destroy$.complete();

    if (this.joinedOrderId) {
      void this.paymentStatusRealtimeService.leaveOrderGroup(this.joinedOrderId);
    }
  }

  public isCurrentOrderAvailable(): boolean {
    return this.orderService.currentOrder$() !== null;
  }

  private initializeForm(): void {
    this.paymentForm = this.formBuilder.group({
      cardholderName: [{ value: '', disabled: false }, [Validators.required]]
    });
  }

  private async initializeStripe(): Promise<void> {
    try {
      this.stripe = await loadStripe(environment.stripePublicKey);

      if (!this.stripe) {
        this.error = 'Stripe failed to initialize. Check browser console for details.';
        return;
      }

      this.elements = this.stripe.elements();
      this.cardNumberElement = this.elements.create('cardNumber');
      this.cardExpiryElement = this.elements.create('cardExpiry');
      this.cardCvcElement = this.elements.create('cardCvc');

      this.startMountRetry();
    } catch (err) {
      this.error = `Stripe error: ${err instanceof Error ? err.message : 'Unknown error'}`;
    }
  }

  private startMountRetry(): void {
    this.tryMountStripeElements();

    if (this.mountRetryTimer) {
      return;
    }

    this.mountRetryCount = 0;
    this.mountRetryTimer = setInterval(() => {
      this.mountRetryCount += 1;
      this.tryMountStripeElements();

      if ((this.cardNumberMounted && this.cardExpiryMounted && this.cardCvcMounted) || this.mountRetryCount >= 40) {
        if (this.mountRetryTimer) {
          clearInterval(this.mountRetryTimer);
          this.mountRetryTimer = null;
        }
      }
    }, 150);
  }

  private tryMountStripeElements(): void {
    const cardNumberContainer = document.getElementById('card-number-element');
    const cardExpiryContainer = document.getElementById('card-expiry-element');
    const cardCvcContainer = document.getElementById('card-cvc-element');

    if (
      !this.cardNumberMounted &&
      cardNumberContainer &&
      this.cardNumberElement &&
      !cardNumberContainer.querySelector('iframe')
    ) {
      this.cardNumberElement.mount('#card-number-element');
      this.cardNumberMounted = true;
    }

    if (
      !this.cardExpiryMounted &&
      cardExpiryContainer &&
      this.cardExpiryElement &&
      !cardExpiryContainer.querySelector('iframe')
    ) {
      this.cardExpiryElement.mount('#card-expiry-element');
      this.cardExpiryMounted = true;
    }

    if (
      !this.cardCvcMounted &&
      cardCvcContainer &&
      this.cardCvcElement &&
      !cardCvcContainer.querySelector('iframe')
    ) {
      this.cardCvcElement.mount('#card-cvc-element');
      this.cardCvcMounted = true;
    }
  }

  private setStripeElementsDisabled(disabled: boolean): void {
    if (this.stripeElementsDisabled === disabled) {
      return;
    }

    if (disabled) {
      this.blurStripeElements();
    }

    this.cardNumberElement?.update({ disabled });
    this.cardExpiryElement?.update({ disabled });
    this.cardCvcElement?.update({ disabled });
    this.stripeElementsDisabled = disabled;
  }

  private blurStripeElements(): void {
    // Prevent accessibility warnings caused by disabling a focused Stripe input.
    this.cardNumberElement?.blur();
    this.cardExpiryElement?.blur();
    this.cardCvcElement?.blur();

    const activeElement = document.activeElement as HTMLElement | null;
    activeElement?.blur();
  }

  private updateFormDisabledState(disabled: boolean): void {
    const controls = this.paymentForm.controls;
    Object.keys(controls).forEach((key) => {
      if (disabled) {
        controls[key].disable();
      } else {
        controls[key].enable();
      }
    });
  }

  private createPaymentIntent(orderId: string): Promise<string> {
    return new Promise((resolve, reject) => {
      this.paymentService.createPaymentIntent({ orderId }).subscribe({
        next: (response) => {
          resolve(response.clientSecret);
        },
        error: (err) => {
          this.error = 'Failed to initialize payment: ' + (err.error?.message || err.message);
          reject(err);
        }
      });
    });
  }

  async onSubmit(): Promise<void> {
    if (this.paymentForm.invalid) {
      this.error = 'Please fill in all required fields';
      return;
    }

    if (!this.stripe || !this.cardNumberElement) {
      this.error = 'Stripe card form is not ready yet. Please try again.';
      return;
    }

    if (!this.isCurrentOrderAvailable()) {
      this.error = 'Create an order first before processing payment.';
      return;
    }

    var order = this.orderService.currentOrder$()!;
    var clientSecret = await this.createPaymentIntent(order.id);

    if (!clientSecret) {
      this.error = 'Payment is not initialized yet. Please try again.';
      return;
    }

    this.loading = true;
    this.error = '';
    this.updateFormDisabledState(true);
    this.setStripeElementsDisabled(true);

    const { cardholderName } = this.paymentForm.value;

    this.stripe
      .confirmCardPayment(clientSecret, {
        payment_method: {
          card: this.cardNumberElement,
          billing_details: {
            name: cardholderName
          }
        }
      })
      .then((result: any) => {
        // Handle the successful payment confirmation
        if (!result.error && result.paymentIntent 
          && (result.paymentIntent.status === 'succeeded' || result.paymentIntent.status === 'processing')) {
          this.confirmPayment(result.paymentIntent.id);
          return;
        }

        // Handle the failed payment confirmation
        this.loading = false;
        this.updateFormDisabledState(false);
        this.setStripeElementsDisabled(false);

        if (result.error) {
          this.error = result.error.message || 'Payment confirmation failed';
          return;
        }

        const paymentIntent = result?.paymentIntent;
        if (!paymentIntent) {
          this.error = 'Stripe did not return a payment result.';
          return;
        }

        if (paymentIntent.status === 'requires_payment_method') {
          this.error = 'Payment method was rejected. Please verify card details or use another test card.';
          return;
        }

        this.error = `Unexpected payment status: ${paymentIntent.status}`;
      })
      .catch((err: unknown) => {
        const message = err instanceof Error ? err.message : 'Unexpected payment error';
        this.error = `Payment failed: ${message}`;
        this.loading = false;
        this.updateFormDisabledState(false);
        this.setStripeElementsDisabled(false);
      });
  }

  private confirmPayment(paymentIntentId: string): void {
    void this.joinCurrentOrderGroup()
      .catch((err) => {
        // TODO: log the error
        console.error('[PaymentConfirmation] Failed to ensure SignalR group join before confirm:', err);
      })
      .finally(() => {
        this.paymentService
          .confirmPayment({
            orderId: this.orderService.currentOrder$()!.id,
            paymentMethodId: paymentIntentId
          })
          .subscribe({
            next: async () => {
              if(!this.success) await this.afterSuccessfulPayment();
            },
            error: (err) => {
              this.error = 'Payment failed: ' + (err.error?.message || err.message);
              this.loading = false;
              this.updateFormDisabledState(false);
              this.setStripeElementsDisabled(false);
            }
          });
      });
  }

  private async afterSuccessfulPayment(): Promise<void> {
    this.success = true;
    this.loading = false;
    this.updateFormDisabledState(false);
    this.setStripeElementsDisabled(false);
    await this.clearBasketAfterPayment();
    this.orderService.clearCurrentOrderAfterPaymentSuccess();
  }

  private clearBasketAfterPayment(): Promise<void> {
    return new Promise((resolve, reject) => {
      this.showBasketService.clearBasket().subscribe({
        next: () => {
          resolve();
        },
        error: (err) => {
          console.error('Failed to clear basket after payment:', err);
          reject(err);
        }
      });
    });
  }

  private async joinCurrentOrderGroup(): Promise<void> {
    const currentOrder = this.orderService.currentOrder$();
    const orderId = currentOrder?.id;
    if (!orderId || orderId === this.joinedOrderId) {
      return;
    }

    try {
      await this.paymentStatusRealtimeService.joinOrderGroup(orderId);
      this.joinedOrderId = orderId;
    } catch (err) {
      // TODO: log the error
      console.error('[PaymentConfirmation] Failed to join SignalR group for order:', orderId, err);
      throw err;
    }
  }

  get cardholderName() {
    return this.paymentForm.get('cardholderName');
  }
}

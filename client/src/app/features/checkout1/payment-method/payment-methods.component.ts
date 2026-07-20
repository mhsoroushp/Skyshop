import { AfterViewInit, Component, Input, OnChanges, OnInit, OnDestroy, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
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
import { Order } from '../../../core/models/order.model';
import { environment } from '../../../../environments/environment';

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
export class PaymentMethodsComponent implements OnInit, OnChanges, AfterViewInit, OnDestroy {
  @Input() orderFromCheckout?: Order;

  paymentForm!: FormGroup;
  paymentInitializing = false;
  processing = false;
  error = '';
  success = false;
  orderId = '';
  order?: Order;
  stripe: Stripe | null = null;
  elements: StripeElements | null = null;
  cardNumberElement: StripeCardNumberElement | null = null;
  cardExpiryElement: StripeCardExpiryElement | null = null;
  cardCvcElement: StripeCardCvcElement | null = null;
  private cardNumberMounted = false;
  private cardExpiryMounted = false;
  private cardCvcMounted = false;
  private mountRetryTimer: ReturnType<typeof setInterval> | null = null;
  private mountRetryCount = 0;
  clientSecret = '';

  get canSubmit(): boolean {
    return !!this.clientSecret && !this.processing && !this.paymentInitializing;
  }

  constructor(
    private router: Router,
    private formBuilder: FormBuilder,
    private paymentService: PaymentService
  ) {
    this.initializeForm();
  }

  ngOnInit(): void {
    void this.initializeStripe();

    if (this.orderFromCheckout?.id) {
      this.applyOrderFromParent(this.orderFromCheckout);
    }
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
  }

  ngOnChanges(changes: SimpleChanges): void {
    const orderChange = changes['orderFromCheckout'];
    if (orderChange?.currentValue?.id) {
      this.applyOrderFromParent(orderChange.currentValue);
    }
  }

  private initializeForm(): void {
    this.paymentForm = this.formBuilder.group({
      cardholderName: [{ value: '', disabled: false }, [Validators.required]]
    });
  }

  private applyOrderFromParent(order: Order): void {
    this.order = order;
    this.orderId = order.id;

    if (!this.clientSecret) {
      this.createPaymentIntent();
    }
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

    this.setStripeElementsDisabled(this.paymentInitializing || this.processing);
  }

  private setStripeElementsDisabled(disabled: boolean): void {
    this.cardNumberElement?.update({ disabled });
    this.cardExpiryElement?.update({ disabled });
    this.cardCvcElement?.update({ disabled });
  }

  private createPaymentIntent(): void {
    if (!this.orderId) {
      return;
    }

    this.paymentInitializing = true;
    this.updateFormDisabledState(true);
    this.setStripeElementsDisabled(true);

    this.paymentService.createPaymentIntent({ orderId: this.orderId }).subscribe({
      next: (response) => {
        this.clientSecret = response.clientSecret;
        this.paymentInitializing = false;
        this.updateFormDisabledState(false);
        this.setStripeElementsDisabled(false);
      },
      error: (err) => {
        this.error = 'Failed to initialize payment: ' + (err.error?.message || err.message);
        this.paymentInitializing = false;
        this.updateFormDisabledState(false);
        this.setStripeElementsDisabled(false);
      }
    });
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

  onSubmit(): void {
    if (this.paymentForm.invalid) {
      this.error = 'Please fill in all required fields';
      return;
    }

    if (!this.orderId) {
      this.error = 'Create an order first before processing payment.';
      return;
    }

    if (!this.clientSecret) {
      this.error = 'Payment is not initialized yet. Please try again.';
      return;
    }

    if (!this.stripe || !this.cardNumberElement) {
      this.error = 'Stripe card form is not ready yet. Please try again.';
      return;
    }

    this.processing = true;
    this.error = '';
    this.updateFormDisabledState(true);
    this.setStripeElementsDisabled(true);

    const { cardholderName } = this.paymentForm.value;

    this.stripe
      .confirmCardPayment(this.clientSecret, {
        payment_method: {
          card: this.cardNumberElement,
          billing_details: {
            name: cardholderName
          }
        }
      })
      .then((result: any) => {
        if (result.error) {
          this.error = result.error.message || 'Payment confirmation failed';
          this.processing = false;
          this.updateFormDisabledState(false);
          this.setStripeElementsDisabled(false);
          return;
        }

        const paymentIntent = result.paymentIntent;
        if (!paymentIntent) {
          this.error = 'Stripe did not return a payment result.';
          this.processing = false;
          this.updateFormDisabledState(false);
          this.setStripeElementsDisabled(false);
          return;
        }

        if (paymentIntent.status === 'succeeded' || paymentIntent.status === 'processing') {
          this.confirmPayment(paymentIntent.id);
          return;
        }

        if (paymentIntent.status === 'requires_payment_method') {
          this.error = 'Payment method was rejected. Please verify card details or use another test card.';
          this.processing = false;
          this.updateFormDisabledState(false);
          this.setStripeElementsDisabled(false);
          return;
        }

        this.error = `Unexpected payment status: ${paymentIntent.status}`;
        this.processing = false;
        this.updateFormDisabledState(false);
        this.setStripeElementsDisabled(false);
      })
      .catch((err: unknown) => {
        const message = err instanceof Error ? err.message : 'Unexpected payment error';
        this.error = `Payment failed: ${message}`;
        this.processing = false;
        this.updateFormDisabledState(false);
        this.setStripeElementsDisabled(false);
      });
  }

  private confirmPayment(paymentIntentId: string): void {
    this.paymentService
      .confirmPayment({
        orderId: this.orderId,
        paymentMethodId: paymentIntentId
      })
      .subscribe({
        next: () => {
          this.success = true;
          this.processing = false;
          this.updateFormDisabledState(false);
          this.setStripeElementsDisabled(false);
          setTimeout(() => {
            this.router.navigate(['/order-confirmation', this.orderId]);
          }, 2000);
        },
        error: (err) => {
          this.error = 'Payment failed: ' + (err.error?.message || err.message);
          this.processing = false;
          this.updateFormDisabledState(false);
          this.setStripeElementsDisabled(false);
        }
      });
  }

  get cardholderName() {
    return this.paymentForm.get('cardholderName');
  }
}

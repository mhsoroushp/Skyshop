import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { ActivatedRoute, Router } from '@angular/router';
import { Stripe, StripeCardElement, StripeElements, loadStripe } from '@stripe/stripe-js';
import { PaymentService } from '../../core/services/payment.service';
import { OrderService } from '../../core/services/order.service';
import { Order } from '../../core/models/order.model';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-payment',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatCardModule,
    MatIconModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './payment.component.html',
  styleUrls: ['./payment.component.scss']
})
export class PaymentComponent implements OnInit {
  paymentForm!: FormGroup;
  loading = false;
  paymentInitializing = false;
  processing = false;
  error = '';
  success = false;
  orderId: string = '';
  order?: Order;
  stripe: Stripe | null = null;
  elements: StripeElements | null = null;
  cardElement: StripeCardElement | null = null;
  clientSecret: string = '';

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private formBuilder: FormBuilder,
    private paymentService: PaymentService,
    private orderService: OrderService
  ) {
    this.initializeForm();
  }

  ngOnInit(): void {
    this.orderId = this.route.snapshot.paramMap.get('orderId') || '';
    this.loadOrder();
    void this.initializeStripe();
  }

  private initializeForm(): void {
    this.paymentForm = this.formBuilder.group({
      cardholderName: [{value: '', disabled: false}, [Validators.required]],
      email: [{value: '', disabled: false}, [Validators.required, Validators.email]],
      postalCode: [{value: '', disabled: false}, [Validators.required]]
    });
  }

  private loadOrder(): void {
    if (!this.orderId) {
      this.error = 'Order ID is missing';
      return;
    }

    this.orderService.getOrder(this.orderId).subscribe({
      next: (order) => {
        this.order = order;
        this.createPaymentIntent();
      },
      error: (err) => {
        this.error = 'Failed to load order: ' + (err.error?.message || err.message);
      }
    });
  }

  private async initializeStripe(): Promise<void> {
    try {
      console.log('Loading Stripe with key:', environment.stripePublicKey);
      this.stripe = await loadStripe(environment.stripePublicKey);

      if (!this.stripe) {
        console.error('Stripe initialization failed - loadStripe returned null');
        this.error = 'Stripe failed to initialize. Check browser console for details.';
        return;
      }

      console.log('Stripe initialized successfully');
      this.elements = this.stripe.elements();
      this.cardElement = this.elements.create('card');
      
      setTimeout(() => {
        const cardContainer = document.getElementById('card-element');
        if (cardContainer && this.cardElement && !cardContainer.querySelector('iframe')) {
          this.cardElement.mount('#card-element');
          console.log('Card element mounted successfully');
        }
      }, 100);
    } catch (err) {
      console.error('Stripe initialization error:', err);
      this.error = `Stripe error: ${err instanceof Error ? err.message : 'Unknown error'}`;
    }
  }

  private createPaymentIntent(): void {
    if (!this.orderId) return;

    this.paymentInitializing = true;
    this.updateFormDisabledState(true);

    this.paymentService.createPaymentIntent({ orderId: this.orderId }).subscribe({
      next: (response) => {
        this.clientSecret = response.clientSecret;
        this.paymentInitializing = false;
        this.updateFormDisabledState(false);
      },
      error: (err) => {
        this.error = 'Failed to initialize payment: ' + (err.error?.message || err.message);
        this.paymentInitializing = false;
        this.updateFormDisabledState(false);
      }
    });
  }

  private updateFormDisabledState(disabled: boolean): void {
    const controls = this.paymentForm.controls;
    Object.keys(controls).forEach(key => {
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

    if (!this.clientSecret) {
      this.error = 'Payment is not initialized yet. Please try again.';
      return;
    }

    if (!this.stripe || !this.cardElement) {
      this.error = 'Stripe card form is not ready yet. Please try again.';
      return;
    }

    this.processing = true;
    this.error = '';
    this.updateFormDisabledState(true);

    const { cardholderName, email, postalCode } = this.paymentForm.value;

    this.stripe
      .confirmCardPayment(this.clientSecret, {
        payment_method: {
          card: this.cardElement,
          billing_details: {
            name: cardholderName,
            email: email,
            address: {
              postal_code: postalCode
            }
          }
        }
      })
      .then((result: any) => {
        if (result.error) {
          this.error = result.error.message || 'Payment confirmation failed';
          this.processing = false;
          this.updateFormDisabledState(false);
          return;
        }

        const paymentIntent = result.paymentIntent;
        if (!paymentIntent) {
          this.error = 'Stripe did not return a payment result.';
          this.processing = false;
          this.updateFormDisabledState(false);
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
          return;
        }

        this.error = `Unexpected payment status: ${paymentIntent.status}`;
        this.processing = false;
        this.updateFormDisabledState(false);
      })
      .catch((err: unknown) => {
        const message = err instanceof Error ? err.message : 'Unexpected payment error';
        this.error = `Payment failed: ${message}`;
        this.processing = false;
        this.updateFormDisabledState(false);
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
          // Redirect to confirmation page
          setTimeout(() => {
            this.router.navigate(['/order-confirmation', this.orderId]);
          }, 2000);
        },
        error: (err) => {
          this.error = 'Payment failed: ' + (err.error?.message || err.message);
          this.processing = false;
          this.updateFormDisabledState(false);
        }
      });
  }

  get cardholderName() {
    return this.paymentForm.get('cardholderName');
  }

  get email() {
    return this.paymentForm.get('email');
  }

  get postalCode() {
    return this.paymentForm.get('postalCode');
  }
}

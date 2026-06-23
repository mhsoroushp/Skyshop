import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatStepperModule } from '@angular/material/stepper';
import { Router } from '@angular/router';
import { OrderService } from '../../core/services/order.service';
import { Order, CreateOrderRequest } from '../../core/models/order.model';

@Component({
  selector: 'app-checkout',
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
    MatStepperModule
  ],
  templateUrl: './checkout.component.html',
  styleUrls: ['./checkout.component.scss']
})
export class CheckoutComponent implements OnInit {
  checkoutForm!: FormGroup;
  loading = false;
  error = '';
  currentOrder?: Order;

  constructor(
    private formBuilder: FormBuilder,
    private orderService: OrderService,
    private router: Router
  ) {
    this.initializeForm();
  }

  ngOnInit(): void {}

  private initializeForm(): void {
    this.checkoutForm = this.formBuilder.group({
      customerName: ['', [Validators.required]],
      customerEmail: ['', [Validators.required, Validators.email]],
      shippingAddress: ['', [Validators.required]],
      city: ['', [Validators.required]],
      zipCode: ['', [Validators.required]],
      country: ['', [Validators.required]]
    });
  }

  onCreateOrder(): void {
    if (this.checkoutForm.invalid) {
      this.error = 'Please fill in all required fields correctly';
      return;
    }

    this.loading = true;
    this.error = '';

    const request: CreateOrderRequest = {
      customerName: this.checkoutForm.get('customerName')?.value,
      customerEmail: this.checkoutForm.get('customerEmail')?.value
    };

    this.orderService.createOrder(request).subscribe({
      next: (response) => {
        this.currentOrder = response.order;
        this.loading = false;
        // Navigate to payment
        this.router.navigate(['/payment', this.currentOrder?.id]);
      },
      error: (err) => {
        this.loading = false;
        this.error = err.error?.message || 'Failed to create order';
      }
    });
  }

  get customerName() {
    return this.checkoutForm.get('customerName');
  }

  get customerEmail() {
    return this.checkoutForm.get('customerEmail');
  }

  get shippingAddress() {
    return this.checkoutForm.get('shippingAddress');
  }

  get city() {
    return this.checkoutForm.get('city');
  }

  get zipCode() {
    return this.checkoutForm.get('zipCode');
  }

  get country() {
    return this.checkoutForm.get('country');
  }
}

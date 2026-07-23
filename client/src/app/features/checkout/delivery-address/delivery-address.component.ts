import { Component, OnInit, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { OrderService } from '../../../core/services/order.service';
import { Router } from '@angular/router';
import { CreateOrderRequest } from '../../../core/models/order.model';
import { MatIconModule } from '@angular/material/icon';
import { ShowBasketService } from '../../../core/services/show-basket.service';

@Component({
  selector: 'app-delivery-address',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule
  ],
  templateUrl: './delivery-address.component.html',
  styleUrls: ['./delivery-address.component.css']
})
export class DeliveryAddressComponent implements OnInit {
  @Output() IsOrderCreated = new EventEmitter<boolean>(false);
  deliveryForm!: FormGroup;
  success = false;
  loading = false;
  error = '';


  constructor(
    private fb: FormBuilder,
    private orderService: OrderService,
    private showBasketService: ShowBasketService,

  ){
    this.initializeForm();
  }

  ngOnInit(): void {}

  private initializeForm(): void {
    this.deliveryForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      firstName: ['', [Validators.required]],
      lastName: ['', [Validators.required]],
      phoneNumber: ['', [Validators.required]],
      shippingCountry: ['', [Validators.required]],
      street: ['', [Validators.required]],
      houseNumber: ['', [Validators.required]],
      city: ['', [Validators.required]],
      postalCode: ['', [Validators.required]],
    });
  }

  get f() {
    return this.deliveryForm.controls;
  }

  canSubmit(): boolean {
    return this.deliveryForm.valid && !this.loading && !this.success && this.showBasketService.basketItemCount() > 0;
  }


  onCreateOrder(): void {
    if (this.deliveryForm.invalid) {
      this.deliveryForm.markAllAsTouched();
      return;
    }

    this.loading = true;
    this.error = '';

    const request: CreateOrderRequest = {
      customerName: this.deliveryForm.get('firstName')?.value + ' ' + this.deliveryForm.get('lastName')?.value,
      customerEmail: this.deliveryForm.get('email')?.value
    };

    this.orderService.createOrder(request).subscribe({
      next: (order) => {
        this.loading = false;
        this.success = true;
        this.IsOrderCreated.emit(true);
      },
      error: (err) => {
        this.loading = false;
        this.success = false;
        this.error = err.error?.message || 'Failed to create order';
        this.IsOrderCreated.emit(false);
      }
    });
  }
}
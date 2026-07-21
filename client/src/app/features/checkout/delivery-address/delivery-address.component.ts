import { Component, OnInit, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { Order } from '../../../core/models/order.model';
import { OrderService } from '../../../core/services/order.service';
import { Router } from '@angular/router';
import { CreateOrderRequest } from '../../../core/models/order.model';

@Component({
  selector: 'app-delivery-address',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
  ],
  templateUrl: './delivery-address.component.html',
  styleUrls: ['./delivery-address.component.css']
})
export class DeliveryAddressComponent implements OnInit {
  @Output() currentOrder = new EventEmitter<Order>();
  @Output() IsOrderCreated = new EventEmitter<boolean>(false);

  deliveryForm!: FormGroup;
  loading = false;
  error = '';
  //currentOrder?: Order;

  constructor(
    private fb: FormBuilder,
    private orderService: OrderService,
    private router: Router
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
      next: (response) => {
        //this.currentOrder = response.order;
        this.loading = false;
        // Navigate to payment
        //this.router.navigate(['/payment', this.currentOrder?.id]);
        this.currentOrder.emit(response?.Order ?? response?.order);
        this.IsOrderCreated.emit(true);
      },
      error: (err) => {
        this.loading = false;
        this.error = err.error?.message || 'Failed to create order';
        this.IsOrderCreated.emit(false);
      }
    });

    

  }
}
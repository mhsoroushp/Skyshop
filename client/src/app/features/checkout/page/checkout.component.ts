import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { DeliveryAddressComponent } from '../delivery-address/delivery-address.component';
import { Order } from '../../../core/models/order.model';
import { ShowBasketListComponent } from '../../basket/components/show/show-basket-list.component';
import { MatExpansionModule } from '@angular/material/expansion';
import { PaymentMethodsComponent } from '../payment-method/payment-methods.component';

interface TabProccessed {
  IsdeliveryAddressCompleted: boolean;
  IsPaymentMethodCompleted: boolean;
}

@Component({
  selector: 'app-checkout',
  standalone: true,
  imports: [
    CommonModule,
    ShowBasketListComponent,
    MatExpansionModule,
    DeliveryAddressComponent,
    PaymentMethodsComponent,
  ],
  templateUrl: './checkout.component.html',
  styleUrls: ['./checkout.component.css']
})
export class CheckoutComponent implements OnInit {
  order?: Order;
  tabProccessed: TabProccessed = {
    IsdeliveryAddressCompleted: false,
    IsPaymentMethodCompleted: false,
  };
  

  ngOnInit(): void {
  }

  onCurrentOrderChanged(order: Order) {
    this.order = order;
    console.log('Order received from child:', order);
    // use it: call API, update form, navigate, etc.
  }

  onDeliveryChanged(isCompleted: boolean) {
    this.tabProccessed.IsdeliveryAddressCompleted = isCompleted;
  }
}
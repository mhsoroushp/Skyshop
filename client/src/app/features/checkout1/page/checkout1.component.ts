import { Component, OnInit } from '@angular/core';
import { DeliveryAddressComponent } from '../delivery-address/delivery-address.component';
import { Order } from '../../../core/models/order.model';
import { ShowBasketListComponent } from '../../basket/components/show/show-basket-list.component';
import { MatExpansionModule } from '@angular/material/expansion';
import { PaymentMethodsComponent } from '../payment-method/payment-methods.component';
import { PaymentSummaryComponent } from '../payment-summary/payment-summary.component';

interface TabProccessed {
  IsdeliveryAddressCompleted: boolean;
  IsPaymentMethodCompleted: boolean;
}

@Component({
  selector: 'app-checkout1',
  standalone: true,
  imports: [
    ShowBasketListComponent,
    MatExpansionModule,
    DeliveryAddressComponent,
    PaymentMethodsComponent,
    PaymentSummaryComponent,
  ],
  templateUrl: './checkout1.component.html',
  styleUrls: ['./checkout1.component.css']
})
export class Checkout1Component implements OnInit {
  order?: Order;
  tabProccessed: TabProccessed = {
    IsdeliveryAddressCompleted: false,
    IsPaymentMethodCompleted: false,
  };
  

  



  ngOnInit(): void {
    console.log("Checkout1Component initialized");
  }

  onCurrentOrderChanged(order: Order) {
    this.order = order;
    console.log('Order received from child:', order);
    // use it: call API, update form, navigate, etc.
  }

  onDeliveryChanged(isCompleted: boolean) {
    this.tabProccessed.IsdeliveryAddressCompleted = isCompleted;
    console.log('Delivery address completion status:', isCompleted);
  }
}
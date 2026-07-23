import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { DeliveryAddressComponent } from '../delivery-address/delivery-address.component';
import { ShowBasketListComponent } from '../../basket/components/show/show-basket-list.component';
import { MatExpansionModule } from '@angular/material/expansion';
import { PaymentMethodsComponent } from '../payment-method/payment-methods.component';
import { PaymentConfirmationComponent } from '../confirmation/payment-confirmation.component';

interface TabProccessed {
  IsAddressTabExpand: boolean;
  IsPaymentTabExpand: boolean;
  IsConfirmationTabExpand: boolean;
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
    PaymentConfirmationComponent
  ],
  templateUrl: './checkout.component.html',
  styleUrls: ['./checkout.component.css']
})
export class CheckoutComponent {
  tabProccessed: TabProccessed = {
    IsAddressTabExpand: true,
    IsPaymentTabExpand: false,
    IsConfirmationTabExpand: false,
  };


  onOrderSuccessCreation(isCompleted: boolean) {
    this.tabProccessed.IsAddressTabExpand = false;
    this.tabProccessed.IsPaymentTabExpand = true; // True
    this.tabProccessed.IsConfirmationTabExpand = false;
  }

  onPaymentSuccess(isCompleted: boolean) {
    this.tabProccessed.IsAddressTabExpand = false;
    this.tabProccessed.IsPaymentTabExpand = false;
    this.tabProccessed.IsConfirmationTabExpand = true;
  }
}
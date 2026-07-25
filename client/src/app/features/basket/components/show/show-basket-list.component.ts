import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatIcon } from '@angular/material/icon';
import { MatButton } from '@angular/material/button';
import { CurrencyPipe } from '@angular/common';
import { ShowBasketService } from '../../../../core/services/show-basket.service';

export interface BasketViewItem {
  id: number;
  title: string;
  description: string;
  price: number;
  quantity: number;
  image: string;
}

@Component({
  selector: 'app-show-basket-list',
  standalone: true,
  imports: [
    MatIcon,
    MatButton,
    RouterLink,
    CurrencyPipe
  ],
  templateUrl: './show-basket-list.component.html',
  styleUrls: ['./show-basket-list.component.css']
})
export class ShowBasketListComponent implements OnInit {
  showBasketService = inject(ShowBasketService);
  
  // TODO: deltet
  // Signal for basket items
  basketItems = signal<BasketViewItem[]>([]);
  
  // Computed signal for total price
  totalPrice = signal<number>(0);
  
  constructor() {
  }

  ngOnInit(): void {
  }
  
  
  /**
   * Update total price computed from basket items
   */
  updateTotalPrice(): void {
    const items = this.basketItems();
    const total = items.reduce((sum, item) => sum + (item.price * item.quantity), 0);
    this.totalPrice.set(total);
  }
  
  /**
   * Remove item from basket
   */
  removeFromBasket(id: number): void {

    this.showBasketService.removeFromBasket(id).subscribe({
      next: () => {
        //TODO: notification
      },
      error: (err: unknown) => {
        //TODO: notification
      }
    });
  }
  

  updateQuantity(id: number, event: Event): void {
    const input = event.target as HTMLInputElement;
    const quantity = Math.max(1, Math.min(99, Number(input.value)));
    
    this.showBasketService.updateQuantity(id, quantity).subscribe({
      next: () => {
        //TODO: notification
      },
      error: (err: unknown) => {
        //TODO: notification
      }
    });
  }

  clearBasket(): void {
    this.showBasketService.clearBasket().subscribe({
      next: () => {
        // TODO: notification
      },
      error: (err: unknown) => {
        // TODO: notification
      }
    });
  }
}
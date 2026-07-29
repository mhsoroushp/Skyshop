import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatIcon } from '@angular/material/icon';
import { MatButton } from '@angular/material/button';
import { CurrencyPipe } from '@angular/common';
import { BasketService } from '../../../../core/services/basket.service';
import { BasketViewItem } from '../../../../core/models/baskets.model';


@Component({
  selector: 'app-basket-list',
  standalone: true,
  imports: [
    MatIcon,
    MatButton,
    RouterLink,
    CurrencyPipe
  ],
  templateUrl: './basket-list.component.html',
  styleUrls: ['./basket-list.component.css']
})
export class BasketListComponent {
  basketService = inject(BasketService);
  
  // TODO: deltet
  // Signal for basket items
  basketItems = signal<BasketViewItem[]>([]);
  
  // Computed signal for total price
  totalPrice = signal<number>(0);
  
  
  updateTotalPrice(): void {
    const items = this.basketItems();
    const total = items.reduce((sum, item) => sum + (item.price * item.quantity), 0);
    this.totalPrice.set(total);
  }
  
  removeFromBasket(id: number): void {

    this.basketService.removeFromBasket(id).subscribe({
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
    
    this.basketService.updateQuantity(id, quantity).subscribe({
      next: () => {
        //TODO: notification
      },
      error: (err: unknown) => {
        //TODO: notification
      }
    });
  }

  clearBasket(): void {
    this.basketService.clearBasket().subscribe({
      next: () => {
        // TODO: notification
      },
      error: (err: unknown) => {
        // TODO: notification
      }
    });
  }

  getProductImageSrc(imageBase64: string | undefined): string {
    if (!imageBase64) {
      return '';
    }
    return `data:image/jpeg;base64,${imageBase64}`;
  }
}
// skyshop/src/app/basket/components/basket-list/basket-list.component.ts
import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { BasketItem, BasketService } from '../../basket.service';


@Component({
  selector: 'app-basket-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './basket-list.component.html',
  styleUrls: ['./basket-list.component.css']
})
export class BasketListComponent implements OnInit {
  basketItems: BasketItem[] = [];
  count: number = 0;
  total: number = 0;
  loading: boolean = true;

  quantityEdits: Record<number, number> = {};

  basketService = inject(BasketService);
  cdr = inject(ChangeDetectorRef);

  ngOnInit() {
    console.log('🛒 BasketListComponent initialized');
    this.loadBasket();
    
    // Subscribe to basket updates from other components (e.g., add-to-basket)
    this.basketService.getBasketUpdated().subscribe(() => {
      console.log('📢 Basket update event received! Reloading basket...');
      this.loadBasket();
    });
  }

  loadBasket() {
    this.loading = true;
    console.log('🛒 Loading basket from API...');
    this.basketService.getBasket().subscribe({
      next: (items) => {
        console.log('✅ Basket response received:', items);
        this.basketItems = Array.isArray(items) ? items : [];
        this.loading = false;

        this.basketItems.forEach((item) => {
          if (this.quantityEdits[item.productId] == null) {
            this.quantityEdits[item.productId] = item.quantity;
          }
        });

        this.count = this.basketItems.length;
        this.total = this.basketItems.reduce((sum, item) => sum + item.quantity, 0);

        this.basketService.getBasketCount().subscribe({
          next: (response) => {
            this.count = response.count;
          },
          error: (err) => console.error('❌ Error loading basket count:', err)
        });

        this.basketService.getBasketTotal().subscribe({
          next: (response) => {
            this.total = response.total;
          },
          error: (err) => console.error('❌ Error loading basket total:', err)
        });

        console.log('loading flag set to:', this.loading);
        
        // Force change detection
        this.cdr.detectChanges();
        console.log('✔️ Change detection triggered');
      },
      error: (err) => {
        console.error('❌ Error loading basket:', err);
        console.error('Status:', err.status);
        console.error('Message:', err.message);
        this.loading = false;
        this.basketItems = [];
        this.cdr.detectChanges();
      },
      complete: () => {
        console.log('✔️ Basket subscription completed');
      }
    });
  }

  addItem(productId: number) {
    this.basketService.addToBasket(productId, 1).subscribe({
      next: () => {
        this.loadBasket();
      },
      error: (err) => {
        console.error('Error adding item:', err);
      }
    });
  }

  removeItem(productId: number) {
    this.basketService.removeFromBasket(productId).subscribe({
      next: () => {
        this.loadBasket();
      },
      error: (err) => {
        console.error('Error removing item:', err);
      }
    });
  }

  clearBasket() {
    this.basketService.clearBasket().subscribe({
      next: () => {
        this.basketItems = [];
        this.count = 0;
        this.total = 0;
      },
      error: (err) => {
        console.error('Error clearing basket:', err);
      }
    });
  }

  updateQuantity(productId: number) {
    const quantity = this.quantityEdits[productId];

    this.basketService.updateQuantity(productId, quantity).subscribe({
      next: () => this.loadBasket(),
      error: (err) => console.error('Error updating quantity:', err)
    });
  }
}
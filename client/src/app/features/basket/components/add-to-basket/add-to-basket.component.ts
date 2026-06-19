
// skyshop/src/app/basket/components/add-to-basket/add-to-basket.component.ts
import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { BasketService } from '../../basket.service';

@Component({
  selector: 'app-add-to-basket',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './add-to-basket.component.html',
  styleUrls: ['./add-to-basket.component.css']
})
export class AddToBasketComponent {
  productId: number = 1;
  quantity: number = 1;
  message: string = '';
  isError: boolean = false;

  basketService = inject(BasketService)

  addToBasket() {
    this.isError = false;
    this.message = '';
    
    console.log('🛍️ Adding product to basket:', this.productId);
    
    this.basketService.addToBasket(this.productId, this.quantity).subscribe({
      next: (response) => {
        console.log('✅ Product added successfully:', response);
        this.message = `Product ${this.productId} added to basket with quantity ${this.quantity}!`;
        console.log('🔔 Notifying basket list of changes...');
        this.basketService.notifyBasketUpdated();
      },
      error: (err) => {
        this.isError = true;
        this.message = 'Error adding product to basket';
        console.error('❌ Error adding product:', err);
        console.error('Status:', err.status);
        console.error('Message:', err.message);
      }
    });
  }
}
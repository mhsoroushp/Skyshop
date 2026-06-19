import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { BasketListComponent } from '../../../basket/components/basket-list/basket-list.component';
import { AddToBasketComponent } from '../../../basket/components/add-to-basket/add-to-basket.component';

@Component({
  selector: 'app-users-page',
  standalone: true,
  imports: [CommonModule, BasketListComponent, AddToBasketComponent],
  template: `
    <div class="app">
      <header>
        <h1>Skyshop</h1>
      </header>
      
      <main>
        <app-add-to-basket></app-add-to-basket>
        <app-basket-list></app-basket-list>
      </main>
    </div>
  `,
  styles: [`
    .app {
      font-family: Arial, sans-serif;
      min-height: 100vh;
    }
    
    header {
      background: #2196F3;
      color: white;
      padding: 20px;
      text-align: center;
    }
    
    h1 {
      margin: 0;
    }
    
    main {
      padding: 20px;
    }
  `]
})
export class UsersPage {}

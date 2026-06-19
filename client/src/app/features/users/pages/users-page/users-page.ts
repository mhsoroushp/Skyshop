import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';

@Component({
  selector: 'app-users-page',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="app">
      <header>
        <h1>Skyshop</h1>
      </header>
      
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

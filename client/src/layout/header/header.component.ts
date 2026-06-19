import { Component, inject, OnInit, signal } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { MatIcon } from '@angular/material/icon';
import { MatBadge } from '@angular/material/badge';
import { ShowBasketService } from '../../app/core/services/show-basket.service';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [    
    MatIcon,
    MatBadge,
    RouterLink,
    RouterLinkActive],
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.css']
})
export class HeaderComponent implements OnInit{

  basketCount = signal(0);
  showBasketService = inject(ShowBasketService);

  ngOnInit(): void {
    // this.loadBasketCount();
  }

  // loadBasketCount(){
  //   this.basketService.getBasketCount().subscribe({
  //     next: (data) => {
  //       console.log("the product count is:", data.count);
  //       this.basketCount.set(data.count);

  //       console.log("the value of basketCount var:", this.basketCount);

  //     },
  //     error: (err) => {
  //       console.error("The error is:", err);
  //     },
  //     complete: () => {
  //       console.log("Basket count subscription completed");
  //     }
  //   })
  // }

}
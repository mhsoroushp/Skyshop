import { Component, OnDestroy, OnInit, inject, signal } from '@angular/core';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { MatIcon } from '@angular/material/icon';
import { MatBadge } from '@angular/material/badge';
import { MatButtonModule } from '@angular/material/button';
import { Subject, takeUntil } from 'rxjs';
import { AuthService } from '../../app/core/services/auth.service';
import { ShowBasketService } from '../../app/core/services/show-basket.service';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [    
    MatIcon,
    MatBadge,
    MatButtonModule,
    RouterLink,
    RouterLinkActive],
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.css']
})
export class HeaderComponent implements OnInit, OnDestroy {

  basketCount = signal(0);
  authEmail = signal<string | null>(null);
  isAuthenticated = signal(false);
  private readonly destroy$ = new Subject<void>();
  showBasketService = inject(ShowBasketService);
  authService = inject(AuthService);
  private readonly router = inject(Router);

  ngOnInit(): void {
    this.authService.authState$
      .pipe(takeUntil(this.destroy$))
      .subscribe((state) => {
        this.authEmail.set(state.email);
        this.isAuthenticated.set(state.isAuthenticated);
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  logout(): void {
    this.authService.logout().subscribe(() => {
      this.router.navigate(['/auth']);
    });
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
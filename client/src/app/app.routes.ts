import { Routes } from '@angular/router';
import { UsersPage } from './features/users/pages/users-page/users-page';
import { AUTH_ROUTES } from './features/auth/auth.routes';
import { DASHBOARD_ROUTES } from './features/dashboard/dashboard.routes';
import { DashboardPage } from './features/dashboard/pages/dashboard/dashboard.component';
import { authGuard } from './core/guards/auth.guard';
import { canDeactivateGuard } from './core/guards/can-deactivate.guard';
import { ShopHomeComponent } from './features/shop/page/shop-home.component';
import { ShowBasketListComponent } from './features/basket/components/show/show-basket-list.component';

export const routes: Routes = [
  {path : '', redirectTo: 'dashboard', pathMatch: 'full'},
  {path: 'users', component: UsersPage},

  {path: 'show-basket-list', component: ShowBasketListComponent},
  {path: 'checkout', 
    loadComponent: () => import('./features/checkout/page/checkout.component')
      .then(c => c.CheckoutComponent)
  },

  {path: 'shop/home', component: ShopHomeComponent},
  
  // {path:'dashboard', children: DASHBOARD_ROUTES},
  {path: 'dashboard', component: DashboardPage},
  {path: 'create-book', 
    loadComponent: () => import('./features/book/components/create-book/create-book.component')
      .then(c => c.CreateBookComponent),
      canActivate: [authGuard],
      canDeactivate: [canDeactivateGuard]
  },
  {path: 'auth', children: AUTH_ROUTES},
  {path: 'not-found', loadComponent: () => import('./shared/components/not-found.component')
    .then(c => c.NotFoundComponent)},
  {path: '**', redirectTo: 'not-found' }
];

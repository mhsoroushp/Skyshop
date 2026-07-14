import { Routes } from '@angular/router';
import { UsersPage } from './features/users/pages/users-page/users-page';
import { AUTH_ROUTES } from './features/auth/auth.routes';
import { DASHBOARD_ROUTES } from './features/dashboard/dashboard.routes';
import { DashboardPage } from './features/dashboard/pages/dashboard-page/dashboard-page';
import { BookListComponent } from './features/book/components/book-list/book-list.component';
import { authGuard } from './core/guards/auth.guard';
import { LoginPage } from './features/auth/pages/login-page/login-page';
import { canDeactivateGuard } from './core/guards/can-deactivate.guard';
import { ShopHomeComponent } from './features/shop/page/shop-home.component';
import { ShowBasketListComponent } from './features/basket/components/show/show-basket-list.component';
import { CheckoutComponent } from './features/checkout/checkout.component';
import { PaymentComponent } from './features/payment/payment.component';
import { OrderConfirmationComponent } from './features/order-confirmation/order-confirmation.component';

export const routes: Routes = [
  {path : '', redirectTo: 'dashboard', pathMatch: 'full'},
  {path: 'users', component: UsersPage},

  {path: 'show-basket-list', component: ShowBasketListComponent},
  {path: 'checkout', component: CheckoutComponent},
  {path: 'payment/:orderId', component: PaymentComponent},
  {path: 'order-confirmation/:orderId', component: OrderConfirmationComponent},

  {path: 'books', 
    loadComponent: () => import('./features/book/components/book-list/book-list.component')
      .then(c => c.BookListComponent),
      canActivate: [authGuard]
  },

  {path: 'shop/home', 
    component: ShopHomeComponent
  },

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

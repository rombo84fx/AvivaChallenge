import { Routes } from '@angular/router';
import { ProductListComponent } from './components/product-list/product-list.component';
import { OrderListComponent } from './components/order-list/order-list.component';
import { OrderDetailComponent } from './components/order-detail/order-detail.component';

export const routes: Routes = [
  { path: '', component: ProductListComponent },
  { path: 'orders', component: OrderListComponent },
  { path: 'orders/:id', component: OrderDetailComponent },
  { path: '**', redirectTo: '' },
];

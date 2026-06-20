import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { OrderResponse } from '../../models/order.model';
import { OrderService } from '../../services/order.service';

@Component({
  selector: 'app-order-list',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './order-list.component.html',
  styleUrl: './order-list.component.scss',
})
export class OrderListComponent implements OnInit {
  orders: OrderResponse[] = [];
  isLoading = true;
  errorMessage = '';
  actionMessage = '';

  constructor(
    private orderService: OrderService,
    private router: Router,
    private cdr: ChangeDetectorRef,
  ) {}

  ngOnInit(): void {
    this.loadOrders();
  }

  loadOrders(): void {
    this.isLoading = true;
    this.orderService.getOrders().subscribe({
      next: (orders) => {
        this.orders = orders;
        this.isLoading = false;
        this.cdr.markForCheck();
      },
      error: () => {
        this.errorMessage = 'Error al cargar las ordenes';
        this.isLoading = false;
        this.cdr.markForCheck();
      },
    });
  }

  cancelOrder(order: OrderResponse): void {
    this.actionMessage = '';
    this.errorMessage = '';
    this.orderService.cancelOrder(order.orderId).subscribe({
      next: () => {
        this.actionMessage = `Orden #${order.orderId} cancelada`;
        this.loadOrders();
      },
      error: () => {
        this.errorMessage = `Error al cancelar la orden #${order.orderId}`;
        this.cdr.markForCheck();
      },
    });
  }

  payOrder(order: OrderResponse): void {
    this.actionMessage = '';
    this.errorMessage = '';
    this.orderService.payOrder(order.orderId).subscribe({
      next: () => {
        this.actionMessage = `Orden #${order.orderId} pagada`;
        this.loadOrders();
      },
      error: () => {
        this.errorMessage = `Error al pagar la orden #${order.orderId}`;
        this.cdr.markForCheck();
      },
    });
  }

  viewDetail(orderId: number): void {
    this.router.navigate(['/orders', orderId]);
  }

  goToProducts(): void {
    this.router.navigate(['/']);
  }

  getStatusClass(status: string): string {
    switch (status) {
      case 'Pending':
        return 'status-pending';
      case 'Paid':
        return 'status-paid';
      case 'Cancelled':
        return 'status-cancelled';
      default:
        return '';
    }
  }

  getStatusLabel(status: string): string {
    switch (status) {
      case 'Pending':
        return 'Pendiente';
      case 'Paid':
        return 'Pagada';
      case 'Cancelled':
        return 'Cancelada';
      default:
        return status;
    }
  }

  getPaymentModeLabel(mode: string): string {
    switch (mode) {
      case 'Cash':
        return 'Efectivo';
      case 'CreditCard':
        return 'Tarjeta de Crédito';
      case 'Transfer':
        return 'Transferencia';
      default:
        return mode;
    }
  }
}

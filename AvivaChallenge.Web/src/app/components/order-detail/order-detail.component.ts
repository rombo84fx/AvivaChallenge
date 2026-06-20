import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { OrderResponse } from '../../models/order.model';
import { OrderService } from '../../services/order.service';

@Component({
  selector: 'app-order-detail',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './order-detail.component.html',
  styleUrl: './order-detail.component.scss',
})
export class OrderDetailComponent implements OnInit {
  order: OrderResponse | null = null;
  isLoading = true;
  errorMessage = '';

  constructor(
    private orderService: OrderService,
    private route: ActivatedRoute,
    private router: Router,
    private cdr: ChangeDetectorRef,
  ) {}

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    if (id) {
      this.orderService.getOrder(id).subscribe({
        next: (order) => {
          this.order = order;
          this.isLoading = false;
          this.cdr.markForCheck();
        },
        error: () => {
          this.errorMessage = 'Orden no encontrada';
          this.isLoading = false;
          this.cdr.markForCheck();
        },
      });
    }
  }

  goBack(): void {
    this.router.navigate(['/orders']);
  }

  getStatusLabel(status: string): string {
    switch (status) {
      case 'Pending': return 'Pendiente';
      case 'Paid': return 'Pagada';
      case 'Cancelled': return 'Cancelada';
      default: return status;
    }
  }

  getPaymentModeLabel(mode: string): string {
    switch (mode) {
      case 'Cash': return 'Efectivo';
      case 'CreditCard': return 'Tarjeta de Crédito';
      case 'Transfer': return 'Transferencia';
      default: return mode;
    }
  }

  getStatusClass(status: string): string {
    switch (status) {
      case 'Pending': return 'status-pending';
      case 'Paid': return 'status-paid';
      case 'Cancelled': return 'status-cancelled';
      default: return '';
    }
  }
}

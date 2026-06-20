import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { SelectableProduct, PaymentMode, CreateOrderRequest } from '../../models/order.model';
import { OrderService } from '../../services/order.service';

@Component({
  selector: 'app-product-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './product-list.component.html',
  styleUrl: './product-list.component.scss',
})
export class ProductListComponent {
  products: SelectableProduct[] = [
    { name: 'Laptop Lenovo', unitPrice: 2221, selected: false },
    { name: 'Laptop Dell', unitPrice: 1200, selected: false },
    { name: 'Monitor Samsung 27"', unitPrice: 4500, selected: false },
    { name: 'Teclado Mecánico', unitPrice: 850, selected: false },
    { name: 'Mouse Inalámbrico', unitPrice: 350, selected: false },
    { name: 'Audífonos Bluetooth', unitPrice: 1500, selected: false },
    { name: 'Webcam HD', unitPrice: 600, selected: false },
    { name: 'Disco SSD 1TB', unitPrice: 1800, selected: false },
  ];

  paymentMode: PaymentMode = 'CreditCard';
  isCreating = false;
  errorMessage = '';
  successMessage = '';

  constructor(
    private orderService: OrderService,
    private router: Router,
  ) {}

  get selectedProducts(): SelectableProduct[] {
    return this.products.filter((p) => p.selected);
  }

  get totalAmount(): number {
    return this.selectedProducts.reduce((sum, p) => sum + p.unitPrice, 0);
  }

  get canCreateOrder(): boolean {
    return this.selectedProducts.length > 0 && !this.isCreating;
  }

  toggleProduct(product: SelectableProduct): void {
    product.selected = !product.selected;
    this.errorMessage = '';
    this.successMessage = '';
  }

  createOrder(): void {
    if (!this.canCreateOrder) return;

    this.isCreating = true;
    this.errorMessage = '';
    this.successMessage = '';

    const request: CreateOrderRequest = {
      paymentMode: this.paymentMode,
      products: this.selectedProducts.map((p) => ({
        name: p.name,
        unitPrice: p.unitPrice,
      })),
    };

    this.orderService.createOrder(request).subscribe({
      next: (order) => {
        this.isCreating = false;
        this.successMessage = `Orden #${order.orderId} creada — Proveedor: ${order.providerName} — Total: $${order.amount.toFixed(2)}`;
        this.products.forEach((p) => (p.selected = false));
      },
      error: (err) => {
        this.isCreating = false;
        this.errorMessage = err.error?.error || 'Error al crear la orden';
      },
    });
  }

  goToOrders(): void {
    this.router.navigate(['/orders']);
  }
}

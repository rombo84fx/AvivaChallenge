export interface Product {
  name: string;
  unitPrice: number;
}

export interface Fee {
  name: string;
  amount: number;
}

export interface CreateOrderRequest {
  paymentMode: string;
  products: Product[];
}

export interface OrderResponse {
  orderId: number;
  amount: number;
  status: string;
  paymentMode: string;
  providerName: string;
  providerOrderId: string;
  fees: Fee[];
  products: Product[];
  createdDate: string;
}

export type PaymentMode = 'Cash' | 'CreditCard' | 'Transfer';

export interface SelectableProduct extends Product {
  selected: boolean;
}

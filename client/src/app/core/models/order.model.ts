export interface Order {
  id: string;
  sessionId: string;
  createdAt: Date;
  totalAmount: number;
  status: string;
  customerEmail?: string;
  customerName?: string;
  items: OrderItem[];
}

export interface OrderItem {
  id: string;
  productId: string;
  quantity: number;
  price: number;
  productName?: string;
}

export interface CreateOrderRequest {
  customerEmail?: string;
  customerName?: string;
}

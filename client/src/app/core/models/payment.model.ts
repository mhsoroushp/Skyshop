export interface Payment {
  id: string;
  orderId: string;
  amount: number;
  status: string;
  paymentMethod: string;
  transactionId?: string;
  stripePaymentIntentId?: string;
  createdAt: Date;
  processedAt?: Date;
}

export interface CreatePaymentIntentRequest {
  orderId: string;
}

export interface CreatePaymentIntentResponse {
  clientSecret: string;
  paymentIntentId: string;
}

export interface ProcessPaymentRequest {
  orderId: string;
  paymentMethodId?: string;
  email?: string;
}

export interface OrderItem {
  productId: string;
  productName: string;
  quantity: number;
  unitPrice: number;
}

export interface Order {
  id: string;
  number: number;
  deliveryAddress: string;
  createdAt: string;
  items: OrderItem[];
  total: number;
}

export interface OrderItemRequest {
  productId: string;
  quantity: number;
}

export interface OrderRequest {
  deliveryAddress: string;
  items: OrderItemRequest[];
}

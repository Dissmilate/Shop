import { Component, inject, OnInit, signal } from '@angular/core';
import { DatePipe, DecimalPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Observable } from 'rxjs';
import { Order, OrderRequest } from '../../api/order';
import { OrderApi } from '../../api/order-api';
import { Product } from '../../api/product';
import { ProductApi } from '../../api/product-api';

@Component({
  selector: 'app-orders',
  imports: [DatePipe, DecimalPipe, ReactiveFormsModule],
  templateUrl: './orders.html',
  styleUrl: './orders.css',
})
export class Orders implements OnInit {
  private readonly orderApi = inject(OrderApi);
  private readonly productApi = inject(ProductApi);
  private readonly fb = inject(NonNullableFormBuilder);

  protected readonly orders = signal<Order[]>([]);
  protected readonly products = signal<Product[]>([]);
  protected readonly error = signal<string | null>(null);
  protected readonly editingId = signal<string | null>(null);

  protected readonly form = this.fb.group({
    deliveryAddress: ['', [Validators.required, Validators.maxLength(500)]],
    items: this.fb.array([this.createItem()]),
  });

  ngOnInit(): void {
    this.load();
    this.productApi.getAll().subscribe({
      next: (products) => this.products.set(products),
      error: () => this.error.set('Failed to load products.'),
    });
  }

  private describe(err: HttpErrorResponse): string {
    const problem = err.error;
    if (problem?.errors) {
      return Object.values(problem.errors).flat().join(' ');
    }
    return problem?.detail ?? problem?.title ?? 'Something went wrong.';
  }

  protected addItem(): void {
    this.form.controls.items.push(this.createItem());
  }

  protected removeItem(index: number): void {
    this.form.controls.items.removeAt(index);
  }

  protected save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const request: OrderRequest = this.form.getRawValue();
    const id = this.editingId();
    const request$: Observable<unknown> = id
      ? this.orderApi.update(id, request)
      : this.orderApi.create(request);

    request$.subscribe({
      next: () => {
        this.cancel();
        this.load();
      },
      error: (err: HttpErrorResponse) => this.error.set(this.describe(err)),
    });
  }

  protected edit(order: Order): void {
    this.editingId.set(order.id);
    this.form.controls.deliveryAddress.setValue(order.deliveryAddress);
    this.form.controls.items.clear();
    for (const item of order.items) {
      this.form.controls.items.push(this.createItem(item.productId, item.quantity));
    }
    this.error.set(null);
  }

  protected cancel(): void {
    this.editingId.set(null);
    this.form.reset();
    this.form.controls.items.clear();
    this.form.controls.items.push(this.createItem());
    this.error.set(null);
  }

  protected remove(order: Order): void {
    if (!confirm(`Delete order #${order.number}?`)) {
      return;
    }

    this.orderApi.delete(order.id).subscribe({
      next: () => {
        if (this.editingId() === order.id) {
          this.cancel();
        }
        this.load();
      },
      error: (err: HttpErrorResponse) => this.error.set(this.describe(err)),
    });
  }

  private load(): void {
    this.orderApi.getAll().subscribe({
      next: (orders) => this.orders.set(orders),
      error: () => this.error.set('Failed to load orders.'),
    });
  }

  private createItem(productId = '', quantity = 1) {
    return this.fb.group({
      productId: [productId, Validators.required],
      quantity: [quantity, [Validators.required, Validators.min(1), Validators.max(500)]],
    });
  }
}

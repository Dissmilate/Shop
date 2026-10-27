import { Component, inject, OnInit, signal } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Observable } from 'rxjs';
import { Product, ProductRequest } from '../../api/product';
import { ProductApi } from '../../api/product-api';

@Component({
  selector: 'app-products',
  imports: [DecimalPipe, ReactiveFormsModule],
  templateUrl: './products.html',
  styleUrl: './products.css',
})
export class Products implements OnInit {
  private readonly api = inject(ProductApi);
  private readonly fb = inject(NonNullableFormBuilder);

  protected readonly products = signal<Product[]>([]);
  protected readonly error = signal<string | null>(null);
  protected readonly editingId = signal<string | null>(null);

  protected readonly form = this.fb.group({
    name: ['', [Validators.required, Validators.maxLength(200)]],
    price: [0, [Validators.required, Validators.min(0.01)]],
  });

  ngOnInit(): void {
    this.load();
  }

  protected save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const request: ProductRequest = this.form.getRawValue();
    const id = this.editingId();
    const request$: Observable<unknown> = id
      ? this.api.update(id, request)
      : this.api.create(request);

    request$.subscribe({
      next: () => {
        this.cancel();
        this.load();
      },
      error: (err: HttpErrorResponse) => this.error.set(this.describe(err)),
    });
  }

  protected edit(product: Product): void {
    this.editingId.set(product.id);
    this.form.setValue({ name: product.name, price: product.price });
    this.error.set(null);
  }

  protected cancel(): void {
    this.editingId.set(null);
    this.form.reset();
    this.error.set(null);
  }

  protected remove(product: Product): void {
    if (!confirm(`Delete "${product.name}"?`)) {
      return;
    }

    this.api.delete(product.id).subscribe({
      next: () => {
        if (this.editingId() === product.id) {
          this.cancel();
        }
        this.load();
      },
      error: (err: HttpErrorResponse) => this.error.set(this.describe(err)),
    });
  }

  private load(): void {
    this.api.getAll().subscribe({
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
}

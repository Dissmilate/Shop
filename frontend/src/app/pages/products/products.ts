import { Component, inject, OnInit, signal } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { Product } from '../../api/product';
import { ProductApi } from '../../api/product-api';

@Component({
  selector: 'app-products',
  imports: [DecimalPipe],
  templateUrl: './products.html',
  styleUrl: './products.css',
})
export class Products implements OnInit {
  private readonly api = inject(ProductApi);

  protected readonly products = signal<Product[]>([]);
  protected readonly error = signal<string | null>(null);

  ngOnInit(): void {
    this.load();
  }

  private load(): void {
    this.api.getAll().subscribe({
      next: (products) => this.products.set(products),
      error: () => this.error.set('Failed to load products.'),
    });
  }
}

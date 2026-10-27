import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { Product, ProductRequest } from './product';

@Injectable({ providedIn: 'root' })
export class ProductApi {
  private readonly http = inject(HttpClient);
  private readonly url = '/api/products';

  getAll(): Observable<Product[]> {
    return this.http.get<Product[]>(this.url);
  }

  create(request: ProductRequest): Observable<Product> {
    return this.http.post<Product>(this.url, request);
  }

  update(id: string, request: ProductRequest): Observable<void> {
    return this.http.put<void>(`${this.url}/${id}`, request);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.url}/${id}`);
  }
}

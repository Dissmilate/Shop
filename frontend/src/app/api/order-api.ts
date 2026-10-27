import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { Order, OrderRequest } from './order';

@Injectable({ providedIn: 'root' })
export class OrderApi {
  private readonly http = inject(HttpClient);
  private readonly url = '/api/orders';

  getAll(): Observable<Order[]> {
    return this.http.get<Order[]>(this.url);
  }

  create(request: OrderRequest): Observable<Order> {
    return this.http.post<Order>(this.url, request);
  }

  update(id: string, request: OrderRequest): Observable<void> {
    return this.http.put<void>(`${this.url}/${id}`, request);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.url}/${id}`);
  }
}

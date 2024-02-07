import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class OrderService {

  private apiUrl:string  = 'http://localhost:8578/api/order'

  constructor(private http: HttpClient) {}

  getOrders(userId: string): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/getOrders/${userId}`);
  }

  buyOrder(body: any): Observable<any> {
    console.log(body)
    return this.http.post<any>(this.apiUrl+ '/createOrder', body);
  }
}

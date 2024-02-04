import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class UserService {

  private baseUrl:string  = 'http://localhost:8578/api/user'
  constructor(private http: HttpClient) {}

  login(body:any): Observable<any> {

    console.log(body);
    return this.http.post<any>(this.baseUrl+ '/login', body);
  }

  register(user: any): Observable<any> {
    return this.http.post<any>(this.baseUrl+ '/register', user);
  }

  update(user: any): Observable<any> {
    return this.http.post<any>(this.baseUrl+ '/update', user);
  }
}

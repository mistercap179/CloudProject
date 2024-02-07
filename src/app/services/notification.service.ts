import { Injectable } from '@angular/core';
import { Observable, Subject } from 'rxjs';
import { webSocket } from "rxjs/webSocket";
import { HttpClient } from '@angular/common/http';

@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  private notificationSubject = new Subject<string>();
  private baseUrl:string  = 'http://localhost:8578/api/user'

  constructor(private http: HttpClient) { }

  public connect(wssLink : any): Observable<any> {

    var observable = new Observable<any>((subcriber) => {

      var subject = webSocket(wssLink)

      subject.subscribe(
        msg => subcriber.next(msg),
        err => console.log(err),
        () => console.log('complete')
      );
    })

    return observable
  }

  getNotificationUrl(): Observable<any[]> {
    return this.http.get<any[]>(this.baseUrl+ '/notification');
  }

}

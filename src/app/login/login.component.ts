import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { UserService } from '../services/user.service';
import { NotificationService } from '../services/notification.service';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent {

  public loginForm : FormGroup;
  
  constructor(private formBuilder : FormBuilder,private router : Router,private userService : UserService,
    private notificationService:NotificationService) {
    
    this.loginForm = this.formBuilder.group({
      email : ['',[Validators.required]],
      password : ['',[Validators.required]]
    });

  }

  public get email() {
    return this.loginForm.get('email');
  }

  public get password() {
    return this.loginForm.get('password') ;
  }

  public register(){
    this.router.navigate(['/registration']);
  }

  public submitForm(){
    
    const data =  this.loginForm.value;

    if(!this.loginForm.valid){
      window.alert('Not valid!');
      return;
    }

    this.userService.login(data).subscribe(response=>{
      
      console.log(response.message);
      if(response.message == "Invalid email or password"){
        alert("Invalid email or password");
      }
      else{
        localStorage.setItem('user',JSON.stringify(response.user)); 
        this.notificationService.getNotificationUrl().subscribe((data:any)=>{
          localStorage.setItem('wssLink',JSON.stringify(data.uri)); 
        })
        this.router.navigate(['/home']); 
      }


    });
  

  }
}

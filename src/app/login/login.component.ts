import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { UserService } from '../services/user.service';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent {

  public loginForm : FormGroup;
  
  constructor(private formBuilder : FormBuilder,private router : Router,private userService : UserService) {
    
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
      localStorage.setItem('user',JSON.stringify(response.user)); 
      this.router.navigate(['/home']); 
    });
  

  }
}

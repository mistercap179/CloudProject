import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent {
  public registrationForm: FormGroup;
  constructor(
    private formBuilder: FormBuilder,
    private router: Router,
    ) {

    this.registrationForm = this.formBuilder.group({
      // account informations
      Firstname: ['', [Validators.required]],
      LastName: ['', [Validators.required]],
      Email: ['', [Validators.email, Validators.required]],
      AccountBalance: ['', Validators.required],
      Password: ['', [Validators.required]],
    });
  }


  public get Firstname() {
    return this.registrationForm.get('name');
  }

  public get LastName() {
    return this.registrationForm.get('surName');
  }

  public get Email() {
    return this.registrationForm.get('email');
  }
  public get AccountBalance() {
    return this.registrationForm.get('image');
  }
  public get Password() {
    return this.registrationForm.get('password');
  }

  public login(){
    this.router.navigate(['login']);
  }

  public submitForm(){
    
    const data =  this.registrationForm.value;

    if(!this.registrationForm.valid){
      window.alert('Not valid!');
      return;
    }

    console.log(data);

  }
  
}

import { Component, ViewChild } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { UserService } from '../services/user.service';
import { MatTableDataSource } from '@angular/material/table';
import { MatPaginator } from '@angular/material/paginator';
import { MatSort } from '@angular/material/sort';
import { ProductService } from '../services/product.service';
import { OrderService } from '../services/order.service';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css'],
  animations: [
    // Add any animations you have here
  ],
})
export class HomeComponent {
  public updateForm: FormGroup;
  public user : any;
  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;
  public displayedColumns: string[] = [
    'name',
    'quantity',
    'description',
    'price',
    "action"
  ];

  public ordersDataSource = new MatTableDataSource<any>();
  public ordersDisplayedColumns: string[] = ['orderId', 'products', 'totalPrice'];

  public cartDisplayedColumns: string[] = ['cartName', 'cartQuantity', 'cartPrice', 'cartAction'];
  public cartDataSource = new MatTableDataSource<any>();

  public dataSource = new MatTableDataSource<any>();

  constructor(
    private formBuilder: FormBuilder,
    private router: Router,
    private userService:UserService,
    private productService: ProductService,
    private orderService : OrderService
    ) {

    this.updateForm = this.formBuilder.group({
      // account informations
      Firstname: ['', [Validators.required]],
      LastName: ['', [Validators.required]],
      Email: ['', [Validators.email, Validators.required]],
      AccountBalance: ['', Validators.required],
      Password: ['', [Validators.required]],
    });

    this.user = JSON.parse(localStorage.getItem('user')!);

    this.productService.getProducts().subscribe(products => {
      console.log(products);
      this.loadTable(products);
    })
    
    this.orderService.getOrders(this.user.userId).subscribe(
      (response) => {
        console.log('Orders found:', response);
        this.ordersDataSource = new MatTableDataSource<any>(response);
        this.dataSource.paginator = this.paginator;
        this.dataSource.sort = this.sort;
      },
      (error) => {
        // Ako nema narudžbina
        if (error.status === 404) {
          console.log('No orders found for the specified user.');
        } else {
          console.error('Error fetching orders:', error);
        }
      }
    );

  }

  loadTable(products: any) {
    this.dataSource = new MatTableDataSource<any>(products);
    this.dataSource.paginator = this.paginator;
    this.dataSource.sort = this.sort;
  }

  public calculateTotalPrice(): number {
    let total = 0;

    for (const cartItem of this.cartDataSource.data) {
      total += cartItem.price * cartItem.quantity;
    }

    return total;
  }

  public submitForm(){
    
    const data =  this.updateForm.value;

    if(!this.updateForm.valid){
      window.alert('Not valid!');
      return;
    }
    data.userId = this.user.userId;
    this.userService.update(data).subscribe(response => {
      localStorage.setItem('user',JSON.stringify(response.user)); 
    })
    

  }

  public addToCart(product: any) {
    const productId = product.productId;
  
    const existingCartItemIndex = this.cartDataSource.data.findIndex(item => item.productId === productId);
  
    if (existingCartItemIndex !== -1) {
      const currentQuantity = this.cartDataSource.data[existingCartItemIndex].quantity;
      if (currentQuantity < product.quantity) {
        this.cartDataSource.data[existingCartItemIndex].quantity += 1;
      } else {
        console.error('Dostignuta maksimalna količina proizvoda u korpi.');
      }
    } else {
      const cartItem = { name: product.name, quantity: 1, price: product.price, productId: productId };
      this.cartDataSource.data = [...this.cartDataSource.data, cartItem];
    }
  }

  public adjustCartItemQuantity(cartItem: any, action: 'increment' | 'decrement') {
    const productId = cartItem.productId;
  
    const productIndex = this.dataSource.data.findIndex(item => item.productId === productId);
    
    const product = this.dataSource.data[productIndex];
  
    if (productIndex !== -1) {

      if (action === 'increment' && cartItem.quantity < product.quantity) {
        cartItem.quantity++;
      } else if (action === 'decrement' && cartItem.quantity > 1) {
        cartItem.quantity--;
      }
      this.cartDataSource.data = [...this.cartDataSource.data];
    } else {
      
      console.error('Proizvod nije pronađen u korpi.');
    }
  }

  public removeFromCart(cartItem: any) {
    this.cartDataSource.data = this.cartDataSource.data.filter(item => item !== cartItem);
  }
  
  public buyItems() {
    for (const cartItem of this.cartDataSource.data) {
      console.log(`Name: ${cartItem.name}, Quantity: ${cartItem.quantity}, Price: ${cartItem.price},ID: ${cartItem.productId}`);
    }
    this.cartDataSource.data = [];
  }

  public logout() {
    localStorage.removeItem('user'); 
    this.router.navigate(['login']);
  }
}

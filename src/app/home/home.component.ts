import { Component, ViewChild } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { UserService } from '../services/user.service';
import { MatTableDataSource } from '@angular/material/table';
import { MatPaginator } from '@angular/material/paginator';
import { MatSort } from '@angular/material/sort';
import { ProductService } from '../services/product.service';
import { OrderService } from '../services/order.service';
import { User } from '../models/user.model';
import { CartItem } from '../models/cartItem.model';
import { ICreateOrderRequest, IPayPalConfig } from 'ngx-paypal';
import { ToastrService } from 'ngx-toastr';
import { NotificationService } from '../services/notification.service';

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
  private totalPrice : any;
  private allProducts : any;
  public type : any;
  selectedCategory: string = 'all';
  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;
  public displayedColumns: string[] = [
    'name',
    'quantity',
    'description',
    'price',
    'category',
    "action"
  ];

  public ordersDataSource = new MatTableDataSource<any>();
  public ordersDisplayedColumns: string[] = ['orderId', 'products', 'totalPrice','type'];

  public cartDisplayedColumns: string[] = ['cartName', 'cartQuantity', 'cartPrice', 'cartAction'];
  public cartDataSource = new MatTableDataSource<any>();

  public uniqueCategories: string[] = [];
  public dataSource = new MatTableDataSource<any>();

  private wssLink :any;
  public payPalConfig?: IPayPalConfig;

  constructor(
    private formBuilder: FormBuilder,
    private router: Router,
    private userService:UserService,
    private productService: ProductService,
    private orderService : OrderService,
    private toastr: ToastrService,
    private notificationService : NotificationService
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
      this.allProducts = products;
      this.allProducts.forEach((product : any) => {
        if (!this.uniqueCategories.includes(product.category)) {
          this.uniqueCategories.push(product.category);
        }
      });
      localStorage.setItem('products',JSON.stringify(this.allProducts)); 
      this.loadTable(this.allProducts);
    })
    
    this.loadOrders();

  }

  showSuccess() {
    this.toastr.success('This is a success message', 'Success');
  }

  showError() {
    this.toastr.error('This is an error message', 'Error');
  }


  ngOnInit(): void {
    this.initConfig();
    this.wssLink = JSON.parse(localStorage.getItem('wssLink')!); 
    this.notificationService.connect(this.wssLink)
                .subscribe((msg)=>{
                  if(msg.userId == this.user.userId){
                    this.toastr.success('Order created success!', 'Success');
                  }
                })
  }


  private initConfig(): void {
    this.payPalConfig = {
    currency: 'USD',
    clientId: 'AVMllih9df50A-JE-sUv29Ks7ROeZ4wf-JoE7W2cy3KenOQz5s87xsbXlequTIsG9Ii9R3ycCY8Hw5x5',
    createOrderOnClient: (data: any): ICreateOrderRequest => {
      return {
        intent: 'CAPTURE',
        purchase_units: [{
          amount: {
            currency_code: 'USD',
            value: this.totalPrice.toString(),
          },
          items:[]
        }]
      };
    },
    advanced: {
      commit: 'true'
    },
    style: {
      label: 'paypal',
      layout: 'vertical'
    },
    onApprove: (data : any, actions : any) => {
      const user : User = {
        UserId : this.user.userId,
        FirstName : this.user.firstName,
        LastName : this.user.lastName,
        Email : this.user.email,
        Password : this.user.password,
        AccountBalance : this.user.accountBalance
     }
 
     const cartItems: CartItem[] = this.cartDataSource.data.map(cartItem => {
       return {
         ProductId: cartItem.productId,
         Name: cartItem.name,
         Description: cartItem.description,
         Price: cartItem.price,
         Quantity: cartItem.quantity
       };
     });
 
     const orderBody = {
       User: user,
       CartItems: cartItems,
       TotalPrice: this.totalPrice,
       Type: "Paypal"
     };
 

     this.orderService.buyOrder(orderBody).subscribe(response => {
       if(response.message == "Order created successfully"){
         this.refreshAll();
       }
       else{
         alert(response.message);
       }
         
     });
 
     this.cartDataSource.data = [];
    },
    onCancel: (data : any, actions : any) => {
      console.log('OnCancel', data, actions);
    },
    onError: (err:any) => {
      console.log('OnError', err);
    }
  };
  }


  applyCategoryFilter() {
    this.dataSource.filter = this.selectedCategory !== 'all' ? this.selectedCategory : '';
  }

  updateProfile(){

    this.user = JSON.parse(localStorage.getItem('user')!);

    this.updateForm.patchValue({
      Firstname: this.user.firstName,
      LastName: this.user.lastName,
      Email: this.user.email,
      AccountBalance: this.user.accountBalance,
      Password: this.user.password,
    });


  }

  loadTable(products: any) {
    this.dataSource = new MatTableDataSource<any>(products);
    this.dataSource.paginator = this.paginator;
    this.dataSource.sort = this.sort;
  }

  loadOrders(){
    this.orderService.getOrders(this.user.userId).subscribe(
      (response) => {
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

  public calculateTotalPrice(): number {
    let total = 0;

    for (const cartItem of this.cartDataSource.data) {
      total += cartItem.price * cartItem.quantity;
    }

    this.totalPrice = total;
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
    const productIndex = this.dataSource.data.findIndex(item => item.productId === productId);

    const totalPrice = this.totalPrice + product.price;

    if (this.user.accountBalance >= totalPrice && this.user.accountBalance >= this.dataSource.data[productIndex].price) {

      const existingCartItemIndex = this.cartDataSource.data.findIndex(item => item.productId === productId);
    
      if (existingCartItemIndex !== -1) {
        
        
        if (productIndex !== -1) {
          this.dataSource.data[productIndex].quantity--;
          this.dataSource.data = [...this.dataSource.data];
        } else {
          console.error('Proizvod nije pronađen u tabeli proizvoda.');
        }
        this.cartDataSource.data[existingCartItemIndex].quantity += 1;
        
      } else {
        const cartItem = { name: product.name, quantity: 1, price: product.price, productId: productId ,description:product.description};
        this.cartDataSource.data = [...this.cartDataSource.data, cartItem];
      
        const productIndex = this.dataSource.data.findIndex(item => item.productId === productId);
        if (productIndex !== -1) {
          this.dataSource.data[productIndex].quantity--;
          this.dataSource.data = [...this.dataSource.data];
        } else {
          console.error('Proizvod nije pronađen u tabeli proizvoda.');
        }
      }
    }
    else{
      alert('Nemate dovoljno sredstava za dodavanje proizvoda u korpu.');
    }
  }

  public adjustCartItemQuantity(cartItem: any, action: 'increment' | 'decrement') {

    const productId = cartItem.productId;
  
    const productIndex = this.dataSource.data.findIndex(item => item.productId === productId);

    const totalPrice = this.totalPrice + cartItem.price;

    const products  = JSON.parse(localStorage.getItem('products')!);

    const desiredProduct = products.find((prod: any) => prod.productId === productId);

    if (productIndex !== -1) {

      if (action === 'increment' && cartItem.quantity < desiredProduct.quantity && (this.user.accountBalance >= totalPrice && this.user.accountBalance >= this.dataSource.data[productIndex].price)) {
        cartItem.quantity++;
        this.dataSource.data[productIndex].quantity--;
      } 
      else if (action === 'decrement' && cartItem.quantity > 1) 
      {
        cartItem.quantity--;
        this.dataSource.data[productIndex].quantity++;
      }
      else {
        alert('Nemate dovoljno sredstava za dodavanje proizvoda u korpu.');
      }
      this.cartDataSource.data = [...this.cartDataSource.data];
    } else 
    {
      console.error('Proizvod nije pronađen u korpi.');
    }
  
    
  }

  public removeFromCart(cartItem: any) {
    const storedProducts  = JSON.parse(localStorage.getItem('products')!);
    const storedProduct = storedProducts.find((product: any) => product.productId === cartItem.productId);
    this.cartDataSource.data = this.cartDataSource.data.filter(item => item !== cartItem);
  
    this.dataSource.data = this.dataSource.data.map(item => (item.productId === cartItem.productId ? storedProduct : item));
    
    this.dataSource.data = [...this.dataSource.data]; // Dodajte ovu liniju
  }
  
  public buyItems() {

    const user : User = {
       UserId : this.user.userId,
       FirstName : this.user.firstName,
       LastName : this.user.lastName,
       Email : this.user.email,
       Password : this.user.password,
       AccountBalance : this.user.accountBalance
    }

    const cartItems: CartItem[] = this.cartDataSource.data.map(cartItem => {
      return {
        ProductId: cartItem.productId,
        Name: cartItem.name,
        Description: cartItem.description,
        Price: cartItem.price,
        Quantity: cartItem.quantity
      };
    });

    const orderBody = {
      User: user,
      CartItems: cartItems,
      TotalPrice: this.totalPrice,
      Type: "Cash"
    };

    this.orderService.buyOrder(orderBody).subscribe(response => {
      if(response.message == "Order created successfully"){
        this.refreshAll();
      }
      else{
        alert(response.message);
      }
        
    });

    this.cartDataSource.data = [];
  }

  public logout() {
    localStorage.removeItem('user'); 
    this.router.navigate(['login']);
  }

  public refreshAll(){
    this.userService.getUserById(this.user.userId).subscribe(user=>{
      localStorage.setItem('user',JSON.stringify(user)); 
      this.updateProfile();
    })
    this.loadOrders();
  }
}

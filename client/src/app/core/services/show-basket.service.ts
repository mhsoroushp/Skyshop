import { computed, Injectable, signal } from "@angular/core";
import { environment } from "../../../environments/environment";
import { HttpClient } from "@angular/common/http";
import { Observable, map } from "rxjs";

export interface BasketItem {
  productId: number;
  quantity: number;
  price?: number;
  productName?: string;
}


export interface AddToBasketRequest {
  productId: number;
  quantity: number;
}

//TODO : find best solution
export interface UpdateQuantityRequest {
  quantity: number;
}

@Injectable({
  providedIn: 'root'
})
export class ShowBasketService {


    constructor(private http: HttpClient) {
        this.getBasket();
    }

    private baseUrl = environment.apiBaseUrl + 'basket';

    private _basketItems = signal<BasketItem[]>([]);

    basketItems = this._basketItems.asReadonly();
    
    basketItemCount = computed(() => {
        return this._basketItems()?.reduce((count, item) => count + item.quantity, 0);
    });

    totolPrice = computed(() => {
        return this._basketItems()?.reduce((total, item) => total + (item.price || 0) * item.quantity, 0);
    });


    getBasket(){
        this.http.get<BasketItem[]>(this.baseUrl).subscribe({
            next: (basket) => {
                console.log('Fetched basket items:', basket);
                this._basketItems.set(basket);
            },
            error: (err) => {
                console.error('Error fetching basket items:', err);
            }
        });
    }

    addToBasket(productId: number, quantity: number): Observable<any> {
        const request : AddToBasketRequest = { productId, quantity };
        return this.http.post<any>(`${this.baseUrl}/add`, request).pipe(
            map((response) => {
                // After adding to basket, refresh the basket items
                this.getBasket();
                return response;
            })
        );
    }

    removeFromBasket(productId: number): Observable<any> {
        return this.http.delete<any>(`${this.baseUrl}/product/${productId}`).pipe(
            map((response) => {
                // After removing from basket, refresh the basket items
                this.getBasket();
                return response;
            })
        );
    }

    updateQuantity(productId: number, quantity: number): Observable<any> {
        const request: UpdateQuantityRequest = { quantity };
        return this.http.put<any>(`${this.baseUrl}/product/${productId}/quantity`, request).pipe(
            map((response) => {
                // After updating quantity, refresh the basket items
                this.getBasket();
                return response;
            })
        );
    }

    clearBasket(): Observable<any> {
        return this.http.delete<any>(`${this.baseUrl}/clear`).pipe(
            map((response) => {
                // After clearing basket, refresh the basket items
                this.getBasket();
                return response;
            })
        );
    }


}
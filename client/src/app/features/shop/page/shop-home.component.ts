import { Component, inject } from "@angular/core";
import { ActivatedRoute } from '@angular/router';
import { Book } from "../../book/models/book.model";
import { ShowBasketService } from "../../../core/services/show-basket.service";

@Component({
    selector: 'app-shop-home', 
    standalone: true,
    imports: [], 
    templateUrl: './shop-home.component.html',
})
export class ShopHomeComponent{

    selectedBook: Book | undefined = undefined;

    showBasketService = inject(ShowBasketService);

    constructor(private route: ActivatedRoute){
        this.route.queryParams.subscribe(_book => {
            this.selectedBook = _book as unknown as Book;
        });
    }

    addToBasket(): void {
        if(this.selectedBook){
            this.showBasketService.addToBasket(500, 1).subscribe({
                next: () => {
                    console.log('Product added to basket successfully');
                },
                error: (err) => {
                    console.error('Error adding product to basket:', err);
                }
            });
        } else {
            console.warn('No book selected to add to basket');
        }
    }

}
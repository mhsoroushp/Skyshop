import { Component } from "@angular/core";
import { ActivatedRoute } from '@angular/router';
import { Book } from "../../book/models/book.model";

@Component({
    selector: 'app-shop-home', 
    standalone: true,
    imports: [], 
    templateUrl: './shop-home.component.html',
})
export class ShopHomeComponent{

    selectedBook: Book | undefined = undefined;

    constructor(private route: ActivatedRoute){
        this.route.queryParams.subscribe(_book => {
            this.selectedBook = _book as unknown as Book;
        });
    }

}
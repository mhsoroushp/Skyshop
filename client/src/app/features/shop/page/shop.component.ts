import { Component, inject, signal } from "@angular/core";
import { ActivatedRoute, RouterLink } from '@angular/router';
import { Book } from "../../../core/models/book.model";
import { CommonModule } from "@angular/common";
import { ShowBasketService } from "../../../core/services/show-basket.service";
import { BookService } from "../../../core/services/book.service";
import { catchError, of, switchMap } from "rxjs";

@Component({
    selector: 'app-shop', 
    standalone: true,
    imports: [CommonModule, RouterLink], 
    templateUrl: './shop.component.html',
})
export class ShopComponent{

    selectedBook = signal<Book | undefined>(undefined);
    isLoading = signal(true);

    showBasketService = inject(ShowBasketService);
    bookService = inject(BookService);

    constructor(private route: ActivatedRoute){
        this.route.queryParamMap.pipe(
            switchMap(params => {
                const selectedBookId = params.get('selectedBookId');

                if (!selectedBookId) {
                    this.selectedBook.set(undefined);
                    this.isLoading.set(false);
                    return of(null);
                }

                this.isLoading.set(true);
                return this.bookService.getBookById(selectedBookId as unknown as number).pipe(
                    catchError(() => of(null))
                );
            })
        ).subscribe(book => {
            if (book) {
                this.selectedBook.set(book);
            }

            this.isLoading.set(false);
        });
    }


    addToBasket(): void {
        const selectedBook = this.selectedBook();
        if(selectedBook){
            this.showBasketService.addToBasket(selectedBook.id, 1).subscribe({
                next: () => {
                },
                error: (err) => {
                    console.error('Error adding product to basket:', err);
                }
            });
        } else {
            console.warn('No book selected to add to basket');
        }
    }

    coverImageSrcsss(): string {
        const selectedBook = this.selectedBook();
        if (!selectedBook?.coverImageBase64) {
            return "";
        }

        return `data:image/jpeg;base64,${selectedBook.coverImageBase64}`;
    }
}
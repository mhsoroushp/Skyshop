import { Component, EventEmitter, Input, Output, inject } from "@angular/core";
import { Book } from "../../models/book.model";
import { CommonModule } from "@angular/common";
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { Router } from "@angular/router";

@Component({
    selector: 'app-book-card', 
    standalone: true,
    imports: [CommonModule, MatCardModule, MatButtonModule], 
    templateUrl: './book-card.component.html',
})
export class BookCardComponent {
    @Input() book!: Book;

    @Output() favorite = new EventEmitter<Book>();

    router = inject(Router);

    markFavorite(): void {
        this.favorite.emit(this.book);
    }

    selectBook(): void {
        this.router.navigate(['/shop/home'], {
            queryParams: this.book
        });
    }
}
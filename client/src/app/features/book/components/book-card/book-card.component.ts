import { Component, Input, inject } from "@angular/core";
import { Book } from "../../../../core/models/book.model";
import { CommonModule } from "@angular/common";
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { Router } from "@angular/router";
import { BookService } from "../../../../core/services/book.service";

@Component({
    selector: 'app-book-card', 
    standalone: true,
    imports: [CommonModule, MatCardModule, MatButtonModule], 
    templateUrl: './book-card.component.html',
    styleUrl: './book-card.component.scss'
})
export class BookCardComponent {
    @Input() book!: Book;

    router = inject(Router);
    bookService = inject(BookService);

    get coverImageSrc(): string {
        if (!this.book?.coverImageBase64) {
            return "";
        }

        return `data:image/jpeg;base64,${this.book.coverImageBase64}`;
    }

    selectBook(): void {
        this.router.navigate(['/shop'], {
            queryParams: { selectedBookId: this.book.id }
        });
    }
}
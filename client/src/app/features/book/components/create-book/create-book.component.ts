import { Component, inject } from "@angular/core";

import {MatFormFieldModule} from '@angular/material/form-field';
import {MatInputModule} from '@angular/material/input';
import {MatButtonModule} from '@angular/material/button';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { BookService } from "../../services/book.service";
import { Book } from "../../models/book.model";
import { Deactivate } from "../../../../core/models/deactivate.model";

@Component({
    selector: 'app-create-book',
    imports: [MatFormFieldModule, MatInputModule, MatButtonModule, ReactiveFormsModule],
    templateUrl: './create-book.component.html',
})
export class CreateBookComponent implements Deactivate {  

    bookService = inject(BookService);

    bookForm = new FormGroup({
        title: new FormControl('', [Validators.required]),
        author: new FormControl('', [Validators.required])
    });

    createBook(): void {
        if (this.bookForm.valid) {
            const { title, author } = this.bookForm.value;
            const newBookId = this.bookService.books().length + 1; // Simple ID generation
            const newBook: Book = { id: newBookId, title: title!, author: author! };
            this.bookService.addBook(newBook);
            this.bookForm.reset();
        }
    }

    canDeactivate(): boolean {
        return !this.bookForm.dirty;
    }
}
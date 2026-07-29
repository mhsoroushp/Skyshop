import { AfterViewChecked, Component, OnInit, ViewChild, inject } from '@angular/core';
import { BookService } from '../../../../core/services/book.service';
import { CommonModule } from '@angular/common';
import { BookCardComponent } from '../book-card/book-card.component';
import { Book } from '../../../../core/models/book.model';
import { MatPaginatorModule, PageEvent, MatPaginator } from '@angular/material/paginator';
import { BookSearchRequest } from '../../../../core/models/bookSearchRequest';

@Component({
  selector: 'app-book-list',
  standalone: true,
  imports: [CommonModule, BookCardComponent, MatPaginatorModule],
  templateUrl: './book-list.component.html'
})
export class BookListComponent implements OnInit, AfterViewChecked {

  @ViewChild(MatPaginator) paginator!: MatPaginator;

  books: Book[] = [];
  request: BookSearchRequest = {
    pageIndex: 0,
    pageSize: 6,
    totalItems: 0
  };

  bookService = inject(BookService);

  ngOnInit(): void {
    this.loadBooks();
  }

  ngAfterViewChecked(): void {
    if (this.paginator && this.request.pageIndex !== this.paginator.pageIndex) {
      this.paginator.pageIndex = this.request.pageIndex;
    }
  }

  loadBooks() {
    this.bookService.getBooks(this.request).subscribe(bookpage => {
      this.books = bookpage.items;
      this.request.totalItems = bookpage.totalItems;
      this.request.pageIndex = bookpage.pageIndex;
      this.request.pageSize = bookpage.pageSize;

      // Force paginator to reflect the correct page
      if (this.paginator) {
        this.paginator.pageIndex = this.request.pageIndex;
      }
    });
  }

  onPageChange(event: PageEvent) {
    this.request.pageIndex = event.pageIndex;
    this.request.pageSize = event.pageSize;
    this.request.totalItems = event.length;
    this.loadBooks();
  }

  trackById(index: number, book: Book) {
    return book.id;
  }
}
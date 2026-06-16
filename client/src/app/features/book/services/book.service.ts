import {Injectable, signal}  from '@angular/core';
import {Book} from '../models/book.model';
import { environment } from '../../../../environments/environment';
import { HttpClient, HttpParams } from '@angular/common/http';
import { BookPaging } from '../models/bookPagin.model';
import { BookSearchRequest } from '../models/bookSearchRequest';

@Injectable({
  providedIn: 'root'
})
export class BookService {

  private baseUrl = environment.apiBaseUrl + '/book';

  constructor(private http:HttpClient){}


  // state
  books = signal<Book[]>([]);

  getBooks(request: BookSearchRequest)
  {
    const params = new HttpParams()
      .set('pageIndex', request.pageIndex.toString())
      .set('pageSize', request.pageSize.toString())
      .set('searchText', request.SearchText ?? '');

    return this.http.get<BookPaging>(
      this.baseUrl,
      { params }
    );
  }

  addBook(book:Book): void {
    this.books.update(currentBooks => [... currentBooks, book]);
  }
}

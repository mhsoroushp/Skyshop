import { Book } from "./book.model";

export interface BookPaging {
  hasPreviousPage: boolean;
  hasNextPage: boolean;
  totalPages: number;
  pageIndex: number;
  pageSize: number;
  totalItems: number;
  items: Book[];
}
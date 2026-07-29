export interface BookSearchRequest {
  pageIndex: number;
  pageSize: number;
  totalItems: number;
  SearchText?: string;
}
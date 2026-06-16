export interface BookSearchRequest {
  pageIndex: number;
  pageSize: number;
  totalCount: number;
  SearchText?: string;
}
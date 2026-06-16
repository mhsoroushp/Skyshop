import { Injectable } from "@angular/core";

@Injectable({
  providedIn: 'root'
})
export class BaseService {

    isLoggedIn(): boolean {
        return true; // assume user is not logged in for this example
    }
}
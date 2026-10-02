import { Injectable, signal } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { User } from '../models/user.model';
import { environment } from '../../../environments/environment';
import { tap } from 'rxjs/internal/operators/tap';
import { map } from 'rxjs/internal/operators/map';


@Injectable({
  providedIn: 'root'
})
export class UserService {
    private readonly apiBaseUrl = environment.apiBaseUrl;
    private _userSignal = signal<User[]>([]);
    public userSignal = this._userSignal.asReadonly();

    constructor(private http: HttpClient) {}

    getAllUsers() {
        return this.http.get<User[]>(`${this.apiBaseUrl}user`).pipe(
        map(users => users.map(user => ({
          id: user.id,
          firstName: user.firstName ?? 'Hadi',
          lastName: user.lastName ?? 'Soroush',
          email: user.email ?? user.email ?? '',
          roles: user.roles,
          assignedRoles: user.assignedRoles ?? [],
          remainingRoles: user.remainingRoles ?? []
        } as User))),
        tap(mappedUsers => {
          this._userSignal.set(mappedUsers);
        })
      );
    }

    removeUserRole(userId: string, role: string) {
        const params = new HttpParams()
            .set('userId', userId)
            .set('role', role);

        return this.http.delete(`${this.apiBaseUrl}user`, { params });
    }

    getRoles() {
        return this.http.get<string[]>(`${this.apiBaseUrl}user/getRoles`);
    }

    addUserRole(userId: string, role: string) {
      const body = { userId, role };
      return this.http.post(`${this.apiBaseUrl}user/addRole`, body);
    }

}
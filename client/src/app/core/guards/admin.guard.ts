import { CanActivateFn, Router } from "@angular/router";
import { AuthService } from "../services/auth.service";
import { inject } from "@angular/core/primitives/di";

export const adminGuard: CanActivateFn = () => {

    const authService = inject(AuthService);
    const router = inject(Router);

    /*if (authService.isLoggedIn() && authService.authState()?.roles?.some(role => role.toLowerCase().includes('admin'))){
        return true;
    }
    */
    return true;    

    return router.createUrlTree(['/']);

};
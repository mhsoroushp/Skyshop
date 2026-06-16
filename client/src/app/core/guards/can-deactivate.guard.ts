import { CanDeactivateFn } from "@angular/router";
import { Deactivate } from "../models/deactivate.model";

export const canDeactivateGuard: CanDeactivateFn<Deactivate> = 
(component: Deactivate): boolean => {
    return component.canDeactivate() ? true
    : confirm('You have unsaved changes. Do you really want to leave?');
};
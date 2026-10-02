import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'userInitials',
  standalone: true
})
export class UserInitialsPipe implements PipeTransform {
  transform(fullName: string | null | undefined): string {
    if (!fullName) {
      return '';
    }

    const parts = fullName
      .trim()
      .split(/\s+/)
      .filter(Boolean);

    if (parts.length === 0) {
      return '';
    }

    if (parts.length === 1) {
      return parts[0].charAt(0).toUpperCase();
    }

    const firstInitial = parts[0].charAt(0);
    const lastInitial = parts[parts.length - 1].charAt(0);

    return `${firstInitial}${lastInitial}`.toUpperCase();
  }
}

export interface User{
  id: string;
  email: string;   
  firstName: string | null;
  lastName: string | null;
  roles: string[];
  assignedRoles: string[];
  remainingRoles: string[];
}
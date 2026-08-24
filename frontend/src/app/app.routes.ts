import { Routes } from '@angular/router';
import { LoginComponent } from './pages/public/login/login';
import { TaskListComponent } from './pages/private/task/list/task-list';
import { TaskFormComponent } from './pages/private/task/form/task-form';
import { authGuard } from './guards/auth.guard';

export const routes: Routes = [
  { path: '', component: LoginComponent },
  { path: 'tasks', component: TaskListComponent, canActivate: [authGuard] },
  { path: 'tasks/new', component: TaskFormComponent, canActivate: [authGuard] },
  { path: 'tasks/edit/:id', component: TaskFormComponent, canActivate: [authGuard] },
];

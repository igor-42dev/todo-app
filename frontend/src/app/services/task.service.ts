import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { Task } from '../models/task.model';

@Injectable({ providedIn: 'root' })
export class TaskService {
  private http = inject(HttpClient);
  private baseUrl = environment.apiUrl;

  getAll(): Observable<Task[]> {
    return this.http.get<Task[]>(`${this.baseUrl}/api/task`);
  }

  getById(id: string): Observable<Task> {
    return this.http.get<Task>(`${this.baseUrl}/api/task/${id}`);
  }

  create(data: { descricao: string }): Observable<Task> {
    return this.http.post<Task>(`${this.baseUrl}/api/task`, data);
  }

  update(id: string, data: { descricao: string }): Observable<Task> {
    return this.http.put<Task>(`${this.baseUrl}/api/task/${id}`, data);
  }
}

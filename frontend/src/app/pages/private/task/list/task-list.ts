import { Component, inject, OnInit } from '@angular/core';
import { NgFor, NgIf } from '@angular/common';
import { Router } from '@angular/router';
import { TaskService } from '../../../../services/task.service';
import { Task } from '../../../../models/task.model';

@Component({
  selector: 'app-task-list',
  standalone: true,
  imports: [NgFor, NgIf],
  templateUrl: './task-list.html',
  styleUrl: './task-list.scss',
})
export class TaskListComponent implements OnInit {
  private taskService = inject(TaskService);
  private router = inject(Router);

  tasks: Task[] = [];
  errorMessage = '';

  ngOnInit(): void {
    this.loadTasks();
  }

  loadTasks(): void {
    this.taskService.getAll().subscribe({
      next: (tasks) => {
        this.tasks = tasks;
      },
      error: (err) => {
        console.error('Error loading tasks', err);
        this.errorMessage = 'Erro ao carregar tarefas';
      },
    });
  }

  onNew(): void {
    this.router.navigate(['/tasks/new']);
  }

  onEdit(task: Task): void {
    this.router.navigate(['/tasks/edit', task.id]);
  }

  onDelete(task: Task): void {
    console.log('Delete task', task);
  }
}

import { Component, inject, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { NgIf } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { TaskService } from '../../../../services/task.service';

@Component({
  selector: 'app-task-form',
  standalone: true,
  imports: [FormsModule, NgIf],
  templateUrl: './task-form.html',
  styleUrl: './task-form.scss',
})
export class TaskFormComponent implements OnInit {
  private taskService = inject(TaskService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);

  taskId: string | null = null;
  descricao = '';
  errorMessage = '';
  isEditing = false;

  ngOnInit(): void {
    this.taskId = this.route.snapshot.paramMap.get('id');
    if (this.taskId) {
      this.isEditing = true;
      this.loadTask(this.taskId);
    }
  }

  loadTask(id: string): void {
    this.taskService.getById(id).subscribe({
      next: (task) => {
        this.descricao = task.descricao;
      },
      error: (err) => {
        console.error('Error loading task', err);
        this.errorMessage = 'Erro ao carregar tarefa';
      },
    });
  }

  onSubmit(): void {
    this.errorMessage = '';
    if (this.isEditing && this.taskId) {
      this.taskService.update(this.taskId, { descricao: this.descricao }).subscribe({
        next: () => {
          this.router.navigate(['/tasks']);
        },
        error: (err) => {
          console.error('Error updating task', err);
          this.errorMessage = 'Erro ao atualizar tarefa';
        },
      });
    } else {
      this.taskService.create({ descricao: this.descricao }).subscribe({
        next: () => {
          this.router.navigate(['/tasks']);
        },
        error: (err) => {
          console.error('Error creating task', err);
          this.errorMessage = 'Erro ao criar tarefa';
        },
      });
    }
  }

  onCancel(): void {
    this.router.navigate(['/tasks']);
  }
}

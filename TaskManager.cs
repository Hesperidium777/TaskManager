using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace TaskManagerApp
{
    // Класс для представления задачи
    public class TaskItem
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? Deadline { get; set; }
        public bool IsCompleted { get; set; }
        public Priority Priority { get; set; }
        public string Category { get; set; }

        public TaskItem()
        {
            CreatedDate = DateTime.Now;
            IsCompleted = false;
            Priority = Priority.Medium;
        }

        public override string ToString()
        {
            string status = IsCompleted ? "[✓]" : "[ ]";
            string deadlineInfo = Deadline.HasValue ? 
                $" (до {Deadline.Value:dd.MM.yyyy})" : "";
            return $"{Id}. {status} {Description} [{Priority}] {deadlineInfo}";
        }
    }

    public enum Priority
    {
        Low,
        Medium,
        High
    }

    // Основной класс менеджера задач
    public class TaskManager
    {
        private List<TaskItem> tasks;
        private int nextId;
        private string dataFilePath;

        public TaskManager(string filePath = "tasks.json")
        {
            dataFilePath = filePath;
            tasks = new List<TaskItem>();
            nextId = 1;
            LoadTasks();
        }

        // Загрузка задач из файла
        private void LoadTasks()
        {
            if (File.Exists(dataFilePath))
            {
                try
                {
                    string json = File.ReadAllText(dataFilePath);
                    tasks = JsonSerializer.Deserialize<List<TaskItem>>(json);
                    
                    if (tasks.Any())
                    {
                        nextId = tasks.Max(t => t.Id) + 1;
                    }
                    
                    Console.WriteLine($"Загружено {tasks.Count} задач.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка загрузки: {ex.Message}");
                    tasks = new List<TaskItem>();
                }
            }
        }

        // Сохранение задач в файл
        private void SaveTasks()
        {
            try
            {
                string json = JsonSerializer.Serialize(tasks, 
                    new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(dataFilePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка сохранения: {ex.Message}");
            }
        }

        // Добавление новой задачи
        public void AddTask()
        {
            Console.Clear();
            Console.WriteLine("=== Добавление новой задачи ===");
            
            Console.Write("Описание задачи: ");
            string description = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(description))
            {
                Console.WriteLine("Описание не может быть пустым!");
                return;
            }

            TaskItem task = new TaskItem
            {
                Id = nextId++,
                Description = description
            };

            // Установка дедлайна
            Console.Write("Установить дедлайн? (y/n): ");
            if (Console.ReadLine().ToLower() == "y")
            {
                Console.Write("Дата (дд.мм.гггг): ");
                if (DateTime.TryParse(Console.ReadLine(), out DateTime deadline))
                {
                    task.Deadline = deadline;
                }
            }

            // Установка приоритета
            Console.WriteLine("Выберите приоритет:");
            Console.WriteLine("1. Низкий");
            Console.WriteLine("2. Средний");
            Console.WriteLine("3. Высокий");
            Console.Write("Ваш выбор (1-3): ");
            
            string priorityChoice = Console.ReadLine();
            task.Priority = priorityChoice switch
            {
                "1" => Priority.Low,
                "3" => Priority.High,
                _ => Priority.Medium
            };

            // Установка категории
            Console.Write("Категория (нажмите Enter чтобы пропустить): ");
            string category = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(category))
            {
                task.Category = category;
            }

            tasks.Add(task);
            SaveTasks();
            Console.WriteLine($"Задача добавлена (ID: {task.Id})!");
        }

        // Просмотр всех задач
        public void ViewTasks(string filter = "all")
        {
            Console.Clear();
            
            var filteredTasks = filter switch
            {
                "active" => tasks.Where(t => !t.IsCompleted),
                "completed" => tasks.Where(t => t.IsCompleted),
                "today" => tasks.Where(t => !t.IsCompleted && 
                    t.Deadline.HasValue && 
                    t.Deadline.Value.Date == DateTime.Today),
                _ => tasks.AsEnumerable()
            };

            if (!filteredTasks.Any())
            {
                Console.WriteLine("Задачи не найдены.");
                return;
            }

            Console.WriteLine($"=== Список задач ({filter}) ===");
            
            // Группировка по категориям
            var groupedTasks = filteredTasks
                .GroupBy(t => t.Category ?? "Без категории")
                .OrderBy(g => g.Key);

            foreach (var group in groupedTasks)
            {
                Console.WriteLine($"\n--- {group.Key} ---");
                foreach (var task in group.OrderBy(t => t.Priority).ThenBy(t => t.Deadline))
                {
                    Console.WriteLine(task);
                    
                    // Проверка просроченных задач
                    if (!task.IsCompleted && 
                        task.Deadline.HasValue && 
                        task.Deadline.Value < DateTime.Now)
                    {
                        Console.WriteLine("  ⚠ ПРОСРОЧЕНО!");
                    }
                }
            }

            // Статистика
            Console.WriteLine($"\n--- Статистика ---");
            Console.WriteLine($"Всего задач: {tasks.Count}");
            Console.WriteLine($"Выполнено: {tasks.Count(t => t.IsCompleted)}");
            Console.WriteLine($"Активных: {tasks.Count(t => !t.IsCompleted)}");
            Console.WriteLine($"Просрочено: {tasks.Count(t => 
                !t.IsCompleted && t.Deadline.HasValue && t.Deadline.Value < DateTime.Now)}");
        }

        // Отметка задачи как выполненной
        public void CompleteTask()
        {
            Console.Clear();
            ViewTasks("active");
            
            if (!tasks.Any(t => !t.IsCompleted))
            {
                Console.WriteLine("\nНет активных задач.");
                return;
            }

            Console.Write("\nВведите ID задачи для отметки как выполненной: ");
            if (int.TryParse(Console.ReadLine(), out int id))
            {
                var task = tasks.FirstOrDefault(t => t.Id == id);
                if (task != null)
                {
                    task.IsCompleted = true;
                    SaveTasks();
                    Console.WriteLine($"Задача {id} отмечена как выполненная!");
                }
                else
                {
                    Console.WriteLine("Задача не найдена.");
                }
            }
        }

        // Удаление задачи
        public void DeleteTask()
        {
            Console.Clear();
            ViewTasks();
            
            if (!tasks.Any())
            {
                Console.WriteLine("\nНет задач для удаления.");
                return;
            }

            Console.Write("\nВведите ID задачи для удаления: ");
            if (int.TryParse(Console.ReadLine(), out int id))
            {
                var task = tasks.FirstOrDefault(t => t.Id == id);
                if (task != null)
                {
                    tasks.Remove(task);
                    SaveTasks();
                    Console.WriteLine($"Задача {id} удалена!");
                }
                else
                {
                    Console.WriteLine("Задача не найдена.");
                }
            }
        }

        // Редактирование задачи
        public void EditTask()
        {
            Console.Clear();
            ViewTasks();
            
            if (!tasks.Any())
            {
                Console.WriteLine("\nНет задач для редактирования.");
                return;
            }

            Console.Write("\nВведите ID задачи для редактирования: ");
            if (int.TryParse(Console.ReadLine(), out int id))
            {
                var task = tasks.FirstOrDefault(t => t.Id == id);
                if (task != null)
                {
                    Console.WriteLine($"Текущее описание: {task.Description}");
                    Console.Write("Новое описание (нажмите Enter чтобы оставить текущее): ");
                    string newDescription = Console.ReadLine();
                    if (!string.IsNullOrWhiteSpace(newDescription))
                    {
                        task.Description = newDescription;
                    }

                    Console.Write("Изменить статус выполнения? (y/n): ");
                    if (Console.ReadLine().ToLower() == "y")
                    {
                        task.IsCompleted = !task.IsCompleted;
                    }

                    SaveTasks();
                    Console.WriteLine("Задача обновлена!");
                }
                else
                {
                    Console.WriteLine("Задача не найдена.");
                }
            }
        }

        // Поиск задач
        public void SearchTasks()
        {
            Console.Clear();
            Console.Write("Введите текст для поиска: ");
            string searchText = Console.ReadLine().ToLower();

            var results = tasks.Where(t => 
                t.Description.ToLower().Contains(searchText) ||
                (t.Category?.ToLower().Contains(searchText) ?? false));

            if (results.Any())
            {
                Console.WriteLine($"=== Результаты поиска '{searchText}' ===");
                foreach (var task in results)
                {
                    Console.WriteLine(task);
                }
                Console.WriteLine($"Найдено: {results.Count()} задач");
            }
            else
            {
                Console.WriteLine("Задачи не найдены.");
            }
        }

        // Экспорт в текстовый файл
        public void ExportToTextFile()
        {
            string exportPath = $"tasks_export_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
            
            using (StreamWriter writer = new StreamWriter(exportPath))
            {
                writer.WriteLine($"Экспорт задач: {DateTime.Now}");
                writer.WriteLine("=".PadRight(50, '='));
                
                foreach (var task in tasks)
                {
                    writer.WriteLine(task.ToString());
                }
                
                writer.WriteLine("\nСтатистика:");
                writer.WriteLine($"Всего задач: {tasks.Count}");
                writer.WriteLine($"Выполнено: {tasks.Count(t => t.IsCompleted)}");
            }

            Console.WriteLine($"Задачи экспортированы в файл: {exportPath}");
        }
    }
}
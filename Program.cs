using System;

namespace TaskManagerApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Менеджер задач v1.0";
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            
            TaskManager taskManager = new TaskManager();
            
            bool exit = false;
            
            while (!exit)
            {
                Console.Clear();
                ShowMenu();
                Console.Write("\nВыберите действие (1-9): ");
                
                string choice = Console.ReadLine();
                
                switch (choice)
                {
                    case "1":
                        taskManager.ViewTasks();
                        break;
                    case "2":
                        taskManager.ViewTasks("active");
                        break;
                    case "3":
                        taskManager.ViewTasks("today");
                        break;
                    case "4":
                        taskManager.AddTask();
                        break;
                    case "5":
                        taskManager.CompleteTask();
                        break;
                    case "6":
                        taskManager.EditTask();
                        break;
                    case "7":
                        taskManager.DeleteTask();
                        break;
                    case "8":
                        taskManager.SearchTasks();
                        break;
                    case "9":
                        taskManager.ExportToTextFile();
                        break;
                    case "0":
                        exit = true;
                        Console.WriteLine("До свидания!");
                        continue;
                    default:
                        Console.WriteLine("Неверный выбор!");
                        break;
                }
                
                if (!exit)
                {
                    Console.WriteLine("\nНажмите любую клавишу для продолжения...");
                    Console.ReadKey();
                }
            }
        }

        static void ShowMenu()
        {
            Console.WriteLine("=== МЕНЕДЖЕР ЗАДАЧ ===");
            Console.WriteLine("1. Показать все задачи");
            Console.WriteLine("2. Показать активные задачи");
            Console.WriteLine("3. Показать задачи на сегодня");
            Console.WriteLine("4. Добавить новую задачу");
            Console.WriteLine("5. Отметить задачу как выполненную");
            Console.WriteLine("6. Редактировать задачу");
            Console.WriteLine("7. Удалить задачу");
            Console.WriteLine("8. Поиск задач");
            Console.WriteLine("9. Экспорт в текстовый файл");
            Console.WriteLine("0. Выход");
            Console.WriteLine("=".PadRight(30, '='));
        }
    }
}
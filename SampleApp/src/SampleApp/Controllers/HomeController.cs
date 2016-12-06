using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Raven.NewClient.Client.Document;
using SampleApp.Models;
using System;
using Raven.NewClient.Client;
using SampleApp.ViewModels;

namespace SampleApp.Controllers
{
    public static class DocumentStoreHolder
    {
        private static readonly Lazy<IDocumentStore> LazyStore = 
            new Lazy<IDocumentStore>(() =>
            {
                var store = new DocumentStore
                {
                    Url = "http://localhost:8080",
                    DefaultDatabase = "MyTasksDatabase"
                };

                //using (var client = new HttpClient())
                //{
                //    // TODO: new client isn't setup to create it yet
                //    client.PutAsync("http://localhost:8080/databases/MyTasksDatabase",
                //            new StringContent(@"{'Settings': {'Raven/DataDir': '~/MyTasksDatabase'} "))
                //        .ConfigureAwait(false)
                //        .GetAwaiter().GetResult();
                //}

                return store.Initialize();
            });

        public static IDocumentStore Store => LazyStore.Value;
    }

    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            // 1. Get Data
            List<MyTask> tasksList = new List<MyTask>();
            List<MyTaskViewModel> tasksListForView = new List<MyTaskViewModel>();

            using (var session = DocumentStoreHolder.Store.OpenSession())
            {
                tasksList = session.Query<MyTask>().ToList();
            }

            // 2. Create Sample Data if does not exist yet
            if (tasksList.Count == 0)
            {
                using (var session = DocumentStoreHolder.Store.OpenSession())
                {
                    for (int i = 0; i < 5; i++)
                    {
                        string taskStr = "Task number " + i.ToString();
                        var sampleTask = new MyTask {TaskToDo = taskStr};
                        session.Store(sampleTask);

                        var SampleTaskForView = new MyTaskViewModel { Name = "MyTasks", Id = i.ToString(), TaskToDo = taskStr};
                        tasksListForView.Add(SampleTaskForView);
                    }
                    session.SaveChanges();
                }
            }
            else
            {
                // 3. Manage view model data
                tasksList.ForEach(x => tasksListForView.Add(new MyTaskViewModel {Id = x.Id.Split('/')[1], Name = "MyTasks", TaskToDo = x.TaskToDo}));
            }

            return View(tasksListForView);
        }

        public IActionResult AddItem(string NewTask) 
        {
            if (String.IsNullOrWhiteSpace(NewTask))
            {
                return Redirect("/");
            }

            using (var session = DocumentStoreHolder.Store.OpenSession())
            {
                var newTask = new MyTask { TaskToDo = NewTask };
                session.Store(newTask);
                session.SaveChanges();
            }

            return Redirect("/");
        }

        public IActionResult RemoveItem(string name, string id)
        {
            using (var session = DocumentStoreHolder.Store.OpenSession())
            {
                session.Delete($"{name}/{id}");
                session.SaveChanges();
            }

            return Redirect("/");
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }

}

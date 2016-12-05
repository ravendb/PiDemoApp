using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Raven.NewClient.Client.Document;
using SampleApp.Models;
using System;
using System.Net.Http;
using Raven.NewClient.Client;

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
            // Get Data
            List<MyTask> tasksList = new List<MyTask>();
            using (var session = DocumentStoreHolder.Store.OpenSession())
            {
                tasksList = session.Query<MyTask>().ToList();
            }

            // Create Sample Data if does not exists yet
            if (tasksList.Count == 0)
            {
                using (var session = DocumentStoreHolder.Store.OpenSession())
                {
                    for (int i = 0; i < 5; i++)
                    {
                        var user = new MyTask {TaskToDo = "Task number " + i};
                        session.Store(user);
                    }
                    session.SaveChanges();
                }

                // Get the new created data and pass to the view
                using (var session = DocumentStoreHolder.Store.OpenSession())
                {
                    tasksList = session.Query<MyTask>().ToList();
                }
            }

            return View(tasksList);
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

        public IActionResult RemoveItem(int id)
        {
            // TODO: need to implement from the UI

            using (var session = DocumentStoreHolder.Store.OpenSession())
            {
                session.Delete($"MyTasks/{id}");
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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Dojodachi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Dojodachi.Controllers
{
    public class HomeController : Controller
    {

        private bool IsHappy()
        {
            Random rand = new Random();
            int Result = rand.Next(0, 4);
            if (Result == 0) { return false; }
            return true;
        }

        private bool IsDead()
        {
            int? Fullness = HttpContext.Session.GetInt32("Fullness");
            int? Happiness = HttpContext.Session.GetInt32("Happiness");
            if (Fullness <= 0 || Happiness <= 0) { return true; }
            return false;
        }

        private bool DidWin()
        {
            int? Energy = HttpContext.Session.GetInt32("Energy");
            int? Fullness = HttpContext.Session.GetInt32("Fullness");
            int? Happiness = HttpContext.Session.GetInt32("Happiness");

            return Energy > 100 && Fullness > 100 && Happiness > 100;
        }

        // GET -> Index
        [HttpGet("")]
        public IActionResult Index()
        {
            // Initialize Dojodachi:
            if (HttpContext.Session.GetInt32("Fullness") == null)
            {
                HttpContext.Session.SetInt32("Fullness", 20);
                HttpContext.Session.SetInt32("Happiness", 20);
                HttpContext.Session.SetInt32("Energy", 50);
                HttpContext.Session.SetInt32("Meals", 3);
                // State can be "Happy", "Angry", "Sad"
                HttpContext.Session.SetString("State", "Happy");
                HttpContext.Session.SetString("Message", "Get your Dojodachi's energy over 100 to win.");
                HttpContext.Session.SetString("GameState", "ongoing");
            }

            ViewBag.Happiness = HttpContext.Session.GetInt32("Happiness");
            ViewBag.Fullness = HttpContext.Session.GetInt32("Fullness");
            ViewBag.Energy = HttpContext.Session.GetInt32("Energy");
            ViewBag.Meals = HttpContext.Session.GetInt32("Meals");
            ViewBag.State = HttpContext.Session.GetString("State");
            ViewBag.Message = HttpContext.Session.GetString("Message");
            ViewBag.GameState = HttpContext.Session.GetString("GameState");

            return View();
        }

        // POST
        [HttpPost("")]
        public IActionResult HandleInput()
        {

            Random rand = new Random();

            if (Request.Form.ContainsKey("Feed"))
            {
                int? Meals = HttpContext.Session.GetInt32("Meals");
                int NumMeals = Meals ?? default(int);

                if (NumMeals > 0)
                {
                    NumMeals -= 1;
                    HttpContext.Session.SetInt32("Meals", NumMeals);

                    bool Happy = IsHappy();
                    if (Happy)
                    {
                        int AddToFullness = rand.Next(5, 11);
                        int? Fullness = HttpContext.Session.GetInt32("Fullness");
                        int NewFullness = Fullness ?? default(int);
                        NewFullness += AddToFullness;
                        HttpContext.Session.SetInt32("Fullness", NewFullness);
                        HttpContext.Session.SetString("State", "Happy");
                        HttpContext.Session.SetString("Message", $"You feed your Dojodachi a meal. It gained {AddToFullness} fullness and feels happy!");
                    }
                    else
                    {
                        HttpContext.Session.SetString("State", "Sad");
                        HttpContext.Session.SetString("Message", $"You feed your Dojodachi a meal. It got an upset tummy and feels sad.");
                    }

                }
                else
                {
                    HttpContext.Session.SetString("Message", "You don't have any food!");
                }

            }

            else if (Request.Form.ContainsKey("Play"))
            {
                Console.WriteLine("Play");
                int? Energy = HttpContext.Session.GetInt32("Energy");
                int NewEnergy = Energy ?? default(int);
                NewEnergy -= 5;
                HttpContext.Session.SetInt32("Energy", NewEnergy);
                bool Happy = IsHappy();
                if (Happy)
                {
                    int AddToHappiness = rand.Next(5, 11);
                    int? Happiness = HttpContext.Session.GetInt32("Happiness");
                    int NewHappiness = Happiness ?? default(int);
                    HttpContext.Session.SetInt32("Happiness", NewHappiness + AddToHappiness);
                    HttpContext.Session.SetString("State", "Happy");
                    HttpContext.Session.SetString("Message", $"You play with your Dojodachi. It gained {AddToHappiness} happiness and feels happy!");
                }
                else
                {
                    HttpContext.Session.SetString("State", "Mad");
                    HttpContext.Session.SetString("Message", $"You play a game with your Dojodachi. It loses and gets mad!");
                }

            }

            else if (Request.Form.ContainsKey("Work"))
            {
                Console.WriteLine("Work");
                int? Energy = HttpContext.Session.GetInt32("Energy");
                int NewEnergy = Energy ?? default(int);
                HttpContext.Session.SetInt32("Energy", NewEnergy -= 5);

                int? Meals = HttpContext.Session.GetInt32("Meals");
                int NewMeals = Meals ?? default(int);
                int AddToMeals = rand.Next(1, 4);
                HttpContext.Session.SetInt32("Meals", NewMeals + AddToMeals);
                HttpContext.Session.SetString("Message", $"You worked, spending 5 energy, and earned {AddToMeals} meals.");
            }

            else if (Request.Form.ContainsKey("Sleep"))
            {
                Console.WriteLine("Sleep");
                int? Energy = HttpContext.Session.GetInt32("Energy");
                int? Happiness = HttpContext.Session.GetInt32("Happiness");
                int? Fullness = HttpContext.Session.GetInt32("Fullness");

                int NewEnergy = Energy ?? default(int);
                int NewHappinness = Happiness ?? default(int);
                int NewFullness = Fullness ?? default(int);

                NewEnergy += 15;
                NewHappinness -= 5;
                NewFullness -= 5;

                HttpContext.Session.SetInt32("Energy", NewEnergy);
                HttpContext.Session.SetInt32("Happiness", NewHappinness);
                HttpContext.Session.SetInt32("Fullness", NewFullness);

                HttpContext.Session.SetString("Message", "You make your Dojodachi take a nap. It gained 15 energy and lost 5 fullness and 5 happiness.");

            }

            else if (Request.Form.ContainsKey("Restart"))
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Index");
            }

            if (IsDead())
            {
                HttpContext.Session.SetString("GameState", "lost");
                HttpContext.Session.SetString("State", "Sad");
                HttpContext.Session.SetString("Message", "Your Dojodachi has kicked the proverbial bucket...");
            }
            else if (DidWin())
            {
                HttpContext.Session.SetString("GameState", "Won");
                HttpContext.Session.SetString("State", "Happy");
                HttpContext.Session.SetString("Message", "Congratulations! You won!");
            }

            return RedirectToAction("Index");

        }








        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

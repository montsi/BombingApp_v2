using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace BombingApp_v2
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class GamePage : Page
    { 
        private int Score { get; set; }         // Muuttuja pisteille
        private int TankCount { get; set; }     // Muuttujat tankkien, jalkaväen, ja pommien määrälle
        private int InfantryCount { get; set; } 
        private int BombsLeft = 30;             

        private List<Bomb> bombs;               // Listat
        private List<Infantry> ukot;
        private List<Tank> tanks;

        private DispatcherTimer gameTimer;      // Ajastimet
        private DispatcherTimer ukkoTimer;
        private DispatcherTimer tankTimer;

        public GamePage()
        {
            this.InitializeComponent();
            // Ikkunan koko käynnistyksessä
            ApplicationView.PreferredLaunchViewSize = new Size(1280, 720);
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;

            // luodaan listat
            bombs = new List<Bomb>();
            ukot = new List<Infantry>();
            tanks = new List<Tank>();

            // luodaan ensimmäiset viholliset
            createInfantry();
            createTank();

            // hiiren kuuntelu/tunnistus
            Window.Current.CoreWindow.PointerPressed += CoreWindow_PointerPressed;

            // ajastin jalkaväelle
            ukkoTimer = new DispatcherTimer();
            ukkoTimer.Interval = new TimeSpan(0, 0, 0, 1);
            ukkoTimer.Tick += UkkoTimer_Tick;
            ukkoTimer.Start();

            // ajastin tankeille
            tankTimer = new DispatcherTimer();
            tankTimer.Interval = new TimeSpan(0, 0, 0, 6);
            tankTimer.Tick += TankTimer_Tick1;
            tankTimer.Start();

            // peliajastin
            gameTimer = new DispatcherTimer();
            gameTimer.Interval = new TimeSpan(0, 0, 0, 0, 1000 / 60);
            gameTimer.Tick += GameTimer_Tick1;
            gameTimer.Start();
        }
        private void UkkoTimer_Tick(object sender, object e)
        {
            // luodaan lisää jalkaväkeä, mikäli niitä ei ole luotu vielä 20:ta
            if (InfantryCount < 25)
            {
                createInfantry();
            }
        }
        private void TankTimer_Tick1(object sender, object e)
        {
            // luodaan lisää jalkaväkeä, mikäli viittä ei ole luotu vielä
            if (TankCount < 5)
            {
                createTank();
            }
        }
        private void GameTimer_Tick1(object sender, object e)
        {
            // poistolista jalkaväelle
            List<Infantry> remove = new List<Infantry>();
            // siirretään jalkaväki poistolistalle, mikäli ylittää pisteen X >= 1000
            foreach (Infantry ukko in ukot)
            {
                ukko.Move();
                if (ukko.LocationX >= 1000)
                {
                    remove.Add(ukko);
                }
            }
            // poistetaan kaikki poistolistalla olevat
            foreach (Infantry ukko in remove)
            {
                ukot.Remove(ukko);
                myCanvas.Children.Remove(ukko);
            }
            remove.Clear();

            // poistolista tankeille
            List<Tank> removeT = new List<Tank>();
            // siirretään tankki poistolistalle, mikäli ylittää pisteen X >= 1000
            foreach (Tank tank in tanks)
            {
                tank.Move();
                if (tank.LocationX >= 1000)
                {
                    removeT.Add(tank);
                }
            }
            // poistetaan kaikki poistolistalla olevat
            foreach (Tank tank in removeT)
            {
                tanks.Remove(tank);
                myCanvas.Children.Remove(tank);
            }
            removeT.Clear();
            // jos kaikki pommit on käytetty pysäytetään ajastimet
            if (BombsLeft == 0)
            {
                gameTimer.Stop();
                ukkoTimer.Stop();
                tankTimer.Stop();
            }
        }

        private void CoreWindow_PointerPressed(Windows.UI.Core.CoreWindow sender, Windows.UI.Core.PointerEventArgs args)
        {
            // jos kaikkia pommeja ei ole käytetty luodaan lisää hiirenpainalluksella, vähennetään käytettävien määrästä ja tarkistetaan osuma
            if (BombsLeft > 0)
            {
                Bomb bomb = new Bomb();
                bomb.LocationX = args.CurrentPoint.Position.X - bomb.Width / 2;
                bomb.LocationY = args.CurrentPoint.Position.Y - bomb.Width / 2;
                myCanvas.Children.Add(bomb);
                bomb.SetLocation();
                bombs.Add(bomb);
                BombsLeft--;
                bombsBlock.Text = BombsLeft.ToString();
                CheckCollision(bomb);
            }
        }

        private void createInfantry()
        {
            // jalkaväen luontikoodi - sijoitetaan satunnaisesti ja kasvatetaan jalkaväen määrä -muuttujaa
            Infantry ukko = new Infantry();
            Random random = new Random();
            ukko.LocationY = random.Next(50, 720 - 120);
            ukko.SetLocation();
            ukot.Add(ukko);
            myCanvas.Children.Add(ukko);
            InfantryCount++;
        }

        private void createTank()
        {
            // tankkien luontikoodi - sijoitetaan satunnaisesti ja kasvatetaan jalkaväen määrä -muuttujaa
            Tank tank = new Tank();
            Random random = new Random();
            tank.LocationY = random.Next(50, 720 - 100);
            tank.SetLocation();
            tanks.Add(tank);
            myCanvas.Children.Add(tank);
            TankCount++;
        }

        private void CheckCollision(Bomb bomb)
        {
            foreach (Infantry ukko in ukot)
            {
                // haetaan pommin ja jalkaväen alueet
                Rect BRect = new Rect(
                    bomb.LocationX, bomb.LocationY, bomb.ActualWidth, bomb.ActualHeight
                    );
                Rect IRect = new Rect(
                    ukko.LocationX, ukko.LocationY, ukko.ActualWidth, ukko.ActualHeight
                    );
                // tarkastetaan alueiden yhteenosuma
                BRect.Intersect(IRect);

                if (!BRect.IsEmpty)
                {
                    // osuma tapahtui - poistetaan jalkaväki - lisätään pisteitä
                    myCanvas.Children.Remove(ukko);
                    ukot.Remove(ukko);
                    Score += 50;
                    scoreBlock.Text = Score.ToString();
                    if (Score > App.Highscore)
                    {
                        App.Highscore = Score;
                        highscoreBlock.Text = Score.ToString();
                    }
                    // play audio
                    //mediaElement.Play();
                    break;
                }
            }

            foreach (Tank tank in tanks)
            {
                // haetaan pommin ja tankin alueet
                Rect BRect = new Rect(
                    bomb.LocationX, bomb.LocationY, bomb.ActualWidth, bomb.ActualHeight
                    );
                Rect TRect = new Rect(
                    tank.LocationX, tank.LocationY, tank.ActualWidth, tank.ActualHeight
                    );
                // tarkastetaan alueiden yhteenosuma
                BRect.Intersect(TRect);

                if (!BRect.IsEmpty)
                {
                    // osuma tapahtui - poistetaan tankki - lisätään pisteitä
                    myCanvas.Children.Remove(tank);
                    tanks.Remove(tank);
                    Score += 30;
                    if (Score > App.Highscore)
                    {
                        App.Highscore = Score;
                        highscoreBlock.Text = Score.ToString();
                    }
                    break;
                }
            }
        }
        // takaisin aloitussivulle painikkeella
        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(StartPage));
        }
    }
}

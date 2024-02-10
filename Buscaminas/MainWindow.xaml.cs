using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Buscaminas
{
    public partial class MainWindow : Window
    {
        private DispatcherTimer timer;
        private int elapsedSecond;
        private Button[,] boardButtons; // matriz de botones
        private int boardSize = 9; //tamaño del tablero (9x9)
        int flag1 = 0; //banderas colocadas de inicio
        int flag2 = 10; //banderas permitidas
        String relativeBomb = "./img/bomba.png";
        String relativeFlag = "./img/flag.png";
        private int clickCount = 0;
        private bool gameWon = false;
        public MainWindow()
        {

            InitializeComponent();

            InicializeBoard();

            // Inicializa el temporizador
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;

            //poniendole función al boton de reinicio

            resertButton.Click += ResertButton_Click;
        }

        //la función es para que pase el tiempo y lo refleje en el TextBlock
        private void Timer_Tick(object sender, EventArgs e)
        {
            elapsedSecond++;
            TimeSpan timeSpan = TimeSpan.FromSeconds(elapsedSecond);
            timerText.Text = $"{timeSpan.Minutes:00}:{timeSpan.Seconds:00}";
        }

        //Función que inicializa el tablero
        private void InicializeBoard()
        {
            boardButtons = new Button[boardSize, boardSize];

            //generamos los botones y los agregamos al grid

            for (int row = 0; row < boardSize; row++)
            {
                for (int col = 0; col < boardSize; col++)
                {
                    Button button = CreateButton(row, col);
                    boardButtons[row, col] = button;

                    //agragar el botón al Grid
                    boardGrid.Children.Add(button);

                    Grid.SetRow(button, row);

                    Grid.SetColumn(button, col);


                    button.Click += Button_Click;

                    button.MouseRightButtonDown += Buttonflag;


                }
            }

            //Generamos las imagenes de las bombas
            GenerateBombs();
        }


        //Genera las imagenes de las bombas de forma aleatoria
        private Button CreateButton(int row, int col)
        {
            return new Button
            {
                Name = $"cell{row}{col}",
                Content = "",
                IsEnabled = true,
                Style = (Style)FindResource("EstiloBoton"),
            };
        }
     
        private void GenerateBombs()
        {
            Random random = new Random();
            List<Tuple<int, int>> bombPositions = new List<Tuple<int, int>>();

            for (int i = 0; i < 10; i++)
            {
                int row, col;
                do
                {
                    row = random.Next(boardSize);
                    col = random.Next(boardSize);
                }
                while (bombPositions.Contains(Tuple.Create(row, col)));

                bombPositions.Add(Tuple.Create(row, col));

                Image bombImage = CreateBombImage();
                Grid.SetRow(bombImage, row);
                Grid.SetColumn(bombImage, col);
                boardGrid.Children.Add(bombImage);

                boardButtons[row, col].Tag = "Bomb";
            }

        }

        private Image CreateBombImage()
        {
            return new Image

            {
                Source = new BitmapImage(new Uri(relativeBomb, UriKind.Relative)),
                Visibility = Visibility.Hidden,
                Width = 35,
                Height = 35,
             };
        }


        //inicializar el temporizador

        private void StartTimer()
        {
            elapsedSecond = 0;
            timerText.Text = "00:00";
            timer.Start();
        }
        private void StopTimer()
        {
            timer.Stop();
        }
    private void Button_Click(object sender, RoutedEventArgs e)
        {
            clickCount++; // Incrementa el contador de clicks
            Button button = (Button)sender;
            int row = Grid.GetRow(button);
            int col = Grid.GetColumn(button);

            Console.WriteLine("Button_Click2");

            if (boardButtons[row, col].Tag != null)
            {
                //La casilla hace click contiene una bomba
                //Mostrar la imagen de la bomba y realizar otras acciones

                Image bombImage = boardGrid.Children
                    .OfType<Image>()
                    .FirstOrDefault(image => Grid.GetRow(image) == row && Grid.GetColumn(image) == col);

                bombImage.Visibility = Visibility.Visible;
                button.IsHitTestVisible = false;
                timer.Stop();
                gameWon = false;
                ShowAllBombs(); //para mostrar todas las bombas
                ShowGameResult();


            }
            else
            {
            //La casilla hace click no contiene una bomba
            //Revelar el contenido de la casilla, como un número o un espacio vacio
            
                int bombsAdjacent = CountAdjacentBombs(row, col);
                if (bombsAdjacent > 0)
                {
                    button.Content = bombsAdjacent;
                    SetNumberColor(button, bombsAdjacent);
                }
                else
                {
                    //Es para que no me muestre ninguna cifra
                    ExpandCells(row, col);
                }

                //establecer el color de los numero en función de la cantidad

                SetNumberColor(button, bombsAdjacent);

                //cambia el color de fondo

                button.Background = Brushes.LightBlue;
                button.IsHitTestVisible = false;
            }
        }

        //función para que enseñe todas las bombas( se muestra en el momento en el que tocas una bomba)
        private void ShowAllBombs()
        {
            for(int i = 0; i< boardSize; i++) 
            { 
                for (int j = 0; j < boardSize; j++)
                {
                    if (boardButtons[i, j].Tag != null && boardButtons[i, j].Tag.ToString() == "Bomb")
                    {
                        Image bombImage = boardGrid.Children
                            .OfType<Image>()
                            .FirstOrDefault(image => Grid.GetRow(image) == i && Grid.GetColumn(image) == j);
                        if(bombImage != null)
                        {
                            bombImage.Visibility = Visibility.Visible;
                        }
                    }
                }
            }
        }

        //colocacion de banderas
       private void Buttonflag(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;


            if(flag1 < flag2)
            {
                //se colocan las banderas 
                Image flagImage = button.Content as Image;
                if(flagImage == null)
                    
                {
                    flagImage = new Image();
                   
                    flagImage.Source = new BitmapImage(new Uri("./img/flag.png", UriKind.Relative));

                    button.Content = flagImage;
                    flag1++;
                }
                else
                {
                    //cuando quitamos la bandera
                    button.Content = null;
                    flag1--;
                }
                //para contabilizar las banderas que nos quedan
                Bandera.Text = (flag2 - flag1).ToString();
            }
        }

        //mensaje de resultado
        private void ShowGameResult()
        {
            string result = gameWon ? "Has ganado" : "Has perdido ";
            MessageBox.Show($"Número de cliks: {clickCount}\nTiempo: {timerText.Text}\n{result}", "Resultado de la partida", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        //función que nos indica las bombas adyacentes
        private int CountAdjacentBombs(int row, int col)
        {
            int bombCount = 0;

            //Definir las direcciones para verificar las casillas adyacentes
            int[] dx = { -1, -1, -1, 0, 0, 1, 1, 1 };
            int[] dy = { -1, 0, 1, -1, 1, -1, 0, 1 };

            for(int i= 0; i < 8; i++)
            {
                int newRow = row + dx[i];
                int newCol = col + dy[i];

                //verificamos si las casillas adyacentes está dentro de los limites del tablero.

                if(newRow >= 0 && newRow < boardSize && newCol >= 0 && newCol < boardSize)
                {
                    if (boardButtons[newRow, newCol].Tag != null && boardButtons[newRow, newCol].Tag.ToString() == "Bomb") 
                    {
                        bombCount++;
                    }
                }
            }
             return bombCount ;

        }
        
        //función para determinar el color del número de las bombas adyacentes.
        private void SetNumberColor(Button button, int bombCount)
        {
            if(bombCount == 1)
            {
                button.Foreground = Brushes.Blue;
            }
            else if(bombCount == 2)
            {
                button.Foreground= Brushes.Green;
            }else if(bombCount == 3)
            {
                button.Foreground= Brushes.Orange;
            }else if(bombCount == 4)
            {
                button.Foreground= Brushes.Purple;
            }else if(bombCount == 5)
            {
                button.Foreground= Brushes.Red;
            }
        }

        //función para que se abrán las casillas cuando tocamos una que esta vacia
        private void ExpandCells(int row, int col)
        {
            //casillas fuera de los limites del tablero

            if (row < 0 || col < 0 || row >= boardSize || col >= boardSize)
            {
                return;
            }
            //casillas que ya se han expandido/abierto
            if (boardButtons[row, col].Content == null)
            {
                return;
            }
            boardButtons[row, col].IsHitTestVisible = false;
            int bombsAdjacent = CountAdjacentBombs(row, col);

            if (bombsAdjacent == 0)
            {
                //la casilla está vacía, la expandimos y continuamos con las casillas adyacentes
                boardButtons[row, col].Content = null;
                boardButtons[row, col].Background = Brushes.LightBlue;
                for (int i = -1; i <= 1; i++)
                {
                    for (int j = -1; j <= 1; j++)
                    {
                        ExpandCells(row + i, col + j);

                    }
                }
            }
            else
            {
                // las casillas tienen número, las revelamos y detenemos la expansión
                boardButtons[row, col].Content = bombsAdjacent;
                SetNumberColor(boardButtons[row, col], bombsAdjacent);
                boardButtons[row, col].Background = Brushes.LightBlue;
            }
        }


        //función que indica lo que se ejecuta cuando pulsamos el boton de reset
        private void ResertButton_Click(object sender, RoutedEventArgs e)
        {
            //reiniciar el tablero
            InicializeBoard();
            //reiniciar el temporizador
            StartTimer();
            //reiniciar las banderas
            flag1 = 0;
            Bandera.Text = flag2.ToString();
        }

       


    }
}

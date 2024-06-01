using System.Runtime.InteropServices;

namespace AutoBot
{
    internal class Program
    {
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetCursorPos(int X, int Y);

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, int dwExtraInfo);

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;
        }

        private const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        private const uint MOUSEEVENTF_LEFTUP = 0x0004;

        private static bool escapePressed = false;
        private static bool startPressed = false;

        static void Main(string[] args)
        {
            //MoveCursorLinear();
            //GetMousePosition();
            //MoveCursorWithClickEventForASingleArea();
            MoveRandomly();

        }

        #region Move Cursor

        private static void MoveRandomly()
        {
            // Esc tuşu dinleyicisini arka planda çalıştır
            Task.Run(() => ListenForEscapeKey());

            Console.WriteLine("Fare imlecini rastgele hareket modlarında hareket ettiriyor. Çıkmak için ESC tuşuna basın.");

            // Hareket yapılacak koordinatların aralığını belirle
            int minX = 760;
            int minY = 460;
            int maxX = 1128;
            int maxY = 730;
            List<int> deletedElementList = new List<int>();
            List<int> moveList = new List<int> { 1, 2, 3, 4 }; 

            Random random = new Random();

            while (!escapePressed)
            {
                int index = random.Next(0, moveList.Count); // 1: Elips, 2: Yörüngesel, 3: Lineer

                switch (moveList[index])
                {
                    case 1:
                        MoveInEllipse(random, minX, minY);
                        break;
                    case 2:
                        MoveInCircularPath(random, maxX, maxY);
                        break;
                    case 3:
                        MoveCursorLinear();
                        break;
                    case 4:
                        MoveCursorBezeir();
                        break;
                }

                if(moveList.Count == 1)
                {
                    deletedElementList.ForEach(t => moveList.Add(t));
                }

                deletedElementList.Add(moveList[index]);
                moveList.RemoveAt(index);

                // Bir süre bekle
                Thread.Sleep(1000); // Her hareket sonrası bekleme süresi
            }

            Console.WriteLine("Fare hareket ettirme işlemi tamamlandı.");
        }   

        private static void MoveCursorLinear()
        {
            // Esc tuşu dinleyicisini arka planda çalıştır
            Task.Run(() => ListenForEscapeKey());

            // Hareket yapılacak koordinatların aralığını belirle
            int startX = 760;
            int startY = 460;
            int endX = 1128;
            int endY = 730;

            Random rnd = new Random();

            // Hareket süresi ve aralıklarını belirle
            int duration = 100000;
            int interval = 10; // 10 ms aralıklarla
            int steps = 10; // Hareketi kaç adıma bölcek

            int elapsed = 0;

            while (elapsed < duration && !escapePressed)
            {
                // Rastgele yeni koordinatlar oluştur
                int targetX = rnd.Next(startX, endX);
                int targetY = rnd.Next(startY, endY);

                // Mevcut fare imleci konumunu al
                GetCursorPos(out POINT currentPos);

                // X ve Y eksenlerindeki toplam hareket miktarını hesapla
                int deltaX = targetX - currentPos.X;
                int deltaY = targetY - currentPos.Y;

                for (int i = 0; i < steps && !escapePressed; i++)
                {
                    // Her adımda yapılacak küçük hareket miktarını hesapla
                    int stepX = currentPos.X + deltaX * i / steps;
                    int stepY = currentPos.Y + deltaY * i / steps;

                    // Fare imlecini yeni adıma taşı
                    SetCursorPos(stepX, stepY);

                    // Bekle
                    Thread.Sleep(interval);
                }

                // Hareket tamamlandıktan sonra yeni hedefe ulaştıktan sonra biraz bekle
                Thread.Sleep(500);

                elapsed += interval*steps + 500;
            }

            Console.WriteLine("Fare rastgele ve yumuşak hareket ettirme işlemi tamamlandı.");
        }

        private static void MoveCursorEliptic()
        {
            // Esc tuşu dinleyicisini arka planda çalıştır
            Task.Run(() => ListenForEscapeKey());

            Console.WriteLine("Fare imlecini rastgele noktalar arasında yarı eliptik yörüngelerde hareket ettiriyor. Çıkmak için ESC tuşuna basın.");

            int screenWidth = 1920; // Ekran genişliği
            int screenHeight = 1080; // Ekran yüksekliği
            Random random = new Random();

            // Başlangıç noktası
            int startX = random.Next(0, screenWidth);
            int startY = random.Next(0, screenHeight);

            while (!escapePressed)
            {
                // Hedef noktayı seç
                int endX = random.Next(0, screenWidth);
                int endY = random.Next(0, screenHeight);

                // Fare imlecini yumuşak bir şekilde yarı eliptik yörüngede hedefe taşı
                MoveMouseInEllipse(startX, startY, endX, endY, 1000);

                // Yeni başlangıç noktası, eski hedef noktası olur
                startX = endX;
                startY = endY;

                // Bir süre bekle
                Thread.Sleep(500); // Rastgele bekleme süresi
            }

            Console.WriteLine("Fare hareket ettirme işlemi tamamlandı.");
        }

        private static void MoveCursorBezeir()
        {
            Random rnd = new Random();
            // Esc tuşu dinleyicisini arka planda çalıştır
            Task.Run(() => ListenForEscapeKey());

            while (!escapePressed)
            {

                // Rastgele başlangıç ve bitiş koordinatları
                int startX = 760; // Ekran genişliği
                int startY = 460; // Ekran yüksekliği
                int endX = 1128;
                int endY = 730;

                // Bezier kontrol noktalarını belirle (daha karmaşık bir yol için)
                var controlPoints = new List<(double, double)>
                {
                    (startX, startY),
                    (startX + rnd.Next(-200, 200), startY + rnd.Next(-200, 200)), // Kontrol noktası 1
                    (endX + rnd.Next(-200, 200), endY + rnd.Next(-200, 200)),     // Kontrol noktası 2
                    (endX, endY)
                };

                // Bezier eğrisi boyunca fare hareketini simüle et
                SimulateBezierMovement(controlPoints);

                // Biraz bekleyin
                Thread.Sleep(rnd.Next(500, 2000));
            }

            Console.WriteLine("Program sonlandırıldı.");
        }

        private static void GetMousePosition()
        {
            Console.WriteLine("Fare imlecinin koordinatlarını görmek için fareyi hareket ettirin.");
            Console.WriteLine("Çıkmak için ESC tuşuna basın.");

            // Esc tuşunun basılıp basılmadığını kontrol eden işaretleyici
            bool escapePressed = false;

            while (!escapePressed)
            {
                // Mevcut fare imleci konumunu al
                GetCursorPos(out POINT currentPos);

                // Koordinatları ekrana yazdır
                Console.SetCursorPosition(0, 2); // İkinci satırda yazmak için pozisyonu ayarla
                Console.Write($"X: {currentPos.X}, Y: {currentPos.Y}    "); // Fazladan boşluklar ekleyerek eski metni sil

                // Esc tuşuna basıldığını kontrol et
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(true);
                    if (key.Key == ConsoleKey.Escape)
                    {
                        escapePressed = true;
                    }
                }

                Thread.Sleep(100); // CPU kullanımını azaltmak için kısa bir süre bekle
            }
        }

        #endregion

        #region Click

        private static void MoveCursorWithClickEventRandomArea()
        {
            // Tıklama yapılacak koordinatların aralığını belirle
            int startX = 100;
            int startY = 100;
            int endX = 800;
            int endY = 600;

            Random rnd = new Random();

            // Fare imlecini başlangıç koordinatına taşı
            SetCursorPos(startX, startY);

            // Farenin sol tuşuna basılı tutma işlemi
            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);

            // Basılı tutarak belirli süre boyunca rastgele hareket et
            int duration = 10000; // 10 saniye
            int elapsed = 0;
            int interval = 100; // 100 ms aralıklarla hareket ettir
            while (elapsed < duration)
            {
                // Rastgele yeni koordinatlar oluştur
                int newX = rnd.Next(startX, endX);
                int newY = rnd.Next(startY, endY);

                // Fare imlecini yeni koordinatlara taşı
                SetCursorPos(newX, newY);

                // Bekle
                Thread.Sleep(interval);
                elapsed += interval;
            }

            // Farenin sol tuşunu bırakma işlemi
            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);

            Console.WriteLine("Fare basılı tutarak rastgele hareket ettirme işlemi tamamlandı.");
        }

        private static void MoveCursorWithClickEventForASingleArea()
        {
            // Esc tuşu dinleyicisini arka planda çalıştır
            Task.Run(() => ListenForEscapeKey());

            // Tıklama yapılacak koordinatların aralığını belirle
            int startX = 956;
            int startY = 597;

            Random rnd = new Random();

            var key = Console.ReadKey(true);
            if (key.Key == ConsoleKey.Enter)
            {
                // Fare imlecini başlangıç koordinatına taşı
                SetCursorPos(startX, startY);

                int interval = 35; // 100 ms aralıklarla hareket ettir
                while (!escapePressed)
                {
                    mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);

                    Thread.Sleep(interval);
                    mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
                }

                Console.WriteLine("Fare basılı tutarak rastgele hareket ettirme işlemi tamamlandı.");
            }
        }

        #endregion

        #region Helpers

        // Esc tuşunu dinleyen metot
        private static void ListenForEscapeKey()
        {
            while (!escapePressed)
            {
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(true);
                    if (key.Key == ConsoleKey.Escape)
                    {
                        escapePressed = true;
                    }
                }
                Thread.Sleep(50); // CPU kullanımını azaltmak için kısa bir süre bekle
            }
        }

        // Fare imlecini yumuşak bir şekilde hareket ettiren metot
        private static void MoveMouseSmoothly(int targetX, int targetY, int duration)
        {
            GetCursorPos(out POINT currentPos);
            int startX = currentPos.X;
            int startY = currentPos.Y;
            int steps = duration / 10;
            for (int i = 0; i <= steps; i++)
            {
                int newX = startX + (targetX - startX) * i / steps;
                int newY = startY + (targetY - startY) * i / steps;
                SetCursorPos(newX, newY);
                Thread.Sleep(10);
            }
        }

        // Fare imlecini yarı eliptik yörüngede hareket ettiren metot
        private static void MoveMouseInEllipse(int startX, int startY, int endX, int endY, int duration)
        {
            int centerX = (startX + endX) / 2;
            int centerY = (startY + endY) / 2;
            int a = Math.Abs(endX - startX) / 2;
            int b = Math.Abs(endY - startY) / 2;

            double angle = 0;
            double speed = Math.PI / (duration / 10); // Yarım elips için açı değişimi

            int steps = duration / 10;
            for (int i = 0; i <= steps; i++)
            {
                if (escapePressed) break; // Eğer ESC tuşuna basıldıysa hareketi durdur

                double currentAngle = angle + speed * i;
                int x = centerX + (int)(a * Math.Cos(currentAngle));
                int y = centerY - (int)(b * Math.Sin(currentAngle)); // Yarı elips için sin dalgası

                SetCursorPos(x, y);
                Thread.Sleep(10);
            }
        }

        // Fare imlecini yörüngesel yörüngede hareket ettiren metot
        private static void MoveInCircularPath(Random random, int screenWidth, int screenHeight)
        {
            int centerX = screenWidth / 2;
            int centerY = screenHeight / 2;
            int radius = random.Next(50, 300);

            double angle = 0;
            double speed = 0.05; // Hareketin hızı (açı değişimi)

            int steps = 100;
            for (int i = 0; i <= steps; i++)
            {
                double currentAngle = angle + speed * i;
                int x = centerX + (int)(radius * Math.Cos(currentAngle));
                int y = centerY - (int)(radius * Math.Sin(currentAngle));

                SetCursorPos(x, y);
                Thread.Sleep(10);
            }
        }

        // Fare imlecini eliptik yörüngede hareket ettiren metot
        private static void MoveInEllipse(Random random, int screenWidth, int screenHeight)
        {
            int centerX = screenWidth / 2;
            int centerY = screenHeight / 2;
            int a = random.Next(50, 300); // Yatay yarı eksen
            int b = random.Next(50, 200); // Dikey yarı eksen

            double angle = 0;
            double speed = 0.05; // Hareketin hızı (açı değişimi)

            int steps = 100;
            for (int i = 0; i <= steps; i++)
            {
                double currentAngle = angle + speed * i;
                int x = centerX + (int)(a * Math.Cos(currentAngle));
                int y = centerY - (int)(b * Math.Sin(currentAngle));

                SetCursorPos(x, y);
                Thread.Sleep(10);
            }
        }

        static void SimulateBezierMovement(List<(double X, double Y)> controlPoints)
        {
            int steps = 100; // Hareketin ne kadar düzgün olacağını belirler
            for (int i = 0; i <= steps; i++)
            {
                double t = i / (double)steps;
                var point = CalculateBezierPoint(t, controlPoints);
                SetCursorPos((int)point.X, (int)point.Y);
                Thread.Sleep(10); // Hareketin hızını ayarlar
            }
        }

        static (double X, double Y) CalculateBezierPoint(double t, List<(double X, double Y)> controlPoints)
        {
            int n = controlPoints.Count - 1;
            double x = 0, y = 0;
            for (int i = 0; i <= n; i++)
            {
                var binomialCoeff = BinomialCoefficient(n, i);
                var term = Math.Pow(1 - t, n - i) * Math.Pow(t, i) * binomialCoeff;
                x += term * controlPoints[i].X;
                y += term * controlPoints[i].Y;
            }
            return (x, y);
        }

        static int BinomialCoefficient(int n, int k)
        {
            int result = 1;
            for (int i = 1; i <= k; i++)
            {
                result *= n--;
                result /= i;
            }
            return result;
        }
        #endregion
    }
}

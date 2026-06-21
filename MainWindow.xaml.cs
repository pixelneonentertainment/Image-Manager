using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Collections.Concurrent;

namespace GorselYoneticisi
{
    public partial class MainWindow : Window
    {
        string[] gorselUzantilari = { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };
        List<string> bulunanDosyalar = new List<string>();
        List<string> aktifListe = new List<string>();
        HashSet<string> secilenDosyalar = new HashSet<string>();
        bool tumunuSecildi = false;

        int mevcutSayfa = 1;
        int sayfaBoyutu = 300;

        int ToplamSayfa => (int)Math.Ceiling((double)aktifListe.Count / sayfaBoyutu);

        // Arka plan resim yüklemelerini sayfa geçişlerinde iptal eden asenkron token kontrolcüsü
        CancellationTokenSource ctsSayfa = new CancellationTokenSource();

        // Aktif klasör/sürücü filtresini tutan değişkenler
        string seciliKlasorYolu = null;
        bool seciliSürücüMu = false;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try { ApplyTheme("Theme1"); }
            catch { }
        }

        private void ThemeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                // 'themeCombo' değişkeni yerine tetikleyici sender nesnesini güvenle kullanıyoruz [1.1.9]
                var comboBox = sender as ComboBox;
                if (comboBox != null && comboBox.SelectedItem is ComboBoxItem it && it.Tag is string tag)
                {
                    ApplyTheme(tag);
                }
            }
            catch { }
        }

        private void ApplyTheme(string themeTag)
        {
            var dict = new ResourceDictionary();
            switch (themeTag)
            {
                case "Theme1": // Koyu
                    dict["LeftPanelBg_Color"] = (Color)ColorConverter.ConvertFromString("#2A2A3E");
                    dict["MainBg_Color"] = (Color)ColorConverter.ConvertFromString("#1E1E2E");
                    dict["CardBg_Color"] = (Color)ColorConverter.ConvertFromString("#23232B");
                    dict["Accent_Color"] = (Color)ColorConverter.ConvertFromString("#6C63FF");
                    dict["ButtonBackground"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3A3A50"));
                    dict["ButtonForeground"] = new SolidColorBrush(Colors.White);
                    break;
                case "Theme2": // Okyanus
                    dict["LeftPanelBg_Color"] = (Color)ColorConverter.ConvertFromString("#0E4D64");
                    dict["MainBg_Color"] = (Color)ColorConverter.ConvertFromString("#06283D");
                    dict["CardBg_Color"] = (Color)ColorConverter.ConvertFromString("#093B4C");
                    dict["Accent_Color"] = (Color)ColorConverter.ConvertFromString("#45A29E");
                    dict["ButtonBackground"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0E7886"));
                    dict["ButtonForeground"] = new SolidColorBrush(Colors.White);
                    break;
                case "Theme3": // Zümrüt Yeşili (Yeni eklenen renk paleti)
                    dict["LeftPanelBg_Color"] = (Color)ColorConverter.ConvertFromString("#122315");
                    dict["MainBg_Color"] = (Color)ColorConverter.ConvertFromString("#09140C");
                    dict["CardBg_Color"] = (Color)ColorConverter.ConvertFromString("#172B1B");
                    dict["Accent_Color"] = (Color)ColorConverter.ConvertFromString("#2ECC71");
                    dict["ButtonBackground"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E5F32"));
                    dict["ButtonForeground"] = new SolidColorBrush(Colors.White);
                    break;
                case "Theme4": // Şeker Pembe (Tüm uygulamanın pembe renge bürünmüş hali) [4]
                    dict["LeftPanelBg_Color"] = (Color)ColorConverter.ConvertFromString("#FFA6C9");
                    dict["MainBg_Color"] = (Color)ColorConverter.ConvertFromString("#FFC0CB");
                    dict["CardBg_Color"] = (Color)ColorConverter.ConvertFromString("#FFD1DC");
                    dict["Accent_Color"] = (Color)ColorConverter.ConvertFromString("#FF1493");
                    dict["ButtonBackground"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E0115F"));
                    dict["ButtonForeground"] = new SolidColorBrush(Colors.White);
                    break;
                case "Theme5": // Rengarenk
                    dict["LeftPanelBg_Color"] = (Color)ColorConverter.ConvertFromString("#3A3A3A");
                    dict["MainBg_Color"] = (Color)ColorConverter.ConvertFromString("#121212");
                    dict["CardBg_Color"] = (Color)ColorConverter.ConvertFromString("#1E1E1E");
                    dict["Accent_Color"] = (Color)ColorConverter.ConvertFromString("#FF6B6B");
                    dict["ButtonBackground"] = new LinearGradientBrush(new GradientStopCollection {
                        new GradientStop((Color)ColorConverter.ConvertFromString("#FF6B6B"), 0),
                        new GradientStop((Color)ColorConverter.ConvertFromString("#FFD93D"), 0.25),
                        new GradientStop((Color)ColorConverter.ConvertFromString("#6BCB77"), 0.5),
                        new GradientStop((Color)ColorConverter.ConvertFromString("#4D96FF"), 0.75),
                        new GradientStop((Color)ColorConverter.ConvertFromString("#9B5DE5"), 1)
                    }, 0);
                    dict["ButtonForeground"] = new SolidColorBrush(Colors.White);
                    break;
            }

            try
            {
                var app = Application.Current;
                if (app == null) return;

                if (dict.Contains("LeftPanelBg_Color")) app.Resources["LeftPanelBg_Color"] = dict["LeftPanelBg_Color"];
                if (dict.Contains("MainBg_Color")) app.Resources["MainBg_Color"] = dict["MainBg_Color"];
                if (dict.Contains("CardBg_Color")) app.Resources["CardBg_Color"] = dict["CardBg_Color"];
                if (dict.Contains("Accent_Color")) app.Resources["Accent_Color"] = dict["Accent_Color"];
                if (dict.Contains("ButtonBackground")) app.Resources["ButtonBackground"] = dict["ButtonBackground"];
                if (dict.Contains("ButtonForeground")) app.Resources["ButtonForeground"] = dict["ButtonForeground"];

                app.Resources["LeftPanelBackground"] = new SolidColorBrush((Color)app.Resources["LeftPanelBg_Color"]);
                app.Resources["WindowBackground"] = new SolidColorBrush((Color)app.Resources["MainBg_Color"]);
                app.Resources["CardBackground"] = new SolidColorBrush((Color)app.Resources["CardBg_Color"]);

                Color bgColor = (Color)app.Resources["MainBg_Color"];
                double bgLum = (0.299 * bgColor.R + 0.587 * bgColor.G + 0.114 * bgColor.B) / 255.0;
                Color textColor = bgLum > 0.5 ? Colors.Black : Colors.White;
                app.Resources["TextForeground"] = new SolidColorBrush(textColor);

                if (textColor == Colors.White)
                    app.Resources["SecondaryForeground"] = new SolidColorBrush(Color.FromArgb(0xFF, 0xCC, 0xCC, 0xCC));
                else
                    app.Resources["SecondaryForeground"] = new SolidColorBrush(Color.FromArgb(0xFF, 0x55, 0x55, 0x55));

                Color accentColor = Colors.Transparent;
                try
                {
                    var accObj = app.Resources["Accent_Color"];
                    if (accObj is Color c) accentColor = c;
                    else if (accObj is SolidColorBrush sb) accentColor = sb.Color;
                }
                catch { }
                app.Resources["AccentBrush"] = new SolidColorBrush(accentColor);
            }
            catch { }
        }

        private async void BtnTara_Click(object sender, RoutedEventArgs e)
        {
            var drives = DriveInfo.GetDrives()
                .Where(d => d.DriveType == DriveType.Fixed || d.DriveType == DriveType.Removable)
                .Select(d => d.Name)
                .ToArray();

            await StartScan(drives, "Taranıyor... (tüm sürücüler)");
        }

        private void BtnDiskSec_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dlg = new DiskSelectionWindow();
                dlg.Owner = this;
                if (dlg.ShowDialog() == true)
                {
                    var chosen = dlg.GetSelectedDrives();
                    if (chosen != null && chosen.Length > 0)
                        _ = StartScan(chosen, "Taranıyor... (seçili sürücüler)");
                }
            }
            catch { }
        }

        private async Task StartScan(string[] roots, string durumMetni)
        {
            // PROGRESS BAR'I ETKİNLEŞTİR VE BUTONLARI DEVRE DIŞI BIRAK
            if (scanProgressBar != null)
            {
                scanProgressBar.Visibility = Visibility.Visible;
                scanProgressBar.IsIndeterminate = true;
            }
            btnTara.IsEnabled = false;
            btnDiskSec.IsEnabled = false;

            try
            {
                gorselPanel.Children.Clear();
                bulunanDosyalar.Clear();
                secilenDosyalar.Clear();
                tumunuSecildi = false;
                mevcutSayfa = 1;

                lblDurum.Text = durumMetni;
                SeciliGuncelle();

                try { trvDosyalar.ItemsSource = null; } catch { }

                // Filtre seçimlerini sıfırla
                seciliKlasorYolu = null;
                seciliSürücüMu = false;
                if (filterType != null) filterType.SelectedIndex = 0;
                if (filterSize != null) filterSize.SelectedIndex = 0;
                if (filterDate != null) filterDate.SelectedIndex = 0;

                var extensions = new HashSet<string>(gorselUzantilari, StringComparer.OrdinalIgnoreCase);

                var bag = new ConcurrentBag<string>();
                int found = 0;

                Action<int> report = (count) =>
                {
                    try { Dispatcher.BeginInvoke(new Action(() => lblDurum.Text = $"{count} görsel bulundu...")); }
                    catch { }
                };

                await Task.Run(() =>
                {
                    var po = new ParallelOptions { MaxDegreeOfParallelism = Math.Max(1, Environment.ProcessorCount - 1) };

                    foreach (var root in roots)
                    {
                        try
                        {
                            if (!Directory.Exists(root)) continue;

                            try
                            {
                                foreach (var f in Directory.EnumerateFiles(root))
                                {
                                    if (extensions.Contains(Path.GetExtension(f)))
                                    {
                                        bag.Add(f);
                                        if (Interlocked.Increment(ref found) % 500 == 0) report(found);
                                    }
                                }
                            }
                            catch { }

                            List<string> topDirs = new List<string>();
                            try { topDirs.AddRange(Directory.EnumerateDirectories(root)); } catch { }

                            Parallel.ForEach(topDirs, po, dir =>
                            {
                                var stack = new Stack<string>();
                                stack.Push(dir);
                                while (stack.Count > 0)
                                {
                                    var d = stack.Pop();
                                    try
                                    {
                                        foreach (var f in Directory.EnumerateFiles(d))
                                        {
                                            if (extensions.Contains(Path.GetExtension(f)))
                                            {
                                                bag.Add(f);
                                                if (Interlocked.Increment(ref found) % 500 == 0) report(found);
                                            }
                                        }
                                        foreach (var sd in Directory.EnumerateDirectories(d))
                                            stack.Push(sd);
                                    }
                                    catch { }
                                }
                            });
                        }
                        catch { }
                    }
                });

                bulunanDosyalar = bag
                    .Distinct()
                    .OrderBy(s => s, StringComparer.OrdinalIgnoreCase)
                    .ToList();

                aktifListe = new List<string>(bulunanDosyalar);
                lblDurum.Text = $"{bulunanDosyalar.Count} görsel bulundu.";

                // Klasör ağacını oluştur
                trvDosyalar.ItemsSource = BuildFolderTree(bulunanDosyalar);

                UygulaFiltreleme();
            }
            catch (Exception ex)
            {
                lblDurum.Text = $"Tarama hatası: {ex.Message}";
            }
            finally
            {
                // PROGRESS BAR'I GİZLE VE BUTONLARI TEKRAR ETKİNLEŞTİR [1]
                if (scanProgressBar != null)
                {
                    scanProgressBar.IsIndeterminate = false;
                    scanProgressBar.Visibility = Visibility.Collapsed;
                }
                btnTara.IsEnabled = true;
                btnDiskSec.IsEnabled = true;
            }
        }

        private void SayfayiGoster(int sayfa)
        {
            // Arayüz nesneleri henüz XAML tarafından yüklenmediyse erken dön (Başlangıç çökmesini önler)
            if (gorselPanel == null) return;

            // ÖNCEKİ SAYFADAN KALAN TÜM RESİM DEŞİFRE İŞLEMLERİNİ ANINDA İPTAL ET
            try
            {
                ctsSayfa?.Cancel();
                ctsSayfa?.Dispose();
            }
            catch { }
            ctsSayfa = new CancellationTokenSource();

            gorselPanel.Children.Clear();

            if (aktifListe.Count == 0)
            {
                lblSayfa.Text = "Sayfa 0 / 0 (0 görsel)";
                return;
            }

            mevcutSayfa = Math.Max(1, Math.Min(sayfa, ToplamSayfa));

            int baslangic = (mevcutSayfa - 1) * sayfaBoyutu;
            int bitis = Math.Min(baslangic + sayfaBoyutu, aktifListe.Count);

            for (int i = baslangic; i < bitis; i++)
            {
                string dosya = aktifListe[i];

                bool yinelenen = dosya.StartsWith("__yinelenen__");
                string gercekYol = yinelenen ? dosya.Substring("__yinelenen__".Length) : dosya;
                GorselKartEkle(gercekYol, yinelenen, ctsSayfa.Token);
            }

            lblSayfa.Text = $"Sayfa {mevcutSayfa} / {ToplamSayfa}  ({aktifListe.Count} görsel)";
        }

        // Asenkron ve donmasız görsel yükleme sistemi
        private async void GorselKartEkle(string dosyaYolu, bool yinelenen, CancellationToken token)
        {
            try
            {
                var border = new Border
                {
                    Width = 150,
                    Height = 175,
                    Margin = new Thickness(5),
                    Background = (Brush)Application.Current.Resources["CardBackground"],
                    BorderBrush = yinelenen ? Brushes.Orange : Brushes.Transparent,
                    BorderThickness = new Thickness(2),
                    Cursor = Cursors.Hand,
                    Tag = dosyaYolu,
                    CornerRadius = new CornerRadius(6)
                };

                var panel = new StackPanel();

                var checkSatir = new Grid();
                checkSatir.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                checkSatir.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                var cb = new CheckBox
                {
                    Margin = new Thickness(5, 4, 0, 0),
                    Tag = dosyaYolu,
                    IsChecked = secilenDosyalar.Contains(dosyaYolu) || tumunuSecildi
                };
                cb.Checked += (s, ev) =>
                {
                    secilenDosyalar.Add(dosyaYolu);
                    SeciliGuncelle();
                };
                cb.Unchecked += (s, ev) =>
                {
                    secilenDosyalar.Remove(dosyaYolu);
                    tumunuSecildi = false;
                    SeciliGuncelle();
                };
                Grid.SetColumn(cb, 0);

                if (yinelenen)
                {
                    var badge = new TextBlock
                    {
                        Text = "Kopya",
                        Foreground = Brushes.White,
                        Background = Brushes.OrangeRed,
                        FontSize = 9,
                        Padding = new Thickness(4, 2, 4, 2),
                        Margin = new Thickness(0, 4, 5, 0)
                    };
                    Grid.SetColumn(badge, 1);
                    checkSatir.Children.Add(badge);
                }

                checkSatir.Children.Add(cb);
                panel.Children.Add(checkSatir);

                var image = new Image
                {
                    Height = 115,
                    Stretch = Stretch.UniformToFill
                };

                var isim = new TextBlock
                {
                    Text = Path.GetFileName(dosyaYolu),
                    Foreground = (Brush)Application.Current.Resources["SecondaryForeground"],
                    FontSize = 10,
                    TextTrimming = TextTrimming.CharacterEllipsis,
                    Margin = new Thickness(4, 3, 4, 0)
                };

                panel.Children.Add(image);
                panel.Children.Add(isim);
                border.Child = panel;

                border.MouseLeftButtonDown += (s, ev) =>
                {
                    if (ev.ClickCount == 1)
                    {
                        var checkbox = BulCheckBox(border);
                        if (checkbox != null)
                            checkbox.IsChecked = !checkbox.IsChecked;
                    }
                    else if (ev.ClickCount == 2)
                    {
                        GorseliBuyut(dosyaYolu);
                    }
                };

                gorselPanel.Children.Add(border);

                if (token.IsCancellationRequested) return;

                // İşlemci yükünü ve okuma işlemlerini arka plana dağıtır (Thread-safe)
                BitmapImage img = await Task.Run(() =>
                {
                    try
                    {
                        if (token.IsCancellationRequested) return null;
                        if (!File.Exists(dosyaYolu)) return null;

                        byte[] fileBytes = File.ReadAllBytes(dosyaYolu);

                        if (token.IsCancellationRequested) return null;

                        var tempImg = new BitmapImage();
                        using (var stream = new MemoryStream(fileBytes))
                        {
                            tempImg.BeginInit();
                            tempImg.DecodePixelWidth = 120;
                            tempImg.CacheOption = BitmapCacheOption.OnLoad;
                            tempImg.StreamSource = stream;
                            tempImg.EndInit();
                            tempImg.Freeze(); // UI iş parçacığına taşımadan önce dondurur
                        }
                        return tempImg;
                    }
                    catch
                    {
                        return null;
                    }
                });

                if (token.IsCancellationRequested) return;

                if (img != null)
                {
                    image.Source = img;
                }
            }
            catch { }
        }

        private void GorseliBuyut(string dosyaYolu)
        {
            try
            {
                var buyukImg = new BitmapImage();
                buyukImg.BeginInit();
                buyukImg.UriSource = new Uri(dosyaYolu, UriKind.Absolute);
                buyukImg.CacheOption = BitmapCacheOption.OnLoad;
                buyukImg.EndInit();
                buyukImg.Freeze();

                var overlay = new Grid
                {
                    Background = new SolidColorBrush(Color.FromArgb(200, 0, 0, 0))
                };

                var buyukGorsel = new Image
                {
                    Source = buyukImg,
                    MaxWidth = 800,
                    MaxHeight = 600,
                    Stretch = Stretch.Uniform,
                    Margin = new Thickness(20)
                };

                var dosyaAdi = new TextBlock
                {
                    Text = Path.GetFileName(dosyaYolu),
                    Foreground = Brushes.White,
                    FontSize = 13,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 0, 0, 10)
                };

                var kapatBtn = new Button
                {
                    Content = "✕  Kapat",
                    Padding = new Thickness(20, 8, 20, 8),
                    Background = new SolidColorBrush(Color.FromRgb(108, 99, 255)),
                    Foreground = Brushes.White,
                    BorderThickness = new Thickness(0),
                    Cursor = Cursors.Hand,
                    FontSize = 13,
                    Margin = new Thickness(0, 10, 0, 0)
                };

                var icerik = new StackPanel
                {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                icerik.Children.Add(buyukGorsel);
                icerik.Children.Add(dosyaAdi);
                icerik.Children.Add(kapatBtn);
                overlay.Children.Add(icerik);

                var anaGrid = (Grid)this.Content;

                // DÜZELTME: 3 kolonlu yeni Grid yapısına uyum sağlaması için Span değeri 3'e yükseltildi [3.4]
                Grid.SetColumnSpan(overlay, 3);

                anaGrid.Children.Add(overlay);

                kapatBtn.Click += (s, ev) => anaGrid.Children.Remove(overlay);
                overlay.MouseLeftButtonDown += (s, ev) =>
                {
                    if (ev.Source == overlay)
                        anaGrid.Children.Remove(overlay);
                };
            }
            catch { }
        }

        // Seçilen ögelerin boyutunu toplayan metot
        private void SeciliGuncelle()
        {
            var secilenler = SecilenleriGetir();
            int adet = secilenler.Count;

            if (adet == 0)
            {
                lblSecili.Text = "0 görsel seçili";
                return;
            }

            long toplamBayt = 0;

            foreach (var dosya in secilenler)
            {
                try
                {
                    string gercekYol = dosya.StartsWith("__yinelenen__") ? dosya.Substring("__yinelenen__".Length) : dosya;
                    if (File.Exists(gercekYol))
                    {
                        toplamBayt += new FileInfo(gercekYol).Length;
                    }
                }
                catch { }
            }

            string boyutMetni = FormatBoyut(toplamBayt);

            if (tumunuSecildi)
                lblSecili.Text = $"{adet} görsel seçili (tümü) - {boyutMetni}";
            else
                lblSecili.Text = $"{adet} görsel seçili - {boyutMetni}";
        }

        private string FormatBoyut(long bayt)
        {
            string[] birimler = { "B", "KB", "MB", "GB", "TB" };
            double geciciBoyut = bayt;
            int birimIndeksi = 0;

            while (geciciBoyut >= 1024 && birimIndeksi < birimler.Length - 1)
            {
                geciciBoyut /= 1024;
                birimIndeksi++;
            }

            return $"{geciciBoyut:N1} {birimler[birimIndeksi]}";
        }

        private void OpenInExplorer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (trvDosyalar.SelectedItem is FolderNode selectedNode && Directory.Exists(selectedNode.FullPath))
                {
                    OpenInExplorer(selectedNode.FullPath);
                }
            }
            catch { }
        }

        private void CopyPath_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (trvDosyalar.SelectedItem is FolderNode selectedNode)
                {
                    Clipboard.SetText(selectedNode.FullPath);
                }
            }
            catch { }
        }

        private void TrvDosyalar_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (trvDosyalar.SelectedItem is FolderNode selectedNode && Directory.Exists(selectedNode.FullPath))
                {
                    OpenInExplorer(selectedNode.FullPath);
                }
            }
            catch { }
        }

        private void OpenInExplorer(string path)
        {
            try
            {
                Process.Start(new ProcessStartInfo("explorer.exe", $"\"{path}\"")
                {
                    UseShellExecute = true
                });
            }
            catch { }
        }

        private CheckBox BulCheckBox(Border b)
        {
            if (b.Child is StackPanel sp)
                foreach (var child in sp.Children)
                    if (child is Grid g)
                        foreach (var gc in g.Children)
                            if (gc is CheckBox cb) return cb;
            return null;
        }

        private List<string> SecilenleriGetir()
        {
            if (tumunuSecildi)
                return new List<string>(aktifListe);
            return new List<string>(secilenDosyalar);
        }

        private void BtnIlkSayfa_Click(object sender, RoutedEventArgs e) => SayfayiGoster(1);
        private void BtnSonSayfa_Click(object sender, RoutedEventArgs e) => SayfayiGoster(ToplamSayfa);
        private void BtnOncekiSayfa_Click(object sender, RoutedEventArgs e) => SayfayiGoster(mevcutSayfa - 1);
        private void BtnSonrakiSayfa_Click(object sender, RoutedEventArgs e) => SayfayiGoster(mevcutSayfa + 1);

        private void BtnTumunuSec_Click(object sender, RoutedEventArgs e)
        {
            foreach (Border b in gorselPanel.Children)
            {
                var cb = BulCheckBox(b);
                if (cb != null)
                {
                    cb.IsChecked = true;
                    if (cb.Tag is string path)
                        secilenDosyalar.Add(path);
                }
            }
            SeciliGuncelle();
        }

        private void BtnSecimiKaldir_Click(object sender, RoutedEventArgs e)
        {
            tumunuSecildi = false;
            secilenDosyalar.Clear();
            foreach (Border b in gorselPanel.Children)
            {
                var cb = BulCheckBox(b);
                if (cb != null) cb.IsChecked = false;
            }
            SeciliGuncelle();
        }

        private async void BtnYinelenenleri_Click(object sender, RoutedEventArgs e)
        {
            tumunuSecildi = false;
            secilenDosyalar.Clear();
            lblDurum.Text = "Yinelenenler aranıyor...";
            btnYinelenenleri.IsEnabled = false;

            // YİNENELEN GÖRSELLERİ ARARKEN DE PROGRESS BAR'I ETKİNLEŞTİR
            if (scanProgressBar != null)
            {
                scanProgressBar.Visibility = Visibility.Visible;
                scanProgressBar.IsIndeterminate = true;
            }

            try
            {
                var hashGruplari = new Dictionary<string, List<string>>();

                await Task.Run(() =>
                {
                    foreach (var dosya in bulunanDosyalar)
                    {
                        try
                        {
                            using (var md5 = MD5.Create())
                            using (var stream = File.OpenRead(dosya))
                            {
                                string hash = BitConverter.ToString(md5.ComputeHash(stream));
                                if (!hashGruplari.ContainsKey(hash))
                                    hashGruplari[hash] = new List<string>();
                                hashGruplari[hash].Add(dosya);
                            }
                        }
                        catch { }
                    }
                });

                var sadecKopyalar = hashGruplari.Values
                    .Where(g => g.Count > 1)
                    .SelectMany(g => g.Skip(1).Select(f => "__yinelenen__" + f))
                    .ToList();

                if (sadecKopyalar.Count == 0)
                {
                    MessageBox.Show("Yinelenen görsel bulunamadı!");
                    lblDurum.Text = "Yinelenen yok.";
                    return;
                }

                aktifListe = sadecKopyalar;
                mevcutSayfa = 1;
                lblDurum.Text = $"{sadecKopyalar.Count} kopya görsel bulundu!";
                SeciliGuncelle();

                // Süzgeçleri de sıfırlayarak ağaç yapısını yinelenenlere göre günceller
                seciliKlasorYolu = null;
                seciliSürücüMu = false;
                if (filterType != null) filterType.SelectedIndex = 0;
                if (filterSize != null) filterSize.SelectedIndex = 0;
                if (filterDate != null) filterDate.SelectedIndex = 0;

                trvDosyalar.ItemsSource = BuildFolderTree(aktifListe);
                SayfayiGoster(1);
            }
            catch { }
            finally
            {
                // PROGRESS BAR'I GİZLE VE BUTONU GERİ AÇ
                if (scanProgressBar != null)
                {
                    scanProgressBar.IsIndeterminate = false;
                    scanProgressBar.Visibility = Visibility.Collapsed;
                }
                btnYinelenenleri.IsEnabled = true;
            }
        }

        private void BtnSil_Click(object sender, RoutedEventArgs e)
        {
            var secililer = SecilenleriGetir();
            if (secililer.Count == 0)
            {
                MessageBox.Show("Lütfen önce görsel seçin.");
                return;
            }

            var onay = MessageBox.Show($"{secililer.Count} görsel silinsin mi?",
                "Silme Onayı", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (onay == MessageBoxResult.Yes)
            {
                int silindi = 0;
                foreach (var dosya in secililer)
                {
                    try
                    {
                        string gercekYol = dosya.StartsWith("__yinelenen__") ? dosya.Substring("__yinelenen__".Length) : dosya;
                        File.Delete(gercekYol);
                        bulunanDosyalar.Remove(gercekYol);
                        aktifListe.Remove(dosya);
                        silindi++;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Silinemedi: {ex.Message}");
                    }
                }

                tumunuSecildi = false;
                secilenDosyalar.Clear();
                lblDurum.Text = $"{silindi} görsel silindi. {aktifListe.Count} görsel kaldı.";
                SeciliGuncelle();
                SayfayiGoster(Math.Max(1, Math.Min(mevcutSayfa, ToplamSayfa)));
            }
        }

        private void BtnKopyala_Click(object sender, RoutedEventArgs e)
        {
            var secililer = SecilenleriGetir();
            if (secililer.Count == 0)
            {
                MessageBox.Show("Lütfen önce görsel seçin.");
                return;
            }

            var dialog = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "Kopyalanacak klasörü seçin"
            };

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                int kopyalandi = 0;
                foreach (var dosya in secililer)
                {
                    try
                    {
                        string gercekYol = dosya.StartsWith("__yinelenen__") ? dosya.Substring("__yinelenen__".Length) : dosya;
                        string hedef = Path.Combine(dialog.SelectedPath, Path.GetFileName(gercekYol));
                        File.Copy(gercekYol, hedef, true);
                        kopyalandi++;
                    }
                    catch { }
                }
                lblDurum.Text = $"{kopyalandi} görsel kopyalandı!";
                MessageBox.Show($"{kopyalandi} görsel kopyalandı!");
            }
        }

        // Lightroom tarzı hiyerarşik ağaç yapısı metotları
        private List<FolderNode> BuildFolderTree(List<string> filePaths)
        {
            var roots = new List<FolderNode>();
            var nodeCache = new Dictionary<string, FolderNode>(StringComparer.OrdinalIgnoreCase);

            foreach (var file in filePaths)
            {
                try
                {
                    string cleanFile = file.StartsWith("__yinelenen__") ? file.Substring("__yinelenen__".Length) : file;
                    string dirPath = Path.GetDirectoryName(cleanFile);
                    if (string.IsNullOrEmpty(dirPath)) continue;

                    string[] parts = dirPath.Split(new[] { Path.DirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 0) continue;

                    string driveLetter = Path.GetPathRoot(dirPath).TrimEnd(Path.DirectorySeparatorChar);
                    if (string.IsNullOrEmpty(driveLetter)) continue;

                    FolderNode driveNode;
                    if (!nodeCache.TryGetValue(driveLetter, out driveNode))
                    {
                        string driveLabel = GetDriveLabel(driveLetter);
                        driveNode = new FolderNode { Name = driveLabel, FullPath = driveLetter + "\\", IsDrive = true };
                        roots.Add(driveNode);
                        nodeCache[driveLetter] = driveNode;
                    }

                    FolderNode currentNode = driveNode;
                    string currentPath = driveLetter + "\\";

                    for (int i = 1; i < parts.Length; i++)
                    {
                        string part = parts[i];
                        currentPath = Path.Combine(currentPath, part);

                        FolderNode childNode;
                        if (!nodeCache.TryGetValue(currentPath, out childNode))
                        {
                            childNode = new FolderNode { Name = part, FullPath = currentPath, IsDrive = false };
                            currentNode.Children.Add(childNode);
                            nodeCache[currentPath] = childNode;
                        }
                        currentNode = childNode;
                    }
                }
                catch { }
            }

            SortTree(roots);
            return roots;
        }

        private string GetDriveLabel(string driveLetter)
        {
            try
            {
                DriveInfo di = new DriveInfo(driveLetter);
                if (di.IsReady)
                {
                    string volumeLabel = di.VolumeLabel;
                    if (string.IsNullOrEmpty(volumeLabel))
                    {
                        return di.DriveType == DriveType.Fixed ? $"Yerel Disk ({driveLetter})" : $"Yeni Birim ({driveLetter})";
                    }
                    return $"{volumeLabel} ({driveLetter})";
                }
            }
            catch { }
            return $"Yeni Birim ({driveLetter})";
        }

        private void SortTree(List<FolderNode> nodes)
        {
            // CS1503 StringComparer uyuşmazlığı, StringComparison enum sabiti ile çözülmüştür
            nodes.Sort((x, y) => string.Compare(x.Name, y.Name, StringComparison.OrdinalIgnoreCase));
            foreach (var node in nodes)
            {
                SortTree(node.Children);
            }
        }

        // Klasör ağacında seçim yapıldığında sağ taraftaki filtreyi günceller
        private void TrvDosyalar_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is FolderNode selectedNode)
            {
                seciliKlasorYolu = selectedNode.FullPath;
                seciliSürücüMu = selectedNode.IsDrive;

                UygulaFiltreleme();
            }
        }

        // Filtre elemanlarından biri değiştiğinde tetiklenir
        private void Filter_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (filterType == null || filterSize == null || filterDate == null) return;
            UygulaFiltreleme();
        }

        // Tüm filtreleri (Klasör seçimi + Tür + Boyut + Tarih) dinamik olarak birleştiren ana filtreleyici
        private void UygulaFiltreleme()
        {
            if (bulunanDosyalar == null) return;

            IEnumerable<string> filtrelenmis = bulunanDosyalar;

            // 1. Klasör Ağacı Filtresi
            if (!string.IsNullOrEmpty(seciliKlasorYolu))
            {
                if (seciliSürücüMu)
                {
                    filtrelenmis = filtrelenmis.Where(f => f.StartsWith(seciliKlasorYolu, StringComparison.OrdinalIgnoreCase));
                }
                else
                {
                    filtrelenmis = filtrelenmis.Where(f => f.StartsWith(seciliKlasorYolu + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) ||
                                                           Path.GetDirectoryName(f).Equals(seciliKlasorYolu, StringComparison.OrdinalIgnoreCase));
                }
            }

            // 2. Dosya Türü Filtresi
            if (filterType.SelectedItem is ComboBoxItem typeItem && typeItem.Content is string typeStr && typeStr != "Tümü")
            {
                string hedefUzantı = "." + typeStr.ToLower();
                filtrelenmis = filtrelenmis.Where(f => Path.GetExtension(f).Equals(hedefUzantı, StringComparison.OrdinalIgnoreCase));
            }

            // 3. Boyut Süzgeci Filtresi
            if (filterSize.SelectedItem is ComboBoxItem sizeItem && sizeItem.Content is string sizeStr && sizeStr != "Her Boyut")
            {
                filtrelenmis = filtrelenmis.Where(f =>
                {
                    try
                    {
                        string gercekYol = f.StartsWith("__yinelenen__") ? f.Substring("__yinelenen__".Length) : f;
                        if (!File.Exists(gercekYol)) return false;
                        long dosyaUzunlugu = new FileInfo(gercekYol).Length;

                        if (sizeStr == "< 500 KB") return dosyaUzunlugu < 500 * 1024;
                        if (sizeStr == "500 KB - 5 MB") return dosyaUzunlugu >= 500 * 1024 && dosyaUzunlugu <= 5 * 1024 * 1024;
                        if (sizeStr == "5 MB - 20 MB") return dosyaUzunlugu >= 5 * 1024 * 1024 && dosyaUzunlugu <= 20 * 1024 * 1024;
                        if (sizeStr == "> 20 MB") return dosyaUzunlugu > 20 * 1024 * 1024;
                    }
                    catch { }
                    return false;
                });
            }

            // 4. Değiştirme Tarihi Filtresi
            if (filterDate.SelectedItem is ComboBoxItem dateItem && dateItem.Content is string dateStr && dateStr != "Her Tarih")
            {
                DateTime simdi = DateTime.Now;
                filtrelenmis = filtrelenmis.Where(f =>
                {
                    try
                    {
                        string gercekYol = f.StartsWith("__yinelenen__") ? f.Substring("__yinelenen__".Length) : f;
                        if (!File.Exists(gercekYol)) return false;
                        DateTime sonGuncelleme = File.GetLastWriteTime(gercekYol);

                        if (dateStr == "Son 24 Saat") return sonGuncelleme >= simdi.AddHours(-24);
                        if (dateStr == "Son 7 Gün") return sonGuncelleme >= simdi.AddDays(-7);
                        if (dateStr == "Son 30 Gün") return sonGuncelleme >= simdi.AddDays(-30);
                        if (dateStr == "Son 1 Yıl") return sonGuncelleme >= simdi.AddYears(-1);
                    }
                    catch { }
                    return false;
                });
            }

            // Sonuçları aktif listeye eşle, ilk sayfayı göster
            aktifListe = filtrelenmis.ToList();
            mevcutSayfa = 1;
            SayfayiGoster(1);
        }
    }

    // Lightroom stili veri modeli
    public class FolderNode
    {
        public string Name { get; set; }
        public string FullPath { get; set; }
        public bool IsDrive { get; set; }
        public List<FolderNode> Children { get; set; } = new List<FolderNode>();

        public Visibility DriveIconVisibility => IsDrive ? Visibility.Visible : Visibility.Collapsed;
        public Visibility FolderIconVisibility => IsDrive ? Visibility.Collapsed : Visibility.Visible;
    }

    // WPF Değer Dönüştürücü sınıfı
    public class PathToFolderConverter : System.Windows.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is string path && !string.IsNullOrEmpty(path))
            {
                try
                {
                    return System.IO.Path.GetFileName(System.IO.Path.GetDirectoryName(path));
                }
                catch { }
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
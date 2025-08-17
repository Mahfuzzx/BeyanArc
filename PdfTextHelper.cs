using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Core;

namespace BeyanArc
{
    /*
            string[] pdfFiles = Directory.GetFiles("C:\\TEMP\\", "*.pdf");
            int unrecognizedCount = 0;
            foreach (string pdfFile in pdfFiles)
            {
                PdfTextHelper pdfTextHelper = new(pdfFile, 1);
                var sgkLabel = pdfTextHelper.hasLabel("SOSYAL GÜVENLİK KURUMU BAŞKANLIĞI");
                var tahakkukLabel = pdfTextHelper.hasLabel("TAHAKKUK FİŞİ");
                var beyannameLabel = pdfTextHelper.hasLabel("BEYANNAMESİ");
                var sgkBildirgeLabel = pdfTextHelper.hasLabel("SİGORTALI HİZMET LİSTESİ");
                if (!sgkLabel && tahakkukLabel)
                {
                    string? vkn = pdfTextHelper.getRightOf("VERGİ KİMLİK NUMARASI");
                    if (vkn?.Length > 11)
                        vkn = vkn[..11];
                    string? date = pdfTextHelper.getBelow("Kabul Tarihi");
                    string? season = pdfTextHelper.getBelow("Vergilendirme Dönemi");
                    listBox1.Items.Add($"Vergi Tahakkuk: {vkn} {date} {season}");
                }
                else if (beyannameLabel)
                {
                    string? tckn = pdfTextHelper.getRightOf("Vergi Kimlik Numarası (TC Kimlik No)");
                    string? vkn = pdfTextHelper.getRightOf("Vergi Kimlik Numarası");
                    if (!string.IsNullOrEmpty(tckn)) vkn = tckn;
                    string? onayTarihi = pdfTextHelper.getRightOf("Onay Zamanı :");
                    string duzeltmeNedeni = pdfTextHelper.getRightOf("Düzeltme Nedeni :", 5, .4) ?? "";
                    string? kodu = pdfTextHelper.getRightOf("BEYANNAMESİ", 15, .4);
                    if (!string.IsNullOrEmpty(duzeltmeNedeni))
                        duzeltmeNedeni = $"({duzeltmeNedeni})";
                    listBox1.Items.Add($"Vergi Beyanname: {vkn} {kodu} {onayTarihi} {duzeltmeNedeni}");
                }
                else if (sgkBildirgeLabel)
                {
                    string? onayTarihi = pdfTextHelper.getRightOf("SİGORTALI HİZMET LİSTESİ -", 5, .4);
                    string? season = pdfTextHelper.getRightOf("Yıl - Ay")?.Replace(":", "").Trim();
                    string? sicilNo = pdfTextHelper.getRightOf("İşyeri Sicil No")?.Replace(":", "").Trim();
                    listBox1.Items.Add($"SGK Bildirge: {onayTarihi} {season} {sicilNo}");
                }
                else if (sgkLabel && tahakkukLabel)
                {
                    string? onayTarihi = pdfTextHelper.getBelow("TAHAKKUK FİŞİ", 20, .4);
                    string? season = pdfTextHelper.getRightOf("AİT OLDUĞU YIL / AY")?.Replace(":", "").Trim();
                    string? sicilNo = pdfTextHelper.getRightOf("Sicil No")?.Replace(":", "").Trim();
                    listBox1.Items.Add($"SGK Tahakkuk: {onayTarihi} {season} {sicilNo}");
                }
                else
                {
                    listBox1.Items.Add(pdfFile);
                    unrecognizedCount++;
                }
                label1.Text = $"Toplam {listBox1.Items.Count} belge bulundu. {unrecognizedCount} tanesi tanınamadı.";
            }
     */
    public class PdfTextHelper
    {
        private readonly List<Word> _words;
        private readonly Page _page;

        public PdfTextHelper(string pdfPath, int pageNumber)
        {
            using var pdf = PdfDocument.Open(pdfPath);
            var page = pdf.GetPage(pageNumber);
            _page = page;
            _words = [.. page.GetWords()];
        }

        public bool hasLabel(string labelText)
        {
            return findLabelPhrase(labelText) != null;
        }

        // GetRightOf ve GetBelow metodları aynı kalabilir, değişiklik FindLabelPhrase içinde.
        public string? getRightOf(string labelText, double yTolerance = 5, double maxGapMultiplier = 1.5)
        {
            var label = findLabelPhrase(labelText);
            if (label == null) return null;

            var labelBox = label.Value.BBox;

            var candidates = _words
                .Where(w =>
                    Math.Abs(w.BoundingBox.Centroid.Y - labelBox.Centroid.Y) < yTolerance &&
                    w.BoundingBox.Left > labelBox.Right)
                .OrderBy(w => w.BoundingBox.Left)
                .ToList();

            return mergeWordsIntoLine(candidates, maxGapMultiplier);
        }

        public string? getBelow(string labelText, double xTolerance = 20, double yGap = 1)
        {
            var label = findLabelPhrase(labelText);
            if (label == null) return null;

            var labelBox = label.Value.BBox;

            var candidates = _words
                .Where(w =>
                    w.BoundingBox.Top < (labelBox.Bottom - yGap) &&
                    w.BoundingBox.Centroid.X >= labelBox.Left - xTolerance &&
                    w.BoundingBox.Centroid.X <= labelBox.Right + xTolerance)
                .ToList();

            if (candidates.Count == 0) return null;

            var closestLine = candidates
                .GroupBy(w => Math.Round(w.BoundingBox.Centroid.Y, 1))
                .OrderByDescending(g => g.Key)
                .FirstOrDefault();

            if (closestLine == null) return null;

            var orderedWords = closestLine.OrderBy(w => w.BoundingBox.Left).ToList();
            return mergeWordsIntoLine(orderedWords);
        }

        /// <summary>
        /// *** DÜZELTİLMİŞ METOT ***
        /// Bir etiketi oluşturan kelimeleri, sayfa genelinde katı bir sıralama yapmadan,
        /// belgedeki doğal akışlarına göre arar. Bu, kelimelerin hafif hizalama sorunlarından
        /// etkilenmesini engeller.
        /// </summary>
        private (string Text, PdfRectangle BBox)? findLabelPhrase(string labelText)
        {
            var labelParts = labelText.Split([' '], StringSplitOptions.RemoveEmptyEntries);
            if (labelParts.Length == 0) return null;

            // Hatalı sıralama satırı kaldırıldı. Doğrudan _words listesi kullanılıyor.
            // var orderedPageWords = _words.OrderByDescending...

            for (int i = 0; i < _words.Count; i++)
            {
                // Etiketin ilk kelimesiyle eşleşme ara
                if (string.Equals(_words[i].Text, labelParts[0], StringComparison.OrdinalIgnoreCase))
                {
                    var foundWords = new List<Word> { _words[i] };
                    if (labelParts.Length == 1)
                    {
                        return (labelText, _words[i].BoundingBox);
                    }

                    bool fullMatch = true;
                    Word lastWord = _words[i];

                    // Etiketin geri kalan kısımlarını ara
                    for (int j = 1; j < labelParts.Length; j++)
                    {
                        // Sonraki kelimeyi bulmak için küçük bir arama penceresi kullan
                        // Bu, araya alakasız ama küçük kelimeler girse bile etiketi bulmayı sağlar
                        int searchWindow = Math.Min(10, _words.Count - (i + j));
                        Word? nextMatch = null;

                        for (int k = 1; k < searchWindow; k++)
                        {
                            var candidateWord = _words[i + k];
                            if (string.Equals(candidateWord.Text, labelParts[j], StringComparison.OrdinalIgnoreCase))
                            {
                                // Aday kelimenin, son bulunan kelimeyle aynı satırda ve sağında olduğunu kontrol et
                                double yDiff = Math.Abs(lastWord.BoundingBox.Centroid.Y - candidateWord.BoundingBox.Centroid.Y);
                                double xDiff = candidateWord.BoundingBox.Left - lastWord.BoundingBox.Right;

                                // Aynı satırda ve makul bir mesafede olmalı
                                if (yDiff < 5 && xDiff > 0 && xDiff < lastWord.BoundingBox.Width * 2)
                                {
                                    nextMatch = candidateWord;
                                    break;
                                }
                            }
                        }

                        if (nextMatch != null)
                        {
                            foundWords.Add(nextMatch);
                            lastWord = nextMatch;
                        }
                        else
                        {
                            fullMatch = false;
                            break;
                        }
                    }

                    if (fullMatch)
                    {
                        var combinedBox = foundWords[0].BoundingBox;
                        for (int k = 1; k < foundWords.Count; k++)
                        {
                            combinedBox = merge(combinedBox, foundWords[k].BoundingBox);
                        }
                        return (labelText, combinedBox);
                    }
                }
            }

            return null; // Eşleşme bulunamadı
        }

        // Merge ve MergeWordsIntoLine metodları aynı kalabilir.
        private static PdfRectangle merge(PdfRectangle a, PdfRectangle b)
        {
            return new PdfRectangle(
                Math.Min(a.Left, b.Left),
                Math.Min(a.Bottom, b.Bottom),
                Math.Max(a.Right, b.Right),
                Math.Max(a.Top, b.Top)
            );
        }

        private static string? mergeWordsIntoLine(List<Word> words, double maxGapMultiplier = 1.5)
        {
            if (words == null || words.Count == 0)
            {
                return null;
            }

            string lineText = "";
            for (int i = 0; i < words.Count; i++)
            {
                if (i > 0)
                {
                    var prevWord = words[i - 1];
                    var currentWord = words[i];
                    // Ortalama karakter genişliği için daha güvenli bir yol
                    var avgCharWidth = prevWord.BoundingBox.Width / (prevWord.Text.Length > 0 ? prevWord.Text.Length : 1);
                    var gap = currentWord.BoundingBox.Left - prevWord.BoundingBox.Right;

                    if (gap > 0) // Sadece aralarında boşluk varsa ekle
                    {
                        if (gap > avgCharWidth * maxGapMultiplier)
                        {
                            lineText += " "; // Normal boşluk
                        }
                    }
                }
                lineText += words[i].Text;
            }

            return lineText.Trim();
        }
    }
}
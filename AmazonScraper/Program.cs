using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace AmazonScraper
{
    class Program
    {
        public static async Task<string> GetHtml()
        {
            try
            {
                string strhtml = String.Empty;

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://www.amazon.com/s?k=headphones&crid=1CRX7JZ1XUMXQ&sprefix=headphones%2Caps%2C512&ref=nb_sb_noss_1");

                request.AutomaticDecompression = DecompressionMethods.GZip;

                request.UserAgent = "*";

                using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync())
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                {
                    strhtml = await reader.ReadToEndAsync();
                }

                var products = GetAmazonProduct(strhtml);
                int count = 0;

                // Displaying information here
                foreach (var product in products)
                {
                    count++;
                    Console.WriteLine($"Product {count}:");
                    Console.WriteLine(product);
                }

                return strhtml; // Return the HTML after processing
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return String.Empty; // Return an empty string in case of an error
            }
        }

        static List<ProductDetails> GetAmazonProduct(string html)
        {
            var products = new List<ProductDetails>();

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);

            var nodeTitles = doc.DocumentNode.SelectNodes("//span[@class='a-size-medium a-color-base a-text-normal']");
            var nodePrices = doc.DocumentNode.SelectNodes("//span[@class='a-offscreen']");
            var nodeRatings = doc.DocumentNode.SelectNodes("//span[@class='a-icon-alt']");
            var nodeUrls = doc.DocumentNode.SelectNodes("//a[@class='a-link-normal s-underline-text s-underline-link-text s-link-style a-text-normal']"); // Adjust the class name

            if (nodeTitles != null && nodePrices != null && nodeRatings != null && nodeUrls != null)
            {
                for (int i = 0; i < Math.Min(Math.Min(nodeTitles.Count, nodePrices.Count), Math.Min(nodeRatings.Count, nodeUrls.Count)); i++)
                {
                    var title = nodeTitles[i].InnerText.Trim();
                    var price = nodePrices[i].InnerText.Trim();
                    var rating = nodeRatings[i].InnerText.Trim();
                    var url = nodeUrls[i].GetAttributeValue("href", "").Trim();

                    var product = new ProductDetails
                    {
                        ProductName = title,
                        Price = price,
                        Rating = rating,
                        URL = url
                    };

                    products.Add(product);
                }
            }

            return products;
        }

        static async Task Main(string[] args)
        {
            var temp = await GetHtml();
        }
    }

    public class ProductDetails
    {
        public string ProductName { get; set; }
        public string Price { get; set; }
        public string Rating { get; set; }
        public string URL { get; set; }

        public override string ToString()
        {
            return $"Name: {ProductName} \nPrice: {Price}\nRating: {Rating}\nURL: {URL}\n";
        }
    }
}
using System;
using System.Windows.Forms;
using AlloCine;


namespace AlloCineClient
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }


        private void buttonRun_Click(object sender, EventArgs e)
        {
            //"ah si j'etais riche" = 42346
            var api = new AlloCineApi();

            textBox1.AppendText("//Look for anything in Allocine that contains 'riche'");
            var alFeed = api.Search("riche", new[] { TypeFilters.Movie }, 8, 1);
            if (alFeed.Error != null)
            {
                textBox1.AppendText("\r\n" + alFeed.Error.Value);
            }
            else
            {
                foreach (var mov in alFeed.MovieList)
                {
                    textBox1.AppendText("\r\n" + mov.Code + "\t" + mov.OriginalTitle + "\t");
                }
            }

            textBox1.AppendText("\r\n\r\n\r\n//Retrieve the details of the Movie 'Ah si j'etais riche");
            var alMovie = api.MovieGetInfo(42346, ResponseProfiles.Large, new[] { TypeFilters.Movie, TypeFilters.News }, new[] { "synopsis" }, new[] { MediaFormat.Mpeg2 });
            if (alMovie.Error != null)
            {
                textBox1.AppendText("\r\n" + alMovie.Error.Value);
            }
            else
            {
                textBox1.AppendText("\r\n" + alMovie.Code + "\t" + alMovie.OriginalTitle + "\t" + alMovie.MovieType.Code);
            }

            textBox1.AppendText("\r\n\r\n\r\n//Retrieve the details about the TvSeries 'Lost'");
            var alTvSeries = api.TvSeriesGetInfo(223, ResponseProfiles.Large, new[] { "synopsis" }, null);
            if (alTvSeries.Error != null)
            {
                textBox1.AppendText("\r\n" + alTvSeries.Error.Value);
            }
            else
            {
                textBox1.AppendText("\r\n" + alTvSeries.Code + "\t" + alTvSeries.OriginalTitle + "\t" + alTvSeries.Title);
            }

        }
    }
}

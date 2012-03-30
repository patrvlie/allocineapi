using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Web;


namespace AlloCine
{
    public class AlloCineApi
    {
        #region Declarations
        private readonly WebClient _client;
        private const string AlloCineBaseAddress = "http://api.allocine.fr/rest/v3/";
        private const string SearchUrl = "search?partner=YW5kcm9pZC12M3M&{0}";
        private const string MovieGetInfoUrl = "movie?partner=YW5kcm9pZC12M3M&{0}";
        private const string MovieGetReviewListUrl = "reviewlist?partner=YW5kcm9pZC12M3M&{0}";
        private const string PersonGetInfoUrl = "person?partner=YW5kcm9pZC12M3M&{0}";
        private const string PersonGetFilmographyUrl = "filmography?partner=YW5kcm9pZC12M3M&{0}";
        private const string MediaGetInfoUrl = "media?partner=YW5kcm9pZC12M3M&{0}";
        private const string TvSeriesGetInfoUrl = "tvseries?partner=YW5kcm9pZC12M3M&{0}";
        private const string TvSeriesSeasonGetInfoUrl = "season?partner=YW5kcm9pZC12M3M&{0}";
        private const string TvSeriesEpisodeGetInfoUrl = "episode?partner=YW5kcm9pZC12M3M&{0}"; 
        #endregion
       

        #region AlloCineApi Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="AlloCineApi"/> class.
        /// </summary>
        public AlloCineApi()
        {
            _client = new WebClient { BaseAddress = AlloCineBaseAddress };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AlloCineApi"/> class with the specified Proxy User/Password credentials.
        /// The Proxy assigned to your default browser will be used.
        /// </summary>
        /// <param name="proxyUserName">Your Proxy User Name credential.</param>
        /// <param name="proxyPassword">Your Proxy Password credential.</param>
        public AlloCineApi(string proxyUserName, string proxyPassword)
            : this()
        {
            // Obtain the 'Proxy' of the  Default browser.
            IWebProxy proxy = _client.Proxy;

            if (proxy != null)
            {
                proxy.Credentials = new NetworkCredential(proxyUserName, proxyPassword);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AlloCineApi"/> class with the specified Proxy Address and User/Password credentials.
        /// </summary>
        /// <param name="proxyServerAddress">Your Proxy absolute address including the port number.</param>
        /// <param name="proxyUserName">Your Proxy User Name credential.</param>
        /// <param name="proxyPassword">Your Proxy Password credential.</param>
        public AlloCineApi(string proxyServerAddress, string proxyUserName, string proxyPassword)
            : this()
        {
            IWebProxy nwp = new WebProxy(proxyServerAddress, false);
            nwp.Credentials = new NetworkCredential(proxyUserName, proxyPassword);

            _client.Proxy = nwp;
        }
        #endregion

        #region Search Function
        /// <summary>
        /// Search any reference of a title in the AlloCine database.
        /// </summary>
        /// <param name="query">The query string you are searching for.</param>
        /// <param name="types">The types of information you whish to include in the response from AlloCine.</param>
        /// <param name="resultsPerPage">The maximum number of results per page to be returned.</param>
        /// <param name="pageNumber">The page you want to show in case your query returns more results than the maximum value you have specified to fit on one page.</param>
        /// <returns>Returns a Feed object.</returns>
        public Feed Search(string query, IEnumerable<TypeFilters> types, int resultsPerPage, int pageNumber)
        {
            var nvc = new NameValueCollection();
            nvc["format"] = ResponseFormat.Json.ToString().ToLower();
            if (!string.IsNullOrEmpty(query))
                nvc["q"] = HttpUtility.UrlEncodeUnicode(query);

            if (types != null)
                nvc["filter"] = string.Join(",", types).ToLower();

            if (resultsPerPage > 0)
                nvc["count"] = resultsPerPage.ToString();

            if (pageNumber > 0)
                nvc["page"] = pageNumber.ToString();

            var searchQuery = string.Join("&", nvc.AllKeys.Select(k => string.Format("{0}={1}", k, nvc[k])));
            var alObjectModel = DownloadData(string.Format(SearchUrl, searchQuery), typeof(AllocineObjectModel)) as AllocineObjectModel;

            if (alObjectModel != null)
            {   //If AlloCine returned an Error, we assigned the Error object to the Feed Error Object for easy check 
                //from the class client side
                if (alObjectModel.Error != null)
                {
                    alObjectModel.Feed = new Feed { Error = alObjectModel.Error };
                }
                return alObjectModel.Feed;
            }
            return null;
        }

        /// <summary>
        /// Search any reference of a title in the AlloCine database.
        /// It only returns records of type "Movie" with a maximum of 100 results per page, showing the first page.
        /// </summary>
        /// <param name="query">The query string you are searching for.</param>
        /// <returns>Returns a Feed object.</returns>
        public Feed Search(string query)
        {
            return Search(query, new[] { TypeFilters.Movie }, 100, 1);
        }

        /// <summary>
        /// Search any reference of a title in the AlloCine database.
        /// It only returns records of type "Movie".
        /// </summary>
        /// <param name="query">The query string you are searching for.</param>
        /// <param name="resultsPerPage">The maximum number of results per page to be returned.</param>
        /// <param name="pageNumber">The page you want to show in case your query returns more results than the maximum value you have specified to fit on one page.</param>
        /// <returns>Returns a Feed object.</returns>
        public Feed Search(string query, int resultsPerPage, int pageNumber)
        {
            return Search(query, new[] { TypeFilters.Movie }, resultsPerPage, pageNumber);
        }

        #endregion

        #region MovieGetInfo Function
        /// <summary>
        /// Retrieves all information about a particular Movie.
        /// </summary>
        /// <param name="movieCode">The AlloCine Code of the Movie you are searching for.</param>
        /// <param name="profile">The level of details returned by AlloCine.</param>
        /// <param name="types">The types of information you whish to include in the response from AlloCine.</param>
        /// <param name="stripTags">Value fields from which you want any HTML tags, if present, to be removed, so the values are returned in plain text.</param>
        /// <param name="mediaFormats">Video formats to return for the Movie.</param>
        /// <returns>Returns a Movie object.</returns>
        public Movie MovieGetInfo(int movieCode, ResponseProfiles profile, IEnumerable<TypeFilters> types, IEnumerable<string> stripTags, IEnumerable<MediaFormat> mediaFormats)
        {
            var nvc = new NameValueCollection();
            nvc["format"] = ResponseFormat.Json.ToString().ToLower();

            nvc["code"] = movieCode.ToString().ToLower();

            nvc["profile"] = profile.ToString().ToLower();

            if (types != null)
                nvc["filter"] = string.Join(",", types).ToLower();

            if (stripTags != null)
                nvc["striptags"] = string.Join(",", stripTags).ToLower();

            if (mediaFormats != null)
                nvc["mediafmt"] = string.Join(",", mediaFormats.ToList().ConvertAll(MediaFormatsGetValue));


            var searchQuery = string.Join("&", nvc.AllKeys.Select(k => string.Format("{0}={1}", k, nvc[k])));
            var alObjectModel =
                DownloadData(string.Format(MovieGetInfoUrl, searchQuery), typeof (AllocineObjectModel)) as
                AllocineObjectModel;

            if (alObjectModel != null )
            {   //If AlloCine returned an Error, we assigned the Error object to the Movie Error Object for easy check 
                //from the class client side
                if (alObjectModel.Error != null)
                {
                    alObjectModel.Movie = new Movie {Error = alObjectModel.Error};
                }
                return alObjectModel.Movie;
            }
            return null;

        }

        /// <summary>
        /// Retrieves all information about a particular movie.
        /// It only returns records of Movie type with Medium details, removing any HTML tags on synopsis and synopsisshort.
        /// </summary>
        /// <param name="movieCode">The AlloCine Code of the movie you are searching for.</param>
        /// <returns>Returns a Movie object.</returns>
        public Movie MovieGetInfo(int movieCode)
        {
            return MovieGetInfo(movieCode, ResponseProfiles.Medium, new[] { TypeFilters.Movie }, new[] { "synopsis", "synopsisshort" }, null);
        }

        /// <summary>
        /// Retrieves all information about a particular movie.
        /// It only returns records of Movie type, removing any HTML tags on synopsis and synopsisshort.
        /// The level of details is left to your choice.
        /// </summary>
        /// <param name="movieCode">The AlloCine Code of the movie you are searching for.</param>
        /// <param name="profile">The level of details returned by AlloCine.</param>
        /// <returns>Returns a Movie object.</returns>
        public Movie MovieGetInfo(int movieCode, ResponseProfiles profile)
        {
            return MovieGetInfo(movieCode, profile, new[] { TypeFilters.Movie }, new[] { "synopsis", "synopsisshort" }, null);
        }

        /// <summary>
        /// Retrieves all information about a particular movie.
        /// It removes any HTML tags on synopsis and synopsisshort.
        /// The level of details and types of records to return is left to your choice.
        /// </summary>
        /// <param name="movieCode">The AlloCine Code of the movie you are searching for.</param>
        /// <param name="profile">The level of details returned by AlloCine.</param>
        /// <param name="types">The types of information you whish to include in the response from AlloCine.</param>
        /// <returns>Returns a Movie object.</returns>
        public Movie MovieGetInfo(int movieCode, ResponseProfiles profile, IEnumerable<TypeFilters> types)
        {
            return MovieGetInfo(movieCode, profile, types, new[] { "synopsis", "synopsisshort" }, null);
        }

        #endregion

        #region MovieGetReviewList Function
        /// <summary>
        /// Retrieves the Reviews about a particular Movie..
        /// </summary>
        /// <param name="movieCode">The AlloCine Code of the Movie you are searching for.</param>
        /// <param name="type">The type of Review, either from Press or Public</param>
        /// <param name="resultsPerPage">The maximum number of results per page to be returned.</param>
        /// <param name="pageNumber">The page you want to show in case your query returns more results than the maximum value you have specified to fit on one page.</param>
        /// <returns>Returns a Feed object.</returns>
        public Feed MovieGetReviewList(int movieCode, ReviewTypes type, int resultsPerPage, int pageNumber)
        {
            var nvc = new NameValueCollection();
            nvc["format"] = ResponseFormat.Json.ToString().ToLower();

            nvc["code"] = movieCode.ToString().ToLower();

            nvc["type"] = "movie";

            if (resultsPerPage > 0)
                nvc["count"] = resultsPerPage.ToString();

            if (pageNumber > 0)
                nvc["page"] = pageNumber.ToString();

            nvc["filter"] = ReviewTypesGetValue(type);


            var searchQuery = string.Join("&", nvc.AllKeys.Select(k => string.Format("{0}={1}", k, nvc[k])));
            var alObjectModel = DownloadData(string.Format(MovieGetReviewListUrl, searchQuery), typeof(AllocineObjectModel)) as AllocineObjectModel;

            if (alObjectModel != null)
            {   //If AlloCine returned an Error, we assigned the Error object to the Feed Error Object for easy check 
                //from the class client side
                if (alObjectModel.Error != null)
                {
                    alObjectModel.Feed = new Feed { Error = alObjectModel.Error };
                }
                return alObjectModel.Feed;
            }
            return null;
        }
        #endregion

        #region PersonGetInfo Function
        /// <summary>
        /// Retrieves all information about a particular Person.
        /// </summary>
        /// <param name="personCode">The AlloCine Code of the Person you are searching for.</param>
        /// <param name="profile">The level of details returned by AlloCine.</param>
        /// <param name="types">The types of information you whish to include in the response from AlloCine.</param>
        /// <returns>Returns a Person object.</returns>
        public Person PersonGetInfo(int personCode, ResponseProfiles profile, IEnumerable<TypeFilters> types)
        {
            var nvc = new NameValueCollection();
            nvc["format"] = ResponseFormat.Json.ToString().ToLower();

            nvc["code"] = personCode.ToString().ToLower();

            nvc["profile"] = profile.ToString().ToLower();

            if (types != null)
                nvc["filter"] = string.Join(",", types).ToLower();

            var searchQuery = string.Join("&", nvc.AllKeys.Select(k => string.Format("{0}={1}", k, nvc[k])));
            var alObjectModel = DownloadData(string.Format(PersonGetInfoUrl, searchQuery), typeof(AllocineObjectModel)) as AllocineObjectModel;

            if (alObjectModel != null)
            {   //If AlloCine returned an Error, we assigned the Error object to the Person Error Object for easy check 
                //from the class client side
                if (alObjectModel.Error != null)
                {
                    alObjectModel.Person = new Person { Error = alObjectModel.Error };
                }
                return alObjectModel.Person;
            }
            return null;
        }
        #endregion

        #region PersonGetFilmography Function
        /// <summary>
        /// Retrieves all Filmography about a particular Person.
        /// </summary>
        /// <param name="personCode">The AlloCine Code of the Person you are searching for.</param>
        /// <param name="profile">The level of details returned by AlloCine.</param>
        /// <param name="types">The types of information you whish to include in the response from AlloCine.</param>
        /// <returns>Returns a Person object.</returns>
        public Person PersonGetFilmography(int personCode, ResponseProfiles profile, IEnumerable<TypeFilters> types)
        {
            var nvc = new NameValueCollection();
            nvc["format"] = ResponseFormat.Json.ToString().ToLower();

            nvc["code"] = personCode.ToString().ToLower();

            nvc["profile"] = profile.ToString().ToLower();

            if (types != null)
                nvc["filter"] = string.Join(",", types).ToLower();

            var searchQuery = string.Join("&", nvc.AllKeys.Select(k => string.Format("{0}={1}", k, nvc[k])));
            var alObjectModel = DownloadData(string.Format(PersonGetFilmographyUrl, searchQuery), typeof(AllocineObjectModel)) as AllocineObjectModel;

            if (alObjectModel != null)
            {   //If AlloCine returned an Error, we assigned the Error object to the Person Error Object for easy check 
                //from the class client side
                if (alObjectModel.Error != null)
                {
                    alObjectModel.Person = new Person { Error = alObjectModel.Error };
                }
                return alObjectModel.Person;
            }
            return null;
        }
        #endregion

        #region MediaGetInfo Function
        /// <summary>
        /// Retrieves all info about a particular Media.
        /// </summary>
        /// <param name="mediaCode">The AlloCine Code of the Media you are searching for.</param>
        /// <param name="profile">The level of details returned by AlloCine.</param>
        /// <param name="mediaFormats">Video formats to return for the Media.</param>
        /// <returns>Returns a Media object.</returns>
        public Media MediaGetInfo(int mediaCode, ResponseProfiles profile, IEnumerable<MediaFormat> mediaFormats = null)
        {
            var nvc = new NameValueCollection();
            nvc["format"] = ResponseFormat.Json.ToString().ToLower();

            nvc["code"] = mediaCode.ToString().ToLower();

            nvc["profile"] = profile.ToString().ToLower();

            if (mediaFormats != null)
                nvc["mediafmt"] = string.Join(",", mediaFormats.ToList().ConvertAll(MediaFormatsGetValue));

            var searchQuery = string.Join("&", nvc.AllKeys.Select(k => string.Format("{0}={1}", k, nvc[k])));
            var alObjectModel = DownloadData(string.Format(MediaGetInfoUrl, searchQuery), typeof(AllocineObjectModel)) as AllocineObjectModel;

            if (alObjectModel != null)
            {   //If AlloCine returned an Error, we assigned the Error object to the Media Error Object for easy check 
                //from the class client side
                if (alObjectModel.Error != null)
                {
                    alObjectModel.Media = new Media { Error = alObjectModel.Error };
                }
                return alObjectModel.Media;
            }
            return null;
        }

        #endregion

        #region TvSeriesGetInfo Function

        /// <summary>
        /// Retrieves all info about a particular TvSerie Season.
        /// </summary>
        /// <param name="tvseriesCode">The AlloCine Code of the TvSeries you are searching for.</param>
        /// <param name="profile">The level of details returned by AlloCine.</param>
        /// <param name="stripTags">Value fields from which you want any HTML tags, if present, to be removed, so the values are returned in plain text.</param>
        /// <param name="mediaFormats">Video formats to return for the TvSeries.</param>
        /// <returns>Returns a TvSeries object.</returns>
        public TvSeries TvSeriesGetInfo(int tvseriesCode, ResponseProfiles profile, IEnumerable<string> stripTags, IEnumerable<MediaFormat> mediaFormats = null)
        {
            var nvc = new NameValueCollection();
            nvc["format"] = ResponseFormat.Json.ToString().ToLower();

            nvc["code"] = tvseriesCode.ToString().ToLower();

            nvc["profile"] = profile.ToString().ToLower();

            if (stripTags != null)
                nvc["striptags"] = string.Join(",", stripTags).ToLower();

            if (mediaFormats != null)
                nvc["mediafmt"] = string.Join(",", mediaFormats.ToList().ConvertAll(MediaFormatsGetValue));

            var searchQuery = string.Join("&", nvc.AllKeys.Select(k => string.Format("{0}={1}", k, nvc[k])));
            var alObjectModel = DownloadData(string.Format(TvSeriesGetInfoUrl, searchQuery), typeof(AllocineObjectModel)) as AllocineObjectModel;

            if (alObjectModel != null)
            {   //If AlloCine returned an Error, we assigned the Error object to the TvSeries Error Object for easy check 
                //from the class client side
                if (alObjectModel.Error != null)
                {
                    alObjectModel.TvSeries = new TvSeries { Error = alObjectModel.Error };
                }
                return alObjectModel.TvSeries;
            }
            return null;
        }

        #endregion

        #region TvSeriesSeasonGetInfo Function

        /// <summary>
        /// Retrieves all info about a particular TvSerie Season.
        /// </summary>
        /// <param name="seasonCode">The AlloCine Code of the Season you are searching for.</param>
        /// <param name="profile">The level of details returned by AlloCine.</param>
        /// <param name="stripTags">Value fields from which you want any HTML tags, if present, to be removed, so the values are returned in plain text.</param>
        /// <param name="mediaFormats">Video formats to return for the Season.</param>
        /// <returns>Returns a Season object.</returns>
        public Season TvSeriesSeasonGetInfo(int seasonCode, ResponseProfiles profile, IEnumerable<string> stripTags, IEnumerable<MediaFormat> mediaFormats = null)
        {
            var nvc = new NameValueCollection();
            nvc["format"] = ResponseFormat.Json.ToString().ToLower();

            nvc["code"] = seasonCode.ToString().ToLower();

            nvc["profile"] = profile.ToString().ToLower();

            if (stripTags != null)
                nvc["striptags"] = string.Join(",", stripTags).ToLower();

            if (mediaFormats != null)
                nvc["mediafmt"] = string.Join(",", mediaFormats.ToList().ConvertAll(MediaFormatsGetValue));

            var searchQuery = string.Join("&", nvc.AllKeys.Select(k => string.Format("{0}={1}", k, nvc[k])));
            var alObjectModel = DownloadData(string.Format(TvSeriesSeasonGetInfoUrl, searchQuery), typeof(AllocineObjectModel)) as AllocineObjectModel;

            if (alObjectModel != null)
            {   //If AlloCine returned an Error, we assigned the Error object to the Season Error Object for easy check 
                //from the class client side
                if (alObjectModel.Error != null)
                {
                    alObjectModel.Season = new Season { Error = alObjectModel.Error };
                }
                return alObjectModel.Season;
            }
            return null;
        }

        #endregion

        #region TvSeriesEpisodeGetInfo Function

        /// <summary>
        /// Retrieves all info about a particular TvSerie Episode.
        /// </summary>
        /// <param name="episodeCode">The AlloCine Code of the Episode you are searching for.</param>
        /// <param name="profile">The level of details returned by AlloCine.</param>
        /// <param name="stripTags">Value fields from which you want any HTML tags, if present, to be removed, so the values are returned in plain text.</param>
        /// <param name="mediaFormats">Video formats to return for the Episode.</param>
        /// <returns>Returns an Episode object.</returns>
        public Episode TvSeriesEpisodeGetInfo(int episodeCode, ResponseProfiles profile, IEnumerable<string> stripTags, IEnumerable<MediaFormat> mediaFormats = null)
        {
            var nvc = new NameValueCollection();
            nvc["format"] = ResponseFormat.Json.ToString().ToLower();

            nvc["code"] = episodeCode.ToString().ToLower();

            nvc["profile"] = profile.ToString().ToLower();

            if (stripTags != null)
                nvc["striptags"] = string.Join(",", stripTags).ToLower();

            if (mediaFormats != null)
                nvc["mediafmt"] = string.Join(",", mediaFormats.ToList().ConvertAll(MediaFormatsGetValue));

            var searchQuery = string.Join("&", nvc.AllKeys.Select(k => string.Format("{0}={1}", k, nvc[k])));
            var alObjectModel = DownloadData(string.Format(TvSeriesEpisodeGetInfoUrl, searchQuery), typeof(AllocineObjectModel)) as AllocineObjectModel;

            if (alObjectModel != null)
            {   //If AlloCine returned an Error, we assigned the Error object to the Episode Error Object for easy check 
                //from the class client side
                if (alObjectModel.Error != null)
                {
                    alObjectModel.Episode = new Episode { Error = alObjectModel.Error };
                }
                return alObjectModel.Episode;
            }
            return null;
        }

        #endregion


        #region DownloadData Function
        private object DownloadData(string url, System.Type type)
        {
            using (var stream = _client.OpenRead(url))
            {
                if (stream == null)
                    return null;
                var dcs = new DataContractJsonSerializer(type);
                var o = dcs.ReadObject(stream);
                return o;
            }
        }
        #endregion


        #region ReviewTypesGetValue Function
        private string ReviewTypesGetValue(ReviewTypes type)
        {
            switch (type)
            {
                case ReviewTypes.DeskPress: return "desk-press";
                case ReviewTypes.Public: return "public";
                default:
                    return "";
            }
        } 
        #endregion

        #region MediaFormatsGetValue Function
        private string MediaFormatsGetValue(MediaFormat format)
        {
            switch (format)
            {
                case MediaFormat.Flv: return "flv";
                case MediaFormat.Mp4Lc: return "mp4-lc";
                case MediaFormat.Mp4Hip: return "mp4-hip";
                case MediaFormat.Mp4Archive: return "mp4-archive";
                case MediaFormat.Mpeg2Theater: return "mpeg2-theater";
                case MediaFormat.Mpeg2: return "mpeg2";
                default:
                    return "";
            }
        } 
        #endregion

    }
}

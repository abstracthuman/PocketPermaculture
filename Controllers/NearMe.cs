using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using PocketPermaculture.Data;
using PocketPermaculture.Hubs;
using WebApiClient;
using static PocketPermaculture.Startup;

namespace PocketPermaculture.Controllers
{
    public class NearMe : Controller
    {
        private readonly ApplicationSettings _applicationSettings;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly ApplicationDbContext _db;

        public readonly MakeWebCall makeWebCall = new MakeWebCall();

        public NearMe(
        ApplicationSettings applicationSettings,
        UserManager<ApplicationUser> userManager,
        IHubContext<ChatHub> hubContext,
        ApplicationDbContext db)
        {
            _applicationSettings = applicationSettings;
            _userManager = userManager;
            _hubContext = hubContext;
            _db = db;
        }

        public readonly string googleURL = "https://maps.googleapis.com/maps/api/geocode/json";

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User); // Authenticated User

            if (user == null)
            {
                ViewBag.isNearMe = "false";
                ViewBag.isAuthenticated = "false";
                return View();
            }

            await _hubContext.Clients.All.SendAsync("Notify", $"Home page loaded at: {DateTime.Now}");

            var users = _userManager.Users.ToList(); // All Users
            var userAddress = _db.UserAddresses.FirstOrDefault(uad => uad.UserId == user.Id);

            AddressComponent userCounty = null;
            Location userLocation = null;
            List<UserMapData> allUserMapData = new List<UserMapData>();

            ViewBag.googleApiKey = _applicationSettings.GoogleApiKey;

            if (userAddress != null)
            {
                string userAddressQuery = "";
                userAddressQuery = GetAddressQuery(userAddress);

                if (userAddressQuery != "")
                {
                    userLocation = GetCoords(userAddressQuery, ViewBag.googleApiKey);
                    userCounty = GetCounty(userAddressQuery, ViewBag.googleApiKey);

                    List<Location> positions = new List<Location>();
                    Double distance = 0;

                    foreach (var member in users)
                    {
                        // Don't load personal pins
                        if (member.Id == user.Id)
                        {
                            continue;
                        }

                        string memberAddressQuery = "";
                        var memberAddress = _db.UserAddresses.FirstOrDefault(uad => uad.UserId == member.Id);
                        var pins = _db.UserPins.Where(uid => uid.UserId == member.Id).ToList();

                        if (memberAddress != null && (pins != null && pins.Count > 0))
                        {
                            memberAddressQuery = GetAddressQuery(memberAddress);
                            if (memberAddressQuery != "")
                            {
                                Location memberLocation = GetCoords(memberAddressQuery, ViewBag.googleApiKey);
                                distance = HaversineDistance(userLocation, memberLocation);

                                positions.Add(memberLocation);

                                if (positions.Count > 1)
                                {

                                    foreach (var position in positions)
                                    {
                                        var latIsEqual = Math.Abs(position.lat - memberLocation.lat) < Double.Epsilon;
                                        var lngIsEqual = Math.Abs(position.lng - memberLocation.lng) < Double.Epsilon;

                                        if (latIsEqual && lngIsEqual)
                                        {
                                            Location locOffset = GetOffset(memberLocation.lat, memberLocation.lng);

                                            memberLocation = new Location
                                            {
                                                lat = locOffset.lat,
                                                lng = locOffset.lng
                                            };
                                        }
                                    }
                                }

                                if (distance < 50)
                                {
                                    Location location = new Location
                                    {
                                        lat = memberLocation.lat,
                                        lng = memberLocation.lng
                                    };

                                    // Need to ecnrypt UserId
                                    UserMapData userMapData = new UserMapData
                                    {
                                        UserId = member.Id,
                                        UserName = member.UserName,
                                        Location = location,
                                        LocationColor = GetLocationColor(pins)
                                    };

                                    allUserMapData.Add(userMapData);
                                }
                            }
                        }
                    }
                }
            }

            if (allUserMapData.Count == 0) 
            {
                ViewBag.isNearMe = "false";
                ViewBag.noPins = "true";
                return View();
            }

            ViewBag.userCounty = userCounty.long_name;
            ViewBag.userLocation = userLocation;
            ViewBag.isNearMe = "true";
            return View(allUserMapData);
        }

        [HttpGet]
        public List<UserPin> GetUserPins(string userName)
        {
            if (userName != null)
            {
                var pins = new List<UserPin>();
                var user = _db.Users.FirstOrDefault(uid => uid.UserName == userName);
                var userPins = _db.UserPins.Where(uid => uid.UserId == user.Id).ToList();

                foreach (var pin in userPins)
                {
                    pins.Add(new UserPin {
                        Title = pin.Title,
                        Description = pin.Description,
                        PinCategory = pin.PinCategory,
                        PinType = pin.PinType
                    });
                }

                return pins;

            }

            return new List<UserPin>();
        }

        public static double HaversineDistance(Location pos1, Location pos2)
        {
            double R = 3960;
            var lat = ToRadians(pos2.lat - pos1.lat);
            var lng = ToRadians(pos2.lng - pos1.lng);
            var h1 = Math.Sin(lat / 2) * Math.Sin(lat / 2) + Math.Cos(ToRadians(pos1.lat)) *
                Math.Cos(ToRadians(pos2.lat)) * Math.Sin(lng / 2) * Math.Sin(lng / 2);
            var h2 = 2 * Math.Asin(Math.Min(1, Math.Sqrt(h1)));
            return R * h2;
        }

        public static string GetAddressQuery(UserAddress userAddress)
        {
            string userAddressQuery = "";

            userAddressQuery += userAddress.Address1.Replace(" ", "+");

            if (userAddress.Address2 != null)
            {
                userAddressQuery += ",+" + userAddress.Address2.Replace(" ", "");
            }

            userAddressQuery += ",+" + userAddress.City.Replace(" ", "");
            userAddressQuery += ",+" + userAddress.State;
            userAddressQuery += ",+" + userAddress.PostalCode;

            return userAddressQuery;
        }

        public AddressComponent GetCounty(string address, string apiKey)
        {
            string query = "?address=" + address + "&key=" + apiKey;

            string response = (string)makeWebCall.GetRequest(googleURL, query);
            RootObject responseObject = JsonConvert.DeserializeObject<RootObject>(response);

            var addressObject = responseObject.results[0].address_components;
            var county = "";
            foreach (var type in addressObject)
            {
                if (type.types[0] == "administrative_area_level_2")
                {
                    county = type.long_name;
                }

            }

            return new AddressComponent
            {
                long_name = county
            };
        }

        public Location GetCoords(string address, string apiKey) {
            string query = "?address=" + address + "&key=" + apiKey;

            string response = (string)makeWebCall.GetRequest(googleURL, query);
            RootObject responseObject = JsonConvert.DeserializeObject<RootObject>(response);

            Location location = new Location
            {
                lat = responseObject.results[0].geometry.location.lat,
                lng = responseObject.results[0].geometry.location.lng
            };

            return location;
        }

        public static Location GetOffset(double lat, double lng)
        {
            var R = 6378137; // Earth’s radius, sphere
            var offsetMeter = 5000;

            // Coordinate offsets in radians
            var dLat = offsetMeter / R;
            var dLon = offsetMeter / (R * Math.Cos(Math.PI * lat / 180));

            // OffsetPosition, decimal degrees
            var latO = lat + dLat * 180 / Math.PI;
            var lonO = lng + dLon * 180 / Math.PI;

            return new Location
            {
                lat = latO,
                lng = lonO
            };
        }

        public static double ToRadians(double angle)
        {
            return Math.PI * angle / 180.0;
        }

        public static string GetLocationColor(List<UserPin> pins)
        {
            bool hasRequest = false;
            bool hasOffer = false;
            string color = "";

            foreach (var pin in pins)
            {
                var pinType = pin.PinType.ToLower();
                if (pinType == "request")
                {
                    hasRequest = true;
                }
                else
                {
                    hasOffer = true;
                }

                if (hasRequest && hasOffer)
                {
                    break;
                }
            }

            color = hasRequest ? "#CC0033" : color;
            color = hasOffer ? "#2E8B57" : color;
            color = hasRequest && hasOffer ? "#CC33CC" : color;

            return color;
        }

        public class UserMapData
        {
            public string UserId { get; set; }
            public string UserName { get; set; }
            public string LocationColor { get; set; }
            public Location Location { get; set; }
        }

        public class AddressComponent
        {
            public string long_name { get; set; }
            public List<string> types { get; set; }
        }

        public class Location
        {
            public double lat { get; set; }
            public double lng { get; set; }
        }

        public class Geometry
        {
            public Location location { get; set; }
        }

        public class Result
        {
            public List<AddressComponent> address_components { get; set; }
            public Geometry geometry { get; set; }
            public List<string> types { get; set; }
        }

        public class RootObject
        {
            public List<Result> results { get; set; }
        }
    }
}
